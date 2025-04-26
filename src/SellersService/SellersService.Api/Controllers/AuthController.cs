using Microsoft.AspNetCore.Mvc;
using SellersService.Api.Models;
using SellersService.Api.Services;

namespace SellersService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var result = await authService.Register(request);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(AuthRequest request)
    {
        var result = await authService.Authenticate(request);
        if (result.IsFailure)
        {
            return Unauthorized(result.Error);
        }
        return Ok(result.Value);
    }
}