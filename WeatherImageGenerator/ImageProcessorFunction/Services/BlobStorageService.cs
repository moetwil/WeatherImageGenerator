using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace ImageProcessorFunction.Services;

public class BlobStorageService
{
    private readonly ILogger<BlobStorageService> _logger;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public BlobStorageService(BlobServiceClient blobServiceClient, string containerName,
        ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = containerName;
        _logger = logger;
    }

    public async Task<string> SaveImageAsync(Stream imageStream, Guid jobId, string imageFileName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            // Create the blob name with the desired path
            string blobName = $"{jobId}/{imageFileName.Replace(" ", "")}.png";
            var blobClient = containerClient.GetBlobClient(blobName);

            // Upload the image stream to the blob
            await blobClient.UploadAsync(imageStream, new BlobHttpHeaders { ContentType = "image/png" });

            // Return the URI of the uploaded blob
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving image to blob storage: {ex.Message}");
            throw; // Re-throw the exception after logging
        }
    }
}