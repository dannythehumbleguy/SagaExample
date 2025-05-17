using KafkaFlow;
using OrdersService.Api.Common.Kafka;

namespace OrdersService.Api.Handlers;

public class StockDeductedHandler() : IMessageHandler<StockDeducted>
{
    public async Task Handle(IMessageContext context, StockDeducted message) => 
        throw new NotImplementedException();
}


public class StockDeducted : IKafkaFlowMessage
{
    public Guid OrderId { get; set; }
    public Guid StockDeductionId { get; set; }
    public Guid BuyerId { get; set; }

    public List<StockDeductionItem> Items { get; set; }
    
    public class StockDeductionItem
    {
        public Guid SellerId { get; set; }
        public long Amount { get; set; }
        public long Price { get; set; }
    }
}