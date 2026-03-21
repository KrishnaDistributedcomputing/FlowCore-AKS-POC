// ──────────────────────────────────────────────────
// Module E – Azure Service Bus
// ──────────────────────────────────────────────────

param projectPrefix string
param environment string
param location string
param serviceBusSku string
param tags object

var sbName = 'sb-${projectPrefix}-${environment}'

resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: sbName
  location: location
  tags: union(tags, { module: 'E-messaging' })
  sku: {
    name: serviceBusSku
    tier: serviceBusSku
  }
}

// ── Topic: flowcore-events ──
resource eventsTopic 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  parent: serviceBus
  name: 'flowcore-events'
  properties: {
    maxSizeInMegabytes: 1024
    defaultMessageTimeToLive: 'P7D'
    enablePartitioning: false
  }
}

// ── Subscriptions ──
resource notificationSub 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  parent: eventsTopic
  name: 'notification-worker'
  properties: {
    maxDeliveryCount: 5
    deadLetteringOnMessageExpiration: true
    lockDuration: 'PT1M'
    defaultMessageTimeToLive: 'P3D'
  }
}

resource notificationFilter 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2022-10-01-preview' = {
  parent: notificationSub
  name: 'notification-filter'
  properties: {
    filterType: 'CorrelationFilter'
    correlationFilter: {
      label: 'NotificationRequested'
    }
  }
}

resource projectionSub 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  parent: eventsTopic
  name: 'projection-worker'
  properties: {
    maxDeliveryCount: 5
    deadLetteringOnMessageExpiration: true
    lockDuration: 'PT1M'
    defaultMessageTimeToLive: 'P3D'
  }
}

resource auditSub 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  parent: eventsTopic
  name: 'audit-worker'
  properties: {
    maxDeliveryCount: 10
    deadLetteringOnMessageExpiration: true
    lockDuration: 'PT1M'
    defaultMessageTimeToLive: 'P7D'
  }
}

resource auditFilter 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2022-10-01-preview' = {
  parent: auditSub
  name: 'audit-filter'
  properties: {
    filterType: 'CorrelationFilter'
    correlationFilter: {
      label: 'AuditRecorded'
    }
  }
}

output namespaceName string = serviceBus.name
output namespaceId string = serviceBus.id
output topicName string = eventsTopic.name
