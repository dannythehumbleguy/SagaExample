namespace SellersService.Api.Configuration;

public class AuthConfiguration
{
    public const string SectionName = "Auth";
    public string SecretKey { get; set; }
}