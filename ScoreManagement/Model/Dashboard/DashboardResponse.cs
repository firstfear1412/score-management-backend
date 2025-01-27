using Microsoft.Identity.Client;
using System.Security;

public class DashboardStatisticsResponse
{
    public class DashboardAccumulatedScore
    {
        public int? MaxAccumulatedScore { get; set; }
        public int? MinAccumulatedScore { get; set; }
        public int? AvgAccumulatedScore { get; set; }
        public double? StdAccumulatedScore { get; set; }
        public int? NumberOfStudents { get; set; }
    }

    public class DashboardMidtermScore
    {
        public int? MaxMidtermScore { get; set; }
        public int? MinMidtermScore { get; set; }
        public int? AvgMidtermScore { get; set; }
        public double? StdMidtermScore { get; set; }
        public int? NumberOfStudents { get; set; }
    }

    public class DashboardFinalScore
    {
        public int? MaxFinalScore { get; set; }
        public int? MinFinalScore { get; set; }
        public int? AvgFinalScore { get; set; }
        public double? StdFinalScore { get; set; }
        public int? NumberOfStudents { get; set; }
    }

    public class DashboardTotalScore
    {
        public int? MaxTotalScore { get; set; }
        public int? MinTotalScore { get; set; }
        public int? AvgTotalScore { get; set; }
        public double? StdTotalScore { get; set; }
        public int? NumberOfStudents { get; set; }
    }

    public class DashboardStudentScore
    {
        public int? sys_subject_no { get; set; }

        public string? subject_id { get; set; }

        public string? subject_name { get; set; }

        public string? academic_year { get; set; }

        public string? semester { get; set; }

        public string? section { get; set; }

        public string? student_id {  get; set; }

        public string? seat_no {  get; set; }

        public int? accumulated_score { get; set; }

        public int? midterm_score { get; set; }

        public int? final_score { get; set; }
    }
}