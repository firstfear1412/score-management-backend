using System.ComponentModel.DataAnnotations;

namespace ScoreManagement.Model.Table
{
    public class NotificationTemplate
    {
        [Key]
        public int template_id { get; set; }

        public string html_content { get; set; }

        public string description { get; set; }

        public string active_status { get; set; }

    }
}
