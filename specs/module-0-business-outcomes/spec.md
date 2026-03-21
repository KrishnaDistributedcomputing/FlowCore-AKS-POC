# Module 0 – Core Business Drivers & Outcome Dimensions

## Objective

The FlowCore AKS POC exists to validate that a modern, cloud-native microservices architecture on Azure Kubernetes Service can replace legacy monolithic systems while delivering measurable improvements across seven standard outcome dimensions. Every technical decision made in Modules A through L traces back to one or more of these business drivers. This document defines the rationale, expected outcomes, and success criteria that justify the investment.

---

## Standard Outcome Dimensions

### 1. Modernization

The primary modernization driver is the transition away from monolithic application design toward a decomposed, independently deployable microservices architecture. Legacy systems are typically characterized by tightly coupled components, outdated frameworks, shared databases, and manual configuration processes. These characteristics slow development, increase risk of regressions, and make it difficult to adopt new technologies.

FlowCore addresses this by decomposing the business domain into six independent C# microservices built on .NET 8. Each service is containerized, has its own deployment lifecycle, and communicates with other services through well-defined REST APIs or asynchronous events on Azure Service Bus. No service shares direct database access with another — each owns its schema within a shared PostgreSQL instance, establishing clear data boundaries from day one.

Infrastructure provisioning moves from manual portal clicks and scripts to declarative Bicep templates stored in source control. Every Azure resource — from the virtual network to the Key Vault — is defined as code, versioned, and deployed through automated pipelines. This eliminates configuration drift and makes the entire environment reproducible.

The net effect is a system where individual business capabilities can evolve, scale, and deploy independently, without requiring coordination across the entire application.

---

### 2. Cloud-Native Scalability

Legacy systems typically run on fixed-capacity infrastructure where the only scaling option is to increase the size of the underlying virtual machine. This approach is expensive, slow, and unable to respond to sudden demand changes. FlowCore replaces this with elastic, container-based orchestration on Azure Kubernetes Service.

The AKS cluster is organized into three node pools — system, application, and worker — each with independent autoscaling policies. The system pool maintains the Kubernetes control plane components. The application pool hosts the six API-facing microservices and scales between one and four nodes based on CPU utilization. The worker pool hosts background processors like the Notification Worker and can scale all the way down to zero nodes when no messages are queued, eliminating idle compute costs entirely.

At the pod level, Horizontal Pod Autoscalers monitor CPU and memory utilization for each service and add or remove pod replicas as demand changes. Critical services like the Customer Service and Order Service maintain a minimum of two replicas at all times, ensuring availability even during rolling deployments or node failures.

Every service exposes Kubernetes readiness and liveness probes. The readiness probe prevents traffic from reaching a pod that is still starting up or has lost its database connection. The liveness probe restarts pods that have entered an unhealthy state. Together with Pod Disruption Budgets, these mechanisms ensure that the system self-heals without manual intervention.

Redis caching sits in front of the database for frequently accessed resources like customer profiles and reporting summaries, reducing database load during traffic spikes and improving response times.

---

### 3. Delivery Acceleration

The current state of many legacy systems involves manual build processes, multi-week release cycles, and no automated quality gates. Changes are built on developer machines, tested informally, and deployed through manual steps that are difficult to reproduce and audit.

FlowCore implements a fully automated CI/CD pipeline using GitHub Actions. The pipeline has three stages that run in sequence on every push to the main branch.

First, the contract validation stage runs. It installs the Spectral CLI and validates the OpenAPI 3.0.3 specification for all synchronous API contracts. It also validates the AsyncAPI 2.6.0 specification for the event-driven messaging contracts. If either contract fails validation, the pipeline stops — no code is built, no image is pushed. This ensures that API contracts are always consistent and correct before any deployment occurs.

Second, the build and test stage restores NuGet packages, compiles all eight .NET projects in the solution, and runs any available unit tests. A build failure stops the pipeline.

Third, the build and deploy stage runs. Each of the eight services is built as a Docker container image, tagged with the Git commit SHA for traceability, and pushed to Azure Container Registry. Once all images are available, the pipeline authenticates to the AKS cluster and applies the Kubernetes manifests, deploying all services in their correct namespaces.

Infrastructure changes follow a separate workflow. The Bicep templates are deployed through a dedicated GitHub Actions workflow that targets the subscription directly, using federated identity credentials — no secrets stored in the repository.

The target is a commit-to-deploy time of less than ten minutes for application changes.

---

### 4. User Experience Transformation

Legacy systems often depend on thick-client desktop applications that are tied to specific operating systems, require local installation, and only work from office networks. This limits accessibility and increases support overhead.

FlowCore takes an API-first approach. The YARP-based API Gateway serves as the single public entry point for all external traffic. It routes requests to the appropriate backend service based on URL path — /customers routes to the Customer Service, /orders to the Order Service, and so on. The gateway itself is stateless and horizontally scalable.

