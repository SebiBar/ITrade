using ITrade.DB.Enums;

namespace ITrade.Services.Requests
{
    public record ProjectRequestReq
    (
        string? Message,
        int ReceiverId,
        int ProjectId,
        ProjectRequestTypeEnum RequestType
    );
}
