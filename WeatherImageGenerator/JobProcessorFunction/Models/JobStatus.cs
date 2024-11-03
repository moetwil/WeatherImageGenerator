using Azure;
using Azure.Data.Tables;
using System;

public class JobStatus : ITableEntity
{
    public string PartitionKey { get; set; } = "JobPartition";
    public string RowKey { get; set; }
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
    public string ImageUrls { get; set; }

}