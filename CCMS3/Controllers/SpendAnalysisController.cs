using CCMS3.Data;
using CCMS3.Models;
using k8s.KubeConfigModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Globalization;

namespace CCMS3.Controllers
{

    public class ByCategory
    {
        public string CategoryName { get; set; }
        public decimal TotalSpend { get; set; }
    }

    public class MonthlyBreakDown
    {
        public string Month { get; set; }
        public decimal MonthSpend { get; set; }
    }

    public class Summary
    {
        public string CategoryName { get; set; }
        public decimal TotalSpend { get; set; }
        public string Month { get; set; }
        public int Year { get; set; }
    }

    public static class SpendAnalysisEnpoint
    {
        public static IEndpointRouteBuilder MapSpendAnalysisEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{userId}/by-category", GetStatsByCategoryAsync);

            app.MapGet("/{userId}/summary", GetSummaryStats);

            app.MapGet("/{userId}/by-month", GetMonthlyBreakDown);

            return app;
        }

        public static async Task<ApiResponse<IEnumerable<Summary>>> GetSummaryStats(
            string userId,
            AppDbContext dbContext
            )
        {

            try
            { 
                var dtInfo = new DateTimeFormatInfo();
                var summary = dbContext.SpendAnalyses
                    .Include(c => c.Category)
                    .GroupBy(c => new { c.Category.CategoryName, c.Year, c.Month })
                    .Select(c => new Summary
                    {
                        CategoryName = c.Key.CategoryName,
                        Month = dtInfo.GetMonthName(c.Key.Month),
                        Year = c.Key.Year, 
                        TotalSpend = c.Sum(a => a.TotalSpend)
                    });

                if (summary.Any())
                {
                    return new ApiResponse<IEnumerable<Summary>>(StatusCodes.Status200OK, summary);
                }
                else
                {
                    return new ApiResponse<IEnumerable<Summary>>(StatusCodes.Status204NoContent, new List<Summary>());
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
                return new ApiResponse<IEnumerable<Summary>>(StatusCodes.Status500InternalServerError, [e.Message]);
            }
        }

        public static async Task<ApiResponse<List<ByCategory>>> GetStatsByCategoryAsync(
                    string userId,
                    AppDbContext dbContext,
                    [FromQuery] int? month = null, // Optional month filter
                    [FromQuery] int? year = null   // Optional year filter
        )
        {
            var dtInfo = new DateTimeFormatInfo();

            // Build query dynamically
            var query = dbContext.SpendAnalyses
                .Where(sa => sa.PersonalDetailsId == userId);

            // Apply year filter if provided
            if (year.HasValue)
            {
                query = query.Where(sa => sa.Year == year.Value);
            }

            // Apply month filter if provided
            if (month.HasValue)
            {
                query = query.Where(sa => sa.Month == month.Value);
            }

            // Group and summarize the data
            var byCategory = await query
                .GroupBy(sa => new { sa.Category.CategoryName, sa.Year, sa.Month })
                .Select(g => new ByCategory
                {
                    CategoryName = g.Key.CategoryName,
                    TotalSpend = g.Sum(sa => sa.TotalSpend)
                })
                .ToListAsync();

            // Check for empty result
            if (byCategory.Count == 0)
            {
                return new ApiResponse<List<ByCategory>>(
                    StatusCodes.Status400BadRequest,
                    ["No spending data available for the user."]
                );
            }

            return new ApiResponse<List<ByCategory>>(
                StatusCodes.Status200OK,
                byCategory,
                "Success"
            );
        }



        public static async Task<ApiResponse<IEnumerable<MonthlyBreakDown>>> GetMonthlyBreakDown(
            [FromQuery] int? year,
            string userId,
            AppDbContext context
            )
        {
            try
            {
                var dtInfo = new DateTimeFormatInfo();
                var monthlyBreakDown = await context.SpendAnalyses
                                        .Where(u => u.PersonalDetailsId.Equals(userId))
                                        .Where(s => s.Year == (year ?? DateTime.Now.Year))
                                        .GroupBy(g => g.Month)
                                        .Select(s => new MonthlyBreakDown
                                        {
                                            Month = dtInfo.GetMonthName(s.Key), // Include the month in the result
                                            MonthSpend = s.Sum(a => a.TotalSpend)
                                        })
                                        .ToListAsync();

                if (monthlyBreakDown.Any())
                {
                    return new ApiResponse<IEnumerable<MonthlyBreakDown>>(StatusCodes.Status200OK, monthlyBreakDown);
                }
                else
                {
                    return new ApiResponse<IEnumerable<MonthlyBreakDown>>(StatusCodes.Status204NoContent, [], "No Data to show");
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
                return new ApiResponse<IEnumerable<MonthlyBreakDown>>(StatusCodes.Status500InternalServerError, [e.Message]);
            }
        }
    }
}
