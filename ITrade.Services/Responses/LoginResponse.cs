namespace ITrade.Services.Responses
{
    public record LoginResponse
    (
        int Id,
        string Username,
        string Email,
        string Jwt,
        string Refresh
    );
}
