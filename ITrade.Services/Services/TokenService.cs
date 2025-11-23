using ITrade.Common.Helpers;
using ITrade.DB;
using ITrade.DB.Entities;
using ITrade.DB.Enums;
using ITrade.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace ITrade.Services.Services
{
    public class TokenService(
        Context context, 
        IOptions<TokenSettings> tokenSettings
        ) : ITokenService
    {
        private readonly int tokenLength = tokenSettings.Value.TokenLength;

        public async Task<string> CreateVerifyEmailTokenAsync(int userId)
        {
            var tokenString = GenerateTokenString();

            var verifyEmailToken = new Token
            {
                TokenStringHash = HashTokenString(tokenString),
                UserId = userId,
                TokenTypeId = (int)TokenTypeEnum.VerifyEmail,
                ExpirationDate = DateTime.UtcNow.AddDays(tokenSettings.Value.VerifyEmailExpiresInDays),
            };

            await context.Tokens.AddAsync(verifyEmailToken);
            await context.SaveChangesAsync();

            return tokenString;
        }

        public string HashTokenString(string tokenString)
        {
            var bytes = Encoding.UTF8.GetBytes(tokenString);
            return Convert.ToHexString(SHA256.HashData(bytes));
        }

        private string GenerateTokenString()
        {
            // Cryptographically secure random number generator
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[tokenLength];
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
