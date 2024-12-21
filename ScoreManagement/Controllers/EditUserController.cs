using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoreManagement.Model;
using ScoreManagement.Interfaces;
using ScoreManagement.Controllers.Base;
using System.Text;
using ScoreManagement.Services.Encrypt;
using ScoreManagement.Model.Table;

namespace ScoreManagement.Controllers
{
    [AllowAnonymous]
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EditUserController : BaseController
    {
        private readonly IUserQuery _userQuery;
        private readonly IEncryptService _encryptService;

        public EditUserController(IUserQuery userQuery, IEncryptService encryptService)
        {
            _userQuery = userQuery;
            _encryptService = encryptService;
        }

        [HttpGet("GetAllUser")]
        public async Task<IActionResult> GetAllUsersEdit()
        {
            bool isSuccess = false;
            string message = string.Empty;
            try
            {
                var users = await _userQuery.GetAllUsers();

                if (users == null || users.Count == 0)
                {
                    message = "No data exist in this table or view";
                }

                isSuccess = true; 
                var response = ApiResponse(
                    isSuccess: isSuccess,
                    messageDescription: message,
                    objectResponse: users

                    ); 
                return StatusCode(200, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred", error = ex.Message });
            }
        }
        [HttpPost("InsertUser")]
        public async Task<IActionResult> InsertUser([FromBody] List<UserResource> resources)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid input data.", errors = ModelState });
            }

            bool isSuccess = false;
            string message = string.Empty;
            try
            {
                foreach (var resource in resources)
                {
                    //user.password = _encryptService.EncryptPassword(user.password!);
                    resource.password = _encryptService.EncryptPassword("1234");

                    resource.create_date = DateTime.Now;
                    //resource.create_by = User.Identity?.Name ?? "Rawee";
                    resource.active_status = "active";

                    isSuccess = await _userQuery.InsertUser(resource);
                    message = isSuccess ? "User inserted successfully." : "Failed to insert user.";
                }

                var response = ApiResponse(
                    isSuccess: isSuccess,
                    messageDescription: message,
                    objectResponse: resources);
                return StatusCode(isSuccess ? 201 : 400, response);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred", error = ex.Message });
            }
        }
    }
}