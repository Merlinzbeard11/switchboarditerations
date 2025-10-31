using Microsoft.EntityFrameworkCore;
using EquifaxEnrichmentAPI.Domain.Entities;
using EquifaxEnrichmentAPI.Domain.Repositories;
using EquifaxEnrichmentAPI.Infrastructure.Persistence;

namespace EquifaxEnrichmentAPI.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of IEnrichmentRepository.
/// Handles persistence of consumer enrichment data to PostgreSQL.
///
/// BDD Feature: REST API Endpoint for Phone Number Enrichment
/// BDD File: features/phase1/feature-1.1-rest-api-endpoint.feature
/// </summary>
public class EnrichmentRepository : IEnrichmentRepository
{
    private readonly EnrichmentDbContext _context;

    public EnrichmentRepository(EnrichmentDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Finds enrichment data by normalized phone number.
    /// BDD Scenario 1-3: Successful lookup
    /// BDD Scenario 4: No match found (returns null)
    /// </summary>
    public async Task<ConsumerEnrichment?> FindByPhoneAsync(
        string normalizedPhone,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(normalizedPhone))
            throw new ArgumentException("Normalized phone cannot be empty", nameof(normalizedPhone));

        // AsNoTracking for read-only queries (performance optimization)
        return await _context.ConsumerEnrichments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.NormalizedPhone == normalizedPhone, cancellationToken);
    }

    /// <summary>
    /// Adds new enrichment record.
    /// Used for seeding test data and future data ingestion.
    /// </summary>
    public async Task AddAsync(ConsumerEnrichment enrichment, CancellationToken cancellationToken = default)
    {
        if (enrichment == null)
            throw new ArgumentNullException(nameof(enrichment));

        await _context.ConsumerEnrichments.AddAsync(enrichment, cancellationToken);
    }

    /// <summary>
    /// Checks if enrichment data exists for phone.
    /// Optimized query - only checks existence without loading entity.
    /// </summary>
    public async Task<bool> ExistsAsync(string normalizedPhone, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(normalizedPhone))
            throw new ArgumentException("Normalized phone cannot be empty", nameof(normalizedPhone));

        return await _context.ConsumerEnrichments
            .AnyAsync(e => e.NormalizedPhone == normalizedPhone, cancellationToken);
    }

    /// <summary>
    /// Saves all pending changes to the database.
    /// Required for Unit of Work pattern with EF Core.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
