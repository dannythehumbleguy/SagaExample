using KafkaFlow;

namespace OrdersService.Api.Common.Kafka;

public class ErrorMiddleware : IMessageMiddleware
{
    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        await next(context);
        
        if(context.Items.TryGetValue("Error", out var objectError))
        {
            if(objectError is Error error)
                Console.WriteLine(error.Message); //TODO: add logging
        }
    }
}