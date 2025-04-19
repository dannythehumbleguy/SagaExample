using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OrdersService.Api.Database.Models;

public class Product
{
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    
    public int OrderedAmount { get; set; }
    
    public DateTimeOffset CreationDate { get; set; }
}