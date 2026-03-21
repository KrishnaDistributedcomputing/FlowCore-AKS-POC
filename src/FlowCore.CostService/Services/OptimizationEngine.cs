using FlowCore.CostService.Models;

namespace FlowCore.CostService.Services;

public class OptimizationEngine
{
    private readonly AzurePricingService _pricing;
    private readonly ILogger<OptimizationEngine> _logger;

    public OptimizationEngine(AzurePricingService pricing, ILogger<OptimizationEngine> logger)
    {
        _pricing = pricing;
        _logger = logger;
    }

    public async Task<OptimizationReport> AnalyzeAsync(CancellationToken ct = default)
    {
        var currentEstimate = await _pricing.GetFullEstimateAsync("canadacentral", ct);
        var recommendations = new List<OptimizationRecommendation>();

        // OPT-001: Right-sizing – check if smaller VM SKUs would suffice for dev/POC
        recommendations.Add(await CheckRightSizingAsync("canadacentral", ct));

        // OPT-002: Reserved Instances comparison
        recommendations.Add(await CheckReservedInstancesAsync("canadacentral", ct));

        // OPT-003: Scale-to-zero for workers during off-hours
        recommendations.Add(GetScaleToZeroRecommendation(currentEstimate));

        // OPT-004: Tier optimization for Redis
        recommendations.Add(await CheckTierOptimizationAsync("canadacentral", ct));

        // OPT-005: Idle resources detection
        recommendations.Add(GetIdleResourcesRecommendation());

        // OPT-006: Region pricing comparison
        recommendations.Add(await CheckRegionPricingAsync(ct));

        recommendations = recommendations.Where(r => r is not null).ToList()!;
        var totalSavings = recommendations.Sum(r => r.EstimatedMonthlySavings);

        return new OptimizationReport
        {
            CurrentTotalMonthlyCost = currentEstimate.TotalMonthlyCost,
            OptimizedTotalMonthlyCost = currentEstimate.TotalMonthlyCost - totalSavings,
            TotalPotentialSavings = totalSavings,
            SavingsPercent = currentEstimate.TotalMonthlyCost > 0
                ? Math.Round(totalSavings / currentEstimate.TotalMonthlyCost * 100, 1)
                : 0,
            Recommendations = recommendations
        };
    }

    public async Task<List<OptimizationRecommendation>> GetRightSizingAsync(CancellationToken ct)
    {
        var rec = await CheckRightSizingAsync("canadacentral", ct);
        return [rec];
    }

    public async Task<List<OptimizationRecommendation>> GetReservedInstancesAsync(CancellationToken ct)
    {
        var rec = await CheckReservedInstancesAsync("canadacentral", ct);
        return [rec];
    }

    private async Task<OptimizationRecommendation> CheckRightSizingAsync(string region, CancellationToken ct)
    {
        // Check if D4s_v5 (app nodes) could be D2s_v5 for POC
        var currentPrice = await _pricing.GetPriceAsync("Virtual Machines", "Standard_D4s_v5", region, ct);
        var smallerPrice = await _pricing.GetPriceAsync("Virtual Machines", "Standard_D2s_v5", region, ct);

        var currentMonthly = (currentPrice?.RetailPrice ?? 0) * 2 * 730; // 2 app nodes
        var smallerMonthly = (smallerPrice?.RetailPrice ?? 0) * 2 * 730;

        return new OptimizationRecommendation
        {
            RuleId = "OPT-001",
            Category = "Right-sizing",
            Severity = "Medium",
            ResourceType = "AKS App Node Pool",
            CurrentConfig = "2x Standard_D4s_v5 (4 vCPU, 16 GB RAM)",
            RecommendedConfig = "2x Standard_D2s_v5 (2 vCPU, 8 GB RAM) — sufficient for POC workloads",
            CurrentMonthlyCost = Math.Round(currentMonthly, 2),
            RecommendedMonthlyCost = Math.Round(smallerMonthly, 2),
            EstimatedMonthlySavings = Math.Round(currentMonthly - smallerMonthly, 2),
            Rationale = "POC workloads typically use < 30% CPU. D2s_v5 provides sufficient capacity with autoscaling enabled.",
            ActionUrl = "https://learn.microsoft.com/en-us/azure/virtual-machines/sizes/general-purpose/dsv5-series"
        };
    }

    private async Task<OptimizationRecommendation> CheckReservedInstancesAsync(string region, CancellationToken ct)
    {
        // Lookup 1-year reservation pricing vs PAYG
        var payg = await _pricing.GetPriceAsync("Virtual Machines", "Standard_D2s_v5", region, ct);
        var paygMonthly = (payg?.RetailPrice ?? 0) * 5 * 730; // 5 total nodes
        var riEstimate = paygMonthly * 0.63m; // ~37% savings typical for 1yr RI

        return new OptimizationRecommendation
        {
            RuleId = "OPT-002",
            Category = "Reserved Instances",
            Severity = "Low",
            ResourceType = "AKS Node Pools (all)",
            CurrentConfig = "Pay-As-You-Go pricing for 5 VM instances",
            RecommendedConfig = "1-Year Reserved Instances (if POC transitions to production)",
            CurrentMonthlyCost = Math.Round(paygMonthly, 2),
            RecommendedMonthlyCost = Math.Round(riEstimate, 2),
            EstimatedMonthlySavings = Math.Round(paygMonthly - riEstimate, 2),
            Rationale = "Reserved Instances offer ~37% savings vs. PAYG for 1-year commitments. Recommended only if POC transitions to long-running production workload.",
            ActionUrl = "https://learn.microsoft.com/en-us/azure/cost-management-billing/reservations/save-compute-costs-reservations"
        };
    }

