using ITrade.Services.Requests;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IAuthService
    {
        public Task RegisterAsync(RegisterRequest request);
        public Task VerifyEmailAsync(string emailedToken);
        public Task<LoginResponse> LoginAsync(LoginRequest request);

    }
}
