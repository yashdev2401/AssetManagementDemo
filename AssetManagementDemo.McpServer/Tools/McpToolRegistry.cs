using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AssetManagementDemo.McpServer.Models;
using AssetManagementDemo.McpServer.Services.Interfaces;

using Microsoft.Extensions.Logging;

namespace AssetManagementDemo.McpServer.Tools
{
    public class McpToolRegistry : IMcpToolRegistry
    {
        private readonly Dictionary<string, IMcpTool> _toolMap = new Dictionary<string, IMcpTool>(StringComparer.OrdinalIgnoreCase);
        private readonly List<McpToolDefinition> _toolDefinitions = new List<McpToolDefinition>();
        private readonly ILogger<McpToolRegistry> _logger;

        public McpToolRegistry(IEnumerable<IMcpTool> tools, ILogger<McpToolRegistry> logger)
        {
            _logger = logger;
            foreach (var tool in tools)
            {
                var definitions = tool.GetToolDefinitions();
                foreach (var def in definitions)
                {
                    if (_toolMap.ContainsKey(def.Name))
                    {
                        _logger.LogWarning("Duplicate tool registration detected for '{ToolName}'", def.Name);
                        continue;
                    }
                    _toolMap[def.Name] = tool;
                    _toolDefinitions.Add(def);
                    _logger.LogInformation("Registered MCP tool: {ToolName}", def.Name);
                }
            }
        }

        public IEnumerable<McpToolDefinition> GetAllToolDefinitions()
        {
            return _toolDefinitions;
        }

        public async Task<McpToolResult> DispatchToolAsync(string toolName, JsonElement? arguments, CancellationToken ct = default)
        {
            if (!_toolMap.TryGetValue(toolName, out var tool))
            {
                _logger.LogWarning("Tool dispatch failed: Tool '{ToolName}' not found in registry", toolName);
                return McpToolResult.Error($"Unknown tool: '{toolName}'");
            }

            _logger.LogInformation("Dispatching tool '{ToolName}' to tool provider {ProviderType}", toolName, tool.GetType().Name);
            return await tool.ExecuteToolAsync(toolName, arguments, ct);
        }
    }
}
