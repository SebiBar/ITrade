using ITrade.DB;
using ITrade.DB.Entities;
using ITrade.DB.Enums;
using ITrade.Services.Interfaces;
using System.Security.Cryptography;

namespace ITrade.Services.Services
{
    public class TokenService(Context context) : ITokenService
    {
        private int _tokenLength = 64;
        private int _verifyEmailTokenExpirationDays = 30;

        public async Task<string> CreateVerifyEmailTokenAsync(int userId)
        {
            var verifyEmailToken = new Token
            {
                TokenString = GenerateTokenString(),
                UserId = userId,
                TokenTypeId = (int)TokenTypeEnum.VerifyEmail,
                ExpirationDate = DateTime.UtcNow.AddDays(_verifyEmailTokenExpirationDays),
            };

            context.Tokens.Add(verifyEmailToken);
            await context.SaveChangesAsync();

            return verifyEmailToken.TokenString;
        }

        private string GenerateTokenString()
        {
            // Cryptographically secure random number generator
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[_tokenLength];
            rng.GetBytes(bytes);

            // Convert to URL-safe Base64 string
            var token = Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');

            return token;
        }
    }
}
