namespace ScoreManagement.Model.ScoreAnnoucement
{
    public class ScoreAnnoucementResource
    {
        public int? row_id { get; set; }
        public string? subject_id { get; set; }
        public string? academic_year { get; set; }
        public int? semester { get; set; }
        public string? section { get; set; }
        public string? student_id { get; set; }
        public int? seat_no { get; set; }
        public int? accumulated_score { get; set; }
        public int? midterm_score { get; set; }
        public int? final_score { get; set; }
        public string? send_status_code { get; set; }
        public string? send_status_desc_th { get; set; }
        public string? send_status_desc_en { get; set; }
        public string? score_active_status { get; set; }
        public DateTime? score_create_date { get; set; }
        public string? score_create_by { get; set; }
        public string? score_update_by { get; set; }
        public string? prefix_th { get; set; }
        public string? prefix_en { get; set; }
        public string? firstname { get; set; }
        public string? lastname { get; set; }
        public string? major_code { get; set; }
        public string? email { get; set; }
        public string? student_active_status { get; set; }
        public DateTime? student_create_date { get; set; }
        public string? student_create_by { get; set; }
        public DateTime? student_update_date { get; set; }
        public string? student_update_by { get; set; }
        public string? subject_name { get; set; }
    }
}
