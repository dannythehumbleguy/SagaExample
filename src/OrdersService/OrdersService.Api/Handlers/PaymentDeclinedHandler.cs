using KafkaFlow;
using OrdersService.Api.Common.Kafka;
using OrdersService.Api.Repositories;

namespace OrdersService.Api.Handlers;

public class PaymentDeclinedHandler(OrderRepository orderRepository) : IMessageHandler<PaymentDeclined>
{
    public async Task Handle(IMessageContext context, PaymentDeclined message) => 
        await context.StoreError(orderRepository.CancelOrder(message.OrderId, message.Reason));
}

public class PaymentDeclined : IKafkaFlowMessage
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; }
}