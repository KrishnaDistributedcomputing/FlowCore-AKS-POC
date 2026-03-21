using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlowCore.Dashboard.Services;

public class FlowCoreApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<FlowCoreApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public FlowCoreApiClient(HttpClient http, ILogger<FlowCoreApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    // ── Customer Service ──
    public async Task<List<CustomerDto>> GetCustomersAsync()
    {
        try
        {
            var resp = await _http.GetAsync("/customers");
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<CustomerDto>>(JsonOpts) ?? [];
        }
        catch (Exception ex) { _logger.LogWarning(ex, "GetCustomers failed"); return []; }
    }

    public async Task<CustomerDto?> CreateCustomerAsync(CustomerDto customer)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("/customers", customer);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<CustomerDto>(JsonOpts);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "CreateCustomer failed"); return null; }
    }

    // ── Order Service ──
    public async Task<List<OrderDto>> GetOrdersAsync()
    {
        try
        {
            var resp = await _http.GetAsync("/orders");
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<OrderDto>>(JsonOpts) ?? [];
        }
        catch (Exception ex) { _logger.LogWarning(ex, "GetOrders failed"); return []; }
    }

    public async Task<OrderDto?> CreateOrderAsync(OrderDto order)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("/orders", order);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<OrderDto>(JsonOpts);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "CreateOrder failed"); return null; }
    }

    // ── Rules Service ──
    public async Task<RulesResultDto?> ValidateOrderAsync(decimal amount)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("/rules/validate", new { amount });
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<RulesResultDto>(JsonOpts);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "ValidateOrder failed"); return null; }
    }

    // ── Reporting Service ──
    public async Task<ReportingSummaryDto?> GetReportingSummaryAsync()
    {
        try
        {
            var resp = await _http.GetAsync("/reporting/summary");
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ReportingSummaryDto>(JsonOpts);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "GetReportingSummary failed"); return null; }
    }

    // ── Audit Service ──
    public async Task<List<AuditEntryDto>> GetAuditEventsAsync()
    {
        try
        {
            var resp = await _http.GetAsync("/audit");
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<AuditEntryDto>>(JsonOpts) ?? [];
        }
        catch (Exception ex) { _logger.LogWarning(ex, "GetAuditEvents failed"); return []; }
    }

    // ── Cost Service ──
    public async Task<CostEstimateDto?> GetCostEstimateAsync(string region = "canadacentral")
    {
        try
        {
            var resp = await _http.GetAsync($"/costs/estimate?region={region}");
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<CostEstimateDto>(JsonOpts);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "GetCostEstimate failed"); return null; }
    }

    public async Task<object?> GetAdvisorRecommendationsAsync()
    {
        try
        {
            var resp = await _http.GetAsync("/costs/advisor");
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<object>(JsonOpts);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "GetAdvisor failed"); return null; }
    }

    public async Task<object?> GetOptimizationAnalysisAsync()
    {
        try
        {
            var resp = await _http.GetAsync("/costs/optimize");
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<object>(JsonOpts);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "GetOptimization failed"); return null; }
    }

    // ── Health Checks ──
    public async Task<ServiceHealthDto> CheckServiceHealthAsync(string serviceName, string healthPath)
    {
        try
        {
            var resp = await _http.GetAsync(healthPath);
            return new ServiceHealthDto
            {
                ServiceName = serviceName,
                Status = resp.IsSuccessStatusCode ? "Healthy" : "Unhealthy",
                ResponseTimeMs = -1,
                LastChecked = DateTime.UtcNow
            };
        }
        catch
        {
            return new ServiceHealthDto
            {
                ServiceName = serviceName,
                Status = "Unreachable",
                ResponseTimeMs = -1,
                LastChecked = DateTime.UtcNow
            };
        }
    }

    public async Task<List<ServiceHealthDto>> CheckAllServicesHealthAsync()
    {
        var services = new (string Name, string Path)[]
        {
            ("API Gateway", "/healthz"),
            ("Customer Service", "/customers/../healthz"),
            ("Order Service", "/orders/../healthz"),
            ("Rules Service", "/rules/../healthz"),
            ("Reporting Service", "/reporting/../healthz"),
            ("Audit Service", "/audit/../healthz"),
            ("Cost Service", "/costs/../healthz")
        };

        var tasks = services.Select(s => CheckServiceHealthWithTimingAsync(s.Name, s.Path));
        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }

    private async Task<ServiceHealthDto> CheckServiceHealthWithTimingAsync(string name, string path)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var resp = await _http.GetAsync(path);
            sw.Stop();
            return new ServiceHealthDto
            {
                ServiceName = name,
                Status = resp.IsSuccessStatusCode ? "Healthy" : "Degraded",
                ResponseTimeMs = sw.ElapsedMilliseconds,
                LastChecked = DateTime.UtcNow
            };
        }
        catch
        {
            sw.Stop();
            return new ServiceHealthDto
            {
                ServiceName = name,
                Status = "Unreachable",
                ResponseTimeMs = sw.ElapsedMilliseconds,
                LastChecked = DateTime.UtcNow
            };
        }
    }
}

// ── DTOs ──
public class CustomerDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class OrderDto
{
    public Guid? Id { get; set; }
    public Guid? CustomerId { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class RulesResultDto
{
    public bool IsValid { get; set; }
    public string? Reason { get; set; }
}

public class ReportingSummaryDto
{
    public int TotalCustomers { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public Dictionary<string, int>? OrdersByStatus { get; set; }
}

public class AuditEntryDto
{
    public Guid? Id { get; set; }
    public string? EventType { get; set; }
    public string? EntityId { get; set; }
    public string? EntityType { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? Details { get; set; }
}

public class CostEstimateDto
{
    public string? Region { get; set; }
    public decimal TotalMonthly { get; set; }
    public List<CostLineItemDto>? LineItems { get; set; }
}

public class CostLineItemDto
{
    public string? ResourceType { get; set; }
    public string? Sku { get; set; }
    public decimal MonthlyCost { get; set; }
}

public class ServiceHealthDto
{
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = "Unknown";
    public long ResponseTimeMs { get; set; }
    public DateTime LastChecked { get; set; }
}
