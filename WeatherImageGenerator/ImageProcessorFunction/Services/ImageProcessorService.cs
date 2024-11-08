using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using ImageProcessorFunction.DTOs;
using Microsoft.Extensions.Logging;

namespace ImageProcessorFunction.Services;

public class ImageProcessorService
{
    private readonly ILogger<ImageProcessorService> _logger;
    private readonly ImageEditService _imageEditService;
    private readonly BlobStorageService _blobStorageService;
    private readonly HttpClient _httpClient;
    private readonly TableClient _tableClient;

    public ImageProcessorService(ILogger<ImageProcessorService> logger, ImageEditService imageEditService,
        BlobStorageService blobStorageService, HttpClient httpClient, TableClient tableClient)
    {
        _logger = logger;
        _imageEditService = imageEditService;
        _blobStorageService = blobStorageService;
        _httpClient = httpClient;
        _tableClient = tableClient;
    }

    public async Task ProcessImageAsync(string message)
    {
        var imageInfo = MapMessageToImageInfo(message);
        var imageStream = await FetchImageStreamAsync(imageInfo.ImageUrl);
        var texts = CreateOverlayTexts(imageInfo.Station);

        using var editedImageStream = _imageEditService.OverlayTextOnImage(imageStream, texts);

        await _blobStorageService.SaveImageAsync(editedImageStream, imageInfo.JobId, imageInfo.Station.StationName);
        
        _logger.LogInformation($"Image processed and saved for station {imageInfo.Station.StationName}");
        
        var imageCount = await _blobStorageService.GetJobImageCount(imageInfo.JobId.ToString());
        
        var jobStatus = await FetchJobStatusAsync(imageInfo.JobId.ToString());
        
        _logger.LogInformation($"Image count: {imageCount}, Total stations: {jobStatus?.TotalStations}");
        
        if (imageCount == jobStatus?.TotalStations)
        {
            _logger.LogInformation($"All images processed for job {imageInfo.JobId}");
            await UpdateJobStatusAsync(imageInfo.JobId.ToString(), "Completed");
        }
    }

    private (string text, (float x, float y) position, int fontSize, string colorHex)[] CreateOverlayTexts(
        StationDTO station)
    {
        return new[]
        {
            (station.StationName, (10f, 10f), 20, "#FFFFFF"),
            ($"Temperature: {station.Temperature}°C", (10f, 40f), 20, "#FFFFFF"),
            ($"Humidity: {station.Humidity}%", (10f, 70f), 20, "#FFFFFF"),
            ($"Wind: {station.WindDirectionDegrees}°", (10f, 100f), 20, "#FFFFFF"),
            ($"Precipitation: {station.Precipitation}mm", (10f, 130f), 20, "#FFFFFF"),
            ($"Sun power: {station.SunPower}", (10f, 160f), 20, "#FFFFFF"),
            ($"Rainfall last 24h: {station.RainFallLast24Hour}mm", (10f, 190f), 20, "#FFFFFF"),
            ($"Rainfall last hour: {station.RainFallLastHour}mm", (10f, 220f), 20, "#FFFFFF")
        };
    }

    private async Task<Stream> FetchImageStreamAsync(string imageUrl)
    {
        try
        {
            var response = await _httpClient.GetAsync(imageUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError($"HTTP request error while fetching image: {httpEx.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error while fetching image: {ex.Message}");
            return null;
        }
    }

    private ImageInfoDTO MapMessageToImageInfo(string message)
    {
        try
        {
            var imageInfo = JsonSerializer.Deserialize<ImageInfoDTO>(message);
            return imageInfo;
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError($"JSON deserialization error: {jsonEx.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error: {ex.Message}");
            throw;
        }
    }
    
    private async Task<JobStatus?> FetchJobStatusAsync(string jobId)
    {
        try
        {
            const string partitionKey = "JobPartition";
            _logger.LogInformation($"Fetching job status for JobId: {jobId} from Table Storage.");

            var jobStatusEntity = await _tableClient.GetEntityIfExistsAsync<JobStatus>(partitionKey, jobId);
            if (jobStatusEntity.HasValue)
            {
                var jobStatus = jobStatusEntity.Value;
                return jobStatus;
            }
            else
            {
                _logger.LogWarning($"Job {jobId} not found");
                return null;
            }
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError($"Error fetching job {jobId}: {ex.Message}");
            return null;
        }
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
                jobStatus.CompletedTime = DateTime.UtcNow;

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