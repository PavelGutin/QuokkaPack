#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Runs automated container functionality tests for QuokkaPack
.DESCRIPTION
    This script runs comprehensive tests for Docker containers including:
    - Build verification tests for all Dockerfiles
    - Docker Compose integration tests for development and production environments
    - Self-host container deployment and functionality tests
    - Container performance and resource usage tests
.PARAMETER TestCategory
    Specific category of tests to run (BuildVerification, Integration, SelfHost, Performance, All)
.PARAMETER Parallel
    Run tests in parallel (default: false for container tests)
.PARAMETER Verbose
    Enable verbose logging
.EXAMPLE
    .\test-containers.ps1
    .\test-containers.ps1 -TestCategory BuildVerification
    .\test-containers.ps1 -TestCategory Integration -Verbose
#>

param(
    [Parameter()]
    [ValidateSet("BuildVerification", "Integration", "SelfHost", "Performance", "All")]
    [string]$TestCategory = "All",
    
    [Parameter()]
    [switch]$Parallel = $false,
    
    [Parameter()]
    [switch]$Verbose = $false
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Get script directory and repository root
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir

Write-Host "🧪 QuokkaPack Container Tests" -ForegroundColor Cyan
Write-Host "Repository: $RepoRoot" -ForegroundColor Gray
Write-Host "Test Category: $TestCategory" -ForegroundColor Gray
Write-Host ""

# Check prerequisites
Write-Host "🔍 Checking prerequisites..." -ForegroundColor Yellow

# Check if Docker is available
try {
    $dockerVersion = docker --version
    Write-Host "✅ Docker: $dockerVersion" -ForegroundColor Green
} catch {
    Write-Error "❌ Docker is not available. Please install Docker Desktop."
    exit 1
}

# Check if Docker Compose is available
try {
    $composeVersion = docker-compose --version
    Write-Host "✅ Docker Compose: $composeVersion" -ForegroundColor Green
} catch {
    Write-Error "❌ Docker Compose is not available. Please install Docker Compose."
    exit 1
}

# Check if .NET is available
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Error "❌ .NET SDK is not available. Please install .NET 9.0 SDK."
    exit 1
}

Write-Host ""

# Build the test project
Write-Host "🔨 Building container test project..." -ForegroundColor Yellow
Push-Location $RepoRoot
try {
    dotnet build tests/QuokkaPack.ContainerTests/QuokkaPack.ContainerTests.csproj --configuration Release
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }
    Write-Host "✅ Test project built successfully" -ForegroundColor Green
} catch {
    Write-Error "❌ Failed to build test project: $_"
    exit 1
} finally {
    Pop-Location
}

Write-Host ""

# Prepare test environment
Write-Host "🧹 Cleaning up previous test containers..." -ForegroundColor Yellow
try {
    # Stop and remove any existing test containers
    $testContainers = docker ps -a --filter "name=*test*" --format "{{.ID}}"
    if ($testContainers) {
        docker stop $testContainers 2>$null
        docker rm $testContainers 2>$null
    }
    
    # Remove test images
    $testImages = docker images --filter "reference=*test*" --format "{{.ID}}"
    if ($testImages) {
        docker rmi $testImages --force 2>$null
    }
    
    Write-Host "✅ Test environment cleaned" -ForegroundColor Green
} catch {
    Write-Warning "⚠️ Some cleanup operations failed, continuing..."
}

Write-Host ""

# Run tests based on category
$testFilter = switch ($TestCategory) {
    "BuildVerification" { "--filter Category=BuildVerification" }
    "Integration" { "--filter Category=Integration" }
    "SelfHost" { "--filter Category=SelfHost" }
    "Performance" { "--filter Category=Performance" }
    "All" { "" }
}

$verbosityLevel = if ($Verbose) { "detailed" } else { "normal" }
$parallelOption = if ($Parallel) { "" } else { "--parallel none" }

Write-Host "🚀 Running container tests..." -ForegroundColor Yellow
Write-Host "Filter: $testFilter" -ForegroundColor Gray
Write-Host ""

Push-Location $RepoRoot
try {
    $testCommand = "dotnet test tests/QuokkaPack.ContainerTests/QuokkaPack.ContainerTests.csproj --configuration Release --verbosity $verbosityLevel $parallelOption $testFilter --logger trx --logger console"
    
    Write-Host "Executing: $testCommand" -ForegroundColor Gray
    Invoke-Expression $testCommand
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ All container tests passed!" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "❌ Some container tests failed!" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Error "❌ Failed to run container tests: $_"
    exit 1
} finally {
    Pop-Location
}

# Final cleanup
Write-Host ""
Write-Host "🧹 Final cleanup..." -ForegroundColor Yellow
try {
    # Clean up any remaining test containers and images
    $testContainers = docker ps -a --filter "name=*test*" --format "{{.ID}}"
    if ($testContainers) {
        docker stop $testContainers 2>$null
        docker rm $testContainers 2>$null
    }
    
    $testImages = docker images --filter "reference=*test*" --format "{{.ID}}"
    if ($testImages) {
        docker rmi $testImages --force 2>$null
    }
    
    # Clean up any test volumes
    $testVolumes = docker volume ls --filter "name=*test*" --format "{{.Name}}"
    if ($testVolumes) {
        docker volume rm $testVolumes 2>$null
    }
    
    Write-Host "✅ Cleanup completed" -ForegroundColor Green
} catch {
    Write-Warning "⚠️ Some cleanup operations failed"
}

Write-Host ""
Write-Host "🎉 Container testing completed!" -ForegroundColor Cyan