using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SellersService.Api.Common;
using SellersService.Api.Common.Attributes;
using SellersService.Api.Database.Models;

namespace SellersService.Api.Controllers;

[ValidateToken]
[Route("api/[controller]")]
public class ProductsController(IMongoCollection<Product> dbProducts) : AbstractController
{
    [HttpGet]
    public async Task<List<Product>> GetProducts()
    {
        return await dbProducts.Find(_ => true).ToListAsync();
    }
}