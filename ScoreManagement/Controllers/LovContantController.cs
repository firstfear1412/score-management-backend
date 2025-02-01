using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoreManagement.Controllers.Base;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;
using ScoreManagement.Services;


namespace ScoreManagement.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class LovContantController : BaseController
    {
        private readonly IEncryptService _encryptService;
        private readonly ILovContantQuery _lovContantQuery;
        private readonly ILogger<LovContantController> _logger;

        public LovContantController(IEncryptService encryptService, ILovContantQuery lovContantQuery, ILogger<LovContantController> logger)
        {
            _encryptService = encryptService;
            _lovContantQuery = lovContantQuery;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("GetLovSendStatus")]
        public async Task<IActionResult> GetLovSendStatus()
        {
            var result = await _lovContantQuery.GetLovSendStatusQuery();

            return await GetLovDataResponse(result);
        }

        [AllowAnonymous]
        [HttpPost("GetLovMajorCode")]
        public async Task<IActionResult> GetLovMajorCode()
        {
            var result = await _lovContantQuery.GetLovMajorCodeQuery();

            return await GetLovDataResponse(result);
        }

        [AllowAnonymous]
        [HttpPost("GetLovRole")]
        public async Task<IActionResult> GetLovRole()
        {
            var result = await _lovContantQuery.GetLovRoleQuery();

            return await GetLovDataResponse(result);
        }

        [AllowAnonymous]
        [HttpPost("GetLovAcademicYear")]
        public async Task<IActionResult> GetLovAcademicYear()
        {
            var result = await _lovContantQuery.GetLovAcedemicYearQuery();

            return await GetLovDataResponse(result);
        }

        [AllowAnonymous]
        [HttpPost("GetLovScoreType")]
        public async Task<IActionResult> GetLovScoreType()
        {
            var result = await _lovContantQuery.GetLovScoreTypeQuery();
            return await GetLovDataResponse(result);
        }

        [AllowAnonymous]
        [HttpPost("GetLovSemester")]
        public async Task<IActionResult> GetLovSemester()
        {
            var result = await _lovContantQuery.GetLovSemesterQuery();
            return await GetLovDataResponse(result);
        }

        [AllowAnonymous]
        [HttpPost("GetLovSection")]
        public async Task<IActionResult> GetLovSection()
        {
            var result = await _lovContantQuery.GetLovSectionQuery();
            return await GetLovDataResponse(result);
        }
        [AllowAnonymous]
        [HttpPost("GetLovSubject")]
        public async Task<IActionResult> GetSubjectByConditionController([FromBody] SubjectResource resource)
        {
            HttpContext pathBase = HttpContext;
            string messageDesc = string.Empty;
            string messageKey = string.Empty;
            object? scoreList = null;  // Store user information here
            bool isSuccess = false;

            try
            {
                //if(!string.IsNullOrEmpty(resource.teacher_code) ) { 
                if (true)
                {
                    var scoreQuery = await _lovContantQuery.GetLovSubject(); // Call GetUserInfo method to retrieve user data


                    if (scoreQuery != null && scoreQuery.Any())
                    {
                        scoreList = scoreQuery;
                        isSuccess = true;
                        messageKey = "data_found";
                        messageDesc = "Data found";
                    }
                    else
                    {
                        messageKey = "data_not_found";
                        messageDesc = "Data not found";
                    }
                }
                else
                {
                    messageDesc = "กรุณากรอกรหัสวิชาหรือรายชื่อวิชา";
                }

            }
            catch (Exception ex)
            {
                //_webEvent.WriteLogException(resource.score_create_by!, messageDesc.Trim(), ex, pathBase);
                messageDesc = ex.Message;
            }

            if (!isSuccess)
            {
                //_webEvent.WriteLogInfo(resource.score_create_by!, messageDesc.Trim(), pathBase);
            }

            // Respond with the user info or error message
            var response = ApiResponse(
                isSuccess: isSuccess,
                messageKey: messageKey,
                messageDescription: messageDesc,
                objectResponse: scoreList
            );

            return StatusCode(200, response);
        }

        [AllowAnonymous]
        [HttpPost("GetLovActiveStatus")]
        public async Task<IActionResult> GetLovActiveStatus()
        {
            var result = await _lovContantQuery.GetLovActiveStatusQuery();
            return await GetLovDataResponse(result);
        }

        private async Task<IActionResult> GetLovDataResponse(object result)
        {
            string messageDesc = string.Empty;
            string messageKey = string.Empty;
            object? lovResultList = result;
            bool isSuccess = false;

            try
            {
                // Invoke the provided query function

                if (result != null)
                {
                    lovResultList = result;
                    isSuccess = true;
                    messageKey = "data_found";
                    messageDesc = "Data found";
                }
                else
                {
                    messageKey = "data_not_found";
                    messageDesc = "No data found";
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while fetching data");

                return StatusCode(500, new { success = false, message = "An error occurred", error = ex.Message });
            }

            // Respond with the result of the query
            var response = ApiResponse(
                isSuccess: isSuccess,
                messageKey: messageKey,
                messageDescription: messageDesc,
                objectResponse: lovResultList
            );

            return StatusCode(200, response);
        }


    }



}
