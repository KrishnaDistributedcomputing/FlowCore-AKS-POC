// ──────────────────────────────────────────────────
// Module I – Key Vault
// ──────────────────────────────────────────────────

param projectPrefix string
param environment string
param location string
param tags object

var kvName = 'kv-${projectPrefix}-${environment}'

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: kvName
  location: location
  tags: union(tags, { module: 'I-security' })
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    enabledForTemplateDeployment: true
  }
}

output keyVaultName string = keyVault.name
output keyVaultId string = keyVault.id
output keyVaultUri string = keyVault.properties.vaultUri
