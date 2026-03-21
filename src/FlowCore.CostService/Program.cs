using FlowCore.CostService.Services;
using FlowCore.Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHealthChecks();

builder.Services.AddStackExchangeRedisCache(opts =>
    opts.Configuration = builder.Configuration.GetConnectionString("Redis"));

builder.Services.AddHttpClient();
builder.Services.AddHttpClient<AzurePricingService>();
builder.Services.AddSingleton<AdvisorService>();
builder.Services.AddSingleton<OptimizationEngine>();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.MapHealthChecks("/healthz");

// ════════════════════════════════════════════════════
// M1. Cost Estimation (Azure Retail Prices API – public, no auth)
// ════════════════════════════════════════════════════

app.MapGet("/costs/estimate", async (AzurePricingService pricing, string? region, CancellationToken ct) =>
{
    var estimate = await pricing.GetFullEstimateAsync(region ?? "canadacentral", ct);
    return Results.Ok(estimate);
});

app.MapGet("/costs/estimate/{resourceType}", async (string resourceType, AzurePricingService pricing, string? region, CancellationToken ct) =>
{
    var estimate = await pricing.GetEstimateByTypeAsync(resourceType, region ?? "canadacentral", ct);
    return estimate is not null ? Results.Ok(estimate) : Results.NotFound($"No pricing found for '{resourceType}'");
});

app.MapGet("/costs/pricing", async (AzurePricingService pricing, string service, string? sku, string? region, CancellationToken ct) =>
{
    var prices = await pricing.SearchPricingAsync(service, sku, region ?? "canadacentral", ct);
    return Results.Ok(prices);
});

app.MapGet("/costs/compare-regions", async (AzurePricingService pricing, string? primary, string? dr, CancellationToken ct) =>
{
    var comparisons = await pricing.CompareRegionsAsync(primary ?? "canadacentral", dr ?? "canadaeast", ct);
    return Results.Ok(comparisons);
});

// ════════════════════════════════════════════════════
// M2. Azure Advisor Integration
// ════════════════════════════════════════════════════

app.MapGet("/costs/advisor", async (AdvisorService advisor, CancellationToken ct) =>
{
    var recs = await advisor.GetRecommendationsAsync(ct: ct);
    return Results.Ok(recs);
});

app.MapGet("/costs/advisor/cost", async (AdvisorService advisor, CancellationToken ct) =>
{
    var recs = await advisor.GetRecommendationsAsync("Cost", ct);
    return Results.Ok(recs);
});

app.MapGet("/costs/advisor/summary", async (AdvisorService advisor, CancellationToken ct) =>
{
    var recs = await advisor.GetRecommendationsAsync(ct: ct);
    return Results.Ok(new
    {
        recs.TotalRecommendations,
        recs.CostRecommendations,
        recs.SecurityRecommendations,
        recs.ReliabilityRecommendations,
        recs.PerformanceRecommendations,
        recs.OperationalExcellenceRecommendations,
        recs.TotalEstimatedSavings,
        Links = new
        {
            Cost = "https://learn.microsoft.com/en-us/azure/advisor/advisor-cost-recommendations",
            Security = "https://learn.microsoft.com/en-us/azure/advisor/advisor-security-recommendations",
            Reliability = "https://learn.microsoft.com/en-us/azure/advisor/advisor-high-availability-recommendations",
            Performance = "https://learn.microsoft.com/en-us/azure/advisor/advisor-performance-recommendations",
            OperationalExcellence = "https://learn.microsoft.com/en-us/azure/advisor/advisor-operational-excellence-recommendations",
            WellArchitected = "https://learn.microsoft.com/en-us/azure/well-architected/",
            CostOptimization = "https://learn.microsoft.com/en-us/azure/well-architected/cost-optimization/"
        }
    });
});

// ════════════════════════════════════════════════════
// M3. Optimization Engine
// ════════════════════════════════════════════════════

app.MapGet("/costs/optimize", async (OptimizationEngine engine, CancellationToken ct) =>
{
    var report = await engine.AnalyzeAsync(ct);
    return Results.Ok(report);
});

app.MapGet("/costs/optimize/rightsizing", async (OptimizationEngine engine, CancellationToken ct) =>
{
    var recs = await engine.GetRightSizingAsync(ct);
    return Results.Ok(recs);
});

app.MapGet("/costs/optimize/reserved", async (OptimizationEngine engine, CancellationToken ct) =>
{
    var recs = await engine.GetReservedInstancesAsync(ct);
    return Results.Ok(recs);
});

app.Run();
