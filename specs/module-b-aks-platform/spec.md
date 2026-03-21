# Module B - AKS Platform

## Objective

Deploy a production-style AKS baseline suitable for microservices validation.

## Components

- AKS cluster
- System node pool
- Application node pool
- Optional worker node pool
- Ingress controller
- Cluster namespaces

## Namespaces

- platform
- apps
- workers
- observability

## Inputs

- Kubernetes version
- Node pool VM sizes
- Min and max node counts
- Ingress strategy
- Namespace policy model

## Outputs

- Running AKS cluster
- Segmented namespaces
- Ready platform for application onboarding

## Assumptions

- One AKS cluster is sufficient for this POC
- Cluster is zonal only if region and SKU support it
- Public ingress is acceptable for POC with controlled access

## Acceptance Criteria

- AKS cluster deploys successfully
- Ingress is reachable
- Separate namespaces exist
- Test pod deploys successfully

## Deliverables

- AKS IaC module under infra/bicep
- Baseline ingress manifests under deploy/manifests
- Namespace policy definitions and validation evidence
