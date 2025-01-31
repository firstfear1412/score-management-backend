using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoreManagement.Controllers.Base;
using ScoreManagement.Entity;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;
using ScoreManagement.Model.ScoreAnnoucement;
using ScoreManagement.Services;

namespace ScoreManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ScoreAnnoucementController : BaseController
    {
        private readonly scoreDB _context;
        private readonly IEncryptService _encryptService;
        private readonly IConfiguration _configuration;
        private readonly IStudentScoreQuery _studentScoreQuery;
        private readonly ILovContantQuery _LovContantQuery;
        public ScoreAnnoucementController(scoreDB context, IEncryptService encryptService, IConfiguration configuration, IStudentScoreQuery studentScoreQuery, ILovContantQuery lovContantQuery)
        {
            _context = context;
            _encryptService = encryptService;
            _configuration = configuration;
            _studentScoreQuery = studentScoreQuery;
            _LovContantQuery = lovContantQuery;
        }

        [AllowAnonymous]
        [HttpPost("GetScoreAnnoucementByCondition")]
        public async Task<IActionResult> GetScoreAnnoucementControllers([FromBody] ScoreAnnoucementResource resource)
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
                    var scoreQuery = await _studentScoreQuery.GetScoreAnnoucementByConditionQuery(resource); // Call GetUserInfo method to retrieve user data


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
        [HttpPost("GetSubjectByCondition")]
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
                    var scoreQuery = await _LovContantQuery.GetSubjectByConditionQuery(resource); // Call GetUserInfo method to retrieve user data


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
    }
}
