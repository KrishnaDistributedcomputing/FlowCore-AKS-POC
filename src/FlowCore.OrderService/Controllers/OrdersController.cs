using FlowCore.OrderService.Data;
using FlowCore.OrderService.Models;
using FlowCore.Shared.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowCore.OrderService.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _db;
    private readonly IEventPublisher _events;
    private readonly ILogger<OrdersController> _logger;

    private static readonly HashSet<string> ValidStates = ["created", "validated", "fulfilled", "failed"];

    public OrdersController(OrderDbContext db, IEventPublisher events, ILogger<OrdersController> logger)
    {
        _db = db;
        _events = events;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest req, CancellationToken ct)
    {
        var order = new Order
        {
            CustomerId = req.CustomerId,
            Amount = req.Amount
        };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        await _events.PublishAsync(new OrderPlaced
        {
            OrderId = order.Id.ToString(),
            CaseId = order.Id.ToString(),
            Amount = order.Amount
        }, ct);

        await _events.PublishAsync(new CaseCreated
        {
            CaseId = order.Id.ToString(),
            CustomerId = order.CustomerId
        }, ct);

        _logger.LogInformation("Order {OrderId} created for customer {CustomerId}", order.Id, order.CustomerId);
        return CreatedAtAction(nameof(Get), new { orderId = order.Id }, ToResponse(order));
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> Get(Guid orderId, CancellationToken ct)
    {
        var order = await _db.Orders.FindAsync([orderId], ct);
        if (order is null) return NotFound();
        return Ok(ToResponse(order));
    }

    [HttpPatch("{orderId:guid}/state")]
    public async Task<IActionResult> UpdateState(Guid orderId, [FromBody] UpdateOrderStateRequest req, CancellationToken ct)
    {
        if (!ValidStates.Contains(req.State))
            return BadRequest($"Invalid state. Valid states: {string.Join(", ", ValidStates)}");

        var order = await _db.Orders.FindAsync([orderId], ct);
        if (order is null) return NotFound();

        order.State = req.State;
        order.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        if (req.State == "fulfilled" || req.State == "failed")
        {
            await _events.PublishAsync(new NotificationRequested
            {
                Channel = "email",
                Recipient = order.CustomerId,
                Template = $"order-{req.State}"
            }, ct);
        }

        _logger.LogInformation("Order {OrderId} state changed to {State}", orderId, req.State);
        return Ok(ToResponse(order));
    }

    private static OrderResponse ToResponse(Order o) =>
        new(o.Id, o.CustomerId, o.Amount, o.State, o.CreatedAtUtc);
}
