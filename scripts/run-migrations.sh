#!/bin/bash
# ============================================================================
# EF Core Migration Runner for Production
# Runs database migrations before application starts
# ============================================================================

set -e

echo "======================================================================"
echo "EF Core Migration Runner - v1.0"
echo "======================================================================"

# Build connection string from environment variables
DB_HOST="${DB_HOST:?DB_HOST environment variable is required}"
DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:?DB_NAME environment variable is required}"
DB_USER="${DB_USER:?DB_USER environment variable is required}"
DB_PASSWORD="${DB_PASSWORD:?DB_PASSWORD environment variable is required}"

CONNECTION_STRING="Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD};Include Error Detail=true"

echo "Database: ${DB_NAME}"
echo "Host: ${DB_HOST}:${DB_PORT}"
echo "User: ${DB_USER}"
echo ""

# Test database connectivity
echo "Testing database connectivity..."
until PGPASSWORD="${DB_PASSWORD}" psql -h "${DB_HOST}" -p "${DB_PORT}" -U "${DB_USER}" -d "${DB_NAME}" -c '\q' 2>/dev/null; do
  echo "Waiting for database to be ready..."
  sleep 2
done

echo "✅ Database connection successful"
echo ""

# Run EF Core migrations
echo "Applying EF Core migrations..."
cd /app
dotnet EquifaxEnrichmentAPI.Infrastructure.dll ef database update \
  --connection "${CONNECTION_STRING}" \
  --project EquifaxEnrichmentAPI.Infrastructure.csproj \
  --startup-project EquifaxEnrichmentAPI.Api.csproj

if [ $? -eq 0 ]; then
  echo "✅ Migrations applied successfully"
  exit 0
else
  echo "❌ Migration failed"
  exit 1
fi
