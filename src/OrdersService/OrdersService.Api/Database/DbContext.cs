using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrdersService.Api.Models;

namespace OrdersService.Api.Database;

public class DbContext
{
    public IMongoClient Client { get; }
    public IMongoDatabase Database { get; }

    public IMongoCollection<Order> Orders { get; }
    public IMongoCollection<Buyer> Buyers { get; }
    
    public DbContext(IMongoClient client, IOptionsMonitor<MongoDbConfiguration> config)
    {
        Client = client;
        Database = client.GetDatabase(config.CurrentValue.DatabaseName);
        Orders = Database.GetCollection<Order>(Order.CollectionName);
        Buyers = Database.GetCollection<Buyer>(Buyer.CollectionName);
    }
} 