using System.Security.Cryptography;
using System.Text;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using SellersService.Api.Configuration;
using SellersService.Api.Models;
using MongoDB.Driver;
using SellersService.Api.Common;
using SellersService.Api.Database;

namespace SellersService.Api.Services;

public class AuthService(DbContext db, IOptions<AuthConfiguration> authConfig)
{
    private readonly AuthConfiguration _authConfig = authConfig.Value;
    
    public async Task<Result<AuthResponse, Error>> Authenticate(AuthRequest request)
    {
        var seller = await db.Sellers.Find(s => s.Login == request.Login).FirstOrDefaultAsync();
        if (seller == null)
            return Result.Failure<AuthResponse, Error>(new Error("User not found"));

        if (seller.Password != HashPassword(request.Password))
            return Result.Failure<AuthResponse, Error>(new Error("Invalid login or password"));

        var token = Guid.NewGuid().ToString();
        seller.Token = token;
        seller.TokenExpiresAt = DateTime.UtcNow.Add(_authConfig.TokenLiveTime);
        await db.Sellers.ReplaceOneAsync(s => s.Id == seller.Id, seller);

        return Result.Success<AuthResponse, Error>(new AuthResponse
        {
            Token = token,
            ExpiresAt = seller.TokenExpiresAt
        });
    }

    public async Task<Result<AuthResponse, Error>> Register(RegisterRequest request)
    {
        var existingSeller = await db.Sellers.Find(s => s.Login == request.Login).FirstOrDefaultAsync();
        if (existingSeller != null)
            return new Error("User with this login already exists");
        
        var seller = new Seller
        {
            Login = request.Login,
            Password = HashPassword(request.Password),
        };

        await db.Sellers.InsertOneAsync(seller);

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
            return Maybe<Guid>.None ;

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