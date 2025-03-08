namespace OrdersService.Api.Database.Models;

public class Buyer
{
    public Guid Id { get; set; }

    public string Login { get; set; }
    public string Password { get; set; }
    public string Token { get; set; }
    
    public DateTimeOffset CreationDate { get; set; }
}