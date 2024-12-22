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
            HttpContext pathBase = HttpContext;
            string messageDesc = string.Empty;
            string messageKey = string.Empty;
            object? tokenResult = null;
            bool isSuccess = false;
            string sql = string.Empty;
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
                            string issuer = _configuration["JWT:Issuer"]!;
                            string privateKey = _configuration["JWT:PrivateKey"]!;
                            double MaxTokenHour = Convert.ToDouble(_configuration["JWT:MaxTokenHour"]!);

                            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey));
                            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                            var claims = new[]
                            {
                                 new Claim("username", resource.username),
                                 new Claim("role", users.role.ToString()!),
                                 //new Claim("password", resource.password),
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
                            sql = @" [date_login] = @date_login, [update_date] = @update_date, [total_failed] = @total_failed ";
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
                            sql = @" [total_failed] = @total_failed, [update_date] = @update_date ";
                            //message = "password incorrect";
                            messageKey = "login_failed";
                            messageDesc = "Invalid password. Please try again or contact admin.";
                        }
                        //update user login
                        flg = await _userQuery.UpdateUser(users, sql);
                    }
                    else
                    {
                        //message = "user not found";
                        messageKey = "login_user_not_found";
                        messageDesc = "User not found. Please try again or contact admin.";
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
                _webEvent.WriteLogException(resource.username!, messageDesc.Trim(), ex, pathBase);
                messageDesc = ex.Message;
            }

            if (!isSuccess)
            {
                _webEvent.WriteLogInfo(resource.username!, messageDesc.Trim(), pathBase);
            }

            var response = ApiResponse(
                isSuccess: isSuccess,
                messageKey: messageKey,
                messageDescription: messageDesc,
                tokenResult: tokenResult
            );
            return StatusCode(200, response);
        }

        [AllowAnonymous]
        [HttpPost("GetUserInfo")]
        public async Task<IActionResult> GetUserInfoControllers([FromBody] UserResource resource)
        {
            HttpContext pathBase = HttpContext;
            string messageDesc = string.Empty;
            string messageKey = string.Empty;
            object? userInfo = null;  // Store user information here
            bool isSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(resource.username))
                {
                    // Fetch user information from GetUserInfo
                    var user = await _userQuery.GetUserInfo(resource); // Call GetUserInfo method to retrieve user data

                    if (user != null)
                    {
                        userInfo = user;  // Store the user info object here
                        isSuccess = true;
                        messageKey = "login_success";
                        messageDesc = "User data retrieved successfully.";
                    }
                    else
                    {
                        messageKey = "user_not_found";
                        messageDesc = "User not found.";
                    }
                }
                else
                {
                    messageDesc = "Username is required";
                }
            }
            catch (Exception ex)
            {
                _webEvent.WriteLogException(resource.username!, messageDesc.Trim(), ex, pathBase);
                messageDesc = ex.Message;
            }

            if (!isSuccess)
            {
                _webEvent.WriteLogInfo(resource.username!, messageDesc.Trim(), pathBase);
            }

            // Respond with the user info or error message
            var response = ApiResponse(
                isSuccess: isSuccess,
                messageKey: messageKey,
                messageDescription: messageDesc,
                objectResponse: userInfo
            );

            return StatusCode(200, response);
        }




        [AllowAnonymous]
        [HttpPost("CreateManual")]
        public IActionResult CreatUserManual([FromBody] User model)
        {
            HttpContext pathBase = HttpContext;
            string messageDesc = string.Empty;
            bool isSuccess = false;
            string hashedPasswordBase64 = string.Empty;
            try
            {
                hashedPasswordBase64 = _encryptService.EncryptPassword(model.password!);
                if (!string.IsNullOrEmpty(hashedPasswordBase64))
                {
                    isSuccess = true;
                    messageDesc = "Password encoded successfully.";
                }
                else
                {
                    messageDesc = "Password is empty or invalid.";
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                messageDesc = ex.Message.ToString();
                _webEvent.WriteLogException("CreateUserManual", messageDesc.Trim(), ex, pathBase);
            }
            var response = ApiResponse(
                isSuccess: isSuccess,
                messageDescription: messageDesc,
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
