using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using AssetManagementDemo.McpServer.Helpers;
using AssetManagementDemo.McpServer.Models;
using AssetManagementDemo.McpServer.Services.Interfaces;

using Microsoft.Extensions.Logging;

namespace AssetManagementDemo.McpServer.Tools
{
    public class HealthTools : IMcpTool
    {
        private readonly IMvcApiClient _apiClient;
        private readonly ILogger<HealthTools> _logger;

        public HealthTools(IMvcApiClient apiClient, ILogger<HealthTools> logger)
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
                    Name = "health",
                    Description = "Verifies system connectivity, REST API status, and MVC application health.",
                    InputSchema = new
                    {
                        type = "object",
                        properties = new { }
                    }
                }
            };
        }

        public async Task<McpToolResult> ExecuteToolAsync(string toolName, JsonElement? arguments, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing Health Tool: {ToolName}", toolName);

            try
            {
                var isHealthy = await _apiClient.CheckHealthAsync(null, ct);
                var healthReport = new
                {
                    status = isHealthy ? "Healthy" : "Degraded",
                    mcpServer = "Online",
                    mvcApiConnected = isHealthy,
                    timestamp = DateTime.UtcNow
                };

                return isHealthy 
                    ? McpToolResult.Success(JsonSerializationHelper.Serialize(healthReport))
                    : McpToolResult.Error(JsonSerializationHelper.Serialize(healthReport));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Health Tool");
                return McpToolResult.Error($"Health check exception: {ex.Message}");
            }
        }
    }
}
