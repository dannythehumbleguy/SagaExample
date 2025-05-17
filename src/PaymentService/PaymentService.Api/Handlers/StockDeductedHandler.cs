using KafkaFlow;
using PaymentService.Api.Common.Kafka;
using PaymentService.Api.Services;

namespace PaymentService.Api.Handlers;

public class StockDeductedHandler(AccountService accountService) : IMessageHandler<StockDeducted>
{
    public async Task Handle(IMessageContext context, StockDeducted message) => 
        await context.StoreError(accountService.PayForOrder(message));
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