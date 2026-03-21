using FlowCore.CostService.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Net.Http.Json;
using System.Text.Json;

namespace FlowCore.CostService.Services;

public class AzurePricingService
{
    private const string BaseUrl = "https://prices.azure.com/api/retail/prices?api-version=2023-01-01-preview";
    private readonly HttpClient _http;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AzurePricingService> _logger;
    private readonly IConfiguration _config;

    // FlowCore resource definitions: what we actually deploy
    private static readonly List<ResourceDefinition> FlowCoreResources =
    [
        new("AKS System Nodes", "Virtual Machines", "Standard_D2s_v5", 2, 730),    // 2 nodes × 730 hrs/month
        new("AKS App Nodes", "Virtual Machines", "Standard_D4s_v5", 2, 730),
        new("AKS Worker Nodes", "Virtual Machines", "Standard_D2s_v5", 1, 730),
        new("PostgreSQL Flexible", "Azure Database for PostgreSQL", "Standard_B1ms", 1, 730),
        new("Redis Cache", "Azure Cache for Redis", "Basic", 1, 730),
        new("Container Registry", "Container Registry", "Basic", 1, 30),             // per day
        new("Service Bus", "Service Bus", "Standard", 1, 1),                         // base unit
        new("Key Vault", "Key Vault", "Standard", 1, 10000),                         // per 10K operations
        new("Log Analytics", "Log Analytics", "Pay-as-you-go", 1, 5),                // ~5 GB/month
        new("Application Insights", "Application Insights", "Enterprise", 1, 5),     // ~5 GB/month
    ];

    public AzurePricingService(HttpClient http, IDistributedCache cache,
        ILogger<AzurePricingService> logger, IConfiguration config)
    {
        _http = http;
        _cache = cache;
        _logger = logger;
        _config = config;
    }

    public async Task<CostSummary> GetFullEstimateAsync(string region = "canadacentral", CancellationToken ct = default)
    {
        var estimates = new List<CostEstimate>();

        foreach (var resource in FlowCoreResources)
        {
            var price = await GetPriceAsync(resource.ServiceName, resource.SkuName, region, ct);
            if (price is not null)
            {
                var monthlyCost = price.RetailPrice * resource.Quantity * resource.MonthlyUnits;
                estimates.Add(new CostEstimate
                {
                    ResourceType = resource.Label,
                    ServiceName = price.ServiceName,
                    SkuName = price.SkuName,
                    Region = region,
                    UnitPrice = price.RetailPrice,
                    UnitOfMeasure = price.UnitOfMeasure,
                    Quantity = resource.Quantity,
                    EstimatedMonthlyCost = Math.Round(monthlyCost, 2),
                    EstimatedDailyCost = Math.Round(monthlyCost / 30, 2),
                    CurrencyCode = price.CurrencyCode
                });
            }
        }

        var totalMonthly = estimates.Sum(e => e.EstimatedMonthlyCost);
        return new CostSummary
        {
            TotalMonthlyCost = totalMonthly,
            TotalDailyCost = Math.Round(totalMonthly / 30, 2),
            Region = region,
            Breakdown = estimates
        };
    }

    public async Task<CostEstimate?> GetEstimateByTypeAsync(string resourceType, string region = "canadacentral", CancellationToken ct = default)
    {
        var resource = FlowCoreResources.FirstOrDefault(r =>
            r.Label.Contains(resourceType, StringComparison.OrdinalIgnoreCase) ||
            r.ServiceName.Contains(resourceType, StringComparison.OrdinalIgnoreCase));

        if (resource is null) return null;

        var price = await GetPriceAsync(resource.ServiceName, resource.SkuName, region, ct);
        if (price is null) return null;

        var monthlyCost = price.RetailPrice * resource.Quantity * resource.MonthlyUnits;
        return new CostEstimate
        {
            ResourceType = resource.Label,
            ServiceName = price.ServiceName,
            SkuName = price.SkuName,
            Region = region,
            UnitPrice = price.RetailPrice,
            UnitOfMeasure = price.UnitOfMeasure,
            Quantity = resource.Quantity,
            EstimatedMonthlyCost = Math.Round(monthlyCost, 2),
            EstimatedDailyCost = Math.Round(monthlyCost / 30, 2),
            CurrencyCode = price.CurrencyCode
        };
    }

