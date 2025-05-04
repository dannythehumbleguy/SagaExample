using CSharpFunctionalExtensions;
using OrdersService.Api.Common;
using OrdersService.Api.Common.Pagination;
using OrdersService.Api.Database;
using OrdersService.Api.Models;
using MongoDB.Driver;

namespace OrdersService.Api.Repositories;

public class OrderRepository(DbContext db)
{
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
    
    public async Task<Result<Guid, Error>> OrderProducts(Guid userId, OrderProductsForm form)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            BuyerId = userId,
            Status = OrderStatus.Created,
            Products = form.Products.Select(p => new Product
            {
                ProductId = p.ProductId,
                OrderedAmount = p.OrderedAmount
            }).ToList(),
            OrderedAt = DateTimeOffset.UtcNow
        };
        
        await db.Orders.InsertOneAsync(order);
        
        return order.Id;
    }

    public async Task<Result<Guid, Error>> CancelOrder(Guid buyerId, Guid orderId)
    {
        var filter = Builders<Order>.Filter.And(
            Builders<Order>.Filter.Eq(o => o.Id, orderId),
            Builders<Order>.Filter.Eq(o => o.BuyerId, buyerId)
        );
        
        var update = Builders<Order>.Update
            .Set(o => o.Status, OrderStatus.Canceled);
            
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