using System.ComponentModel.DataAnnotations;

namespace ScoreManagement.Model.Table
{
    public class SubjectLecturer
    {
        [Key]
        public int row_id { get; set; }

        public int? sys_subject_no { get; set; }

        public string? teacher_code { get; set; }

        public string? active_status { get; set; }

        public DateTime? create_date { get; set; }

        public string? create_by { get; set; }

        public DateTime? update_date { get; set; }

        public string? update_by { get; set; }

    }
}
