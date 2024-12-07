using System.ComponentModel.DataAnnotations;

namespace ScoreManagement.Model
{
    public class SubjectResource
    {
        public int row_id { get; set; }

        public string? subject_id { get; set; }

        public string? subject_name { get; set; }

        public DateTime? create_date { get; set; }

        public string? active_status { get; set; }

        public string? create_by { get; set; }

        public DateTime? update_date { get; set; }

        public string? update_by { get; set; }

    }
}
