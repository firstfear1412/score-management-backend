using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
namespace ScoreManagement.Hubs
{
    public class NotificationHub : Hub, INotificationHub
    {
        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.Claims
                .FirstOrDefault(c => c.Type == "username")?.Value;
            if (!string.IsNullOrEmpty(userName))
            {
                // เพิ่ม ConnectionId เข้ากลุ่มของ User
                await Groups.AddToGroupAsync(Context.ConnectionId, userName);
            }
            Console.WriteLine($"User {userName} added to group.");

            await base.OnConnectedAsync();
        }

        // เรียกเมื่อผู้ใช้ตัดการเชื่อมต่อ
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.Claims
                .FirstOrDefault(c => c.Type == "username")?.Value;
            if (!string.IsNullOrEmpty(userName))
            {
                // ลบ ConnectionId ออกจากกลุ่มของ User
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userName);
            }
            Console.WriteLine($"User {userName} removed from group.");

            await base.OnDisconnectedAsync(exception);
        }

        // ส่งข้อความถึงผู้ใช้คนเดียว
        public async Task SendNotifyToUser(string userName, string message)
        {
            Console.WriteLine($"user : {userName} \n message : {message}");
            await Clients.Group(userName).SendAsync("ReceiveNotification", message);
        }

        // ส่งข้อความถึงทุกคน
        public async Task SendNotifyToAll(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }

    }
}
