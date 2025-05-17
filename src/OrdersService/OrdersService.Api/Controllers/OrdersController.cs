using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrdersService.Api.Common;
using OrdersService.Api.Common.Auth;
using OrdersService.Api.Common.Pagination;
using OrdersService.Api.Models;
using OrdersService.Api.Repositories;
using OrdersService.Api.Services;

namespace OrdersService.Api.Controllers;

[ValidateToken]
[Route("api/[controller]")]
public class OrdersController(OrderService orderService, OrderRepository orderRepository) : AbstractController
{
    [HttpGet]
    public async Task<Paged<OrderDto>> GetOrders([FromQuery] PaginationRequest request) => 
        await orderRepository.GetOrders(UserId, request);
    
    [HttpPost]
    public async Task<Results<Ok<Guid>, BadRequest<Error>>> OrderProducts([FromBody] OrderProductsRequest request) =>
        await Wrap(orderService.OrderProducts(UserId, request));

    [HttpPatch("cancel/{orderId:guid}")]
    public async Task<Results<Ok<Guid>, BadRequest<Error>>> CancelOrder([FromRoute] Guid orderId) =>
        await Wrap(orderService.CancelOrder(UserId, orderId));

}