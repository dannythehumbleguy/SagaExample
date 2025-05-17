using PaymentService.Api.Common.Kafka;

namespace PaymentService.Api.Events;

public class PaymentDeclined : IKafkaFlowMessage
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; }
}