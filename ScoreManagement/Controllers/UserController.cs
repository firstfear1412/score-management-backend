using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using ScoreManagement.Common;
using ScoreManagement.Controllers.Base;
using ScoreManagement.Entity;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;
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
    public class UserController : BaseController
    {
        private readonly scoreDB _context;
        private readonly IEncryptService _encryptService;
        private readonly IConfiguration _configuration;
        private readonly IUserQuery _userQuery;
        public UserController(scoreDB context, IEncryptService encryptService, IConfiguration configuration, IUserQuery userQuery)
        {
            _context = context;
            _encryptService = encryptService;
            _configuration = configuration;
            _userQuery = userQuery;
        }
        [AllowAnonymous]
        [HttpPost("GetToken")]
        public async Task<IActionResult> GetTokenControllers([FromBody] UserResource resource)
        {
            var pathBase = HttpContext;
            var messageDesc = string.Empty;
            var messageKey = string.Empty;
            object? tokenResult = null;
            var isSuccess = false;
            var ErrorMessage = new ErrorMessage();
            var response = new ApiResponse<object>();
            try
            {

                if (!string.IsNullOrEmpty(resource.username) && !string.IsNullOrEmpty(resource.password))
                {
                    bool flg = false;
                    var users = await _userQuery.GetUser(resource)!;

                    if (users != null)
                    {
                        flg = _encryptService.VerifyHashedPassword(users.password!, resource.password);
                        if (flg)
                        {
                            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
                            IConfigurationRoot configuration = builder.Build();

                            string issuer = configuration["JWT:Issuer"]!;
                            string privateKey = configuration["JWT:PrivateKey"]!;
                            double MaxTokenHour = Convert.ToDouble(configuration["JWT:MaxTokenHour"]!);

                            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey));
                            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                            var claims = new[]
                            {
                                 new Claim("user", resource.username),
                                 new Claim("password", resource.password),
                                 //new Claim("tokenType", "login"),
                            };

                            var jwtToken = new JwtSecurityToken(
                              issuer: issuer,
                              audience: issuer,
                              claims: claims,
                              //expires: DateTime.UtcNow.AddMinutes(3),
                              expires: DateTime.UtcNow.AddHours(MaxTokenHour),
                              signingCredentials: creds);

                            users.date_login = DateTime.Now;
                            users.update_date = DateTime.Now;
                            users.total_failed = 0;
                            tokenResult = new
                            {
                                token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                                expiration = jwtToken.ValidTo
                            };
                            isSuccess = true;
                            //message = string.Format("Token will expire : {0}", tokenResult.expiration);
                        }
                        else
                        {
                            users.total_failed = users.total_failed + 1;
                            users.update_date = DateTime.Now;
                            //message = "password incorrect";
                            messageKey = "login_failed";
                            messageDesc  = "username / password incorrect";
                        }
                    }
                    else
                    {
                        //message = "user not found";
                        messageKey = "login_user_not_found";
                        messageDesc = "user not found";
                    }
                }
                else
                {
                    //message = "input required";

                    messageDesc = "field is required";
                }

                //_context.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorMessage.WriteLogException(resource.username!, messageDesc.Trim(), ex, pathBase);
                messageDesc = ex.Message;
            }

            if (!isSuccess)
            {
                ErrorMessage.WriteLogInfo(resource.username!, messageDesc.Trim(), pathBase);
            }
            response.IsSuccess = isSuccess;
            response.Message = new ApiMessage
            {
                MessageKey = messageKey,
                MessageDescription = messageDesc
            };
            response.TokenResult = tokenResult;
            return StatusCode(200, response);
        }

        [AllowAnonymous]
        [HttpPost("CreateManual")]
        public IActionResult CreatUserManual([FromBody] User model)
        {

            bool isSuccess = false;
            string message = string.Empty;
            string hashedPasswordBase64 = string.Empty;
            try
            {
                hashedPasswordBase64 = _encryptService.EncryptPassword(model.password!);
                if (!string.IsNullOrEmpty(hashedPasswordBase64))
                {
                    isSuccess = true;
                    message = "Password encoded successfully.";
                }
                else
                {
                    message = "Password is empty or invalid.";
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                message = ex.Message.ToString();
            }
            var response = ApiResponse(
                isSuccess: isSuccess,
                messageDescription: message,
                objectResponse: new { hashedPassword = hashedPasswordBase64 }
            );
            //จะ response
            /*
             {
                  "isSuccess": true,
                  "message": {
                    "messageKey": "",
                    "messageDescription": "Password encoded successfully."
                  },
                  "objectResponse": {
                    "hashedPassword": "AGf9vEWj4/yBNtpkeGtB7SFT+cL+aIr5UKMsNbAAF2gjE4v3B9VBxpVOYdVKMW7FSA=="
                  }
             } 
            */
            return StatusCode(200, response);
        }
    }
}
