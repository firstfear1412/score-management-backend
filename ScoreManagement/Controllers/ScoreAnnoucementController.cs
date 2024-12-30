using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Cmp;
using ScoreManagement.Common;
using ScoreManagement.Controllers.Base;
using ScoreManagement.Entity;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;
using ScoreManagement.Model.ScoreAnnoucement;
using ScoreManagement.Model.Table;
using ScoreManagement.Query;
using ScoreManagement.Services.Encrypt;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        public ScoreAnnoucementController(scoreDB context, IEncryptService encryptService, IConfiguration configuration, IStudentScoreQuery studentScoreQuery)
        {
            _context = context;
            _encryptService = encryptService;
            _configuration = configuration;
            _studentScoreQuery = studentScoreQuery;
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
                    var scoreQuery = await _studentScoreQuery.GetScoreAnnoucementByCondition(resource); // Call GetUserInfo method to retrieve user data
                    

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
            catch (Exception ex)
            {
                _webEvent.WriteLogException(resource.score_create_by!, messageDesc.Trim(), ex, pathBase);
                messageDesc = ex.Message;
            }

            if (!isSuccess)
            {
                _webEvent.WriteLogInfo(resource.score_create_by!, messageDesc.Trim(), pathBase);
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
