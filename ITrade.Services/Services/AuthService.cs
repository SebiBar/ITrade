using ITrade.Common.Helpers;
using ITrade.DB;
using ITrade.DB.Entities;
using ITrade.DB.Enums;
using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using ITrade.Services.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ITrade.Services.Services
{
    public class AuthService(
        Context context,
        IPasswordHasher<User> hasher,
        ITokenService tokenService,
        IEmailService emailService,
        ITemplateService templateService,
        IOptions<TemplateSettings> templateSettings,
        IOptions<UrlSettings> urlSettings
        ) : IAuthService
    {
        public async Task RegisterAsync(RegisterRequest registerRequest)
        {
            ValidateRegisterRequest(registerRequest);

            var user = new User
            {
                Email = registerRequest.Email,
                FullName = registerRequest.FullName,
                UserRoleId = (int)registerRequest.Role
            };

            user.PasswordHash = hasher.HashPassword(user, registerRequest.Password);

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var token = await tokenService.CreateVerifyEmailTokenAsync(user.Id);

            await SendVerificationEmailAsync(user.Email, user.FullName, token);
        }

        public async Task VerifyEmailAsync(string emailedToken)
        {
            if (emailedToken == null)
            {
                throw new ArgumentException("Invalid token.", nameof(emailedToken));
            }

            var emailedHashed = tokenService.HashTokenString(emailedToken);

            var token = await context.Tokens
                .Where(t => t.TokenStringHash == emailedHashed)
                .Include(t => t.User)
                .FirstOrDefaultAsync()
                ?? throw new ArgumentException("Invalid token.", nameof(emailedToken));

            if (token.ExpirationDate < DateTime.UtcNow)
            {
                throw new ArgumentException("Invalid token.", nameof(emailedToken));
            }

            var user = token.User;
            user.IsEmailConfirmed = true;
            user.UpdatedAt = DateTime.UtcNow;

            context.Tokens.Remove(token);
            await context.SaveChangesAsync();
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            ValidateLoginRequest(request);

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email)
                ?? throw new ArgumentException("Invalid credentials.", nameof(request));

            if (hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            {
                throw new ArgumentException("Invalid credentials.", nameof(request));
            }

            if (user.IsEmailConfirmed == false)
            {
                throw new InvalidOperationException("Please confirm your email.");
            }

            var jwt = tokenService.CreateJwtAsync(user.Id);
            var refresh = await tokenService.CreateRefreshTokenAsync(user.Id);

            return new LoginResponse
            (
                user.Id,
                user.FullName,
                user.Email,
                jwt,
                refresh
            );
        }

        private void ValidateLoginRequest(LoginRequest req)
        {
            if (req == null)
                throw new ArgumentException("Request cannot be null.", nameof(req));
            if (string.IsNullOrWhiteSpace(req.Email))
                throw new ArgumentException("Email is required.", nameof(req.Email));
            if (string.IsNullOrWhiteSpace(req.Password))
                throw new ArgumentException("Password is required.", nameof(req.Password));
        }

        private async void ValidateRegisterRequest(RegisterRequest req)
        {
            if (req == null)
                throw new ArgumentException("Request cannot be null.", nameof(req));

            if (req.Role == UserRoleEnum.Admin)
                throw new ArgumentException("Cannot register as an admin.", nameof(req.Role));

            if (string.IsNullOrWhiteSpace(req.FullName))
                throw new ArgumentException("Full name is required.", nameof(req.FullName));

            if (req.FullName.Length > 200)
                throw new ArgumentException("Full name too long (max 200).", nameof(req.FullName));

            if (string.IsNullOrWhiteSpace(req.Password))
                throw new ArgumentException("Password is required.", nameof(req.Password));

            if (string.IsNullOrWhiteSpace(req.Email))
                throw new ArgumentException("Email is required.", nameof(req.Email));

            if (!System.Net.Mail.MailAddress.TryCreate(req.Email, out _))
                throw new ArgumentException("Email format is invalid.", nameof(req.Email));

            if (await context.Users.FirstOrDefaultAsync(u => u.Email == req.Email) != null)
                throw new InvalidOperationException("User with this email already exists.");
        }

        private async Task SendVerificationEmailAsync(string toEmail, string fullName, string tokenValue)
        {
            var apiBase = urlSettings.Value.ApiBase.TrimEnd('/');
            var verifyLink = $"{apiBase}/auth/verify-email?token={tokenValue}";

            var model = new Dictionary<string, string>
            {
                ["FullName"] = string.IsNullOrWhiteSpace(fullName) ? "there" : fullName,
                ["VerifyLink"] = verifyLink
            };

            var html = await templateService.RenderAsync(templateSettings.Value.Email.VerifyHtml, model);
            var text = await templateService.RenderAsync(templateSettings.Value.Email.VerifyText, model);

            await emailService.SendEmailAsync(
                toEmail: toEmail,
                title: "Verify your ITrade account",
                textBody: text,
                htmlBody: html
            );
        }
    }
}
