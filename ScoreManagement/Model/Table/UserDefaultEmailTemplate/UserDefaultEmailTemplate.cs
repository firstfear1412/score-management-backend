namespace ScoreManagement.Model.Table
{
    public class UserDefaultEmailTemplate
    {
        public string username { get; set; } = string.Empty;

        public int template_id { get; set; }

        public string active_status { get; set; } = string.Empty;

        public DateTime? create_date { get; set; }

        public string create_by { get; set; } = string.Empty;

        public DateTime? update_date { get; set; }

        public string update_by { get; set; } = string.Empty;

    }
}
