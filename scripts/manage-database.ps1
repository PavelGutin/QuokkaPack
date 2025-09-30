# Database Management Script for QuokkaPack
# This script provides utilities for managing the containerized database

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("backup", "restore", "migrate", "seed", "health", "logs", "reset")]
    [string]$Action,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("dev", "prod")]
    [string]$Environment = "dev",
    
    [Parameter(Mandatory=$false)]
    [string]$BackupFile = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$Force
)

# Configuration
$ComposeFile = if ($Environment -eq "prod") { "docker-compose.prod.yml" } else { "docker-compose.dev.yml" }
$ServiceName = "sqlserver"
$DatabaseName = "QuokkaPackDb"
$SAPassword = if ($Environment -eq "prod") { $env:SA_PASSWORD } else { "YourStrongPassword123!" }

Write-Host "QuokkaPack Database Management - Environment: $Environment" -ForegroundColor Green

function Test-DockerCompose {
    if (-not (Get-Command docker-compose -ErrorAction SilentlyContinue)) {
        Write-Error "docker-compose is not installed or not in PATH"
        exit 1
    }
}

function Test-ServiceRunning {
    $running = docker-compose -f $ComposeFile ps -q $ServiceName
    if (-not $running) {
        Write-Warning "SQL Server container is not running. Starting it now..."
        docker-compose -f $ComposeFile up -d $ServiceName
        
        Write-Host "Waiting for SQL Server to be ready..." -ForegroundColor Yellow
        $timeout = 120
        $elapsed = 0
        do {
            Start-Sleep -Seconds 5
            $elapsed += 5
            $health = docker-compose -f $ComposeFile ps --format "table {{.Service}}\t{{.Status}}" | Select-String $ServiceName
            Write-Host "." -NoNewline
        } while ($elapsed -lt $timeout -and $health -notmatch "healthy")
        
        Write-Host ""
        if ($elapsed -ge $timeout) {
            Write-Error "SQL Server failed to start within $timeout seconds"
            exit 1
        }
        Write-Host "SQL Server is ready!" -ForegroundColor Green
    }
}

function Invoke-SqlCommand {
    param([string]$Query)
    
    docker-compose -f $ComposeFile exec $ServiceName /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SAPassword -Q $Query
}

function Backup-Database {
    Test-ServiceRunning
    
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupFileName = if ($BackupFile) { $BackupFile } else { "${DatabaseName}_${timestamp}.bak" }
    $backupPath = "/var/opt/mssql/backup/$backupFileName"
    
    Write-Host "Creating backup: $backupFileName" -ForegroundColor Yellow
    
    $query = "BACKUP DATABASE [$DatabaseName] TO DISK = '$backupPath' WITH FORMAT, INIT, SKIP, NOREWIND, NOUNLOAD, STATS = 10, COMPRESSION;"
    Invoke-SqlCommand -Query $query
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Backup completed successfully: $backupFileName" -ForegroundColor Green
        
        # Copy backup to host if in development
        if ($Environment -eq "dev") {
            $hostBackupDir = "./data/dev/sql/backup"
            if (-not (Test-Path $hostBackupDir)) {
                New-Item -ItemType Directory -Path $hostBackupDir -Force | Out-Null
            }
            docker cp "$(docker-compose -f $ComposeFile ps -q $ServiceName):$backupPath" "$hostBackupDir/$backupFileName"
            Write-Host "Backup copied to: $hostBackupDir/$backupFileName" -ForegroundColor Green
        }
    } else {
        Write-Error "Backup failed!"
        exit 1
    }
}

function Restore-Database {
    if (-not $BackupFile) {
        Write-Error "BackupFile parameter is required for restore operation"
        exit 1
    }
    
    Test-ServiceRunning
    
    Write-Host "Restoring database from: $BackupFile" -ForegroundColor Yellow
    
    # First, set database to single user mode
    $query1 = "ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;"
    Invoke-SqlCommand -Query $query1
    
    # Restore the database
    $backupPath = "/var/opt/mssql/backup/$BackupFile"
    $query2 = "RESTORE DATABASE [$DatabaseName] FROM DISK = '$backupPath' WITH REPLACE, STATS = 10;"
    Invoke-SqlCommand -Query $query2
    
    # Set database back to multi user mode
    $query3 = "ALTER DATABASE [$DatabaseName] SET MULTI_USER;"
    Invoke-SqlCommand -Query $query3
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Database restored successfully!" -ForegroundColor Green
    } else {
        Write-Error "Restore failed!"
        exit 1
    }
}

function Run-Migrations {
    Test-ServiceRunning
    
    Write-Host "Running Entity Framework migrations..." -ForegroundColor Yellow
    
    # Run migrations using the API container
    docker-compose -f $ComposeFile exec quokkapack.api dotnet ef database update --project /app/src/QuokkaPack.Data --startup-project /app/src/QuokkaPack.API --no-build
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Migrations completed successfully!" -ForegroundColor Green
    } else {
        Write-Error "Migrations failed!"
        exit 1
    }
}

function Seed-Database {
    Test-ServiceRunning
    
    Write-Host "Seeding database with development data..." -ForegroundColor Yellow
    
    # The DatabaseInitializationService will handle seeding automatically
    # We just need to restart the API service to trigger it
    docker-compose -f $ComposeFile restart quokkapack.api
    
    Write-Host "Database seeding initiated. Check API logs for details." -ForegroundColor Green
}

function Check-Health {
    Write-Host "Checking database health..." -ForegroundColor Yellow
    
    $query = "SELECT 1 as HealthCheck;"
    Invoke-SqlCommand -Query $query
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Database is healthy!" -ForegroundColor Green
        
        # Additional health checks
        $query2 = "SELECT name, state_desc, user_access_desc FROM sys.databases WHERE name = '$DatabaseName';"
        Write-Host "Database status:" -ForegroundColor Yellow
        Invoke-SqlCommand -Query $query2
    } else {
        Write-Error "Database health check failed!"
        exit 1
    }
}

function Show-Logs {
    Write-Host "Showing SQL Server logs..." -ForegroundColor Yellow
    docker-compose -f $ComposeFile logs -f $ServiceName
}

function Reset-Database {
    if (-not $Force) {
        $confirmation = Read-Host "This will completely reset the database. All data will be lost. Type 'RESET' to confirm"
        if ($confirmation -ne "RESET") {
            Write-Host "Operation cancelled." -ForegroundColor Yellow
            exit 0
        }
    }
    
    Write-Host "Resetting database..." -ForegroundColor Red
    
    # Stop all services
    docker-compose -f $ComposeFile down
    
    # Remove database volumes
    $volumePrefix = if ($Environment -eq "prod") { "sql_prod" } else { "sql_dev" }
    docker volume rm "${volumePrefix}_data" "${volumePrefix}_log" "${volumePrefix}_backup" -f
    
    # Start services again
    docker-compose -f $ComposeFile up -d
    
    Write-Host "Database reset completed!" -ForegroundColor Green
}

# Main execution
Test-DockerCompose

switch ($Action) {
    "backup" { Backup-Database }
    "restore" { Restore-Database }
    "migrate" { Run-Migrations }
    "seed" { Seed-Database }
    "health" { Check-Health }
    "logs" { Show-Logs }
    "reset" { Reset-Database }
}

Write-Host "Operation completed!" -ForegroundColor Green