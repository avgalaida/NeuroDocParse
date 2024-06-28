using System;

namespace collector.Domain
{
    public class RequestHistory
    {
        public string? RequestId { get; set; }
        public string?  UserId { get; set; }
        public string? RequestType { get; set; }
        public string? BucketName { get; set; }
        public string? ObjectName { get; set; }
        public string? ResultJson { get; set; }
        public DateTime Timestamp { get; set; }
    }
}