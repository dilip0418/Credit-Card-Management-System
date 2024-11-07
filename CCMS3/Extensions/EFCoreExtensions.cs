using CCMS3.Data;
using Microsoft.EntityFrameworkCore;

namespace CCMS3.Extensions
{
    public static class EFCoreExtensions
    {
        public static IServiceCollection InjectDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(
                options =>
                        options.UseSqlServer(
                            configuration.GetConnectionString("DevDB"))
                        );
            return services;
        }
    }
}
