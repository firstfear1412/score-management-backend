using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
//using NPOI.OpenXmlFormats;
//using NPOI.SS.Formula.Functions;
//using Org.BouncyCastle.Asn1.Cmp;
using ScoreManagement.Controllers.Base;
using ScoreManagement.Entity;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;
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
        public async Task<IActionResult> UploadStudentScore([FromBody] SubjectScoreResource resource)
        {
            return View(resource);
        }

        [AllowAnonymous]
        [HttpPost("SendStudentScore")]
        public async Task<IActionResult> SendStudentScore([FromBody] SendStudentScoreResource resource)
        {
            try
            {
                if(resource == null || resource.SubjectDetail == null || resource.EmailDetail == null) {
                    return StatusCode(400, new
                    {
                        isSuccess = false,
                        message = "resource is required"
                    });
                }
                // 1. แทนที่ Placeholder ใน subjectEmail
                string subjectEmail = await ReplacePlaceholders(resource.EmailDetail.SubjectEmail, resource.SubjectDetail, resource.username);

                // 2. แทนที่ Placeholder ใน contentEmail
                string contentEmail = await ReplacePlaceholders(resource.EmailDetail.ContentEmail, resource.SubjectDetail, resource.username);

                string contentHTML = $@"<pre style='tab-size: 6;font-size: 13px; white-space: pre;'>{contentEmail}</pre>";

                _mailService.SendMail(subjectEmail, contentHTML, "pamornpon.t@live.ku.th", true);

                return Ok(new
                {
                    SubjectEmail = subjectEmail,
                    ContentEmail = contentEmail
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        #endregion controller


        #region function
        private async Task<string> ReplacePlaceholders(string template, SubjectDetail subjectDetail, string username)
        {
            foreach (var placeholderKey in GetPlaceholderKeys(template))
            {
                var mapping = _studentScoreQuery.GetPlaceholderMapping(placeholderKey);
                if (mapping != null)
                {
                    var fieldValue = await _studentScoreQuery.GetFieldValue(subjectDetail, mapping.source_table!, mapping.field_name!, mapping.condition!, username);
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
