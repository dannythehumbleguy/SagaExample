using System.Text.Json.Serialization;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrdersService.Api.Common.Auth;
using OrdersService.Api.Common.Kafka;
using OrdersService.Api.Database;
using OrdersService.Api.Events;
using OrdersService.Api.Handlers;
using OrdersService.Api.Repositories;
using OrdersService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<MongoDbConfiguration>(
    builder.Configuration.GetSection(MongoDbConfiguration.SectionName));
builder.Services.Configure<AuthConfiguration>(
    builder.Configuration.GetSection(AuthConfiguration.SectionName));

// Db
builder.Services.AddSingleton<IMongoClient>(u =>
{
    var config = u.GetRequiredService<IOptionsMonitor<MongoDbConfiguration>>();
    return new MongoClient(config.CurrentValue.ConnectionString);
});
builder.Services.AddScoped<DbContext>();

// Common
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Orders Service API", Version = "v1" });
    
    c.AddSecurityDefinition("Token", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Authorization header using the token. Example: \"Authorization: your-token-here\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });

    c.OperationFilter<ValidateTokenOperationFilter>();

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Kafka
var kafkaConfiguration = builder.Configuration.GetSection(KafkaConfiguration.SectionName).Get<KafkaConfiguration>();
builder.Services.AddKafka(kafka => kafka
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers([kafkaConfiguration.BootstrapServices])
        .CreateTopicIfNotExists(kafkaConfiguration.UserEventsTopic, 1, 1)
        .AddProducer<BuyerRegistered>(producer => producer
            .DefaultTopic(kafkaConfiguration.UserEventsTopic)
            .AddMiddlewares(m => m
                .AddSerializer<JsonCoreSerializer, CustomMessageTypeResolver>()
            )
        )
    
        .CreateTopicIfNotExists(kafkaConfiguration.OrderEventsTopic, 1, 1)
        .AddProducer<OrderCanceled>(producer => producer
            .DefaultTopic(kafkaConfiguration.OrderEventsTopic)
            .AddMiddlewares(m => m
                .AddSerializer<JsonCoreSerializer, CustomMessageTypeResolver>()
            )
        )
        .AddProducer<OrderCreated>(producer => producer
            .DefaultTopic(kafkaConfiguration.OrderEventsTopic)
            .AddMiddlewares(m => m
                .AddSerializer<JsonCoreSerializer, CustomMessageTypeResolver>()
            )
        )
        
        .CreateTopicIfNotExists(kafkaConfiguration.SellerEventsTopic, 1, 1)
        .AddConsumer(consumer => consumer
            .Topic(kafkaConfiguration.SellerEventsTopic)
            .WithGroupId("OrdersService.SellersEventsConsumer")
            .WithBufferSize(100)
            .WithWorkersCount(10)
            .AddMiddlewares(u => u
                .Add<ErrorMiddleware>()
                .AddDeserializer<JsonCoreDeserializer, CustomMessageTypeResolver>()
                .AddTypedHandlers(v => v
                    .AddHandler<StockDeductionRefusedHandler>()
                    .WithHandlerLifetime(InstanceLifetime.Scoped)
                )
                .AddTypedHandlers(v => v
                    .AddHandler<StockDeductedHandler>()
                    .WithHandlerLifetime(InstanceLifetime.Scoped)
                )
            )
        )
    
        .CreateTopicIfNotExists(kafkaConfiguration.PaymentEventsTopic, 1, 1)
        .AddConsumer(consumer => consumer
            .Topic(kafkaConfiguration.SellerEventsTopic)
            .WithGroupId("OrdersService.PaymentEventsConsumer")
            .WithBufferSize(100)
            .WithWorkersCount(10)
            .AddMiddlewares(u => u
                .Add<ErrorMiddleware>()
                .AddDeserializer<JsonCoreDeserializer, CustomMessageTypeResolver>()
                .AddTypedHandlers(v => v
                    .AddHandler<PaymentDeclinedHandler>()
                    .WithHandlerLifetime(InstanceLifetime.Scoped)
                )
                .AddTypedHandlers(v => v
                    .AddHandler<OrderPaidHandler>()
                    .WithHandlerLifetime(InstanceLifetime.Scoped)
                )
            )
        )
    )
);

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<OrderRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders Service API V1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

await app.RunAsync();