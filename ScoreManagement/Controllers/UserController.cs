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
        public UserController(demoDB context, IEncryptService encryptService)
        {
            _context = context;
            _encryptService = encryptService;
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

                            //var tokenResult = new

                            //{
                            //    token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                            //    expiration = jwtToken.ValidTo
                            //};
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
                result = isSuccess,
                message = message,
                tokenResult = tokenResult
            });
        }

        [AllowAnonymous]
        [HttpPost("create/passmanual")]
        public async Task<IActionResult> CreatUserManual([FromBody] User model)
        {
            var ErrorMessage = new ErrorMessage();
            const int Pbkdf2SubkeyLength = 256 / 8; // 256 bits
            const int SaltSize = 128 / 8; // 128 bits
            string passHash = _encryptService.Hash(model.password, "sha256");
            byte[] salt = new byte[SaltSize];
            string hashedPasswordBase64 = "";
            bool isError = false;
            string massage = "";
            try
            {
                if (!string.IsNullOrEmpty(model.password))
                {
                    using (var rng = RandomNumberGenerator.Create())
                    {
                        rng.GetBytes(salt);
                    }

                    using (var pbkdf2 = new Rfc2898DeriveBytes(model.password, salt, 1000, HashAlgorithmName.SHA1))
                    {
                        byte[] subkey = pbkdf2.GetBytes(Pbkdf2SubkeyLength);

                        byte[] decodedHashedPassword = new byte[1 + SaltSize + Pbkdf2SubkeyLength];
                        decodedHashedPassword[0] = 0x00;
                        Buffer.BlockCopy(salt, 0, decodedHashedPassword, 1, SaltSize);
                        Buffer.BlockCopy(subkey, 0, decodedHashedPassword, 1 + SaltSize, Pbkdf2SubkeyLength);
                        hashedPasswordBase64 = Convert.ToBase64String(decodedHashedPassword);
                        isError = true;
                    }
                }
                else
                {
                    isError = false;
                    massage = "Password is empty";
                }
            }
            catch (Exception ex)
            {
                isError = false;
                massage = ex.Message.ToString();
            }
            var response = new
            {
                IsError = isError,
                Massage = massage,
                HashedPassword = hashedPasswordBase64
            };
            return Ok(response);
        }
    }
}
