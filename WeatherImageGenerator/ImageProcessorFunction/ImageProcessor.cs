using Azure.Storage.Queues.Models;
using ImageProcessorFunction.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ImageProcessorFunction;

public class ImageProcessor
{
    private readonly ILogger<ImageProcessor> _logger;
    private readonly string _imageQueue;
    private readonly ImageProcessorService _imageProcessorService;



    public ImageProcessor(ILogger<ImageProcessor> logger, ImageProcessorService imageProcessorService, IConfiguration configuration)
    {
        _logger = logger;
        _imageProcessorService = imageProcessorService;
        _imageQueue = configuration["ImageQueue"];
    }

    [Function(nameof(ImageProcessor))]
    public async Task Run([QueueTrigger("%ImageQueue%", Connection = "")] QueueMessage message)
    {
        _logger.LogInformation("Queue triggered");
        await _imageProcessorService.ProcessImageAsync(message.MessageText);
    }
}