All service contracts are defined using OpenAPI 3.0.3. This means any frontend technology — a React web application, a mobile app built with .NET MAUI, a command-line tool, or a third-party integration — can consume the APIs using auto-generated client libraries. The API design is completely decoupled from any specific user interface.

The APIs are stateless by design. No server-side sessions are maintained. Authentication and authorization context travels with each request. This enables truly device-agnostic access — a user can start a workflow on a desktop browser and continue it on a mobile device without session conflicts.

The Kubernetes LoadBalancer service exposes the API Gateway on a public IP address, making the system accessible from anywhere with an internet connection while keeping all internal services isolated within the cluster network.

---

### 5. Cost Optimization

Infrastructure cost is a critical concern, especially for proof-of-concept environments that may run for weeks or months during validation. FlowCore implements multiple cost optimization levers that together can reduce POC infrastructure spend significantly compared to a traditional always-on deployment.

**Elastic scaling** is the primary mechanism. The AKS node autoscaler adjusts the number of virtual machines in each pool based on actual workload demand. During business hours when API traffic is active, the application pool scales up to handle the load. During evenings, weekends, and idle periods, it scales back down to the configured minimum. The worker node pool is configured with a minimum of zero, meaning it costs nothing when no background messages are being processed.

**Environment lifecycle management** ensures that non-production environments do not run indefinitely. The project includes three operational scripts. The scale-down script reduces all node pools to their minimum counts for off-hours operation, cutting compute costs by approximately 60%. The scale-up script restores normal capacity for business hours. The teardown script deletes the entire resource group and all contained resources in a single command, ensuring no orphaned resources accumulate charges.

**Right-sizing** applies to every resource. The PostgreSQL database uses a Burstable B1ms SKU — the most cost-effective tier that provides sufficient performance for POC workloads while allowing bursts when needed. Redis uses the Basic C1 tier. The Container Registry uses the Basic tier. These choices are deliberate — the smallest SKU that meets the POC requirements, with a clear upgrade path to production-grade tiers when the project graduates.

**Managed services** eliminate operational overhead. Azure manages patching, backups, high availability, and security updates for PostgreSQL, Redis, Service Bus, and Key Vault. The team focuses on application development rather than infrastructure maintenance.

The Cost Service (Module M) provides real-time cost estimation using the Azure Retail Prices API, integration with Azure Advisor for optimization recommendations, and a built-in optimization engine that analyzes the deployment against best practices.

---

### 6. Architectural Standardization

Moving from a monolithic architecture to a standardized microservices design requires deliberate decomposition across four layers: frontend, backend services, data, and integration.

**At the frontend layer**, the API Gateway (YARP reverse proxy) serves as the boundary between external consumers and internal services. No external client communicates directly with a backend service. The gateway handles routing, correlation ID injection, and will serve as the future attachment point for rate limiting and authentication middleware.

**At the backend services layer**, six microservices implement the core business logic. The Customer Service manages customer profiles and contact information. The Order Service handles the full lifecycle of business transactions — creation, validation, fulfillment, and failure. The Rules Service provides synchronous validation logic, evaluating business rules and returning pass/fail decisions. The Reporting Service aggregates read-optimized views for dashboards and status reports. The Audit Service maintains an immutable event trail. The Cost Service provides real-time infrastructure cost visibility. Each service owns exactly one business domain and exposes a focused API surface.

**At the data layer**, schema-per-service isolation enforces data ownership boundaries. All four domain databases — customer, case_order, reporting, and audit — reside on a single PostgreSQL Flexible Server instance, but each service accesses only its own schema. Cross-schema writes are prohibited. If one service needs data owned by another, it must call that service's API or consume its events. This rule is the foundation of eventual service extraction — when the time comes to split into fully independent databases, no cross-schema dependencies need to be unwound.

**At the integration layer**, Azure Service Bus provides the event backbone. Services publish domain events to a shared topic. Subscribers — the Notification Worker, Projection Worker, and Audit Worker — each have their own subscription with independent retry policies and dead-letter queues. Publishers and subscribers are completely decoupled. Adding a new consumer requires only creating a new subscription — no changes to the publishing service.

The five standardization principles that govern the entire architecture are: single responsibility (each service owns one domain), data ownership (schema-per-service with no shared writes), contract-first design (OpenAPI for synchronous, AsyncAPI for asynchronous), infrastructure-as-code (all resources defined in Bicep modules), and observable-by-default (health checks, structured logging, and Application Insights telemetry on every service).

---

### 7. Integration Modernization

Legacy integration patterns — file-based data exchange over FTP, batch scripts that run overnight, point-to-point connectors that break when either side changes — are replaced with modern, resilient, event-driven patterns.

**API-first integration** means every service contract is defined in OpenAPI 3.0.3 before implementation begins. The contract specifies request and response schemas, validation rules, and error formats. External systems integrate by consuming these standardized APIs rather than parsing proprietary file formats or screen-scraping legacy interfaces.

