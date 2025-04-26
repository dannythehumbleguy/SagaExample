using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using OrdersService.Api.Models;
using OrdersService.Api.Common;

namespace OrdersService.Api.Services;

public interface IAuthService
{
    Task<Result<AuthResponse, Error>> Authenticate(AuthRequest request);
    Task<Result<AuthResponse, Error>> Register(RegisterRequest request);
    Task<bool> ValidateToken(string token);
}