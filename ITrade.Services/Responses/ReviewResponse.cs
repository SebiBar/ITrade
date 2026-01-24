namespace ITrade.Services.Responses
{
    public record ReviewResponse
    (
        int Id,
        int ReviewerId,
        string ReviewerUsername,
        int RevieweeId,
        string RevieweeUsername,
        int Rating,
        string? Title,
        string? Comment,
        DateTime CreatedAt
    );
}
