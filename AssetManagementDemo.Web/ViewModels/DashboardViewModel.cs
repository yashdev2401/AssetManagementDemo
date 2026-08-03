using System.Collections.Generic;
using AssetManagementDemo.Web.DTOs;

namespace AssetManagementDemo.Web.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int TotalAssets { get; set; }
        public int AssignedAssets { get; set; }
        public int AvailableAssets { get; set; }
        public int UnderRepairAssets { get; set; }
        public int RetiredAssets { get; set; }

        public List<AssignmentDto> RecentlyAssignedAssets { get; set; } = new List<AssignmentDto>();
        public List<EmployeeDto> RecentlyAddedEmployees { get; set; } = new List<EmployeeDto>();
    }
}
