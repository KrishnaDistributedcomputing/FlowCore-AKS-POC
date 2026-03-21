#!/bin/bash
# ──────────────────────────────────────────────────
# FlowCore POC – Off-Hours Scale Down
# Business Outcome: Cost Optimization (Dimension 5)
# Scales AKS node pools to minimum for off-hours savings
# ──────────────────────────────────────────────────

set -euo pipefail

SUBSCRIPTION_ID="${SUBSCRIPTION_ID:-e62428e7-08dd-4bc2-82e2-2c51586d9105}"
RESOURCE_GROUP="${RESOURCE_GROUP:-rg-flowcore-poc}"
AKS_NAME="${AKS_NAME:-aks-flowcore-poc}"

echo "╔══════════════════════════════════════════════════╗"
echo "║  FlowCore POC – Off-Hours Scale Down             ║"
echo "╚══════════════════════════════════════════════════╝"

az account set --subscription "$SUBSCRIPTION_ID"

echo "Scaling apps node pool to 1..."
az aks nodepool update \
  --resource-group "$RESOURCE_GROUP" \
  --cluster-name "$AKS_NAME" \
  --name apps \
  --min-count 1 \
  --max-count 2 \
  --update-cluster-autoscaler

echo "Scaling workers node pool to 0..."
az aks nodepool update \
  --resource-group "$RESOURCE_GROUP" \
  --cluster-name "$AKS_NAME" \
  --name workers \
  --min-count 0 \
  --max-count 1 \
  --update-cluster-autoscaler

echo "✅ Scale-down complete. Estimated off-hours savings: ~60%"
