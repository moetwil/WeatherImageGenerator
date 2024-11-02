using System.Text.Json;
using Azure.Storage.Queues;
using JobProcessorFunction.Clients;
using JobProcessorFunction.DTOs;
using Microsoft.Extensions.Logging;

namespace JobProcessorFunction.Services;

public class JobProcessorService
{
    private readonly ILogger<JobProcessorService> _logger;
    private readonly WeatherClient _weatherClient;
    private readonly ImageClient _imageClient;
    private readonly QueueClient _imageQueueClient;

    public JobProcessorService(
        ILogger<JobProcessorService> logger,
        WeatherClient weatherClient,
        ImageClient imageClient,
        QueueClient imageQueueClient)
    {
        _logger = logger;
        _weatherClient = weatherClient;
        _imageClient = imageClient;
        _imageQueueClient = imageQueueClient;
    }

    public async Task ProcessJobAsync(string jobId)
    {
        _logger.LogInformation($"Processing job {jobId}");
        var weatherData = await _weatherClient.GetWeatherDataAsync(jobId);
        _logger.LogInformation($"Weather data received for job {jobId}");

        foreach (var station in weatherData.Actual.StationMeasurements)
        {
            _logger.LogInformation($"Processing station {station.StationId}");
            await ProcessWeatherStation(jobId, station);
        }
    }

    private async Task ProcessWeatherStation(string jobId, StationMeasurementDTO station)
    {
        var imageUrl = await _imageClient.GetRandomImageUrlAsync();
        await AddImageToQueueAsync(Guid.Parse(jobId), station, imageUrl);
    }

    private async Task AddImageToQueueAsync(Guid jobId, StationMeasurementDTO station, string imageUrl)
    {
        _logger.LogInformation("Adding image to queue...");
        await _imageQueueClient.CreateIfNotExistsAsync();
        var message = JsonSerializer.Serialize(new { JobId = jobId, Station = station, ImageUrl = imageUrl });
        await _imageQueueClient.SendMessageAsync(message);
        _logger.LogInformation("Image added to queue.");
    }
}
