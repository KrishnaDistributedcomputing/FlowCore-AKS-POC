# Module A - Foundation

## Objective

Provision the minimum Azure foundation required to host the FlowCore POC.

## Components

- Resource Group
- Virtual Network
- Subnets
- Private DNS zones as needed
- Naming convention
- Tagging convention

## Inputs

- Azure region
- Subscription
- CIDR ranges
- Environment name
- Project prefix

## Outputs

- POC resource group
- Network boundaries
- Deployment naming standard

## Assumptions

- Single region
- Non-production environment
- Connectivity to Azure services is allowed
- DNS and routing are managed inside Azure scope

## Acceptance Criteria

- Resource group created
- VNet and subnets deployed
- Tags applied consistently
- Naming standard documented

## Deliverables

- Foundation IaC module under infra/bicep
- Naming and tagging standard document under docs
- Validation checklist and deployment evidence
