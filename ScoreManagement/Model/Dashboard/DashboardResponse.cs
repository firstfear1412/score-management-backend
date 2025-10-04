using Microsoft.Identity.Client;
using System.Security;

public class DashboardStatisticsResponse
{
    public class DashboardAccumulatedScore
    {
        public decimal? MaxAccumulatedScore { get; set; }
        public decimal? MinAccumulatedScore { get; set; }
        public decimal? AvgAccumulatedScore { get; set; }
        public double? StdAccumulatedScore { get; set; }
        public int? NumberOfStudents { get; set; }
    }

    public class DashboardMidtermScore
    {
        public decimal? MaxMidtermScore { get; set; }
        public decimal? MinMidtermScore { get; set; }
        public decimal? AvgMidtermScore { get; set; }
        public double? StdMidtermScore { get; set; }
        public int? NumberOfStudents { get; set; }
    }

    public class DashboardFinalScore
    {
        public decimal? MaxFinalScore { get; set; }
        public decimal? MinFinalScore { get; set; }
        public decimal? AvgFinalScore { get; set; }
        public double? StdFinalScore { get; set; }
        public int? NumberOfStudents { get; set; }
    }

    public class DashboardTotalScore
    {
        public decimal? MaxTotalScore { get; set; }
        public decimal? MinTotalScore { get; set; }
        public decimal? AvgTotalScore { get; set; }
        public decimal? StdTotalScore { get; set; }
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

        public decimal? accumulated_score { get; set; }

        public decimal? midterm_score { get; set; }

        public decimal? final_score { get; set; }
    }
}