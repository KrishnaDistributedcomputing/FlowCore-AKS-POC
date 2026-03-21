// ──────────────────────────────────────────────────
// Module C – Azure Cache for Redis
// ──────────────────────────────────────────────────

param projectPrefix string
param environment string
param location string
param redisSku string
param redisCapacity int
param tags object

var redisName = 'redis-${projectPrefix}-${environment}'

resource redis 'Microsoft.Cache/redis@2023-08-01' = {
  name: redisName
  location: location
  tags: union(tags, { module: 'C-shared-services' })
  properties: {
    sku: {
      name: redisSku
      family: redisSku == 'Premium' ? 'P' : 'C'
      capacity: redisCapacity
    }
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
  }
}

output redisName string = redis.name
output redisHostName string = redis.properties.hostName
output redisPort int = redis.properties.sslPort
