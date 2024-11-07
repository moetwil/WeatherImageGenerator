using Azure.Data.Tables;
using Azure.Storage.Blobs;
using ImageProcessorFunction.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient();
        
        // Table Storage
        var tableConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var tableName = Environment.GetEnvironmentVariable("JobStatusTableName") ?? "JobStatus";
        services.AddSingleton(new TableClient(tableConnectionString, tableName));

        var blobServiceClient = new BlobServiceClient(context.Configuration["AzureWebJobsStorage"]);
        services.AddSingleton(blobServiceClient);

        string containerName = context.Configuration["ImageContainerName"];

        // Register the BlobStorageService
        services.AddScoped<BlobStorageService>(provider =>
            new BlobStorageService(provider.GetRequiredService<BlobServiceClient>(), containerName,
                provider.GetRequiredService<ILogger<BlobStorageService>>()));

        // Register the ImageProcessorService
        services.AddScoped<ImageEditService>();
        services.AddScoped<ImageProcessorService>();

        // Application Insights telemetry
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();