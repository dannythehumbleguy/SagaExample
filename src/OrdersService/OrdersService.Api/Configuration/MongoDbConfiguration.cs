namespace OrdersService.Api.Configuration;

public class MongoDbConfiguration
{
    public const string SectionName = "Database";
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
}