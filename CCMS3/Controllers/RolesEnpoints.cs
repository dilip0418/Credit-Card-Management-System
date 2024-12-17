using CCMS3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CCMS3.Controllers
{
    public static class RolesEnpoints
    {
        public static IEndpointRouteBuilder MapRolesEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/create", CreateRole);
            app.MapPost("/assign", AssignRoleToUser);
            app.MapGet("/user/{userId}", GetUserRoles);
            app.MapDelete("/{roleName}", DeleteRole);
            app.MapDelete("/unassign-role", UnAssignRoleAsync);
            app.MapGet("/users-with-roles", GetUsersWithRolesAsync);
            app.MapGet("/", GetAllRoles);

            return app;
        }


        private static async Task<IResult> GetAllRoles(RoleManager<IdentityRole> roleMngr)
        {
            var roles = await roleMngr.Roles.Select(x => x.Name).ToListAsync();
            if (roles.Any())
            {
                return Results.Ok(roles);
            }
            else
            {
                return Results.NoContent();
            }
        }

        [Authorize(Roles ="Admin")]
        private static async Task<IResult> CreateRole(
            RoleManager<IdentityRole> roleManager,
            [FromBody] string roleName)
        {
            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
        }

        [Authorize]
        private static async Task<IResult> AssignRoleToUser(
            UserManager<AppUser> userManager,
            string userId,
            string roleName)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound("User not found");

            var result = await userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
        }

        [Authorize]
        private static async Task<IResult> UnAssignRoleAsync(
            string userId, 
            string roleName,
            UserManager<AppUser> _userManager)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
            {
                return Results.BadRequest("User ID and Role Name must be provided.");
            }

            // Find the user by ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Results.NotFound("User not found.");
            }

            // Get all roles of the user
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles == null || !userRoles.Any())
            {
                return Results.BadRequest("User has no roles assigned.");
            }

            // Ensure the user has more than one role
            if (userRoles.Count <= 1)
            {
                return Results.BadRequest("Cannot unassign role. The user must have at least one role.");
            }

            // Check if the user has the role to be removed
            if (!userRoles.Contains(roleName))
            {
                return Results.BadRequest("The user does not have the specified role.");
            }

            // Remove the role
            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                return Results.StatusCode(500);
            }

            return Results.Ok($"Role '{roleName}' has been removed from user '{user.UserName}'.");

        }

        [Authorize]
        private static async Task<IResult> GetUserRoles(
            UserManager<AppUser> userManager,
            string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound("User not found");

            var roles = await userManager.GetRolesAsync(user);
            return Results.Ok(roles);
        }

        [Authorize]
        private static async Task<IResult> DeleteRole(
            RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager,
            string roleName)
        {
            // Check if the role exists
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return Results.NotFound(new { Message = "Role not found." });
            }

            // Check if any users are assigned to this role
            var usersInRole = await userManager.GetUsersInRoleAsync(roleName);
            if (usersInRole.Any())
            {
                return Results.BadRequest(new { Message = "Role cannot be deleted as it has users assigned." });
            }

            // Delete the role
            var result = await roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                return Results.Json(new { Message = "An error occurred while deleting the role.", status = 500 });
            }

            return Results.Ok(new { Message = "Role deleted successfully." });
        }


        [Authorize]
        private static async Task<IResult> GetUsersWithRolesAsync(
            UserManager<AppUser> _userManager)
        {
            // Get all users
            var users = _userManager.Users.ToList();

            // Prepare a list to store users and their roles
            var userRolesList = new List<object>();

            foreach (var user in users)
            {
                // Get roles for each user
                var roles = await _userManager.GetRolesAsync(user);

                // Add user and their roles to the list
                userRolesList.Add(new
                {
                    UserId = user.Id,
                    Username = user.UserName,
                    Roles = roles
                });
            }

            return Results.Ok(userRolesList);
        }

    }
}
