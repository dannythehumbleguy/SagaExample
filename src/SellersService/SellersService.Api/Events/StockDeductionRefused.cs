namespace SellersService.Api.Events;

public class StockDeductionRefused
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; }
}