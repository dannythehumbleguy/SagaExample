using CSharpFunctionalExtensions;
using MongoDB.Driver;
using SellersService.Api.Common;
using SellersService.Api.Common.Pagination;
using SellersService.Api.Database;
using SellersService.Api.Models;

namespace SellersService.Api.Repositories;

public class ProductRepository(DbContext db)
{
    public async Task<Result<Paged<ProductDto>, Error>> GetProducts(PaginationRequest form)
    {
        try
        {
            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Empty,
                Builders<Product>.Filter.Eq(p => p.DeletedAt, null));
            
            var result = await db.Products.ToPagedResultAsync(filter, form);
            return new Paged<ProductDto>(
                result.Items.Select(p => new ProductDto(p)).ToList(),
                result.TotalCount,
                result.PageNumber,
                result.PageSize
            );
        }
        catch (Exception ex)
        {
            return new Error(ex.Message);
        }
    }

    public async Task<Result<Paged<ProductDto>, Error>> GetProducts(Guid sellerId, PaginationRequest form)
    {
        try
        {
            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Eq(p => p.SellerId, sellerId),
                Builders<Product>.Filter.Eq(p => p.DeletedAt, null)
            );
            var result = await db.Products.ToPagedResultAsync(filter, form);
            return new Paged<ProductDto>(
                result.Items.Select(p => new ProductDto(p)).ToList(),
                result.TotalCount,
                result.PageNumber,
                result.PageSize
            );
        }
        catch (Exception ex)
        {
            return new Error(ex.Message);
        }
    }

    public async Task<Result<Guid, Error>> CreateProduct(Guid sellerId, CreateProductRequest request)
    {
        try
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                SellerId = sellerId,
                Name = request.Name,
                Price = request.Price,
                Amount = request.Amount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await db.Products.InsertOneAsync(product);
            return product.Id;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message);
        }
    }

    public async Task<Result<Guid, Error>> UpdateProduct(Guid sellerId, UpdateProductRequest request)
    {
        try
        {
            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Eq(p => p.Id, request.Id),
                Builders<Product>.Filter.Eq(p => p.SellerId, sellerId),
                Builders<Product>.Filter.Eq(p => p.DeletedAt, null)
            );

            var updates = new List<UpdateDefinition<Product>>
            { Builders<Product>.Update.Set(p => p.UpdatedAt, DateTimeOffset.UtcNow) };

            if (request.Name   != null) 
                updates.Add(Builders<Product>.Update.Set(p => p.Name,   request.Name));
            if (request.Amount != null) 
                updates.Add(Builders<Product>.Update.Set(p => p.Amount, request.Amount));
            if (request.Price  != null) 
                updates.Add(Builders<Product>.Update.Set(p => p.Price,  request.Price));
            
            var updateQuery = Builders<Product>.Update.Combine(updates);
            

            var result = await db.Products.UpdateOneAsync(filter, updateQuery);
            
            if (result.MatchedCount == 0)
                return new Error("Product not found or you don't have permission to update it");

            return request.Id;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message);
        }
    }

    public async Task<Result<Guid, Error>> DeleteProduct(Guid userId, Guid productId)
    {
        try
        {
            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Eq(p => p.Id, productId),
                Builders<Product>.Filter.Eq(p => p.SellerId, userId)
            );

            var update = Builders<Product>.Update
                .Set(p => p.DeletedAt, DateTimeOffset.UtcNow)
                .Set(p => p.UpdatedAt, DateTimeOffset.UtcNow);

            var result = await db.Products.UpdateOneAsync(filter, update);
            
            if (result.MatchedCount == 0)
                return new Error("Product not found or you don't have permission to delete it");

            return productId;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message);
        }
    }
}