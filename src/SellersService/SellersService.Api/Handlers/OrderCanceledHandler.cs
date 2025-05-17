using KafkaFlow;
using SellersService.Api.Common.Kafka;
using SellersService.Api.Services;

namespace SellersService.Api.Handlers;

public class OrderCanceledHandler(ProductService productService) : IMessageHandler<OrderCanceled>
{
    public async Task Handle(IMessageContext context, OrderCanceled message) => 
        await context.StoreError(productService.CancelStockDeduction(message.OrderId));
}

public class OrderCanceled : IKafkaFlowMessage
{
    public Guid OrderId { get; set; }
}