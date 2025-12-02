namespace ITrade.Services.Responses
{
    public record ProjectResponse
    (
        int Id,
        string Name,
        string? Description,
        int OwnerId,
        string OwnerUsername,
        int? WorkerId,
        string? WorkerUsername,
        DateTime Deadline,
        int ProjectStatusTypeId,
        string ProjectStatusType,
        ICollection<ProjectTagResponse> Tags,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
