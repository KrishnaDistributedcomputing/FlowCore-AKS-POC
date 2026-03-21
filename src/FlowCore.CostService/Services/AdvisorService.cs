using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Identity;
using FlowCore.CostService.Models;

namespace FlowCore.CostService.Services;

public class AdvisorService
{
    private readonly IConfiguration _config;
    private readonly ILogger<AdvisorService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly Dictionary<string, string> AdvisorDocsLinks = new()
    {
        ["Cost"] = "https://learn.microsoft.com/en-us/azure/advisor/advisor-cost-recommendations",
        ["Security"] = "https://learn.microsoft.com/en-us/azure/advisor/advisor-security-recommendations",
        ["Reliability"] = "https://learn.microsoft.com/en-us/azure/advisor/advisor-high-availability-recommendations",
        ["OperationalExcellence"] = "https://learn.microsoft.com/en-us/azure/advisor/advisor-operational-excellence-recommendations",
        ["Performance"] = "https://learn.microsoft.com/en-us/azure/advisor/advisor-performance-recommendations",
    };

    public AdvisorService(IConfiguration config, ILogger<AdvisorService> logger, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AdvisorSummary> GetRecommendationsAsync(string? categoryFilter = null, CancellationToken ct = default)
    {
        var subscriptionId = _config["Azure:SubscriptionId"] ?? "e62428e7-08dd-4bc2-82e2-2c51586d9105";
        var resourceGroup = _config["Azure:ResourceGroup"] ?? "rg-flowcore-poc";

        try
        {
            var credential = new DefaultAzureCredential();
            var token = await credential.GetTokenAsync(
                new Azure.Core.TokenRequestContext(["https://management.azure.com/.default"]), ct);

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

            var url = $"https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.Advisor/recommendations?api-version=2023-01-01&$filter=resourceGroup eq '{resourceGroup}'";
            var response = await client.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonDocument.Parse(json);

            var recommendations = new List<AdvisorRecommendation>();

            foreach (var element in doc.RootElement.GetProperty("value").EnumerateArray())
            {
                var props = element.GetProperty("properties");
                var category = props.TryGetProperty("category", out var catProp) ? catProp.GetString() ?? "Unknown" : "Unknown";

                if (categoryFilter is not null && !category.Equals(categoryFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                var shortDesc = props.TryGetProperty("shortDescription", out var sdProp) ? sdProp : default;

                recommendations.Add(new AdvisorRecommendation
                {
                    Id = element.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty,
                    Category = category,
                    Impact = props.TryGetProperty("impact", out var impProp) ? impProp.GetString() ?? "Unknown" : "Unknown",
                    ShortDescription = shortDesc.ValueKind != JsonValueKind.Undefined && shortDesc.TryGetProperty("problem", out var probProp) ? probProp.GetString() ?? string.Empty : string.Empty,
                    DetailedDescription = shortDesc.ValueKind != JsonValueKind.Undefined && shortDesc.TryGetProperty("solution", out var solProp) ? solProp.GetString() ?? string.Empty : string.Empty,
                    ResourceId = props.TryGetProperty("resourceMetadata", out var rmProp) && rmProp.TryGetProperty("resourceId", out var ridProp) ? ridProp.GetString() ?? string.Empty : string.Empty,
                    ResourceType = props.TryGetProperty("impactedField", out var ifProp) ? ifProp.GetString() ?? string.Empty : string.Empty,
                    RecommendationUrl = AdvisorDocsLinks.GetValueOrDefault(category, "https://learn.microsoft.com/en-us/azure/advisor/")
                });
            }

            return new AdvisorSummary
            {
                TotalRecommendations = recommendations.Count,
                CostRecommendations = recommendations.Count(r => r.Category == "Cost"),
                SecurityRecommendations = recommendations.Count(r => r.Category == "Security"),
                ReliabilityRecommendations = recommendations.Count(r => r.Category == "Reliability"),
                PerformanceRecommendations = recommendations.Count(r => r.Category == "Performance"),
                OperationalExcellenceRecommendations = recommendations.Count(r => r.Category == "OperationalExcellence"),
                TotalEstimatedSavings = recommendations.Sum(r => r.EstimatedSavings ?? 0),
                Recommendations = recommendations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Advisor recommendations");
            return new AdvisorSummary
            {
                Recommendations =
                [
                    new AdvisorRecommendation
                    {
                        Category = "Info",
                        ShortDescription = "Azure Advisor requires authenticated access. Please ensure the service has Reader RBAC on the resource group.",
                        RecommendationUrl = "https://learn.microsoft.com/en-us/azure/advisor/advisor-overview"
                    }
                ]
            };
        }
    }
}
