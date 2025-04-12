namespace PaymentService.Api.Events;

public class PaymentDeclined
{
    public Guid OrderId { get; set; }
    public Guid Reason { get; set; }
}