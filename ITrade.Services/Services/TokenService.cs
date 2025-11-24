using ITrade.Common.Helpers;
using ITrade.DB;
using ITrade.DB.Entities;
using ITrade.DB.Enums;
using ITrade.Services.Interfaces;
using ITrade.Services.Responses;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ITrade.Services.Services
{
    public class TokenService(
        Context context, 
        IOptions<TokenSettings> tokenSettings,
        IOptions<JwtSettings> jwtSettings
        ) : ITokenService
    {
        private readonly int tokenLength = tokenSettings.Value.TokenLength;

        public string CreateJwtAsync(int userId)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Secret)),
                SecurityAlgorithms.HmacSha256
            );

            var jwt = new JwtSecurityToken(
                issuer: jwtSettings.Value.Issuer,
                audience: jwtSettings.Value.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(jwtSettings.Value.ExpiresInHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public async Task<string> CreateRefreshTokenAsync(int userId)
        {
            var tokenString = GenerateTokenString();

            var refreshToken = new Token
            {
                TokenStringHash = HashTokenString(tokenString),
                UserId = userId,
                TokenTypeId = (int)TokenTypeEnum.Refresh,
                ExpirationDate = DateTime.UtcNow.AddDays(tokenSettings.Value.RefreshExpiresInDays),
            };

            await context.Tokens.AddAsync(refreshToken);
            await context.SaveChangesAsync();

            return tokenString;
        }

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

        public async Task<RefreshTokensResponse> RefreshTokensAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentException("Invalid or expired token.", nameof(refreshToken));
            }

            var oldRefresh = context.Tokens
                .FirstOrDefault(t => t.TokenStringHash == HashTokenString(refreshToken)
                    && t.TokenTypeId == (int)TokenTypeEnum.Refresh
                    && t.ExpirationDate > DateTime.UtcNow)
                ?? throw new ArgumentException("Invalid or expired token.", nameof(refreshToken));

            var newJwt = CreateJwtAsync(oldRefresh.UserId);
            var newRefresh = await CreateRefreshTokenAsync(oldRefresh.UserId);

            context.Tokens.Remove(oldRefresh);
            await context.SaveChangesAsync();

            return new RefreshTokensResponse(newJwt, newRefresh);
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
