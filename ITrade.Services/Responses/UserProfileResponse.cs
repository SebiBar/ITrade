namespace ITrade.Services.Responses
{
    public record UserProfileResponse
    (
        int Id,
        string Username,
        string Email,
        string Role
    );
}
