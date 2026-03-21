# Module N – AKS Change Management & Platform Volatility

## Objective

Azure Kubernetes Service evolves at a pace that is fundamentally different from traditional infrastructure. Kubernetes itself follows a four-month minor release cadence, Azure applies node image updates weekly, APIs are deprecated and removed on a fixed timeline, and add-on versions shift independently of the cluster. This module addresses the operational challenges that arise from this constant change and defines the strategies, tooling, and processes FlowCore uses to stay current without introducing instability.

---

## The Nature of the Challenge

AKS is not a static platform. It is a layered system where each layer moves on its own schedule, and changes at any layer can affect workloads running above it.

At the Kubernetes control plane level, minor versions are released roughly every four months. Each minor version is supported for twelve months from its release date. After that, the version falls out of support and the cluster must be upgraded or it becomes ineligible for Azure support and security patches. This means teams must plan and execute at least one Kubernetes version upgrade per year, and realistically two or three to stay within the supported window.

At the node image level, Azure publishes updated VM images for AKS nodes on a weekly basis. These images contain OS-level security patches, kernel updates, container runtime fixes, and updated versions of kubelet and other node-level components. Nodes running outdated images accumulate known vulnerabilities. The challenge is applying these updates without disrupting running workloads.

At the Kubernetes API level, resources and fields are deprecated in one version and removed in a later version. A manifest that deploys successfully on Kubernetes 1.29 may fail on 1.31 because an API version it references (such as `policy/v1beta1` for PodDisruptionBudgets) has been removed. These breaking changes are documented in upstream release notes but are easy to overlook when teams are focused on application development.

At the add-on and extension level, components like the Azure CNI network plugin, the CSI storage drivers, CoreDNS, kube-proxy, and the OMS agent for monitoring are updated on their own schedule. An AKS upgrade may change the version of these components, and the new version may behave differently — different default configurations, changed metric names, deprecated flags.

At the tooling level, Helm charts, kubectl plugins, Bicep provider versions, and GitHub Actions runners all evolve independently. A CI/CD pipeline that worked last month may fail because a GitHub Actions runner updated its bundled kubectl version, or because a Bicep provider changed its resource schema.

The cumulative effect is that even a team that makes zero application changes will still face platform-level drift that requires attention on a recurring basis.

---

## Kubernetes Version Lifecycle Strategy

FlowCore targets Kubernetes version 1.29 at initial deployment. The version lifecycle strategy ensures the cluster never falls more than one minor version behind the latest AKS-supported release.

**Version monitoring** begins with awareness. The FlowCore operations runbook includes a monthly check against the AKS release calendar. The Azure CLI command `az aks get-versions --location canadacentral --output table` returns the currently supported versions and their support status. When the currently deployed version shows a "preview" deprecation warning or when a newer stable version becomes available, the upgrade planning process begins.

**Upgrade planning** happens in three phases. First, the team reviews the Kubernetes changelog for the target version, focusing on removed APIs, changed defaults, and deprecated features. Second, the team runs `kubectl convert` or uses tools like Pluto and kubent to scan all deployed manifests and Helm charts for API references that will break in the target version. Third, the team updates any affected manifests in source control before the upgrade begins.

**Upgrade execution** uses the AKS blue-green node pool strategy. Rather than upgrading nodes in place, a new node pool running the target Kubernetes version is created alongside the existing pool. Workloads are gradually migrated to the new pool using node selectors or taints. Once all workloads are running on the new pool and health checks confirm stability, the old pool is drained and deleted. This approach allows instant rollback — if the new pool exhibits problems, traffic shifts back to the old pool with no data loss.

**Post-upgrade validation** runs the full CI/CD pipeline against the upgraded cluster. All Kubernetes manifests are reapplied, health endpoints are verified, and a smoke test confirms that the API Gateway routes requests successfully to all backend services. The upgrade is not considered complete until the pipeline passes end-to-end.

---

## Node Image Update Strategy

Node images are updated separately from Kubernetes version upgrades. FlowCore uses the AKS node image auto-upgrade channel set to `NodeImage`, which automatically applies the latest node image to each node pool on a rolling basis.

