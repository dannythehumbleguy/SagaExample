using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using OrdersService.Api.Configuration;
using OrdersService.Api.Models;
using OrdersService.Api.Database.Models;
using MongoDB.Driver;
using OrdersService.Api.Common;

namespace OrdersService.Api.Services;

public class AuthService(IMongoCollection<Buyer> buyers, IOptions<AuthConfiguration> authConfig) : IAuthService
{
    private readonly AuthConfiguration _authConfig = authConfig.Value;

    private string HashPassword(string password)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_authConfig.SecretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hash);
    }

    public async Task<Result<AuthResponse, Error>> Authenticate(AuthRequest request)
    {
        var buyer = await buyers.Find(b => b.Login == request.Login).FirstOrDefaultAsync();
        if (buyer == null)
            return new Error("User not found");
        
        if (buyer.Password != HashPassword(request.Password))
            return new Error("Invalid login or password");
            
        var token = Guid.NewGuid().ToString();
        buyer.Token = token;
        buyer.TokenExpiresAt = DateTime.UtcNow.AddHours(24);
        await buyers.ReplaceOneAsync(b => b.Login == buyer.Login, buyer);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = buyer.TokenExpiresAt
        };
    }

    public async Task<Result<AuthResponse, Error>> Register(RegisterRequest request)
    {
        var existingBuyer = await buyers.Find(b => b.Login == request.Login).FirstOrDefaultAsync();
        if (existingBuyer != null)
            return new Error("User with this login already exists");

        var buyer = new Buyer
        {
            Login = request.Login,
            Password = HashPassword(request.Password)
        };

        await buyers.InsertOneAsync(buyer);

        return await Authenticate(new AuthRequest
        {
            Login = request.Login,
            Password = request.Password
        });
    }

    public async Task<bool> ValidateToken(string token)
    {
        var buyer = await buyers.Find(b => b.Token == token).FirstOrDefaultAsync();
        if (buyer == null)
            return false;

        if (buyer.TokenExpiresAt <= DateTime.UtcNow)
            return false;
        
        return true;
    }
}