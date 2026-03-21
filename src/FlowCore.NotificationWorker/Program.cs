using Azure.Messaging.ServiceBus;
using FlowCore.NotificationWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplicationInsightsTelemetryWorkerService();
builder.Services.AddSingleton(new ServiceBusClient(
    builder.Configuration.GetConnectionString("ServiceBus")));
builder.Services.AddHostedService<NotificationProcessor>();

var host = builder.Build();
host.Run();
