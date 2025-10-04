using System.ComponentModel.DataAnnotations;

namespace ScoreManagement.Model.Table
{
    public class Notification
    {
        [Key]
        public int notification_id { get; set; }

        public string username { get; set; }

        public int? template_id { get; set; }

        public string data { get; set; }

        public string active_status { get; set; }

        public string create_by { get; set; }

        public DateTime? create_date { get; set; }

        public string update_by { get; set; }

        public DateTime? update_date { get; set; }

    }
}
