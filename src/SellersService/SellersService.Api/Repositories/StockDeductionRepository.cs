using CSharpFunctionalExtensions;
using MongoDB.Driver;
using SellersService.Api.Common;
using SellersService.Api.Database;
using SellersService.Api.Models;

namespace SellersService.Api.Repositories;

public class StockDeductionRepository(DbContext db)
{
    public async Task<Result<Guid, Error>> DeductStock(StockDeductionForm form)
    {
        using var session = await db.Client.StartSessionAsync();
        session.StartTransaction();

        try
        {
            var products = await db.Products.Find(session,
                    Builders<Product>.Filter.And(Builders<Product>.Filter.Eq(p => p.DeletedAt, null),
                        Builders<Product>.Filter.In(p => p.Id, form.Items.Select(product => product.ProductId))))
                .ToListAsync();

            var deduction = new StockDeduction
            {
                Id = Guid.NewGuid(),
                OrderId = form.OrderId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Items = form.Items.Select(u => new StockDeductionItem
                {
                    ProductId = u.ProductId,
                    Amount = u.Amount,
                    Price = products.FirstOrDefault(p => p.Id == u.ProductId)?.Price ??
                            -1, // We exclude products that we could find.
                }).Where(u => u.Price > 0).ToList()
            };
            await db.StockDeductions.InsertOneAsync(session, deduction);

            var bulkOperations = new List<WriteModel<Product>>();
            var currentTime = DateTimeOffset.UtcNow;
            foreach (var item in deduction.Items)
            {
                var filter = Builders<Product>.Filter.Eq(p => p.Id, item.ProductId);
                var update = Builders<Product>.Update
                    .Inc(p => p.Amount, -item.Amount)
                    .Set(p => p.UpdatedAt, currentTime);

                bulkOperations.Add(new UpdateOneModel<Product>(filter, update));
            }

            var bulkWriteResult = await db.Products.BulkWriteAsync(session, bulkOperations,
                new BulkWriteOptions { IsOrdered = true });

            if (bulkWriteResult.ModifiedCount != deduction.Items.Count)
                throw new InvalidOperationException(
                    $"Expected to update {deduction.Items.Count} products, but only {bulkWriteResult.ModifiedCount} were modified.");

            await session.CommitTransactionAsync();
            return deduction.Id;
        }
        catch (Exception e)
        {
            await session.AbortTransactionAsync();
            return new Error(e.Message);
        }
    }

    public async Task<Result<bool, Error>> CancelStockDeduction(Guid orderId)
    {
        var currentTime = DateTimeOffset.UtcNow;
        using var session = await db.Client.StartSessionAsync();
        session.StartTransaction();

        try
        {
            var deduction = await db.StockDeductions
                .Find(session, Builders<StockDeduction>.Filter.Eq(d => d.OrderId, orderId)).FirstOrDefaultAsync();

            if (deduction == null)
                return new Error($"No stock deductions found for order {orderId}");

            var bulkOperations = new List<WriteModel<Product>>();
            foreach (var item in deduction.Items)
            {
                var filter = Builders<Product>.Filter.Eq(p => p.Id, item.ProductId);
                var update = Builders<Product>.Update
                    .Inc(p => p.Amount, item.Amount) // Add back the deducted amount
                    .Set(p => p.UpdatedAt, currentTime);

                bulkOperations.Add(new UpdateOneModel<Product>(filter, update));
            }

            var bulkWriteResult = await db.Products.BulkWriteAsync(session, bulkOperations,
                new BulkWriteOptions { IsOrdered = true });

            if (bulkWriteResult.ModifiedCount != deduction.Items.Count)
                throw new InvalidOperationException(
                    $"Expected to update {{totalItemsToUpdate}} products, but only {bulkWriteResult.ModifiedCount} were modified.");

            await db.StockDeductions.UpdateOneAsync(
                session,
                Builders<StockDeduction>.Filter.Eq(d => d.Id, deduction.Id),
                Builders<StockDeduction>.Update
                    .Set(d => d.UpdatedAt, currentTime)
                    .Set(d => d.RevertedAt, currentTime)
            );

            await session.CommitTransactionAsync();
            return true;
        }
        catch (Exception e)
        {
            // CRITICAL: notify support
            await session.AbortTransactionAsync();
            return new Error(e.Message);
        }
    }
}