using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PaymentService.Api.Database.Models;

public class Transaction
{
    public const string CollectionName = "Transactions";

    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    
    [BsonRepresentation(BsonType.String)]
    public Guid OrderId { get; set; }
    
    // In coins
    public long Amount { get; set; }
    
    public DateTimeOffset CreationDate { get; set; }
}