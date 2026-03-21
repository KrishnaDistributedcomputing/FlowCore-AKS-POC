# Module D - Data Layer

## Objective

Provide a relational persistence layer for the microservices.

## Components

- Azure Database for PostgreSQL Flexible Server
- Separate schemas by domain
- DB connection policy
- Migration framework
- Data ownership model

## Data Ownership Model

- customer schema -> Customer Service
- case_order schema -> Case/Order Service
- reporting schema -> Reporting Service
- audit schema -> Audit Service

## Inputs

- PostgreSQL SKU
- Storage size
- HA mode
- Schema ownership rules

## Outputs

- PostgreSQL instance
- Domain-aligned schemas
- DB access model per service

## Assumptions

- Single PostgreSQL server for the POC
- Separate schemas provide sufficient early service isolation
- Cross-schema writes are not allowed outside approved interfaces

## Acceptance Criteria

- PostgreSQL deployed
- Schemas created
- Each service connects only to its owned schema and tables
- Migration scripts run automatically

## Deliverables

- Data-layer IaC module under infra/bicep
- SQL migrations under apps service folders
- Data ownership and access-control rules in docs
