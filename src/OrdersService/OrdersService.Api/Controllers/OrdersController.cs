using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OrdersService.Api.Database.Models;

namespace OrdersService.Api.Controllers;

[Route("api/[controller]")]
public class OrdersController(IMongoCollection<Order> dbOrders) : ControllerBase
{
    [HttpGet]
    public async Task<List<Order>> GetOrders()
    {
        return await dbOrders.Find(_ => true).ToListAsync();
    }
}