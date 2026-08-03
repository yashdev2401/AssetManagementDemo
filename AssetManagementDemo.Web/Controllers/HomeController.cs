using System.Diagnostics;
using System.Threading.Tasks;
using AssetManagementDemo.Web.Services;
using AssetManagementDemo.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagementDemo.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public HomeController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var metrics = await _dashboardService.GetDashboardMetricsAsync();
            return View(metrics);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
