using CSharpFunctionalExtensions;
using KafkaFlow;
using OrdersService.Api.Common;
using OrdersService.Api.Events;
using OrdersService.Api.Models;
using OrdersService.Api.Repositories;

namespace OrdersService.Api.Services;

public class OrderService(OrderRepository orderRepository, 
    IMessageProducer<OrderCreated> orderCreatedProducer, 
    IMessageProducer<OrderCanceled> orderCanceledProducer)
{
    public async Task<Result<Guid, Error>> OrderProducts(Guid buyerId, OrderProductsRequest request)
    {
        var order = await orderRepository.OrderProducts(buyerId, request);
        if (order.IsFailure)
            return order.Error;
        
        await orderCreatedProducer.ProduceAsync(order.Value.Id, new OrderCreated
        {
            OrderId = order.Value.Id,
            BuyerId = buyerId,
            Products = order.Value.Products.Select(u => new Events.Product
            {
                Id = u.ProductId,
                Amount = u.OrderedAmount
            })
        });

        return order.Value.Id;
    }
    
    public async Task<Result<Guid, Error>> CancelOrder(Guid buyerId, Guid orderId)
    {
        var canceledOrderId = await orderRepository.CancelOrder(buyerId, orderId, "Canceled by user");
        if (canceledOrderId.IsFailure)
            return canceledOrderId.Error;

        await orderCanceledProducer.ProduceAsync(canceledOrderId.Value.ToString(), new OrderCanceled
        {
            OrderId = canceledOrderId.Value
        });
        
        return canceledOrderId.Value;
    }
}