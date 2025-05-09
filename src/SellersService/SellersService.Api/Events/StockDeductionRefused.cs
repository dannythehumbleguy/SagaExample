using SellersService.Api.Common.Kafka;

namespace SellersService.Api.Events;

public class StockDeductionRefused : IKafkaFlowMessage
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; }
}