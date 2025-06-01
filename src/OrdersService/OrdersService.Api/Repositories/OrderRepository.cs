using CSharpFunctionalExtensions;
using OrdersService.Api.Common;
using OrdersService.Api.Common.Pagination;
using OrdersService.Api.Database;
using OrdersService.Api.Models;
using MongoDB.Driver;

namespace OrdersService.Api.Repositories;

public class OrderRepository(DbContext db)
{
    public async Task<Result<Guid, Error>> SetBuyerTransactionId(Guid orderId, Guid buyerTransactionId)
    {
        var filter = Builders<Order>.Filter.Eq(o => o.Id, orderId);
        var update = Builders<Order>.Update.Set(o => o.BuyerTransactionId, buyerTransactionId);
            
        var order = await db.Orders.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<Order> { ReturnDocument = ReturnDocument.After }
        );

        if (order == null)
            return new Error($"Order with id {orderId} not found");

        return order.Id;
    }
    
    public async Task<Result<Guid, Error>> SetStockDeductionId(Guid orderId, Guid transactionId)
    {
        var filter = Builders<Order>.Filter.Eq(o => o.Id, orderId);
        var update = Builders<Order>.Update.Set(o => o.StockDeductionId, transactionId);
            
        var order = await db.Orders.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<Order> { ReturnDocument = ReturnDocument.After }
        );

        if (order == null)
            return new Error($"Order with id {orderId} not found");

        return order.Id;
    }
    
    public async Task<Paged<OrderDto>> GetOrders(Guid buyerId, PaginationRequest form)
    {
        var filter = Builders<Order>.Filter.Eq(o => o.BuyerId, buyerId);
        var sort = Builders<Order>.Sort.Descending(o => o.OrderedAt);
        
        var pagedOrders = await db.Orders.ToPagedResultAsync(filter, sort, form);
        
        return new Paged<OrderDto>(
            pagedOrders.Items.Select(o => new OrderDto
            {
                Id = o.Id,
                Status = o.Status,
                Products = o.Products.Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    OrderedAmount = p.OrderedAmount
                }).ToList(),
                OrderedAt = o.OrderedAt
            }).ToList(),
            pagedOrders.TotalCount,
            pagedOrders.PageNumber,
            pagedOrders.PageSize
        );
    }
    
    public async Task<Result<Order, Error>> OrderProducts(Guid userId, OrderProductsRequest request)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            BuyerId = userId,
            Status = OrderStatus.Created,
            Products = request.Products.Select(p => new Product
            {
                ProductId = p.ProductId,
                OrderedAmount = p.OrderedAmount
            }).ToList(),
            OrderedAt = DateTimeOffset.UtcNow
        };
        
        await db.Orders.InsertOneAsync(order);
        
        return order;
    }

    public async Task<Result<Guid, Error>> CancelOrder(Guid buyerId, Guid orderId, string reason) =>
        await CancelOrderCommon(buyerId, orderId, reason);
    
    public async Task<Result<Guid, Error>> CancelOrder(Guid orderId, string reason) =>
        await CancelOrderCommon(null, orderId, reason);

    private async Task<Result<Guid, Error>> CancelOrderCommon(Guid? buyerId, Guid orderId, string reason)
    {
        var filter = Builders<Order>.Filter.And(
            Builders<Order>.Filter.Eq(o => o.Id, orderId),
            buyerId != null ? Builders<Order>.Filter.Eq(o => o.BuyerId, buyerId) : Builders<Order>.Filter.Empty
        );
        
        var update = Builders<Order>.Update
            .Set(o => o.Status, OrderStatus.Canceled)
            .Set(u => u.CancelReason, reason);
            
        var order = await db.Orders.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<Order> { ReturnDocument = ReturnDocument.After }
        );

        if (order == null)
            return new Error("Order not found");

        return order.Id;
    }
}