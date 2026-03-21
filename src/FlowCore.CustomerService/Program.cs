using Azure.Messaging.ServiceBus;
using FlowCore.CustomerService.Data;
using FlowCore.Shared.Events;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services.AddDbContext<CustomerDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("CustomerDb")));

builder.Services.AddStackExchangeRedisCache(opts =>
    opts.Configuration = builder.Configuration.GetConnectionString("Redis"));

builder.Services.AddSingleton(new ServiceBusClient(
    builder.Configuration.GetConnectionString("ServiceBus")));
builder.Services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
    await db.Database.MigrateAsync();
}

app.MapHealthChecks("/healthz");
app.MapControllers();
app.Run();
