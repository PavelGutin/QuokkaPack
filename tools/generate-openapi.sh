#!/bin/bash
# Generate OpenAPI spec by temporarily running the API
# Bash equivalent of generate-openapi.ps1

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
API_PROJECT="$SCRIPT_DIR/../src/QuokkaPack.API"
ANGULAR_PROJECT="$SCRIPT_DIR/../src/QuokkaPack.Angular"
ARTIFACTS_DIR="$SCRIPT_DIR/../artifacts"

set -e

# Build API if not called from MSBuild
if [ -z "$MSBUILD_RUNNING" ]; then
    echo "Building API project..."
    dotnet build "$API_PROJECT/QuokkaPack.API.csproj" -c Release

    if [ $? -ne 0 ]; then
        echo "Build failed!"
        exit 1
    fi
else
    echo "Skipping build (already built by MSBuild)..."
fi

echo "Starting API to generate OpenAPI spec..."

# Start API in background
cd "$API_PROJECT"
ASPNETCORE_URLS="http://localhost:5000" ASPNETCORE_ENVIRONMENT="Development" \
    dotnet run --no-build -c Release > /dev/null 2>&1 &
API_PID=$!

# Ensure we kill the API on exit
trap "kill $API_PID 2>/dev/null || true; pkill -P $API_PID 2>/dev/null || true" EXIT

# Create artifacts directory
mkdir -p "$ARTIFACTS_DIR"

echo "Fetching OpenAPI spec from http://localhost:5000/openapi/v1.json..."

MAX_RETRIES=15
RETRY_COUNT=0
SUCCESS=false

while [ $RETRY_COUNT -lt $MAX_RETRIES ] && [ "$SUCCESS" = "false" ]; do
    sleep 2

    if curl -s -f -o "$ARTIFACTS_DIR/openapi.json" "http://localhost:5000/openapi/v1.json" 2>/dev/null; then
        SUCCESS=true
        echo "OpenAPI spec generated successfully!"
    else
        RETRY_COUNT=$((RETRY_COUNT + 1))
        echo "Waiting for API to start... (attempt $RETRY_COUNT/$MAX_RETRIES)"
    fi
done

if [ "$SUCCESS" = "false" ]; then
    echo "Failed to fetch OpenAPI spec after $MAX_RETRIES attempts"
    exit 1
fi

# Generate TypeScript client using NSwag
echo "Generating TypeScript client..."
cd "$ANGULAR_PROJECT"

dotnet nswag run codegen/nswag.json

if [ $? -eq 0 ]; then
    echo "Angular API client generation completed successfully!"
    # Apply fixes to the generated client
    echo "Applying fixes to generated client..."
    node codegen/fix-api-client.js
fi
