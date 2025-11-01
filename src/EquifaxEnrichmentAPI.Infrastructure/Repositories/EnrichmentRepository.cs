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
    /// Finds enrichment data by searching phone across multiple columns.
    /// Feature 1.3: Multi-Phone Search Database Enhancement
    /// BDD File: features/phase1/feature-1.3-database-query-multi-phone.feature
    ///
    /// Searches Phone1 through Phone10 columns using multi-column OR query.
    /// PostgreSQL will use bitmap index scan to combine indexes efficiently.
    ///
    /// BDD Scenario 1: Phone1 match (100% confidence)
    /// BDD Scenario 2: Phone2-Phone10 match (95%-55% confidence)
    /// BDD Scenario 3: No match found (returns null)
    /// BDD Scenario 4: Duplicate phone returns first match (highest confidence)
    /// </summary>
    public async Task<ConsumerEnrichment?> FindByPhoneAsync(
        string normalizedPhone,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(normalizedPhone))
            throw new ArgumentException("Normalized phone cannot be empty", nameof(normalizedPhone));

        // Multi-column OR query: Search Phone1 through Phone10 + legacy NormalizedPhone
        // PostgreSQL bitmap index scan combines indexes for optimal performance
        // AsNoTracking for read-only queries (N+1 gotcha mitigation)
        //
        // NOTE: NormalizedPhone included for backward compatibility during migration
        // TODO (Slice 5): After populating Phone1-Phone10, remove NormalizedPhone from query
        return await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.NormalizedPhone == normalizedPhone  // Legacy - remove after migration
                     || e.Phone1 == normalizedPhone
                     || e.Phone2 == normalizedPhone
                     || e.Phone3 == normalizedPhone
                     || e.Phone4 == normalizedPhone
                     || e.Phone5 == normalizedPhone
                     || e.Phone6 == normalizedPhone
                     || e.Phone7 == normalizedPhone
                     || e.Phone8 == normalizedPhone
                     || e.Phone9 == normalizedPhone
                     || e.Phone10 == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
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
