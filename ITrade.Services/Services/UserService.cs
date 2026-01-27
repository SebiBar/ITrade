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
            return await context.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserProfileResponse
            (
                new UserResponse(u.Id, u.Username, u.UserRole.Name),
                u.UserProfileLinks
                    .Select(pl => new UserProfileLinkResponse(pl.Id, pl.UserId, pl.Url))
                    .ToList(),
                    u.UserProfileTags
                        .Select(pt => new UserProfileTagResponse(pt.Id, pt.Tag.Name))
                        .ToList(),
                    (
                        u.UserRoleId == (int)UserRoleEnum.Client
                            ? u.OwnedProjects
                            : u.AssignedProjects
                    )
                    .Where(p => !p.IsDeleted)
                    .Select(p => new ProjectResponse
                    (
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
                            .Select(t => new ProjectTagResponse(t.Id, t.Tag.Name))
                            .ToList(),
                        p.CreatedAt,
                        p.UpdatedAt
                    ))
                    .ToList(),
                    u.ReceivedReviews
                        .Select(r => new ReviewResponse
                        (
                            r.Id,
                            r.ReviewerId,
                            r.Reviewer.Username,
                            r.RevieweeId,
                            r.Reviewee.Username,
                            r.Rating,
                            r.Title,
                            r.Comment,
                            r.CreatedAt
                        ))
                        .ToList()
                ))
                .AsSplitQuery()
                .FirstOrDefaultAsync()
                        ?? throw new Exception("User not found.");
        }

        public async Task<CurrentUserResponse> GetCurrentUserProfileAsync()
        {
            var userId = currentUserService.UserId;

            return await context.Users
            .Where(u => u.Id == userId)
            .Select(u => new CurrentUserResponse
            (
                new UserResponse(u.Id, u.Username, u.UserRole.Name),
                u.Email,
                u.CreatedAt,
                u.UpdatedAt,
                u.IsEmailConfirmed,
                u.UserProfileLinks
                    .Select(pl => new UserProfileLinkResponse(pl.Id, pl.UserId, pl.Url))
                    .ToList(),
                    u.UserProfileTags
                        .Select(pt => new UserProfileTagResponse(pt.Id, pt.Tag.Name))
                        .ToList(),
                    (
                        u.UserRoleId == (int)UserRoleEnum.Client
                            ? u.OwnedProjects
                            : u.AssignedProjects
                    )
                    .Where(p => !p.IsDeleted)
                    .Select(p => new ProjectResponse
                    (
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
                            .Select(t => new ProjectTagResponse(t.Id, t.Tag.Name))
                            .ToList(),
                        p.CreatedAt,
                        p.UpdatedAt
                    ))
                    .ToList(),
                    u.ReceivedReviews
                        .Select(r => new ReviewResponse
                        (
                            r.Id,
                            r.ReviewerId,
                            r.Reviewer.Username,
                            r.RevieweeId,
                            r.Reviewee.Username,
                            r.Rating,
                            r.Title,
                            r.Comment,
                            r.CreatedAt
                        ))
                        .ToList()
                ))
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException("User not found.");
        }

        public async Task<ICollection<UserResponse>> SearchUsersAsync(string userName)
        {
            return await context.Users
                .Where(u => u.Username.Contains(userName))
                .Where(u => u.UserRoleId != (int)UserRoleEnum.Admin)
                .Select(u => new UserResponse
                (
                    u.Id,
                    u.Username,
                    u.UserRole.Name
                ))
                .ToListAsync();
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
    }
}
