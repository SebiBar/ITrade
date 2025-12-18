using ITrade.DB;
using ITrade.DB.Entities;
using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using ITrade.Services.Responses;
using Microsoft.EntityFrameworkCore;

namespace ITrade.Services.Services
{
    public class NotificationService(
        Context context,
        ICurrentUserService currentUserService) : INotificationService
    {
        public async Task<int> CreateNotificationAsync(NotificationRequest notificationCreateRequest)
        {
            ValidateNotificationRequest(notificationCreateRequest);

            var notification = new Notification
            {
                Name = notificationCreateRequest.Name,
                Content = notificationCreateRequest.Content,
                UserId = notificationCreateRequest.UserId
            };

            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            return notification.Id;
        }

        public async Task DeleteNotificationAsync(int notificationId)
        {
            var notification = await context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == currentUserService.UserId)
                ?? throw new Exception("Notification not found.");

            context.Notifications.Remove(notification);
            await context.SaveChangesAsync();
        }

        public async Task<ICollection<NotificationResponse>> GetNotificationsAsync()
        {
            return await context.Notifications
                .Where(n => n.UserId == currentUserService.UserId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationResponse
                (
                    n.Id,
                    n.Name,
                    n.Content,
                    n.IsRead,
                    n.CreatedAt
                )).ToListAsync();
        }

        private void ValidateNotificationRequest(NotificationRequest request)
        {
            if (request.Name.Length > 200)
            {
                throw new ArgumentException("Notification name exceeds maximum length of 200 characters.");
            }
            if (request.Content.Length > 2000)
            {
                throw new ArgumentException("Notification content exceeds maximum length of 2000 characters.");
            }
        }
    }
}
