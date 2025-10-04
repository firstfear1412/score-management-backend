using ScoreManagement.Model;
using ScoreManagement.Model.Table;
using ScoreManagement.Model.ScoreAnnoucement;
using ScoreManagement.Model.ExcelScore;

namespace ScoreManagement.Interfaces.Dashboard
{
    public interface IDashboardQuery
    {
        Task<object> GetDashboardStatistics(ScoreAnnoucementDashboard resource);

        Task<List<SubjectResponse>> GetSubjectDashboard(string? teacher_code);

        Task<List<ExcelScoreModel_Other>> GetScoreReportAsync_Other(ExcelScoreRequest request);
    }
}