using CCMS3.Data;
using CCMS3.Eceptions;
using CCMS3.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace CCMS3.Services
{

    public class StateResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CityResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class StateCityService
    {
        private const string API = "https://country-state-city-search-rest-api.p.rapidapi.com/";
        private const string API_HOST = "country-state-city-search-rest-api.p.rapidapi.com";
        private const string getByState = "states-by-countrycode";
        private const string getByCity = "cities-by-countrycode-and-statecode";

        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly AppDbContext _context;

        public StateCityService(
            HttpClient httpClient, 
            IConfiguration configuration,
            AppDbContext context)
        {
            _httpClient = httpClient;
            _apiKey = configuration["RapidApiKey"]; // Using binding to access API key
            _context = context;
        }

        public async Task<IEnumerable<State>> FetchStatesAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                API + "" + getByState + "?countrycode=in");

            request.Headers.Add("x-rapidapi-host", API_HOST);
            request.Headers.Add("x-rapidapi-key", _apiKey); // API key added from config

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var apiStates = System.Text.Json.JsonSerializer.Deserialize<List<ApiState>>(json);


            IList<string> states = ["Karnataka", "Tamil Nadu", "Kerala", "Andhra Pradesh", "Telangana"];
            // Map API data to State entity
            return apiStates
                .Where(state => states.Contains(state.name))
                .Select(apiState => new State
                {
                    Name = apiState.name,
                    StateCode = apiState.isoCode
                }).ToList();
        }

        public async Task<IEnumerable<City>> FetchCitiesAsync(string stateCode)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
        API + getByCity + "?countrycode=in&statecode=" + stateCode);

            request.Headers.Add("x-rapidapi-host", API_HOST);
            request.Headers.Add("x-rapidapi-key", _apiKey); // API key added from config

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var apiCities = System.Text.Json.JsonSerializer.Deserialize<List<ApiCity>>(json); // Assume you have a DTO for cities

            // Map API data to City entity
            return apiCities.Select(apiCity => new City
            {
                Name = apiCity.name, // Ensure this matches the JSON response key
                                         // StateId will be set later when storing in the database
            }).ToList();
        }


        public IEnumerable<StateResponse> GetStates()
        {
            try
            {
                var states = _context.States.Select(s => new StateResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                });
                return states;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        public IEnumerable<CityResponse> GetCities(int stateId)
        {
            var state = _context.States
                .Include(c => c.Cities)
                .First(s => s.Id == stateId) ?? throw new EntityNotFoundException("State not found!");

            return state.Cities.Select(c => new CityResponse
            {
                Id= c.Id,
                Name = c.Name,
            }); 
        }
    }
}
