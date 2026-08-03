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
    public class AssignmentTools : IMcpTool
    {
        private readonly IMvcApiClient _apiClient;
        private readonly ILogger<AssignmentTools> _logger;

        public AssignmentTools(IMvcApiClient apiClient, ILogger<AssignmentTools> logger)
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
                    Name = "assign_asset",
                    Description = "Assigns an available asset to an employee.",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            employeeId = new { type = "integer", description = "Target Employee ID" },
                            assetId = new { type = "integer", description = "Target Asset ID" },
                            assignedDate = new { type = "string", description = "Assigned Date (YYYY-MM-DD, default today)" },
                            remarks = new { type = "string", description = "Optional assignment remarks" }
                        },
                        required = new[] { "employeeId", "assetId" }
                    }
                },
                new McpToolDefinition
                {
                    Name = "return_asset",
                    Description = "Processes the return of an active asset assignment.",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            assignmentId = new { type = "integer", description = "Assignment ID to return" },
                            returnDate = new { type = "string", description = "Return Date (YYYY-MM-DD, default today)" },
                            remarks = new { type = "string", description = "Optional return remarks" }
                        },
                        required = new[] { "assignmentId" }
                    }
                },
                new McpToolDefinition
                {
                    Name = "assignment_history",
                    Description = "Retrieves assignment logs/history for an employee or asset.",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            searchTerm = new { type = "string", description = "Search by employee code, name, asset code, or asset name" },
                            assignedDate = new { type = "string", description = "Filter by assignment date (YYYY-MM-DD)" },
                            pageNumber = new { type = "integer", description = "Page index (default 1)" },
                            pageSize = new { type = "integer", description = "Items per page (10, 20, 50, 100)" }
                        }
                    }
                },
                new McpToolDefinition
                {
                    Name = "active_assignments",
                    Description = "Retrieves currently active (unreturned) asset assignments.",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            searchTerm = new { type = "string", description = "Search query" },
                            pageNumber = new { type = "integer", description = "Page index (default 1)" },
                            pageSize = new { type = "integer", description = "Items per page (10, 20, 50, 100)" }
                        }
                    }
                }
            };
        }

        public async Task<McpToolResult> ExecuteToolAsync(string toolName, JsonElement? args, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing AssignmentTool: {ToolName}", toolName);

            try
            {
                return toolName.ToLower() switch
                {
                    "assign_asset" => await ExecuteAssignAssetAsync(args, ct),
                    "return_asset" => await ExecuteReturnAssetAsync(args, ct),
                    "assignment_history" => await ExecuteAssignmentHistoryAsync(args, ct),
                    "active_assignments" => await ExecuteActiveAssignmentsAsync(args, ct),
                    _ => McpToolResult.Error($"Unknown assignment tool '{toolName}'")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing AssignmentTool {ToolName}", toolName);
                return McpToolResult.Error($"Execution error: {ex.Message}");
            }
        }

        private async Task<McpToolResult> ExecuteAssignAssetAsync(JsonElement? args, CancellationToken ct)
        {
            var employeeId = GetIntProp(args, "employeeId");
            var assetId = GetIntProp(args, "assetId");

            if (!employeeId.HasValue || !assetId.HasValue)
            {
                return McpToolResult.Error("Parameters 'employeeId' and 'assetId' are required.");
            }

            var dto = new AssignAssetDto
            {
                EmployeeId = employeeId.Value,
                AssetId = assetId.Value,
                AssignedDate = GetDateTimeProp(args, "assignedDate") ?? DateTime.Today,
                Remarks = GetStringProp(args, "remarks")
            };

            var response = await _apiClient.AssignAssetAsync(dto, ct: ct);
            return McpToolResult.Success(JsonSerializationHelper.Serialize(response));
        }

        private async Task<McpToolResult> ExecuteReturnAssetAsync(JsonElement? args, CancellationToken ct)
        {
            var assignmentId = GetIntProp(args, "assignmentId");
            if (!assignmentId.HasValue) return McpToolResult.Error("Parameter 'assignmentId' is required.");

            var dto = new ReturnAssetDto
            {
                AssignmentId = assignmentId.Value,
                ReturnDate = GetDateTimeProp(args, "returnDate") ?? DateTime.Today,
                Remarks = GetStringProp(args, "remarks")
            };

            var response = await _apiClient.ReturnAssetAsync(dto, ct: ct);
            return McpToolResult.Success(JsonSerializationHelper.Serialize(response));
        }

        private async Task<McpToolResult> ExecuteAssignmentHistoryAsync(JsonElement? args, CancellationToken ct)
        {
            var searchTerm = GetStringProp(args, "searchTerm");
            var assignedDate = GetStringProp(args, "assignedDate");
            var pageNumber = GetIntProp(args, "pageNumber") ?? 1;
            var pageSize = GetIntProp(args, "pageSize") ?? 10;

            var response = await _apiClient.GetAssignmentsAsync(searchTerm, null, assignedDate, "AssignedDate", true, pageNumber, pageSize, ct: ct);
            return McpToolResult.Success(JsonSerializationHelper.Serialize(response));
        }

        private async Task<McpToolResult> ExecuteActiveAssignmentsAsync(JsonElement? args, CancellationToken ct)
        {
            var searchTerm = GetStringProp(args, "searchTerm");
            var pageNumber = GetIntProp(args, "pageNumber") ?? 1;
            var pageSize = GetIntProp(args, "pageSize") ?? 10;

            var response = await _apiClient.GetAssignmentsAsync(searchTerm, true, null, "AssignedDate", true, pageNumber, pageSize, ct: ct);
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

        private static DateTime? GetDateTimeProp(JsonElement? element, string name)
        {
            var str = GetStringProp(element, name);
            if (!string.IsNullOrWhiteSpace(str) && DateTime.TryParse(str, out var dt)) return dt;
            return null;
        }

        #endregion
    }
}
