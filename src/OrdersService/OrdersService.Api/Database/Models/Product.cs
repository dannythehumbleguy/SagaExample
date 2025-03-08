namespace OrdersService.Api.Database.Models;

public class Product
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    
    public string Name { get; set; }
    public int Price { get; set; }
    public int Amount { get; set; }
}