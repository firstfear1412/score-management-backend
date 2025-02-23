using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoreManagement.Model;
using ScoreManagement.Interfaces;
using ScoreManagement.Controllers.Base;
using System.Text;
using ScoreManagement.Services;
using ScoreManagement.Model.Table;

namespace ScoreManagement.Controllers
{
    //[AllowAnonymous]
    [Authorize(Policy = "Admin")]
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


        //[HttpPost("InsertUser")]
        //public async Task<IActionResult> InsertUser([FromBody] List<UserResource> resources, [FromQuery] string language = "th")
        //{
        //    language = string.IsNullOrEmpty(language) ? "th" : language.ToLower();

        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(new
        //        {
        //            success = false,
        //            message = language == "th" ? "ข้อมูลที่ส่งมาไม่ถูกต้อง" : "Invalid input data.",
        //            errors = ModelState
        //        });
        //    }

        //    try
        //    {

        //        var existingEmails = new List<string>();
        //        foreach (var resource in resources)
        //        {
        //            if (await _userQuery.CheckEmailExist(resource.email))
        //            {
        //                existingEmails.Add(resource.email);
        //            }
        //        }

        //        if (existingEmails.Any())
        //        {
        //            return BadRequest(new
        //            {
        //                success = false,
        //                message = language == "th" ? "มีอีเมลบางรายการที่ใช้งานแล้ว" : "Some emails are already in use.",
        //                errors = existingEmails.Select(email => new
        //                {
        //                    th = $"{email}",
        //                    en = $"Email {email} already exists in the system."
        //                }).Select(e => e.GetType().GetProperty(language).GetValue(e).ToString())
        //            });
        //        }

        //        var existingTeacherCode = new List<string>();
        //        foreach (var resource in resources)
        //        {
        //            if (await _userQuery.CheckTeacherCodeExist(resource.teacher_code))
        //            {
        //                existingTeacherCode.Add(resource.teacher_code);
        //            }
        //        }

        //        if (existingTeacherCode.Any())
        //        {
        //            return BadRequest(new
        //            {
        //                success = false,
        //                message = language == "th" ? "มีรหัสอาจารย์ถูกใช้งานแล้ว" : "Some TeacherCode are already in use.",
        //                errors = existingTeacherCode.Select(teacher_code => new
        //                {
        //                    th = $"{teacher_code}",
        //                    en = $"{teacher_code}"
        //                }).Select(e => e.GetType().GetProperty(language).GetValue(e).ToString())
        //            });
        //        }

        //        // Insert users
        //        foreach (var resource in resources)
        //        {
        //            resource.email = resource.email?.Trim();
        //            resource.teacher_code = resource.teacher_code?.Trim();
        //            resource.prefix = resource.prefix?.Trim();
        //            resource.firstname = resource.firstname?.Trim();
        //            resource.lastname = resource.lastname?.Trim();
        //            resource.role = resource.role?.Trim();
        //            resource.create_by = resource.create_by?.Trim();

        //            resource.password = _encryptService.EncryptPassword(resource.teacher_code);
        //            resource.create_date = DateTime.Now;
        //            resource.active_status = "active";

        //            await _userQuery.InsertUser(resource);
        //        }

        //        return Ok(new
        //        {
        //            success = true,
        //            message = language == "th" ? "บันทึกข้อมูลสำเร็จ" : "All users inserted successfully.",
        //            data = resources
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            success = false,
        //            message = language == "th" ? "เกิดข้อผิดพลาด" : "An error occurred",
        //            error = ex.Message
        //        });
        //    }
        //}

        [HttpPost("InsertUser")]
        public async Task<IActionResult> InsertUser([FromBody] List<UserResource> resources, [FromQuery] string language = "th")
        {
            language = string.IsNullOrEmpty(language) ? "th" : language.ToLower();

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = language == "th" ? "ข้อมูลที่ส่งมาไม่ถูกต้อง" : "Invalid input data.",
                    errors = ModelState
                });
            }

            try
            {
                var existingEmails = new List<string>();
                var existingTeacherCode = new List<string>();
                var bothDuplicateData = new List<UserResource>();

                foreach (var resource in resources)
                {
                    // ตรวจสอบอีเมลซ้ำ
                    if (await _userQuery.CheckEmailExist(resource.email))
                    {
                        existingEmails.Add(resource.email);
                    }

                    // ตรวจสอบรหัสอาจารย์ซ้ำ
                    if (await _userQuery.CheckTeacherCodeExist(resource.teacher_code))
                    {
                        existingTeacherCode.Add(resource.teacher_code);
                    }

                    // ตรวจสอบกรณีที่มีทั้งอีเมลและรหัสอาจารย์ซ้ำ
                    if (await _userQuery.CheckEmailExist(resource.email) && await _userQuery.CheckTeacherCodeExist(resource.teacher_code))
                    {
                        bothDuplicateData.Add(resource);
                    }
                }

                // แยกข้อมูลที่ไม่ซ้ำ
                var nonDuplicateData = resources.Where(r =>
                    !existingEmails.Contains(r.email) && !existingTeacherCode.Contains(r.teacher_code)).ToList();

                // ส่งข้อมูลกลับไปยัง frontend
                if (nonDuplicateData.Any())
                {
                    var existingDataMessage = new
                    {
                        success = true,
                        message = language == "th" ? "ข้อมูลที่สามารถบันทึกได้" : "Valid Data Found",
                        validResources = nonDuplicateData.Select(r => new
                        {
                            r.email,
                            r.teacher_code,
                            r.prefix,
                            r.firstname,
                            r.lastname,
                            r.role,
                            r.create_by // Add other necessary fields here
                        }).ToList(),
                        existingEmails,
                        existingTeacherCode,
                        bothDuplicateData
                    };

                    return Ok(existingDataMessage);
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = language == "th" ? "ไม่มีข้อมูลที่สามารถบันทึกได้" : "No valid data",
                        errors = existingEmails.Select(email => $"{email}")
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = language == "th" ? "เกิดข้อผิดพลาด" : "An error occurred",
                    error = ex.Message
                });
            }
        }


        [HttpPost("SaveValidUsers")]
        public async Task<IActionResult> SaveValidUsers([FromBody] List<UserResource> validResources, [FromQuery] string language = "th")
        {
            try
            {
                foreach (var resource in validResources)
                {
                    // Trim and add additional information as needed
                    resource.email = resource.email?.Trim();
                    resource.teacher_code = resource.teacher_code?.Trim();
                    resource.prefix = resource.prefix?.Trim();
                    resource.firstname = resource.firstname?.Trim();
                    resource.lastname = resource.lastname?.Trim();
                    resource.role = resource.role?.Trim();
                    resource.create_by = resource.create_by?.Trim();

                    // Encrypt password and set other necessary fields
                    resource.password = _encryptService.EncryptPassword(resource.teacher_code);
                    resource.create_date = DateTime.Now;
                    resource.active_status = "active";

                    // Insert into the database
                    await _userQuery.InsertUser(resource);
                }

                return Ok(new
                {
                    success = true,
                    message = language == "th" ? "บันทึกข้อมูลสำเร็จ" : "All users inserted successfully.",
                    data = validResources
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = language == "th" ? "เกิดข้อผิดพลาดในการบันทึกข้อมูล" : "An error occurred while saving data",
                    error = ex.Message
                });
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