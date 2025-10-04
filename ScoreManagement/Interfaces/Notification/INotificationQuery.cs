using ScoreManagement.Model;

namespace ScoreManagement.Interfaces
{
    public interface INotificationQuery
    {
        Task<List<NotificationResponse<string>>> GetNotifications(string username);
    }
}
