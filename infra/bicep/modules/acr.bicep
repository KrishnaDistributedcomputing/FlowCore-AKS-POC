// ──────────────────────────────────────────────────
// Module C – Azure Container Registry
// ──────────────────────────────────────────────────

param projectPrefix string
param environment string
param location string
param acrSku string
param tags object

var acrName = replace('acr${projectPrefix}${environment}', '-', '')

resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: acrName
  location: location
  tags: union(tags, { module: 'C-shared-services' })
  sku: {
    name: acrSku
  }
  properties: {
    adminUserEnabled: false
  }
}

output acrName string = acr.name
output acrLoginServer string = acr.properties.loginServer
output acrId string = acr.id
