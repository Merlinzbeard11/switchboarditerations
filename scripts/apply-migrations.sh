#!/bin/bash
# ============================================================================
# SQL Migration Runner for Production
# Applies idempotent EF Core migrations via SQL script
# ============================================================================

set -e

echo "======================================================================"
echo "SQL Migration Runner - v1.0"
echo "Date: $(date)"
echo "======================================================================"

# Validate environment variables
: "${DB_HOST:?DB_HOST environment variable is required}"
: "${DB_PORT:=5432}"
: "${DB_NAME:?DB_NAME environment variable is required}"
: "${DB_USER:?DB_USER environment variable is required}"
: "${DB_PASSWORD:?DB_PASSWORD environment variable is required}"

echo "Database: ${DB_NAME}@${DB_HOST}:${DB_PORT}"
echo "User: ${DB_USER}"
echo ""

# Test connectivity
echo "Testing database connectivity..."
export PGPASSWORD="${DB_PASSWORD}"
until psql -h "${DB_HOST}" -p "${DB_PORT}" -U "${DB_USER}" -d "${DB_NAME}" -c '\q' 2>/dev/null; do
  echo "⏳ Waiting for database to be ready..."
  sleep 3
done

echo "✅ Database connection successful"
echo ""

# Apply SQL migrations
echo "Applying SQL migrations..."
psql -h "${DB_HOST}" -p "${DB_PORT}" -U "${DB_USER}" -d "${DB_NAME}" -f /app/migrations.sql -v ON_ERROR_STOP=1

if [ $? -eq 0 ]; then
  echo "✅ Migrations applied successfully"
  exit 0
else
  echo "❌ Migration failed"
  exit 1
fi
