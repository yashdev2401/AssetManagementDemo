using System;
using System.IO;
using System.Threading.Tasks;

using AssetManagementDemo.McpServer.Services.Interfaces;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AssetManagementDemo.McpServer.Endpoints
{
    public static class McpEndpointRouteBuilderExtensions
    {
        public static IEndpointRouteBuilder MapMcpEndpoints(this IEndpointRouteBuilder endpoints)
        {
            // MCP JSON-RPC 2.0 POST Endpoints
            endpoints.MapPost("/mcp", HandleMcpPostRequestAsync).RequireRateLimiting("ApiPolicy");
            endpoints.MapPost("/mcp/message", HandleMcpPostRequestAsync).RequireRateLimiting("ApiPolicy");

            // Browser Friendly GET Status Endpoints
            endpoints.MapGet("/", HandleStatusGetRequestAsync).RequireRateLimiting("ApiPolicy");
            endpoints.MapGet("/mcp", HandleStatusGetRequestAsync).RequireRateLimiting("ApiPolicy");
            endpoints.MapGet("/mcp/message", HandleStatusGetRequestAsync).RequireRateLimiting("ApiPolicy");

            return endpoints;
        }

        private static async Task HandleMcpPostRequestAsync(HttpContext context)
        {
            var processor = context.RequestServices.GetRequiredService<IMcpJsonRpcProcessor>();

            string correlationId = context.Request.Headers["X-Correlation-ID"].ToString();
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString("N");
            }
            context.Response.Headers["X-Correlation-ID"] = correlationId;

            using var reader = new StreamReader(context.Request.Body);
            var rawJson = await reader.ReadToEndAsync(context.RequestAborted);

            var jsonResponse = await processor.ProcessRequestAsync(rawJson, correlationId, context.RequestAborted);

            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsync(jsonResponse, context.RequestAborted);
        }

        private static async Task HandleStatusGetRequestAsync(HttpContext context)
        {
            var processor = context.RequestServices.GetRequiredService<IMcpJsonRpcProcessor>();
            var healthJson = await processor.ProcessRequestAsync("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"health\"}", null, context.RequestAborted);

            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsync(healthJson, context.RequestAborted);
        }
    }
}
