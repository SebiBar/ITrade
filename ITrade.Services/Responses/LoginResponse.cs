namespace ITrade.Services.Responses
{
    public record LoginResponse
    (
        int Id,
        string FullName,
        string Email,
        string Jwt,
        string Refresh
    );
}
