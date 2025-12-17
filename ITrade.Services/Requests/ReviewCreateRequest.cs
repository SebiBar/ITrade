namespace ITrade.Services.Requests
{
    public record ReviewCreateRequest
    (
        int RevieweeId,
        string Title,
        string? Comment,
        int Rating
    );
}
