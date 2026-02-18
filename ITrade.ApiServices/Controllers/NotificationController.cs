using ITrade.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITrade.ApiServices.Controllers
{
    [ApiController, Authorize, Route("api/notifications")]
    public class NotificationController(INotificationService notificationService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            return Ok(await notificationService.GetNotificationsAsync());
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteNotification([FromQuery] int notificationId)
        {
            await notificationService.DeleteNotificationAsync(notificationId);
            return Ok();
        }
    }
}
