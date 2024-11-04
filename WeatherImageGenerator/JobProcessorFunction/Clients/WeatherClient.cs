using System.Net.Http.Json;
using JobProcessorFunction.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JobProcessorFunction.Clients;

public class WeatherClient
{
    private readonly ILogger<WeatherClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;

    public WeatherClient(ILogger<WeatherClient> logger, HttpClient httpClient, IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClient;
        _endpoint = configuration["WeatherEndpoint"];
    }

    public async Task<WeatherDataDTO> GetWeatherDataAsync(string jobId)
    {
        try
        {
            var response = await _httpClient.GetAsync(_endpoint);
            response.EnsureSuccessStatusCode();

            var weatherData = await response.Content.ReadFromJsonAsync<WeatherDataDTO>();
            _logger.LogInformation(weatherData.Actual.StationMeasurements.Count.ToString());
            return weatherData;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Error fetching weather data: {ex.Message}");
            throw;
        }
    }
}