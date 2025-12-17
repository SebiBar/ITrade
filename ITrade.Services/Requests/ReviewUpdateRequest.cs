namespace ITrade.Services.Requests
{
    public record ReviewUpdateRequest
    (
        int ReviewId,
        string? Title,
        string? Comment,
        int? Rating
    );
}
