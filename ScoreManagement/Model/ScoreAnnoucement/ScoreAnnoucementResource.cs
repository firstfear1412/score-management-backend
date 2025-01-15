namespace ScoreManagement.Model.ScoreAnnoucement
{
    public class ScoreAnnoucementResource
    {

        public int? sys_subject_no { get; set; }
        public string? subject_id { get; set; }
        public string? academic_year { get; set; }
        public int? semester { get; set; }
        public string? section { get; set; }
        public string? teacher_code { get; set; }
        //public string? lecturer_active_status { get; set; }
        public string? student_id { get; set; }
        public string? prefix_code { get; set; }
        public string? prefix_desc_th { get; set; }
        public string? prefix_desc_en { get; set; }
        public string? studentSearch { get; set; }
        public string? firstname { get; set; }
        public string? lastname { get; set; }
        public string? seat_no { get; set; }
        public int? accumulated_score { get; set; }
        public int? midterm_score { get; set; }
        public int? final_score { get; set; }
        public string? send_status_code { get; set; }
        public string? send_status_code_desc_th { get; set; }
        public string? send_status_code_desc_en { get; set; }
        public string? send_desc { get; set; }
        public string? email { get; set; }
        public int? role { get; set; }
        public string? subjectSearch { get; set; }
        public string? major_code { get; set; }

    }

    public class ScoreAnnoucementDashboard
    {
        //public string? subject_search { get; set; }
        public string? subject_id { get; set; }
        public string? subject_name { get; set; }
        public string? academic_year { get; set; }
        public string? semester { get; set; }
        public string? section { get; set; }
        public string? score_type { get; set; }
    }
}