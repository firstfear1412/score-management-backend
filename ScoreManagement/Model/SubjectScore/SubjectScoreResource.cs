namespace ScoreManagement.Model.Table.SubjectScore
{
    public class SubjectScoreResource
    {
        public int row_id { get; set; }

        public string? subject_id { get; set; }

        public string? academic_year { get; set; }

        public string? section { get; set; }

        public string? student_id { get; set; }

        public int? accumulated_score { get; set; }

        public int? midterm_score { get; set; }

        public int? final_score { get; set; }

        public string? active_status { get; set; }

        public DateTime? create_date { get; set; }

        public string? create_by { get; set; }

        public DateTime? update_date { get; set; }

        public string? update_by { get; set; }

    }
}
