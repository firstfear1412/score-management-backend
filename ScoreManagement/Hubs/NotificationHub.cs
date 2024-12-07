using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
namespace ScoreManagement.Hubs
{
    public class NotificationHub : Hub
    {
        //public override async Task OnConnectedAsync()
        //{
        //    var username = Context.GetHttpContext()?.Request.Headers["username"].ToString();
        //    if (!string.IsNullOrWhiteSpace(username))
        //    {
        //        Console.WriteLine($"User {username} connected.");
        //    }
        //    else
        //    {
        //        Console.WriteLine("Username is missing from headers.");
        //    }
        //    await base.OnConnectedAsync();
        //}

        //public async Task SendNotificationToUser(string user, string message)
        //{
        //    Console.WriteLine("Username is " + user + "message : " + message);
        //    await Clients.User(user).SendAsync("ReceiveNotification", message);
        //}

        //public async Task BroadcastNotification(string message)
        //{
        //    await Clients.All.SendAsync("ReceiveNotification", message);
        //}

        public override async Task OnConnectedAsync()
        {
            var username = Context.GetHttpContext()?.Request.Headers["username"].ToString();
            if (!string.IsNullOrWhiteSpace(username))
            {
                // เพิ่ม User ไปยัง Connection Mapping
                await Groups.AddToGroupAsync(Context.ConnectionId, username);
                Console.WriteLine($"User {username} connected.");
            }
            else
            {
                Console.WriteLine("Username is missing from headers.");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.GetHttpContext()?.Request.Headers["username"].ToString();
            if (!string.IsNullOrWhiteSpace(username))
            {
                // ลบ User ออกจาก Group
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, username);
                Console.WriteLine($"User {username} disconnected.");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotificationToUser(string user)
        {
            // สร้าง mockup data
            var mockNotifications = new List<string>
            {
                "This is your first notification",
                "You have a new message",
                "System update available",
                "Reminder: Your meeting starts in 10 minutes"
            };
            // ส่ง notification ไปยังผู้ใช้
            foreach (var notification in mockNotifications)
            {
                // ส่ง notification สำหรับผู้ใช้งานที่ระบุ
                await Clients.User(user).SendAsync("ReceiveNotification", notification);
            }
            // ส่งข้อมูลให้ผู้ใช้งานคนที่ระบุเท่านั้น
            //await Clients.User(user).SendAsync("ReceiveNotification", message);
        }

        public async Task BroadcastNotification(string message)
        {
            // ส่งข้อมูลให้ทุกคน
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
}
