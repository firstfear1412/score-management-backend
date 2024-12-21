using ScoreManagement.Model;
using ScoreManagement.Model.Table;

namespace ScoreManagement.Interfaces
{
    public interface IStudentScoreQuery
    {
        PlaceholderMapping GetPlaceholderMapping(string placeholderKey);
        Task<string> GetFieldValue(SubjectDetail subjectDetail, string sourceTable, string fieldName, string condition, string username);
    }
}
