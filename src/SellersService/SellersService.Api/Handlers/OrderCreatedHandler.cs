using KafkaFlow;
using SellersService.Api.Common.Kafka;
using SellersService.Api.Services;

namespace SellersService.Api.Handlers;

public class OrderCreatedHandler(ProductService productService) : IMessageHandler<OrderCreated>
{
    public async Task Handle(IMessageContext context, OrderCreated message) => 
        await context.StoreError(productService.DeductStock(message));
}

public class OrderCreated : IKafkaFlowMessage
{
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public IEnumerable<Product> Products { get; set; }
}

public class Product
{
    public Guid Id { get; set; }
    public int Amount { get; set; }
}