using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SellersService.Api.Handlers;

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
    public DateTimeOffset? RevertedAt { get; set; }
}

public class StockDeductionItem
{
    public Guid ProductId { get; set; }
    public int Amount { get; set; }
    public int Price { get; set; }
}

// Responses and Requests

public class StockDeductionRequest
{
    public Guid OrderId { get; set; }
    public List<StockDeductionItem> Items { get; set; }
    
    public StockDeductionRequest(OrderCreated message)
    {
        OrderId = message.OrderId;
        Items = message.Products.Select(p => new StockDeductionItem(p)).ToList();
    }
    
    public class StockDeductionItem
    {
        public Guid ProductId { get; set; }
        public int Amount { get; set; }

        public StockDeductionItem(SellersService.Api.Handlers.Product product)
        {
            ProductId = product.Id;
            Amount = product.Amount;
        }
    }
}

public class DeductStockResponse
{
    public Guid DeductionId { get; set; }
    public IEnumerable<StockDeductionItem> Items { get; set; }
    
    public class StockDeductionItem
    {
        public Guid SellerId { get; set; }
        public long Amount { get; set; }
        public long Price { get; set; }
    }
}

