using ITrade.Services.Requests;

namespace ITrade.Services.Interfaces
{
    public interface IAuthService
    {
        public Task RegisterAsync(RegisterRequest request);
        public Task VerifyEmailAsync(string emailedToken);
    }
}
