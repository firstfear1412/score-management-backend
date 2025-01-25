using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ScoreManagement.Interfaces.ExcelScore;
using ScoreManagement.Helpers;
using ScoreManagement.Model.ExcelScore;
using ScoreManagement.Controllers.Base;

namespace ScoreManagement.Controllers
{
    [Authorize]
    //[AllowAnonymous]
    [ApiController]
    [Route("api/ExcelCreate")]
    public class ExcelReportController : BaseController
    {
        private readonly IExcelScore _excel_score;

        public ExcelReportController(IExcelScore excel_score)
        {
            _excel_score = excel_score;
        }

        [HttpPost("CreateExcelScore")]
        public async Task<IActionResult> CreateExcelScore([FromBody] ExcelScoreRequest request)
        {
            var data = await _excel_score.GetScoreReportAsync(request);
            if (data == null || !data.Any())
            {
                return NotFound("No data found.");
            }

            string base64Excel = ExcelHelper.GenerateExcelBase64(data);
            return Ok(new { file = base64Excel });
        }
    }
}