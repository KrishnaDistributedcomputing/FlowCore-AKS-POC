# Module 0 – Core Business Drivers & Outcome Dimensions

## Objective

Define the business rationale, expected outcomes, and value drivers that justify the FlowCore AKS POC investment. This module anchors all technical decisions (Modules A–L) to measurable business outcomes.

---

## Standard Outcome Dimensions

### 1. Modernization

**Goal:** Transition from legacy technologies to a modern, maintainable architecture.

| Current State | Target State |
|---------------|-------------|
| Monolithic application | Microservices-based design |
| Tight coupling between components | Loose coupling via APIs and events |
| Outdated frameworks and dependencies | .NET 8, containerized workloads |
| Manual configuration and patching | Infrastructure-as-Code (Bicep) |

**How FlowCore Delivers:**
- Six independent C# microservices replace monolithic business logic
- Event-driven decoupling via Azure Service Bus
- Each service owns its data schema — no shared database coupling
- Bicep IaC eliminates manual infrastructure provisioning

---

### 2. Cloud-Native Scalability

**Goal:** Adopt containerization and orchestration to enable horizontal scaling and high availability.

| Current State | Target State |
|---------------|-------------|
| Fixed-capacity infrastructure | Elastic container orchestration (AKS) |
| Scale-up only | Scale-out with pod autoscaling |
| Single-instance deployments | Multi-replica, zone-aware deployments |
| Manual failover | Automated health checks and self-healing |

**How FlowCore Delivers:**
- AKS cluster with autoscaling node pools (system, apps, workers)
- Kubernetes readiness and liveness probes on every service
- Worker services scale independently from API services
- Redis caching reduces database load during traffic spikes

---

### 3. Delivery Acceleration

**Goal:** Implement CI/CD pipelines to shift from manual releases to automated deployments.

| Current State | Target State |
|---------------|-------------|
| Manual build and deploy | GitHub Actions CI/CD |
| Release cycles measured in weeks | Continuous delivery on merge |
| No contract validation | OpenAPI + AsyncAPI linting in pipeline |
| Manual infrastructure changes | Automated Bicep deployments |

**How FlowCore Delivers:**
- Two GitHub Actions workflows: infrastructure deploy + service build/deploy
- Container images built, tagged, and pushed to ACR on every commit
- Kubernetes manifests applied automatically via pipeline
- Infrastructure deployed declaratively from Bicep templates

---

### 4. User Experience Transformation

**Goal:** Move from thick-client/desktop applications to web-based interfaces with device-agnostic access.

| Current State | Target State |
|---------------|-------------|
| Desktop-bound thick clients | RESTful APIs accessible from any client |
| Platform-specific UIs | API-first design enabling web, mobile, CLI |
| Coupled presentation and business logic | Decoupled API Gateway + backend services |
| Location-dependent access | Anywhere access via public ingress |

**How FlowCore Delivers:**
- YARP-based API Gateway provides a single public entry point
- OpenAPI 3.0.3 contract enables any frontend framework to integrate
- Stateless API design supports session-free, device-agnostic access
- LoadBalancer ingress exposes services securely

---

### 5. Cost Optimization

**Goal:** Reduce infrastructure and operational overhead through elastic scaling and environment lifecycle management.

| Current State | Target State |
|---------------|-------------|
| Always-on, fixed-size infrastructure | Elastic scaling (scale up/down to demand) |
| Permanent non-production environments | Environment lifecycle management (auto shutdown/deletion) |
| Over-provisioned resources | Right-sized, burstable SKUs for POC |
| High operational overhead | Managed services (AKS, PostgreSQL Flexible, Redis, Service Bus) |

**How FlowCore Delivers:**
- AKS autoscaler reduces nodes to minimum during idle periods
- Worker node pool scales from 0 when no background work is queued
- Burstable PostgreSQL SKU (`Standard_B1ms`) for cost-effective POC
- Basic Redis and ACR SKUs sized for validation, not production load
- Infrastructure-as-Code enables full environment teardown and recreation

**Cost Optimization Levers:**

| Lever | Mechanism |
|-------|-----------|
| Elastic scaling | AKS node autoscaler: min 0–1 → max per pool |
| Environment lifecycle | `az group delete` tears down entire POC in one command |
| Right-sizing | Burstable VM and database SKUs for non-production |
| Managed services | No patching, backup, or HA management overhead |

