namespace ScoreManagement.Model
{
    public class NotificationResponse<T>
    {
        public int notificationId { get; set; }

        public int templateId { get; set; }

        public T data  { get; set; }

        public DateTime? createDate { get; set; }

    }
}
