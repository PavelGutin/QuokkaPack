#!/usr/bin/env pwsh

Write-Host "Cleaning up Visual Studio Docker containers..." -ForegroundColor Yellow

# Stop all containers
Write-Host "Stopping all containers..." -ForegroundColor Gray
docker stop $(docker ps -aq) 2>$null

# Remove all containers
Write-Host "Removing all containers..." -ForegroundColor Gray
docker rm $(docker ps -aq) 2>$null

# Remove Visual Studio specific containers
Write-Host "Removing Visual Studio containers..." -ForegroundColor Gray
docker ps -a --filter "label=com.microsoft.created-by=visual-studio" --format "{{.ID}}" | ForEach-Object {
    docker rm -f $_ 2>$null
}

# Clean up networks
Write-Host "Cleaning up networks..." -ForegroundColor Gray
docker network prune -f 2>$null

# Clean up volumes (optional - comment out if you want to keep data)
Write-Host "Cleaning up volumes..." -ForegroundColor Gray
docker volume prune -f 2>$null

Write-Host "Cleanup complete!" -ForegroundColor Green
Write-Host "You can now try running Docker Compose from Visual Studio again." -ForegroundColor Cyan