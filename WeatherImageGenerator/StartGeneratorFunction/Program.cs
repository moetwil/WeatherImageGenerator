using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using StartGeneratorFunction.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddScoped<JobService>();
        
        // Queue
        var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var queueName = Environment.GetEnvironmentVariable("QueueName");
        services.AddSingleton(new QueueClient(connectionString, queueName));
        
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();