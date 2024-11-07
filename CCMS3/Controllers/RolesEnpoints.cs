using CCMS3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CCMS3.Controllers
{
    public static class RolesEnpoints
    {
        public static IEndpointRouteBuilder MapRolesEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/create", CreateRole)
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest);

            app.MapPost("/assign", AssignRoleToUser)
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest);

            app.MapGet("/user/{userId}", GetUserRoles)
                .Produces<List<string>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            app.MapDelete("/{roleName}", DeleteRole)
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);

            return app;
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
            [FromBody] string roleName)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound("User not found");

            var result = await userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
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
    }
}
