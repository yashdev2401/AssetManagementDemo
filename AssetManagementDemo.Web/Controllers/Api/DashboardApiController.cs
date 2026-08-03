using System.Threading.Tasks;
using AssetManagementDemo.Web.DTOs;
using AssetManagementDemo.Web.Services;
using AssetManagementDemo.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagementDemo.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DashboardApiController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardApiController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Gets dashboard summary metrics.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<DashboardViewModel>>> GetDashboardMetrics()
        {
            var metrics = await _dashboardService.GetDashboardMetricsAsync();
            return Ok(ApiResponse<DashboardViewModel>.SuccessResponse(metrics));
        }
    }
}
