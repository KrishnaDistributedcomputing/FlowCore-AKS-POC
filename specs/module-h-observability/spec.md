# Module H - Observability

## Objective

Provide end-to-end observability across platform, services, and workers.

## Components

- Azure Monitor workspace integration
- Log Analytics workspace
- Application Insights per critical service group
- Metrics collection (cluster, node, pod, app)
- Distributed tracing and correlation IDs
- Dashboards and alert rules

## Inputs

- Logging retention period
- Alert thresholds and severity mapping
- Dashboard personas (ops, engineering, product)
- Trace correlation standard

## Outputs

- Unified log and metric visibility
- Service health dashboards
- Alerting and incident signal baseline

## Assumptions

- Observability scope is POC-grade but production-style
- Correlation ID is propagated through sync and async paths
- Alert routing can target a shared team channel for POC

## Acceptance Criteria

- AKS and app logs are centralized
- Basic SLO views are available
- Alerts trigger on synthetic failure conditions
- Event trace can be followed across gateway, services, and workers

## Deliverables

- Observability configuration manifests
- Dashboard definitions and alert rules
- Incident triage quickstart guide
