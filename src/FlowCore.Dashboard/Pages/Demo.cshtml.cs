using System.Diagnostics;
using FlowCore.Dashboard.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlowCore.Dashboard.Pages;

public class DemoModel : PageModel
{
    private readonly FlowCoreApiClient _api;

    public DemoModel(FlowCoreApiClient api) => _api = api;

    public List<DemoStep> Steps { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Step 1: Health check
        var step1 = await ExecuteStep("Gateway Health Check",
            "Verify API Gateway is responding",
            async () => await _api.GetAsync("/healthz"));
        Steps.Add(step1);

        // Step 2: Create Customer
        var step2 = await ExecuteStep("Create Customer",
            "POST to Customer Service via Gateway — tests synchronous CRUD",
            async () => await _api.PostAsync("/customers",
                new { name = "Demo User", email = "demo@flowcore.dev", phone = "+1-555-0100" }));
        Steps.Add(step2);

        // Step 3: Rules Validation
        var step3 = await ExecuteStep("Rules Validation",
            "POST to Rules Service — validates business rule (amount check)",
            async () => await _api.PostAsync("/rules/validate",
                new { orderId = "demo-001", amount = 500.00, customerId = "demo" }));
        Steps.Add(step3);

        // Step 4: Place Order
        var step4 = await ExecuteStep("Place Order",
            "POST to Order Service — creates order, triggers OrderPlaced event",
            async () => await _api.PostAsync("/orders",
                new { customerId = "demo-user", description = "POC Demo Order", amount = 500.00 }));
        Steps.Add(step4);

        // Step 5: Audit Trail
        var step5 = await ExecuteStep("Query Audit Trail",
            "GET from Audit Service — shows immutable event log",
            async () => await _api.GetAsync("/audit"));
        Steps.Add(step5);

        // Step 6: Reporting
        var step6 = await ExecuteStep("Reporting Summary",
            "GET from Reporting Service — aggregated read-only view",
            async () => await _api.GetAsync("/reporting/summary"));
        Steps.Add(step6);
    }

    private async Task<DemoStep> ExecuteStep(string title, string description, Func<Task<(bool ok, string detail)>> action)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var (ok, detail) = await action();
            sw.Stop();
            return new DemoStep
            {
                Title = title, Description = description, Executed = true,
                Success = ok, Detail = detail, ResponseTime = $"{sw.ElapsedMilliseconds}ms"
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new DemoStep
            {
                Title = title, Description = description, Executed = true,
                Success = false, Detail = ex.Message, ResponseTime = $"{sw.ElapsedMilliseconds}ms"
            };
        }
    }
}

public class DemoStep
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public bool Executed { get; set; }
    public bool Success { get; set; }
    public string Detail { get; set; } = "";
    public string ResponseTime { get; set; } = "";
}
