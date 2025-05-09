using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PaymentService.Api.Models;

namespace PaymentService.Api.Database;

public class DbContext
{
    public IMongoClient Client { get; }
    public IMongoDatabase Database { get; }

    public IMongoCollection<Transaction> Transactions { get; }
    public IMongoCollection<Account> Accounts { get; }
    
    public DbContext(IMongoClient client, IOptionsMonitor<MongoDbConfiguration> config)
    {
        Client = client;
        Database = client.GetDatabase(config.CurrentValue.DatabaseName);
        Transactions = Database.GetCollection<Transaction>(Transaction.CollectionName);
        Accounts = Database.GetCollection<Account>(Account.CollectionName);
    }
} 