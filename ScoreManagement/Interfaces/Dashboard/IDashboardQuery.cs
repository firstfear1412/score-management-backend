using ScoreManagement.Model.ScoreAnnoucement;

namespace ScoreManagement.Interfaces.Dashboard
{
    public interface IDashboardQuery
    {
        Task<object> GetDashboardStatistics(ScoreAnnoucementDashboard resource);
    }
}