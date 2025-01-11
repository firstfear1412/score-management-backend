using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
namespace ScoreManagement.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Claims
                .FirstOrDefault(c => c.Type == "username")?.Value;
            if (!string.IsNullOrEmpty(username))
            {

                    // เพิ่ม ConnectionId เข้ากลุ่มของ User
                    await Groups.AddToGroupAsync(Context.ConnectionId, username);
                    Console.WriteLine($"User {username} added to group with ConnectionId: {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        // เรียกเมื่อผู้ใช้ตัดการเชื่อมต่อ
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.User?.Claims
                .FirstOrDefault(c => c.Type == "username")?.Value;
            if (!string.IsNullOrEmpty(username))
            {
                // ลบ ConnectionId ออกจากกลุ่มของ User
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, username);
            }
            Console.WriteLine($"User {username} removed from group.");

            await base.OnDisconnectedAsync(exception);
        }

        // ส่งข้อความถึงผู้ใช้คนเดียว
        public async Task SendNotifyToUser(string username, string message)
        {
            Console.WriteLine($"user : {username} \n message : {message}");
            await Clients.Group(username).SendAsync("ReceiveNotification", message);
        }

        // ส่งข้อความถึงทุกคน
        public async Task SendNotifyToAll(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }

    }
}