The auto-upgrade respects Pod Disruption Budgets. When a node needs to be reimaged, AKS cordons the node (preventing new pods from being scheduled), drains existing pods (respecting PDB constraints so that minimum availability is maintained), reimages the node with the latest image, and uncordons it to accept new pods. This process happens one node at a time within each pool, ensuring that workloads remain available throughout.

For the FlowCore POC environment, the maintenance window is configured to allow node image updates during off-hours — weeknights between 10 PM and 6 AM Eastern Time. This minimizes the chance that a node reimage disrupts active development or testing.

If a node image update introduces a regression — for example, a kubelet version that changes eviction behavior or a container runtime update that affects image pull performance — the team can pin the node pool to a specific node image version using `az aks nodepool update --node-image-version` and file an Azure support ticket while investigating.

---

## API Deprecation and Manifest Compatibility

Kubernetes API deprecation follows a predictable but strict lifecycle. An API version is first marked as deprecated, then removed after a defined number of releases. FlowCore addresses this with proactive scanning integrated into the CI/CD pipeline.

**Pre-deployment scanning** uses the Pluto open-source tool to analyze all YAML manifests in the `deploy/manifests/` directory. Pluto identifies any resource that references a deprecated or removed API version and reports the target version where the removal occurs. This scan runs as part of the contract validation stage in the GitHub Actions pipeline, alongside the OpenAPI and AsyncAPI linting. If Pluto detects a removed API reference for the target cluster version, the pipeline fails with a clear error message identifying the affected manifest and the required API version change.

**Manifest version pinning** is avoided. Rather than hardcoding API versions and updating them reactively, FlowCore manifests use the latest stable API version at the time of creation. When a new Kubernetes version deprecates a referenced API, the manifest is updated immediately in source control — not deferred to upgrade time.

**Common deprecation patterns** that affect FlowCore include the transition from `policy/v1beta1` to `policy/v1` for PodDisruptionBudgets (completed in Kubernetes 1.25), the transition from `autoscaling/v2beta2` to `autoscaling/v2` for HorizontalPodAutoscalers (completed in Kubernetes 1.26), and the ongoing evolution of `networking.k8s.io/v1` for NetworkPolicy resources. Each of these transitions requires updating the `apiVersion` field in the affected manifest and potentially adjusting field names or structures that changed between versions.

---

## Add-On and Extension Drift

AKS clusters include several managed add-ons that update independently. FlowCore uses three add-ons that require monitoring: the OMS agent (for Log Analytics integration), the Azure CNI network plugin, and the Azure Key Vault CSI driver (for future secret injection).

**OMS agent updates** can change the structure of container logs sent to Log Analytics. When the agent version changes, the Kusto queries used in dashboards and alerts may need adjustment. FlowCore mitigates this by using structured JSON logging from all .NET services rather than relying on plain-text log parsing. Structured logs with well-defined field names (correlationId, serviceName, eventType, timestamp) survive agent updates because the query targets specific JSON properties rather than line formats.

**Azure CNI updates** can affect pod IP allocation behavior, network policy enforcement, and DNS resolution. FlowCore uses Azure CNI with Azure network policy, and any behavioral change in how policies are evaluated could affect the network segmentation defined in `deploy/manifests/network-policies.yaml`. After an AKS upgrade that changes the CNI version, the team runs a connectivity validation that verifies each allowed network path (gateway to services, services to PostgreSQL, services to Redis, services to Service Bus) and confirms that denied paths (direct cross-namespace traffic, external access to internal services) remain blocked.

**Helm and third-party charts** are not currently used in FlowCore, but the architecture anticipates their introduction for components like cert-manager, external-dns, or NGINX ingress. When third-party Helm charts are added, their versions will be pinned in a `helmfile.yaml` or Flux configuration and updated through the same pull-request review process used for application code changes.

---

## CI/CD Pipeline Resilience to Platform Changes

The GitHub Actions CI/CD pipeline itself is vulnerable to platform drift. Runner images update monthly, bundled tool versions change, and GitHub occasionally deprecates action versions.

