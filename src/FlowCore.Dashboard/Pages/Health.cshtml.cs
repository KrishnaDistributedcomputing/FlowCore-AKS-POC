using System.Diagnostics;
using FlowCore.Dashboard.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlowCore.Dashboard.Pages;

public class HealthModel : PageModel
{
    private readonly FlowCoreApiClient _api;

    public HealthModel(FlowCoreApiClient api) => _api = api;

    public List<ServiceHealthInfo> Services { get; set; } = new();

    public async Task OnGetAsync()
    {
        var checks = new (string Name, string Ns, string Path, string Endpoint, int Replicas)[]
        {
            ("API Gateway", "platform", "/healthz", "api-gateway:80", 2),
            ("Customer Service", "apps", "/customers/health", "customer-service:80", 2),
            ("Order Service", "apps", "/orders/health", "order-service:80", 2),
            ("Rules Service", "apps", "/rules/health", "rules-service:80", 1),
            ("Reporting Service", "apps", "/reporting/health", "reporting-service:80", 1),
            ("Audit Service", "apps", "/audit/health", "audit-service:80", 1),
            ("Cost Service", "apps", "/costs/health", "cost-service:80", 1),
        };

        foreach (var (name, ns, path, endpoint, replicas) in checks)
        {
            var sw = Stopwatch.StartNew();
            bool healthy;
            try { healthy = await _api.CheckHealthAsync(path); }
            catch { healthy = false; }
            sw.Stop();

            Services.Add(new ServiceHealthInfo
            {
                Name = name,
                Namespace = ns,
                Endpoint = endpoint,
                Healthy = healthy,
                ResponseTime = $"{sw.ElapsedMilliseconds}ms",
                Replicas = replicas
            });
        }
    }
}

public class ServiceHealthInfo
{
    public string Name { get; set; } = "";
    public string Namespace { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public bool Healthy { get; set; }
    public string ResponseTime { get; set; } = "";
    public int Replicas { get; set; }
}
