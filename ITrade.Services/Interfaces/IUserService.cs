using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IUserService
    {
        Task<CurrentUserResponse> GetCurrentUserProfileAsync();
        Task<UserProfileResponse> GetUserProfileAsync(int userId);
        Task<ICollection<UserResponse>> SearchUsersAsync(string userName);
        Task ChangeUsernameAsync(string newUsername);
        Task<int> CreateProfileLinkAsync(string url);
        Task RemoveProfileLinkAsync(int profileLinkId);
    }
}
