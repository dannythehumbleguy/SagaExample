using KafkaFlow;
using OrdersService.Api.Common.Kafka;

namespace OrdersService.Api.Handlers;

public class OrderPaidHandler : IMessageHandler<OrderPaid>
{
    public async Task Handle(IMessageContext context, OrderPaid message) => 
        throw new NotImplementedException();
}

public class OrderPaid : IKafkaFlowMessage
{
    public Guid OrderId { get; set; }
    public Guid BuyerTransactionId { get; set; }
}