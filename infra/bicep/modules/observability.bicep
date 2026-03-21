// ──────────────────────────────────────────────────
// Module H – Observability (Log Analytics + App Insights)
// ──────────────────────────────────────────────────

param projectPrefix string
param environment string
param location string
param tags object

var lawName = 'law-${projectPrefix}-${environment}'
var aiName = 'ai-${projectPrefix}-${environment}'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: lawName
  location: location
  tags: union(tags, { module: 'H-observability' })
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: aiName
  location: location
  kind: 'web'
  tags: union(tags, { module: 'H-observability' })
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

output logAnalyticsWorkspaceId string = logAnalytics.id
output logAnalyticsWorkspaceName string = logAnalytics.name
output appInsightsConnectionString string = appInsights.properties.ConnectionString
output appInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey
