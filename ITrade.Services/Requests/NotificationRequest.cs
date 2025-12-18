namespace ITrade.Services.Requests
{
    public record NotificationRequest
    (
        string Name,
        string Content,
        int UserId
    );
}
