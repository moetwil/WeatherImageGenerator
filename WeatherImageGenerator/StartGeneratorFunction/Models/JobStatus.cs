using Azure;
using Azure.Data.Tables;
using System;
using System.Text.Json;

public class JobStatus : ITableEntity
{
    public string PartitionKey { get; set; } = "JobPartition"; // Set a constant partition key
    public string RowKey { get; set; } // Unique identifier for the job
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; } = ETag.All;

    public string JobId
    {
        get => RowKey;
        set => RowKey = value;
    }
    
    public string Status { get; set; } 
    public DateTime CreatedTime { get; set; } 
    public DateTime? CompletedTime { get; set; }
    
    public string ImageUrls { get; set; } = string.Empty;
}