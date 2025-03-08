namespace OrdersService.Api.Database.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid BuyerId { get; set; }
    
    public OrderStatus Status { get; set; }
    public List<Product> Products { get; set; }
    public Guid TransactionId { get; set; }
    public Guid StockDeductionId  { get; set; }
    
    public DateTimeOffset CreationDate { get; set; }
}