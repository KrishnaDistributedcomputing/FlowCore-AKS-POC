# Module K - Resiliency and Recovery Validation

## Objective

Test failure scenarios and measure recovery behavior.

## Scenarios

### K1. Pod failure

- Kill running pod
- Verify reschedule and recovery

### K2. Node failure

- Drain or isolate node
- Verify service continuity

### K3. Worker failure

- Inject malformed event
- Verify retry and DLQ

### K4. Database failover event

- Validate reconnect and recovery behavior

### K5. Secret rotation

- Rotate secret
- Verify application recovery process

### K6. Service rollback

- Deploy broken version
- Roll back to previous stable version

## Outputs

- Recovery measurements
- Operational runbook notes
- Known failure modes

## Assumptions

- Full regional DR is not part of first POC
- Focus is in-cluster and in-region resilience
- Measured RTO is more important than theoretical RTO

## Acceptance Criteria

- Each failure scenario documented
- Recovery time captured
- Manual steps clearly identified
- Gaps and recommendations logged

## Deliverables

- Chaos and failure-injection test scripts
- Recovery runbook with measured RTO per scenario
- Final resiliency gap report
