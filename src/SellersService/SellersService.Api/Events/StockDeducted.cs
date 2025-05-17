using SellersService.Api.Common.Kafka;
using SellersService.Api.Models;

namespace SellersService.Api.Events;

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
        
        public StockDeductionItem(DeductStockResponse.StockDeductionItem stockDeductionItem)
        {
            SellerId = stockDeductionItem.SellerId;
            Amount = stockDeductionItem.Amount;
            Price = stockDeductionItem.Price;
        }
    }
}