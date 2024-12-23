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
                    resource.password = _encryptService.EncryptPassword(resource.teacher_code);

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
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred", error = ex.Message });
            }
        }
        [HttpPost("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UserResource resource)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid input data.", errors = ModelState });
            }

            try
            {
                resource.password = _encryptService.EncryptPassword(resource.password);
                resource.update_date = DateTime.Now;
                // สร้าง query string แบบ dynamic โดยระบุเฉพาะฟิลด์ที่มีค่า
                var queryBuilder = new List<string>();

                if (!string.IsNullOrWhiteSpace(resource.firstname))
                {
                    queryBuilder.Add("[firstname] = @firstname");
                }

                if (!string.IsNullOrWhiteSpace(resource.lastname))
                {
                    queryBuilder.Add("[lastname] = @lastname");
                }

                if (!string.IsNullOrWhiteSpace(resource.email))
                {
                    queryBuilder.Add("[email] = @email");
                }

                //if (!string.IsNullOrWhiteSpace(resource.username)) 
                //{
                //    queryBuilder.Add("[username] = @username");
                //}

                if (!string.IsNullOrWhiteSpace(resource.password))
                {
                    queryBuilder.Add("[password] = @password");
                }

                if (!string.IsNullOrWhiteSpace(resource.role))
                {
                    queryBuilder.Add("[role] = @role");
                }

                if (!string.IsNullOrWhiteSpace(resource.teacher_code))
                {
                    queryBuilder.Add("[teacher_code] = @teacher_code");
                }

                if (!string.IsNullOrWhiteSpace(resource.prefix))
                {
                    queryBuilder.Add("[prefix] = @prefix");
                }

                if (!string.IsNullOrWhiteSpace(resource.active_status))
                {
                    queryBuilder.Add("[active_status] = @active_status");
                }

                queryBuilder.Add("[update_date] = @update_date");

                queryBuilder.Add("[update_by] = @update_by");


                if (queryBuilder.Count == 0)
                {
                    return BadRequest(new { success = false, message = "No fields to update." });
                }

                var query = string.Join(", ", queryBuilder);

                // เรียกใช้ UpdateUser ใน IUserQuery
                var isUpdated = await _userQuery.UpdateUserById(resource, query);

                if (isUpdated)
                {
                    return Ok(new { success = true, message = "User updated successfully." });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to update user." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred", error = ex.Message });
            }
        }
    }
}