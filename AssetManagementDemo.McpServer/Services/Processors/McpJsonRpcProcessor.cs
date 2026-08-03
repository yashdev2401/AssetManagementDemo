using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using AssetManagementDemo.McpServer.Constants;
using AssetManagementDemo.McpServer.Helpers;
using AssetManagementDemo.McpServer.Models;
using AssetManagementDemo.McpServer.Services.Interfaces;

using Microsoft.Extensions.Logging;

namespace AssetManagementDemo.McpServer.Services.Providers
{
    public class McpJsonRpcProcessor : IMcpJsonRpcProcessor
    {
        private readonly IMcpToolRegistry _toolRegistry;
        private readonly ILogger<McpJsonRpcProcessor> _logger;

        public McpJsonRpcProcessor(IMcpToolRegistry toolRegistry, ILogger<McpJsonRpcProcessor> logger)
        {
            _toolRegistry = toolRegistry;
            _logger = logger;
        }

        public async Task<string> ProcessRequestAsync(string rawJson, string? correlationId = null, CancellationToken ct = default)
        {
            var cid = correlationId ?? Guid.NewGuid().ToString("N");
            var sw = Stopwatch.StartNew();
            _logger.LogInformation("[CorrelationId: {CorrelationId}] Processing incoming MCP JSON-RPC payload", cid);

            McpJsonRpcRequest? request = null;
            try
            {
                request = JsonSerializationHelper.Deserialize<McpJsonRpcRequest>(rawJson);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[CorrelationId: {CorrelationId}] JSON parse error on raw payload", cid);
                var parseError = McpJsonRpcResponse.Failure(null, McpConstants.ErrorCodes.ParseError, "Parse error: Invalid JSON payload.", ex.Message);
                return JsonSerializationHelper.Serialize(parseError);
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Method))
            {
                _logger.LogWarning("[CorrelationId: {CorrelationId}] Invalid JSON-RPC request: missing method", cid);
                var invalidReq = McpJsonRpcResponse.Failure(request?.Id, McpConstants.ErrorCodes.InvalidRequest, "Invalid Request: 'method' is required.");
                return JsonSerializationHelper.Serialize(invalidReq);
            }

            if (request.JsonRpc != McpConstants.JsonRpcVersion)
            {
                _logger.LogWarning("[CorrelationId: {CorrelationId}] Invalid JSON-RPC version '{Version}'", cid, request.JsonRpc);
                var invalidVer = McpJsonRpcResponse.Failure(request.Id, McpConstants.ErrorCodes.InvalidRequest, $"Invalid Request: Expected jsonrpc version '{McpConstants.JsonRpcVersion}'.");
                return JsonSerializationHelper.Serialize(invalidVer);
            }

            try
            {
                McpJsonRpcResponse response = request.Method.ToLower() switch
                {
                    McpConstants.Methods.Initialize => HandleInitialize(request),
                    McpConstants.Methods.Ping => HandlePing(request),
                    "health" => await HandleHealthAsync(request, cid, ct),
                    McpConstants.Methods.ToolsList => HandleToolsList(request),
                    McpConstants.Methods.ToolsCall => await HandleToolsCallAsync(request, cid, ct),
                    _ => McpJsonRpcResponse.Failure(request.Id, McpConstants.ErrorCodes.MethodNotFound, $"Method '{request.Method}' not found.")
                };

                sw.Stop();
                _logger.LogInformation("[CorrelationId: {CorrelationId}] Successfully processed MCP Method '{Method}' in {ElapsedMs}ms", cid, request.Method, sw.ElapsedMilliseconds);
                return JsonSerializationHelper.Serialize(response);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "[CorrelationId: {CorrelationId}] Unhandled internal error processing method '{Method}' in {ElapsedMs}ms", cid, request.Method, sw.ElapsedMilliseconds);
                var errResp = McpJsonRpcResponse.Failure(request.Id, McpConstants.ErrorCodes.InternalError, "Internal error processing request: " + ex.Message);
                return JsonSerializationHelper.Serialize(errResp);
            }
        }

        private McpJsonRpcResponse HandleInitialize(McpJsonRpcRequest request)
        {
            var initResult = new
            {
                protocolVersion = McpConstants.ProtocolVersion,
                capabilities = new
                {
                    tools = new
                    {
                        listChanged = false
                    }
                },
                serverInfo = new
                {
                    name = McpConstants.ServerName,
                    version = McpConstants.ServerVersion
                }
            };

            return McpJsonRpcResponse.Success(request.Id, initResult);
        }

        private McpJsonRpcResponse HandlePing(McpJsonRpcRequest request)
        {
            return McpJsonRpcResponse.Success(request.Id, new { });
        }

        private async Task<McpJsonRpcResponse> HandleHealthAsync(McpJsonRpcRequest request, string correlationId, CancellationToken ct)
        {
            var toolResult = await _toolRegistry.DispatchToolAsync("health", null, ct);
            return McpJsonRpcResponse.Success(request.Id, toolResult);
        }

        private McpJsonRpcResponse HandleToolsList(McpJsonRpcRequest request)
        {
            var tools = _toolRegistry.GetAllToolDefinitions();
            return McpJsonRpcResponse.Success(request.Id, new { tools });
        }

        private async Task<McpJsonRpcResponse> HandleToolsCallAsync(McpJsonRpcRequest request, string correlationId, CancellationToken ct)
        {
            if (!request.Params.HasValue || request.Params.Value.ValueKind != JsonValueKind.Object)
            {
                return McpJsonRpcResponse.Failure(request.Id, McpConstants.ErrorCodes.InvalidParams, "Invalid params: JSON object expected for tools/call");
            }

            var paramsElement = request.Params.Value;
            if (!paramsElement.TryGetProperty("name", out var nameProp) || nameProp.ValueKind != JsonValueKind.String)
            {
                return McpJsonRpcResponse.Failure(request.Id, McpConstants.ErrorCodes.InvalidParams, "Invalid params: 'name' property is required");
            }

            var toolName = nameProp.GetString() ?? string.Empty;
            JsonElement? arguments = null;
            if (paramsElement.TryGetProperty("arguments", out var argsProp) && argsProp.ValueKind == JsonValueKind.Object)
            {
                arguments = argsProp;
            }

            var toolResult = await _toolRegistry.DispatchToolAsync(toolName, arguments, ct);
            return McpJsonRpcResponse.Success(request.Id, toolResult);
        }
    }
}