#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Comprehensive Docker cleanup and maintenance script for QuokkaPack.

.DESCRIPTION
    This script provides various cleanup operations for Docker containers,
    images, volumes, and networks related to QuokkaPack development and production.

.PARAMETER Type
    Type of cleanup to perform. Valid values:
    'containers', 'images', 'volumes', 'networks', 'all', 'system'
    Default: 'containers'

.PARAMETER Environment
    Target environment. Valid values: 'dev', 'prod', 'both', 'all'
    Default: 'both'

.PARAMETER Force
    Skip confirmation prompts

.PARAMETER DryRun
    Show what would be cleaned up without actually doing it

.PARAMETER Prune
    Perform Docker system prune operations

.EXAMPLE
    .\cleanup-containers.ps1
    Clean up stopped containers for both environments

.EXAMPLE
    .\cleanup-containers.ps1 -Type images -Environment dev
    Clean up development images only

.EXAMPLE
    .\cleanup-containers.ps1 -Type all -Force
    Complete cleanup without confirmation

.EXAMPLE
    .\cleanup-containers.ps1 -DryRun
    Show what would be cleaned up
#>

param(
    [ValidateSet('containers', 'images', 'volumes', 'networks', 'all', 'system')]
    [string]$Type = 'containers',
    
    [ValidateSet('dev', 'prod', 'both', 'all')]
    [string]$Environment = 'both',
    
    [switch]$Force,
    [switch]$DryRun,
    [switch]$Prune
)

# Set error action preference
$ErrorActionPreference = 'Stop'

