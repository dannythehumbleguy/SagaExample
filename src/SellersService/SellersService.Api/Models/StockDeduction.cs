using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SellersService.Api.Models;

public class StockDeduction
{
    public const string CollectionName = "StockDeductions";

    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    
    [BsonRepresentation(BsonType.String)]
    public Guid OrderId { get; set; }

    public List<StockDeductionItem> Items { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class StockDeductionItem
{
    public Guid ProductId { get; set; }
    public int Amount { get; set; }
    public int Price { get; set; }
}

public class StockDeductionForm
{
    public Guid OrderId { get; set; }
    public List<StockDeductionItemFrom> Items { get; set; }
}

public class StockDeductionItemFrom
{
    public Guid ProductId { get; set; }
    public int Amount { get; set; }
}