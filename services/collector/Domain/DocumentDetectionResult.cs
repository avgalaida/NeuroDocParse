namespace collector.Domain;

public class DocumentDetectionResult
{ 
    public string? ClientId { get; set; }
    public string? BucketName { get; set; }
    public string? ObjectName { get; set; }
    public string? DocumentName { get; set; }
}