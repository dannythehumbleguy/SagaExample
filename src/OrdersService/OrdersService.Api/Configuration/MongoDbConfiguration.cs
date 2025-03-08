namespace OrdersService.Api.Configuration;

public class MongoDbConfiguration
{
    public static string SectionName = "OrdersDatabase";
    
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;
}