using '../main.bicep'

param environment = 'poc'
param projectPrefix = 'flowcore'
param location = 'canadacentral'
param drLocation = 'canadaeast'
param vnetAddressPrefix = '10.100.0.0/16'
param aksSubnetPrefix = '10.100.0.0/22'
param servicesSubnetPrefix = '10.100.4.0/24'
param dbSubnetPrefix = '10.100.5.0/24'
param kubernetesVersion = '1.33'
param systemNodeCount = 2
param appNodeCount = 2
param workerNodeCount = 1
param systemNodeVmSize = 'Standard_D2s_v5'
param appNodeVmSize = 'Standard_D4s_v5'
param workerNodeVmSize = 'Standard_D2s_v5'
param acrSku = 'Basic'
param redisSku = 'Basic'
param redisCapacity = 1
param postgresqlSku = 'Standard_B1ms'
param postgresqlStorageSizeGB = 32
param postgresAdminLogin = 'flowcoreadmin'
param serviceBusSku = 'Standard'
