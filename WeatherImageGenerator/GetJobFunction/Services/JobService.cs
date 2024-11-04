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

    public JobService(ILogger<JobService> logger, TableClient tableClient)
    {
        _logger = logger;
        _tableClient = tableClient;
    }

    public async Task<IActionResult> GetJobAsync(string jobId)
    {
        var jobStatus = await FetchJobStatusAsync(jobId);

        if (jobStatus == null)
        {
            return new NotFoundObjectResult(new { Status = "NotFound", Message = $"Job {jobId} not found" });
        }

        if (new[] { "Created", "InProgress" }.Contains(jobStatus.Status))
        {
            return new OkObjectResult(new { Status = jobStatus.Status, Message = "The job has not finished yet" });
        }

        var imageUrls = string.IsNullOrEmpty(jobStatus.ImageUrls)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(jobStatus.ImageUrls) ?? new List<string>();

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

                if (!string.IsNullOrEmpty(jobStatus.ImageUrls))
                {
                    var imageUrls = JsonSerializer.Deserialize<List<string>>(jobStatus.ImageUrls);
                    if (imageUrls != null)
                    {
                        _logger.LogInformation($"Image URLs before cleanup: {JsonSerializer.Serialize(imageUrls)}");

                        for (int i = 0; i < imageUrls.Count; i++)
                        {
                            imageUrls[i] = CleanSasToken(imageUrls[i]);
                        }

                        jobStatus.ImageUrls = JsonSerializer.Serialize(imageUrls);
                    }
                }

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