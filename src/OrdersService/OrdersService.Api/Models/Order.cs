using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OrdersService.Api.Models;

public class Order
{
    public const string CollectionName = "Orders";
    
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    
    [BsonRepresentation(BsonType.String)]
    public OrderStatus Status { get; set; }
    public List<Product> Products { get; set; }
    
    [BsonRepresentation(BsonType.String)]
    public Guid BuyerId { get; set; }
    
    [BsonRepresentation(BsonType.String)]
    public Guid? TransactionId { get; set; }
    
    [BsonRepresentation(BsonType.String)]
    public Guid? StockDeductionId  { get; set; }
    
    public DateTimeOffset OrderedAt { get; set; }
}


public class OrderDto
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
    public List<ProductDto> Products { get; set; }
    
    public DateTimeOffset OrderedAt { get; set; }
}

public class OrderProductsForm
{
    public List<ProductDto> Products { get; set; }
}