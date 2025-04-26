using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SellersService.Api.Configuration;
using SellersService.Api.Database.Models;
using SellersService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<MongoDbConfiguration>(
    builder.Configuration.GetSection(MongoDbConfiguration.SectionName));
builder.Services.Configure<AuthConfiguration>(
    builder.Configuration.GetSection(AuthConfiguration.SectionName));
// Db
builder.Services.AddSingleton<IMongoCollection<Product>>((u) =>
{
    var config = u.GetRequiredService<IOptionsMonitor<MongoDbConfiguration>>();
    var mongoClient = new MongoClient(config.CurrentValue.ConnectionString);
    var mongoDatabase = mongoClient.GetDatabase(config.CurrentValue.DatabaseName);

    return mongoDatabase.GetCollection<Product>(Product.CollectionName);
});
builder.Services.AddSingleton<IMongoCollection<Seller>>((u) =>
{
    var config = u.GetRequiredService<IOptionsMonitor<MongoDbConfiguration>>();
    var mongoClient = new MongoClient(config.CurrentValue.ConnectionString);
    var mongoDatabase = mongoClient.GetDatabase(config.CurrentValue.DatabaseName);

    return mongoDatabase.GetCollection<Seller>(Seller.CollectionName);
});
builder.Services.AddSingleton<IMongoCollection<StockDeduction>>((u) =>
{
    var config = u.GetRequiredService<IOptionsMonitor<MongoDbConfiguration>>();
    var mongoClient = new MongoClient(config.CurrentValue.ConnectionString);
    var mongoDatabase = mongoClient.GetDatabase(config.CurrentValue.DatabaseName);

    return mongoDatabase.GetCollection<StockDeduction>(StockDeduction.CollectionName);
});

// Common
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Sellers Service API", Version = "v1" });
});

builder.Services.AddScoped<IAuthService, AuthService>();

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

app.Run();