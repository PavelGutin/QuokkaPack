#!/bin/bash
# Enhanced Production Database Backup Script
# This script performs automated backups of the QuokkaPack database with retry logic

set -e

# Configuration from environment variables
DB_SERVER="${DB_SERVER:-sqlserver}"
DB_NAME="${DB_NAME:-QuokkaPackDb}"
SA_PASSWORD="${SA_PASSWORD}"
BACKUP_DIR="/var/opt/mssql/backup"
RETENTION_DAYS="${RETENTION_DAYS:-7}"
MAX_RETRIES=3
RETRY_DELAY=30

# Validate required environment variables
if [ -z "$SA_PASSWORD" ]; then
    echo "ERROR: SA_PASSWORD environment variable is required"
    exit 1
fi

# Create backup filename with timestamp
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="${BACKUP_DIR}/${DB_NAME}_${TIMESTAMP}.bak"

echo "=== QuokkaPack Database Backup ==="
echo "Server: $DB_SERVER"
echo "Database: $DB_NAME"
echo "Backup file: $BACKUP_FILE"
echo "Retention: $RETENTION_DAYS days"
echo "Timestamp: $(date)"
echo "=================================="

# Function to test database connection
test_connection() {
    /opt/mssql-tools/bin/sqlcmd -S "$DB_SERVER" -U sa -P "$SA_PASSWORD" -Q "SELECT 1" > /dev/null 2>&1
}

# Wait for database to be available with retry logic
echo "Testing database connection..."
retry_count=0
while ! test_connection; do
    retry_count=$((retry_count + 1))
    if [ $retry_count -ge $MAX_RETRIES ]; then
        echo "ERROR: Cannot connect to database after $MAX_RETRIES attempts"
        exit 1
    fi
    echo "Connection failed, retrying in $RETRY_DELAY seconds... (attempt $retry_count/$MAX_RETRIES)"
    sleep $RETRY_DELAY
done

echo "Database connection successful!"

# Perform the backup with retry logic
echo "Starting backup..."
retry_count=0
while [ $retry_count -lt $MAX_RETRIES ]; do
    if /opt/mssql-tools/bin/sqlcmd -S "$DB_SERVER" -U sa -P "$SA_PASSWORD" -Q "
        BACKUP DATABASE [${DB_NAME}] 
        TO DISK = '${BACKUP_FILE}' 
        WITH FORMAT, INIT, SKIP, NOREWIND, NOUNLOAD, STATS = 10, COMPRESSION;
    "; then
        echo "✓ Backup completed successfully: ${BACKUP_FILE}"
        
        # Verify backup integrity
        echo "Verifying backup integrity..."
        if /opt/mssql-tools/bin/sqlcmd -S "$DB_SERVER" -U sa -P "$SA_PASSWORD" -Q "
            RESTORE VERIFYONLY FROM DISK = '${BACKUP_FILE}';
        "; then
            echo "✓ Backup verification successful"
        else
            echo "⚠ WARNING: Backup verification failed, but backup file was created"
        fi
        
        # Get backup file size
        BACKUP_SIZE=$(ls -lh "$BACKUP_FILE" | awk '{print $5}')
        echo "Backup size: $BACKUP_SIZE"
        
        break
    else
        retry_count=$((retry_count + 1))
        if [ $retry_count -lt $MAX_RETRIES ]; then
            echo "Backup attempt $retry_count failed, retrying in $RETRY_DELAY seconds..."
            sleep $RETRY_DELAY
        else
            echo "ERROR: Backup failed after $MAX_RETRIES attempts"
            exit 1
        fi
    fi
done

# Clean up old backups
echo "Cleaning up old backups (retention: $RETENTION_DAYS days)..."
OLD_BACKUPS=$(find "$BACKUP_DIR" -name "${DB_NAME}_*.bak" -type f -mtime +$RETENTION_DAYS)
if [ -n "$OLD_BACKUPS" ]; then
    echo "Removing old backups:"
    echo "$OLD_BACKUPS"
    find "$BACKUP_DIR" -name "${DB_NAME}_*.bak" -type f -mtime +$RETENTION_DAYS -delete
    echo "✓ Old backups cleaned up"
else
    echo "No old backups to clean up"
fi

# List current backups
echo ""
echo "Current backups in $BACKUP_DIR:"
ls -lh "$BACKUP_DIR"/${DB_NAME}_*.bak 2>/dev/null || echo "No backup files found"

echo ""
echo "✓ Backup operation completed successfully!"
echo "Backup file: $BACKUP_FILE"
echo "Completed at: $(date)"