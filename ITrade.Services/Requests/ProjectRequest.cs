using ITrade.DB.Enums;

namespace ITrade.Services.Requests
{
    public record ProjectRequest
    (
        string Name,
        string? Description,
        DateTime Deadline,
        ProjectStatusTypeEnum? Status,
        ICollection<int> TagIds
    );
}
