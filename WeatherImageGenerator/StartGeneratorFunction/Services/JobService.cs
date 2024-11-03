using System.Text;
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
        var jobId = Guid.NewGuid().ToString(); // Create a unique job ID
        await AddJobStatusToTableAsync(jobId, "Created");
        await AddJobToQueueAsync(jobId);
        return Guid.Parse(jobId); // Return as Guid if needed
    }
    
    private async Task AddJobToQueueAsync(string jobId)
    {
        _logger.LogInformation("Adding job to queue...");
        await _queueClient.CreateIfNotExistsAsync();
        var message = jobId; 
        var bytes = Encoding.UTF8.GetBytes(message);
        await _queueClient.SendMessageAsync(Convert.ToBase64String(bytes));
        _logger.LogInformation("Job added to queue.");
    }
    
    private async Task AddJobStatusToTableAsync(string jobId, string status)
    {
        _logger.LogInformation($"Updating job status in Table Storage for Job ID: {jobId}");
        
        var jobStatus = new JobStatus
        {
            JobId = jobId, // Set the unique job ID
            Status = status,
            CreatedTime = DateTime.UtcNow,
        };

        await _tableClient.CreateIfNotExistsAsync();
        await _tableClient.AddEntityAsync(jobStatus);
        _logger.LogInformation($"Job status updated to {status} in Table Storage.");
    }
}