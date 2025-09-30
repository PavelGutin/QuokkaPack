#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Stops the QuokkaPack production environment.

.DESCRIPTION
    This script safely stops all production services with proper
    graceful shutdown procedures and optional cleanup.

.PARAMETER Services
    Comma-separated list of specific services to stop
    Valid services: api, razor, blazor, angular, sqlserver

.PARAMETER Remove
    Removes containers after stopping them

.PARAMETER Volumes
    Removes named volumes (WARNING: This will delete production database data)

.PARAMETER Images
    Removes built production images

.PARAMETER Force
    Forces immediate shutdown without graceful stop

.PARAMETER Backup
    Creates database backup before stopping (requires sqlcmd)

.EXAMPLE
    .\stop-prod.ps1
    Gracefully stops all production services

.EXAMPLE
    .\stop-prod.ps1 -Backup
    Creates database backup before stopping

.EXAMPLE
    .\stop-prod.ps1 -Services "api,razor" -Remove
    Stops and removes specific services

.EXAMPLE
    .\stop-prod.ps1 -Force
    Forces immediate shutdown of all services
#>

param(
    [string]$Services,
    [switch]$Remove,
    [switch]$Volumes,
    [switch]$Images,
    [switch]$Force,
    [switch]$Backup
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

function Backup-Database {
    Write-Info "Creating production database backup..."
    
    try {
        $backupScript = "scripts/backup-db.sh"
        if (Test-Path $backupScript) {
            & $backupScript
            Write-Success "✓ Database backup completed"
        } else {
            Write-Warning "Backup script not found. Skipping database backup."
        }
    }
    catch {
        Write-Warning "Database backup failed: $($_.Exception.Message)"
        Write-Warning "Continuing with shutdown..."
    }
}

function Stop-Services {
    param(
        [string[]]$ServiceList,
        [bool]$ForceStop
    )
    
    if ($ForceStop) {
        Write-Warning "Force stopping production services..."
        $stopArgs = @('docker-compose', '-f', $ComposeFile, '-p', $ProjectName, 'kill')
    } else {
        Write-Info "Gracefully stopping production services..."
        $stopArgs = @('docker-compose', '-f', $ComposeFile, '-p', $ProjectName, 'stop', '-t', '30')
    }
    
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
    
    # Wait for graceful shutdown
    if (-not $ForceStop) {
        Write-Info "Waiting for graceful shutdown..."
        Start-Sleep -Seconds 5
    }
}

function Remove-Containers {
    param([string[]]$ServiceList)
    
    Write-Info "Removing production containers..."
    
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
    Write-Error "DANGER: Removing volumes will DELETE ALL PRODUCTION DATA!"
    Write-Warning "This includes:"
    Write-Warning "• Production database"
    Write-Warning "• User data"
    Write-Warning "• Application logs"
    Write-Warning "• SSL certificates"
    
    $confirmation = Read-Host "Type 'DELETE PRODUCTION DATA' to confirm volume removal"
    if ($confirmation -ne 'DELETE PRODUCTION DATA') {
        Write-Info "Volume removal cancelled for safety."
        return
    }
    
    Write-Warning "Removing production volumes..."
    
    & docker-compose -f $ComposeFile -p $ProjectName down -v
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "✓ Production volumes removed"
    }
}

function Remove-Images {
    Write-Info "Removing production images..."
    
    $images = @(
        'quokkapack-api:latest',
        'quokkapack-razor:latest',
        'quokkapack-blazor:latest',
        'quokkapack-angular:latest'
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
    
    Write-Success "✓ Production images cleanup completed"
}

function Show-Status {
    Write-Info "Current production environment status:"
    
    try {
        & docker-compose -f $ComposeFile -p $ProjectName ps
    }
    catch {
        Write-Info "No running services found"
    }
    
    Write-Info ""
    Write-Info "Production images:"
    & docker images --filter "reference=quokkapack*:latest" --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}\t{{.CreatedSince}}"
}

# Main execution
try {
    Write-Info "Stopping QuokkaPack Production Environment"
    Write-Info "=========================================="
    
    # Production safety warning
    Write-Warning "You are stopping the PRODUCTION environment!"
    Write-Warning "This may affect live users and services."
    
    if (-not $Force -and -not $env:QUOKKAPACK_AUTO_CONFIRM) {
        $confirmation = Read-Host "Are you sure you want to stop production services? (y/N)"
        if ($confirmation -notmatch '^[Yy]') {
            Write-Info "Production stop cancelled."
            return
        }
    }
    
    # Parse services if specified
    $serviceList = $null
    if ($Services) {
        $serviceList = $Services -split ',' | ForEach-Object { $_.Trim() }
    }
    
    # Create database backup if requested
    if ($Backup) {
        Backup-Database
    }
    
    # Stop services
    Stop-Services -ServiceList $serviceList -ForceStop $Force
    
    # Remove containers if requested
    if ($Remove) {
        Remove-Containers -ServiceList $serviceList
    }
    
    # Remove volumes if requested (with extra safety)
    if ($Volumes) {
        Remove-Volumes
    }
    
    # Remove images if requested
    if ($Images) {
        Remove-Images
    }
    
    Write-Success "✓ Production environment stopped successfully!"
    
    # Show final status
    Write-Info ""
    Show-Status
    
    Write-Info ""
    Write-Info "Production Management:"
    Write-Info "• To restart: .\start-prod.ps1"
    Write-Info "• Check logs: docker-compose -f $ComposeFile -p $ProjectName logs"
    Write-Info "• Monitor: docker stats"
}
catch {
    Write-Error "Failed to stop production environment: $($_.Exception.Message)"
    Write-Info ""
    Write-Info "Emergency commands:"
    Write-Info "• Force stop: docker-compose -f $ComposeFile -p $ProjectName kill"
    Write-Info "• Remove all: docker-compose -f $ComposeFile -p $ProjectName down -v"
    exit 1
}