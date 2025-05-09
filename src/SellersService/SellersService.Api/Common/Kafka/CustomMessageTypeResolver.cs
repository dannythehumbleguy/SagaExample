using System.Reflection;
using KafkaFlow;
using KafkaFlow.Middlewares.Serializer.Resolvers;

namespace SellersService.Api.Common.Kafka;

public class CustomMessageTypeResolver : IMessageTypeResolver
{
    private const string MessageType = "Message-Type";
    private readonly Dictionary<string, Type> _typeMap;

    public CustomMessageTypeResolver()
    {
        _typeMap = BuildTypeMap();
    }

    private Dictionary<string, Type> BuildTypeMap()
    {
        var result = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        var messageTypes = GetAllMessageTypes();

        foreach (var type in messageTypes)
        {
            result.TryAdd(type.Name, type);
        }

        return result;
    }

    private IEnumerable<Type> GetAllMessageTypes()
    {
        var messageTypes = new List<Type>();

        var assemblies = new[]
        {
            Assembly.GetExecutingAssembly()
        };

        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t is { IsClass: true, IsAbstract: false } &&
                                typeof(IKafkaFlowMessage).IsAssignableFrom(t));

                messageTypes.AddRange(types);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading types from assembly {assembly.FullName}: {ex.Message}");
            }
        }

        return messageTypes;
    }

    public ValueTask<Type> OnConsumeAsync(IMessageContext context)
    {
        var className = context.Headers.GetString(MessageType);
        if (string.IsNullOrEmpty(className))
            throw new InvalidOperationException($"Message header '{MessageType}' is missing");

        if (_typeMap.TryGetValue(className, out var type))
            return ValueTask.FromResult(type);

        throw new InvalidOperationException($"Cannot resolve message type: {className}");
    }

    public ValueTask OnProduceAsync(IMessageContext context)
    {
        if (context.Message.Value is null)
            return ValueTask.CompletedTask;

        var messageType = context.Message.Value.GetType();

        context.Headers.SetString(MessageType, messageType.Name);

        return ValueTask.CompletedTask;
    }
}