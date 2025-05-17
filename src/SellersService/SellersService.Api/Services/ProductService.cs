using CSharpFunctionalExtensions;
using KafkaFlow;
using SellersService.Api.Common;
using SellersService.Api.Events;
using SellersService.Api.Handlers;
using SellersService.Api.Models;
using SellersService.Api.Repositories;

namespace SellersService.Api.Services;

public class ProductService(StockDeductionRepository stockDeductionRepository, 
    IMessageProducer<StockDeducted> stockDeductedProducer,
    IMessageProducer<StockDeductionRefused> stockDeductionRefusedProducer)
{
    public async Task<Result<Guid, Error>> DeductStock(OrderCreated message)
    {
        var deductionResult = await stockDeductionRepository.DeductStock(new StockDeductionRequest(message));
        if (deductionResult.IsFailure)
        {
            await stockDeductionRefusedProducer.ProduceAsync(message.OrderId.ToString(), new StockDeductionRefused
            {
                OrderId = message.OrderId,
                Reason = deductionResult.Error.Message
            });

            return deductionResult.Error;
        }
        
        await stockDeductedProducer.ProduceAsync(message.OrderId.ToString(), new StockDeducted
        {
            OrderId = message.OrderId,
            StockDeductionId = deductionResult.Value.DeductionId,
            BuyerId = message.BuyerId,
            Items = deductionResult.Value.Items.Select(u => new StockDeducted.StockDeductionItem(u)).ToList()
        });
        
        return deductionResult.Value.DeductionId;
    }
    
    public async Task<Result<Guid, Error>> CancelStockDeduction(Guid orderId)
    {
        var cancelDeductionResult = await stockDeductionRepository.CancelStockDeduction(orderId);
        if(cancelDeductionResult.IsFailure)
            return cancelDeductionResult.Error; // TODO: Critical alert

        return orderId;
    }
}