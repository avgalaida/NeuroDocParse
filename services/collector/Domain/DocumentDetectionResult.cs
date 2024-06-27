namespace collector.Domain;

public class DocumentDetectionResult
{ 
    public string? RequestId { get; set; }
    public string? BucketName { get; set; }
    public string? ObjectName { get; set; }
    public string? DocumentName { get; set; }
}