using System.ComponentModel.DataAnnotations.Schema;

namespace ScoreManagement.Model
{
    public class UploadScoreResource
    {
        public SubjectDetailUpload subject { get; set; } = new SubjectDetailUpload();

        public List<ScoreStudent> data { get; set; } = new List<ScoreStudent>();

        public string username { get; set; }

    }
    public class ScoreStudent
    {
        public string? seat_no { get; set; }

        public string? student_id { get; set; }

        public string? prefix { get; set; }

        public string? firstname { get; set; }

        public string? lastname { get; set; }

        public string? major_code { get; set; }

        public string? email { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? accumulated_score { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? midterm_score { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? final_score { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? total_score { get; set; }
    }

    public class SubjectDetailUpload
    {
        public string subject_id { get; set; }
        public string subject_name { get; set; }
        public int academic_year { get; set; }
        public int semester { get; set; }
        public int section { get; set; }
        public List<string> teacher { get; set; }
    }
}