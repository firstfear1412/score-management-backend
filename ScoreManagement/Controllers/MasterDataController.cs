using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoreManagement.Controllers.Base;
using ScoreManagement.Entity;
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
        public MasterDataController(scoreDB context)
        {
            _context = context;
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

            var translation = await _context.Languages
                .Where(l => l.language_code == language)
                .ToDictionaryAsync(l => l.message_key, l => l.message_content);

            var response = ApiResponse(
                isSuccess: isSuccess,
                messageDescription: message,
                objectResponse: translation
            );
            return StatusCode(200, response);
        }

        //public async Task<IActionResult> GetLanguage()
        //{
        //    var translation = await _context.Languages.ToListAsync();

        //    var result = new
        //    {
        //        en = translation.ToDictionary(t => t.message_key!, t => t.message_en),
        //        th = translation.ToDictionary(t => t.message_key!, t => t.message_th),
        //    };
        //    return StatusCode(200, result);
        //}

        //[AllowAnonymous]
        [HttpGet("SystemParam")]
        public async Task<IActionResult> Masterdata(string reference)
        {
            List<SystemParam> lst = new List<SystemParam>();
            var message = string.Empty;
            var isSuccess = false;
            try
            {
                var data = await _context.SystemParams.Where(x => x.byte_reference!.Equals(reference)
                            ).ToListAsync();
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
            return StatusCode(200, new
            {
                isSuccess = isSuccess,
                message = message,
                data = lst
            });
        }

    }
}
