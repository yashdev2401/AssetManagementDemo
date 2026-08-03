using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using AssetManagementDemo.McpServer.Models;

namespace AssetManagementDemo.McpServer.Services.Interfaces
{
    public interface IMcpToolRegistry
    {
        IEnumerable<McpToolDefinition> GetAllToolDefinitions();
        Task<McpToolResult> DispatchToolAsync(string toolName, JsonElement? arguments, CancellationToken ct = default);
    }
}