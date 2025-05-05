using CSharpFunctionalExtensions;
using PaymentService.Api.Common;
using PaymentService.Api.Database;
using PaymentService.Api.Models;
using MongoDB.Driver;

namespace PaymentService.Api.Repositories;

public class AccountRepository(DbContext db)
{
    public async Task<Result<AccountDto, Error>> GetAccount(Guid userId)
    {
        var account = await db.Accounts.Find(a => a.UserId == userId).FirstOrDefaultAsync();
        if (account == null)
            return new Error("Account not found");

        return new AccountDto
        {
            UserId = account.UserId,
            Money = account.Money,
        };
    }
    
    public async Task<Result<Guid, Error>> CreateAccount(Guid userId)
    {
        try
        {
            var account = new Account
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Money = 0,
                Transactions = new List<Transaction>(),
                CreationAt = DateTimeOffset.UtcNow
            };

            await db.Accounts.InsertOneAsync(account);
            return account.Id;
        }
        catch (Exception ex)
        {
            return new Error($"Failed to create account: {ex.Message}");
        }
    }
    
    public async Task<Result<Guid, Error>> ChangeBalance(ChangeBalanceForm form)
    {
        try
        {
            var account = await db.Accounts.Find(a => a.UserId == form.UserId).FirstOrDefaultAsync();
            if (account == null)
                return new Error("Account not found");
            
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = form.Amount,
                Reason = "Balance changed manually",
                CreationAt = DateTimeOffset.UtcNow
            };

            var update = Builders<Account>.Update
                .Inc(a => a.Money, form.Amount)
                .Push(a => a.Transactions, transaction);

            await db.Accounts.UpdateOneAsync(a => a.Id == account.Id, update);
            return transaction.Id;
        }
        catch (Exception ex)
        {
            return new Error($"Failed to change balance: {ex.Message}");
        }
    }
    
    public async Task<Result<Guid, Error>> PayForOrder(PayForOrderForm form)
    {
        try
        {
            var account = await db.Accounts.Find(a => a.UserId == form.UserId).FirstOrDefaultAsync();
            if (account == null)
                return new Error("Account not found");

            if (account.Money < form.Amount)
                return new Error("Insufficient funds");
            

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                OrderId = form.OrderId,
                Amount = -form.Amount,
                Reason = $"Payment for the order {form.OrderId}",
                CreationAt = DateTimeOffset.UtcNow
            };

            var update = Builders<Account>.Update
                .Inc(a => a.Money, -form.Amount)
                .Push(a => a.Transactions, transaction);

            await db.Accounts.UpdateOneAsync(a => a.Id == account.Id, update);
            return transaction.Id;
        }
        catch (Exception ex)
        {
            return new Error($"Failed to pay for order: {ex.Message}");
        }
    }
}