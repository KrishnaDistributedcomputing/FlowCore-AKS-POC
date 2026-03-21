using FlowCore.CustomerService.Data;
using FlowCore.CustomerService.Models;
using FlowCore.Shared.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace FlowCore.CustomerService.Controllers;

[ApiController]
[Route("[controller]")]
public class CustomersController : ControllerBase
{
    private readonly CustomerDbContext _db;
    private readonly IEventPublisher _events;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(CustomerDbContext db, IEventPublisher events,
        IDistributedCache cache, ILogger<CustomersController> logger)
    {
        _db = db;
        _events = events;
        _cache = cache;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest req, CancellationToken ct)
    {
        var customer = new Customer
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            Email = req.Email
        };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);

        await _events.PublishAsync(new CustomerUpdated
        {
            CustomerId = customer.Id.ToString(),
            Changes = ["created"]
        }, ct);

        _logger.LogInformation("Customer {CustomerId} created", customer.Id);
        return CreatedAtAction(nameof(Get), new { customerId = customer.Id },
            ToResponse(customer));
    }

    [HttpGet("{customerId:guid}")]
    public async Task<IActionResult> Get(Guid customerId, CancellationToken ct)
    {
        var cacheKey = $"customer:{customerId}";
        var cached = await _cache.GetStringAsync(cacheKey, ct);
        if (cached is not null)
            return Ok(JsonSerializer.Deserialize<CustomerResponse>(cached));

        var customer = await _db.Customers.FindAsync([customerId], ct);
        if (customer is null) return NotFound();

        var response = ToResponse(customer);
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) }, ct);
        return Ok(response);
    }

    [HttpPut("{customerId:guid}")]
    public async Task<IActionResult> Update(Guid customerId, [FromBody] UpdateCustomerRequest req, CancellationToken ct)
    {
        var customer = await _db.Customers.FindAsync([customerId], ct);
        if (customer is null) return NotFound();

        var changes = new List<string>();
        if (req.FirstName is not null) { customer.FirstName = req.FirstName; changes.Add("firstName"); }
        if (req.LastName is not null) { customer.LastName = req.LastName; changes.Add("lastName"); }
        if (req.Email is not null) { customer.Email = req.Email; changes.Add("email"); }
        customer.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync($"customer:{customerId}", ct);

        await _events.PublishAsync(new CustomerUpdated
        {
            CustomerId = customerId.ToString(),
            Changes = changes
        }, ct);

        return Ok(ToResponse(customer));
    }

    private static CustomerResponse ToResponse(Customer c) =>
        new(c.Id, c.FirstName, c.LastName, c.Email, c.CreatedAtUtc);
}
