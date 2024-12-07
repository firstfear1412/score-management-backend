using System.ComponentModel.DataAnnotations;

namespace ScoreManagement.Model.Table
{
    public class Student
    {
        [Key]
        public int row_id { get; set; }

        public string? student_id { get; set; }

        public string? firstname { get; set; }

        public string? lastname { get; set; }

        public string? major_code { get; set; }

        public string? email { get; set; }

        public string? active_status { get; set; }

        public DateTime? create_date { get; set; }

        public string? create_by { get; set; }

        public DateTime? update_date { get; set; }

        public string? update_by { get; set; }

    }
}
