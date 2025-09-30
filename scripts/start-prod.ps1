#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Starts the QuokkaPack production environment using Docker Compose.

.DESCRIPTION
    This script starts the complete production environment including:
    - All frontend services with production optimizations
    - SQL Server database with production configuration
    - Health checks and monitoring
    - Resource constraints and security settings

.PARAMETER Build
    Forces rebuild of images before starting

.PARAMETER Detached
    Runs containers in detached mode (background) - default for production

.PARAMETER Services
    Comma-separated list of specific services to start
    Valid services: api, razor, blazor, angular, sqlserver

.PARAMETER Scale
    Scales specific services. Format: service=count,service=count
    Example: "api=2,razor=2"

.PARAMETER Logs
    Shows logs for running services

.EXAMPLE
    .\start-prod.ps1
    Starts all production services in background

.EXAMPLE
    .\start-prod.ps1 -Build
    Rebuilds and starts all production services

.EXAMPLE
    .\start-prod.ps1 -Scale "api=2,razor=2"
    Starts with 2 instances each of API and Razor services
#>

param(
    [switch]$Build,
    [switch]$Detached = $true,
    [string]$Services,
    [string]$Scale,
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
$ComposeFile = 'docker-compose.prod.yml'
$ProjectName = 'quokkapack-prod'

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
    
    # Check if .env.prod file exists
    if (-not (Test-Path '.env.prod')) {
        Write-Warning ".env.prod file not found. Using default environment variables."
        Write-Warning "For production deployment, ensure proper environment configuration!"
    }
    
    # Warn about production deployment
    Write-Warning "Starting PRODUCTION environment. Ensure:"
    Write-Warning "• Production environment variables are configured"
    Write-Warning "• SSL certificates are properly configured"
    Write-Warning "• Database backups are in place"
    Write-Warning "• Monitoring and logging are configured"
}

function Start-Services {
    param(
        [string[]]$ServiceList,
        [bool]$BuildImages,
        [bool]$RunDetached,
        [hashtable]$ScaleConfig
    )
    
    $composeArgs = @('docker-compose', '-f', $ComposeFile, '-p', $ProjectName)
    
    if ($BuildImages) {
        Write-Info "Building production images..."
        $buildArgs = $composeArgs + @('build')
        if ($ServiceList) {
            $buildArgs += $ServiceList
        }
        & $buildArgs
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to build Docker images"
        }
    }
    
    Write-Info "Starting production services..."
    $upArgs = $composeArgs + @('up')
    
    if ($RunDetached) {
        $upArgs += '-d'
    }
    
    # Add scaling configuration
    if ($ScaleConfig) {
        foreach ($service in $ScaleConfig.Keys) {
            $upArgs += "--scale"
            $upArgs += "$service=$($ScaleConfig[$service])"
        }
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
    Write-Info "Production Service Status:"
    & docker-compose -f $ComposeFile -p $ProjectName ps
    
    Write-Info ""
    Write-Info "Health Check Status:"
    $containers = & docker-compose -f $ComposeFile -p $ProjectName ps -q
    foreach ($container in $containers) {
        if ($container) {
            $health = & docker inspect --format='{{.State.Health.Status}}' $container 2>$null
            $name = & docker inspect --format='{{.Name}}' $container 2>$null
            if ($health) {
                $status = if ($health -eq 'healthy') { '✓' } elseif ($health -eq 'unhealthy') { '✗' } else { '⚠' }
                Write-Info "$status $($name.TrimStart('/')): $health"
            }
        }
    }
    
    Write-Info ""
    Write-Info "Production Service URLs:"
    Write-Info "• API (Swagger): https://localhost:5443/swagger"
    Write-Info "• Razor Pages: https://localhost:5444"
    Write-Info "• Blazor Server: https://localhost:5445"
    Write-Info "• Angular: https://localhost:5446"
    Write-Info ""
    Write-Info "Database:"
    Write-Info "• SQL Server: localhost:1434 (production credentials required)"
}

function Show-Logs {
    param([string[]]$ServiceList)
    
    Write-Info "Showing production logs..."
    $logArgs = @('docker-compose', '-f', $ComposeFile, '-p', $ProjectName, 'logs', '-f')
    
    if ($ServiceList) {
        $logArgs += $ServiceList
    }
    
    & $logArgs
}

function Parse-ScaleConfig {
    param([string]$ScaleString)
    
    $scaleConfig = @{}
    if ($ScaleString) {
        $pairs = $ScaleString -split ','
        foreach ($pair in $pairs) {
            $parts = $pair.Trim() -split '='
            if ($parts.Length -eq 2) {
                $service = $parts[0].Trim()
                $count = [int]$parts[1].Trim()
                $scaleConfig[$service] = $count
                Write-Info "Will scale $service to $count instances"
            }
        }
    }
    return $scaleConfig
}

# Main execution
try {
    Write-Info "Starting QuokkaPack Production Environment"
    Write-Info "========================================="
    
    # Validate prerequisites
    Test-Prerequisites
    
    # Parse services if specified
    $serviceList = $null
    if ($Services) {
        $serviceList = $Services -split ',' | ForEach-Object { $_.Trim() }
        Write-Info "Starting specific services: $($serviceList -join ', ')"
    }
    
    # Parse scaling configuration
    $scaleConfig = Parse-ScaleConfig -ScaleString $Scale
    
    # Handle logs-only mode
    if ($Logs) {
        Show-Logs -ServiceList $serviceList
        return
    }
    
    # Confirmation for production start
    if (-not $Services -and -not $env:QUOKKAPACK_AUTO_CONFIRM) {
        $confirmation = Read-Host "Are you sure you want to start the PRODUCTION environment? (y/N)"
        if ($confirmation -notmatch '^[Yy]') {
            Write-Info "Production start cancelled."
            return
        }
    }
    
    # Start services
    Start-Services -ServiceList $serviceList -BuildImages $Build -RunDetached $Detached -ScaleConfig $scaleConfig
    
    Write-Success "✓ Production environment started successfully!"
    
    # Wait a moment for health checks to initialize
    Start-Sleep -Seconds 10
    
    Show-ServiceStatus
    
    Write-Info ""
    Write-Info "Production Management Commands:"
    Write-Info "• View logs: .\start-prod.ps1 -Logs"
    Write-Info "• Stop services: .\stop-prod.ps1"
    Write-Info "• Monitor health: docker-compose -f $ComposeFile -p $ProjectName ps"
    Write-Info "• Scale services: .\start-prod.ps1 -Scale 'api=3,razor=2'"
}
catch {
    Write-Error "Failed to start production environment: $($_.Exception.Message)"
    Write-Info ""
    Write-Info "Troubleshooting tips:"
    Write-Info "• Ensure Docker Desktop is running"
    Write-Info "• Check if production ports (5443-5446, 1434) are available"
    Write-Info "• Verify production environment variables in .env.prod"
    Write-Info "• Run '.\stop-prod.ps1' to clean up any stuck containers"
    Write-Info "• Check logs with 'docker-compose -f $ComposeFile -p $ProjectName logs'"
    exit 1
}