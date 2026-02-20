using ITrade.Services.Requests;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequest request);
        Task VerifyEmailAsync(string emailedToken);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task LogoutAsync(string refreshToken);
        Task ForgotPasswordAsync(ForgotPasswordRequest request);
        Task ResolveForgotPasswordAsync(ResolveForgotPasswordRequest resolveForgotPasswordRequest);
        Task ChangePasswordAsync(ChangePasswordRequest request);
    }
}
