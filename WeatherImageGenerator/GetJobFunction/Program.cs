using Azure.Data.Tables;
using GetJobFunction.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddScoped<JobService>();

        // Table Storage
        var tableConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var tableName = Environment.GetEnvironmentVariable("JobStatusTableName") ?? "JobStatus";
        services.AddSingleton(new TableClient(tableConnectionString, tableName));

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();