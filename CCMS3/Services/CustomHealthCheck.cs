using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CCMS3.Services
{
    public class CustomHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            // Implement your custom health check logic here
            // For example, check an external service or resource
            bool isHealthy = true;
            return Task.FromResult(isHealthy
                ? HealthCheckResult.Healthy("The custom health check is healthy")
                : HealthCheckResult.Unhealthy("The custom health check is unhealthy"));
        }
    }
}
