using System.ComponentModel.DataAnnotations;

namespace ScoreManagement.Model.Table
{
    public class EmailTemplate
    {
        [Key]
        public int template_id { get; set; }

        public string template_name { get; set; } = string.Empty;

        public bool is_private { get; set; }

        public string subject { get; set; } = string.Empty;

        public string body { get; set; } = string.Empty;

        public string active_status { get; set; } = string.Empty;

        public DateTime create_date { get; set; }

        public string create_by { get; set; } = string.Empty;

        public DateTime update_date { get; set; }

        public string update_by { get; set; } = string.Empty;

    }
}
