using Azure.Storage.Queues;
using Azure.Data.Tables;
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
        var queueConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var queueName = Environment.GetEnvironmentVariable("QueueName");
        services.AddSingleton(new QueueClient(queueConnectionString, queueName));

        // Table Storage
        var tableConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var tableName = Environment.GetEnvironmentVariable("JobStatusTableName") ?? "JobStatus";
        services.AddSingleton(new TableClient(tableConnectionString, tableName));

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();