using ITrade.DB.Enums;

namespace ITrade.Services.Requests
{
    public record ProjectUpdateReq
    (
        string? Name,
        string? Description,
        DateTime? Deadline,
        ProjectStatusTypeEnum? Status
    );
}
