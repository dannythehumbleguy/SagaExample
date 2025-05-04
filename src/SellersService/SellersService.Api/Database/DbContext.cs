using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SellersService.Api.Configuration;
using SellersService.Api.Models;

namespace SellersService.Api.Database;

public class DbContext
{
    public IMongoClient Client { get; }
    public IMongoDatabase Database { get; }

    public IMongoCollection<Product> Products { get; }
    
    public IMongoCollection<Seller> Sellers { get; }
    
    public IMongoCollection<StockDeduction> StockDeductions { get; }
    
    public DbContext(IMongoClient client, IOptionsMonitor<MongoDbConfiguration> config)
    {
        Client = client;
        Database = client.GetDatabase(config.CurrentValue.DatabaseName);
        Products = Database.GetCollection<Product>(Product.CollectionName);
        Sellers = Database.GetCollection<Seller>(Seller.CollectionName);
        StockDeductions = Database.GetCollection<StockDeduction>(StockDeduction.CollectionName);
    }
}