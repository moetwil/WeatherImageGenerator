using System.Text.Json;
using Azure.Data.Tables;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;

namespace BlobActionFunction.Services;

public class BlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly TableClient _tableClient;
    private readonly ILogger<BlobService> _logger;
    private readonly string _containerName;
    private readonly string _accountName;
    private readonly string _accountKey;
    private readonly string _baseUrl;
    

    public BlobService(BlobServiceClient blobServiceClient, ILogger<BlobService> logger, string containerName, TableClient tableClient, string accountName, string accountKey, string baseUrl)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
        _containerName = containerName;
        _tableClient = tableClient;
        _accountName = accountName;
        _accountKey = accountKey;
        _baseUrl = baseUrl;
    }

    public async Task ProcessBlobAsync(string name)
    {
        // Extract jobId from the blob name
        var parts = name.Split('/');
        if (parts.Length > 1)
        {
            string jobId = parts[0]; // The jobId is the first part
            string blobName = parts[1]; // The blobName is the second part

            // Generate SAS token for the blob
            var sasToken = GenerateSasToken(name);
            _logger.LogInformation(sasToken);
        
            // Update job status with the extracted jobId
            await UpdateJobStatusAsync(jobId, sasToken);
        }
        else
        {
            _logger.LogWarning("Invalid blob name format: {BlobName}. Unable to extract jobId.", name);
        }
    }

    private string GenerateSasToken(string blobName)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

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
        
        return $"{_baseUrl}/{_accountName}/{_containerName}/{blobName}?{sasToken}";
    }
    
    private async Task UpdateJobStatusAsync(string jobId, string sasToken)
    {
        try
        {
            var existingJob = await _tableClient.GetEntityIfExistsAsync<JobStatus>("JobPartition", jobId);
            
            if (existingJob.HasValue)
            {
                var jobStatus = existingJob.Value;

                // Deserialize the existing ImageUrls into a string array
                var imageUrls = string.IsNullOrEmpty(jobStatus.ImageUrls) || jobStatus.ImageUrls == "[]" 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(jobStatus.ImageUrls) ?? new List<string>();

                // Add the new SAS token to the list
                imageUrls.Add(sasToken);

                // Serialize the updated list back to JSON
                jobStatus.ImageUrls = JsonSerializer.Serialize(imageUrls);

                await _tableClient.UpsertEntityAsync(jobStatus);
                _logger.LogInformation($"SasToken updated for Job {jobId}");
            }
            
            // if (existingJob.HasValue)
            // {
            //     var jobStatus = existingJob.Value;
            //     jobStatus.ImageUrls = sasToken;
            //
            //     await _tableClient.UpsertEntityAsync(jobStatus);
            // }
            else
            {
                _logger.LogWarning($"Job {jobId} not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job status");
            throw;
        }
    }

    
}