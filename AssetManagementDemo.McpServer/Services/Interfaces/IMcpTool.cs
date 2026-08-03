using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using AssetManagementDemo.McpServer.Models;

namespace AssetManagementDemo.McpServer.Services.Interfaces
{
    public interface IMcpTool
    {
        IEnumerable<McpToolDefinition> GetToolDefinitions();
        Task<McpToolResult> ExecuteToolAsync(string toolName, JsonElement? arguments, CancellationToken ct = default);
    }
}
