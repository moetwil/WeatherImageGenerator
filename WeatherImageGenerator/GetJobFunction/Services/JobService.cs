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
        
        if (new[] { "Created", "InProgress" }.Contains(jobStatus.Status))
        {
            return new OkObjectResult(new { Status = jobStatus.Status, Message = "The job has not finished yet" });
        }

        return new OkObjectResult(new { Status = "Completed", ImageUrls = jobStatus.ImageUrls });
    }
    
    private async Task<JobStatus?> FetchJobStatusAsync(string jobId)
    {
        try
        {
            const string partitionKey = "JobPartition";
            _logger.LogInformation($"Fetching job status for JobId: {jobId} from Table Storage.");

            var jobStatusEntity = await _tableClient.GetEntityIfExistsAsync<JobStatus>(partitionKey, jobId);
            return jobStatusEntity.HasValue ? jobStatusEntity.Value : null;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError($"Error fetching job {jobId}: {ex.Message}");
            return null;
        }
    }
}