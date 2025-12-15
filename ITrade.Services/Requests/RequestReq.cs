using ITrade.DB.Enums;

namespace ITrade.Services.Requests
{
    public record RequestReq
    (
        string? Message,
        int ReceiverId,
        int ProjectId,
        ProjectRequestTypeEnum RequestType
    );
}
