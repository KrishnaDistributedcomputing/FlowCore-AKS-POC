# Module I - Security and Secrets

## Objective

Externalize sensitive configuration and validate secure service access.

## Components

- Azure Key Vault
- Secret references
- Service identity model
- TLS for ingress
- Security rules

## Security Rules

- No secrets in source code
- No secrets in plain Kubernetes manifests
- DB credentials stored in Key Vault
- Messaging credentials stored securely
- TLS on external endpoint

## Inputs

- Secret names
- Access policy model
- Identity mapping

## Outputs

- Secure secret access
- Centralized secret storage
- Service identity pattern

## Assumptions

- Key Vault is the system of record for secrets
- Secret rotation is manual or semi-automated in POC
- POC uses simplified access scope

## Acceptance Criteria

- Services retrieve secrets without hardcoding
- Secret references work at runtime
- Rotation procedure documented
- No plaintext secrets in repositories or YAML

## Deliverables

- Key Vault and identity IaC under infra/bicep
- External secrets or CSI integration manifests
- Secret rotation and break-glass runbook
