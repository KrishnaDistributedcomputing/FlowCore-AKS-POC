using Azure.Messaging.ServiceBus;
using FlowCore.Shared.Events;
using System.Text.Json;

namespace FlowCore.NotificationWorker;

public class NotificationProcessor : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger<NotificationProcessor> _logger;

    public NotificationProcessor(ServiceBusClient client, ILogger<NotificationProcessor> logger)
    {
        _logger = logger;
        _processor = client.CreateProcessor("flowcore-events", "notification-worker",
            new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 5,
                AutoCompleteMessages = false
            });
        _processor.ProcessMessageAsync += HandleMessageAsync;
        _processor.ProcessErrorAsync += HandleErrorAsync;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification worker starting...");
        await _processor.StartProcessingAsync(stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            var notification = JsonSerializer.Deserialize<NotificationRequested>(body);

            if (notification is null)
            {
                _logger.LogWarning("Received null notification payload, dead-lettering");
                await args.DeadLetterMessageAsync(args.Message, "NullPayload", "Payload deserialized to null");
                return;
            }

            // Simulate dispatch
            _logger.LogInformation(
                "Dispatching {Channel} notification to {Recipient} using template '{Template}' [NotificationId={NotificationId}, CorrelationId={CorrelationId}]",
                notification.Channel, notification.Recipient, notification.Template,
                notification.NotificationId, notification.CorrelationId);

            // Simulate processing delay
            await Task.Delay(100);

            await args.CompleteMessageAsync(args.Message);
            _logger.LogInformation("Notification {NotificationId} dispatched successfully", notification.NotificationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing notification message {MessageId}", args.Message.MessageId);
            // Let Service Bus handle retry via abandon
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception,
            "Service Bus error: Source={Source}, Namespace={Namespace}, EntityPath={EntityPath}",
            args.ErrorSource, args.FullyQualifiedNamespace, args.EntityPath);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Notification worker stopping...");
        await _processor.StopProcessingAsync(cancellationToken);
        await _processor.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
