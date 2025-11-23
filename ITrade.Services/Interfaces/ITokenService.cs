namespace ITrade.Services.Interfaces
{
    public interface ITokenService
    {
        public string HashTokenString(string tokenString);
        Task<string> CreateVerifyEmailTokenAsync(int userId);

    }
}
