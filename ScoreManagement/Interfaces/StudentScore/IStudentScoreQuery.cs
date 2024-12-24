using Microsoft.AspNetCore.Mvc;
using ScoreManagement.Model;
using ScoreManagement.Model.SubjectScore;
using ScoreManagement.Model.Table;

namespace ScoreManagement.Interfaces
{
    public interface IStudentScoreQuery
    {
        PlaceholderMapping GetPlaceholderMapping(string placeholderKey);
        Task<string> GetFieldValue(SubjectDetail subjectDetail, string sourceTable, string fieldName, string condition, string username);
        Task<bool> UpdateTemplateEmail(EmailTemplateResource resource);
        Task<bool> CreateTemplateEmail(EmailTemplateResource resource);
        Task<bool> DeleteTemplateEmail(EmailTemplateResource resource);
    }
}
