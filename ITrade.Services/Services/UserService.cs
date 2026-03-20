using ITrade.DB;
using ITrade.DB.Entities;
using ITrade.DB.Enums;
using ITrade.Services.Interfaces;
using ITrade.Services.Responses;
using Microsoft.EntityFrameworkCore;

namespace ITrade.Services.Services
{
    public class UserService
    (
        Context context,
        ICurrentUserService currentUserService
    ) : IUserService
    {
        public async Task<UserProfileResponse> GetUserProfileAsync(int userId)
        {
            var userData = await context.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    User = new UserResponse(u.Id, u.Username, u.UserRole.Name),
                    u.UserRoleId,
                    ProfileLinks = u.UserProfileLinks
                        .Select(pl => new UserProfileLinkResponse(pl.Id, pl.UserId, pl.Url))
                        .ToList(),
                    ProfileTags = u.UserProfileTags
                        .Select(pt => new UserProfileTagResponse(pt.Id, pt.Tag.Name))
                        .ToList(),
                    OwnedProjects = u.OwnedProjects
                        .Select(p => new ProjectResponse(
                            p.Id, p.Name, p.Description, p.OwnerId, p.Owner.Username,
                            p.WorkerId, p.Worker != null ? p.Worker.Username : null,
                            p.Deadline, p.ProjectStatusTypeId, p.ProjectStatusType.Name,
                            p.ProjectTags.Select(t => new ProjectTagResponse(t.Id, t.Tag.Name)).ToList(),
                            p.CreatedAt, p.UpdatedAt))
                        .ToList(),
                    AssignedProjects = u.AssignedProjects
                        .Select(p => new ProjectResponse(
                            p.Id, p.Name, p.Description, p.OwnerId, p.Owner.Username,
                            p.WorkerId, p.Worker != null ? p.Worker.Username : null,
                            p.Deadline, p.ProjectStatusTypeId, p.ProjectStatusType.Name,
                            p.ProjectTags.Select(t => new ProjectTagResponse(t.Id, t.Tag.Name)).ToList(),
                            p.CreatedAt, p.UpdatedAt))
                        .ToList(),
                    Reviews = u.ReceivedReviews
                        .Select(r => new ReviewResponse(
                            r.Id, r.ReviewerId, r.Reviewer.Username, r.RevieweeId,
                            r.Reviewee.Username, r.Rating, r.Title, r.Comment, r.CreatedAt))
                        .ToList()
                })
                .AsSplitQuery()
                .FirstOrDefaultAsync()
                ?? throw new Exception("User not found.");

            // Combine projects in memory based on user role
            var projects = userData.UserRoleId == (int)UserRoleEnum.Client
                ? userData.OwnedProjects
                : userData.AssignedProjects;

