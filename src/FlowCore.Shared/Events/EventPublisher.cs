using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace FlowCore.Shared.Events;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : BaseEvent;
}

public class ServiceBusEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusSender _sender;

    public ServiceBusEventPublisher(ServiceBusClient client, string topicName = "flowcore-events")
    {
        _sender = client.CreateSender(topicName);
    }

    public async Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : BaseEvent
    {
        var message = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(@event))
        {
            ContentType = "application/json",
            Subject = @event.EventType,
            CorrelationId = @event.CorrelationId,
            MessageId = @event.EventId.ToString()
        };
        message.ApplicationProperties["eventType"] = @event.EventType;
        await _sender.SendMessageAsync(message, ct);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
    }
}
