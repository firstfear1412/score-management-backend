namespace ScoreManagement.Model.ExcelScore
{
    public class ExcelScoreModel
    {
        public string? SubjectId { get; set; }
        public string? SubjectName { get; set; }
        public string? AcademicYear { get; set; }
        public string? Semester { get; set; }
        public string? Section { get; set; }
        public string? ScoreType { get; set; }
        public int? StudentCount { get; set; }
        public decimal? AverageScore { get; set; }
        public decimal? MinScore { get; set; }
        public decimal? MaxScore { get; set; }
        public double? StandardDeviation { get; set; }
        public int? Sum0_39 { get; set; }
        public int? Sum40_49 { get; set; }
        public int? Sum50_59 { get; set; }
        public int? Sum60_69 { get; set; }
        public int? Sum70_79 { get; set; }
        public int? Count80Plus { get; set; }
    }

    public class ExcelScoreRequest
    {
        public string? subject_id { get; set; }
        public string? academic_year { get; set; }
        public string? semester { get; set; }
        public string? section { get; set; }
        public string? score_type { get; set; }
    }

}
