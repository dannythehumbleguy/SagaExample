using System.Text.Json.Serialization;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PaymentService.Api.Common.Kafka;
using PaymentService.Api.Database;
using PaymentService.Api.Events;
using PaymentService.Api.Handlers;
using PaymentService.Api.Repositories;
using PaymentService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Payment Service API", Version = "v1" });
});

// Configuration
builder.Services.Configure<MongoDbConfiguration>(
    builder.Configuration.GetSection(MongoDbConfiguration.SectionName));
builder.Services.Configure<KafkaConfiguration>(
    builder.Configuration.GetSection(KafkaConfiguration.SectionName));

// Db
builder.Services.AddSingleton<IMongoClient>(u =>
{
    var config = u.GetRequiredService<IOptionsMonitor<MongoDbConfiguration>>();
    return new MongoClient(config.CurrentValue.ConnectionString);
});
builder.Services.AddScoped<DbContext>();

// Services
builder.Services.AddScoped<AccountRepository>();
builder.Services.AddScoped<AccountService>();


// Kafka 
var kafkaConfiguration = builder.Configuration.GetSection(KafkaConfiguration.SectionName).Get<KafkaConfiguration>();
builder.Services.AddKafka(kafka => kafka
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers([kafkaConfiguration.BootstrapServices])
        .CreateTopicIfNotExists(kafkaConfiguration.PaymentEventsTopic, 1, 1)
        .AddProducer<OrderPaid>(producer => producer
            .DefaultTopic(kafkaConfiguration.PaymentEventsTopic)
            .AddMiddlewares(m => m
                .AddSerializer<JsonCoreSerializer, CustomMessageTypeResolver>()
            )
        )
        .AddProducer<PaymentDeclined>(producer => producer
            .DefaultTopic(kafkaConfiguration.PaymentEventsTopic)
            .AddMiddlewares(m => m
                .AddSerializer<JsonCoreSerializer, CustomMessageTypeResolver>()
            )
        )
        
        .CreateTopicIfNotExists(kafkaConfiguration.UserEventsTopic, 1, 1)
        .AddConsumer(consumer => consumer
            .Topic(kafkaConfiguration.UserEventsTopic)
            .WithGroupId("PaymentService.UserEventsConsumer")
            .WithBufferSize(100)
            .WithWorkersCount(10)
            .AddMiddlewares(u => u
                .Add<ErrorMiddleware>()
                .AddDeserializer<JsonCoreDeserializer, CustomMessageTypeResolver>()
                .AddTypedHandlers(v => v
                    .AddHandler<BuyerRegisteredHandler>()
                    .WithHandlerLifetime(InstanceLifetime.Scoped)
                )
                .AddTypedHandlers(v => v
                    .AddHandler<SellerRegisteredHandler>()
                    .WithHandlerLifetime(InstanceLifetime.Scoped)
                )
            )
        )
    
        .CreateTopicIfNotExists(kafkaConfiguration.OrderEventsTopic, 1, 1)
        .AddConsumer(consumer => consumer
            .Topic(kafkaConfiguration.OrderEventsTopic)
            .WithGroupId("PaymentService.OrderEventsConsumer")
            .WithBufferSize(100)
            .WithWorkersCount(10)
            .AddMiddlewares(u => u
                .Add<ErrorMiddleware>()
                .AddDeserializer<JsonCoreDeserializer, CustomMessageTypeResolver>()
                .AddTypedHandlers(v => v
                    .AddHandler<OrderCanceledHandler>()
                    .WithHandlerLifetime(InstanceLifetime.Scoped)
                )
            )
        )

        .CreateTopicIfNotExists(kafkaConfiguration.SellerEventsTopic, 1, 1)
        .AddConsumer(consumer => consumer
            .Topic(kafkaConfiguration.SellerEventsTopic)
            .WithGroupId("PaymentService.SellerEventsConsumer")
            .WithBufferSize(100)
            .WithWorkersCount(10)
            .AddMiddlewares(u => u
                .Add<ErrorMiddleware>()
                .AddDeserializer<JsonCoreDeserializer, CustomMessageTypeResolver>()
                .AddTypedHandlers(v => v
                    .AddHandler<StockDeductedHandler>()
                    .WithHandlerLifetime(InstanceLifetime.Scoped)
                )
            )
        )
    )
);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Service API V1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

app.Run();