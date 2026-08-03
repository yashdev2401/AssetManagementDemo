using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetManagementDemo.Web.DTOs;
using AssetManagementDemo.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagementDemo.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EmployeesApiController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesApiController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        /// <summary>
        /// Retrieves a paged list of employees with search, department, status, location, sorting, and pagination options.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResultDto<EmployeeDto>>>> GetEmployees(
            [FromQuery] string? searchTerm,
            [FromQuery] string? department,
            [FromQuery] string? status,
            [FromQuery] string? location,
            [FromQuery] string? sortBy,
            [FromQuery] bool sortDescending = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _employeeService.GetEmployeesPagedAsync(searchTerm, department, status, location, sortBy, sortDescending, pageNumber, pageSize);
            
            var pagedDto = new PagedResultDto<EmployeeDto>
            {
                Items = result.Items.Select(e => _employeeService.MapToDto(e)).ToList(),
                TotalItems = result.TotalItems,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages,
                HasPreviousPage = result.HasPreviousPage,
                HasNextPage = result.HasNextPage,
                SortBy = result.SortBy,
                SortDescending = result.SortDescending
            };

            return Ok(ApiResponse<PagedResultDto<EmployeeDto>>.SuccessResponse(pagedDto));
        }

        /// <summary>
        /// Retrieves an employee by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetEmployeeById(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound(ApiResponse<EmployeeDto>.ErrorResponse($"Employee with ID {id} not found."));
            }

            return Ok(ApiResponse<EmployeeDto>.SuccessResponse(_employeeService.MapToDto(employee)));
        }

        /// <summary>
        /// Creates a new employee.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> CreateEmployee([FromBody] CreateEmployeeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<EmployeeDto>.ErrorResponse("Validation failed."));
            }

            var entity = _employeeService.MapToEntity(dto);
            var created = await _employeeService.CreateEmployeeAsync(entity);
            var resultDto = _employeeService.MapToDto(created);

            return CreatedAtAction(nameof(GetEmployeeById), new { id = created.EmployeeId }, 
                ApiResponse<EmployeeDto>.SuccessResponse(resultDto, "Employee created successfully."));
        }

        /// <summary>
        /// Updates an existing employee.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
        {
            var existing = await _employeeService.GetEmployeeByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<EmployeeDto>.ErrorResponse($"Employee with ID {id} not found."));
            }

            existing.Department = !string.IsNullOrWhiteSpace(dto.Department) ? dto.Department : existing.Department;
            existing.Designation = dto.Designation ?? existing.Designation;
            existing.Email = dto.Email ?? existing.Email;
            existing.Phone = dto.Phone ?? existing.Phone;
            existing.Location = dto.Location ?? existing.Location;
            existing.JoiningDate = dto.JoiningDate ?? existing.JoiningDate;
            existing.Status = !string.IsNullOrWhiteSpace(dto.Status) ? dto.Status : existing.Status;

            await _employeeService.UpdateEmployeeAsync(existing);
            return Ok(ApiResponse<EmployeeDto>.SuccessResponse(_employeeService.MapToDto(existing), "Employee updated successfully."));
        }

        /// <summary>
        /// Deletes an employee by ID.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteEmployee(int id)
        {
            var success = await _employeeService.DeleteEmployeeAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse($"Employee with ID {id} not found."));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Employee deleted successfully."));
        }
    }
}
