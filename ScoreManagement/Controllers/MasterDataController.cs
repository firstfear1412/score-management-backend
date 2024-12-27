using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoreManagement.Controllers.Base;
using ScoreManagement.Entity;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;
using ScoreManagement.Model.Table;
using ScoreManagement.Services.Encrypt;
using System.Collections.Generic;
using System.Linq;

namespace ScoreManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MasterDataController : BaseController
    {
        private readonly scoreDB _context;
        private readonly IMasterDataQuery _masterDataQuery;
        public MasterDataController(scoreDB context,IMasterDataQuery masterDataQuery)
        {
            _context = context;
            _masterDataQuery = masterDataQuery;
        }
        [AllowAnonymous]
        [HttpGet("Language")]
        public async Task<IActionResult> GetLanguage(string language)
        {
            bool isSuccess = false;
            string message = string.Empty;
            Dictionary<string, string> translation = new Dictionary<string, string>();
            try
            {
                translation = await _masterDataQuery.GetLanguage(language);
                if(translation.Count > 0)
                {
                    isSuccess = true;
                }
                else
                {
                    message = "Data not found.";
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            var response = ApiResponse(
                isSuccess: isSuccess,
                messageDescription: message,
                objectResponse: translation
            );
            return StatusCode(200, response);
        }

        //[AllowAnonymous]
        [HttpGet("SystemParam")]
        public async Task<IActionResult> Masterdata(string reference)
        {
            List<SystemParam> lst = new List<SystemParam>();
            var message = string.Empty;
            var isSuccess = false;
            try
            {
                List<SystemParam> data = await _masterDataQuery.GetSystemParams(reference);
                if (data.Count > 0)
                {
                    
                    data.ForEach(x =>
                    {
                        SystemParam md = new SystemParam();
                        md.byte_code = x.byte_code;
                        md.byte_desc_th = x.byte_desc_th;
                        md.byte_desc_en = x.byte_desc_en;
                        lst.Add(md);
                    });
                    isSuccess = true;
                    //resource.response.data = lst;
                }
                else
                {
                    message = "data not found";
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            //if (!string.IsNullOrEmpty(ErrorMessage.ErrorText))
            //    resource.response.ErrorMessage.Add(ErrorMessage.ErrorText);
            var response = ApiResponse(
                isSuccess: isSuccess,
                messageDescription: message,
                objectResponse: lst
            );
            return StatusCode(200, response);
        }

        //[AllowAnonymous]
        [HttpGet("EmailPlaceholder")]
        public async Task<IActionResult> GetEmailPlaceholder()
        {
            bool isSuccess = false;
            string message = string.Empty;
            List<EmailPlaceholder> placeholders = new List<EmailPlaceholder>();
            try
            {
                placeholders = await _masterDataQuery.GetEmailPlaceholder();
                if (placeholders.Count > 0)
                {
                    isSuccess = true;
                }
                else
                {
                    message = "Data not found.";
                }
            }
            catch(Exception ex)
            {
                message = ex.Message;
            }
            
            var response = ApiResponse(
                isSuccess: isSuccess,
                messageDescription: message,
                objectResponse: placeholders
            );
            return StatusCode(200, response);
        }

        [AllowAnonymous]
        [HttpGet("EmailTemplate")]
        public async Task<IActionResult> GetEmailTemplate(string username)
        {
            bool isSuccess = false;
            string message = string.Empty;
            EmailTemplateGroup groupedTemplates = new EmailTemplateGroup();
            try
            {
                List<EmailTemplate> templates = await _masterDataQuery.GetEmailTemplate(username);
                if (templates.Count > 0)
                {
                    isSuccess = true;
                }
                else
                {
                    message = "Data not found.";
                }
                Dictionary<string, int?> defaultTemplate = await _masterDataQuery.GetDefaultEmailTemplate(username);

                groupedTemplates = new EmailTemplateGroup
                {
                    PrivateTemplates = templates
                        .Where(x => x.is_private)
                        .Select(x => new TemplateCollection
                        {
                            TemplateId = x.template_id,
                            TemplateName = x.template_name,
                            Detail = new TemplateDetail
                            {
                                Subject = x.subject,
                                Body = x.body.Replace("\\n", "\n").Replace("\\t", "\t")
                            }
                        }).ToList(),
                    BasicTemplates = templates
                        .Where(x => !x.is_private)
                        .Select(x => new TemplateCollection
                        {
                            TemplateId = x.template_id,
                            TemplateName = x.template_name,
                            Detail = new TemplateDetail
                            {
                                Subject = x.subject,
                                Body = x.body.Replace("\\n", "\n").Replace("\\t", "\t")
                            }
                        }).ToList(),
                    DefaultTemplates = defaultTemplate
                };
            }
            catch (Exception ex) {
                message = ex.Message; 
            }
            var response = ApiResponse(
                isSuccess: isSuccess,
                messageDescription: message,
                objectResponse: groupedTemplates
            );
            return StatusCode(200, response);
        }

    }
}
