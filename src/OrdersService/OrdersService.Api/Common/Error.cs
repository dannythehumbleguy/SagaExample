namespace OrdersService.Api.Common;

public class Error(string message)
{
    public string Message { get; set; } = message;
}