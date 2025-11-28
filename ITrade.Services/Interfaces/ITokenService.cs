using ITrade.DB.Entities;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateJwt(int userId, string userRole);
        Task<string> CreateRefreshTokenAsync(int userId);
        Task<string> CreateVerifyEmailTokenAsync(int userId);
        string HashTokenString(string tokenString);
        Task<RefreshTokensResponse> RefreshTokensAsync(string refreshToken);
    }
}
