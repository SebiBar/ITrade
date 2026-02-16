using ITrade.Services.Requests;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface INotificationService
    {
        Task<ICollection<NotificationResponse>> GetNotificationsAsync();
        Task<int> GetUnreadNotificationCountAsync();
        Task<int> CreateNotificationAsync(NotificationRequest notificationCreateRequest);
        Task DeleteNotificationAsync(int notificationId);
    }
}
