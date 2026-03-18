using ITrade.Common.Helpers;
using ITrade.DB;
using ITrade.DB.Entities;
using ITrade.DB.Enums;
using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using ITrade.Services.Responses;
using Microsoft.AspNetCore.Http;
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
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor,
        IOptions<TemplateSettings> templateSettings,
        IOptions<UrlSettings> urlSettings
    ) : IAuthService
    {
        public async Task RegisterAsync(RegisterRequest registerRequest)
        {
            await ValidateRegisterRequest(registerRequest);

            var user = new User
            {
                Email = registerRequest.Email,
                Username = registerRequest.Username,
                UserRoleId = (int)registerRequest.Role
            };

            user.PasswordHash = hasher.HashPassword(user, registerRequest.Password);

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var token = await tokenService.CreateVerifyEmailTokenAsync(user.Id);

            await SendVerificationEmailAsync(user.Email, user.Username, token);
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
                .Where(t => t.TokenTypeId == (int)TokenTypeEnum.VerifyEmail)
                .Include(t => t.User)
                .FirstOrDefaultAsync()
                ?? throw new ArgumentException("Invalid token.", nameof(emailedToken));

            if (token.ExpirationDate < DateTime.UtcNow)
            {
                throw new ArgumentException("Invalid token.", nameof(emailedToken));
            }

            var user = token.User;
            
            if (!user.IsEmailConfirmed)
            {
                user.IsEmailConfirmed = true;
                user.UpdatedAt = DateTime.UtcNow;
            }

            context.Tokens.Remove(token);
            await context.SaveChangesAsync();
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            ValidateLoginRequest(request);

            var user = await context.Users
                .Include(u => u.UserRole)
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

            var jwt = tokenService.CreateJwt(user.Id, user.UserRole.Name);
            var refresh = await tokenService.CreateRefreshTokenAsync(user.Id);

            return new LoginResponse
            (
                new UserResponse(user.Id, user.Username, user.UserRole.Name),
                jwt,
                refresh
            );
        }

        public async Task LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentException("Refresh token is required.", nameof(refreshToken));
            }

            var hashedToken = tokenService.HashTokenString(refreshToken);

            var token = await context.Tokens
                .Where(t => t.TokenStringHash == hashedToken)
                .Where(t => t.TokenTypeId == (int)TokenTypeEnum.Refresh)
                .FirstOrDefaultAsync();

            if (token != null)
            {
                context.Tokens.Remove(token);
                await context.SaveChangesAsync();
            }
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            ValidateEmail(request.Email);

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email)
                ?? throw new ArgumentException("User with this email does not exist.", nameof(request.Email));

            var token = await tokenService.CreateForgotPasswordTokenAsync(user.Id);

            await SendForgotPasswordEmailAsync(user.Email, user.Username, token);
        }

        public async Task ResolveForgotPasswordAsync(ResolveForgotPasswordRequest resolveForgotPasswordRequest)
        {
            ValidatePassword(resolveForgotPasswordRequest.NewPassword);

            var emailedHashed = tokenService.HashTokenString(resolveForgotPasswordRequest.EmailedToken);

            var token = await context.Tokens
                .Where(t => t.TokenStringHash == emailedHashed)
                .Where(t => t.TokenTypeId == (int)TokenTypeEnum.ForgotPassword)
                .Include(t => t.User)
                .FirstOrDefaultAsync()
                ?? throw new ArgumentException("Invalid token.", nameof(resolveForgotPasswordRequest.EmailedToken));

            if (token.ExpirationDate < DateTime.UtcNow)
            {
                throw new ArgumentException("Invalid token.", nameof(resolveForgotPasswordRequest.EmailedToken));
            }

            var user = token.User;
            user.PasswordHash = hasher.HashPassword(user, resolveForgotPasswordRequest.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            context.Tokens.Remove(token);
            await context.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest request)
        {
            ValidatePassword(request.NewPassword);

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == currentUserService.UserId)
                ?? throw new ArgumentException("User not found.", nameof(request.NewPassword));

            user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
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

        private async Task ValidateRegisterRequest(RegisterRequest req)
        {
            if (req == null)
                throw new ArgumentException("Request cannot be null.", nameof(req));

            if (req.Role == UserRoleEnum.Admin)
                throw new ArgumentException("Cannot register as an admin.", nameof(req.Role));

            if (string.IsNullOrWhiteSpace(req.Username))
                throw new ArgumentException("Username is required.", nameof(req.Username));

            if (req.Username.Length > 200)
                throw new ArgumentException("Username too long (max 200).", nameof(req.Username));

            if (string.IsNullOrWhiteSpace(req.Password))
                throw new ArgumentException("Password is required.", nameof(req.Password));

            if (string.IsNullOrWhiteSpace(req.Email))
                throw new ArgumentException("Email is required.", nameof(req.Email));

            if (!System.Net.Mail.MailAddress.TryCreate(req.Email, out _))
                throw new ArgumentException("Email format is invalid.", nameof(req.Email));

            if (await context.Users.AnyAsync(u => u.Email == req.Email))
                throw new InvalidOperationException("User with this email already exists.");
        }

        private async Task SendVerificationEmailAsync(string toEmail, string username, string tokenValue)
        {
            var verifyLink = BuildFrontendVerifyEmailLink(tokenValue);

            var model = new Dictionary<string, string>
            {
                ["FullName"] = string.IsNullOrWhiteSpace(username) ? "there" : username,
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

        private string BuildFrontendVerifyEmailLink(string tokenValue)
        {
            var normalizedBase = ResolveFrontendBaseUrl();
            var verifyPath = NormalizePath(urlSettings.Value.VerifyEmailPath, "/auth/verify-email");
            var encodedToken = Uri.EscapeDataString(tokenValue);

            return $"{normalizedBase}{verifyPath}?token={encodedToken}";
        }

        private string ResolveFrontendBaseUrl()
        {
            var configuredFrontendBase = NormalizeConfiguredBaseUrl(urlSettings.Value.FrontendBase);
            if (configuredFrontendBase != null)
            {
                return configuredFrontendBase;
            }

            var request = httpContextAccessor.HttpContext?.Request;
            if (request != null)
            {
                if (TryGetUrlOrigin(request.Headers["Origin"].ToString(), out var originBase))
                {
                    return originBase;
                }

                if (TryGetUrlOrigin(request.Headers["Referer"].ToString(), out var refererBase))
                {
                    return refererBase;
                }
            }

            return urlSettings.Value.ApiBase.TrimEnd('/');
        }

        private static string? NormalizeConfiguredBaseUrl(string? rawBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(rawBaseUrl))
            {
                return null;
            }

            var trimmed = rawBaseUrl.Trim();
            if (trimmed == "/")
            {
                return null;
            }

            return trimmed.TrimEnd('/');
        }

        private static bool TryGetUrlOrigin(string candidate, out string origin)
        {
            origin = string.Empty;
            if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(uri.Scheme) || string.IsNullOrWhiteSpace(uri.Authority))
            {
                return false;
            }

            origin = $"{uri.Scheme}://{uri.Authority}".TrimEnd('/');
            return true;
        }

        private static string NormalizePath(string rawPath, string fallbackPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                return fallbackPath;
            }

            var trimmedPath = rawPath.Trim();
            return trimmedPath.StartsWith('/') ? trimmedPath : $"/{trimmedPath}";
        }

        private async Task SendForgotPasswordEmailAsync(string toEmail, string username, string tokenValue)
        {
            var apiBase = urlSettings.Value.ApiBase.TrimEnd('/');
            var changePasswordLink = $"{apiBase}/auth/forgot-password?token={tokenValue}";

            var model = new Dictionary<string, string>
            {
                ["FullName"] = string.IsNullOrWhiteSpace(username) ? "there" : username,
                ["ChangePasswordLink"] = changePasswordLink
            };

            var html = await templateService.RenderAsync(templateSettings.Value.Email.ResetHtml, model);
            var text = await templateService.RenderAsync(templateSettings.Value.Email.ResetText, model);

            await emailService.SendEmailAsync(
                toEmail: toEmail,
                title: "Proceed to change your password",
                textBody: text,
                htmlBody: html
            );
        }

        private void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));
            if (!System.Net.Mail.MailAddress.TryCreate(email, out _))
                throw new ArgumentException("Email format is invalid.", nameof(email));
        }

        private void ValidatePassword(string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Password is required.", nameof(newPassword));
        }
    }
}
