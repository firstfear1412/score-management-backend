using ScoreManagement.Model.ExcelScore;

namespace ScoreManagement.Interfaces.ExcelScore
{
    public interface IExcelScore
    {
        Task<List<ExcelScoreModel>> GetScoreReportAsync(ExcelScoreRequest request);
    }
}