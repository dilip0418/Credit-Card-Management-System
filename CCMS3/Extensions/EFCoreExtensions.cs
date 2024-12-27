using CCMS3.Data;
using Microsoft.EntityFrameworkCore;

namespace CCMS3.Extensions
{
    public static class EFCoreExtensions
    {
        public static IServiceCollection InjectDbContext(this IServiceCollection services, IConfiguration configuration)
        {


            var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");
            var dbPassword = Environment.GetEnvironmentVariable("DB_SA_PASSWORD");
            var connectionString = $"Data Source={dbHost},8003;Initial Catalog={dbName};User ID=sa;Password={dbPassword};TrustServerCertificate=True";
            services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString));
            return services;
        }
    }
}
//configuration.GetConnectionString("DevDB")) for local http run without docker