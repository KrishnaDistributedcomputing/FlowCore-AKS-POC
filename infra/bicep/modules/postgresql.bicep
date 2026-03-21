// ──────────────────────────────────────────────────
// Module D – PostgreSQL Flexible Server
// ──────────────────────────────────────────────────

param projectPrefix string
param environment string
param location string
param postgresqlSku string
param storageSizeGB int
@secure()
param adminLogin string
param dbSubnetId string
param privateDnsZoneId string
param tags object

var serverName = 'psql-${projectPrefix}-${environment}'

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2023-06-01-preview' = {
  name: serverName
  location: location
  tags: union(tags, { module: 'D-data-layer' })
  sku: {
    name: postgresqlSku
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: adminLogin
    administratorLoginPassword: 'P@ss${uniqueString(serverName)}!1'
    storage: {
      storageSizeGB: storageSizeGB
    }
    network: {
      delegatedSubnetResourceId: dbSubnetId
      privateDnsZoneArmResourceId: privateDnsZoneId
    }
    highAvailability: {
      mode: 'Disabled'
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
  }
}

// Create the four domain databases
resource customerDb 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-06-01-preview' = {
  parent: postgres
  name: 'flowcore_customer'
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

resource orderDb 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-06-01-preview' = {
  parent: postgres
  name: 'flowcore_caseorder'
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

resource reportingDb 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-06-01-preview' = {
  parent: postgres
  name: 'flowcore_reporting'
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

resource auditDb 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-06-01-preview' = {
  parent: postgres
  name: 'flowcore_audit'
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

output fqdn string = postgres.properties.fullyQualifiedDomainName
output serverName string = postgres.name
