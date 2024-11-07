using CCMS3.Data;
using CCMS3.Dtos;
using CCMS3.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace CCMS3.Services.Implementations
{
    public class UserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public UserService(IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager, AppDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _context = context;
        }

        public string GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst("UserID")?.Value!;
        }

        public async Task<bool> ActivateUserAsync(ActivateAccountDto model)
        {

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || user.ActivationCode != model.ActivationCode || user.CodeExpiration < DateTime.UtcNow)
                {
                    Log.Error("Invalid or expired activation code.!");
                    return false;
                }

                user.IsActive = true;
                user.ActivationCode = "";
                user.CodeExpiration = DateTime.Now;
                user.EmailConfirmed = true;


                var entry = _context.Entry(user);
                if (entry.State == EntityState.Detached)
                {
                    // Attach the user to the context
                    _context.Attach(user);
                }

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    throw new Exception("Failed to update user: " + string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }
            return true;
        }

        public static string GenerateActivationCode() => new Random().Next(100000, 999999).ToString();
    }

}
