using CCMS3.Data;
using CCMS3.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;


namespace CCMS3.Extensions
{
    public static class HealthCheckExtensions
    {
        public static void InjectHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<AppDbContext>()
                .AddCheck<CustomHealthCheck>("/custom-health-check")
                .AddSqlServer(configuration.GetConnectionString("DevDb")!, name:"Sql Server Instance");

            services.AddHealthChecksUI()
                .AddInMemoryStorage();
        }

        public static void UseHealthCheck(this IApplicationBuilder app)
        {
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            // Use the health check UI middleware
            app.UseHealthChecksUI(options =>
            {
                options.UIPath = "/health-ui";
                options.ApiPath = "/health-api";
            });
        }
    }   
}
