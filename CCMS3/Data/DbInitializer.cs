using CCMS3.Models;
using CCMS3.Services;
using Microsoft.EntityFrameworkCore;

namespace CCMS3.Data
{
    public static class DbInitializer
    {

        public static async Task SeedAsync(AppDbContext context, StateCityService stateCityService)
        {
            // Seed states if not already present
            if (!context.States.Any())
                await SeedStatesAsync(context, stateCityService); // Your existing state seeding method

            if (!context.Cities.Any())
                await SeedCitiesForAllStatesAsync(context, stateCityService); // Now seed cities for the states

        }


        public static async Task SeedStatesAsync(AppDbContext context, StateCityService stateCityService)
        {
            // Check if the States table already has data
            if (!await context.States.AnyAsync())
            {
                // Fetch state data from the API
                var states = await stateCityService.FetchStatesAsync();

                // Add the states to the database
                await context.States.AddRangeAsync(states);

                // Save changes to the database
                await context.SaveChangesAsync();
            }
        }



        private static async Task SeedCitiesForAllStatesAsync(AppDbContext context, StateCityService stateCityService)
        {
            var states = await context.States.ToListAsync();

            // Create a list of tasks to fetch cities concurrently
            var tasks = states.Select(async state =>
            {
                var cities = await stateCityService.FetchCitiesAsync(state.StateCode); // Fetch cities for the current state
                foreach (City city in cities)
                {
                    city.StateId = state.Id; // Assign the stateId to the city
                }
                return cities; // Return the list of cities
            }).ToList();

            // Wait for all tasks to complete
            var citiesList = await Task.WhenAll(tasks);

            // Flatten the list of lists into a single list of cities
            var allCities = citiesList.SelectMany(cities => cities).ToList();

            // Save all cities to the database
            await context.Cities.AddRangeAsync(allCities);
            await context.SaveChangesAsync();
        }

    }
}
