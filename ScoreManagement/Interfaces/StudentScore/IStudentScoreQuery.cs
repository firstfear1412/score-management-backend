using ScoreManagement.Model;
using ScoreManagement.Model.ScoreAnnoucement;
using ScoreManagement.Model.SubjectScore;
using ScoreManagement.Model.Table;

namespace ScoreManagement.Interfaces
{
    public interface IStudentScoreQuery
    {
        PlaceholderMapping GetPlaceholderMapping(string placeholderKey);
        Task<string> GetFieldValue(SubjectDetail subjectDetail, string studentId, string sourceTable, string fieldName, string condition, string username);
        Task<bool> UpdateTemplateEmail(EmailTemplateResource resource);
        Task<bool> CreateTemplateEmail(EmailTemplateResource resource);
        Task<bool> DeleteTemplateEmail(EmailTemplateResource resource);
        Task<bool> SetDefaultTemplateEmail(EmailTemplateResource resource);
        Task<bool> UploadStudentScore(SubjectDetailUpload subject, ScoreStudent student, string username);
        Task<bool> UpdateSendEmail(SubjectDetail resource, string studentId, string username, int send_status, string send_desc = "");
        Task<string> GetEmailStudent(string student_id);
        Task<List<ScoreAnnoucementResource>> GetScoreAnnoucementByConditionQuery(ScoreAnnoucementResource resource);
    }
}
