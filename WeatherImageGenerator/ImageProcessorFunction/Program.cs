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

        // Retrieve the connection string using the configuration context
        var blobServiceClient = new BlobServiceClient(context.Configuration["AzureWebJobsStorage"]);
        services.AddSingleton(blobServiceClient);

        // Specify the container name you want to use
        string containerName = context.Configuration["ImageContainerName"];

        // Register the BlobStorageService with the container name
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