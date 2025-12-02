using ITrade.DB.Enums;
using ITrade.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ITrade.Services.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public int UserId { get; }
        public UserRoleEnum UserRole { get; }

        public CurrentUserService(IHttpContextAccessor accessor)
        {
            var user = accessor.HttpContext?.User;
            if (user is null || !(user.Identity?.IsAuthenticated ?? false))
                return;

            // UserId
            if (int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id))
                UserId = id;

            // Role
            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            if (Enum.TryParse<UserRoleEnum>(role, out var parsedRole))
                UserRole = parsedRole;
        }
    }
}
