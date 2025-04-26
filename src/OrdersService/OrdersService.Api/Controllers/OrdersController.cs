using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OrdersService.Api.Common;
using OrdersService.Api.Common.Attributes;
using OrdersService.Api.Database.Models;

namespace OrdersService.Api.Controllers;

[ValidateToken]
[Route("api/[controller]")]
public class OrdersController(IMongoCollection<Order> dbOrders) : AbstractController
{
    [HttpGet]
    public async Task<List<Order>> GetOrders()
    {
        return await dbOrders.Find(_ => true).ToListAsync();
    }
}