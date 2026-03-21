using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlowCore.Dashboard.Pages;

public class InfrastructureModel : PageModel
{
    public List<AzureResource> Resources { get; set; } = new();

    public void OnGet()
    {
        Resources = new List<AzureResource>
        {
            new("aks-flowcore-poc", "AKS Cluster", "Standard", "Container orchestration – K8s 1.33"),
            new("acrflowcorepoc", "Container Registry", "Basic", "Docker image store – 9 service images"),
            new("vnet-flowcore-poc", "Virtual Network", "—", "10.100.0.0/16 with 3 subnets"),
            new("psql-flowcore-poc", "PostgreSQL Flexible", "Burstable B1ms", "4 databases: customer, case_order, reporting, audit"),
            new("redis-flowcore-poc", "Azure Cache for Redis", "Basic C1", "Session and query caching"),
            new("sb-flowcore-poc", "Service Bus", "Standard", "Event backbone – flowcore-events topic"),
            new("kv-flowcore-poc", "Key Vault", "Standard", "Secrets management with RBAC"),
            new("log-flowcore-poc", "Log Analytics", "PerGB2018", "Centralized logging – 30 day retention"),
            new("ai-flowcore-poc", "Application Insights", "—", "APM telemetry for all services"),
        };
    }
}

public class AzureResource
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Sku { get; set; }
    public string Purpose { get; set; }

    public AzureResource(string name, string type, string sku, string purpose)
    {
        Name = name; Type = type; Sku = sku; Purpose = purpose;
    }
}
