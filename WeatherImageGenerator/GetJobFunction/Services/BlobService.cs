using System.Text.Json;
using Azure.Data.Tables;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;

namespace GetJobFunction.Services;

public class BlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;
    private readonly TableClient _tableClient;
    private readonly ILogger<BlobService> _logger;
    private readonly string _containerName;
    private readonly string _accountName;
    private readonly string _accountKey;
    private readonly string _baseUrl;


    public BlobService(BlobServiceClient blobServiceClient, ILogger<BlobService> logger, string containerName,
        TableClient tableClient, string accountName, string accountKey, string baseUrl)
    {
        _blobServiceClient = blobServiceClient;
        _containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        _logger = logger;
        _containerName = containerName;
        _tableClient = tableClient;
        _accountName = accountName;
        _accountKey = accountKey;
        _baseUrl = baseUrl;
    }

    public async Task<int> GetJobImageCount(string jobId)
    {
        // Define the path to list only blobs in /weather-images/{jobId}/
        string prefix = $"{jobId}/";
        int count = 0;

        await foreach (var blobItem in _containerClient.GetBlobsAsync(prefix: prefix))
        {
            count++;
        }

        return count;
    }
    
    public async Task<List<string>> GetImageUrls(string jobId)
    {
        // Access the container (assumes the container is named "weather-images")
        // BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

        // Define the path to list only blobs in /weather-images/{jobId}/
        string prefix = $"{jobId}/";
        List<string> imageUrls = new List<string>();

        await foreach (var blobItem in _containerClient.GetBlobsAsync(prefix: prefix))
        {
            imageUrls.Add(GenerateSasToken(blobItem.Name));
        }

        return imageUrls;
    }

    private string GenerateSasToken(string blobName)
    {
        // Create the SAS token
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerName,
            BlobName = blobName,
            ExpiresOn = DateTimeOffset.UtcNow.AddYears(100)
        };

        // Set the permissions (read)
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        // Generate the SAS token
        string sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(
            _accountName,
            _accountKey
        )).ToString();

        // Check if the environment is Azure or local
        bool isAzure = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME"));

        // Determine the base URL based on environment
        string baseUrl = isAzure ? $"https://{_accountName}.blob.core.windows.net/" : _baseUrl;

        // Return the local development URL
        if (!isAzure)
        {
            return $"{_baseUrl}/{_accountName}/{_containerName}/{blobName}?{sasToken}";
        }

        // Return the Azure URL
        return $"{baseUrl}{_containerName}/{blobName}?{sasToken}";
    }
}