using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ScoreManagement.Hubs;
using ScoreManagement.Model;

namespace ScoreManagement.Controllers
{
    public class NotificationController : Controller
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationController(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationResource request)
        {
            await _hubContext.Clients.User(request.User).SendAsync("ReceiveNotification", request.Message);
            return Ok(new { Message = "Notification sent successfully!" });
        }
    }
}
