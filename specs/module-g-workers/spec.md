# Module G - Worker Services

## Objective

Validate event-driven background processing.

## Services Included

### G1. Notification Worker

- Consumes NotificationRequested
- Simulates email and SMS dispatch
- Writes status log

### G2. Optional Projection Worker

- Consumes domain events
- Updates reporting model

## Inputs

- Service Bus subscription
- Retry policy
- Worker scaling settings

## Outputs

- Background processing capability
- Event-driven processing proof
- Retry and DLQ handling

## Assumptions

- Worker is independently scalable
- Worker failure should not fail the main API transaction
- Notification destination can be mocked

## Acceptance Criteria

- Worker consumes events
- Worker retries failed events
- DLQ captures poison messages
- Processing status is observable

## Deliverables

- Notification worker source under apps/notification-worker
- Worker deployment manifests with scale settings
- Runbook for retry and DLQ handling
