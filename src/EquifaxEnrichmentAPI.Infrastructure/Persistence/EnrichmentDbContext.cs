using Microsoft.EntityFrameworkCore;
using EquifaxEnrichmentAPI.Domain.Entities;

namespace EquifaxEnrichmentAPI.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext for Equifax Enrichment API.
/// Configured for PostgreSQL 18.
///
/// BDD Feature: REST API Endpoint for Phone Number Enrichment
/// BDD File: features/phase1/feature-1.1-rest-api-endpoint.feature
/// </summary>
public class EnrichmentDbContext : DbContext
{
    public EnrichmentDbContext(DbContextOptions<EnrichmentDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Consumer enrichment data
    /// </summary>
    public DbSet<ConsumerEnrichment> ConsumerEnrichments => Set<ConsumerEnrichment>();

    /// <summary>
    /// Buyer accounts with API key authentication
    /// BDD Feature: API Key Authentication (feature-2.1-api-key-authentication.feature)
    /// </summary>
    public DbSet<Buyer> Buyers => Set<Buyer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EnrichmentDbContext).Assembly);
    }
}
