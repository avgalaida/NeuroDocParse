namespace collector.Domain;

public class DocumentDetectionRequest
{ 
    public string? ClientId { get; set; }
    public string? BucketName { get; set; }
    public string? ObjectName { get; set; }
    public string? Model { get; set; }
}