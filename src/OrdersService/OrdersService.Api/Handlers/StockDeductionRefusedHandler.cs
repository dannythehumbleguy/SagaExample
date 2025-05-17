using KafkaFlow;
using OrdersService.Api.Common.Kafka;
using OrdersService.Api.Repositories;

namespace OrdersService.Api.Handlers;

public class StockDeductionRefusedHandler(OrderRepository orderRepository) : IMessageHandler<StockDeductionRefused>
{
    public async Task Handle(IMessageContext context, StockDeductionRefused message) => 
        await context.StoreError(orderRepository.CancelOrder(message.OrderId, message.Reason));
}

public class StockDeductionRefused : IKafkaFlowMessage
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; }
}