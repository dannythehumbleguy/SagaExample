namespace SellersService.Api.Events;

public class StockDeducted
{
    public Guid OrderId { get; set; }
    public Guid StockDeductionId { get; set; }
}