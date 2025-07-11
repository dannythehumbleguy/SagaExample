using System.Text.Json.Serialization;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SellersService.Api.Common.Auth;
using SellersService.Api.Common.Kafka;
using SellersService.Api.Database;
using SellersService.Api.Events;
using SellersService.Api.Handlers;
using SellersService.Api.Repositories;
using SellersService.Api.Services;

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
    c.SwaggerDoc("v1", new() { Title = "Sellers Service API", Version = "v1" });
    
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
        .AddProducer<SellerRegistered>(producer => producer
            .DefaultTopic(kafkaConfiguration.UserEventsTopic)
            .AddMiddlewares(m => m
                .AddSerializer<JsonCoreSerializer, CustomMessageTypeResolver>()
            )
        )
        
        .CreateTopicIfNotExists(kafkaConfiguration.SellerEventsTopic, 1, 1)
        .AddProducer<StockDeducted>(producer => producer
            .DefaultTopic(kafkaConfiguration.SellerEventsTopic)
            .AddMiddlewares(m => m
                .AddSerializer<JsonCoreSerializer, CustomMessageTypeResolver>()
            )
        )
        .AddProducer<StockDeductionRefused>(producer => producer
            .DefaultTopic(kafkaConfiguration.SellerEventsTopic)
            .AddMiddlewares(m => m
                .AddSerializer<JsonCoreSerializer, CustomMessageTypeResolver>()
            )
        )
    
        .CreateTopicIfNotExists(kafkaConfiguration.OrderEventsTopic, 1, 1)
        .AddConsumer(consumer => consumer
            .Topic(kafkaConfiguration.OrderEventsTopic)
            .WithGroupId("SellersService.OrderEventsConsumer")
            .WithBufferSize(100)
            .WithWorkersCount(10)
            .AddMiddlewares(u => u
                .Add<ErrorMiddleware>()
                .AddDeserializer<JsonCoreDeserializer, CustomMessageTypeResolver>()
                .AddTypedHandlers(v => v
                    .AddHandler<OrderCanceledHandler>()
                    .WithHandlerLifetime(InstanceLifetime.Scoped)
                )
                .AddTypedHandlers(v => v
                    .AddHandler<OrderCreatedHandler>()
                    .WithHandlerLifetime(InstanceLifetime.Scoped)
                )
            )
        )
        .AddConsumer(consumer => consumer
            .Topic(kafkaConfiguration.PaymentEventsTopic)
            .WithGroupId("SellersService.PaymentEventsConsumer")
            .WithBufferSize(100)
            .WithWorkersCount(10)
            .AddMiddlewares(u => u
                .Add<ErrorMiddleware>()
                .AddDeserializer<JsonCoreDeserializer, CustomMessageTypeResolver>()
                .AddTypedHandlers(v => v
                    .AddHandler<PaymentDeclinedHandler>()
                    .WithHandlerLifetime(InstanceLifetime.Scoped)
                )
            )
        )
    )
);

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<StockDeductionRepository>();
builder.Services.AddScoped<ProductService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sellers Service API V1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

await app.RunAsync();