    public async Task<List<RegionComparison>> CompareRegionsAsync(
        string primaryRegion = "canadacentral", string drRegion = "canadaeast", CancellationToken ct = default)
    {
        var comparisons = new List<RegionComparison>();

        foreach (var resource in FlowCoreResources)
        {
            var primaryPrice = await GetPriceAsync(resource.ServiceName, resource.SkuName, primaryRegion, ct);
            var drPrice = await GetPriceAsync(resource.ServiceName, resource.SkuName, drRegion, ct);

            if (primaryPrice is not null && drPrice is not null)
            {
                var diff = drPrice.RetailPrice - primaryPrice.RetailPrice;
                var diffPercent = primaryPrice.RetailPrice > 0
                    ? Math.Round(diff / primaryPrice.RetailPrice * 100, 2)
                    : 0;

                comparisons.Add(new RegionComparison
                {
                    ResourceType = resource.Label,
                    SkuName = resource.SkuName,
                    PrimaryRegionPrice = primaryPrice.RetailPrice,
                    PrimaryRegion = primaryRegion,
                    DrRegionPrice = drPrice.RetailPrice,
                    DrRegion = drRegion,
                    PriceDifference = Math.Round(diff, 4),
                    PriceDifferencePercent = diffPercent
                });
            }
        }

        return comparisons;
    }

    public async Task<AzureRetailPrice?> GetPriceAsync(string serviceName, string skuName, string region, CancellationToken ct)
    {
        var cacheKey = $"price:{serviceName}:{skuName}:{region}";
        var cached = await _cache.GetStringAsync(cacheKey, ct);
        if (cached is not null)
            return JsonSerializer.Deserialize<AzureRetailPrice>(cached);

        var filter = $"serviceName eq '{serviceName}' and armRegionName eq '{region}' and priceType eq 'Consumption'";
        if (!string.IsNullOrEmpty(skuName) && skuName != "Standard" && skuName != "Basic" && skuName != "Pay-as-you-go" && skuName != "Enterprise")
            filter += $" and armSkuName eq '{skuName}'";
        else if (skuName == "Basic" || skuName == "Standard")
            filter += $" and skuName eq '{skuName}'";

        var url = $"{BaseUrl}&$filter={Uri.EscapeDataString(filter)}";

        try
        {
            var response = await _http.GetFromJsonAsync<AzureRetailPriceResponse>(url, ct);
            var price = response?.Items.FirstOrDefault(p => p.Type == "Consumption" && p.RetailPrice > 0);

            if (price is not null)
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(price),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6) }, ct);
            }

            return price;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch price for {Service}/{Sku} in {Region}", serviceName, skuName, region);
            return null;
        }
    }

    public async Task<List<AzureRetailPrice>> SearchPricingAsync(string serviceName, string? skuName, string region, CancellationToken ct)
    {
        var filter = $"serviceName eq '{serviceName}' and armRegionName eq '{region}' and priceType eq 'Consumption'";
        if (!string.IsNullOrEmpty(skuName))
            filter += $" and contains(skuName, '{skuName}')";

        var url = $"{BaseUrl}&$filter={Uri.EscapeDataString(filter)}";

        try
        {
            var response = await _http.GetFromJsonAsync<AzureRetailPriceResponse>(url, ct);
            return response?.Items.Where(p => p.RetailPrice > 0).ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to search pricing for {Service}", serviceName);
            return [];
        }
    }

    private record ResourceDefinition(string Label, string ServiceName, string SkuName, int Quantity, decimal MonthlyUnits);
}
