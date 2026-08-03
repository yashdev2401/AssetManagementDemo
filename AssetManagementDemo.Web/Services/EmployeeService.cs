using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetManagementDemo.Web.DTOs;
using AssetManagementDemo.Web.Helpers;
using AssetManagementDemo.Web.Models;
using AssetManagementDemo.Web.Repositories;
using AssetManagementDemo.Web.ViewModels;
using Microsoft.Extensions.Logging;

namespace AssetManagementDemo.Web.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(IEmployeeRepository employeeRepository, ILogger<EmployeeService> logger)
        {
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public async Task<PagedResult<Employee>> GetEmployeesPagedAsync(
            string? searchTerm, 
            string? department, 
            string? status, 
            string? location,
            string? sortBy,
            bool sortDescending,
            int pageNumber, 
            int pageSize)
        {
            _logger.LogInformation("Querying employees. Search: {SearchTerm}, Dept: {Department}, Status: {Status}, Location: {Location}, SortBy: {SortBy}, SortDesc: {SortDesc}, Page: {PageNumber}, Size: {PageSize}",
                searchTerm, department, status, location, sortBy, sortDescending, pageNumber, pageSize);

            var query = _employeeRepository.GetQueryable(tracking: false);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(e => e.EmployeeCode.ToLower().Contains(term) ||
                                         e.EmployeeName.ToLower().Contains(term) ||
                                         e.Department.ToLower().Contains(term) ||
                                         (e.Designation != null && e.Designation.ToLower().Contains(term)) ||
                                         (e.Email != null && e.Email.ToLower().Contains(term)) ||
                                         (e.Location != null && e.Location.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(department))
            {
                query = query.Where(e => e.Department == department);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(e => e.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(e => e.Location == location);
            }

            query = sortBy?.ToLower() switch
            {
                "employeecode" or "code" => sortDescending ? query.OrderByDescending(e => e.EmployeeCode) : query.OrderBy(e => e.EmployeeCode),
                "employeename" or "name" => sortDescending ? query.OrderByDescending(e => e.EmployeeName) : query.OrderBy(e => e.EmployeeName),
                "department" => sortDescending ? query.OrderByDescending(e => e.Department) : query.OrderBy(e => e.Department),
                "designation" => sortDescending ? query.OrderByDescending(e => e.Designation) : query.OrderBy(e => e.Designation),
                "joiningdate" => sortDescending ? query.OrderByDescending(e => e.JoiningDate) : query.OrderBy(e => e.JoiningDate),
                "status" => sortDescending ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
                _ => sortDescending ? query.OrderByDescending(e => e.EmployeeId) : query.OrderByDescending(e => e.EmployeeId)
            };

            return await query.ToPagedResultAsync(pageNumber, pageSize, searchTerm, sortBy, sortDescending);
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _employeeRepository.GetAllAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _employeeRepository.GetByIdAsync(id);
        }

        public async Task<Employee?> GetEmployeeByCodeAsync(string code)
        {
            return await _employeeRepository.GetByCodeAsync(code);
        }

        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            _logger.LogInformation("Creating employee with Code: {EmployeeCode}", employee.EmployeeCode);

            if (!await _employeeRepository.IsCodeUniqueAsync(employee.EmployeeCode))
            {
                _logger.LogWarning("Employee creation failed. Code '{EmployeeCode}' already exists", employee.EmployeeCode);
                throw new InvalidOperationException($"Employee Code '{employee.EmployeeCode}' is already in use.");
            }

            employee.CreatedDate = DateTime.Now;
            await _employeeRepository.AddAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            _logger.LogInformation("Successfully created employee ID {EmployeeId} ({EmployeeName})", employee.EmployeeId, employee.EmployeeName);
            return employee;
        }

        public async Task<bool> UpdateEmployeeAsync(Employee employee)
        {
            _logger.LogInformation("Updating employee ID {EmployeeId}", employee.EmployeeId);

            var existing = await _employeeRepository.GetByIdAsync(employee.EmployeeId);
            if (existing == null)
            {
                _logger.LogWarning("Employee update failed. ID {EmployeeId} not found", employee.EmployeeId);
                return false;
            }

            if (!await _employeeRepository.IsCodeUniqueAsync(employee.EmployeeCode, employee.EmployeeId))
            {
                _logger.LogWarning("Employee update failed. Code '{EmployeeCode}' already exists for another employee", employee.EmployeeCode);
                throw new InvalidOperationException($"Employee Code '{employee.EmployeeCode}' is already in use.");
            }

            existing.EmployeeCode = employee.EmployeeCode;
            existing.EmployeeName = employee.EmployeeName;
            existing.Department = employee.Department;
            existing.Designation = employee.Designation;
            existing.Email = employee.Email;
            existing.Phone = employee.Phone;
            existing.Location = employee.Location;
            existing.JoiningDate = employee.JoiningDate;
            existing.Status = employee.Status;
            existing.UpdatedDate = DateTime.Now;

            _employeeRepository.Update(existing);
            await _employeeRepository.SaveChangesAsync();

            _logger.LogInformation("Successfully updated employee ID {EmployeeId}", employee.EmployeeId);
            return true;
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            _logger.LogInformation("Deleting employee ID {EmployeeId}", id);

            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                _logger.LogWarning("Employee deletion failed. ID {EmployeeId} not found", id);
                return false;
            }

            _employeeRepository.Remove(employee);
            await _employeeRepository.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted employee ID {EmployeeId}", id);
            return true;
        }

        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
        {
            return await _employeeRepository.IsCodeUniqueAsync(code, excludeId);
        }

        public EmployeeDto MapToDto(Employee employee)
        {
            return new EmployeeDto
            {
                EmployeeId = employee.EmployeeId,
                EmployeeCode = employee.EmployeeCode,
                EmployeeName = employee.EmployeeName,
                Department = employee.Department,
                Designation = employee.Designation,
                Email = employee.Email,
                Phone = employee.Phone,
                Location = employee.Location,
                JoiningDate = employee.JoiningDate,
                Status = employee.Status,
                CreatedDate = employee.CreatedDate,
                UpdatedDate = employee.UpdatedDate
            };
        }

        public Employee MapToEntity(CreateEmployeeDto dto)
        {
            return new Employee
            {
                EmployeeCode = dto.EmployeeCode,
                EmployeeName = dto.EmployeeName,
                Department = dto.Department,
                Designation = dto.Designation,
                Email = dto.Email,
                Phone = dto.Phone,
                Location = dto.Location,
                JoiningDate = dto.JoiningDate,
                Status = dto.Status ?? "Active"
            };
        }
    }
}
