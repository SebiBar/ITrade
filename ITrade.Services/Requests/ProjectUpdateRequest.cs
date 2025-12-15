using ITrade.DB.Enums;

namespace ITrade.Services.Requests
{
    public record ProjectUpdateRequest
    (
        string? Name,
        string? Description,
        DateTime? Deadline,
        ProjectStatusTypeEnum? Status
    );
}
