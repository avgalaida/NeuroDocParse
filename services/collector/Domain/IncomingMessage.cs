namespace collector.Domain;

public class IncomingMessage
{
    public string? ClientId { get; set; }
    public string? RequestType { get; set; }
    public string? BucketName { get; set; }
    public string? ObjectName { get; set; }
    public string? Model { get; set; }
}