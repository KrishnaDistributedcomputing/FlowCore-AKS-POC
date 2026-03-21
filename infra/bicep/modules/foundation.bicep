// ──────────────────────────────────────────────────
// Module A – Foundation: VNet, Subnets, Private DNS
// ──────────────────────────────────────────────────

param projectPrefix string
param environment string
param location string
param vnetAddressPrefix string
param aksSubnetPrefix string
param servicesSubnetPrefix string
param dbSubnetPrefix string
param tags object

var vnetName = 'vnet-${projectPrefix}-${environment}'

resource vnet 'Microsoft.Network/virtualNetworks@2023-09-01' = {
  name: vnetName
  location: location
  tags: union(tags, { module: 'A-foundation' })
  properties: {
    addressSpace: {
      addressPrefixes: [vnetAddressPrefix]
    }
    subnets: [
      {
        name: 'snet-aks'
        properties: {
          addressPrefix: aksSubnetPrefix
        }
      }
      {
        name: 'snet-services'
        properties: {
          addressPrefix: servicesSubnetPrefix
        }
      }
      {
        name: 'snet-db'
        properties: {
          addressPrefix: dbSubnetPrefix
          delegations: [
            {
              name: 'dlg-postgres'
              properties: {
                serviceName: 'Microsoft.DBforPostgreSQL/flexibleServers'
              }
            }
          ]
        }
      }
    ]
  }
}

// Private DNS zone for PostgreSQL
resource postgresDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: '${projectPrefix}-${environment}.private.postgres.database.azure.com'
  location: 'global'
  tags: union(tags, { module: 'A-foundation' })
}

resource postgresDnsVnetLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: postgresDnsZone
  name: 'link-${vnetName}'
  location: 'global'
  properties: {
    virtualNetwork: {
      id: vnet.id
    }
    registrationEnabled: false
  }
}

output vnetName string = vnet.name
output vnetId string = vnet.id
output aksSubnetId string = vnet.properties.subnets[0].id
output servicesSubnetId string = vnet.properties.subnets[1].id
output dbSubnetId string = vnet.properties.subnets[2].id
output postgresPrivateDnsZoneId string = postgresDnsZone.id
