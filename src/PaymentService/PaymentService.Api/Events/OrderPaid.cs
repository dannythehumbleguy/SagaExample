namespace PaymentService.Api.Events;

public class OrderPaid
{
    public Guid OrderId { get; set; }
    public Guid TransactionId { get; set; }
}