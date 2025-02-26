using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ScoreManagement.Interfaces.ExcelScore;
using ScoreManagement.Helpers;
using ScoreManagement.Model.ExcelScore;
using ScoreManagement.Controllers.Base;
using System.Collections.Generic;

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
        public async Task<IActionResult> CreateExcelScore([FromBody] List<ExcelScoreRequest> requests)
        {
            var allData_total = new List<ExcelScoreModel>();
            var allData_other = new List<ExcelScoreModel_Other>();

            foreach (var request in requests)
            {
                if (request.score_type == "คะแนนรวม")
                {
                    var data = await _excel_score.GetScoreReportAsync(request);
                    if (data != null && data.Any())
                    {
                        allData_total.AddRange(data);
                    }
                    else
                    {
                        return NotFound(new { error = $"No data found for request: {request.subject_id}" });
                    }
                }
                else
                {
                    var data = await _excel_score.GetScoreReportAsync_Other(request);
                    if (data != null && data.Any())
                    {
                        allData_other.AddRange(data); 
                    }
                    else
                    {
                        return NotFound(new { error = $"No data found for request: {request.subject_id}" });
                    }
                }
            }

            string base64ExcelTotal = null;
            if (allData_total.Any())
            {
                base64ExcelTotal = ExcelHelper.GenerateExcelBase64(allData_total);
                return Ok(new { file = base64ExcelTotal });
            }

            string base64ExcelOther = null;
            if (allData_other.Any())
            {
                base64ExcelOther = ExcelHelper.GenerateExcelBase64_Other(allData_other); 
                return Ok(new { file = base64ExcelOther });
            }

            return BadRequest(new { error = "No data to generate Excel." });
        }

    }
}