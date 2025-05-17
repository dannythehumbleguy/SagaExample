using KafkaFlow;
using PaymentService.Api.Common.Kafka;
using PaymentService.Api.Repositories;

namespace PaymentService.Api.Handlers;

public class OrderCanceledHandler(AccountRepository accountRepository) : IMessageHandler<OrderCanceled>
{
    public async Task Handle(IMessageContext context, OrderCanceled message) => 
        await context.StoreError(accountRepository.MakeRefund(message.OrderId));
}

public class OrderCanceled : IKafkaFlowMessage
{
    public Guid OrderId { get; set; }
}