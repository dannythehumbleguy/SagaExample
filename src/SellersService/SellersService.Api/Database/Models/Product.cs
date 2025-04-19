using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SellersService.Api.Database.Models;

public class Product
{
    public const string CollectionName = "Products";

    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    [BsonRepresentation(BsonType.String)]
    public Guid SellerId { get; set; }
    
    public string Name { get; set; }
    public int Price { get; set; }
    public int Amount { get; set; }
    
    public DateTimeOffset CreationDate { get; set; }
}