using CSharpFunctionalExtensions;
using SellersService.Api.Models;
using SellersService.Api.Common;

namespace SellersService.Api.Services;

public interface IAuthService
{
    Task<Result<AuthResponse, Error>> Authenticate(AuthRequest request);
    Task<Result<AuthResponse, Error>> Register(RegisterRequest request);
    Task<bool> ValidateToken(string token);
}