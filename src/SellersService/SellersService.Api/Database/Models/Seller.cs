using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SellersService.Api.Database.Models;

public class Seller
{
    public const string CollectionName = "Sellers";

    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string Login { get; set; }
    public string Password { get; set; }
    public string Token { get; set; }
    
    public DateTimeOffset CreationDate { get; set; }
    public DateTimeOffset TokenExpiresAt { get; set; }
}