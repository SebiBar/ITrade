using ITrade.DB;
using ITrade.DB.Entities;
using ITrade.DB.Enums;
using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using ITrade.Services.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITrade.Services.Services
{
    public class AuthService(
        Context context,
        IPasswordHasher<User> hasher,
        ITokenService tokenService,
        IEmailService emailService) : IAuthService
    {
        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            ValidateRegisterRequest(request);

            var user = new User
            {
                Email = request.Email,
                PasswordHash = hasher.HashPassword(null!, request.Password),
                FullName = request.FullName,
                UserRoleId = (int)request.Role
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var token = await tokenService.CreateVerifyEmailTokenAsync(user.Id);

            await emailService.SendEmailAsync(
                toEmail: user.Email,
                title: "Verify your email",
                textBody: $"Please verify your email by visiting the following link: https://yourdomain.com/verify-email?token={token}",
                htmlBody: $"Please verify your email by clicking the following link: https://yourdomain.com/verify-email?token={token}"
            );

            return new RegisterResponse(
                UserId: user.Id,
                Email: user.Email,
                FullName: user.FullName
            );
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
    }
}
