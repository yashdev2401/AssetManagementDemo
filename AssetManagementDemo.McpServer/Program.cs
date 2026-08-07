using AssetManagementDemo.McpServer.Endpoints;
using AssetManagementDemo.McpServer.Middleware;
using AssetManagementDemo.McpServer.Bootstrapper;
using AssetManagementDemo.McpServer.Models;
using AssetManagementDemo.McpServer.Constants;
using AssetManagementDemo.McpServer.Helpers;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Register Services
builder.Services.AddMcpServerServices(builder.Configuration);

int permitLimit = builder.Configuration.GetValue<int>("RateLimiting:PermitLimit", 20);
int windowInSeconds = builder.Configuration.GetValue<int>("RateLimiting:WindowInSeconds", 60);
int queueLimit = builder.Configuration.GetValue<int>("RateLimiting:QueueLimit", 0);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("ApiPolicy", httpContext =>
    {
        string clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) && !string.IsNullOrWhiteSpace(forwardedFor))
        {
            clientIp = forwardedFor.ToString().Split(',')[0].Trim();
        }
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp,
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowInSeconds),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = queueLimit
            });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers["Retry-After"] = windowInSeconds.ToString();
        context.HttpContext.Response.ContentType = "application/json; charset=utf-8";

        var jsonRpcError = McpJsonRpcResponse.Failure(
            id: null,
            code: McpConstants.ErrorCodes.InternalError,
            message: "Too Many Requests. Rate limit exceeded.",
            data: $"Maximum allowed requests: {permitLimit} per {windowInSeconds} seconds. Please retry after {windowInSeconds} seconds."
        );

        await context.HttpContext.Response.WriteAsync(
            JsonSerializationHelper.Serialize(jsonRpcError),
            cancellationToken: token);
    };
});
var app = builder.Build();

app.UseRouting();
app.UseRateLimiter();
// Configure Pipeline
app.UseMiddleware<McpExceptionMiddleware>();

// Map MCP Protocol Endpoints (/mcp, /mcp/message)
app.MapMcpEndpoints();

app.Run();
