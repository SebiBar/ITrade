using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface ITokenService
    {
        public string CreateJwtAsync(int userId);
        public Task<string> CreateRefreshTokenAsync(int userId);
        public Task<string> CreateVerifyEmailTokenAsync(int userId);
        public string HashTokenString(string tokenString);
        public Task<RefreshTokensResponse> RefreshTokensAsync(string refreshToken);
    }
}
