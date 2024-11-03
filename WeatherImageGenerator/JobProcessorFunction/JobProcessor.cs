using Azure.Storage.Queues.Models;
using JobProcessorFunction.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JobProcessorFunction;

public class JobProcessor
{
    private readonly ILogger<JobProcessor> _logger;
    private readonly JobProcessorService _jobProcessorService;
    private readonly string _jobQueueName;
    
    public JobProcessor(ILogger<JobProcessor> logger, JobProcessorService jobProcessorService, IConfiguration configuration)
    {
        _logger = logger;
        _jobProcessorService = jobProcessorService;
        _jobQueueName = configuration["JobQueueName"];
    }

    [Function(nameof(JobProcessor))]
    public async Task Run([QueueTrigger("%JobQueueName%", Connection = "")] QueueMessage message)
    {
        _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");
        await _jobProcessorService.ProcessJobAsync(message.MessageText);
    }
}