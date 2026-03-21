using Azure.Messaging.ServiceBus;
using FlowCore.OrderService.Data;
using FlowCore.Shared.Events;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services.AddDbContext<OrderDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("OrderDb")));

builder.Services.AddSingleton(new ServiceBusClient(
    builder.Configuration.GetConnectionString("ServiceBus")));
builder.Services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await db.Database.MigrateAsync();
}

app.MapHealthChecks("/healthz");
app.MapControllers();
app.Run();
