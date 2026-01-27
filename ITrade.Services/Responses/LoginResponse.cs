namespace ITrade.Services.Responses
{
    public record LoginResponse
    (
        UserResponse User,
        string Jwt,
        string Refresh
    );
}
