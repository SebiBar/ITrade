using ITrade.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ITrade.Services.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public int UserId { get; }
        public string UserRole { get; }

        public CurrentUserService(IHttpContextAccessor accessor)
        {
            var user = accessor.HttpContext?.User;
            UserId = int.Parse(user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            UserRole = user?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}
