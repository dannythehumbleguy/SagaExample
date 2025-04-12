using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SellersService.Api.Database.Models;

public class StockDeduction
{
    public const string CollectionName = "StockDeductions";

    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    
    [BsonRepresentation(BsonType.String)]
    public Guid OrderId { get; set; }

    public List<Product> Products { get; set; }
    
    public DateTimeOffset CreationDate { get; set; }
}