# Module C - Shared Platform Services

## Objective

Deploy shared services needed by multiple applications.

## Components

- Azure Container Registry (ACR)
- Azure Cache for Redis
- Ingress DNS and certificate management
- Optional API Management for later phase

## Inputs

- Registry SKU
- Cache size
- Ingress hostname
- TLS approach

## Outputs

- Shared image registry
- Shared cache endpoint
- Standardized ingress path

## Assumptions

- Redis is used for hot reads and non-persistent caching
- ACR is the central image source
- APIM is optional for this POC

## Acceptance Criteria

- Images can be pushed and pulled from ACR
- Redis is reachable from AKS
- Ingress routes to a sample service

## Deliverables

- Shared services IaC module under infra/bicep
- Ingress DNS and TLS setup guide under docs
- Validation scripts and connectivity checks
