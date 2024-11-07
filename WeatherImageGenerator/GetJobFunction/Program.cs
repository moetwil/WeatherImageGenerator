using Azure.Data.Tables;
using Azure.Storage.Blobs;
using GetJobFunction.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddScoped<JobService>();

        // Table Storage
        var tableConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var tableName = Environment.GetEnvironmentVariable("JobStatusTableName") ?? "JobStatus";
        services.AddSingleton(new TableClient(tableConnectionString, tableName));
        
        string containerName = context.Configuration["ImageContainerName"];
        string accountName = context.Configuration["AccountName"];
        string accountKey = context.Configuration["AccountKey"];
        string baseUrl = context.Configuration["BaseUrl"];
        
        // Register BlobService with BlobServiceClient, ILogger<BlobService>, and container name
        services.AddScoped<BlobService>(provider =>
        {
            var blobServiceClient = provider.GetRequiredService<BlobServiceClient>();
            var tableClient = provider.GetRequiredService<TableClient>();
            var logger = provider.GetRequiredService<ILogger<BlobService>>();
            return new BlobService(blobServiceClient, logger, containerName, tableClient, accountName, accountKey,
                baseUrl);
        });
        
        // Register BlobServiceClient using the connection string from configuration
        string blobConnectionString = context.Configuration["AzureWebJobsStorage"];
        services.AddSingleton(new BlobServiceClient(blobConnectionString));

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();