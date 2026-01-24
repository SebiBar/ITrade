namespace ITrade.Services.Responses
{
    public record UserProfileLinkResponse
    (
        int Id,
        int UserId,
        string Url
    );
}
