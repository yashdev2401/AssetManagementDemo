using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using AssetManagementDemo.McpServer.DTOs;
using AssetManagementDemo.McpServer.Helpers;
using AssetManagementDemo.McpServer.Models;
using AssetManagementDemo.McpServer.Services.Interfaces;

using Microsoft.Extensions.Logging;

namespace AssetManagementDemo.McpServer.Tools
{
    public class EmployeeTools : IMcpTool
    {
        private readonly IMvcApiClient _apiClient;
        private readonly ILogger<EmployeeTools> _logger;

        public EmployeeTools(IMvcApiClient apiClient, ILogger<EmployeeTools> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public IEnumerable<McpToolDefinition> GetToolDefinitions()
        {
            return new[]
            {
                new McpToolDefinition
                {
                    Name = "get_employees",
                    Description = "Retrieves a paged list of employees with optional filtering, sorting, and pagination.",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            searchTerm = new { type = "string", description = "Search by code, name, department, designation, email..." },
                            department = new { type = "string", description = "Filter by department (e.g. IT, HR, Finance)" },
                            status = new { type = "string", description = "Filter by status (Active, Inactive)" },
                            location = new { type = "string", description = "Filter by office location" },
                            sortBy = new { type = "string", description = "Column to sort by (EmployeeCode, EmployeeName, Department, Designation, JoiningDate, Status)" },
                            sortDescending = new { type = "boolean", description = "Sort descending flag" },
                            pageNumber = new { type = "integer", description = "Page index (default 1)" },
                            pageSize = new { type = "integer", description = "Items per page (10, 20, 50, 100)" }
                        }
                    }
                },
                new McpToolDefinition
                {
                    Name = "get_employee_by_id",
                    Description = "Retrieves a single employee record by Employee ID.",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            id = new { type = "integer", description = "Target Employee ID" }
                        },
                        required = new[] { "id" }
                    }
                },
                new McpToolDefinition
                {
                    Name = "search_employees",
                    Description = "Searches for employees matching a search query string across code, name, department, designation, and email.",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            query = new { type = "string", description = "Search query string" }
                        },
                        required = new[] { "query" }
                    }
                },
                new McpToolDefinition
                {
                    Name = "create_employee",
                    Description = "Creates a new employee record in the system.",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            employeeCode = new { type = "string", description = "Unique employee code (e.g. EMP001)" },
                            employeeName = new { type = "string", description = "Full name of employee" },
                            department = new { type = "string", description = "Department" },
                            designation = new { type = "string", description = "Designation or title" },
                            email = new { type = "string", description = "Email address" },
                            phone = new { type = "string", description = "Phone number" },
                            location = new { type = "string", description = "Work location" },
                            joiningDate = new { type = "string", description = "Joining Date (YYYY-MM-DD)" },
                            status = new { type = "string", description = "Status (Active/Inactive)" }
                        },
                        required = new[] { "employeeCode", "employeeName", "department" }
                    }
                },
                new McpToolDefinition
                {
                    Name = "update_employee",
                    Description = "Updates an existing employee record by ID.",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            id = new { type = "integer", description = "Employee ID to update" },
                            employeeCode = new { type = "string", description = "Employee code" },
                            employeeName = new { type = "string", description = "Full name" },
                            department = new { type = "string", description = "Department" },
                            designation = new { type = "string", description = "Designation" },
                            email = new { type = "string", description = "Email" },
                            phone = new { type = "string", description = "Phone" },
                            location = new { type = "string", description = "Location" },
                            joiningDate = new { type = "string", description = "Joining Date (YYYY-MM-DD)" },
                            status = new { type = "string", description = "Status" }
                        },
                        required = new[] { "id" }
                    }
                },
                new McpToolDefinition
                {
                    Name = "delete_employee",
                    Description = "Deletes an employee record by ID.",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            id = new { type = "integer", description = "Employee ID to delete" }
                        },
                        required = new[] { "id" }
                    }
                }
            };
        }

        public async Task<McpToolResult> ExecuteToolAsync(string toolName, JsonElement? args, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing EmployeeTool: {ToolName}", toolName);

            try
            {
                return toolName.ToLower() switch
                {
                    "get_employees" => await ExecuteGetEmployeesAsync(args, ct),
                    "get_employee_by_id" => await ExecuteGetEmployeeByIdAsync(args, ct),
                    "search_employees" => await ExecuteSearchEmployeesAsync(args, ct),
                    "create_employee" => await ExecuteCreateEmployeeAsync(args, ct),
                    "update_employee" => await ExecuteUpdateEmployeeAsync(args, ct),
                    "delete_employee" => await ExecuteDeleteEmployeeAsync(args, ct),
                    _ => McpToolResult.Error($"Unknown employee tool '{toolName}'")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing EmployeeTool {ToolName}", toolName);
                return McpToolResult.Error($"Execution error: {ex.Message}");
            }
        }

        private async Task<McpToolResult> ExecuteGetEmployeesAsync(JsonElement? args, CancellationToken ct)
        {
            var searchTerm = GetStringProp(args, "searchTerm");
            var department = GetStringProp(args, "department");
            var status = GetStringProp(args, "status");
            var location = GetStringProp(args, "location");
            var sortBy = GetStringProp(args, "sortBy");
            var sortDesc = GetBoolProp(args, "sortDescending") ?? false;
            var pageNumber = GetIntProp(args, "pageNumber") ?? 1;
            var pageSize = GetIntProp(args, "pageSize") ?? 10;

            var response = await _apiClient.GetEmployeesAsync(searchTerm, department, status, location, sortBy, sortDesc, pageNumber, pageSize, ct: ct);
            return McpToolResult.Success(JsonSerializationHelper.Serialize(response));
        }

        private async Task<McpToolResult> ExecuteGetEmployeeByIdAsync(JsonElement? args, CancellationToken ct)
        {
            var id = GetIntProp(args, "id");
            if (!id.HasValue) return McpToolResult.Error("Parameter 'id' is required");

            var response = await _apiClient.GetEmployeeByIdAsync(id.Value, ct: ct);
            return McpToolResult.Success(JsonSerializationHelper.Serialize(response));
        }

        private async Task<McpToolResult> ExecuteSearchEmployeesAsync(JsonElement? args, CancellationToken ct)
        {
            var query = GetStringProp(args, "query") ?? GetStringProp(args, "searchTerm");
            if (string.IsNullOrWhiteSpace(query)) return McpToolResult.Error("Parameter 'query' is required");

            var response = await _apiClient.GetEmployeesAsync(query, null, null, null, null, false, 1, 20, ct: ct);
            return McpToolResult.Success(JsonSerializationHelper.Serialize(response));
        }

        private async Task<McpToolResult> ExecuteCreateEmployeeAsync(JsonElement? args, CancellationToken ct)
        {
            var dto = new CreateEmployeeDto
            {
                EmployeeCode = GetStringProp(args, "employeeCode") ?? string.Empty,
                EmployeeName = GetStringProp(args, "employeeName") ?? string.Empty,
                Department = GetStringProp(args, "department") ?? string.Empty,
                Designation = GetStringProp(args, "designation"),
                Email = GetStringProp(args, "email"),
                Phone = GetStringProp(args, "phone"),
                Location = GetStringProp(args, "location"),
                JoiningDate = GetDateTimeProp(args, "joiningDate"),
                Status = GetStringProp(args, "status") ?? "Active"
            };

            if (string.IsNullOrWhiteSpace(dto.EmployeeCode) || string.IsNullOrWhiteSpace(dto.EmployeeName) || string.IsNullOrWhiteSpace(dto.Department))
            {
                return McpToolResult.Error("Parameters 'employeeCode', 'employeeName', and 'department' are required.");
            }

            var response = await _apiClient.CreateEmployeeAsync(dto, ct: ct);
            return McpToolResult.Success(JsonSerializationHelper.Serialize(response));
        }

        private async Task<McpToolResult> ExecuteUpdateEmployeeAsync(JsonElement? args, CancellationToken ct)
        {
            var id = GetIntProp(args, "id");
            if (!id.HasValue) return McpToolResult.Error("Parameter 'id' is required");

            var dto = new UpdateEmployeeDto
            {
                EmployeeCode = GetStringProp(args, "employeeCode"),
                EmployeeName = GetStringProp(args, "employeeName"),
                Department = GetStringProp(args, "department"),
                Designation = GetStringProp(args, "designation"),
                Email = GetStringProp(args, "email"),
                Phone = GetStringProp(args, "phone"),
                Location = GetStringProp(args, "location"),
                JoiningDate = GetDateTimeProp(args, "joiningDate"),
                Status = GetStringProp(args, "status")
            };

            var response = await _apiClient.UpdateEmployeeAsync(id.Value, dto, ct: ct);
            return McpToolResult.Success(JsonSerializationHelper.Serialize(response));
        }

        private async Task<McpToolResult> ExecuteDeleteEmployeeAsync(JsonElement? args, CancellationToken ct)
        {
            var id = GetIntProp(args, "id");
            if (!id.HasValue) return McpToolResult.Error("Parameter 'id' is required");

            var response = await _apiClient.DeleteEmployeeAsync(id.Value, ct: ct);
            return McpToolResult.Success(JsonSerializationHelper.Serialize(response));
        }

        #region Helper Extraction Methods

        private static string? GetStringProp(JsonElement? element, string name)
        {
            if (element.HasValue && element.Value.ValueKind == JsonValueKind.Object && element.Value.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String)
                return prop.GetString();
            return null;
        }

        private static int? GetIntProp(JsonElement? element, string name)
        {
            if (element.HasValue && element.Value.ValueKind == JsonValueKind.Object && element.Value.TryGetProperty(name, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var val)) return val;
                if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out var parsed)) return parsed;
            }
            return null;
        }

        private static bool? GetBoolProp(JsonElement? element, string name)
        {
            if (element.HasValue && element.Value.ValueKind == JsonValueKind.Object && element.Value.TryGetProperty(name, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.True || prop.ValueKind == JsonValueKind.False) return prop.GetBoolean();
                if (prop.ValueKind == JsonValueKind.String && bool.TryParse(prop.GetString(), out var parsed)) return parsed;
            }
            return null;
        }

        private static DateTime? GetDateTimeProp(JsonElement? element, string name)
        {
            var str = GetStringProp(element, name);
            if (!string.IsNullOrWhiteSpace(str) && DateTime.TryParse(str, out var dt)) return dt;
            return null;
        }

        #endregion
    }
}
