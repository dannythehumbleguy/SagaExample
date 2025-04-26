using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrdersService.Api.Common.Swagger;
using OrdersService.Api.Configuration;
using OrdersService.Api.Database.Models;
using OrdersService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Db
builder.Services.Configure<MongoDbConfiguration>(
    builder.Configuration.GetSection(MongoDbConfiguration.SectionName));
builder.Services.AddSingleton<IMongoCollection<Order>>(u =>
{
    var config = u.GetRequiredService<IOptionsMonitor<MongoDbConfiguration>>();
    var mongoClient = new MongoClient(config.CurrentValue.ConnectionString);
    var mongoDatabase = mongoClient.GetDatabase(config.CurrentValue.DatabaseName);

    return mongoDatabase.GetCollection<Order>(Order.CollectionName);
});
builder.Services.AddSingleton<IMongoCollection<Buyer>>(u =>
{
    var config = u.GetRequiredService<IOptionsMonitor<MongoDbConfiguration>>();
    var mongoClient = new MongoClient(config.CurrentValue.ConnectionString);
    var mongoDatabase = mongoClient.GetDatabase(config.CurrentValue.DatabaseName);

    return mongoDatabase.GetCollection<Buyer>(Buyer.CollectionName);
});

// Common
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
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

// Auth
builder.Services.AddScoped<AuthService>();
builder.Services.Configure<AuthConfiguration>(
    builder.Configuration.GetSection(AuthConfiguration.SectionName));

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

app.Run();