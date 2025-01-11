using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoreManagement.Controllers.Base;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;

namespace ScoreManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : BaseController
    {
        private readonly INotificationQuery _notificationQuery;

        public NotificationController(INotificationQuery notificationQuery)
        {
            _notificationQuery = notificationQuery;
        }

        [HttpGet("GetNotify/{username}")]
        public async Task<IActionResult> GetNotifications(string username)
        {
            HttpContext pathBase = HttpContext;
            bool isSuccess = false;
            string message = string.Empty;
            List<NotificationResponse<string>> notifications = new List<NotificationResponse<string>>();
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    return StatusCode(400, new
                    {
                        isSuccess = isSuccess,
                        message = "username is required"
                    });
                }
                // Send notification using the service (which could be NotificationHub)
                notifications = await _notificationQuery.GetNotifications(username);
                isSuccess = true;
            }
            catch(Exception ex)
            {
                message = ex.Message;
                _webEvent.WriteLogException(username, message, ex, pathBase);
            }
            var response = ApiResponse(
                isSuccess: isSuccess,
                messageDescription: message,
                objectResponse: notifications
            );
            return StatusCode(200,response);
        }
    }
}
