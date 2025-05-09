using CSharpFunctionalExtensions;
using KafkaFlow;

namespace PaymentService.Api.Common.Kafka;

public static class KafkaFlowExtensions
{
    public static async Task StoreError<T>(this IMessageContext context, Task<Result<T, Error>> task)
    {
        var result = await task;
        if(result.IsSuccess)
            return;
        
        context.Items.Add("Error", result.Error);
    }
}