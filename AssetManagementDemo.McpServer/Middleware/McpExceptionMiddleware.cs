using System;
using System.Threading.Tasks;

using AssetManagementDemo.McpServer.Constants;
using AssetManagementDemo.McpServer.Helpers;
using AssetManagementDemo.McpServer.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AssetManagementDemo.McpServer.Middleware
{
    public class McpExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<McpExceptionMiddleware> _logger;

        public McpExceptionMiddleware(RequestDelegate next, ILogger<McpExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled Exception in MCP Middleware Pipeline");

                if (!context.Response.HasStarted)
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    context.Response.StatusCode = StatusCodes.Status200OK;

                    var jsonRpcError = McpJsonRpcResponse.Failure(null, McpConstants.ErrorCodes.InternalError, "Unhandled middleware exception", ex.Message);
                    await context.Response.WriteAsync(JsonSerializationHelper.Serialize(jsonRpcError));
                }
            }
        }
    }
}
