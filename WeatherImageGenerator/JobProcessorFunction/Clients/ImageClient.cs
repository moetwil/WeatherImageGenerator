using JobProcessorFunction.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JobProcessorFunction.Clients;

public class ImageClient
{
    private readonly ILogger<WeatherClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    
    public ImageClient(ILogger<WeatherClient> logger, HttpClient httpClient, IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClient;
        _endpoint = configuration["ImageEndpoint"];
    }
    
    public async Task<string> GetRandomImageUrlAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(_endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var finalUrl = response.RequestMessage.RequestUri.ToString();
                return finalUrl; // Return the URL of the image
            }
            else
            {
                _logger.LogWarning($"Failed to fetch image. Status code: {response.StatusCode}");
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"An error occurred while fetching the random image URL: {ex.Message}");
            return null;
        }
    }
}