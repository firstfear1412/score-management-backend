namespace ScoreManagement.Model.Dashboard
{
    public class DashboardStatisticsResponse
    {
        public int? MaxAccumulatedScore { get; set; }
        public int? MinAccumulatedScore { get; set; }
        public int? AvgAccumulatedScore { get; set; }
        public double? StdAccumulatedScore { get; set; }
        public int? NumberOfStudents { get; set; }
    }
}