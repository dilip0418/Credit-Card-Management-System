using CCMS3.Eceptions;
using CCMS3.Models;
using CCMS3.Services;
using Microsoft.AspNetCore.Mvc;

namespace CCMS3.Controllers
{
    public static class StateCityEndpoints
    {
        public static IEndpointRouteBuilder MapStateCityEnpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/states", GetStates);
            app.MapGet("/cities", GetCitiesByState);
            return app;
        }

        public static ApiResponse<IEnumerable<StateResponse>> GetStates(StateCityService service)
        {
            try
            {
                var states = service.GetStates();
                if (states.Any())
                {
                    return new ApiResponse<IEnumerable<StateResponse>>(StatusCodes.Status200OK, states, "Successfully retrieved", states.Count());
                }
                else
                {
                    return new ApiResponse<IEnumerable<StateResponse>>(StatusCodes.Status204NoContent, []);
                }
            }
            catch(EntityNotFoundException e)
            {
                return new ApiResponse<IEnumerable<StateResponse>>(StatusCodes.Status400BadRequest, [e.Message]);
            }
            catch (Exception e)
            {
                return new ApiResponse<IEnumerable<StateResponse>>(StatusCodes.Status500InternalServerError, [e.Message]);
            }
        }

        public static ApiResponse<IEnumerable<CityResponse>> GetCitiesByState(
            [FromQuery] int stateId,
            StateCityService service)
        {
            try
            {
                var cities = service.GetCities(stateId);
                if (cities.Any())
                {
                    return new ApiResponse<IEnumerable<CityResponse>>(StatusCodes.Status200OK, cities, "Succesfully retrieved", cities.Count());
                }
                else
                {
                    return new ApiResponse<IEnumerable<CityResponse>>(StatusCodes.Status204NoContent, []);
                }
            }
            catch(InvalidOperationException e)
            {
                return new ApiResponse<IEnumerable<CityResponse>>(StatusCodes.Status204NoContent, [e.Message]);
            }
            catch (Exception e)
            {
                return new ApiResponse<IEnumerable<CityResponse>>(StatusCodes.Status500InternalServerError, [e.Message]);
            }
        }
    }
}
