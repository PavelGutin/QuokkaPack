#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Starts the QuokkaPack development environment using Docker Compose.

.DESCRIPTION
    This script starts the complete development environment including:
    - All frontend services (API, Razor, Blazor, Angular)
    - SQL Server database with development configuration
    - Volume mounts for hot reload
    - Debug port exposure

.PARAMETER Build
    Forces rebuild of images before starting

.PARAMETER Detached
    Runs containers in detached mode (background)

.PARAMETER Services
    Comma-separated list of specific services to start
    Valid services: api, razor, blazor, angular, sqlserver

.PARAMETER Logs
    Shows logs for running services (requires -Detached)

.EXAMPLE
    .\start-dev.ps1
    Starts all development services in foreground

.EXAMPLE
    .\start-dev.ps1 -Build -Detached
    Rebuilds and starts all services in background

.EXAMPLE
    .\start-dev.ps1 -Services "api,razor,sqlserver"
    Starts only API, Razor, and SQL Server services
#>

param(
    [switch]$Build,
    [switch]$Detached,
    [string]$Services,
    [switch]$Logs
)

# Set error action preference
$ErrorActionPreference = 'Stop'

# Color functions for output
function Write-Success { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-Info { param($Message) Write-Host $Message -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host $Message -ForegroundColor Red }

# Configuration
$ComposeFile = 'docker-compose.dev.yml'
$ProjectName = 'quokkapack-dev'

function Test-Prerequisites {
    # Check if Docker is running
    try {
        docker info | Out-Null
    }
    catch {
        throw "Docker is not running. Please start Docker Desktop and try again."
    }
    
    # Check if docker-compose file exists
    if (-not (Test-Path $ComposeFile)) {
        throw "Docker Compose file '$ComposeFile' not found. Please ensure you're running this script from the project root."
    }
    
    # Check if .env.dev file exists
    if (-not (Test-Path '.env.dev')) {
        Write-Warning ".env.dev file not found. Using default environment variables."
    }
}

function Start-Services {
    param(
        [string[]]$ServiceList,
        [bool]$BuildImages,
        [bool]$RunDetached
    )
    
    $composeArgs = @('docker-compose', '-f', $ComposeFile, '-p', $ProjectName)
    
    if ($BuildImages) {
        Write-Info "Building images before starting services..."
        $buildArgs = $composeArgs + @('build')
        if ($ServiceList) {
            $buildArgs += $ServiceList
        }
        & $buildArgs
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to build Docker images"
        }
    }
    
    Write-Info "Starting development services..."
    $upArgs = $composeArgs + @('up')
    
    if ($RunDetached) {
        $upArgs += '-d'
    }
    
    if ($ServiceList) {
        $upArgs += $ServiceList
    }
    
    & $upArgs
    
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to start services"
    }
}

function Show-ServiceStatus {
    Write-Info "Service Status:"
    & docker-compose -f $ComposeFile -p $ProjectName ps
    
    Write-Info ""
    Write-Info "Service URLs (when running):"
    Write-Info "• API (Swagger): http://localhost:5000/swagger"
    Write-Info "• Razor Pages: http://localhost:5001"
    Write-Info "• Blazor Server: http://localhost:5002"
    Write-Info "• Angular: http://localhost:4200"
    Write-Info "• SQL Server: localhost:1433 (sa/YourStrong@Passw0rd)"
}

function Show-Logs {
    Write-Info "Showing logs for running services..."
    & docker-compose -f $ComposeFile -p $ProjectName logs -f
}

# Main execution
try {
    Write-Info "Starting QuokkaPack Development Environment"
    Write-Info "=========================================="
    
    # Validate prerequisites
    Test-Prerequisites
    
    # Parse services if specified
    $serviceList = $null
    if ($Services) {
        $serviceList = $Services -split ',' | ForEach-Object { $_.Trim() }
        Write-Info "Starting specific services: $($serviceList -join ', ')"
    }
    
    # Handle logs-only mode
    if ($Logs) {
        if (-not $Detached) {
            Write-Warning "Logs option is typically used with detached mode. Showing logs for currently running services..."
        }
        Show-Logs
        return
    }
    
    # Start services
    Start-Services -ServiceList $serviceList -BuildImages $Build -RunDetached $Detached
    
    if ($Detached) {
        Write-Success "✓ Development environment started successfully in background!"
        Show-ServiceStatus
        Write-Info ""
        Write-Info "To view logs: .\start-dev.ps1 -Logs"
        Write-Info "To stop: .\stop-dev.ps1"
    } else {
        Write-Success "✓ Development environment started successfully!"
        Write-Info "Press Ctrl+C to stop all services"
    }
}
catch {
    Write-Error "Failed to start development environment: $($_.Exception.Message)"
    Write-Info ""
    Write-Info "Troubleshooting tips:"
    Write-Info "• Ensure Docker Desktop is running"
    Write-Info "• Check if ports 5000-5002, 4200, 1433 are available"
    Write-Info "• Run '.\stop-dev.ps1' to clean up any stuck containers"
    Write-Info "• Check logs with 'docker-compose -f $ComposeFile logs'"
    exit 1
}