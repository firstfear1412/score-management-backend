﻿using System.ComponentModel.DataAnnotations;

namespace ScoreManagement.Model.Table
{
    public class SubjectScore
    {
        [Key]
        public int sys_subject_no { get; set; }

        [Key]
        public string? subject_id { get; set; }

        public string? academic_year { get; set; }

        public string? section { get; set; }

        public string? student_id { get; set; }

        public string? seat_no { get; set; }

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
