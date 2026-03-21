#!/bin/bash
# ──────────────────────────────────────────────────
# FlowCore POC – Environment Teardown Script
# Business Outcome: Cost Optimization (Dimension 5)
# Destroys the entire POC resource group and all resources
# ──────────────────────────────────────────────────

set -euo pipefail

SUBSCRIPTION_ID="${SUBSCRIPTION_ID:-e62428e7-08dd-4bc2-82e2-2c51586d9105}"
RESOURCE_GROUP="${RESOURCE_GROUP:-rg-flowcore-poc}"

echo "╔══════════════════════════════════════════════════╗"
echo "║  FlowCore POC – Environment Teardown             ║"
echo "║  Subscription: $SUBSCRIPTION_ID  ║"
echo "║  Resource Group: $RESOURCE_GROUP                 ║"
echo "╚══════════════════════════════════════════════════╝"
echo ""

# Confirm
read -p "⚠️  This will DELETE all resources in $RESOURCE_GROUP. Continue? (yes/no): " CONFIRM
if [ "$CONFIRM" != "yes" ]; then
    echo "Aborted."
    exit 0
fi

echo "Setting subscription..."
az account set --subscription "$SUBSCRIPTION_ID"

echo "Deleting resource group $RESOURCE_GROUP..."
az group delete --name "$RESOURCE_GROUP" --yes --no-wait

echo "✅ Teardown initiated. Resource group deletion is in progress."
echo "   Monitor: az group show -n $RESOURCE_GROUP --query provisioningState"
