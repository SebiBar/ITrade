namespace ITrade.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateVerifyEmailTokenAsync(int userId);

    }
}
