namespace ITrade.Services.Responses
{
    public record RequestResponse
    (
        int Id,
        string? Message,
        int SenderId,
        string SenderUsername,
        int ReceiverId,
        string ReceiverUsername,
        int ProjectId,
        string ProjectName,
        string RequestType,
        bool? Accepted,
        DateTime CreatedAt,
        double? MatchScore
    );
}
