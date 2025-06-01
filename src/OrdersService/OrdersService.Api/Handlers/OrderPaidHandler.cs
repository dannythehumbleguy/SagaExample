using KafkaFlow;
using OrdersService.Api.Common.Kafka;
using OrdersService.Api.Repositories;

namespace OrdersService.Api.Handlers;

public class OrderPaidHandler(OrderRepository orderRepository) : IMessageHandler<OrderPaid>
{
    public async Task Handle(IMessageContext context, OrderPaid message) => 
        await orderRepository.SetBuyerTransactionId(message.OrderId, message.BuyerTransactionId);
}

public class OrderPaid : IKafkaFlowMessage
{
    public Guid OrderId { get; set; }
    public Guid BuyerTransactionId { get; set; }
}