using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SellersService.Api.Common;
using SellersService.Api.Common.Auth;
using SellersService.Api.Models;
using SellersService.Api.Services;

namespace SellersService.Api.Controllers;

public class AuthController(AuthService authService) : AbstractController
{
    [HttpPost("register")]
    public async Task<Results<Ok<AuthResponse>, BadRequest<Error>>> Register(RegisterRequest request) => 
        await Wrap(authService.Register(request));

    [HttpPost("login")]
    public async Task<Results<Ok<AuthResponse>, BadRequest<Error>>> Login(AuthRequest request) => 
        await Wrap(authService.Authenticate(request));
    
    [ValidateToken]
    [HttpGet("user/id")]
    public Guid GetUserId() => UserId;
}