using PaymentService.Api.Common.Kafka;

namespace PaymentService.Api.Events;

public class OrderPaid : IKafkaFlowMessage
{
    public Guid OrderId { get; set; }
    public Guid BuyerTransactionId { get; set; }
}