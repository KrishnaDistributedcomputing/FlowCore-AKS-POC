using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Identity;

namespace FlowCore.Dashboard.Services;

public class AzureResourceClient
{
    private readonly IConfiguration _config;
    private readonly ILogger<AzureResourceClient> _logger;
    private readonly IHttpClientFactory _httpFactory;

    public AzureResourceClient(IConfiguration config, ILogger<AzureResourceClient> logger, IHttpClientFactory httpFactory)
    {
        _config = config;
        _logger = logger;
        _httpFactory = httpFactory;
    }

    public async Task<List<AzureResourceDto>> GetResourcesAsync()
    {
        var subscriptionId = _config["Azure:SubscriptionId"] ?? "";
        var resourceGroup = _config["Azure:ResourceGroup"] ?? "rg-flowcore-poc";

        try
        {
            var credential = new DefaultAzureCredential();
            var token = await credential.GetTokenAsync(
                new Azure.Core.TokenRequestContext(["https://management.azure.com/.default"]));

            var client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

            var url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/resources?api-version=2021-04-01";
            var resp = await client.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            var resources = new List<AzureResourceDto>();
            foreach (var el in doc.RootElement.GetProperty("value").EnumerateArray())
            {
                var type = el.GetProperty("type").GetString() ?? "";
                resources.Add(new AzureResourceDto
                {
                    Name = el.GetProperty("name").GetString() ?? "",
                    Type = type,
                    Location = el.GetProperty("location").GetString() ?? "",
                    Kind = el.TryGetProperty("kind", out var k) ? k.GetString() : null,
                    Sku = el.TryGetProperty("sku", out var sku) && sku.TryGetProperty("name", out var sn) ? sn.GetString() : null,
                    Category = CategorizeResource(type),
                    Module = GetModuleForResource(type),
                    Icon = GetIconForResource(type)
                });
            }

            return resources.OrderBy(r => r.Category).ThenBy(r => r.Name).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch Azure resources");
            return GetFallbackResources();
        }
    }

