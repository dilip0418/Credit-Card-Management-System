using CCMS3.Repositories.Implementations;
using CCMS3.Repositories.Interfaces;
using CCMS3.Services;
using CCMS3.Services.Implementations;
using CCMS3.Services.Interfaces;
using MailKit;

namespace CCMS3.Extensions
{
    public static class InjectDependenciesExtesions
    {
        public static IServiceCollection InjectDependencies(this IServiceCollection services)
        {
            services.AddSingleton<FileStorageService>();
            
            services.AddScoped<IPersonalDetailsRepository, PersonalDetailsRepositoryImpl>();
            services.AddScoped<IPersonalDetailsService, PersonalDetailsServiceImpl>();
            
            services.AddScoped<UserService>();
            
            services.AddTransient<IEmailService, EmailServiceImpl>();
            
            services.AddScoped<ICreditCardApplicationRepository,CreditCardApplicationRepositoryImpl>();
            services.AddScoped<ICreditCardApplicationservice, CreditCardApplicationService>();

            services.AddScoped<ICreditCardRepository, CreditCardRepository>();
            services.AddScoped<ICreditCardService, CreditCardService>();
            return services;
        }
    }
}
