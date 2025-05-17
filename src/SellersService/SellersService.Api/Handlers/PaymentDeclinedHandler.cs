using KafkaFlow;
using SellersService.Api.Common.Kafka;
using SellersService.Api.Services;

namespace SellersService.Api.Handlers;

public class PaymentDeclinedHandler(ProductService productService) : IMessageHandler<PaymentDeclined>
{
    public async Task Handle(IMessageContext context, PaymentDeclined message) => 
        await context.StoreError(productService.CancelStockDeduction(message.OrderId));
}

public class PaymentDeclined : IKafkaFlowMessage
{
    public Guid OrderId { get; set; }
    public Guid Reason { get; set; }
}