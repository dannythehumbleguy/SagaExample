namespace OrdersService.Api.Database.Models;

public enum OrderStatus
{
    Created = 0,
    ReadyToDelivery = 1,
    Canceled = 2,
}