using ITrade.DB;
using ITrade.DB.Enums;
using ITrade.Services.Interfaces;
using ITrade.Services.Responses;
using Microsoft.EntityFrameworkCore;

namespace ITrade.Services.Services
{
    public class MatchingService(
        Context context,
        ICurrentUserService currentUserService
    ) : IMatchingService
    {
        private const int MaxRecommendations = 10;

        public async Task<ICollection<ProjectMatchedResponse>> RecommandProjectsForSpecialist()
        {
            var userId = currentUserService.UserId;

            // Get the specialist's profile tags
            var specialistTagIds = await context.UserProfileTags
                .Where(upt => upt.UserId == userId)
                .Select(upt => upt.TagId)
                .ToListAsync();

            if (specialistTagIds.Count == 0)
            {
                return [];
            }

            // Get the specialist's completed project tag experience (how many times worked on projects with each tag)
            var tagExperience = await context.Projects
                .Where(p => p.WorkerId == userId
                    && p.ProjectStatusTypeId == (int)ProjectStatusTypeEnum.Completed)
                .SelectMany(p => p.ProjectTags)
                .GroupBy(pt => pt.TagId)
                .Select(g => new { TagId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TagId, x => x.Count);

            // Find open projects (Hiring status, no worker assigned) that match specialist's tags
            var matchingProjects = await context.Projects
                .Where(p => !p.IsDeleted
                    && p.WorkerId == null
                    && p.ProjectStatusTypeId == (int)ProjectStatusTypeEnum.Hiring
                    && p.ProjectTags.Any(pt => specialistTagIds.Contains(pt.TagId)))
                .Select(p => new
                {
                    Project = new ProjectResponse(
                        p.Id,
                        p.Name,
                        p.Description,
                        p.OwnerId,
                        p.Owner.Username,
                        p.WorkerId,
                        p.Worker != null ? p.Worker.Username : null,
                        p.Deadline,
                        p.ProjectStatusTypeId,
                        p.ProjectStatusType.Name,
                        p.ProjectTags
                            .Select(pt => new ProjectTagResponse(pt.Id, pt.Tag.Name))
                            .ToList(),
                        p.CreatedAt,
                        p.UpdatedAt
                    ),
                    ProjectTagIds = p.ProjectTags.Select(pt => pt.TagId).ToList(),
                    TotalTags = p.ProjectTags.Count
                })
                .ToListAsync();

            // Calculate match percentage and experience score for each project
            var scoredProjects = matchingProjects
                .Select(p =>
                {
                    var matchingTagIds = p.ProjectTagIds.Intersect(specialistTagIds).ToList();
                    var matchPercentage = p.TotalTags > 0
                        ? (double)matchingTagIds.Count / p.TotalTags * 100
                        : 0;

                    // Calculate experience score based on completed projects with matching tags
                    var experienceScore = matchingTagIds
                        .Sum(tagId => tagExperience.GetValueOrDefault(tagId, 0));

                    return new
                    {
                        p.Project,
                        MatchPercentage = matchPercentage,
                        ExperienceScore = experienceScore
                    };
                })
                .OrderByDescending(p => p.MatchPercentage)
                .ThenByDescending(p => p.ExperienceScore)
                .Take(MaxRecommendations)
                .Select(p => new ProjectMatchedResponse(p.Project, Math.Round(p.MatchPercentage, 1)))
                .ToList();

            return scoredProjects;
        }

        public async Task<ICollection<UserMatchedResponse>> RecommandSpecialistsForProject(int projectId)
        {
            var result = await RecommandSpecialistsForProjects([projectId]);
            return result.TryGetValue(projectId, out var matches) ? matches : [];
        }

        public async Task<IDictionary<int, ICollection<UserMatchedResponse>>> RecommandSpecialistsForProjects(ICollection<int> projectIds)
        {
            if (projectIds.Count == 0)
            {
                return new Dictionary<int, ICollection<UserMatchedResponse>>();
            }

            // 1. Single query: get all tags for all requested projects
            var projectTagRows = await context.ProjectTags
                .Where(pt => projectIds.Contains(pt.ProjectId))
                .Select(pt => new { pt.ProjectId, pt.TagId })
                .ToListAsync();

            // Build per-project tag dictionaries
            var tagsByProject = projectTagRows
                .GroupBy(r => r.ProjectId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.TagId).ToList());

            // Collect the union of all tag IDs across every project
            var allTagIds = tagsByProject.Values.SelectMany(t => t).Distinct().ToList();

            if (allTagIds.Count == 0)
            {
                return projectIds.ToDictionary(id => id, _ => (ICollection<UserMatchedResponse>)[]);
            }

            // 2. Single query: find all specialists whose profile tags overlap with any of those tags
            var matchingSpecialists = await context.Users
                .Where(u => !u.IsDeleted
                    && u.UserRoleId == (int)UserRoleEnum.Specialist
                    && u.UserProfileTags.Any(upt => allTagIds.Contains(upt.TagId)))
                .Select(u => new
                {
                    User = new UserResponse(u.Id, u.Username, u.UserRole.Name),
                    UserTagIds = u.UserProfileTags.Select(upt => upt.TagId).ToList(),
                    ExperienceTagIds = u.AssignedProjects
                        .Where(p => p.ProjectStatusTypeId == (int)ProjectStatusTypeEnum.Completed
                            && p.ProjectTags.Any(pt => allTagIds.Contains(pt.TagId)))
                        .SelectMany(p => p.ProjectTags)
                        .Where(pt => allTagIds.Contains(pt.TagId))
                        .Select(pt => pt.TagId)
                        .ToList(),
                    AverageRating = u.ReceivedReviews.Any()
                        ? u.ReceivedReviews.Average(r => r.Rating)
                        : 0.0
                })
                .ToListAsync();

            // 3. In-memory: score each specialist per project and build the result dictionary
            var result = new Dictionary<int, ICollection<UserMatchedResponse>>();

            foreach (var projectId in projectIds)
            {
                if (!tagsByProject.TryGetValue(projectId, out var projectTagIds) || projectTagIds.Count == 0)
                {
                    result[projectId] = [];
                    continue;
                }

                var totalProjectTags = projectTagIds.Count;

                var scored = matchingSpecialists
                    .Select(s =>
                    {
                        var matchingTagCount = s.UserTagIds.Intersect(projectTagIds).Count();
                        if (matchingTagCount == 0) return null;

                        var matchPercentage = (double)matchingTagCount / totalProjectTags * 100;
                        var experienceScore = s.ExperienceTagIds.Count(t => projectTagIds.Contains(t));

                        return new
                        {
                            s.User,
                            MatchPercentage = matchPercentage,
                            ExperienceScore = experienceScore,
                            s.AverageRating
                        };
                    })
                    .Where(s => s != null)
                    .OrderByDescending(s => s!.MatchPercentage)
                    .ThenByDescending(s => s!.ExperienceScore)
                    .ThenByDescending(s => s!.AverageRating)
                    .Take(MaxRecommendations)
                    .Select(s => new UserMatchedResponse(s!.User, Math.Round(s.MatchPercentage, 1)))
                    .ToList();

                result[projectId] = scored;
            }

            return result;
        }
    }
}
