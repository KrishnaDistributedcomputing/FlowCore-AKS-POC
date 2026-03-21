# Module J - CI/CD and Deployment

## Objective

Automate build, test, security scanning, and phased deployment to AKS.

## Components

- Source control branching strategy
- Build pipelines for all services
- Container image build and push to ACR
- Contract validation (OpenAPI and AsyncAPI)
- IaC validation and deployment automation
- Progressive deployment strategy (rolling or canary for POC)

## Inputs

- Pipeline platform (GitHub Actions or Azure DevOps)
- Environment promotion model
- Required quality gates
- Deployment credentials model

## Outputs

- Repeatable CI and CD workflows
- Artifact traceability from commit to deployment
- Automated deployment sequence aligned to module phases

## Assumptions

- POC can use simplified release approvals
- Build and deploy identities are centrally managed
- Rollback can be manual trigger for first POC iteration

## Acceptance Criteria

- Each service builds and publishes container image
- Contract lint and unit test gates run on PR
- Deployment pipeline can deploy all phases in order
- Rollback to previous stable image is validated

## Deliverables

- Pipeline definitions under pipelines folder
- Deployment scripts under infra/scripts and deploy folders
- CI/CD operational guide and release checklist
