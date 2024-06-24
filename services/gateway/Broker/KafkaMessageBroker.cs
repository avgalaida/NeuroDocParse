namespace gateway.Broker;

using Confluent.Kafka;
using System.Threading;
using System.Threading.Tasks;

public class KafkaMessageBroker : IMessageBroker
{
    private readonly string _bootstrapServers;

    public KafkaMessageBroker(string bootstrapServers)
    {
        _bootstrapServers = bootstrapServers;
    }

    public async Task SendMessageAsync(string topic, string message)
    {
        var config = new ProducerConfig { BootstrapServers = _bootstrapServers };
        using var producer = new ProducerBuilder<Null, string>(config).Build();
        await producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
    }

    public async Task<string> ReceiveMessageAsync(string topic, string groupId, CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(topic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(cancellationToken);
                if (consumeResult != null)
                {
                    return consumeResult.Message.Value;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation
        }
        finally
        {
            consumer.Close();
        }

        return null;
    }
}