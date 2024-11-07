using CCMS3.Helpers;
using CCMS3.Models;

namespace CCMS3.Extensions
{
    public static class AppConfigExtensions
    {
        public static IServiceCollection AddAppConfig(
            this IServiceCollection services,
            IConfiguration config)
        {
            services.Configure<AppSettings>(config.GetSection("AppSettings"));
            return services;
        }

        public static IServiceCollection AddMailConfig(
            this IServiceCollection services,
            IConfiguration config)
        {
            services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
            return services;
        }
    }
}
