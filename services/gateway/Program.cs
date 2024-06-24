using gateway.GraphQL;
using gateway.Service;
using gateway.Storage;
using gateway.Broker;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddScoped<GatewayService>();
builder.Services.AddScoped<IStorage, MinioStorage>();
builder.Services.AddScoped<Query>();
builder.Services.AddScoped<Mutation>();
builder.Services.AddScoped<IMessageBroker>(provider =>
    new KafkaMessageBroker("kafka:9092"));

// builder.Services.AddScoped<Subscription>();

builder.Services
    .AddCors(options =>
    {
        options.AddPolicy("AllowedOrigins", policy =>
        {
            policy.WithOrigins("http://localhost:5137")
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
    })
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddType<JsonType>();
    // .AddSubscriptionType<Subscription>()
    // .AddInMemorySubscriptions();

var app = builder.Build();


app.UseCors("AllowedOrigins");
app.UseWebSockets();
app.MapGraphQL("/graphql");

app.Urls.Add("http://0.0.0.0:5050");
app.UseStaticFiles();
app.Run();