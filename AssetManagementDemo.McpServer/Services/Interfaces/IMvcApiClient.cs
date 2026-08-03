using System.Threading;
using System.Threading.Tasks;

using AssetManagementDemo.McpServer.DTOs;

namespace AssetManagementDemo.McpServer.Services.Interfaces
{
    public interface IMvcApiClient
    {
        // Health Check
        Task<bool> CheckHealthAsync(string? correlationId = null, CancellationToken ct = default);

        // Employees API Client Methods
        Task<ApiResponseDto<PagedResultDto<EmployeeDto>>?> GetEmployeesAsync(string? searchTerm, string? department, string? status, string? location, string? sortBy, bool sortDescending, int pageNumber, int pageSize, string? correlationId = null, CancellationToken ct = default);
        Task<ApiResponseDto<EmployeeDto>?> GetEmployeeByIdAsync(int id, string? correlationId = null, CancellationToken ct = default);
        Task<ApiResponseDto<EmployeeDto>?> CreateEmployeeAsync(CreateEmployeeDto dto, string? correlationId = null, CancellationToken ct = default);
        Task<ApiResponseDto<EmployeeDto>?> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto, string? correlationId = null, CancellationToken ct = default);
        Task<ApiResponseDto<bool>?> DeleteEmployeeAsync(int id, string? correlationId = null, CancellationToken ct = default);

        // Assets API Client Methods
        Task<ApiResponseDto<PagedResultDto<AssetDto>>?> GetAssetsAsync(string? searchTerm, string? category, string? brand, string? status, string? sortBy, bool sortDescending, int pageNumber, int pageSize, string? correlationId = null, CancellationToken ct = default);
        Task<ApiResponseDto<AssetDto>?> GetAssetByIdAsync(int id, string? correlationId = null, CancellationToken ct = default);
        Task<ApiResponseDto<AssetDto>?> CreateAssetAsync(CreateAssetDto dto, string? correlationId = null, CancellationToken ct = default);
        Task<ApiResponseDto<AssetDto>?> UpdateAssetAsync(int id, UpdateAssetDto dto, string? correlationId = null, CancellationToken ct = default);
        Task<ApiResponseDto<bool>?> DeleteAssetAsync(int id, string? correlationId = null, CancellationToken ct = default);

        // Assignments API Client Methods
        Task<ApiResponseDto<PagedResultDto<AssignmentDto>>?> GetAssignmentsAsync(string? searchTerm, bool? isActive, string? assignedDate, string? sortBy, bool sortDescending, int pageNumber, int pageSize, string? correlationId = null, CancellationToken ct = default);
        Task<ApiResponseDto<AssignmentDto>?> GetAssignmentByIdAsync(int id, string? correlationId = null, CancellationToken ct = default);
        Task<ApiResponseDto<AssignmentDto>?> AssignAssetAsync(AssignAssetDto dto, string? correlationId = null, CancellationToken ct = default);
        Task<ApiResponseDto<bool>?> ReturnAssetAsync(ReturnAssetDto dto, string? correlationId = null, CancellationToken ct = default);
    }
}
