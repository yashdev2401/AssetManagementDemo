using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssetManagementDemo.Web.DTOs;
using AssetManagementDemo.Web.Models;
using AssetManagementDemo.Web.ViewModels;

namespace AssetManagementDemo.Web.Services
{
    public interface IAssetAssignmentService
    {
        Task<PagedResult<AssetAssignment>> GetAssignmentsPagedAsync(
            string? searchTerm, 
            bool? isActive, 
            DateTime? assignedDate,
            string? sortBy,
            bool sortDescending,
            int pageNumber, 
            int pageSize);
        Task<IEnumerable<AssetAssignment>> GetAssignmentsByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<AssetAssignment>> GetAssignmentsByAssetIdAsync(int assetId);
        Task<AssetAssignment?> GetAssignmentByIdAsync(int id);
        Task<AssetAssignment> AssignAssetAsync(int employeeId, int assetId, DateTime assignedDate, string? remarks);
        Task<bool> ReturnAssetAsync(int assignmentId, DateTime returnDate, string? remarks);
        AssignmentDto MapToDto(AssetAssignment assignment);
    }
}