---

### 6. Architectural Standardization

**Goal:** Move toward a modular, microservices-based design that decouples frontend, backend services, data, and integrations.

| Layer | FlowCore Implementation |
|-------|------------------------|
| **Frontend** | Decoupled — API Gateway (YARP) serves as the boundary |
| **Backend Services** | 6 independent microservices, each with single responsibility |
| **Data** | Schema-per-service isolation on shared PostgreSQL |
| **Integration** | Event-driven via Azure Service Bus topics and subscriptions |

**How FlowCore Delivers:**
- API Gateway → Customer Service → Order Service → Rules Service (synchronous chain)
- Order Service → Service Bus → Notification Worker (asynchronous chain)
- Audit Service independently records all domain events
- Reporting Service reads from its own schema — no cross-service data access
- Each service has its own Dockerfile, deployment manifest, and health endpoint

**Standardization Principles Applied:**

| Principle | Implementation |
|-----------|---------------|
| Single Responsibility | Each service owns one business domain |
| Data Ownership | Schema-per-service with no shared writes |
| Contract-First | OpenAPI for sync, AsyncAPI for async |
| Infrastructure-as-Code | All resources defined in Bicep modules |
| Observable by Default | Health checks, structured logging, App Insights |

---

### 7. Integration Modernization

**Goal:** Replace legacy integrations (file-based, scripts) with API-first and event-driven patterns to improve interoperability and extensibility.

| Current State | Target State |
|---------------|-------------|
| File-based data exchange (CSV, FTP) | REST APIs with OpenAPI contracts |
| Batch scripts and scheduled jobs | Event-driven processing via Service Bus |
| Point-to-point integrations | Topic/subscription pub-sub model |
| Brittle, fragile connectors | Resilient messaging with DLQ and retry |

**How FlowCore Delivers:**
- **API-First:** All service contracts defined in OpenAPI 3.0.3
- **Event-Driven:** 5 core event types published to Service Bus topics
- **Pub-Sub:** Multiple subscribers (notification, projection, audit) consume independently
- **Resilient Messaging:** Dead-letter queues, configurable retry counts, message TTL
- **Extensible:** New consumers subscribe to existing topics without changing publishers

**Core Event Types:**

| Event | Publisher | Consumers |
|-------|-----------|-----------|
| `CustomerUpdated` | Customer Service | Projection Worker, Audit Worker |
| `CaseCreated` | Order Service | Projection Worker, Audit Worker |
| `OrderPlaced` | Order Service | Projection Worker, Audit Worker |
| `NotificationRequested` | Order Service | Notification Worker |
| `AuditRecorded` | All Services | Audit Worker |

---

## Traceability: Business Outcomes → Technical Modules

| Business Outcome | Primary Modules | Evidence |
|-----------------|-----------------|----------|
| Modernization | F (Services), A (Foundation) | .NET 8 microservices, Bicep IaC |
| Cloud-Native Scalability | B (AKS), C (Shared Services) | Autoscaling node pools, Redis cache |
| Delivery Acceleration | J (CI/CD), C (ACR) | GitHub Actions, container registry |
| UX Transformation | F1 (API Gateway) | YARP reverse proxy, OpenAPI contract |
| Cost Optimization | A–E (Infrastructure) | Burstable SKUs, autoscaler, IaC teardown |
| Architectural Standardization | F (Services), D (Data), E (Messaging) | Schema isolation, event backbone |
| Integration Modernization | E (Messaging), G (Workers) | Service Bus pub-sub, DLQ handling |

---

## Success Metrics (Tied to Module L – Exit Criteria)

| Dimension | Metric | Target |
|-----------|--------|--------|
| Modernization | Services deployed as independent containers | 7/7 |
| Scalability | Pod autoscaler responds to load within threshold | < 60s |
| Delivery | Commit-to-deploy pipeline time | < 10 min |
| Cost | POC monthly run rate within budget | Defined at kickoff |
| Standardization | All services pass contract validation | 100% |
| Integration | Event end-to-end latency (publish → consume) | < 5s |
| Resilience | Recovery time for Module K scenarios | Documented |
