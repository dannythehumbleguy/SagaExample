using SellersService.Api.Common.Kafka;

namespace SellersService.Api.Events;

public class StockDeducted : IKafkaFlowMessage
{
    public Guid OrderId { get; set; }
    public Guid StockDeductionId { get; set; }
}