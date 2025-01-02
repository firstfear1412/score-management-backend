using MathNet.Numerics.Distributions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ScoreManagement.Controllers.Base;
using ScoreManagement.Entity;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;
using ScoreManagement.Model.SubjectScore;
using ScoreManagement.Model.Table;
using ScoreManagement.Query;
using ScoreManagement.Services.Encrypt;
using ScoreManagement.Services.Mail;
//using System.Reflection;
using System.Text.RegularExpressions;

namespace ScoreManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentScoreController : BaseController
    {
        private readonly scoreDB _context;
        private readonly IEncryptService _encryptService;
        private readonly IConfiguration _configuration;
        private readonly IStudentScoreQuery _studentScoreQuery;
        private readonly MailService _mailService;
        public StudentScoreController(scoreDB context, IEncryptService encryptService, IConfiguration configuration, IStudentScoreQuery studentScoreQuery, IMailService mailService)
        {
            _context = context;
            _encryptService = encryptService;
            _configuration = configuration;
            _studentScoreQuery = studentScoreQuery;
            _mailService = (MailService)mailService;
        }

        #region controller
        [AllowAnonymous]
        [HttpPost("UploadScore")]
        public async Task<IActionResult> UploadStudentScore([FromBody] UploadScoreResource resource)
        {
            HttpContext pathBase = HttpContext;
            bool isSuccess = false;
            string message = string.Empty;
            try
            {
                if (resource == null || resource.data == null)
                {
                        return StatusCode(400, new
                    {
                        isSuccess = false,
                        message = "resource is required"
                    });
                }
                foreach (var student in resource.data)
                {
                    var result = await _studentScoreQuery.UploadStudentScore(resource.subject, student, resource.username);

                    if (!result)
                    {
                        message = $"Failed to upload score for student ID: {student.student_id}";
                        break; // ออกจากลูปเมื่อพบความล้มเหลว
                    }
                    else
                    {
                        isSuccess = true;
                    }
                }
                if (isSuccess)
                {
                    message = $"All student scores uploaded successfully.";
                }

            }
            catch (Exception ex)
            {
                message = ex.Message;
                _webEvent.WriteLogException(resource.username, message.Trim(), ex, pathBase);
            }
            var response = ApiResponse<string>(
                isSuccess: isSuccess,
                messageDescription: message
            );
            return StatusCode(200, response);
        }

        //[AllowAnonymous]
        [HttpPost("SendStudentScore")]
        public async Task<IActionResult> SendStudentScore([FromBody] SendStudentScoreResource resource)
        {
            HttpContext pathBase = HttpContext;
            bool isSuccess = false; // Default success, will change to false if any failure occurs
            string message = string.Empty;
            List<string> sendFailStudentList = new List<string>();
            // ใช้ Dictionary เพื่อเก็บ studentId และ Error Message
            Dictionary<string, string> sendFailStudentDetails = new Dictionary<string, string>();

            try
            {
                if (resource == null || resource.SubjectDetail == null || resource.EmailDetail == null)
                {
                    return StatusCode(400, new
                    {
                        message = "Resource is required"
                    });
                }

                // 1. Loop through each student_id in the list
                foreach (var studentId in resource.student_id)
                {
                    try
                    {
                        // 2. Check and Get email for the current student
                        var email = await _studentScoreQuery.GetEmailStudent(studentId);
                        if (string.IsNullOrWhiteSpace(email))
                        {
                            throw new Exception("Email not found or empty");
                        }
                        // 3. Replace Placeholders in subjectEmail and contentEmail
                        string subjectEmail = await ReplacePlaceholders(resource.EmailDetail.SubjectEmail, studentId, resource.SubjectDetail, resource.username);
                        string contentEmail = await ReplacePlaceholders(resource.EmailDetail.ContentEmail, studentId, resource.SubjectDetail, resource.username);
                        string contentHTML = $@"<pre style='tab-size: 8;font-size: 13px; white-space: pre;'>{contentEmail}</pre>";


                        // 4. Send email
                        bool isSent = _mailService.SendMail(subjectEmail, contentHTML, email, true);

                        // 5. If email is sent successfully, update send_status to success
                        if (isSent)
                        {
                            await _studentScoreQuery.UpdateSendEmail(resource.SubjectDetail, studentId, resource.username, 3);
                        }
                        else
                        {
                            throw new Exception("Email sending failed");
                        }
                    }
                    catch (Exception ex)
                    {
                        sendFailStudentDetails[studentId] = ex.Message;
                        await _studentScoreQuery.UpdateSendEmail(resource.SubjectDetail, studentId, resource.username, 2, ex.Message);
                        //message = ex.Message; // Store the error message
                        //_webEvent.WriteLogException(resource.username, message.Trim(), ex, pathBase); // Log exception
                    }
                }

                // Set success flag if there are no failures
                if (sendFailStudentDetails.Count > 0)
                {
                    var errorDetails = string.Join("; ", sendFailStudentDetails.Select(x => $"{x.Key}: {x.Value}"));
                    message = $"Failed to send email to the following students: {errorDetails}";
                }
                else
                {
                    isSuccess = true;
                    message = "Emails sent successfully";
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                _webEvent.WriteLogException(resource.username, message.Trim(), ex, pathBase);
            }

            // Return response with success status and error messages
            var response = ApiResponse<string>(
                isSuccess: isSuccess,
                messageDescription: message
            );
            return StatusCode(200, response);
        }


        //[AllowAnonymous]
        [HttpPost("UpdateTemplate")]
        public async Task<IActionResult> UpdateTemplateEmail([FromBody] EmailTemplateResource resource)
        {
            HttpContext pathBase = HttpContext;
            bool isSuccess = false;
            string message = string.Empty;
            try
            {
                bool result = result = await _studentScoreQuery.UpdateTemplateEmail(resource);
                if (result)
                {
                    isSuccess = true;
                    message = "Update template success.";
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                _webEvent.WriteLogException(resource.username, message.Trim(), ex, pathBase);
            }
            var response = ApiResponse<string>(
                isSuccess: isSuccess,
                messageDescription: message
            );
            return StatusCode(200, response);
        }

        [HttpPost("CreateTemplate")]
        public async Task<IActionResult> CreateTemplateEmail([FromBody] EmailTemplateResource resource)
        {
            HttpContext pathBase = HttpContext;
            bool isSuccess = false;
            string message = string.Empty;
            try
            {
                bool result = await _studentScoreQuery.CreateTemplateEmail(resource);
                if (result)
                {
                    isSuccess = true;
                    message = "Insert template success.";
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                _webEvent.WriteLogException(resource.username, message.Trim(), ex, pathBase);
            }
            var response = ApiResponse<string>(
                isSuccess: isSuccess,
                messageDescription: message
            );

            return StatusCode(200, response);
        }
        [HttpPost("DeleteTemplate")]
        public async Task<IActionResult> DeleteTemplateEmail([FromBody] EmailTemplateResource resource)
        {
            HttpContext pathBase = HttpContext;
            bool isSuccess = false;
            string message = string.Empty;
            try
            {
                bool result = await _studentScoreQuery.DeleteTemplateEmail(resource);
                if (result)
                {
                    isSuccess = true;
                    message = "Delete template success.";
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                _webEvent.WriteLogException(resource.username, message.Trim(), ex, pathBase);
            }
            var response = ApiResponse<string>(
                isSuccess: isSuccess,
                messageDescription: message
            );

            return StatusCode(200, response);
        }
        [HttpPost("SetDefaultTemplate")]
        public async Task<IActionResult> SetDefaultTemplateEmail([FromBody] EmailTemplateResource resource)
        {
            HttpContext pathBase = HttpContext;
            bool isSuccess = false;
            string message = string.Empty;
            try
            {
                bool result = await _studentScoreQuery.SetDefaultTemplateEmail(resource);
                if (result)
                {
                    isSuccess = true;
                    message = "set Default template success.";
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                _webEvent.WriteLogException(resource.username, message.Trim(), ex, pathBase);
            }
            var response = ApiResponse<string>(
                isSuccess: isSuccess,
                messageDescription: message
            );

            return StatusCode(200, response);
        }

        #endregion controller


        #region function
        private async Task<string> ReplacePlaceholders(string template, string studentId, SubjectDetail subjectDetail, string username)
        {
            foreach (var placeholderKey in GetPlaceholderKeys(template))
            {
                var mapping = _studentScoreQuery.GetPlaceholderMapping(placeholderKey);
                if (mapping != null)
                {
                    var fieldValue = await _studentScoreQuery.GetFieldValue(subjectDetail, studentId, mapping.source_table!, mapping.field_name!, mapping.condition!, username);
                    template = template.Replace(placeholderKey, fieldValue);
                }
            }
            return template;
        }

        private List<string> GetPlaceholderKeys(string template)
        {
            var regex = new Regex(@"\{!(.*?)\}");
            var matches = regex.Matches(template);
            return matches.Cast<Match>().Select(m => m.Value).ToList();
        }
        #endregion function
    }
}
