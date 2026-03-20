using ITrade.Common.Helpers;
using ITrade.DB;
using ITrade.DB.Enums;
using ITrade.Services.Interfaces;
using ITrade.Services.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ITrade.Services.Services
{
    public class MatchingService(
        Context context,
        ICurrentUserService currentUserService,
        IOptions<MatchingSettings> matchingOptions
    ) : IMatchingService
    {
        private readonly MatchingSettings options = matchingOptions.Value;

        private sealed record ResolvedMatchingPreferences(
            int TagMatchMaxPercentage,
            int ExperienceMaxPercentage,
            int ReviewsMaxPercentage);

        private sealed record SpecialistScoringInput(
            HashSet<int> TagIds,
            Dictionary<int, int> ExperienceTagCounts,
            double? AverageRating,
            int ActiveProjectCount,
            DateTime CreatedAt);

        public async Task<IDictionary<(int SpecialistId, int ProjectId), double>> ComputeMatchScoresAsync(
            ICollection<(int SpecialistId, int ProjectId)> pairs)
        {
            if (pairs.Count == 0)
            {
                return new Dictionary<(int, int), double>();
            }

            var specialistIds = pairs.Select(p => p.SpecialistId).Distinct().ToList();
            var projectIds = pairs.Select(p => p.ProjectId).Distinct().ToList();

            var viewerPreferences = await GetPreferencesForUserAsync(currentUserService.UserId);

            var projectTagRows = await context.ProjectTags
                .Where(pt => projectIds.Contains(pt.ProjectId))
                .Select(pt => new { pt.ProjectId, pt.TagId })
                .ToListAsync();

            var tagsByProject = projectTagRows
                .GroupBy(r => r.ProjectId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.TagId).ToHashSet());

            var allTagIds = tagsByProject.Values.SelectMany(t => t).Distinct().ToList();
            var specialistInputs = await GetSpecialistScoringInputsAsync(specialistIds, allTagIds);

            var result = new Dictionary<(int, int), double>();

            foreach (var (specialistId, projectId) in pairs)
            {
                if (!specialistInputs.TryGetValue(specialistId, out var specialistInput))
                {
                    result[(specialistId, projectId)] = 0;
                    continue;
                }

                var score = CalculateMatchScore(
                    specialistInput,
                    tagsByProject.GetValueOrDefault(projectId) ?? [],
                    viewerPreferences);

                result[(specialistId, projectId)] = score;
            }

            return result;
        }

        private double CalculateMatchScore(
            SpecialistScoringInput specialist,
            HashSet<int> projectTags,
            ResolvedMatchingPreferences preferences)
        {
            if (projectTags.Count == 0 || specialist.TagIds.Count == 0)
            {
                return 0;
            }

            var matchingTags = specialist.TagIds.Intersect(projectTags).ToList();
            if (matchingTags.Count == 0)
            {
                return 0;
            }

            var baseScore = 0d;
            if (preferences.TagMatchMaxPercentage > 0)
            {
                var matchPercentage = (double)matchingTags.Count / projectTags.Count;
                baseScore = matchPercentage * preferences.TagMatchMaxPercentage;
            }

            var experienceBonus = 0d;
            if (preferences.ExperienceMaxPercentage > 0)
            {
                var experienceScore = matchingTags.Sum(tagId => specialist.ExperienceTagCounts.GetValueOrDefault(tagId, 0));
                experienceBonus = Math.Min(
                    experienceScore * options.ExperiencePointsPerCompletedProject,
                    preferences.ExperienceMaxPercentage);
            }

            var reviewsScore = 0d;
            if (preferences.ReviewsMaxPercentage > 0)
            {
                reviewsScore = specialist.AverageRating.HasValue
                    ? (specialist.AverageRating.Value - 3) * (preferences.ReviewsMaxPercentage / 2d)
                    : GetMedianBonusForNewAccount(specialist.CreatedAt, preferences.ReviewsMaxPercentage);
            }

            var availabilityPenalty = options.AvailabilityPenaltyPerActiveProject > 0
                ? specialist.ActiveProjectCount * options.AvailabilityPenaltyPerActiveProject
                : 0;

            var totalScore = baseScore + experienceBonus + reviewsScore - availabilityPenalty;
            return Math.Round(Math.Clamp(totalScore, 0, 100), 1);
        }

        private double GetMedianBonusForNewAccount(DateTime accountCreatedAt, int reviewsMaxPercentage)
        {
            var recentDays = Math.Max(0, options.RecentAccountDaysForMedianReviewBonus);
            if (recentDays == 0)
            {
                return 0;
            }

            var isRecentAccount = accountCreatedAt >= DateTime.UtcNow.AddDays(-recentDays);
            return isRecentAccount ? reviewsMaxPercentage / 2d : 0;
        }

        public async Task<ICollection<ProjectMatchedResponse>> RecommandProjectsForSpecialist()
        {
            var userId = currentUserService.UserId;
            var viewerPreferences = await GetPreferencesForUserAsync(userId);

            var specialistInputs = await GetSpecialistScoringInputsAsync([userId], []);
            if (!specialistInputs.TryGetValue(userId, out var specialistInput) || specialistInput.TagIds.Count == 0)
            {
                return [];
            }

            var specialistTagIds = specialistInput.TagIds;

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

            var scoredProjects = matchingProjects
                .Select(p => new
                {
                    p.Project,
                    Score = CalculateMatchScore(specialistInput, p.ProjectTagIds, viewerPreferences)
                })
                .Where(p => p.Score > 0)
                .OrderByDescending(p => p.Score)
                .Take(options.MaxRecommendations)
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

            var viewerPreferences = await GetPreferencesForUserAsync(currentUserService.UserId);

            var projectTagRows = await context.ProjectTags
                .Where(pt => projectIds.Contains(pt.ProjectId))
                .Select(pt => new { pt.ProjectId, pt.TagId })
                .ToListAsync();

            var tagsByProject = projectTagRows
                .GroupBy(r => r.ProjectId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.TagId).ToHashSet());

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

            var specialistIds = await context.Users
                .Where(u => !u.IsDeleted
                    && u.UserRoleId == (int)UserRoleEnum.Specialist
                    && u.UserProfileTags.Any(upt => allTagIds.Contains(upt.TagId)))
                .Select(u => u.Id)
                .ToListAsync();

            var specialistInputs = await GetSpecialistScoringInputsAsync(specialistIds, allTagIds);

            var matchingSpecialists = await context.Users
                .Where(u => specialistIds.Contains(u.Id))
                .Select(u => new UserResponse(u.Id, u.Username, u.UserRole.Name))
                .ToListAsync();

            var userById = matchingSpecialists.ToDictionary(u => u.Id, u => u);
            var result = new Dictionary<int, ICollection<UserMatchedResponse>>();

            foreach (var projectId in projectIds)
            {
                if (!tagsByProject.TryGetValue(projectId, out var projectTagIds) || projectTagIds.Count == 0)
                {
                    result[projectId] = [];
                    continue;
                }

                var involvedSpecialists = involvedSpecialistsLookup.GetValueOrDefault(projectId) ?? [];

                var scored = specialistInputs
                    .Where(s => !involvedSpecialists.Contains(s.Key) && userById.ContainsKey(s.Key))
                    .Select(s => new
                    {
                        User = userById[s.Key],
                        Score = CalculateMatchScore(s.Value, projectTagIds, viewerPreferences)
                    })
                    .Where(s => s.Score > 0)
                    .OrderByDescending(s => s.Score)
                    .Take(options.MaxRecommendations)
                    .Select(s => new UserMatchedResponse(s.User, s.Score))
                    .ToList();

                result[projectId] = scored;
            }

            return result;
        }

        private async Task<Dictionary<int, SpecialistScoringInput>> GetSpecialistScoringInputsAsync(
            ICollection<int> specialistIds,
            ICollection<int> relevantTagIds)
        {
            if (specialistIds.Count == 0)
            {
                return [];
            }

            var specialistTagRows = await context.UserProfileTags
                .Where(upt => specialistIds.Contains(upt.UserId))
                .Select(upt => new { upt.UserId, upt.TagId })
                .ToListAsync();

            var tagsBySpecialist = specialistTagRows
                .GroupBy(r => r.UserId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.TagId).ToHashSet());

            var experienceQuery = context.Projects
                .Where(p => specialistIds.Contains(p.WorkerId ?? 0)
                    && p.ProjectStatusTypeId == (int)ProjectStatusTypeEnum.Completed)
                .SelectMany(p => p.ProjectTags.Select(pt => new { p.WorkerId, pt.TagId }));

            if (relevantTagIds.Count > 0)
            {
                experienceQuery = experienceQuery.Where(x => relevantTagIds.Contains(x.TagId));
            }

            var experienceRows = await experienceQuery.ToListAsync();

            var experienceBySpecialist = experienceRows
                .GroupBy(r => r.WorkerId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(x => x.TagId).ToDictionary(tg => tg.Key, tg => tg.Count()));

            var specialistRows = await context.Users
                .Where(u => specialistIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.CreatedAt,
                    AverageRating = u.ReceivedReviews.Any()
                        ? u.ReceivedReviews.Average(r => r.Rating)
                        : (double?)null,
                    ActiveProjectCount = u.AssignedProjects.Count(p =>
                        p.ProjectStatusTypeId == (int)ProjectStatusTypeEnum.InProgress
                        || p.ProjectStatusTypeId == (int)ProjectStatusTypeEnum.OnHold)
                })
                .ToListAsync();

            var result = new Dictionary<int, SpecialistScoringInput>();

            foreach (var specialist in specialistRows)
            {
                result[specialist.Id] = new SpecialistScoringInput(
                    tagsBySpecialist.GetValueOrDefault(specialist.Id) ?? [],
                    experienceBySpecialist.GetValueOrDefault(specialist.Id) ?? [],
                    specialist.AverageRating,
                    specialist.ActiveProjectCount,
                    specialist.CreatedAt);
            }

            return result;
        }

        private async Task<ResolvedMatchingPreferences> GetPreferencesForUserAsync(int userId)
        {
            var userPreferences = await context.UserMatchingPreferences
                .Where(p => p.UserId == userId)
                .Select(p => new
                {
                    p.TagMatchMaxPercentage,
                    p.ExperienceMaxPercentage,
                    p.ReviewsMaxPercentage
                })
                .FirstOrDefaultAsync();

            if (userPreferences is null)
            {
                return ResolvePreferences(null, null, null);
            }

            return ResolvePreferences(
                userPreferences.TagMatchMaxPercentage,
                userPreferences.ExperienceMaxPercentage,
                userPreferences.ReviewsMaxPercentage);
        }

        private ResolvedMatchingPreferences ResolvePreferences(
            int? tagMatchMaxPercentage,
            int? experienceMaxPercentage,
            int? reviewsMaxPercentage)
        {
            return new ResolvedMatchingPreferences(
                ClampPercentage(tagMatchMaxPercentage ?? options.DefaultTagMatchMaxPercentage),
                ClampPercentage(experienceMaxPercentage ?? options.DefaultExperienceMaxPercentage),
                ClampPercentage(reviewsMaxPercentage ?? options.DefaultReviewsMaxPercentage));
        }

        private static int ClampPercentage(int value)
        {
            return Math.Clamp(value, 0, 100);
        }
    }
}
