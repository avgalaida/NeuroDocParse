using collector.Service;
using collector.Broker;
using collector.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        var connectionString = configuration.GetConnectionString("PostgresConnection");

        services.AddSingleton<IDataBase>(provider => new PostgresDatabase(connectionString));
        services.AddScoped<IMessageBroker>(provider => new KafkaMessageBroker("kafka:9092")); // адрес Kafka брокера
        services.AddHostedService<CollectorService>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .Build();

await host.RunAsync();