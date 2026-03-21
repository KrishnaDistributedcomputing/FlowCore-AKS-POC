namespace FlowCore.CostService.Models;

public record CostEstimate
{
    public string ResourceType { get; init; } = string.Empty;
    public string ServiceName { get; init; } = string.Empty;
    public string SkuName { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public int Quantity { get; init; } = 1;
    public decimal EstimatedMonthlyCost { get; init; }
    public decimal EstimatedDailyCost { get; init; }
    public string CurrencyCode { get; init; } = "CAD";
    public DateTime PricingDate { get; init; } = DateTime.UtcNow;
}

public record CostSummary
{
    public decimal TotalMonthlyCost { get; init; }
    public decimal TotalDailyCost { get; init; }
    public string CurrencyCode { get; init; } = "CAD";
    public string Region { get; init; } = "canadacentral";
    public List<CostEstimate> Breakdown { get; init; } = [];
    public DateTime ComputedAtUtc { get; init; } = DateTime.UtcNow;
}

public record RegionComparison
{
    public string ResourceType { get; init; } = string.Empty;
    public string SkuName { get; init; } = string.Empty;
    public decimal PrimaryRegionPrice { get; init; }
    public string PrimaryRegion { get; init; } = "canadacentral";
    public decimal DrRegionPrice { get; init; }
    public string DrRegion { get; init; } = "canadaeast";
    public decimal PriceDifference { get; init; }
    public decimal PriceDifferencePercent { get; init; }
}

public record AdvisorRecommendation
{
    public string Id { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Impact { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
    public string DetailedDescription { get; init; } = string.Empty;
    public string ResourceId { get; init; } = string.Empty;
    public string ResourceType { get; init; } = string.Empty;
    public decimal? EstimatedSavings { get; init; }
    public string RecommendationUrl { get; init; } = string.Empty;
}

public record AdvisorSummary
{
    public int TotalRecommendations { get; init; }
    public int CostRecommendations { get; init; }
    public int SecurityRecommendations { get; init; }
    public int ReliabilityRecommendations { get; init; }
    public int PerformanceRecommendations { get; init; }
    public int OperationalExcellenceRecommendations { get; init; }
    public decimal TotalEstimatedSavings { get; init; }
    public List<AdvisorRecommendation> Recommendations { get; init; } = [];
}

public record OptimizationRecommendation
{
    public string RuleId { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Severity { get; init; } = "Medium";
    public string ResourceType { get; init; } = string.Empty;
    public string CurrentConfig { get; init; } = string.Empty;
    public string RecommendedConfig { get; init; } = string.Empty;
    public decimal CurrentMonthlyCost { get; init; }
    public decimal RecommendedMonthlyCost { get; init; }
    public decimal EstimatedMonthlySavings { get; init; }
    public string Rationale { get; init; } = string.Empty;
    public string ActionUrl { get; init; } = string.Empty;
}

public record OptimizationReport
{
    public decimal CurrentTotalMonthlyCost { get; init; }
    public decimal OptimizedTotalMonthlyCost { get; init; }
    public decimal TotalPotentialSavings { get; init; }
    public decimal SavingsPercent { get; init; }
    public List<OptimizationRecommendation> Recommendations { get; init; } = [];
    public DateTime ComputedAtUtc { get; init; } = DateTime.UtcNow;
}

public record AzureRetailPrice
{
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal RetailPrice { get; set; }
    public decimal UnitPrice { get; set; }
    public string ArmRegionName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string ServiceFamily { get; set; } = string.Empty;
    public string SkuName { get; set; } = string.Empty;
    public string ArmSkuName { get; set; } = string.Empty;
    public string MeterName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsPrimaryMeterRegion { get; set; }
    public DateTime EffectiveStartDate { get; set; }
}

public record AzureRetailPriceResponse
{
    public string? NextPageLink { get; set; }
    public int Count { get; set; }
    public List<AzureRetailPrice> Items { get; set; } = [];
}
