using ITrade.DB;
using ITrade.DB.Entities;
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
        public async Task<UserResponse> GetUserAsync()
        {
            return await context.Users
                .Where(u => u.Id == currentUserService.UserId)
                .Select(u => new UserResponse
                (
                    u.Id,
                    u.Username,
                    u.Email,
                    u.UserRole.Name,
                    u.Notifications.Count(n => !n.IsRead)
                )).FirstOrDefaultAsync()
                ?? throw new Exception("User not found.");
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

        public async Task<int> AddProfileTagAsync(int tagId)
        {
            var newTag = new UserProfileTag
            {
                TagId = tagId,
                UserId = currentUserService.UserId
            };

            await context.UserProfileTags.AddAsync(newTag);
            await context.SaveChangesAsync();

            return newTag.Id;
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

        public async Task RemoveProfileTagAsync(int tagId)
        {
            var profileTag = await context.UserProfileTags
                .Where(upt => upt.TagId == tagId && upt.UserId == currentUserService.UserId)
                .FirstOrDefaultAsync()
                ?? throw new Exception("Profile tag not found.");

            context.UserProfileTags.Remove(profileTag);
            await context.SaveChangesAsync();
        }
    }
}
