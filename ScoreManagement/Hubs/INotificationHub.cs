namespace ScoreManagement.Hubs
{
    public interface INotificationHub
    {
        Task OnConnectedAsync();
        Task OnDisconnectedAsync(Exception? exception);
        Task SendNotifyToUser(string userName, string message);
        Task SendNotifyToAll(string message);
    }
}