# Color functions for output
function Write-Success { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-Info { param($Message) Write-Host $Message -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host $Message -ForegroundColor Red }

# Configuration
$QuokkaPackProjects = @('quokkapack-dev', 'quokkapack-prod')
$QuokkaPackImages = @(
    'quokkapack-api',
    'quokkapack-razor',
    'quokkapack-blazor',
    'quokkapack-angular',
    'quokkapack-selfhost-razor',
    'quokkapack-selfhost-blazor',
    'quokkapack-selfhost-angular'
)

function Get-TargetProjects {
    param([string]$Env)
    
    switch ($Env) {
        'dev' { return @('quokkapack-dev') }
        'prod' { return @('quokkapack-prod') }
        'both' { return @('quokkapack-dev', 'quokkapack-prod') }
        'all' { return $QuokkaPackProjects }
        default { return $QuokkaPackProjects }
    }
}

function Get-QuokkaPackContainers {
    param([string[]]$Projects)
    
    $containers = @()
    foreach ($project in $Projects) {
        try {
            $projectContainers = & docker ps -a --filter "label=com.docker.compose.project=$project" --format "{{.ID}}" 2>$null
            if ($projectContainers) {
                $containers += $projectContainers
            }
        }
        catch {
            Write-Warning "Could not get containers for project: $project"
        }
    }
    return $containers
}

function Get-QuokkaPackImages {
    param([string]$Env)
    
    $imageFilters = @()
    foreach ($image in $QuokkaPackImages) {
        if ($Env -eq 'dev') {
            $imageFilters += "${image}:dev"
        } elseif ($Env -eq 'prod') {
            $imageFilters += "${image}:latest"
        } else {
            $imageFilters += "${image}:dev"
            $imageFilters += "${image}:latest"
        }
    }
    
    $images = @()
    foreach ($filter in $imageFilters) {
        try {
            $foundImages = & docker images --filter "reference=$filter" --format "{{.ID}}" 2>$null
            if ($foundImages) {
                $images += $foundImages
            }
        }
        catch {
            # Image doesn't exist, continue
        }
    }
    return $images | Sort-Object -Unique
}

function Get-QuokkaPackVolumes {
    param([string[]]$Projects)
    
    $volumes = @()
    foreach ($project in $Projects) {
        try {
            $projectVolumes = & docker volume ls --filter "label=com.docker.compose.project=$project" --format "{{.Name}}" 2>$null
            if ($projectVolumes) {
                $volumes += $projectVolumes
            }
        }
        catch {
            Write-Warning "Could not get volumes for project: $project"
        }
    }
    return $volumes
}

function Get-QuokkaPackNetworks {
    param([string[]]$Projects)
    
    $networks = @()
    foreach ($project in $Projects) {
        try {
            $projectNetworks = & docker network ls --filter "label=com.docker.compose.project=$project" --format "{{.ID}}" 2>$null
            if ($projectNetworks) {
                $networks += $projectNetworks
            }
        }
        catch {
            Write-Warning "Could not get networks for project: $project"
        }
    }
    return $networks
}

function Cleanup-Containers {
    param([string[]]$Projects, [bool]$IsDryRun)
    
    Write-Info "Cleaning up QuokkaPack containers..."
    
    $containers = Get-QuokkaPackContainers -Projects $Projects
    
    if ($containers.Count -eq 0) {
        Write-Info "No QuokkaPack containers found to clean up."
        return
    }
    
    Write-Info "Found $($containers.Count) containers to clean up:"
    foreach ($container in $containers) {
        $info = & docker inspect --format='{{.Name}} ({{.State.Status}})' $container 2>$null
        Write-Info "  • $info"
    }
    
    if ($IsDryRun) {
        Write-Warning "[DRY RUN] Would remove $($containers.Count) containers"
        return
    }
    
    if (-not $Force) {
        $confirmation = Read-Host "Remove $($containers.Count) containers? (y/N)"
        if ($confirmation -notmatch '^[Yy]') {
            Write-Info "Container cleanup cancelled."
            return
        }
    }
    
    foreach ($container in $containers) {
        try {
            & docker rm $container -f 2>$null
            Write-Success "✓ Removed container: $container"
        }
        catch {
            Write-Warning "Failed to remove container: $container"
        }
    }
}

function Cleanup-Images {
    param([string]$Env, [bool]$IsDryRun)
    
    Write-Info "Cleaning up QuokkaPack images..."
    
    $images = Get-QuokkaPackImages -Env $Env
    
    if ($images.Count -eq 0) {
        Write-Info "No QuokkaPack images found to clean up."
        return
    }
    
    Write-Info "Found $($images.Count) images to clean up:"
    foreach ($image in $images) {
        $info = & docker inspect --format='{{.RepoTags}} {{.Size}}' $image 2>$null
        Write-Info "  • $info"
    }
    
    if ($IsDryRun) {
        Write-Warning "[DRY RUN] Would remove $($images.Count) images"
        return
    }
    
    if (-not $Force) {
        $confirmation = Read-Host "Remove $($images.Count) images? (y/N)"
        if ($confirmation -notmatch '^[Yy]') {
            Write-Info "Image cleanup cancelled."
            return
        }
    }
    
    foreach ($image in $images) {
        try {
            & docker rmi $image -f 2>$null
            Write-Success "✓ Removed image: $image"
        }
        catch {
            Write-Warning "Failed to remove image: $image"
        }
    }
}

function Cleanup-Volumes {
    param([string[]]$Projects, [bool]$IsDryRun)
    
    Write-Warning "Volume cleanup will DELETE ALL DATA in QuokkaPack volumes!"
    
    $volumes = Get-QuokkaPackVolumes -Projects $Projects
    
    if ($volumes.Count -eq 0) {
        Write-Info "No QuokkaPack volumes found to clean up."
        return
    }
    
    Write-Info "Found $($volumes.Count) volumes to clean up:"
    foreach ($volume in $volumes) {
        Write-Info "  • $volume"
    }
    
    if ($IsDryRun) {
        Write-Warning "[DRY RUN] Would remove $($volumes.Count) volumes"
        return
    }
    
    if (-not $Force) {
        Write-Error "This will DELETE ALL DATA in these volumes!"
        $confirmation = Read-Host "Type 'DELETE DATA' to confirm volume removal"
        if ($confirmation -ne 'DELETE DATA') {
            Write-Info "Volume cleanup cancelled for safety."
            return
        }
    }
    
    foreach ($volume in $volumes) {
        try {
            & docker volume rm $volume 2>$null
            Write-Success "✓ Removed volume: $volume"
        }
        catch {
            Write-Warning "Failed to remove volume: $volume"
        }
    }
}

function Cleanup-Networks {
    param([string[]]$Projects, [bool]$IsDryRun)
    
    Write-Info "Cleaning up QuokkaPack networks..."
    
    $networks = Get-QuokkaPackNetworks -Projects $Projects
    
    if ($networks.Count -eq 0) {
        Write-Info "No QuokkaPack networks found to clean up."
        return
    }
    
    Write-Info "Found $($networks.Count) networks to clean up:"
    foreach ($network in $networks) {
        $info = & docker inspect --format='{{.Name}}' $network 2>$null
        Write-Info "  • $info"
    }
    
    if ($IsDryRun) {
        Write-Warning "[DRY RUN] Would remove $($networks.Count) networks"
        return
    }
    
    if (-not $Force) {
        $confirmation = Read-Host "Remove $($networks.Count) networks? (y/N)"
        if ($confirmation -notmatch '^[Yy]') {
            Write-Info "Network cleanup cancelled."
            return
        }
    }
    
    foreach ($network in $networks) {
        try {
            & docker network rm $network 2>$null
            Write-Success "✓ Removed network: $network"
        }
        catch {
            Write-Warning "Failed to remove network: $network"
        }
    }
}

function Perform-SystemPrune {
    param([bool]$IsDryRun)
    
    Write-Info "Performing Docker system prune..."
    
    if ($IsDryRun) {
        Write-Warning "[DRY RUN] Would perform system prune"
        & docker system df
        return
    }
    
    if (-not $Force) {
        Write-Warning "System prune will remove:"
        Write-Warning "• All stopped containers"
        Write-Warning "• All networks not used by at least one container"
        Write-Warning "• All dangling images"
        Write-Warning "• All build cache"
        
        $confirmation = Read-Host "Perform system prune? (y/N)"
        if ($confirmation -notmatch '^[Yy]') {
            Write-Info "System prune cancelled."
            return
        }
    }
    
    & docker system prune -f
    Write-Success "✓ System prune completed"
}

function Show-DockerUsage {
    Write-Info "Docker System Usage:"
    & docker system df
    
    Write-Info ""
    Write-Info "QuokkaPack Resources:"
    
    $projects = Get-TargetProjects -Env $Environment
    
    Write-Info "Containers:"
    $containers = Get-QuokkaPackContainers -Projects $projects
    Write-Info "  Count: $($containers.Count)"
    
    Write-Info "Images:"
    $images = Get-QuokkaPackImages -Env $Environment
    Write-Info "  Count: $($images.Count)"
    
    Write-Info "Volumes:"
    $volumes = Get-QuokkaPackVolumes -Projects $projects
    Write-Info "  Count: $($volumes.Count)"
    
    Write-Info "Networks:"
    $networks = Get-QuokkaPackNetworks -Projects $projects
    Write-Info "  Count: $($networks.Count)"
}

# Main execution
try {
    Write-Info "QuokkaPack Docker Cleanup Utility"
    Write-Info "================================="
    Write-Info "Type: $Type"
    Write-Info "Environment: $Environment"
    Write-Info "Dry Run: $DryRun"
    Write-Info ""
    
    # Verify Docker is available
    try {
        docker --version | Out-Null
    }
    catch {
        throw "Docker is not available. Please ensure Docker is installed and running."
    }
    
    $projects = Get-TargetProjects -Env $Environment
    
    # Show current usage
    Show-DockerUsage
    Write-Info ""
    
    # Perform cleanup based on type
    switch ($Type) {
        'containers' {
            Cleanup-Containers -Projects $projects -IsDryRun $DryRun
        }
        'images' {
            Cleanup-Images -Env $Environment -IsDryRun $DryRun
        }
        'volumes' {
            Cleanup-Volumes -Projects $projects -IsDryRun $DryRun
        }
        'networks' {
            Cleanup-Networks -Projects $projects -IsDryRun $DryRun
        }
        'all' {
            Cleanup-Containers -Projects $projects -IsDryRun $DryRun
            Cleanup-Images -Env $Environment -IsDryRun $DryRun
            Cleanup-Networks -Projects $projects -IsDryRun $DryRun
            if ($Force -or $DryRun) {
                Cleanup-Volumes -Projects $projects -IsDryRun $DryRun
            }
        }
        'system' {
            Perform-SystemPrune -IsDryRun $DryRun
        }
    }
    
    # Perform system prune if requested
    if ($Prune) {
        Write-Info ""
        Perform-SystemPrune -IsDryRun $DryRun
    }
    
    if (-not $DryRun) {
        Write-Success "✓ Cleanup completed successfully!"
        Write-Info ""
        Show-DockerUsage
    }
}
catch {
    Write-Error "Cleanup failed: $($_.Exception.Message)"
    exit 1
}