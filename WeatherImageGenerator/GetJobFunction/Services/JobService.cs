using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GetJobFunction.Services;

public class JobService
{
    private readonly ILogger<JobService> _logger;
    private readonly TableClient _tableClient;
    private readonly BlobService _blobService;

    public JobService(ILogger<JobService> logger, TableClient tableClient, BlobService blobService)
    {
        _logger = logger;
        _tableClient = tableClient;
        _blobService = blobService;
    }

    public async Task<IActionResult> GetJobAsync(string jobId)
    {
        var jobStatus = await FetchJobStatusAsync(jobId);
        if (jobStatus == null)
        {
            return new NotFoundObjectResult(new { Status = "NotFound", Message = $"Job {jobId} not found" });
        }

        var imageCount = await _blobService.GetJobImageCount(jobId);
        _logger.LogInformation($"Found {imageCount} images for job {jobId}");

        if (jobStatus.TotalStations != imageCount)
        {
            return new OkObjectResult(new { Status = jobStatus.Status, Message = "The job has not finished yet" });
        }
        
        var imageUrls = await _blobService.GetImageUrls(jobId);

        return new OkObjectResult(new { Status = "Completed", ImageUrls = imageUrls });
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

    private string CleanSasToken(string sasToken)
    {
        return sasToken
            .Replace("\\u0026", "&")
            .Replace("\\u003D", "=")
            .Replace("\\u002F", "/");
    }
}