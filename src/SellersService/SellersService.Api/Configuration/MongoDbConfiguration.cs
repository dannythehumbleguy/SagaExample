namespace SellersService.Api.Configuration;

public class MongoDbConfiguration
{
    public static string SectionName = "Database";
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
}