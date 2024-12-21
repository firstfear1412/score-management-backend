using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoreManagement.Controllers.Base;
using ScoreManagement.Entity;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;
using ScoreManagement.Model.Table;
using ScoreManagement.Services.Encrypt;

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
            if (string.IsNullOrEmpty(language))
            {
                message = "Language parameter is required.";
            }

            var translation = await _masterDataQuery.GetLanguage(language);
            if(translation.Count > 0)
            {
                isSuccess = true;
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
                var data = await _masterDataQuery.GetSystemParams(reference);
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

        [AllowAnonymous]
        [HttpGet("EmailPlaceholder")]
        public async Task<IActionResult> GetEmailPlaceholder()
        {
            bool isSuccess = false;
            string message = string.Empty;
            var placeholders = await _masterDataQuery.GetEmailPlaceholder();
            if (placeholders.Count > 0)
            {
                isSuccess = true;
            }
            var response = ApiResponse(
                isSuccess: isSuccess,
                messageDescription: message,
                objectResponse: placeholders
            );
            return StatusCode(200, response);
        }

        //[AllowAnonymous]
        //[HttpGet("EmailTemplate")]
        //public async Task<IActionResult> GetEmailTemplate()
        //{
        //    bool isSuccess = false;
        //    string message = string.Empty;
        //    var placeholders = await _context.EmailPlaceholders.Where(x => x.active_status == "active").ToListAsync();
        //    if (placeholders.Count > 0)
        //    {
        //        isSuccess = true;
        //    }
        //    var response = ApiResponse(
        //        isSuccess: isSuccess,
        //        messageDescription: message,
        //        objectResponse: placeholders
        //    );
        //    return StatusCode(200, response);
        //}

    }
}
