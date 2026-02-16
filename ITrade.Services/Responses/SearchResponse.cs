namespace ITrade.Services.Responses
{
    public record SearchResponse
    (
        ICollection<ProjectResponse> Projects,
        ICollection<UserResponse> Users,
        int TotalProjects,
        int TotalUsers,
        int Page,
        int PageSize
    );
}
