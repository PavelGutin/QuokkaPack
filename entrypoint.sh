#!/bin/bash
set -e

echo "Starting QuokkaPack entrypoint..."

# Run EF Core migrations using the migration bundle
echo "Running Entity Framework migrations..."
cd /app

# Check if migration bundle exists
if [ -f "/app/efbundle" ]; then
    # Run migrations with retry logic
    MIGRATION_RETRIES=5
    MIGRATION_COUNT=0

    while [ $MIGRATION_COUNT -lt $MIGRATION_RETRIES ]; do
        if /app/efbundle --connection "$ConnectionStrings__DefaultConnection" 2>&1; then
            echo "Migrations applied successfully!"
            break
        fi

        MIGRATION_COUNT=$((MIGRATION_COUNT + 1))
        if [ $MIGRATION_COUNT -eq $MIGRATION_RETRIES ]; then
            echo "ERROR: Failed to apply migrations after $MIGRATION_RETRIES attempts"
            exit 1
        fi

        echo "Migration attempt $MIGRATION_COUNT failed, retrying in 5 seconds..."
        sleep 5
    done
else
    echo "No migration bundle found, skipping migrations"
fi

echo "Starting application with supervisord..."
exec /usr/bin/supervisord -c /etc/supervisor/conf.d/supervisord.conf
