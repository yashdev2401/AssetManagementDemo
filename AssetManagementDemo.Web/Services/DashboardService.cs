using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetManagementDemo.Web.DTOs;
using AssetManagementDemo.Web.Repositories;
using AssetManagementDemo.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AssetManagementDemo.Web.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IAssetRepository _assetRepository;
        private readonly IAssetAssignmentRepository _assignmentRepository;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IEmployeeRepository employeeRepository, 
            IAssetRepository assetRepository,
            IAssetAssignmentRepository assignmentRepository,
            ILogger<DashboardService> logger)
        {
            _employeeRepository = employeeRepository;
            _assetRepository = assetRepository;
            _assignmentRepository = assignmentRepository;
            _logger = logger;
        }

        public async Task<DashboardViewModel> GetDashboardMetricsAsync()
        {
            _logger.LogInformation("Calculating dashboard metrics and recent enterprise activity");

            var totalEmployees = await _employeeRepository.CountAsync();
            var totalAssets = await _assetRepository.CountAsync();
            var assignedAssets = await _assetRepository.CountAsync(a => a.Status == "Assigned");
            var availableAssets = await _assetRepository.CountAsync(a => a.Status == "Available");
            var underRepairAssets = await _assetRepository.CountAsync(a => a.Status == "Under Repair");
            var retiredAssets = await _assetRepository.CountAsync(a => a.Status == "Retired");

            // Recently assigned assets (top 5)
            var recentAssignmentsEntities = await _assignmentRepository.GetQueryable(tracking: false)
                .Include(aa => aa.Employee)
                .Include(aa => aa.Asset)
                .OrderByDescending(aa => aa.AssignedDate)
                .ThenByDescending(aa => aa.AssignmentId)
                .Take(5)
                .ToListAsync();

            var recentAssignments = recentAssignmentsEntities.Select(aa => new AssignmentDto
            {
                AssignmentId = aa.AssignmentId,
                EmployeeId = aa.EmployeeId,
                EmployeeName = aa.Employee?.EmployeeName,
                EmployeeCode = aa.Employee?.EmployeeCode,
                AssetId = aa.AssetId,
                AssetName = aa.Asset?.AssetName,
                AssetCode = aa.Asset?.AssetCode,
                AssignedDate = aa.AssignedDate,
                ReturnDate = aa.ReturnDate,
                Remarks = aa.Remarks,
                IsActive = aa.IsActive,
                CreatedDate = aa.CreatedDate
            }).ToList();

            // Recently added employees (top 5)
            var recentEmployeesEntities = await _employeeRepository.GetQueryable(tracking: false)
                .OrderByDescending(e => e.CreatedDate)
                .ThenByDescending(e => e.EmployeeId)
                .Take(5)
                .ToListAsync();

            var recentEmployees = recentEmployeesEntities.Select(e => new EmployeeDto
            {
                EmployeeId = e.EmployeeId,
                EmployeeCode = e.EmployeeCode,
                EmployeeName = e.EmployeeName,
                Department = e.Department,
                Designation = e.Designation,
                Email = e.Email,
                Phone = e.Phone,
                Location = e.Location,
                JoiningDate = e.JoiningDate,
                Status = e.Status,
                CreatedDate = e.CreatedDate,
                UpdatedDate = e.UpdatedDate
            }).ToList();

            return new DashboardViewModel
            {
                TotalEmployees = totalEmployees,
                TotalAssets = totalAssets,
                AssignedAssets = assignedAssets,
                AvailableAssets = availableAssets,
                UnderRepairAssets = underRepairAssets,
                RetiredAssets = retiredAssets,
                RecentlyAssignedAssets = recentAssignments,
                RecentlyAddedEmployees = recentEmployees
            };
        }
    }
}