    public async Task<AksClusterDto?> GetAksDetailsAsync()
    {
        var subscriptionId = _config["Azure:SubscriptionId"] ?? "";
        var resourceGroup = _config["Azure:ResourceGroup"] ?? "rg-flowcore-poc";

        try
        {
            var credential = new DefaultAzureCredential();
            var token = await credential.GetTokenAsync(
                new Azure.Core.TokenRequestContext(["https://management.azure.com/.default"]));

            var client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

            var url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.ContainerService/managedClusters/aks-flowcore-poc?api-version=2024-01-01";
            var resp = await client.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var props = doc.RootElement.GetProperty("properties");

            var pools = new List<NodePoolDto>();
            foreach (var pool in props.GetProperty("agentPoolProfiles").EnumerateArray())
            {
                pools.Add(new NodePoolDto
                {
                    Name = pool.GetProperty("name").GetString() ?? "",
                    VmSize = pool.GetProperty("vmSize").GetString() ?? "",
                    Count = pool.GetProperty("count").GetInt32(),
                    MinCount = pool.TryGetProperty("minCount", out var mi) ? mi.GetInt32() : 0,
                    MaxCount = pool.TryGetProperty("maxCount", out var ma) ? ma.GetInt32() : 0,
                    Mode = pool.TryGetProperty("mode", out var m) ? m.GetString() : ""
                });
            }

            return new AksClusterDto
            {
                Name = "aks-flowcore-poc",
                KubernetesVersion = props.GetProperty("kubernetesVersion").GetString() ?? "",
                Fqdn = props.TryGetProperty("fqdn", out var fqdn) ? fqdn.GetString() : "",
                PowerState = props.TryGetProperty("powerState", out var ps) && ps.TryGetProperty("code", out var code) ? code.GetString() : "Unknown",
                NodePools = pools
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch AKS details");
            return null;
        }
    }

    private static string CategorizeResource(string type) => type switch
    {
        var t when t.Contains("virtualNetworks") => "Networking",
        var t when t.Contains("privateDnsZones") => "Networking",
        var t when t.Contains("managedClusters") => "Compute",
        var t when t.Contains("ContainerRegistry") => "Containers",
        var t when t.Contains("flexibleServers") => "Data",
        var t when t.Contains("redis") => "Data",
        var t when t.Contains("ServiceBus") => "Messaging",
        var t when t.Contains("KeyVault") => "Security",
        var t when t.Contains("Insights") || t.Contains("OperationalInsights") || t.Contains("AlertRules") || t.Contains("Solutions") => "Observability",
        _ => "Other"
    };

    private static string GetModuleForResource(string type) => type switch
    {
        var t when t.Contains("virtualNetworks") || t.Contains("privateDnsZones") => "Module A",
        var t when t.Contains("managedClusters") => "Module B",
        var t when t.Contains("ContainerRegistry") || t.Contains("redis") => "Module C",
        var t when t.Contains("flexibleServers") => "Module D",
        var t when t.Contains("ServiceBus") => "Module E",
        var t when t.Contains("Insights") || t.Contains("OperationalInsights") || t.Contains("Solutions") => "Module H",
        var t when t.Contains("KeyVault") => "Module I",
        var t when t.Contains("AlertRules") => "Module H",
        _ => "—"
    };

    private static string GetIconForResource(string type) => type switch
    {
        var t when t.Contains("virtualNetworks") => "🌐",
        var t when t.Contains("privateDnsZones") => "🔗",
        var t when t.Contains("managedClusters") => "☸️",
        var t when t.Contains("ContainerRegistry") => "📦",
        var t when t.Contains("flexibleServers") => "🐘",
        var t when t.Contains("redis") => "⚡",
        var t when t.Contains("ServiceBus") => "📨",
        var t when t.Contains("KeyVault") => "🔐",
        var t when t.Contains("Insights/components") => "📊",
        var t when t.Contains("OperationalInsights") => "📋",
        var t when t.Contains("AlertRules") => "🔔",
        var t when t.Contains("Solutions") => "📈",
        _ => "🔧"
    };

    private static List<AzureResourceDto> GetFallbackResources() =>
    [
        new() { Name = "vnet-flowcore-poc", Type = "Microsoft.Network/virtualNetworks", Location = "canadacentral", Category = "Networking", Module = "Module A", Icon = "🌐" },
        new() { Name = "aks-flowcore-poc", Type = "Microsoft.ContainerService/managedClusters", Location = "canadacentral", Category = "Compute", Module = "Module B", Icon = "☸️" },
        new() { Name = "acrflowcorepoc", Type = "Microsoft.ContainerRegistry/registries", Location = "canadacentral", Category = "Containers", Module = "Module C", Icon = "📦", Sku = "Basic" },
        new() { Name = "redis-flowcore-poc", Type = "Microsoft.Cache/redis", Location = "canadacentral", Category = "Data", Module = "Module C", Icon = "⚡", Sku = "Basic C1" },
        new() { Name = "psql-flowcore-poc", Type = "Microsoft.DBforPostgreSQL/flexibleServers", Location = "canadacentral", Category = "Data", Module = "Module D", Icon = "🐘", Sku = "Standard_B1ms" },
        new() { Name = "sb-flowcore-poc", Type = "Microsoft.ServiceBus/namespaces", Location = "canadacentral", Category = "Messaging", Module = "Module E", Icon = "📨", Sku = "Standard" },
        new() { Name = "law-flowcore-poc", Type = "Microsoft.OperationalInsights/workspaces", Location = "canadacentral", Category = "Observability", Module = "Module H", Icon = "📋" },
        new() { Name = "ai-flowcore-poc", Type = "Microsoft.Insights/components", Location = "canadacentral", Category = "Observability", Module = "Module H", Icon = "📊" },
        new() { Name = "kv-flowcore-poc", Type = "Microsoft.KeyVault/vaults", Location = "canadacentral", Category = "Security", Module = "Module I", Icon = "🔐", Sku = "Standard" },
    ];
}

public class AzureResourceDto
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Location { get; set; } = "";
    public string? Kind { get; set; }
    public string? Sku { get; set; }
    public string Category { get; set; } = "";
    public string Module { get; set; } = "";
    public string Icon { get; set; } = "🔧";
}

public class AksClusterDto
{
    public string Name { get; set; } = "";
    public string KubernetesVersion { get; set; } = "";
    public string? Fqdn { get; set; }
    public string? PowerState { get; set; }
    public List<NodePoolDto> NodePools { get; set; } = [];
}

public class NodePoolDto
{
    public string Name { get; set; } = "";
    public string VmSize { get; set; } = "";
    public int Count { get; set; }
    public int MinCount { get; set; }
    public int MaxCount { get; set; }
    public string? Mode { get; set; }
}
