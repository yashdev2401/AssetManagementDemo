using AssetManagementDemo.Web.Data;
using AssetManagementDemo.Web.Repositories;
using AssetManagementDemo.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace AssetManagementDemo.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Server=(localdb)\\mssqllocaldb;Database=AssetManagementDB;Trusted_Connection=True;TrustServerCertificate=True;";

            services.AddDbContext<AssetDbContext>(options =>
                options.UseSqlServer(connectionString));

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IAssetRepository, AssetRepository>();
            services.AddScoped<IAssetAssignmentRepository, AssetAssignmentRepository>();

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IAssetService, AssetService>();
            services.AddScoped<IAssetAssignmentService, AssetAssignmentService>();

            return services;
        }

        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Asset Management REST API",
                    Version = "v1",
                    Description = "RESTful APIs for Asset Management Enterprise Template (compatible with future AssetManagementDemo.McpServer)",
                });
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerUIConfiguration(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Asset Management API v1");
                c.RoutePrefix = "swagger";
            });

            return app;
        }
    }
}
