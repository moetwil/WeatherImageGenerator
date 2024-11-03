using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;

namespace StartGeneratorFunction.Services;

public class JobService
{
    private readonly ILogger<JobService> _logger;
    private readonly QueueClient _queueClient;
    private readonly TableClient _tableClient;
    
    public JobService(ILogger<JobService> logger, QueueClient queueClient, TableClient tableClient)
    {
        _logger = logger;
        _queueClient = queueClient;
        _tableClient = tableClient;
    }
    
    public async Task<Guid> StartJobAsync()
    {
        _logger.LogInformation("Starting job...");
        var jobId = Guid.NewGuid();
        await AddJobStatusToTableAsync(jobId, "InProgress");
        await AddJobToQueueAsync(jobId);
        return jobId;
    }
    
    private async Task AddJobToQueueAsync(Guid jobId)
    {
        _logger.LogInformation("Adding job to queue...");
        await _queueClient.CreateIfNotExistsAsync();
        var message = jobId.ToString();
        await _queueClient.SendMessageAsync(message);
        _logger.LogInformation("Job added to queue.");
    }
    
    private async Task AddJobStatusToTableAsync(Guid jobId, string status)
    {
        _logger.LogInformation($"Updating job status in Table Storage for Job ID: {jobId}");
        var jobStatus = new JobStatus
        {
            RowKey = jobId.ToString(),
            Status = status,
            CreatedTime = DateTime.UtcNow,
            Timestamp = DateTimeOffset.UtcNow
        };

        await _tableClient.CreateIfNotExistsAsync();
        await _tableClient.AddEntityAsync(jobStatus);
        _logger.LogInformation($"Job status updated to {status} in Table Storage.");
    }
}