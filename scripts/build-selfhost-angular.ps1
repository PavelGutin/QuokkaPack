#!/usr/bin/env pwsh
# Build script for QuokkaPack Angular self-host Docker image

param(
    [string]$Tag = "quokkapack-selfhost-angular:latest",
    [switch]$NoBuild,
    [switch]$Run,
    [string]$DataPath = "./quokkapack-data"
)

Write-Host "Building QuokkaPack Angular self-host Docker image..." -ForegroundColor Green

# Build the Docker image
if (-not $NoBuild) {
    Write-Host "Building Docker image with tag: $Tag" -ForegroundColor Yellow
    docker build -f Dockerfile.selfhost.angular -t $Tag .
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Docker build failed!"
        exit 1
    }
    
    Write-Host "Docker image built successfully!" -ForegroundColor Green
}

# Run the container if requested
if ($Run) {
    Write-Host "Starting self-host container..." -ForegroundColor Yellow
    
    # Ensure data directory exists
    if (-not (Test-Path $DataPath)) {
        New-Item -ItemType Directory -Path $DataPath -Force | Out-Null
        Write-Host "Created data directory: $DataPath" -ForegroundColor Cyan
    }
    
    # Stop any existing container with the same name
    docker stop quokkapack-selfhost-angular 2>$null | Out-Null
    docker rm quokkapack-selfhost-angular 2>$null | Out-Null
    
    # Run the container
    docker run -d `
        --name quokkapack-selfhost-angular `
        -p 8080:80 `
        -v "${DataPath}:/app/data" `
        -e JWT_SECRET="$(openssl rand -base64 32)" `
        $Tag
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Container started successfully!" -ForegroundColor Green
        Write-Host "Application available at: http://localhost:8080" -ForegroundColor Cyan
        Write-Host "API documentation: http://localhost:8080/swagger" -ForegroundColor Cyan
        Write-Host "Data directory: $DataPath" -ForegroundColor Cyan
        Write-Host "Container name: quokkapack-selfhost-angular" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "To view logs: docker logs -f quokkapack-selfhost-angular" -ForegroundColor Yellow
        Write-Host "To stop: docker stop quokkapack-selfhost-angular" -ForegroundColor Yellow
    } else {
        Write-Error "Failed to start container!"
        exit 1
    }
}

Write-Host "Build script completed!" -ForegroundColor Green