using ITrade.DB.Enums;

namespace ITrade.Services.Interfaces
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        UserRoleEnum UserRole { get; }
    }
}
