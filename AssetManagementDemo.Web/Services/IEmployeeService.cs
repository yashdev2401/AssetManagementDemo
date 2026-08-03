using System.Collections.Generic;
using System.Threading.Tasks;
using AssetManagementDemo.Web.DTOs;
using AssetManagementDemo.Web.Models;
using AssetManagementDemo.Web.ViewModels;

namespace AssetManagementDemo.Web.Services
{
    public interface IEmployeeService
    {
        Task<PagedResult<Employee>> GetEmployeesPagedAsync(
            string? searchTerm, 
            string? department, 
            string? status, 
            string? location,
            string? sortBy,
            bool sortDescending,
            int pageNumber, 
            int pageSize);
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task<Employee?> GetEmployeeByCodeAsync(string code);
        Task<Employee> CreateEmployeeAsync(Employee employee);
        Task<bool> UpdateEmployeeAsync(Employee employee);
        Task<bool> DeleteEmployeeAsync(int id);
        Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
        EmployeeDto MapToDto(Employee employee);
        Employee MapToEntity(CreateEmployeeDto dto);
    }
}