**Event-driven integration** replaces batch processing with real-time event flow. When a customer profile is updated, the Customer Service publishes a `CustomerUpdated` event to the Service Bus topic. When an order is placed, the Order Service publishes both an `OrderPlaced` event and a `CaseCreated` event. When an order reaches a terminal state (fulfilled or failed), a `NotificationRequested` event triggers the Notification Worker to dispatch an email or SMS. All domain actions also generate `AuditRecorded` events consumed by the Audit Worker.

**Pub-sub decoupling** means publishers do not know or care which consumers exist. The Notification Worker subscribes to `NotificationRequested` events. The Projection Worker subscribes to all events to maintain the reporting model. The Audit Worker subscribes to `AuditRecorded` events. Each subscription has its own delivery count, lock duration, and dead-letter policy. If a new analytics system needs to consume order events in the future, it simply creates a new subscription — no changes to the Order Service.

**Resilient messaging** ensures that transient failures do not lose events. Each subscription allows up to five retry attempts with a one-minute lock duration. Messages that cannot be processed after all retries are automatically moved to the dead-letter queue (DLQ) where they can be inspected, reprocessed, or archived. Message time-to-live is configured between three and seven days depending on the subscription, preventing queue buildup from unprocessed messages.

**Correlation tracking** ties the entire flow together. Every HTTP request entering the API Gateway receives a correlation ID (either from the `X-Correlation-ID` header or auto-generated). This ID propagates through all synchronous service-to-service calls via the CorrelationIdDelegatingHandler and is attached to all Service Bus messages published during the request. The Notification Worker, Projection Worker, and Audit Worker all log this correlation ID, enabling end-to-end tracing of a business transaction from the initial API call through every synchronous and asynchronous processing step.

---

## Traceability: Business Outcomes to Technical Modules

Each business outcome dimension maps directly to one or more technical modules in the FlowCore architecture. This traceability ensures that every infrastructure component and service exists to serve a defined business purpose.

**Modernization** is delivered primarily through Module F (Core Services) and Module A (Foundation). The six .NET 8 microservices represent the modernized business logic, while the Bicep IaC templates represent the modernized infrastructure provisioning model.

**Cloud-Native Scalability** is delivered through Module B (AKS Platform) and Module C (Shared Services). The AKS cluster with its autoscaling node pools and Horizontal Pod Autoscalers provides elastic compute. Redis cache in Module C absorbs read-heavy workloads and protects the database from traffic spikes.

**Delivery Acceleration** is delivered through Module J (CI/CD) and Module C (ACR). GitHub Actions workflows automate the entire build, validate, and deploy cycle. Azure Container Registry provides the artifact repository for all container images.

**User Experience Transformation** is delivered through the API Gateway component of Module F (Service F1). The YARP reverse proxy and OpenAPI contracts enable any frontend to integrate with the backend services.

**Cost Optimization** is delivered across Modules A through E (all infrastructure modules) and Module M (Cost Management). Burstable SKUs, node autoscaling, scale-to-zero worker pools, and environment lifecycle scripts all contribute to cost control. The Cost Service provides real-time visibility into current spend.

**Architectural Standardization** is delivered through Module F (Core Services), Module D (Data Layer), and Module E (Messaging). Schema-per-service isolation, the event backbone, and contract-first API design enforce the standardized architecture.

**Integration Modernization** is delivered through Module E (Messaging) and Module G (Workers). The Service Bus pub-sub model, dead-letter queue handling, and background worker processing replace legacy file-based and batch integration patterns.

---

## Success Metrics

These metrics are tied to the Module L exit criteria and define when the POC is considered successful.

**Modernization** is measured by the number of services deployed as independent containers. The target is all seven services (API Gateway, Customer, Order, Rules, Reporting, Audit, Cost) running as separate Kubernetes deployments.

**Cloud-Native Scalability** is measured by how quickly the pod autoscaler responds to increased load. The target is new pods becoming ready within 60 seconds of the autoscaler detecting a threshold breach.

**Delivery Acceleration** is measured by the end-to-end pipeline time from commit to deployment. The target is less than 10 minutes for a full build, validate, push, and deploy cycle.

**Cost Optimization** is measured by the POC monthly run rate against the defined budget. The specific budget threshold is set at project kickoff based on organizational constraints.

**Architectural Standardization** is measured by contract validation pass rate. The target is 100% of services passing OpenAPI and AsyncAPI contract linting in the CI/CD pipeline.

**Integration Modernization** is measured by event end-to-end latency — the time from when a service publishes an event to when the subscribing worker completes processing. The target is less than 5 seconds under normal load.

**Resilience** is measured by the recovery time observed during Module K failure scenarios. Each scenario (pod failure, node drain, worker failure, database reconnect, secret rotation, service rollback) must have a documented recovery time and any manual steps clearly identified.
