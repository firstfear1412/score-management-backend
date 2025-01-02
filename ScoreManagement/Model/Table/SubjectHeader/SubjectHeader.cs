using System.ComponentModel.DataAnnotations;

namespace ScoreManagement.Model.Table
{
    public class SubjectHeader
    {
        [Key]
        public int sys_subject_no { get; set; }

        public string? subject_id { get; set; }

        public string? academic_year { get; set; }

        public int? semester { get; set; }

        public string? section { get; set; }

        public string? active_status { get; set; }

        public DateTime? create_date { get; set; }

        public string? create_by { get; set; }

        public DateTime? update_date { get; set; }

        public string? update_by { get; set; }

    }
}
