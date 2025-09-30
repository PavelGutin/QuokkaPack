#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Stops the QuokkaPack development environment.

.DESCRIPTION
    This script stops all development services and optionally cleans up
    containers, networks, and volumes.

.PARAMETER Services
    Comma-separated list of specific services to stop
    Valid services: api, razor, blazor, angular, sqlserver

.PARAMETER Remove
    Removes containers after stopping them

.PARAMETER Volumes
    Removes named volumes (WARNING: This will delete database data)

.PARAMETER Images
    Removes built images

.PARAMETER All
    Performs complete cleanup (containers, networks, volumes, images)

.EXAMPLE
    .\stop-dev.ps1
    Stops all development services

.EXAMPLE
    .\stop-dev.ps1 -Remove
    Stops and removes all containers

.EXAMPLE
    .\stop-dev.ps1 -Services "api,razor"
    Stops only API and Razor services

.EXAMPLE
    .\stop-dev.ps1 -All
    Complete cleanup of development environment
#>

param(
    [string]$Services,
    [switch]$Remove,
    [switch]$Volumes,
    [switch]$Images,
    [switch]$All
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

function Stop-Services {
    param([string[]]$ServiceList)
    
    Write-Info "Stopping development services..."
    
    $stopArgs = @('docker-compose', '-f', $ComposeFile, '-p', $ProjectName, 'stop')
    
    if ($ServiceList) {
        $stopArgs += $ServiceList
        Write-Info "Stopping specific services: $($ServiceList -join ', ')"
    }
    
    & $stopArgs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "✓ Services stopped successfully"
    } else {
        Write-Warning "Some services may have already been stopped"
    }
}

function Remove-Containers {
    param([string[]]$ServiceList)
    
    Write-Info "Removing containers..."
    
    $downArgs = @('docker-compose', '-f', $ComposeFile, '-p', $ProjectName, 'down')
    
    if ($ServiceList) {
        # For specific services, we need to stop and remove individually
        foreach ($service in $ServiceList) {
            $containerName = "${ProjectName}_${service}_1"
            try {
                & docker rm $containerName -f 2>$null
                Write-Info "Removed container: $containerName"
            }
            catch {
                Write-Warning "Container $containerName may not exist"
            }
        }
    } else {
        & $downArgs
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "✓ Containers removed successfully"
        }
    }
}

function Remove-Volumes {
    Write-Warning "Removing volumes will DELETE ALL DATABASE DATA!"
    
    if (-not $All) {
        $confirmation = Read-Host "Are you sure you want to remove volumes? This will delete all data! (y/N)"
        if ($confirmation -notmatch '^[Yy]') {
            Write-Info "Volume removal cancelled."
            return
        }
    }
    
    Write-Info "Removing volumes..."
    
    & docker-compose -f $ComposeFile -p $ProjectName down -v
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "✓ Volumes removed successfully"
    }
}

function Remove-Images {
    Write-Info "Removing development images..."
    
    $images = @(
        'quokkapack-api:dev',
        'quokkapack-razor:dev',
        'quokkapack-blazor:dev',
        'quokkapack-angular:dev'
    )
    
    foreach ($image in $images) {
        try {
            & docker rmi $image 2>$null
            Write-Info "Removed image: $image"
        }
        catch {
            Write-Warning "Image $image may not exist"
        }
    }
    
    Write-Success "✓ Images cleanup completed"
}

function Show-Status {
    Write-Info "Current development environment status:"
    
    try {
        & docker-compose -f $ComposeFile -p $ProjectName ps
    }
    catch {
        Write-Info "No running services found"
    }
    
    Write-Info ""
    Write-Info "Development images:"
    & docker images --filter "reference=quokkapack*:dev" --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}\t{{.CreatedSince}}"
}

# Main execution
try {
    Write-Info "Stopping QuokkaPack Development Environment"
    Write-Info "==========================================="
    
    # Parse services if specified
    $serviceList = $null
    if ($Services) {
        $serviceList = $Services -split ',' | ForEach-Object { $_.Trim() }
    }
    
    # Handle complete cleanup
    if ($All) {
        Write-Warning "Performing COMPLETE cleanup of development environment!"
        Write-Warning "This will remove containers, volumes, networks, and images."
        
        $confirmation = Read-Host "Are you sure? This will delete all development data! (y/N)"
        if ($confirmation -notmatch '^[Yy]') {
            Write-Info "Complete cleanup cancelled."
            return
        }
        
        $Remove = $true
        $Volumes = $true
        $Images = $true
    }
    
    # Stop services
    Stop-Services -ServiceList $serviceList
    
    # Remove containers if requested
    if ($Remove -or $All) {
        Remove-Containers -ServiceList $serviceList
    }
    
    # Remove volumes if requested
    if ($Volumes -or $All) {
        Remove-Volumes
    }
    
    # Remove images if requested
    if ($Images -or $All) {
        Remove-Images
    }
    
    Write-Success "✓ Development environment cleanup completed!"
    
    # Show final status
    if (-not $All) {
        Write-Info ""
        Show-Status
    }
    
    Write-Info ""
    Write-Info "To start development environment again: .\start-dev.ps1"
}
catch {
    Write-Error "Failed to stop development environment: $($_.Exception.Message)"
    exit 1
}