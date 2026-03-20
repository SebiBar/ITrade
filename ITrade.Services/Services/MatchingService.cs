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

        public async Task<IDictionary<(int SpecialistId, int ProjectId), double>> ComputeMatchScoresAsync(
            ICollection<(int SpecialistId, int ProjectId)> pairs)
        {
            if (pairs.Count == 0)
            {
                return new Dictionary<(int, int), double>();
            }

            var specialistIds = pairs.Select(p => p.SpecialistId).Distinct().ToList();
            var projectIds = pairs.Select(p => p.ProjectId).Distinct().ToList();

            // 1. Batch fetch all project tags
            var projectTagRows = await context.ProjectTags
                .Where(pt => projectIds.Contains(pt.ProjectId))
                .Select(pt => new { pt.ProjectId, pt.TagId })
                .ToListAsync();

            var tagsByProject = projectTagRows
                .GroupBy(r => r.ProjectId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.TagId).ToHashSet());

            // 2. Batch fetch all specialist profile tags
            var specialistTagRows = await context.UserProfileTags
                .Where(upt => specialistIds.Contains(upt.UserId))
                .Select(upt => new { upt.UserId, upt.TagId })
                .ToListAsync();

            var tagsBySpecialist = specialistTagRows
                .GroupBy(r => r.UserId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.TagId).ToHashSet());

            // 3. Batch fetch experience data (completed projects with matching tags)
            var allTagIds = tagsByProject.Values.SelectMany(t => t).Distinct().ToList();

            var experienceRows = await context.Projects
                .Where(p => specialistIds.Contains(p.WorkerId ?? 0)
                    && p.ProjectStatusTypeId == (int)ProjectStatusTypeEnum.Completed)
                .SelectMany(p => p.ProjectTags.Select(pt => new { p.WorkerId, pt.TagId }))
                .Where(x => allTagIds.Contains(x.TagId))
                .ToListAsync();

            var experienceBySpecialist = experienceRows
                .GroupBy(r => r.WorkerId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(x => x.TagId).ToDictionary(tg => tg.Key, tg => tg.Count()));

            // 4. Batch fetch average ratings for all specialists
            var ratingRows = await context.Users
                .Where(u => specialistIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    AverageRating = u.ReceivedReviews.Any()
                        ? u.ReceivedReviews.Average(r => r.Rating)
                        : (double?)null
                })
                .ToListAsync();

            var ratingsBySpecialist = ratingRows.ToDictionary(r => r.Id, r => r.AverageRating);

            // 5. Calculate scores for each pair
            var result = new Dictionary<(int, int), double>();

            foreach (var (specialistId, projectId) in pairs)
            {
                var score = CalculateMatchScore(
                    tagsBySpecialist.GetValueOrDefault(specialistId) ?? [],
                    tagsByProject.GetValueOrDefault(projectId) ?? [],
                    experienceBySpecialist.GetValueOrDefault(specialistId) ?? [],
                    ratingsBySpecialist.GetValueOrDefault(specialistId));

                result[(specialistId, projectId)] = score;
            }

            return result;
        }

        /// <summary>
        /// Calculates a match score (0-100) based on:
        /// - Tag match percentage (base score, up to 60%)
        /// - Experience bonus (up to 20% based on completed projects with matching tags)
        /// - Review rating bonus/penalty (up to +-20% based on average rating)
        /// </summary>
        private static double CalculateMatchScore(
            HashSet<int> specialistTags,
            HashSet<int> projectTags,
            Dictionary<int, int> tagExperience,
            double? averageRating)
        {
            if (projectTags.Count == 0 || specialistTags.Count == 0)
            {
                return 0;
            }

            var matchingTags = specialistTags.Intersect(projectTags).ToList();
            if (matchingTags.Count == 0)
            {
                return 0;
            }

            // Base score: tag match percentage scaled to 60% max
            var matchPercentage = (double)matchingTags.Count / projectTags.Count;
            var baseScore = matchPercentage * 60;

            // Experience bonus: up to 20% based on completed projects with matching tags
            var experienceScore = matchingTags.Sum(tagId => tagExperience.GetValueOrDefault(tagId, 0));
            var experienceBonus = Math.Min(experienceScore * 2, 20);

            // Review rating modifier: -20% to +20% based on average rating (1-5 scale)
            // No reviews = neutral (0), 3 stars = neutral, 5 stars = +20%, 1 star = -20%
            double ratingModifier = 0;
            if (averageRating.HasValue)
            {
                // Map 1-5 rating to -20 to +20 range: (rating - 3) * 10
                ratingModifier = (averageRating.Value - 3) * 10;
            }

            var totalScore = baseScore + experienceBonus + ratingModifier;
            return Math.Round(Math.Clamp(totalScore, 0, 100), 1);
        }

        public async Task<ICollection<ProjectMatchedResponse>> RecommandProjectsForSpecialist()
        {
            var userId = currentUserService.UserId;

            // Get the specialist's profile tags
            var specialistTagIds = await context.UserProfileTags
                .Where(upt => upt.UserId == userId)
                .Select(upt => upt.TagId)
                .ToHashSetAsync();

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

            // Get the specialist's average rating
            var specialistRating = await context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.ReceivedReviews.Any()
                    ? (double?)u.ReceivedReviews.Average(r => r.Rating)
                    : null)
                .FirstOrDefaultAsync();

            // Find open projects (Hiring status, no worker assigned) that match specialist's tags
            // Exclude projects the specialist has already applied to or been invited to
            var matchingProjects = await context.Projects
                .Where(p => !p.IsDeleted
                    && p.WorkerId == null
                    && p.ProjectStatusTypeId == (int)ProjectStatusTypeEnum.Hiring
                    && !p.Requests.Any(r => r.SenderId == userId || r.ReceiverId == userId)
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
                    ProjectTagIds = p.ProjectTags.Select(pt => pt.TagId).ToHashSet()
                })
                .ToListAsync();

            // Calculate match score for each project
            var scoredProjects = matchingProjects
                .Select(p =>
                {
                    var score = CalculateMatchScore(
                        specialistTagIds,
                        p.ProjectTagIds,
                        tagExperience,
                        specialistRating);

                    return new
                    {
                        p.Project,
                        Score = score
                    };
                })
                .Where(p => p.Score > 0)
                .OrderByDescending(p => p.Score)
                .Take(MaxRecommendations)
                .Select(p => new ProjectMatchedResponse(p.Project, p.Score))
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

            // Get all tags for all requested projects
            var projectTagRows = await context.ProjectTags
                .Where(pt => projectIds.Contains(pt.ProjectId))
                .Select(pt => new { pt.ProjectId, pt.TagId })
                .ToListAsync();

            // Build per-project tag dictionaries
            var tagsByProject = projectTagRows
                .GroupBy(r => r.ProjectId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.TagId).ToHashSet());

            // Collect the union of all tag IDs across every project
            var allTagIds = tagsByProject.Values.SelectMany(t => t).Distinct().ToList();

            if (allTagIds.Count == 0)
            {
                return projectIds.ToDictionary(id => id, _ => (ICollection<UserMatchedResponse>)[]);
            }

            var involvedSpecialistsByProject = await context.Requests
                .Where(r => projectIds.Contains(r.ProjectId))
                .Select(r => new 
                { 
                    r.ProjectId, 
                    SpecialistId = r.RequestTypeId == (int)ProjectRequestTypeEnum.Invitation ? r.ReceiverId : r.SenderId 
                })
                .ToListAsync();

            var involvedSpecialistsLookup = involvedSpecialistsByProject
                .GroupBy(r => r.ProjectId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.SpecialistId).ToHashSet());

            // Find all specialists whose profile tags overlap with any of those tags
            var matchingSpecialistsRaw = await context.Users
                .Where(u => !u.IsDeleted
                    && u.UserRoleId == (int)UserRoleEnum.Specialist
                    && u.UserProfileTags.Any(upt => allTagIds.Contains(upt.TagId)))
                .Select(u => new
                {
                    User = new UserResponse(u.Id, u.Username, u.UserRole.Name),
                    UserTagIds = u.UserProfileTags.Select(upt => upt.TagId).ToList(),
                    ExperienceTagCounts = u.AssignedProjects
                        .Where(p => p.ProjectStatusTypeId == (int)ProjectStatusTypeEnum.Completed)
                        .SelectMany(p => p.ProjectTags)
                        .Where(pt => allTagIds.Contains(pt.TagId))
                        .GroupBy(pt => pt.TagId)
                        .Select(g => new { TagId = g.Key, Count = g.Count() })
                        .ToList(),
                    AverageRating = u.ReceivedReviews.Any()
                        ? (double?)u.ReceivedReviews.Average(r => r.Rating)
                        : null
                })
                .ToListAsync();

            // Convert to in-memory collections for efficient scoring
            var matchingSpecialists = matchingSpecialistsRaw
                .Select(s => new
                {
                    s.User,
                    UserTagIds = s.UserTagIds.ToHashSet(),
                    ExperienceTagCounts = s.ExperienceTagCounts.ToDictionary(x => x.TagId, x => x.Count),
                    s.AverageRating
                })
                .ToList();

            // In-memory: score each specialist per project and build the result dictionary
            var result = new Dictionary<int, ICollection<UserMatchedResponse>>();

            foreach (var projectId in projectIds)
            {
                if (!tagsByProject.TryGetValue(projectId, out var projectTagIds) || projectTagIds.Count == 0)
                {
                    result[projectId] = [];
                    continue;
                }

                // Get specialists already involved with this project (invited or applied)
                var involvedSpecialists = involvedSpecialistsLookup.GetValueOrDefault(projectId) ?? [];

                var scored = matchingSpecialists
                    .Where(s => !involvedSpecialists.Contains(s.User.Id))
                    .Select(s =>
                    {
                        var score = CalculateMatchScore(
                            s.UserTagIds,
                            projectTagIds,
                            s.ExperienceTagCounts,
                            s.AverageRating);

                        if (score == 0) return null;

                        return new
                        {
                            s.User,
                            Score = score
                        };
                    })
                    .Where(s => s != null)
                    .OrderByDescending(s => s!.Score)
                    .Take(MaxRecommendations)
                    .Select(s => new UserMatchedResponse(s!.User, s.Score))
                    .ToList();

                result[projectId] = scored;
            }

            return result;
        }
    }
}
