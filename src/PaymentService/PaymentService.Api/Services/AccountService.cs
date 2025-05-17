using CSharpFunctionalExtensions;
using KafkaFlow;
using PaymentService.Api.Common;
using PaymentService.Api.Events;
using PaymentService.Api.Handlers;
using PaymentService.Api.Repositories;

namespace PaymentService.Api.Services;

public class AccountService(AccountRepository accountRepository,
    IMessageProducer<OrderPaid> orderPaidProducer,
    IMessageProducer<PaymentDeclined> paymentDeclinedProducer)
{
    public async Task<Result<Guid, Error>> PayForOrder(StockDeducted message)
    {
        var payingResult = await accountRepository.PayForOrder(message);
        if (payingResult.IsFailure)
        {
            await paymentDeclinedProducer.ProduceAsync(message.OrderId, new PaymentDeclined
            {
                OrderId = message.OrderId,
                Reason = payingResult.Error.Message
            });

            return payingResult.Error;
        }
        
        await orderPaidProducer.ProduceAsync(message.OrderId, new OrderPaid
        {
            OrderId = message.OrderId,
            BuyerTransactionId = payingResult.Value
        });
        
        return payingResult.Value;
    }
}