            return new UserProfileResponse(
                userData.User,
                userData.ProfileLinks,
                userData.ProfileTags,
                projects,
                userData.Reviews);
        }

        public async Task<CurrentUserResponse> GetCurrentUserProfileAsync()
        {
            var userId = currentUserService.UserId;

            var userData = await context.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    User = new UserResponse(u.Id, u.Username, u.UserRole.Name),
                    u.Email,
                    u.CreatedAt,
                    u.UpdatedAt,
                    u.IsEmailConfirmed,
                    u.UserRoleId,
                    ProfileLinks = u.UserProfileLinks
                        .Select(pl => new UserProfileLinkResponse(pl.Id, pl.UserId, pl.Url))
                        .ToList(),
                    ProfileTags = u.UserProfileTags
                        .Select(pt => new UserProfileTagResponse(pt.Id, pt.Tag.Name))
                        .ToList(),
                    OwnedProjects = u.OwnedProjects
                        .Select(p => new ProjectResponse(
                            p.Id, p.Name, p.Description, p.OwnerId, p.Owner.Username,
                            p.WorkerId, p.Worker != null ? p.Worker.Username : null,
                            p.Deadline, p.ProjectStatusTypeId, p.ProjectStatusType.Name,
                            p.ProjectTags.Select(t => new ProjectTagResponse(t.Id, t.Tag.Name)).ToList(),
                            p.CreatedAt, p.UpdatedAt))
                        .ToList(),
                    AssignedProjects = u.AssignedProjects
                        .Select(p => new ProjectResponse(
                            p.Id, p.Name, p.Description, p.OwnerId, p.Owner.Username,
                            p.WorkerId, p.Worker != null ? p.Worker.Username : null,
                            p.Deadline, p.ProjectStatusTypeId, p.ProjectStatusType.Name,
                            p.ProjectTags.Select(t => new ProjectTagResponse(t.Id, t.Tag.Name)).ToList(),
                            p.CreatedAt, p.UpdatedAt))
                        .ToList(),
                    Reviews = u.ReceivedReviews
                        .Select(r => new ReviewResponse(
                            r.Id, r.ReviewerId, r.Reviewer.Username, r.RevieweeId,
                            r.Reviewee.Username, r.Rating, r.Title, r.Comment, r.CreatedAt))
                        .ToList()
                })
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException("User not found.");

            // Combine projects in memory based on user role
            var projects = userData.UserRoleId == (int)UserRoleEnum.Client
                ? userData.OwnedProjects
                : userData.AssignedProjects;

            return new CurrentUserResponse(
                userData.User,
                userData.Email,
                userData.CreatedAt,
                userData.UpdatedAt,
                userData.IsEmailConfirmed,
                userData.ProfileLinks,
                userData.ProfileTags,
                projects,
                userData.Reviews
            );
        }

        public async Task ChangeUsernameAsync(string newUsername)
        {
            var user = context.Users.Find(currentUserService.UserId)
                ?? throw new Exception("User not found.");

            user.Username = newUsername;
            user.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }

        public async Task<int> CreateProfileLinkAsync(string url)
        {
            var newLink = new UserProfileLink
            {
                Url = url,
                UserId = currentUserService.UserId
            };

            await context.UserProfileLinks.AddAsync(newLink);
            await context.SaveChangesAsync();

            return newLink.Id;
        }

        public async Task RemoveProfileLinkAsync(int profileLinkId)
        {
            var profileLink = await context.UserProfileLinks
                .Where(upl => upl.Id == profileLinkId && upl.UserId == currentUserService.UserId)
                .FirstOrDefaultAsync()
                ?? throw new Exception("Profile link not found.");

            context.UserProfileLinks.Remove(profileLink);
            await context.SaveChangesAsync();
        }

        public async Task SoftDeleteAccountAsync()
        {
            var user = await context.Users.FindAsync(currentUserService.UserId)
                ?? throw new KeyNotFoundException("User not found.");

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }

        public async Task RestoreUserAsync(int userId)
        {
            if (currentUserService.UserRole != UserRoleEnum.Admin)
                throw new UnauthorizedAccessException("Only admins can restore users.");

            var user = await context.Users
                .IgnoreQueryFilters()
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException("Deleted User not found.");

            user.IsDeleted = false;
            user.UpdatedAt= DateTime.UtcNow;

            await context.SaveChangesAsync();
        }

        public async Task HardDeleteUserAsync(int userId)
        {
            var user = await context.Users
                .IgnoreQueryFilters()
                .Include(u => u.Tokens)
                .Include(u => u.Notifications)
                .Include(u => u.UserProfileLinks)
                .Include(u => u.UserProfileTags)
                .Include(u => u.SentReviews)
                .Include(u => u.ReceivedReviews)
                .Include(u => u.SentRequests)
                .Include(u => u.ReceivedRequests)
                .Include(u => u.OwnedProjects)
                    .ThenInclude(p => p.ProjectTags)
                .Include(u => u.OwnedProjects)
                    .ThenInclude(p => p.Requests)
                .Include(u => u.AssignedProjects)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new KeyNotFoundException("User not found.");

            // Remove tokens
            context.Tokens.RemoveRange(user.Tokens);

            // Remove notifications
            context.Notifications.RemoveRange(user.Notifications);

            // Remove profile links and tags
            context.UserProfileLinks.RemoveRange(user.UserProfileLinks);
            context.UserProfileTags.RemoveRange(user.UserProfileTags);

            // Remove reviews (both sent and received)
            context.Reviews.RemoveRange(user.SentReviews);
            context.Reviews.RemoveRange(user.ReceivedReviews);

            // Remove requests (both sent and received)
            context.Requests.RemoveRange(user.SentRequests);
            context.Requests.RemoveRange(user.ReceivedRequests);

            // Remove owned projects and their related data
            foreach (var project in user.OwnedProjects)
            {
                context.ProjectTags.RemoveRange(project.ProjectTags);
                context.Requests.RemoveRange(project.Requests);
            }
            context.Projects.RemoveRange(user.OwnedProjects);

            // Unassign from assigned projects (don't delete them)
            foreach (var project in user.AssignedProjects)
            {
                project.WorkerId = null;
                project.Worker = null;
            }

            // Finally remove the user
            context.Users.Remove(user);

            await context.SaveChangesAsync();
        }

        public async Task UpdateMatchingPreferencesAsync(MatchingPreferencesEnum preference)
        {
            var userId = currentUserService.UserId;

            var percentages = preference switch
            {
                MatchingPreferencesEnum.Balanced => (TagMatch: 60, Experience: 20, Reviews: 20),
                MatchingPreferencesEnum.PrioritizeSkills => (TagMatch: 80, Experience: 10, Reviews: 10),
                MatchingPreferencesEnum.PrioritizeReputation => (TagMatch: 40, Experience: 30, Reviews: 30),
                _ => throw new ArgumentOutOfRangeException(nameof(preference), "Invalid matching preference option.")
            };

            var user = await context.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.Id })
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException("User not found.");

            var existingPreferences = await context.UserMatchingPreferences
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (existingPreferences is null)
            {
                await context.UserMatchingPreferences.AddAsync(new UserMatchingPreferences
                {
                    UserId = user.Id,
                    TagMatchMaxPercentage = percentages.TagMatch,
                    ExperienceMaxPercentage = percentages.Experience,
                    ReviewsMaxPercentage = percentages.Reviews
                });
            }
            else
            {
                existingPreferences.TagMatchMaxPercentage = percentages.TagMatch;
                existingPreferences.ExperienceMaxPercentage = percentages.Experience;
                existingPreferences.ReviewsMaxPercentage = percentages.Reviews;
                existingPreferences.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
        }
    }
}
