using SellersService.Api.Common.Kafka;

namespace SellersService.Api.Events;

public class SellerRegistered(Guid userId, string accountType) : IKafkaFlowMessage
{
    public Guid UserId { get; set; } = userId;
    public string AccountType { get; set; } = accountType;
}