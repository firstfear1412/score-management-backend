using NPOI.SS.Formula.Functions;

namespace ScoreManagement.Model
{
    public class NotificationResource
    {
        public int notificationId { get; set; }

        public int templateId { get; set; }

        public string? data { get; set; }

        public string? username { get; set; }
    }
}
