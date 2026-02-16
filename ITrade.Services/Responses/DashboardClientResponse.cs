namespace ITrade.Services.Responses
{
    public record DashboardClientResponse
    (
        // Base
        int UnreadNotificationCount,

        // Top layer "Needs Attention"
        ICollection<RequestResponse> PendingApplications,

        // Main section "Top Matches for Your Open Roles"
        ICollection<ProjectWithMatchesResponse> OpenProjectsWithMatches,

        // Details
        int ActiveProjectCount
    ) : DashboardResponse(UnreadNotificationCount);
}
