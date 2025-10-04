using ScoreManagement.Model.Table;
using System.Threading.Tasks;
using ScoreManagement.Model;

namespace ScoreManagement.Interfaces
{
    public interface ISystemParamQuery
    {
        Task<List<SystemParamResource>> GetSysbyteDesc();

        Task<bool> UpdateSystemParam(SystemParamResource param);

        Task<(bool IsSuccess, string Message)> InsertSystemParam(SystemParamResource param);

    }
}