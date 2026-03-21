# Module M – Cost Management & Optimization

## Objective

Provide real-time cost visibility, Azure Advisor best-practice recommendations, and an optimization engine that analyzes deployed infrastructure against live Azure pricing and usage patterns.

---

## Components

### M1. Cost Estimation Engine (Azure Retail Prices API)

Uses the public [Azure Retail Prices REST API](https://learn.microsoft.com/en-us/rest/api/cost-management/retail-prices/azure-retail-prices) to fetch live pricing for all deployed resources, calculates estimated monthly/daily costs, and compares against budget thresholds.

**Endpoints:**
- `GET /costs/estimate` – Real-time cost estimate for all FlowCore infrastructure
- `GET /costs/estimate/{resourceType}` – Cost estimate for a specific resource type (AKS, PostgreSQL, Redis, etc.)
- `GET /costs/pricing?service={service}&sku={sku}&region={region}` – Raw Azure pricing lookup

### M2. Azure Advisor Integration

Pulls recommendations from [Azure Advisor REST API](https://learn.microsoft.com/en-us/rest/api/advisor/) across all five categories (Cost, Security, Reliability, Operational Excellence, Performance).

**Endpoints:**
- `GET /costs/advisor` – All Advisor recommendations for the POC resource group
- `GET /costs/advisor/cost` – Cost-specific recommendations only
- `GET /costs/advisor/summary` – Category summary with counts and estimated savings

### M3. Optimization Engine

Analyzes current deployment against best practices and Azure pricing to produce actionable optimization recommendations.

**Endpoints:**
- `GET /costs/optimize` – Full optimization report
- `GET /costs/optimize/rightsizing` – SKU right-sizing recommendations based on utilization
- `GET /costs/optimize/reserved` – Reserved Instance vs. Pay-As-You-Go comparison

**Optimization Rules:**
| Rule ID | Category | Description |
|---------|----------|-------------|
| OPT-001 | Right-sizing | Detect over-provisioned VM SKUs based on CPU/memory utilization |
| OPT-002 | Reserved Instances | Compare PAYG vs. 1yr/3yr RI pricing for persistent workloads |
| OPT-003 | Scale-to-Zero | Identify workloads that can scale to zero during off-hours |
| OPT-004 | Tier Optimization | Suggest lower-cost SKU tiers where SLA allows |
| OPT-005 | Idle Resources | Flag unused or orphaned resources |
| OPT-006 | Region Pricing | Compare pricing across Canada Central vs. Canada East |

---

## Inputs

- Azure subscription ID
- Resource group name
- Azure Advisor API credentials (managed identity or service principal)
- Azure Retail Prices API (public, no auth required)
- Cost Management API credentials

## Outputs

- Real-time cost dashboard data
- Advisor recommendation feed
- Optimization report with specific savings estimates
- Budget tracking and alerting

## Azure Resource Pricing Sources

| Resource | Pricing Model | API Filter |
|----------|---------------|-----------|
| AKS Node Pools | Virtual Machines per-hour | `serviceName eq 'Virtual Machines' and armSkuName eq 'Standard_D4s_v5'` |
| PostgreSQL Flexible | vCore per-hour + Storage per-GB | `serviceName eq 'Azure Database for PostgreSQL'` |
| Redis Cache | Per-hour by tier | `serviceName eq 'Azure Cache for Redis'` |
| Service Bus | Per-million operations + base | `serviceName eq 'Service Bus'` |
| Container Registry | Per-day by tier | `serviceName eq 'Container Registry'` |
| Key Vault | Per-transaction | `serviceName eq 'Key Vault'` |
| Log Analytics | Per-GB ingested | `serviceName eq 'Log Analytics'` |
| Application Insights | Per-GB ingested | `serviceName eq 'Application Insights'` |

## Assumptions

- Azure Retail Prices API is public (no authentication required)
- Advisor API requires Azure RBAC Reader access on the resource group
- Cost Management API requires Cost Management Reader role
- Live pricing may have 24-hour cache lag vs. actual billing

## Acceptance Criteria

- Cost estimation returns current pricing for all FlowCore resources
- Advisor recommendations are retrieved and categorized
- Optimization engine produces at least 3 actionable recommendations
- Budget threshold alerts are configurable
- Region comparison (Canada Central vs. Canada East) is available

## Deliverables

- Cost Management service under `src/FlowCore.CostService`
- Bicep module for budget alerts under `infra/bicep/modules`
- K8s deployment manifest under `deploy/manifests`
- Cost estimation API aligned with OpenAPI spec
