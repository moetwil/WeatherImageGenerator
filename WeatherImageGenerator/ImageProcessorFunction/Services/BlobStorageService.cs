using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace ImageProcessorFunction.Services;

public class BlobStorageService
{
    private readonly ILogger<BlobStorageService> _logger;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;
    private readonly string _containerName;

    public BlobStorageService(BlobServiceClient blobServiceClient, string containerName,
        ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = containerName;
        _containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

        _logger = logger;
    }

    public async Task SaveImageAsync(Stream imageStream, Guid jobId, string imageFileName)
    {
        try
        {
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            // Create the blob name with the desired path
            string blobName = $"{jobId}/{imageFileName.Replace(" ", "")}.png";
            var blobClient = _containerClient.GetBlobClient(blobName);

            // Upload the image stream to the blob
            await blobClient.UploadAsync(imageStream, new BlobHttpHeaders { ContentType = "image/png" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving image to blob storage: {ex.Message}");
            throw;
        }
    }
    
    public async Task<int> GetJobImageCount(string jobId)
    {
        string prefix = $"{jobId}/";
        int count = 0;

        await foreach (var blobItem in _containerClient.GetBlobsAsync(prefix: prefix))
        {
            count++;
        }

        return count;
    }
}