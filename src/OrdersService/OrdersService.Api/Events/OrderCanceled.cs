namespace OrdersService.Api.Events;

public class OrderCanceled
{
    public Guid OrderId { get; set; }
}