using System;
using System.IO;
using System.Threading.Tasks;
using BlobActionFunction.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BlobActionFunction;

public class BlobAction
{
    private readonly ILogger<BlobAction> _logger;
    private readonly BlobService _blobService;

    public BlobAction(ILogger<BlobAction> logger, BlobService blobService)
    {
        _logger = logger;
        _blobService = blobService;
    }

    [Function(nameof(BlobAction))]
    public async Task Run([BlobTrigger("weather-images/{name}", Connection = "")] Stream stream, string name)
    {
        _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name}");
        await _blobService.ProcessBlobAsync(name);
        
    }
}