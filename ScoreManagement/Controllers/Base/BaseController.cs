using Microsoft.AspNetCore.Mvc;
using ScoreManagement.Common;
using ScoreManagement.Model;

namespace ScoreManagement.Controllers.Base
{
    public class BaseController : Controller
    {
        protected readonly WebEvent _webEvent;
        public BaseController()
        {
            _webEvent = new WebEvent();
        }
        protected ApiResponse<T> ApiResponse<T>(bool isSuccess, string messageKey = "", string messageDescription = "", T? objectResponse = default, T? parameter = default, T? tokenResult = default)
        {
            int objectCount = 0;
            if (objectResponse is IEnumerable<object> collection)
            {
                objectCount = collection.Count();
            }
            return new ApiResponse<T>
            {
                IsSuccess = isSuccess,
                ObjectCount = objectCount,
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
