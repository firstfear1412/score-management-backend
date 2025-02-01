using ScoreManagement.Model;

namespace ScoreManagement.Interfaces
{
    public interface ILovContantQuery
    {
        Task<List<LovContantsResource>> GetLovSendStatusQuery();
        Task<List<LovContantsResource>> GetLovMajorCodeQuery();
        Task<List<LovContantsResource>> GetLovRoleQuery();
        Task<List<LovContantsResource>> GetLovAcedemicYearQuery();
        Task<List<LovContantsResource>> GetLovScoreTypeQuery();
        Task<List<LovContantsResource>> GetLovSemesterQuery();
        Task<List<LovContantsResource>> GetLovSectionQuery();
        Task<List<LovContantsResource>> GetLovActiveStatusQuery();
        Task<List<SubjectResource>> GetSubjectByConditionQuery(SubjectResource resource);
        Task<List<SubjectResource>> GetLovSubject();
    }

}
