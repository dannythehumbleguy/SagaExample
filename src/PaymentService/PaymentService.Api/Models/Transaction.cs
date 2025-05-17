using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PaymentService.Api.Models;

public class Transaction
{
    public const string CollectionName = "Transactions";

    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    
    [BsonRepresentation(BsonType.String)]
    public Guid? OrderId { get; set; }
    
    // In coins
    public long Amount { get; set; }

    public string Reason { get; set; }
    public DateTimeOffset CreationAt { get; set; }
}


public class PayForOrderRequest
{
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }
    public long Amount { get; set; }
}

public class ChangeBalanceRequest
{
    public Guid UserId { get; set; }
    public long Amount { get; set; }
}