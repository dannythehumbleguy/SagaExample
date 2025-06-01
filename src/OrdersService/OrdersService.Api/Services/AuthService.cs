using System.Security.Cryptography;
using System.Text;
using CSharpFunctionalExtensions;
using KafkaFlow;
using Microsoft.Extensions.Options;
using OrdersService.Api.Models;
using MongoDB.Driver;
using OrdersService.Api.Common;
using OrdersService.Api.Common.Auth;
using OrdersService.Api.Database;
using OrdersService.Api.Events;

namespace OrdersService.Api.Services;

public class AuthService(DbContext db, IOptions<AuthConfiguration> authConfig, IMessageProducer<BuyerRegistered> messageProducer)
{
    private readonly AuthConfiguration _authConfig = authConfig.Value;
    
    public async Task<Result<AuthResponse, Error>> Authenticate(AuthRequest request)
    {
        var buyer = await db.Buyers.Find(b => b.Login == request.Login).FirstOrDefaultAsync();
        if (buyer == null)
            return new Error("User not found");
        
        if (buyer.Password != HashPassword(request.Password))
            return new Error("Invalid login or password");
            
        var token = Guid.NewGuid().ToString();
        buyer.Token = token;
        buyer.TokenExpiresAt = DateTime.UtcNow.Add(_authConfig.TokenLiveTime);
        await db.Buyers.ReplaceOneAsync(b => b.Id == buyer.Id, buyer);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = buyer.TokenExpiresAt
        };
    }

    public async Task<Result<AuthResponse, Error>> Register(RegisterRequest request)
    {
        var existingBuyer = await db.Buyers.Find(b => b.Login == request.Login).FirstOrDefaultAsync();
        if (existingBuyer != null)
            return new Error("User with this login already exists");
        
        var buyer = new Buyer
        {
            Id = Guid.NewGuid(),
            Login = request.Login,
            Password = HashPassword(request.Password),
            CreatedAt = DateTimeOffset.UtcNow,
            Token = Guid.NewGuid().ToString(),
            TokenExpiresAt = DateTime.UtcNow.Add(_authConfig.TokenLiveTime)
        };

        await db.Buyers.InsertOneAsync(buyer);
        await messageProducer.ProduceAsync(buyer.Id.ToString(), new BuyerRegistered(buyer.Id, "Buyer"));

        return new AuthResponse
        {
            Token = buyer.Token,
            ExpiresAt = buyer.TokenExpiresAt
        };
    }

    public async Task<Maybe<Guid>> ValidateToken(string token)
    {
        var buyer = await db.Buyers.Find(b => b.Token == token).FirstOrDefaultAsync();
        if (buyer == null)
            return Maybe<Guid>.None;

        if (buyer.TokenExpiresAt <= DateTime.UtcNow)
            return Maybe<Guid>.None;
        
        return buyer.Id;
    }
    
    private string HashPassword(string password)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_authConfig.SecretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hash);
    }
}