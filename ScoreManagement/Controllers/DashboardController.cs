using Microsoft.AspNetCore.Mvc;
using ScoreManagement.Interfaces.Dashboard;
using ScoreManagement.Model.ScoreAnnoucement;
using ScoreManagement.Controllers.Base;
using Microsoft.AspNetCore.Authorization;

namespace ScoreManagement.Controllers
{
    //[Authorize]
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : BaseController
    {
        private readonly IDashboardQuery _dashboardQuery;

        public DashboardController(IDashboardQuery dashboardQuery)
        {
            _dashboardQuery = dashboardQuery;
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
    }
}