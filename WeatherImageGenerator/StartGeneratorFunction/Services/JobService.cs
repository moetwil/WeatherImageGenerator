using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;

namespace StartGeneratorFunction.Services;

public class JobService
{
    private readonly ILogger<JobService> _logger;
    private readonly QueueClient _queueClient;
    
    public JobService(ILogger<JobService> logger, QueueClient queueClient)
    {
        _logger = logger;
        _queueClient = queueClient;
    }
    
    public async Task<Guid> StartJobAsync()
    {
        _logger.LogInformation("Starting job...");
        var jobId = Guid.NewGuid();
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
}