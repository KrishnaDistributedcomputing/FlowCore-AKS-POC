// ──────────────────────────────────────────────────
// FlowCore AKS POC – Main Orchestrator
// Deploys Modules A → I in dependency order
// ──────────────────────────────────────────────────

targetScope = 'subscription'

@description('Environment name')
param environment string = 'poc'

@description('Project prefix for naming')
param projectPrefix string = 'flowcore'

@description('Primary Azure region')
param location string = 'canadacentral'

@description('DR Azure region')
param drLocation string = 'canadaeast'

// ── Networking ──
param vnetAddressPrefix string = '10.100.0.0/16'
param aksSubnetPrefix string = '10.100.0.0/22'
param servicesSubnetPrefix string = '10.100.4.0/24'
param dbSubnetPrefix string = '10.100.5.0/24'

// ── AKS ──
param kubernetesVersion string = '1.33'
param systemNodeCount int = 2
param appNodeCount int = 2
param workerNodeCount int = 1
param systemNodeVmSize string = 'Standard_D2s_v5'
param appNodeVmSize string = 'Standard_D4s_v5'
param workerNodeVmSize string = 'Standard_D2s_v5'

// ── Shared Services ──
param acrSku string = 'Basic'
param redisSku string = 'Basic'
param redisCapacity int = 1

// ── Data Layer ──
param postgresqlSku string = 'Standard_B1ms'
param postgresqlStorageSizeGB int = 32
@secure()
param postgresAdminLogin string

// ── Messaging ──
param serviceBusSku string = 'Standard'

// ── Derived names ──
var rgName = 'rg-${projectPrefix}-${environment}'
var tags = {
  project: projectPrefix
  environment: environment
  managedBy: 'bicep'
  module: 'main'
}

// ════════════════════════════════════════════════════
// MODULE A – Foundation
// ════════════════════════════════════════════════════
resource rg 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: rgName
  location: location
  tags: tags
}

module foundation 'modules/foundation.bicep' = {
  scope: rg
  name: 'foundation'
  params: {
    projectPrefix: projectPrefix
    environment: environment
    location: location
    vnetAddressPrefix: vnetAddressPrefix
    aksSubnetPrefix: aksSubnetPrefix
    servicesSubnetPrefix: servicesSubnetPrefix
    dbSubnetPrefix: dbSubnetPrefix
    tags: tags
  }
}

// ════════════════════════════════════════════════════
// MODULE I – Security (Key Vault – early, needed by others)
// ════════════════════════════════════════════════════
module keyvault 'modules/keyvault.bicep' = {
  scope: rg
  name: 'keyvault'
  params: {
    projectPrefix: projectPrefix
    environment: environment
    location: location
    tags: tags
  }
}

// ════════════════════════════════════════════════════
// MODULE H – Observability (Log Analytics + App Insights)
// ════════════════════════════════════════════════════
module observability 'modules/observability.bicep' = {
  scope: rg
  name: 'observability'
  params: {
    projectPrefix: projectPrefix
    environment: environment
    location: location
    tags: tags
  }
}

// ════════════════════════════════════════════════════
// MODULE B – AKS Platform
// ════════════════════════════════════════════════════
module aks 'modules/aks.bicep' = {
  scope: rg
  name: 'aks'
  params: {
    projectPrefix: projectPrefix
    environment: environment
    location: location
    kubernetesVersion: kubernetesVersion
    aksSubnetId: foundation.outputs.aksSubnetId
    systemNodeCount: systemNodeCount
    appNodeCount: appNodeCount
    workerNodeCount: workerNodeCount
    systemNodeVmSize: systemNodeVmSize
    appNodeVmSize: appNodeVmSize
    workerNodeVmSize: workerNodeVmSize
    logAnalyticsWorkspaceId: observability.outputs.logAnalyticsWorkspaceId
    tags: tags
  }
}

// ════════════════════════════════════════════════════
// MODULE C – Shared Platform Services
// ════════════════════════════════════════════════════
module acr 'modules/acr.bicep' = {
  scope: rg
  name: 'acr'
  params: {
    projectPrefix: projectPrefix
    environment: environment
    location: location
    acrSku: acrSku
    tags: tags
  }
}

module redis 'modules/redis.bicep' = {
  scope: rg
  name: 'redis'
  params: {
    projectPrefix: projectPrefix
    environment: environment
    location: location
    redisSku: redisSku
    redisCapacity: redisCapacity
    tags: tags
  }
}

// ════════════════════════════════════════════════════
// MODULE D – Data Layer
// ════════════════════════════════════════════════════
module postgresql 'modules/postgresql.bicep' = {
  scope: rg
  name: 'postgresql'
  params: {
    projectPrefix: projectPrefix
    environment: environment
    location: location
    postgresqlSku: postgresqlSku
    storageSizeGB: postgresqlStorageSizeGB
    adminLogin: postgresAdminLogin
    dbSubnetId: foundation.outputs.dbSubnetId
    privateDnsZoneId: foundation.outputs.postgresPrivateDnsZoneId
    tags: tags
  }
}

// ════════════════════════════════════════════════════
// MODULE E – Messaging Layer
// ════════════════════════════════════════════════════
module servicebus 'modules/servicebus.bicep' = {
  scope: rg
  name: 'servicebus'
  params: {
    projectPrefix: projectPrefix
    environment: environment
    location: location
    serviceBusSku: serviceBusSku
    tags: tags
  }
}

// ════════════════════════════════════════════════════
// Outputs
// ════════════════════════════════════════════════════
output resourceGroupName string = rg.name
output vnetName string = foundation.outputs.vnetName
output aksClusterName string = aks.outputs.aksClusterName
output acrLoginServer string = acr.outputs.acrLoginServer
output postgresqlFqdn string = postgresql.outputs.fqdn
output serviceBusNamespace string = servicebus.outputs.namespaceName
output keyVaultName string = keyvault.outputs.keyVaultName
output appInsightsConnectionString string = observability.outputs.appInsightsConnectionString
