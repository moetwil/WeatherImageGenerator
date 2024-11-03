using System.Text.Json;
using Azure.Data.Tables;
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
    private readonly TableClient _tableClient;

    public JobProcessorService(
        ILogger<JobProcessorService> logger,
        WeatherClient weatherClient,
        ImageClient imageClient,
        QueueClient imageQueueClient,
        TableClient tableClient)
    {
        _logger = logger;
        _weatherClient = weatherClient;
        _imageClient = imageClient;
        _imageQueueClient = imageQueueClient;
        _tableClient = tableClient;
    }

    public async Task ProcessJobAsync(string jobId)
    {
        _logger.LogInformation($"Processing job {jobId}");
        await UpdateJobStatusAsync(jobId, "Processing");
        
        var weatherData = await _weatherClient.GetWeatherDataAsync(jobId);
        _logger.LogInformation($"Weather data received for job {jobId}");

        foreach (var station in weatherData.Actual.StationMeasurements)
        {
            _logger.LogInformation($"Processing station {station.StationId}");
            await ProcessWeatherStation(jobId, station);
        }
        
        await UpdateJobStatusAsync(jobId, "Completed");
        _logger.LogInformation($"Job {jobId} completed");
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
    
    private async Task UpdateJobStatusAsync(string jobId, string status)
    {
        try
        {
            var existingJob = await _tableClient.GetEntityIfExistsAsync<JobStatus>("JobPartition", jobId);
            
            
            if (existingJob.HasValue)
            {
                var jobStatus = existingJob.Value;
                jobStatus.Status = status;

                await _tableClient.UpsertEntityAsync(jobStatus);
                _logger.LogInformation($"Job {jobId} status updated to {status}");
            }
            else
            {
                _logger.LogWarning($"Job {jobId} not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job status");
            throw;
        }
    }
}
