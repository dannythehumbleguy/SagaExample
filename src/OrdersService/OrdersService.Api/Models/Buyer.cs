using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OrdersService.Api.Models;

public class Buyer
{
    public const string CollectionName = "Buyers";

    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string Login { get; set; }
    public string Password { get; set; }
    public string Token { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset TokenExpiresAt { get; set; }
}