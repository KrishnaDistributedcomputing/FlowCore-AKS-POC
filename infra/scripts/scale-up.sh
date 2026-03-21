#!/bin/bash
# ──────────────────────────────────────────────────
# FlowCore POC – Business Hours Scale Up
# Business Outcome: Cost Optimization (Dimension 5)
# Restores AKS node pools to normal capacity
# ──────────────────────────────────────────────────

set -euo pipefail

SUBSCRIPTION_ID="${SUBSCRIPTION_ID:-e62428e7-08dd-4bc2-82e2-2c51586d9105}"
RESOURCE_GROUP="${RESOURCE_GROUP:-rg-flowcore-poc}"
AKS_NAME="${AKS_NAME:-aks-flowcore-poc}"

echo "╔══════════════════════════════════════════════════╗"
echo "║  FlowCore POC – Business Hours Scale Up          ║"
echo "╚══════════════════════════════════════════════════╝"

az account set --subscription "$SUBSCRIPTION_ID"

echo "Scaling apps node pool to normal capacity..."
az aks nodepool update \
  --resource-group "$RESOURCE_GROUP" \
  --cluster-name "$AKS_NAME" \
  --name apps \
  --min-count 1 \
  --max-count 4 \
  --update-cluster-autoscaler

echo "Scaling workers node pool to normal capacity..."
az aks nodepool update \
  --resource-group "$RESOURCE_GROUP" \
  --cluster-name "$AKS_NAME" \
  --name workers \
  --min-count 0 \
  --max-count 3 \
  --update-cluster-autoscaler

echo "✅ Scale-up complete. All pools at business-hours capacity."