**Tool version pinning** ensures reproducible builds. The pipeline specifies exact versions for all critical tools: `actions/checkout@v4`, `azure/setup-kubectl@v4`, `azure/aks-set-context@v4`. The .NET SDK version is pinned through a `global.json` file in the repository root. Docker Buildx is initialized with a specific version. This prevents a runner update from silently changing the build environment.

**Action deprecation monitoring** is handled by Dependabot, which is configured to watch the `.github/workflows/` directory and create pull requests when GitHub Actions used in the pipeline have newer versions available. Major version bumps (which may include breaking changes) are reviewed manually before merging.

**Bicep provider versioning** follows the same principle. The Bicep templates in `infra/bicep/` target a specific API version for each Azure resource (for example, `Microsoft.ContainerService/managedClusters@2024-01-01`). When Azure introduces a new API version with additional properties or changed defaults, the Bicep templates are updated in a dedicated pull request with a clear description of what changed and why.

---

## Operational Runbook: Change Cadence Summary

The following recurring activities address AKS platform volatility and are scheduled as part of FlowCore operations.

**Weekly** — Review the AKS release notes feed for any urgent advisories, security patches, or breaking change announcements. Node image auto-upgrade handles routine weekly patches automatically.

**Monthly** — Run `az aks get-versions` to check the support status of the deployed Kubernetes version. Run Pluto against all manifests to detect newly deprecated APIs. Review Dependabot pull requests for GitHub Actions version updates.

**Quarterly** — Evaluate whether a Kubernetes minor version upgrade is needed. If the current version is more than one minor version behind the latest stable release, begin the upgrade planning process. Test the upgrade path in a separate resource group before applying to the primary cluster.

**On each AKS upgrade** — Run the full manifest compatibility scan. Execute the blue-green node pool migration. Validate all network policies. Confirm health check endpoints respond correctly. Run the CI/CD pipeline end-to-end against the upgraded cluster. Document the upgrade in the operations log with the before and after versions, any issues encountered, and resolution steps.

---

## Risk Register

**Risk: Forced upgrade due to version end-of-life.** If the team delays upgrades and the deployed Kubernetes version reaches end-of-life, Azure will eventually force an upgrade. Forced upgrades happen on Azure's schedule, not the team's, and may occur during business hours. Mitigation: maintain the quarterly review cadence and never allow the cluster to fall more than one minor version behind.

**Risk: Breaking API changes in manifests.** A Kubernetes upgrade removes an API version used in a deployed manifest, causing kubectl apply to fail. Mitigation: Pluto scanning in the CI/CD pipeline catches this before deployment. The quarterly upgrade planning phase includes a full manifest review.

**Risk: Node image regression.** A weekly node image update introduces a kubelet bug or container runtime regression that affects workload stability. Mitigation: Pod Disruption Budgets limit blast radius. Node image version pinning provides a rollback path. Off-hours maintenance windows reduce impact.

**Risk: Add-on behavioral change.** An AKS upgrade changes the version of a managed add-on (OMS agent, CNI, CoreDNS) in a way that affects logging, networking, or DNS resolution. Mitigation: structured logging reduces dependency on log format. Post-upgrade connectivity validation confirms network policy behavior. DNS resolution tests are included in the smoke test suite.

**Risk: CI/CD pipeline breakage from external changes.** A GitHub Actions runner update, a tool deprecation, or a provider schema change causes the pipeline to fail with no application code changes. Mitigation: tool version pinning, Dependabot monitoring, and a `global.json` for .NET SDK version control.

---

## Success Criteria

The AKS change management process is considered effective when the following conditions are met.

The cluster Kubernetes version is never more than one minor version behind the latest AKS-supported stable release. Node images are updated automatically with zero manual intervention and zero workload downtime. All manifests pass Pluto API deprecation scanning in the CI/CD pipeline with no warnings for the target cluster version. Kubernetes version upgrades complete within a single maintenance window using the blue-green node pool strategy, with documented rollback if issues arise. The CI/CD pipeline has no failures attributable to platform drift — all tool versions are pinned and monitored for updates through Dependabot.
