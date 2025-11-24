namespace ITrade.Services.Responses
{
    public record RefreshTokensResponse
    (
        string Jwt,
        string Refresh
    );
}
