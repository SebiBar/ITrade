using ITrade.DB.Enums;

namespace ITrade.Services.Requests
{
    public record RegisterRequest
    (
        string Username,
        string Email,
        string Password,
        UserRoleEnum Role
    );
}
