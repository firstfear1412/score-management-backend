using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoreManagement.Model;
using ScoreManagement.Interfaces;
using ScoreManagement.Controllers.Base;

namespace ScoreManagement.Controllers
{
    //[AllowAnonymous]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GetUserController : BaseController
    {
        private readonly IUserQuery _userQuery;

        public GetUserController(IUserQuery userQuery)
        {
            _userQuery = userQuery;
        }

        [HttpGet("users")]
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
    }
}