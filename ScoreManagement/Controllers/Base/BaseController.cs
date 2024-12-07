using Microsoft.AspNetCore.Mvc;
using ScoreManagement.Model;

namespace ScoreManagement.Controllers.Base
{
    public class BaseController : Controller
    {
        protected ApiResponse<T> ApiResponse<T>(bool isSuccess, string messageKey = "", string messageDescription = "", T? objectResponse = default, T? parameter = default, T? tokenResult = default)
        {
            return new ApiResponse<T>
            {
                IsSuccess = isSuccess,
                Message = new ApiMessage
                {
                    MessageKey = messageKey,
                    MessageDescription = messageDescription
                },
                ObjectResponse = objectResponse,
                Parameter = parameter,
                TokenResult = tokenResult
            };
        }
    }
}
