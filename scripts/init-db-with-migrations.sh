#!/bin/bash
# Database Initialization Script with Entity Framework Migrations
# This script waits for SQL Server to be ready and then runs EF migrations

set -e

echo "Starting database initialization with migrations..."

# Configuration
DB_SERVER="${DB_SERVER:-sqlserver}"
DB_NAME="${DB_NAME:-QuokkaPackDb}"
SA_PASSWORD="${SA_PASSWORD:-YourStrongPassword123!}"
MAX_RETRIES=30
RETRY_INTERVAL=5

# Function to test SQL Server connection
test_connection() {
    /opt/mssql-tools/bin/sqlcmd -S "$DB_SERVER" -U sa -P "$SA_PASSWORD" -Q "SELECT 1" > /dev/null 2>&1
}

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to be ready..."
retry_count=0
while ! test_connection; do
    retry_count=$((retry_count + 1))
    if [ $retry_count -ge $MAX_RETRIES ]; then
        echo "ERROR: SQL Server is not ready after $MAX_RETRIES attempts"
        exit 1
    fi
    echo "SQL Server not ready, waiting... (attempt $retry_count/$MAX_RETRIES)"
    sleep $RETRY_INTERVAL
done

echo "SQL Server is ready!"

# Create database if it doesn't exist
echo "Creating database if it doesn't exist..."
/opt/mssql-tools/bin/sqlcmd -S "$DB_SERVER" -U sa -P "$SA_PASSWORD" -Q "
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '$DB_NAME')
BEGIN
    CREATE DATABASE [$DB_NAME];
    PRINT 'Database $DB_NAME created successfully.';
END
ELSE
BEGIN
    PRINT 'Database $DB_NAME already exists.';
END
"

# Run Entity Framework migrations if dotnet is available
if command -v dotnet &> /dev/null; then
    echo "Running Entity Framework migrations..."
    
    # Set connection string for migrations
    export ConnectionStrings__DefaultConnection="Server=$DB_SERVER;Database=$DB_NAME;User Id=sa;Password=$SA_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true;"
    
    # Run migrations with retry logic
    migration_retry_count=0
    while [ $migration_retry_count -lt 5 ]; do
        if dotnet ef database update --project /app/src/QuokkaPack.Data --startup-project /app/src/QuokkaPack.API --no-build; then
            echo "Entity Framework migrations completed successfully!"
            break
        else
            migration_retry_count=$((migration_retry_count + 1))
            echo "Migration attempt $migration_retry_count failed, retrying in $RETRY_INTERVAL seconds..."
            sleep $RETRY_INTERVAL
        fi
    done
    
    if [ $migration_retry_count -ge 5 ]; then
        echo "WARNING: Entity Framework migrations failed after 5 attempts"
        echo "The application will attempt to run migrations at startup"
    fi
else
    echo "dotnet CLI not available, migrations will be handled by application startup"
fi

echo "Database initialization completed!"