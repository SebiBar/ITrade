using ITrade.DB.Enums;

namespace ITrade.Services.Requests
{
    public record RegisterRequest
        (
        string FullName,
        string Email,
        string Password,
        UserRoleEnum Role
    );
}
