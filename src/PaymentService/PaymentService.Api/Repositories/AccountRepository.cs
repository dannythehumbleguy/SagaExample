using CSharpFunctionalExtensions;
using PaymentService.Api.Common;
using PaymentService.Api.Database;
using PaymentService.Api.Models;
using MongoDB.Driver;
using PaymentService.Api.Handlers;

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
    
    public async Task<Result<Guid, Error>> CreateAccount(Guid userId, string accountType)
    {
        try
        {
            var account = new Account
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountType = accountType,
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
    
    public async Task<Result<Guid, Error>> ChangeBalance(ChangeBalanceRequest request)
    {
        try
        {
            var account = await db.Accounts.Find(a => a.UserId == request.UserId).FirstOrDefaultAsync();
            if (account == null)
                return new Error("Account not found");
            
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = request.Amount,
                Reason = "Balance changed manually",
                CreationAt = DateTimeOffset.UtcNow
            };

            var update = Builders<Account>.Update
                .Inc(a => a.Money, request.Amount)
                .Push(a => a.Transactions, transaction);

            await db.Accounts.UpdateOneAsync(a => a.Id == account.Id, update);
            return transaction.Id;
        }
        catch (Exception ex)
        {
            return new Error($"Failed to change balance: {ex.Message}");
        }
    }
    
    public async Task<Result<Guid, Error>> PayForOrder(StockDeducted message)
    {
        try
        {
            using var session = await db.Client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var buyerAccount = await db.Accounts.Find(a => a.UserId == message.BuyerId).FirstOrDefaultAsync();
                if (buyerAccount == null)
                    return new Error("Buyer account not found");
                
                var sellerTotals = message.Items
                    .GroupBy(item => item.SellerId)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Sum(item => item.Amount * item.Price)
                    );
                
                var sellerAccounts = await db.Accounts
                    .Find(a => sellerTotals.Keys.Contains(a.UserId))
                    .ToListAsync();
                if (sellerAccounts.Count != sellerTotals.Count)
                    return new Error("One or more seller accounts not found");

                var totalOrderAmount = sellerTotals.Values.Sum();
                if (buyerAccount.Money < totalOrderAmount)
                    return new Error("Insufficient funds");
                
                var buyerTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    OrderId = message.OrderId,
                    Amount = -totalOrderAmount,
                    Reason = $"Payment for the order {message.OrderId}",
                    CreationAt = DateTimeOffset.UtcNow
                };
                var buyerUpdate = Builders<Account>.Update
                    .Inc(a => a.Money, -totalOrderAmount)
                    .Push(a => a.Transactions, buyerTransaction);
                await db.Accounts.UpdateOneAsync(session, a => a.Id == buyerAccount.Id, buyerUpdate);
                
                var sellerUpdates = sellerAccounts.Select(sellerAccount =>
                {
                    var sellerAmount = sellerTotals[sellerAccount.UserId];
                    var sellerTransaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        OrderId = message.OrderId,
                        Amount = sellerAmount,
                        Reason = $"Received payment for the order {message.OrderId}",
                        CreationAt = DateTimeOffset.UtcNow
                    };

                    return new UpdateOneModel<Account>(
                        Builders<Account>.Filter.Where(a => a.Id == sellerAccount.Id),
                        Builders<Account>.Update
                            .Inc(a => a.Money, sellerAmount)
                            .Push(a => a.Transactions, sellerTransaction)
                    );
                }).ToList();

                await db.Accounts.BulkWriteAsync(session, sellerUpdates);
                await session.CommitTransactionAsync();

                return buyerTransaction.Id;
            }
            catch (Exception)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            return new Error($"Failed to process payment: {ex.Message}");
        }
    }

    public async Task<Result<Guid, Error>> MakeRefund(Guid orderId)
    {
        try
        {
            using var session = await db.Client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var accounts = await db.Accounts
                    .Find(a => a.Transactions.Any(t => t.OrderId == orderId))
                    .ToListAsync();
                if (accounts.Count == 0)
                    return new Error("No transactions found for this order");
                
                // Buyer
                var buyerAccount = accounts.FirstOrDefault(a => a.Transactions.Any(t => t.OrderId == orderId && t.Amount < 0));
                if (buyerAccount == null )
                    return new Error("Invalid transaction pattern found for refund");

                var buyerTransaction = buyerAccount.Transactions.First(t => t.OrderId == orderId);
                var totalRefundAmount = Math.Abs(buyerTransaction.Amount);
                var buyerRefundTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    Amount = totalRefundAmount,
                    Reason = $"Refund for order {orderId}",
                    CreationAt = DateTimeOffset.UtcNow
                };
                
                var buyerUpdate = Builders<Account>.Update
                    .Inc(a => a.Money, totalRefundAmount)
                    .Push(a => a.Transactions, buyerRefundTransaction);
                await db.Accounts.UpdateOneAsync(session, a => a.Id == buyerAccount.Id, buyerUpdate);
                
                // Sellers
                var sellerAccounts = accounts.Where(a => a.Transactions.Any(t => t.OrderId == orderId && t.Amount > 0)).ToList();
                if (sellerAccounts.Count == 0)
                    return new Error("Invalid transaction pattern found for refund");
    
                var sellerUpdates = sellerAccounts.Select(sellerAccount =>
                {
                    var sellerTransaction = sellerAccount.Transactions.First(t => t.OrderId == orderId);
                    var sellerRefundTransaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderId,
                        Amount = -sellerTransaction.Amount,
                        Reason = $"Refund payment for order {orderId}",
                        CreationAt = DateTimeOffset.UtcNow
                    };

                    return new UpdateOneModel<Account>(
                        Builders<Account>.Filter.Where(a => a.Id == sellerAccount.Id),
                        Builders<Account>.Update
                            .Inc(a => a.Money, -sellerTransaction.Amount)
                            .Push(a => a.Transactions, sellerRefundTransaction)
                    );
                }).ToList();

                await db.Accounts.BulkWriteAsync(session, sellerUpdates);
                await session.CommitTransactionAsync();

                return buyerRefundTransaction.Id;
            }
            catch (Exception)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            return new Error($"Failed to process refund: {ex.Message}");
        }
    }
}