using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using ScoreManagement.Controllers.Base;
using ScoreManagement.Entity;
using ScoreManagement.Hubs;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;
using ScoreManagement.Services;
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
        private readonly IHubContext<NotificationHub> _notifyHub;
        private readonly IUtilityService _utilityService;
        public StudentScoreController(scoreDB context, IEncryptService encryptService, IConfiguration configuration, IStudentScoreQuery studentScoreQuery, IMailService mailService, IHubContext<NotificationHub> notifyHub, IUtilityService utilityService)
        {
            _context = context;
            _encryptService = encryptService;
            _configuration = configuration;
            _studentScoreQuery = studentScoreQuery;
            _mailService = (MailService)mailService;
            _notifyHub = notifyHub;
            _utilityService = utilityService;
        }

        #region controller
        [AllowAnonymous]
        [HttpPost("UploadScore")]
        public async Task<IActionResult> UploadStudentScore([FromBody] UploadScoreResource resource)
        {
            HttpContext pathBase = HttpContext;
            bool isSuccess = false;
            string message = string.Empty;
            string messageKey = string.Empty;
            var parameter = new Dictionary<string, string>();
            //List<string> failedStudentIds = new List<string>();
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

                var (flg, failedStudentIds) = await _studentScoreQuery.UploadStudentScore(resource, resource.username);


                if (flg == true && failedStudentIds.Count == 0)
                {
                    isSuccess = flg;
                    message = "All student scores uploaded successfully.";
                }
                else
                {
                    isSuccess = false;
                    string failedIds = string.Join(", ", failedStudentIds);
                    message = $"Failed to upload scores for the following students: {failedIds}";
                    messageKey = "uploadScore_error_uploadSomeStudentFailed"; // มีข้อผิดพลาดในการอัปโหลดคะแนนสำหรับนักเรียนดังต่อไปนี้:
                    parameter = new Dictionary<string, string>
                    {
                        { "studentId_List", failedIds }
                    };
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                _webEvent.WriteLogException(resource.username, message.Trim(), ex, pathBase);
            }
            var response = ApiResponse<Dictionary<string, string>>(
                isSuccess: isSuccess,
                messageKey: messageKey,
                messageDescription: message,
                parameter: parameter
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
            int successCount = 0; // เก็บจำนวนการส่งสำเร็จ
            int failCount = 0;    // เก็บจำนวนการส่งล้มเหลว

            try
            {
                if (resource == null || resource.SubjectDetail == null || resource.EmailDetail == null)
                {
                    return StatusCode(400, new
                    {
                        isSuccess = isSuccess,
                        message = "Resource is required"
                    });
                }

                // 1. Loop through each student_id in the list
                foreach (var studentId in resource.student_id)
                {
                    try
                    {
                        // 2. Check and Get email for the current student
                        string email = await _studentScoreQuery.GetEmailStudent(studentId);
                        if (string.IsNullOrWhiteSpace(email))
                        {
                            throw new Exception("Email not found or empty");
                        }
                        if (_utilityService.IsValidEmail(email))
                        {
                            throw new Exception("Email format is invalid");
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
                            successCount++;
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
                        failCount++;
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

                var data = new
                {
                    subject_id = resource.SubjectDetail.subject_id,
                    total = resource.student_id.Count,
                    success = successCount,
                    fail = failCount,
                };
                var notifyResource = new NotificationResource
                {
                    templateId = 1,
                    data = JsonConvert.SerializeObject(data),
                    username = resource.username,
                };

                int notifyId = await _studentScoreQuery.InsertNotification(notifyResource);
                if (notifyId != 0)
                {
                    var notifyResponse = await _studentScoreQuery.GetLatestNotification(notifyId);
                    await _notifyHub.Clients.Group(resource.username).SendAsync("ReceiveNotification", JsonConvert.SerializeObject(notifyResponse));
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
