using AssetManagementDemo.McpServer.Configuration;
using AssetManagementDemo.McpServer.Services.Clients;
using AssetManagementDemo.McpServer.Services.Interfaces;
using AssetManagementDemo.McpServer.Services.Providers;
using AssetManagementDemo.McpServer.Tools;

namespace AssetManagementDemo.McpServer.Bootstrapper
{
    public static class DependencyInjectionBootstrapper
	{
        public static IServiceCollection AddMcpServerServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind Configuration under MvcApi section
            var apiSection = configuration.GetSection("MvcApi");
            services.Configure<MvcApiOptions>(apiSection);
            var mvcOptions = apiSection.Get<MvcApiOptions>() ?? new MvcApiOptions();

            // Register Strongly Typed HttpClient
            services.AddHttpClient<IMvcApiClient, MvcApiClient>(client =>
            {
                var baseUrl = mvcOptions.BaseUrl ?? "http://localhost:5091/";
                client.BaseAddress = new Uri(baseUrl.EndsWith("/") ? baseUrl : $"{baseUrl}/");
                client.Timeout = TimeSpan.FromSeconds(mvcOptions.TimeoutSeconds > 0 ? mvcOptions.TimeoutSeconds : 30);
                client.DefaultRequestHeaders.Add("User-Agent", "AssetManagementDemo.McpServer/1.0");
				client.DefaultRequestHeaders.Add("X-Api-Key",configuration["ApiSecurity:ApiKey"]);
			});

            // Register Tool Providers
            services.AddSingleton<IMcpTool, HealthTools>();
            services.AddSingleton<IMcpTool, EmployeeTools>();
            services.AddSingleton<IMcpTool, AssetTools>();
            services.AddSingleton<IMcpTool, AssignmentTools>();

            // Register Tool Registry and Protocol Processor
            services.AddSingleton<IMcpToolRegistry, McpToolRegistry>();
            services.AddSingleton<IMcpJsonRpcProcessor, McpJsonRpcProcessor>();

            return services;
        }
    }
}
