using System.Diagnostics;
using FlowCore.Dashboard.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlowCore.Dashboard.Pages;

public class IndexModel : PageModel
{
    private readonly FlowCoreApiClient _api;

    public IndexModel(FlowCoreApiClient api) => _api = api;

    public int ServiceCount { get; set; }
    public int HealthyCount { get; set; }
    public List<ServiceStatus> Services { get; set; } = new();

    public async Task OnGetAsync()
    {
        var checks = new (string Name, string Ns, string Path)[]
        {
            ("API Gateway", "platform", "/healthz"),
            ("Customer Service", "apps", "/customers/health"),
            ("Order Service", "apps", "/orders/health"),
            ("Rules Service", "apps", "/rules/health"),
            ("Reporting Service", "apps", "/reporting/health"),
            ("Audit Service", "apps", "/audit/health"),
            ("Cost Service", "apps", "/costs/health"),
        };

        ServiceCount = checks.Length;

        foreach (var (name, ns, path) in checks)
        {
            var sw = Stopwatch.StartNew();
            bool healthy;
            try
            {
                var resp = await _api.CheckHealthAsync(path);
                healthy = resp;
            }
            catch
            {
                healthy = false;
            }
            sw.Stop();

            Services.Add(new ServiceStatus
            {
                Name = name,
                Namespace = ns,
                Healthy = healthy,
                ResponseTime = $"{sw.ElapsedMilliseconds}ms"
            });
        }

        HealthyCount = Services.Count(s => s.Healthy);
    }
}

public class ServiceStatus
{
    public string Name { get; set; } = "";
    public string Namespace { get; set; } = "";
    public bool Healthy { get; set; }
    public string ResponseTime { get; set; } = "";
}
