using collector.Service;
using collector.Broker;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<CollectorService>();
        services.AddScoped<IMessageBroker>(provider =>
            new KafkaMessageBroker("kafka:9092")); // адрес Kafka брокера
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .Build();

await host.RunAsync();