    private OptimizationRecommendation GetScaleToZeroRecommendation(CostSummary current)
    {
        // Workers + App nodes scaled down 14 hrs/day (evenings/weekends)
        var workerEstimate = current.Breakdown
            .Where(b => b.ResourceType.Contains("Worker"))
            .Sum(b => b.EstimatedMonthlyCost);
        var offHoursSavings = workerEstimate * 0.6m; // ~60% time off

        return new OptimizationRecommendation
        {
            RuleId = "OPT-003",
            Category = "Scale-to-Zero",
            Severity = "High",
            ResourceType = "AKS Worker Node Pool",
            CurrentConfig = "Worker nodes running 24/7",
            RecommendedConfig = "Scale worker pool to 0 during off-hours (6 PM - 8 AM + weekends) using infra/scripts/scale-down.sh",
            CurrentMonthlyCost = Math.Round(workerEstimate, 2),
            RecommendedMonthlyCost = Math.Round(workerEstimate - offHoursSavings, 2),
            EstimatedMonthlySavings = Math.Round(offHoursSavings, 2),
            Rationale = "Worker nodes process async events that are not needed outside business hours. Scale-to-zero eliminates idle compute costs.",
            ActionUrl = "https://learn.microsoft.com/en-us/azure/aks/scale-cluster#scale-the-cluster-nodes"
        };
    }

    private async Task<OptimizationRecommendation> CheckTierOptimizationAsync(string region, CancellationToken ct)
    {
        var current = await _pricing.GetPriceAsync("Azure Cache for Redis", "Basic", region, ct);
        var currentMonthly = (current?.RetailPrice ?? 0) * 730;

        return new OptimizationRecommendation
        {
            RuleId = "OPT-004",
            Category = "Tier Optimization",
            Severity = "Info",
            ResourceType = "Azure Cache for Redis",
            CurrentConfig = "Basic C1 (suitable for POC)",
            RecommendedConfig = "Confirm Basic tier is sufficient. Upgrade to Standard only if HA is required.",
            CurrentMonthlyCost = Math.Round(currentMonthly, 2),
            RecommendedMonthlyCost = Math.Round(currentMonthly, 2),
            EstimatedMonthlySavings = 0,
            Rationale = "Basic Redis is the most cost-effective tier for POC. Standard tier adds replication but doubles cost. Only upgrade if the POC requires cache HA.",
            ActionUrl = "https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/cache-overview#choosing-the-right-tier"
        };
    }

    private OptimizationRecommendation GetIdleResourcesRecommendation()
    {
        return new OptimizationRecommendation
        {
            RuleId = "OPT-005",
            Category = "Idle Resources",
            Severity = "Medium",
            ResourceType = "All Resources",
            CurrentConfig = "Manual identification required",
            RecommendedConfig = "Use Azure Advisor idle resource detection + infra/scripts/teardown.sh for full cleanup",
            CurrentMonthlyCost = 0,
            RecommendedMonthlyCost = 0,
            EstimatedMonthlySavings = 0,
            Rationale = "After POC validation, tear down the entire resource group to avoid ongoing charges. Use `infra/scripts/teardown.sh` for orchestrated cleanup.",
            ActionUrl = "https://learn.microsoft.com/en-us/azure/advisor/advisor-cost-recommendations#reduce-costs-by-eliminating-unprovisioned-expressroute-circuits"
        };
    }

    private async Task<OptimizationRecommendation> CheckRegionPricingAsync(CancellationToken ct)
    {
        var comparisons = await _pricing.CompareRegionsAsync("canadacentral", "canadaeast", ct);
        var cheaper = comparisons.Where(c => c.PriceDifference < 0).ToList();
        var cheaperCount = cheaper.Count;

        return new OptimizationRecommendation
        {
            RuleId = "OPT-006",
            Category = "Region Pricing",
            Severity = "Info",
            ResourceType = "All Resources",
            CurrentConfig = "Canada Central (primary region)",
            RecommendedConfig = $"Canada East has lower pricing for {cheaperCount}/{comparisons.Count} resource types. Use /costs/estimate?region=canadaeast for detailed comparison.",
            CurrentMonthlyCost = 0,
            RecommendedMonthlyCost = 0,
            EstimatedMonthlySavings = 0,
            Rationale = "Canada Central and Canada East typically have similar pricing. Regional price differences are usually < 5% for compute resources.",
            ActionUrl = "https://azure.microsoft.com/en-us/pricing/details/virtual-machines/linux/"
        };
    }
}
