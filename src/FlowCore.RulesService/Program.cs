using FlowCore.Shared.Middleware;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.MapHealthChecks("/healthz");

app.MapPost("/rules/validate", ([FromBody] ValidateRequest req) =>
{
    var messages = new List<string>();
    var passed = true;

    if (req.RequestType == "order")
    {
        if (req.Payload.TryGetValue("amount", out var amountObj) &&
            amountObj is System.Text.Json.JsonElement el &&
            el.TryGetDecimal(out var amount))
        {
            if (amount <= 0)
            {
                passed = false;
                messages.Add("Amount must be greater than zero");
            }
            if (amount > 100_000)
            {
                passed = false;
                messages.Add("Amount exceeds maximum allowed value of 100,000");
            }
        }
        else
        {
            passed = false;
            messages.Add("Amount is required for order validation");
        }
    }

    return Results.Ok(new { passed, messages, requestType = req.RequestType });
});

app.Run();

record ValidateRequest(string RequestType, Dictionary<string, object> Payload);
