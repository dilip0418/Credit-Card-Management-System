using CCMS3.Dtos;
using CCMS3.Models;
using CCMS3.Services.Implementations;
using CCMS3.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CCMS3.Controllers
{
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegistrationModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
    }

    public class UserResponse()
    {
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public bool IsProfileComplete { get; set; }
        public string FullName { get; set; }
    }
    public static class UserEndpoints
    {
        public static IEndpointRouteBuilder MapIdentityUserEndpoints(
            this IEndpointRouteBuilder app)
        {
            app.MapPost("/signup", CreateUser);
            app.MapPost("/signin", SignIn);
            app.MapGet("/activate-user", ActivateUser);
            app.MapGet("/{id}", GetUserById);
            return app;
        }


        private static async Task<IResult> CreateUser(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService,
            [FromBody] RegistrationModel userRegistrationModel)
        {
            var activationCode = UserService.GenerateActivationCode();
            AppUser user = new()
            {
                UserName = userRegistrationModel.Email,
                Email = userRegistrationModel.Email,
                FullName = userRegistrationModel.FullName,
                ActivationCode = activationCode,
                CodeExpiration = DateTime.UtcNow.AddMinutes(15)
            };

            var activateAccountDto = new ActivateAccountDto()
            {
                Email = user.Email,
                ActivationCode = activationCode
            };
            try
            {

                var result = await userManager.CreateAsync(user,
                    userRegistrationModel.Password);

                if (result.Succeeded)
                {
                    // Retrieve the existing "User" role
                    var role = await roleManager.FindByNameAsync("User");
                    if (role != null)
                    {
                        // Assign the "User" role to the newly created user
                        var roleResult = await userManager.AddToRoleAsync(user, role.Name!);
                        if (!roleResult.Succeeded)
                        {
                            return Results.BadRequest(roleResult.Errors);
                        }

                        await emailService.SendActivationEmailAsync(activateAccountDto);


                        return Results.Ok(new { UserId = user.Id, Message = "User created successfully and assigned to the User role." });
                    }
                    else
                    {
                        return Results.NotFound(new { Message = "Role 'User' not found." });
                    }
                }
                else
                {
                    return Results.BadRequest(result.Errors);
                }
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        }



        [AllowAnonymous]
        private static async Task<IResult> SignIn(
        UserManager<AppUser> userManager,
            [FromBody] LoginModel loginModel,
            IOptions<AppSettings> appSettings,
            IEmailService emailService)
        {
            var user = await userManager.FindByEmailAsync(loginModel.Email);

            // Check if user exists
            if (user == null)
            {
                return Results.BadRequest(new { message = "Username or password is incorrect." });
            }

            // Check password
            if (!await userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                return Results.BadRequest(new { message = "Username or password is incorrect." });
            }

            // Handle inactive user
            if (!user.IsActive)
            {
                user.CodeExpiration = DateTime.UtcNow.AddMinutes(15);
                var code = UserService.GenerateActivationCode();
                user.ActivationCode = code;
                await userManager.UpdateAsync(user);
                var activateAccountDto = new ActivateAccountDto
                {
                    Email = loginModel.Email,
                    ActivationCode = code
                };

                try
                {
                    await emailService.SendActivationEmailAsync(activateAccountDto);
                    return Results.Problem("Your account is inactive. An activation link has been sent to your email!");
                }
                catch (Exception ex)
                {
                    // Consider logging the exception
                    return Results.BadRequest(new { message = "Failed to send activation email: " + ex.Message });
                }
            }

            // Get user roles
            var roles = await userManager.GetRolesAsync(user);
            if (roles.Count == 0)
            {
                return Results.BadRequest(new { message = "User has no assigned roles." });
            }

            // Create token
            var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Value.JWTSecretKey));
            ClaimsIdentity claims = new ClaimsIdentity(new[]
            {
                new Claim("UserID", user.Id.ToString()),
                new Claim(ClaimTypes.Role, roles[0]), // Be cautious about index access
                new Claim(ClaimTypes.Email, user.Email) // Optional: add email claim
            });

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddDays(1), // Shortened expiration for better security
                SigningCredentials = new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(securityToken);
            return Results.Ok(new { token, user.Id });
        }

        private static async Task<ApiResponse<string>> ActivateUser(
            IEmailService emailService,
            UserService userService,
            [FromQuery] string email,
            [FromQuery] string code)
        {
            try
            {
                var model = new ActivateAccountDto
                {
                    Email = email,
                    ActivationCode = code
                };
                bool isActivated = userService.ActivateUserAsync(model).Result;
                if (isActivated)
                {
                    await emailService.SendPromotionInformationalMailAsync(email);
                    return new ApiResponse<string>(StatusCodes.Status202Accepted, "Account Activation Successful!");
                }
                return new ApiResponse<string>(StatusCodes.Status404NotFound, "User not found or activation failed.");
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        private static ApiResponse<UserResponse> GetUserById(
            string id,
            UserService userService
            )
        {
            try
            {
                var user = userService.GetUserById(id);
                if (user == null)
                {
                    return new ApiResponse<UserResponse>(StatusCodes.Status404NotFound, ["user not found"]);
                }
                var response = new UserResponse
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    UserEmail = user.Email,
                    IsProfileComplete = (user.PersonalDetails != null)
                };

                return new ApiResponse<UserResponse>(StatusCodes.Status200OK, response, "User found successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserResponse>(StatusCodes.Status500InternalServerError, [ex.Message]);
            }
        }
    }
}
