using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OrdersService.Api.Models;

public class Product
{
    [BsonRepresentation(BsonType.String)]
    public Guid ProductId { get; set; }
    
    public int OrderedAmount { get; set; }
}

public class ProductDto
{
    public Guid ProductId { get; set; }
    
    public int OrderedAmount { get; set; }
}