namespace collector.Broker;

public interface IMessageBroker
{
    Task SendMessageAsync(string topic, string message);
    Task<string> ReceiveMessageAsync(string topic, string groupId, CancellationToken cancellationToken);
}