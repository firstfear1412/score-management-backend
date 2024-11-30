using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ScoreManagement.Common;
using ScoreManagement.Entity;
using ScoreManagement.Model.Table.User;
using ScoreManagement.Model.User;
using ScoreManagement.Services.Encrypt;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ScoreManagement.Controllers
{
    [Authorize]
    [Route("api")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly demoDB _context;
        private readonly IEncryptService _encryptService;
        private readonly IConfiguration _configuration;
        public UserController(demoDB context, IEncryptService encryptService, IConfiguration configuration)
        {
            _context = context;
            _encryptService = encryptService;
            _configuration = configuration;
        }
        [AllowAnonymous]
        [HttpPost("GetToken")]
        public async Task<IActionResult> GetTokenControllers([FromBody] UserResource resource)
        {
            var pathBase = HttpContext;
            var message = string.Empty;
            object? tokenResult = null;
            var isSuccess = false;
            var ErrorMessage = new ErrorMessage();
            try
            {

                if (!string.IsNullOrEmpty(resource.username) && !string.IsNullOrEmpty(resource.password))
                {
                    bool flg = false;
                    var users = await _context.Users.Where(a => a.username!.Equals(resource.username!)
                                                                    && (a.active_status!.Equals("active"))
                                                                    ).FirstOrDefaultAsync();
                    if (users != null)
                    {
                        #region comment
                        //if (users.role == 0)
                        //{
                        //    if (!string.IsNullOrEmpty(ErrorMessage.ErrorText))
                        //        resource.response.ErrorMessage.Add(ErrorMessage.ErrorText);
                        //    return StatusCode(200, resource.response);
                        //}
                        //if (users.total_failed >= 3 && users.date_login.Value.Date == DateTime.Now.Date)
                        //{
                        //    //ErrorMessage.Add("Your Account Has been Blocked !!");
                        //    ErrorMessage.AddMessageFromStatusCode(409); //error : Your username is blocked. Please contact the staff. 
                        //    if (!string.IsNullOrEmpty(ErrorMessage.ErrorText))
                        //        resource.response.ErrorMessage.Add(ErrorMessage.ErrorText);
                        //    return StatusCode(200, resource.response);
                        //}
                        #endregion comment
                        flg = _encryptService.VerifyHashedPassword(users.password, resource.password);
                        if (flg)
                        {
                            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
                            IConfigurationRoot configuration = builder.Build();

                            string issuer = configuration["JWT:Issuer"];
                            string privateKey = configuration["JWT:PrivateKey"];
                            double MaxTokenHour = Convert.ToDouble(configuration["JWT:MaxTokenHour"]);

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
                                //expiration = jwtToken.ValidTo
                                expiration = DateTime.Now.AddHours(MaxTokenHour)
                            };
                            isSuccess = true;
                            //message = string.Format("Token will expire : {0}", tokenResult.expiration);
                        }
                        else
                        {
                            #region comment
                            //int i = users.total_failed.Value;
                            //if (users.date_login.Value.Date != DateTime.Now.Date)
                            //{
                            //    users.total_failed = 1;
                            //    users.date_login = DateTime.Now;
                            //    resource.response.totalLoginFailOfDay = users.total_failed.Value;
                            //    //ErrorMessage.Add("Username / password is incorrect.");
                            //    ErrorMessage.AddMessageFromStatusCode(408); //error : Your username / password is incorrect. You can only enter it incorrectly 3 times.
                            //}
                            //else if (users.date_login.Value.Date == DateTime.Now.Date && users.total_failed < 3)
                            //{
                            //    users.total_failed = i + 1;
                            //    users.date_login = DateTime.Now;
                            //    resource.response.totalLoginFailOfDay = users.total_failed.Value;
                            //    ErrorMessage.AddMessageFromStatusCode(408); //error : Your username / password is incorrect. You can only enter it incorrectly 3 times.
                            //}
                            //else
                            //{
                            //    //ErrorMessage.Add("Your Account Has been Blocked !!");
                            //    ErrorMessage.AddMessageFromStatusCode(409); //error : Your username is blocked. Please contact the staff. 
                            //}
                            #endregion comment
                            users.total_failed = users.total_failed + 1;
                            users.update_date = DateTime.Now;
                            message = "password incorrect";
                        }
                    }
                    else
                    {
                        message = "user not found";
                    }
                }
                else
                {
                    message = "input required";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //ErrorMessage.Add("Exception : " + ex.Message);
                ////ErrorMessage.Add("There is a problem with the information, please contact the developer !");
                ErrorMessage.WriteLog(resource.username, ErrorMessage.ErrorText.Trim(), ex, pathBase);
                message = ex.Message;
            }

            //if (!string.IsNullOrEmpty(ErrorMessage.ErrorText))
            //    resource.response.ErrorMessage.Add(ErrorMessage.ErrorText);
            return StatusCode(200, new
            {
                isSuccess = isSuccess,
                message = message,
                tokenResult = tokenResult
            });
        }

        [AllowAnonymous]
        [HttpPost("create/manual")]
        public async Task<IActionResult> CreatUserManual([FromBody] User model)
        {

            bool isSuccess = false;
            string message = string.Empty;
            string hashedPasswordBase64 = string.Empty;
            try
            {
                hashedPasswordBase64 = _encryptService.EncryptPassword(model.password);
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
            var response = new
            {
                isSuccess = isSuccess,
                message = message,
                hashedPassword = hashedPasswordBase64
            };
            return Ok(response);
        }
    }
}
