namespace FlowCore.Shared.Events;

public abstract class BaseEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}

public class CustomerUpdated : BaseEvent
{
    public CustomerUpdated() => EventType = nameof(CustomerUpdated);
    public string CustomerId { get; set; } = string.Empty;
    public List<string> Changes { get; set; } = [];
}

public class CaseCreated : BaseEvent
{
    public CaseCreated() => EventType = nameof(CaseCreated);
    public string CaseId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
}

public class OrderPlaced : BaseEvent
{
    public OrderPlaced() => EventType = nameof(OrderPlaced);
    public string OrderId { get; set; } = string.Empty;
    public string CaseId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class NotificationRequested : BaseEvent
{
    public NotificationRequested() => EventType = nameof(NotificationRequested);
    public string NotificationId { get; set; } = Guid.NewGuid().ToString();
    public string Channel { get; set; } = "email";
    public string Recipient { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
}

public class AuditRecorded : BaseEvent
{
    public AuditRecorded() => EventType = nameof(AuditRecorded);
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
}
