using FlowCore.ReportingService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services.AddDbContext<ReportingDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("ReportingDb")));

builder.Services.AddStackExchangeRedisCache(opts =>
    opts.Configuration = builder.Configuration.GetConnectionString("Redis"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
    await db.Database.MigrateAsync();
}

app.MapHealthChecks("/healthz");

app.MapGet("/reporting/summary", async (ReportingDbContext db) =>
{
    var summaries = await db.ReportSummaries
        .OrderByDescending(r => r.ComputedAtUtc)
        .Take(20)
        .ToListAsync();
    return Results.Ok(summaries);
});

app.Run();
