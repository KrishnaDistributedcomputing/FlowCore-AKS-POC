using FlowCore.AuditService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddDbContext<AuditDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("AuditDb")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
    await db.Database.MigrateAsync();
}

app.MapHealthChecks("/healthz");

app.MapPost("/audit/events", async ([FromBody] AuditEventRequest req, AuditDbContext db, CancellationToken ct) =>
{
    var entry = new AuditEntry
    {
        EntityType = req.EntityType,
        EntityId = req.EntityId,
        Action = req.Action,
        PerformedBy = req.PerformedBy
    };
    db.AuditEntries.Add(entry);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/audit/events/{entry.Id}", entry);
});

app.MapGet("/audit/events", async (AuditDbContext db, [FromQuery] string? entityType, [FromQuery] string? entityId) =>
{
    var query = db.AuditEntries.AsQueryable();
    if (entityType is not null) query = query.Where(a => a.EntityType == entityType);
    if (entityId is not null) query = query.Where(a => a.EntityId == entityId);
    return Results.Ok(await query.OrderByDescending(a => a.RecordedAtUtc).Take(50).ToListAsync());
});

app.Run();

record AuditEventRequest(string EntityType, string EntityId, string Action, string PerformedBy);
