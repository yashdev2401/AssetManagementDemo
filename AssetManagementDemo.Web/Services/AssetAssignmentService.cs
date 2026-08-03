using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetManagementDemo.Web.DTOs;
using AssetManagementDemo.Web.Helpers;
using AssetManagementDemo.Web.Models;
using AssetManagementDemo.Web.Repositories;
using AssetManagementDemo.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AssetManagementDemo.Web.Services
{
    public class AssetAssignmentService : IAssetAssignmentService
    {
        private readonly IAssetAssignmentRepository _assignmentRepository;
        private readonly IAssetRepository _assetRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<AssetAssignmentService> _logger;

        public AssetAssignmentService(
            IAssetAssignmentRepository assignmentRepository,
            IAssetRepository assetRepository,
            IEmployeeRepository employeeRepository,
            ILogger<AssetAssignmentService> logger)
        {
            _assignmentRepository = assignmentRepository;
            _assetRepository = assetRepository;
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public async Task<PagedResult<AssetAssignment>> GetAssignmentsPagedAsync(
            string? searchTerm, 
            bool? isActive, 
            DateTime? assignedDate,
            string? sortBy,
            bool sortDescending,
            int pageNumber, 
            int pageSize)
        {
            _logger.LogInformation("Querying asset assignments. Search: {SearchTerm}, IsActive: {IsActive}, AssignedDate: {AssignedDate}, SortBy: {SortBy}, SortDesc: {SortDesc}, Page: {PageNumber}, Size: {PageSize}",
                searchTerm, isActive, assignedDate, sortBy, sortDescending, pageNumber, pageSize);

            var query = _assignmentRepository.GetQueryable(tracking: false)
                .Include(aa => aa.Employee)
                .Include(aa => aa.Asset)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(aa => (aa.Employee != null && (aa.Employee.EmployeeName.ToLower().Contains(term) || aa.Employee.EmployeeCode.ToLower().Contains(term))) ||
                                          (aa.Asset != null && (aa.Asset.AssetName.ToLower().Contains(term) || aa.Asset.AssetCode.ToLower().Contains(term))) ||
                                          (aa.Remarks != null && aa.Remarks.ToLower().Contains(term)));
            }

            if (isActive.HasValue)
            {
                query = query.Where(aa => aa.IsActive == isActive.Value);
            }

            if (assignedDate.HasValue)
            {
                var targetDate = assignedDate.Value.Date;
                query = query.Where(aa => aa.AssignedDate.Date == targetDate);
            }

            query = sortBy?.ToLower() switch
            {
                "assigneddate" or "date" => sortDescending ? query.OrderByDescending(aa => aa.AssignedDate) : query.OrderBy(aa => aa.AssignedDate),
                "returndate" => sortDescending ? query.OrderByDescending(aa => aa.ReturnDate) : query.OrderBy(aa => aa.ReturnDate),
                "employeename" or "employee" => sortDescending ? query.OrderByDescending(aa => aa.Employee != null ? aa.Employee.EmployeeName : string.Empty) : query.OrderBy(aa => aa.Employee != null ? aa.Employee.EmployeeName : string.Empty),
                "assetname" or "asset" => sortDescending ? query.OrderByDescending(aa => aa.Asset != null ? aa.Asset.AssetName : string.Empty) : query.OrderBy(aa => aa.Asset != null ? aa.Asset.AssetName : string.Empty),
                _ => sortDescending ? query.OrderByDescending(aa => aa.AssignmentId) : query.OrderByDescending(aa => aa.AssignmentId)
            };

            return await query.ToPagedResultAsync(pageNumber, pageSize, searchTerm, sortBy, sortDescending);
        }

        public async Task<IEnumerable<AssetAssignment>> GetAssignmentsByEmployeeIdAsync(int employeeId)
        {
            return await _assignmentRepository.GetAssignmentsByEmployeeIdAsync(employeeId);
        }

        public async Task<IEnumerable<AssetAssignment>> GetAssignmentsByAssetIdAsync(int assetId)
        {
            return await _assignmentRepository.GetAssignmentsByAssetIdAsync(assetId);
        }

        public async Task<AssetAssignment?> GetAssignmentByIdAsync(int id)
        {
            return await _assignmentRepository.GetWithDetailsByIdAsync(id);
        }

        public async Task<AssetAssignment> AssignAssetAsync(int employeeId, int assetId, DateTime assignedDate, string? remarks)
        {
            _logger.LogInformation("Attempting to assign Asset ID {AssetId} to Employee ID {EmployeeId}", assetId, employeeId);

            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
            {
                _logger.LogWarning("Assignment failed. Employee ID {EmployeeId} not found", employeeId);
                throw new InvalidOperationException($"Employee with ID {employeeId} does not exist.");
            }

            var asset = await _assetRepository.GetByIdAsync(assetId);
            if (asset == null)
            {
                _logger.LogWarning("Assignment failed. Asset ID {AssetId} not found", assetId);
                throw new InvalidOperationException($"Asset with ID {assetId} does not exist.");
            }

            if (asset.Status == "Assigned")
            {
                _logger.LogWarning("Assignment failed. Asset '{AssetCode}' is already assigned", asset.AssetCode);
                throw new InvalidOperationException($"Asset '{asset.AssetCode} - {asset.AssetName}' is already assigned.");
            }

            var activeAssignment = await _assignmentRepository.GetActiveAssignmentByAssetIdAsync(assetId);
            if (activeAssignment != null)
            {
                _logger.LogWarning("Assignment failed. Asset '{AssetCode}' already has active assignment record ID {AssignmentId}", asset.AssetCode, activeAssignment.AssignmentId);
                throw new InvalidOperationException($"Asset '{asset.AssetCode}' already has an active assignment.");
            }

            // Create assignment record
            var assignment = new AssetAssignment
            {
                EmployeeId = employeeId,
                AssetId = assetId,
                AssignedDate = assignedDate,
                Remarks = remarks,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            // Update Asset status
            asset.Status = "Assigned";
            asset.UpdatedDate = DateTime.Now;
            _assetRepository.Update(asset);

            await _assignmentRepository.AddAsync(assignment);
            await _assignmentRepository.SaveChangesAsync();

            _logger.LogInformation("Successfully assigned Asset '{AssetCode}' to Employee '{EmployeeCode}'. Assignment ID: {AssignmentId}", asset.AssetCode, employee.EmployeeCode, assignment.AssignmentId);

            return await _assignmentRepository.GetWithDetailsByIdAsync(assignment.AssignmentId) ?? assignment;
        }

        public async Task<bool> ReturnAssetAsync(int assignmentId, DateTime returnDate, string? remarks)
        {
            _logger.LogInformation("Attempting to return assignment ID {AssignmentId}", assignmentId);

            var assignment = await _assignmentRepository.GetWithDetailsByIdAsync(assignmentId);
            if (assignment == null)
            {
                _logger.LogWarning("Return failed. Assignment ID {AssignmentId} not found", assignmentId);
                return false;
            }

            if (assignment.IsActive == false)
            {
                _logger.LogWarning("Return failed. Assignment ID {AssignmentId} is already marked inactive/returned", assignmentId);
                throw new InvalidOperationException("This asset assignment has already been returned.");
            }

            assignment.ReturnDate = returnDate;
            assignment.IsActive = false;
            if (!string.IsNullOrWhiteSpace(remarks))
            {
                assignment.Remarks = string.IsNullOrWhiteSpace(assignment.Remarks) 
                    ? $"[Return]: {remarks}" 
                    : $"{assignment.Remarks} | [Return]: {remarks}";
            }

            var asset = await _assetRepository.GetByIdAsync(assignment.AssetId);
            if (asset != null)
            {
                asset.Status = "Available";
                asset.UpdatedDate = DateTime.Now;
                _assetRepository.Update(asset);
            }

            _assignmentRepository.Update(assignment);
            await _assignmentRepository.SaveChangesAsync();

            _logger.LogInformation("Successfully returned assignment ID {AssignmentId}. Asset '{AssetCode}' set to Available", assignmentId, asset?.AssetCode);
            return true;
        }

        public AssignmentDto MapToDto(AssetAssignment assignment)
        {
            return new AssignmentDto
            {
                AssignmentId = assignment.AssignmentId,
                EmployeeId = assignment.EmployeeId,
                EmployeeName = assignment.Employee?.EmployeeName,
                EmployeeCode = assignment.Employee?.EmployeeCode,
                AssetId = assignment.AssetId,
                AssetName = assignment.Asset?.AssetName,
                AssetCode = assignment.Asset?.AssetCode,
                AssignedDate = assignment.AssignedDate,
                ReturnDate = assignment.ReturnDate,
                Remarks = assignment.Remarks,
                IsActive = assignment.IsActive,
                CreatedDate = assignment.CreatedDate
            };
        }
    }
}
