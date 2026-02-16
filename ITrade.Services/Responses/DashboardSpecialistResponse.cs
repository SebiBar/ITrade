namespace ITrade.Services.Responses
{
    public record DashboardSpecialistResponse
    (
        // Base
        int UnreadNotificationCount,

        // Top layer "Needs Attention"
        ICollection<RequestResponse> PendingInvitations,

        // Middle layer "My Active Workspace"
        ICollection<ProjectResponse> ActiveProjects,

        // Bottom Section "Recommended Opportunities"
        ICollection<ProjectMatchedResponse> RecommendedProjects
    ) : DashboardResponse(UnreadNotificationCount);
}
