using ITrade.DB.Enums;

namespace ITrade.Services.Requests
{
    public record ProjectReq
    (
        string Name,
        string? Description,
        DateTime Deadline,
        ProjectStatusTypeEnum Status = ProjectStatusTypeEnum.Hiring
    );
}
