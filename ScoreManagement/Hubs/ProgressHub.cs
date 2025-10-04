using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
namespace ScoreManagement.Hubs
{
    public class ProgressHub : Hub
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

        public async Task SendProgress(int completed, int failed)
        {
            await Clients.Caller.SendAsync("ReceiveProgress", completed, failed);
        }

    }
}
