using AssetManagementDemo.McpServer.Endpoints;
using AssetManagementDemo.McpServer.Middleware;
using Microsoft.AspNetCore.Builder;
using AssetManagementDemo.McpServer.Bootstrapper;

var builder = WebApplication.CreateBuilder(args);

// Register Services
builder.Services.AddMcpServerServices(builder.Configuration);

var app = builder.Build();

// Configure Pipeline
app.UseMiddleware<McpExceptionMiddleware>();

// Map MCP Protocol Endpoints (/mcp, /mcp/message)
app.MapMcpEndpoints();

app.Run();
