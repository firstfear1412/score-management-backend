using System.ComponentModel.DataAnnotations;

namespace ScoreManagement.Model.Table.User
{
    public class User
    {
        [Key]
        public int row_id { get; set; }

        public string? username { get; set; }

        public string? password { get; set; }

        public int? role { get; set; }

        public string? teacher_code { get; set; }

        public string? prefix { get; set; }

        public string? firstname { get; set; }

        public string? lastname { get; set; }
        public string? email { get; set; }
        public int? total_failed { get; set; }
        public DateTime? date_login { get; set; }
        public string? active_status { get; set; }

        public DateTime? create_date { get; set; }

        public string? create_by { get; set; }

        public DateTime? update_date { get; set; }

        public string? update_by { get; set; }

    }
}
