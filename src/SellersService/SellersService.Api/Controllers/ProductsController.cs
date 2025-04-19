using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SellersService.Api.Database.Models;

namespace SellersService.Api.Controllers;

[Route("api/[controller]")]
public class ProductsController(IMongoCollection<Product> dbProducts) : ControllerBase
{
    [HttpGet]
    public async Task<List<Product>> GetProducts()
    {
        return await dbProducts.Find(_ => true).ToListAsync();
    }
}