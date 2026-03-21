namespace FlowCore.OrderService.Models;

public record CreateOrderRequest(string CustomerId, decimal Amount);
public record UpdateOrderStateRequest(string State);
public record OrderResponse(Guid Id, string CustomerId, decimal Amount, string State, DateTime CreatedAtUtc);
