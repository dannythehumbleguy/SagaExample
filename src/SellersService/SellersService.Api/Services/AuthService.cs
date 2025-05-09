using System.Security.Cryptography;
using System.Text;
using CSharpFunctionalExtensions;
using KafkaFlow;
using Microsoft.Extensions.Options;
using SellersService.Api.Models;
using MongoDB.Driver;
using SellersService.Api.Common;
using SellersService.Api.Common.Auth;
using SellersService.Api.Database;
using SellersService.Api.Events;

namespace SellersService.Api.Services;

public class AuthService(DbContext db, IOptions<AuthConfiguration> authConfig, IMessageProducer<SellerRegistered> messageProducer)
{
    private readonly AuthConfiguration _authConfig = authConfig.Value;
    
    public async Task<Result<AuthResponse, Error>> Authenticate(AuthRequest request)
    {
        var seller = await db.Sellers.Find(s => s.Login == request.Login).FirstOrDefaultAsync();
        if (seller == null)
            return new Error("User not found");

        if (seller.Password != HashPassword(request.Password))
            return new Error("Invalid login or password");

        var token = Guid.NewGuid().ToString();
        seller.Token = token;
        seller.TokenExpiresAt = DateTime.UtcNow.Add(_authConfig.TokenLiveTime);
        await db.Sellers.ReplaceOneAsync(s => s.Id == seller.Id, seller);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = seller.TokenExpiresAt
        };
    }

    public async Task<Result<AuthResponse, Error>> Register(RegisterRequest request)
    {
        var existingSeller = await db.Sellers.Find(s => s.Login == request.Login).FirstOrDefaultAsync();
        if (existingSeller != null)
            return new Error("User with this login already exists");
        
        var seller = new Seller
        {
            Id = Guid.NewGuid(),
            Login = request.Login,
            Password = HashPassword(request.Password),
            CreatedAt = DateTimeOffset.UtcNow,
            Token = Guid.NewGuid().ToString(),
            TokenExpiresAt = DateTime.UtcNow.Add(_authConfig.TokenLiveTime)
        };

        await db.Sellers.InsertOneAsync(seller);
        await messageProducer.ProduceAsync(seller.Id.ToString(), new SellerRegistered(seller.Id, "Seller"));

        return await Authenticate(new AuthRequest
        {
            Login = request.Login,
            Password = request.Password
        });
    }

    public async Task<Maybe<Guid>> ValidateToken(string token)
    {
        var seller = await db.Sellers.Find(s => s.Token == token).FirstOrDefaultAsync();
        if (seller == null)
            return Maybe<Guid>.None;

        if (seller.TokenExpiresAt <= DateTime.UtcNow)
            return Maybe<Guid>.None;

        return seller.Id;
    }
    
    private string HashPassword(string password)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_authConfig.SecretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hash);
    }
}