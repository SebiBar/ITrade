using ITrade.DB.Enums;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IUserService
    {
        Task<CurrentUserResponse> GetCurrentUserProfileAsync();
        Task<UserProfileResponse> GetUserProfileAsync(int userId);
        Task ChangeUsernameAsync(string newUsername);
        Task<int> CreateProfileLinkAsync(string url);
        Task RemoveProfileLinkAsync(int profileLinkId);
        Task UpdateMatchingPreferencesAsync(MatchingPreferencesEnum preference);
        Task SoftDeleteAccountAsync();
        Task RestoreUserAsync(int userId);
        Task HardDeleteUserAsync(int userId);
    }
}
