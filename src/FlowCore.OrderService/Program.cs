using Polly;
using Azure.Messaging.ServiceBus;
using FlowCore.OrderService.Data;
using FlowCore.Shared.Events;
using FlowCore.Shared.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddControllers();

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("OrderDb")!, name: "postgresql");

builder.Services.AddDbContext<OrderDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("OrderDb")));

builder.Services.AddSingleton(new ServiceBusClient(
    builder.Configuration.GetConnectionString("ServiceBus")));
builder.Services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();
builder.Services.AddHttpClient("rules-service", c =>
    c.BaseAddress = new Uri(builder.Configuration["Services:RulesService"] ?? "http://rules-service.apps.svc.cluster.local"))
    .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await db.Database.MigrateAsync();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.MapHealthChecks("/healthz");
app.MapControllers();
app.Run();
