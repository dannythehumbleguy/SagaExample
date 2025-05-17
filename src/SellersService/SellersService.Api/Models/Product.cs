using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SellersService.Api.Models;

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
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

// Responses and Requests

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Price { get; set; }
    public int Amount { get; set; }
    
    
    public ProductDto(Product product)
    {
        Id = product.Id;
        Name = product.Name;
        Price = product.Price;
        Amount = product.Amount;
    }
}

public class UpdateProductRequest
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int? Price { get; set; }
    public int? Amount { get; set; }
}

public class CreateProductRequest
{
    public string Name { get; set; }
    public int Price { get; set; }
    public int Amount { get; set; }
}
