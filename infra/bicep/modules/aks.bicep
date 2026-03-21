// ──────────────────────────────────────────────────
// Module B – AKS Platform
// ──────────────────────────────────────────────────

param projectPrefix string
param environment string
param location string
param kubernetesVersion string
param aksSubnetId string
param systemNodeCount int
param appNodeCount int
param workerNodeCount int
param systemNodeVmSize string
param appNodeVmSize string
param workerNodeVmSize string
param logAnalyticsWorkspaceId string
param tags object

var aksName = 'aks-${projectPrefix}-${environment}'

resource aks 'Microsoft.ContainerService/managedClusters@2024-01-01' = {
  name: aksName
  location: location
  tags: union(tags, { module: 'B-aks-platform' })
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    dnsPrefix: '${projectPrefix}-${environment}'
    kubernetesVersion: kubernetesVersion
    networkProfile: {
      networkPlugin: 'azure'
      networkPolicy: 'azure'
      serviceCidr: '10.200.0.0/16'
      dnsServiceIP: '10.200.0.10'
    }
    agentPoolProfiles: [
      {
        name: 'system'
        mode: 'System'
        count: systemNodeCount
        vmSize: systemNodeVmSize
        osType: 'Linux'
        vnetSubnetID: aksSubnetId
        enableAutoScaling: true
        minCount: 1
        maxCount: systemNodeCount + 1
      }
      {
        name: 'apps'
        mode: 'User'
        count: appNodeCount
        vmSize: appNodeVmSize
        osType: 'Linux'
        vnetSubnetID: aksSubnetId
        enableAutoScaling: true
        minCount: 1
        maxCount: appNodeCount + 2
        nodeTaints: []
        nodeLabels: {
          workload: 'apps'
        }
      }
      {
        name: 'workers'
        mode: 'User'
        count: workerNodeCount
        vmSize: workerNodeVmSize
        osType: 'Linux'
        vnetSubnetID: aksSubnetId
        enableAutoScaling: true
        minCount: 0
        maxCount: workerNodeCount + 2
        nodeLabels: {
          workload: 'workers'
        }
      }
    ]
    addonProfiles: {
      omsagent: {
        enabled: true
        config: {
          logAnalyticsWorkspaceResourceID: logAnalyticsWorkspaceId
        }
      }
    }
  }
}

output aksClusterName string = aks.name
output aksClusterId string = aks.id
output kubeletIdentityObjectId string = aks.properties.identityProfile.kubeletidentity.objectId
