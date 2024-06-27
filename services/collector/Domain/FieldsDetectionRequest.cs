namespace collector.Domain;

public class FieldsDetectionRequest
{ 
    public string? RequestId { get; set; }
    public string? BucketName { get; set; }
    public string? ObjectName { get; set; }
    public string? DocumentName { get; set; }
}