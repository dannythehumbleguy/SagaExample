using OrdersService.Api.Common.Kafka;

namespace OrdersService.Api.Events;

public class BuyerRegistered(Guid userId, string accountType) : IKafkaFlowMessage
{
    public Guid UserId { get; set; } = userId;
    public string AccountType { get; set; } = accountType;
}