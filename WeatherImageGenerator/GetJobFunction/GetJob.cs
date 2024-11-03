using GetJobFunction.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GetJobFunction;

public class GetJob
{
    private readonly ILogger<GetJob> _logger;
    private readonly JobService _jobService;

    public GetJob(ILogger<GetJob> logger, JobService jobService)
    {
        _logger = logger;
        _jobService = jobService;
    }

    [Function("GetJob")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "jobs/{jobId}")] HttpRequest req, string jobId)
    {
        _logger.LogInformation("Get Job request received for job ID: {jobId}", jobId);
        var res = await _jobService.GetJobAsync();
        return new OkObjectResult("Welcome to Azure Functions!");
        
    }

}