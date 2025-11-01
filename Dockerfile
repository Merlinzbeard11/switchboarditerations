# ============================================================================
# Multi-Stage Dockerfile for ASP.NET Core 9.0 - AWS App Runner Optimized
# Addresses App Runner gotchas: port 8080, health checks, non-root user
# Research source: AWS official docs + Stack Overflow best practices 2025
# ============================================================================

# ============================================================================
# STAGE 1: Build Stage
# Uses full SDK to restore packages and compile application
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy project files for dependency restoration
# Doing this separately leverages Docker layer caching
COPY src/EquifaxEnrichmentAPI.Domain/*.csproj ./src/EquifaxEnrichmentAPI.Domain/
COPY src/EquifaxEnrichmentAPI.Application/*.csproj ./src/EquifaxEnrichmentAPI.Application/
COPY src/EquifaxEnrichmentAPI.Infrastructure/*.csproj ./src/EquifaxEnrichmentAPI.Infrastructure/
COPY src/EquifaxEnrichmentAPI.Api/*.csproj ./src/EquifaxEnrichmentAPI.Api/

# Restore dependencies (from API project - pulls all dependencies)
# This layer is cached until project files change
WORKDIR /source/src/EquifaxEnrichmentAPI.Api
RUN dotnet restore

# Copy all source code
WORKDIR /source
COPY src/ ./src/

# Build and publish the application
# --configuration Release: Optimized production build
# --output /app: Output to /app directory
# --no-restore: Skip restore (already done above)
# /p:UseAppHost=false: Don't create native executable (not needed in container)
WORKDIR /source/src/EquifaxEnrichmentAPI.Api
RUN dotnet publish \
    --configuration Release \
    --output /app \
    --no-restore \
    /p:UseAppHost=false

# ============================================================================
# STAGE 2: Runtime Stage
# Minimal runtime image - no SDK, smaller size, faster startup
# ============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Install PostgreSQL client for migration runner
RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /app

# Copy published application from build stage
COPY --from=build /app .

# Copy documentation folder with full enrichment example
COPY docs/ /app/docs/

# Copy migrations and runner script
COPY migrations.sql /app/migrations.sql
COPY scripts/apply-migrations.sh /app/apply-migrations.sh
RUN chmod +x /app/apply-migrations.sh

# ============================================================================
# AWS App Runner Requirements (Critical - addresses gotchas)
# ============================================================================

# GOTCHA FIX #1: App Runner requires port 8080
# Default ASP.NET Core uses port 5000/5001, but App Runner expects 8080
ENV ASPNETCORE_URLS=http://+:8080

# GOTCHA FIX #2: Set production environment
ENV ASPNETCORE_ENVIRONMENT=Production

# GOTCHA FIX #3: Run as non-root user for security
# AWS App Runner best practice: don't run as root
RUN adduser --disabled-password --gecos "" apprunner && \
    chown -R apprunner:apprunner /app
USER apprunner

# Expose port 8080 (App Runner requirement)
EXPOSE 8080

# Health check endpoint (App Runner requirement)
# App Runner will ping /health to verify container is healthy
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# ============================================================================
# Application Entry Point
# ============================================================================
ENTRYPOINT ["dotnet", "EquifaxEnrichmentAPI.Api.dll"]
