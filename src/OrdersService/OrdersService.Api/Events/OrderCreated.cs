using OrdersService.Api.Common.Kafka;

namespace OrdersService.Api.Events;

public class OrderCreated : IKafkaFlowMessage
{
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public IEnumerable<Product> Products { get; set; }
}

public class Product
{
    public Guid Id { get; set; }
    public int Amount { get; set; }
}