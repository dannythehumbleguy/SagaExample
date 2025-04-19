using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PaymentService.Api.Configuration;
using PaymentService.Api.Database.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Payment Service API", Version = "v1" });
});

// Db
// Configuration
builder.Services.Configure<MongoDbConfiguration>(
    builder.Configuration.GetSection(MongoDbConfiguration.SectionName));
builder.Services.AddSingleton<IMongoCollection<Transaction>>((u) =>
{
    var config = u.GetRequiredService<IOptionsMonitor<MongoDbConfiguration>>();
    var mongoClient = new MongoClient(config.CurrentValue.ConnectionString);
    var mongoDatabase = mongoClient.GetDatabase(config.CurrentValue.DatabaseName);

    return mongoDatabase.GetCollection<Transaction>(Transaction.CollectionName);
});
builder.Services.AddSingleton<IMongoCollection<Account>>((u) =>
{
    var config = u.GetRequiredService<IOptionsMonitor<MongoDbConfiguration>>();
    var mongoClient = new MongoClient(config.CurrentValue.ConnectionString);
    var mongoDatabase = mongoClient.GetDatabase(config.CurrentValue.DatabaseName);

    return mongoDatabase.GetCollection<Account>(Account.CollectionName);
});

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

app.Run();