namespace ITrade.Services.Responses
{
    public record NotificationResponse
    (
        int Id,
        string Name,
        string Content,
        bool IsRead,
        DateTime CreatedAt
    );
}
