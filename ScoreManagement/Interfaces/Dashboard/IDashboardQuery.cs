using System.Threading.Tasks;
using ScoreManagement.Model.Dashboard;
using ScoreManagement.Model.ScoreAnnoucement;
using ScoreManagement.Model.ScoreAnnoucement;

namespace ScoreManagement.Interfaces.Dashboard
{
    public interface IDashboardQuery
    {
        Task<DashboardStatisticsResponse> GetDashboardStatistics(ScoreAnnoucementDashboard resource);
    }
}