using KafkaFlow;
using PaymentService.Api.Common.Kafka;
using PaymentService.Api.Repositories;

namespace PaymentService.Api.Handlers;

public class BuyerRegisteredHandler(AccountRepository accountRepository) : IMessageHandler<BuyerRegistered>
{
    public async Task Handle(IMessageContext context, BuyerRegistered message) => 
        await context.StoreError(accountRepository.CreateAccount(message.UserId, message.AccountType));
}

public class BuyerRegistered : IKafkaFlowMessage
{
    public Guid UserId { get; set; }
    public string AccountType { get; set; }
}
