using CCMS3.Data;
using CCMS3.Models;
using CCMS3.Services;
using Microsoft.AspNetCore.Identity;

namespace CCMS3.Extensions
{
    public static class DataSeedExtensions
    {
        public static async Task SeedAdminUserAsync(this IServiceProvider services)
        {
            const string adminRole = "Admin";
            const string adminEmail = "admin@example.com";
            const string adminPassword = "Admin@123"; // Replace with a strong password

            using var scope = services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            // Check if the Admin role exists; if not, create it
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            // Check if an admin user exists; if not, create it
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = false,
                    FullName = "Admin123",
                    IsActive=true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                }
            }
        }

        public static async Task SeedDatabaseAsync(this IHost app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<AppDbContext>();
            var stateCityService = services.GetRequiredService<StateCityService>();

            // Seed data only if the States table is empty
            await DbInitializer.SeedAsync(context, stateCityService);
        }
    }
}
