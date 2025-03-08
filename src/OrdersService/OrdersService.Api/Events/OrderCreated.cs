namespace OrdersService.Api.Events;

public class OrderCreated
{
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public List<Product> Products { get; set; }
}

public class Product
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public int Price { get; set; }
    public int Amount { get; set; }
}