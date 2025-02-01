using ScoreManagement.Model;
using ScoreManagement.Model.Table;
using ScoreManagement.Model.ScoreAnnoucement;

namespace ScoreManagement.Interfaces.Dashboard
{
    public interface IDashboardQuery
    {
        Task<object> GetDashboardStatistics(ScoreAnnoucementDashboard resource);

        Task<List<SubjectResponse>> GetSubjectDashboard(string? teacher_code);
    }
}