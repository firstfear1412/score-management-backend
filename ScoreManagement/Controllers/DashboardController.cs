using Microsoft.AspNetCore.Mvc;
using ScoreManagement.Interfaces.Dashboard;
using ScoreManagement.Model.ScoreAnnoucement;
using ScoreManagement.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using ScoreManagement.Model;
using ScoreManagement.Entity;
using Microsoft.EntityFrameworkCore;
using ScoreManagement.Interfaces;
using ScoreManagement.Model.Table;
using ScoreManagement.Query;
using Microsoft.Data.SqlClient;
using ScoreManagement.Model.ExcelScore;
using ScoreManagement.Interfaces.ExcelScore;

namespace ScoreManagement.Controllers
{
    [Authorize]
    //[AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : BaseController
    {
        private readonly scoreDB _context;
        private readonly IDashboardQuery _dashboardQuery;
        private readonly IMasterDataQuery _masterDataQuery;

        public DashboardController(IDashboardQuery dashboardQuery, scoreDB context, IMasterDataQuery masterDataQuery)
        {
            _context = context;
            _dashboardQuery = dashboardQuery;
            _masterDataQuery = masterDataQuery;
        }

        [HttpPost("GetDashboardStats")]
        public async Task<IActionResult> GetDashboardStats([FromBody] ScoreAnnoucementDashboard resource)
        {
            try
            {
                var stats = await _dashboardQuery.GetDashboardStatistics(resource);

                var response = ApiResponse(
                    isSuccess: stats != null,
                    messageDescription: stats == null ? "No data found" : "",
                    objectResponse: stats
                );

                return StatusCode(200, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("GetSubjectDashboard")]
        public async Task<IActionResult> GetSubjectDashboard([FromBody] SubjectRequest request)
        {
            try
            {
                var subjects = await _dashboardQuery.GetSubjectDashboard(request.teacher_code);
                return Ok(subjects);
            }
            catch (Exception ex)
            {

                return StatusCode(500, "An internal error occurred.");
            }
        }
        [HttpPost("GetDashboardTable")]
        public async Task<IActionResult> GetDashboardTable([FromBody] ExcelScoreRequest request)
        {
            try
            {
                var data = await _dashboardQuery.GetScoreReportAsync_Other(request);

                if(data == null || data.Count == 0)
                {
                    return NotFound("No data found");
                }
                return Ok(data);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}