namespace ITrade.Services.Responses
{
    public record UserResponse
    (
        int Id,
        string Username,
        string Email,
        string Role,
        int UnreadNotificationsCount
    );
}
