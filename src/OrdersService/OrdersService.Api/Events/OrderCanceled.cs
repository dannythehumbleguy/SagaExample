using OrdersService.Api.Common.Kafka;

namespace OrdersService.Api.Events;

public class OrderCanceled : IKafkaFlowMessage
{
    public Guid OrderId { get; set; }
}