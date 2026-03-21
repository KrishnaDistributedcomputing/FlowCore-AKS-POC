# Module F - Core Microservices

## Objective

Deploy the main synchronous business services.

## Services Included

### F1. API Gateway

- Role: external routing
- Data ownership: none
- Interfaces: public API entry

### F2. Customer Service

- Role: customer profile management
- Data ownership: customer
- Interfaces:
  - create customer
  - update customer
  - get customer

### F3. Case / Order Service

- Role: main business transaction
- Data ownership: case_order
- Interfaces:
  - create case/order
  - update state
  - get details

### F4. Rules / Validation Service

- Role: validation and business rule checks
- Data ownership: optional small rules table or config-only
- Interfaces:
  - validate request
  - return pass/fail and rule messages

### F5. Reporting Service

- Role: read-only view aggregation
- Data ownership: reporting
- Interfaces:
  - summary reporting
  - lookup views
  - status views

### F6. Audit Service

- Role: immutable event audit trail
- Data ownership: audit
- Interfaces:
  - append event
  - query audit trail

## Inputs

- Container images
- Service contracts
- Config maps
- Secret references

## Outputs

- Running business services
- Internal and external APIs
- Service-to-service dependency model

## Assumptions

- Services are containerized .NET/C# apps
- APIs are REST for POC simplicity
- Rules service can remain synchronous
- Reporting service is read-optimized

## Acceptance Criteria

- All core services deploy
- Internal routing works
- APIs return expected responses
- End-to-end business flow completes

## Deliverables

- App source under apps folder
- OpenAPI contracts under this module folder
- Deployment manifests and health checks
