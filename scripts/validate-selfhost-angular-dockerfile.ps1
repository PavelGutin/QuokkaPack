#!/usr/bin/env pwsh
# Validation script for QuokkaPack Angular self-host Dockerfile

Write-Host "Validating Dockerfile.selfhost.angular..." -ForegroundColor Green

# Check if Dockerfile exists
if (-not (Test-Path "Dockerfile.selfhost.angular")) {
    Write-Error "Dockerfile.selfhost.angular not found!"
    exit 1
}

# Basic syntax validation
$dockerfileContent = Get-Content "Dockerfile.selfhost.angular" -Raw

# Check for required FROM statements
if ($dockerfileContent -notmatch "FROM.*sdk.*AS api-build") {
    Write-Error "Missing API build stage FROM statement"
    exit 1
}

if ($dockerfileContent -notmatch "FROM.*node.*AS angular-build") {
    Write-Error "Missing Angular build stage FROM statement"
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

if ($dockerfileContent -notmatch "COPY.*angular-build.*dist") {
    Write-Error "Missing Angular build COPY statement"
    exit 1
}

# Check for Angular build process
if ($dockerfileContent -notmatch "npm ci") {
    Write-Error "Missing npm ci command for Angular dependencies"
    exit 1
}

if ($dockerfileContent -notmatch "npm run build") {
    Write-Error "Missing npm run build command for Angular"
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

# Check for nginx configuration
if ($dockerfileContent -notmatch "nginx\.conf") {
    Write-Error "Missing nginx configuration"
    exit 1
}

if ($dockerfileContent -notmatch "location /api/") {
    Write-Error "Missing API proxy configuration in nginx"
    exit 1
}

if ($dockerfileContent -notmatch "try_files.*index\.html") {
    Write-Error "Missing Angular SPA fallback configuration"
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

# Check for SQLite configuration
if ($dockerfileContent -notmatch "sqlite3") {
    Write-Error "Missing SQLite installation"
    exit 1
}

if ($dockerfileContent -notmatch "Data Source=/app/data/quokkapack\.db") {
    Write-Error "Missing SQLite connection string configuration"
    exit 1
}

# Check for supervisor configuration
if ($dockerfileContent -notmatch "supervisor") {
    Write-Error "Missing supervisor installation"
    exit 1
}

if ($dockerfileContent -notmatch "supervisord\.conf") {
    Write-Error "Missing supervisor configuration"
    exit 1
}

Write-Host "Dockerfile validation passed!" -ForegroundColor Green

# Validate related files exist
$requiredFiles = @(
    "src/QuokkaPack.API/QuokkaPack.API.csproj",
    "src/QuokkaPack.Angular/QuokkaPack.Angular.esproj",
    "src/QuokkaPack.Angular/package.json",
    "src/QuokkaPack.Angular/angular.json",
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