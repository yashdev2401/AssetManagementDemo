using System.Threading.Tasks;
using AssetManagementDemo.Web.ViewModels;

namespace AssetManagementDemo.Web.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardMetricsAsync();
    }
}
