﻿namespace ScoreManagement.Model
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

        public int? accumulated_score { get; set; }

        public int? midterm_score { get; set; }

        public int? final_score { get; set; }

        public int? total_score { get; set; }
    }

    public class SubjectDetailUpload
    {
        public string subject_id { get; set; }
        public string subject_name { get; set; }
        public string academic_year { get; set; }
        public string semester { get; set; }
        public string section { get; set; }
    }
}