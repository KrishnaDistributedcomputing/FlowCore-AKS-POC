# Module E - Messaging Layer

## Objective

Enable async service-to-service communication.

## Components

- Azure Service Bus namespace
- Topic(s)
- Subscription(s)
- Dead-letter handling
- Retry policy

## Core Events

- CustomerUpdated
- CaseCreated
- OrderPlaced
- NotificationRequested
- AuditRecorded

## Inputs

- Topic names
- Subscription names
- Retry counts
- DLQ policy

## Outputs

- Messaging backbone
- Event publishing path
- Event consumption path

## Assumptions

- Async messaging is required for background processing
- DLQ is mandatory
- Event replay is manual or scripted for this POC

## Acceptance Criteria

- Services can publish messages
- Worker can consume messages
- Failed messages move to DLQ
- Replay procedure documented

## Deliverables

- Messaging IaC module under infra/bicep
- Async API contract under this module folder
- Replay and DLQ operations guide under docs
