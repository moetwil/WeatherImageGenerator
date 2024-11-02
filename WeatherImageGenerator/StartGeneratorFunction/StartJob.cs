using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartGeneratorFunction.Services;

namespace StartGeneratorFunction;

public class StartJob
{
    private readonly ILogger<StartJob> _logger;
    private readonly JobService _jobService;

    public StartJob(ILogger<StartJob> logger, JobService jobService)
    {
        _logger = logger;
        _jobService = jobService;
    }

    [Function("StartJob")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("Request received to start a job.");
        var jobId = await _jobService.StartJobAsync();
        _logger.LogInformation($"Job started with ID: {jobId}");
        return new OkObjectResult(new JobDTO { JobId = jobId.ToString() });
    }

}