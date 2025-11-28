using ITrade.DB.Enums;

namespace ITrade.Services.Requests
{
    public record ProjectRequestRequest
    (
        string? Message,
        int ReceiverId,
        int ProjectId,
        ProjectRequestTypeEnum RequestType
    );
}
