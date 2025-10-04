using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScoreManagement.Model
{
    public class SubjectScoreResource
    {
        public int sys_subject_no { get; set; }

        public string? student_id { get; set; }

        public string? seat_no { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? accumulated_score { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? midterm_score { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? final_score { get; set; }

        public string? active_status { get; set; }

        public DateTime? create_date { get; set; }

        public string? create_by { get; set; }

        public DateTime? update_date { get; set; }

        public string? update_by { get; set; }
    }
}
