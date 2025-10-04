using ScoreManagement.Model.ExcelScore;

namespace ScoreManagement.Interfaces.ExcelScore
{
    public interface IExcelScore
    {
        Task<List<ExcelScoreModel>> GetScoreReportAsync(ExcelScoreRequest request);

        Task<List<ExcelScoreModel_Other>> GetScoreReportAsync_Other(ExcelScoreRequest request);
    }
}