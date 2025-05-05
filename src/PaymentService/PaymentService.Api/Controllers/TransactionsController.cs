using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PaymentService.Api.Models;

namespace PaymentService.Api.Controllers;

[Route("api/[controller]")]
public class TransactionsController(IMongoCollection<Transaction> dbTransactions) : ControllerBase
{
    [HttpGet]
    public async Task<List<Transaction>> GetTransactions()
    {
        return await dbTransactions.Find(_ => true).ToListAsync();
    }
}