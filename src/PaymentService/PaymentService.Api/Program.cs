using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PaymentService.Api.Configuration;
using PaymentService.Api.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Payment Service API", Version = "v1" });
});

// Configuration
builder.Services.Configure<MongoDbConfiguration>(
    builder.Configuration.GetSection(MongoDbConfiguration.SectionName));

// Db
builder.Services.AddSingleton<IMongoClient>(u =>
{
    var config = u.GetRequiredService<IOptionsMonitor<MongoDbConfiguration>>();
    return new MongoClient(config.CurrentValue.ConnectionString);
});
builder.Services.AddScoped<DbContext>();

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