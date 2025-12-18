using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> GetUserAsync();
        Task ChangeUsernameAsync(string newUsername);
        Task<int> AddProfileTagAsync(int tagId);
        Task RemoveProfileTagAsync(int tagId);
        Task<int> CreateProfileLinkAsync(string url);
        Task RemoveProfileLinkAsync(int profileLinkId);
    }
}
