using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Azure.Storage.Queues; // Make sure to include this
using JobProcessorFunction.Clients;
using JobProcessorFunction.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Register HttpClient
        services.AddHttpClient<WeatherClient>();
        services.AddHttpClient<ImageClient>();

        // Register your JobProcessorService
        services.AddScoped<JobProcessorService>(); 

        // Register the ImageQueueClient
        var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var imageQueueName = Environment.GetEnvironmentVariable("ImageQueue");
        var imageQueueClient = new QueueClient(connectionString, imageQueueName);
        imageQueueClient.CreateIfNotExists();
        services.AddSingleton(imageQueueClient);
    })
    .Build();

host.Run();