using KafkaFlow;
using PaymentService.Api.Common.Kafka;
using PaymentService.Api.Repositories;

namespace PaymentService.Api.Handlers;

public class SellerRegisteredHandler(AccountRepository accountRepository) : IMessageHandler<SellerRegistered>
{
    public async Task Handle(IMessageContext context, SellerRegistered message) => 
        await context.StoreError(accountRepository.CreateAccount(message.UserId, message.AccountType));
}

public class SellerRegistered : IKafkaFlowMessage
{
    public Guid UserId { get; set; }
    public string AccountType { get; set; }
}