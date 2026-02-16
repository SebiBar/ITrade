using ITrade.DB.Entities;

namespace ITrade.Services.Responses
{
    public record DashboardAdminResponse
    (
        int UnreadNotificationCount,
        ICollection<Tag> Tags
    ) : DashboardResponse(UnreadNotificationCount);
}
