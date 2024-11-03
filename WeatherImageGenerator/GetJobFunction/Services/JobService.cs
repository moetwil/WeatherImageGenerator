using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GetJobFunction.Services;

public class JobService
{
    private readonly ILogger<JobService> _logger;
    
    public JobService(ILogger<JobService> logger)
    {
        _logger = logger;
    }
    
    public async Task<IActionResult> GetJobAsync()
    {
        _logger.LogInformation("GetJobAsync called");
        
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}