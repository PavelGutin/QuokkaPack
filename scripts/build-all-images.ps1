#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds all QuokkaPack Docker images with a single command.

.DESCRIPTION
    This script builds all Docker images for the QuokkaPack application including:
    - Individual project images (API, Razor, Blazor, Angular)
    - Self-host all-in-one images
    - Development and production variants

.PARAMETER ImageType
    Specifies which images to build. Valid values: 'all', 'individual', 'selfhost'
    Default: 'all'

.PARAMETER Environment
    Specifies the target environment. Valid values: 'dev', 'prod', 'both'
    Default: 'both'

.PARAMETER NoCache
    Forces Docker to rebuild images without using cache

.EXAMPLE
    .\build-all-images.ps1
    Builds all images for both development and production

.EXAMPLE
    .\build-all-images.ps1 -ImageType individual -Environment dev
    Builds only individual project images for development

.EXAMPLE
    .\build-all-images.ps1 -NoCache
    Builds all images without using Docker cache
#>

param(
    [ValidateSet('all', 'individual', 'selfhost')]
    [string]$ImageType = 'all',
    
    [ValidateSet('dev', 'prod', 'both')]
    [string]$Environment = 'both',
    
    [switch]$NoCache
)

# Set error action preference
$ErrorActionPreference = 'Stop'

# Color functions for output
function Write-Success { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-Info { param($Message) Write-Host $Message -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host $Message -ForegroundColor Red }

# Build configuration
$BuildArgs = @()
if ($NoCache) {
    $BuildArgs += '--no-cache'
}

# Image definitions
$IndividualImages = @(
    @{ Name = 'quokkapack-api'; Dockerfile = 'src/QuokkaPack.API/Dockerfile'; Context = '.' }
    @{ Name = 'quokkapack-razor'; Dockerfile = 'src/QuokkaPack.Razor/Dockerfile'; Context = '.' }
    @{ Name = 'quokkapack-blazor'; Dockerfile = 'src/QuokkaPack.Blazor/Dockerfile'; Context = '.' }
    @{ Name = 'quokkapack-angular'; Dockerfile = 'src/QuokkaPack.Angular/Dockerfile'; Context = '.' }
)

$SelfHostImages = @(
    @{ Name = 'quokkapack-selfhost-razor'; Dockerfile = 'Dockerfile.selfhost.razor'; Context = '.' }
    @{ Name = 'quokkapack-selfhost-blazor'; Dockerfile = 'Dockerfile.selfhost.blazor'; Context = '.' }
    @{ Name = 'quokkapack-selfhost-angular'; Dockerfile = 'Dockerfile.selfhost.angular'; Context = '.' }
)

function Build-Image {
    param(
        [string]$Name,
        [string]$Dockerfile,
        [string]$Context,
        [string]$Tag
    )
    
    Write-Info "Building image: $Name with tag: $Tag"
    
    try {
        $buildCmd = @('docker', 'build') + $BuildArgs + @('-f', $Dockerfile, '-t', "$Name`:$Tag", $Context)
        & $buildCmd
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "✓ Successfully built $Name`:$Tag"
        } else {
            throw "Docker build failed with exit code $LASTEXITCODE"
        }
    }
    catch {
        Write-Error "✗ Failed to build $Name`:$Tag - $($_.Exception.Message)"
        throw
    }
}

function Build-ImageSet {
    param(
        [array]$Images,
        [string]$Environment
    )
    
    foreach ($image in $Images) {
        $tags = @()
        
        if ($Environment -eq 'dev' -or $Environment -eq 'both') {
            $tags += 'dev'
        }
        if ($Environment -eq 'prod' -or $Environment -eq 'both') {
            $tags += 'latest'
        }
        
        foreach ($tag in $tags) {
            Build-Image -Name $image.Name -Dockerfile $image.Dockerfile -Context $image.Context -Tag $tag
        }
    }
}

# Main execution
try {
    Write-Info "Starting QuokkaPack Docker image build process..."
    Write-Info "Image Type: $ImageType"
    Write-Info "Environment: $Environment"
    Write-Info "No Cache: $NoCache"
    Write-Info ""
    
    # Verify Docker is available
    try {
        docker --version | Out-Null
    }
    catch {
        throw "Docker is not available. Please ensure Docker is installed and running."
    }
    
    $startTime = Get-Date
    
    # Build individual project images
    if ($ImageType -eq 'all' -or $ImageType -eq 'individual') {
        Write-Info "Building individual project images..."
        Build-ImageSet -Images $IndividualImages -Environment $Environment
        Write-Success "Individual project images completed!"
        Write-Info ""
    }
    
    # Build self-host images
    if ($ImageType -eq 'all' -or $ImageType -eq 'selfhost') {
        Write-Info "Building self-host all-in-one images..."
        Build-ImageSet -Images $SelfHostImages -Environment $Environment
        Write-Success "Self-host images completed!"
        Write-Info ""
    }
    
    $endTime = Get-Date
    $duration = $endTime - $startTime
    
    Write-Success "🎉 All Docker images built successfully!"
    Write-Info "Total build time: $($duration.ToString('mm\:ss'))"
    
    # Display built images
    Write-Info ""
    Write-Info "Built images:"
    docker images --filter "reference=quokkapack*" --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}\t{{.CreatedSince}}"
}
catch {
    Write-Error "Build process failed: $($_.Exception.Message)"
    exit 1
}