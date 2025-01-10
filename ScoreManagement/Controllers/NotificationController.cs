using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ScoreManagement.Hubs;
using ScoreManagement.Model;

namespace ScoreManagement.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationHub _notificationHub;

        public NotificationController(INotificationHub notificationHub)
        {
            _notificationHub = notificationHub;
        }

        [HttpPost]
        public async Task<IActionResult> SendNotification(string userName, string message)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(message))
            {
                return BadRequest("User name or message cannot be empty.");
            }

            // Send notification using the service (which could be NotificationHub)
            await _notificationHub.SendNotifyToUser(userName, message);

            return Ok("Notification sent successfully.");
        }
        //private readonly IHubContext<NotificationHub> _hubContext;

        //public NotificationController(IHubContext<NotificationHub> hubContext)
        //{
        //    _hubContext = hubContext;
        //}

        //[HttpPost("send")]
        //public async Task<IActionResult> SendNotification([FromBody] NotificationResource request)
        //{
        //    await _hubContext.Clients.User(request.User).SendAsync("ReceiveNotification", request.Message);
        //    return Ok(new { Message = "Notification sent successfully!" });
        //}
    }
}
