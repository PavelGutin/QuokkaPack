#!/usr/bin/env pwsh
# Validation script for QuokkaPack self-host Dockerfile

Write-Host "Validating Dockerfile.selfhost.razor..." -ForegroundColor Green

# Check if Dockerfile exists
if (-not (Test-Path "Dockerfile.selfhost.razor")) {
    Write-Error "Dockerfile.selfhost.razor not found!"
    exit 1
}

# Basic syntax validation
$dockerfileContent = Get-Content "Dockerfile.selfhost.razor" -Raw

# Check for required FROM statements
if ($dockerfileContent -notmatch "FROM.*sdk.*AS build") {
    Write-Error "Missing build stage FROM statement"
    exit 1
}

if ($dockerfileContent -notmatch "FROM.*aspnet.*AS final") {
    Write-Error "Missing final stage FROM statement"
    exit 1
}

# Check for required COPY statements
if ($dockerfileContent -notmatch "COPY.*publish.*api") {
    Write-Error "Missing API publish COPY statement"
    exit 1
}

if ($dockerfileContent -notmatch "COPY.*publish.*razor") {
    Write-Error "Missing Razor publish COPY statement"
    exit 1
}

# Check for required RUN statements
if ($dockerfileContent -notmatch "nginx") {
    Write-Error "Missing nginx installation"
    exit 1
}

if ($dockerfileContent -notmatch "RUN.*startup\.sh") {
    Write-Error "Missing startup script creation"
    exit 1
}

# Check for required EXPOSE statement
if ($dockerfileContent -notmatch "EXPOSE 80") {
    Write-Error "Missing EXPOSE 80 statement"
    exit 1
}

# Check for required VOLUME statement
if ($dockerfileContent -notmatch "VOLUME.*app/data") {
    Write-Error "Missing VOLUME statement for data persistence"
    exit 1
}

# Check for required HEALTHCHECK
if ($dockerfileContent -notmatch "HEALTHCHECK") {
    Write-Error "Missing HEALTHCHECK statement"
    exit 1
}

# Check for required ENTRYPOINT
if ($dockerfileContent -notmatch "ENTRYPOINT.*startup\.sh") {
    Write-Error "Missing ENTRYPOINT statement"
    exit 1
}

Write-Host "Dockerfile validation passed!" -ForegroundColor Green

# Validate related files exist
$requiredFiles = @(
    "src/QuokkaPack.API/QuokkaPack.API.csproj",
    "src/QuokkaPack.Razor/QuokkaPack.Razor.csproj",
    "src/QuokkaPack.Data/QuokkaPack.Data.csproj",
    "src/QuokkaPack.Shared/QuokkaPack.Shared.csproj",
    "src/QuokkaPack.ServerCommon/QuokkaPack.ServerCommon.csproj"
)

foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        Write-Warning "Required file not found: $file"
    } else {
        Write-Host "✓ Found: $file" -ForegroundColor Cyan
    }
}

Write-Host "Validation completed successfully!" -ForegroundColor Green