using Microsoft.AspNetCore.Mvc;
using ScoreManagement.Model;
using ScoreManagement.Model.ScoreAnnoucement;
using ScoreManagement.Model.SubjectScore;
using ScoreManagement.Model.Table;

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
    }

}
