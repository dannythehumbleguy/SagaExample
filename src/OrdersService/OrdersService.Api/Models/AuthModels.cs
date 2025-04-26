namespace OrdersService.Api.Models;

public class AuthRequest
{
    public string Login { get; set; }
    public string Password { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}

public class RegisterRequest
{
    public string Login { get; set; }
    public string Password { get; set; }
}

public class RefreshTokenRequest
{
    public string Token { get; set; }
}