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

            // Check if the requests are empty
            if (requests == null || !requests.Any())
            {
                return BadRequest(new { error = "Request data is empty." });
            }

            // Get the username from the first request (assuming all requests have the same username)
            var username = requests.FirstOrDefault()?.username;

            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { error = "Username not found in the request." });
            }

            try
            {
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
                            _webEvent.WriteLogInfo(username, $"No data found for request: {request.subject_id}", HttpContext);
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
                            _webEvent.WriteLogInfo(username, $"No data found for request: {request.subject_id}", HttpContext);
                            return NotFound(new { error = $"No data found for request: {request.subject_id}" });
                        }
                    }
                }

                string base64ExcelTotal = null;
                if (allData_total.Any())
                {
                    base64ExcelTotal = ExcelHelper.GenerateExcelBase64(allData_total);
                    // Log the success event
                    _webEvent.WriteLogInfo(username, "Generated Excel for Total Scores", HttpContext);
                    return Ok(new { file = base64ExcelTotal });
                }

                string base64ExcelOther = null;
                if (allData_other.Any())
                {
                    base64ExcelOther = ExcelHelper.GenerateExcelBase64_Other(allData_other);
                    // Log the success event
                    _webEvent.WriteLogInfo(username, "Generated Excel for Other Scores", HttpContext);
                    return Ok(new { file = base64ExcelOther });
                }

                // If no data to generate
                _webEvent.WriteLogInfo(username, "No data to generate Excel.", HttpContext);
                return BadRequest(new { error = "No data to generate Excel." });
            }
            catch (Exception ex)
            {
                // Log the exception if an error occurs
                _webEvent.WriteLogException(username, "Error while generating Excel", ex, HttpContext);
                return StatusCode(500, new { error = "An error occurred while generating Excel." });
            }
        }
    }
}