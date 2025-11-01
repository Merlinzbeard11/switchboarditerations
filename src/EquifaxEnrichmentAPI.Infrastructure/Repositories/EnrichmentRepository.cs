using Microsoft.EntityFrameworkCore;
using EquifaxEnrichmentAPI.Domain.Entities;
using EquifaxEnrichmentAPI.Domain.Repositories;
using EquifaxEnrichmentAPI.Domain.ValueObjects;
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
    /// Slice 4: Returns PhoneSearchResult with matched column and calculated confidence.
    /// Searches Phone1-Phone10 in priority order (Phone1 first = 100% confidence).
    /// PostgreSQL indexes on each column enable efficient sequential search.
    ///
    /// BDD Scenario 1: Phone1 match (100% confidence)
    /// BDD Scenario 2: Phone2-Phone10 match (95%-55% confidence)
    /// BDD Scenario 3: No match found (returns PhoneSearchResult.CreateNoMatch())
    /// BDD Scenario 4: Duplicate phone returns first match (highest confidence)
    /// BDD Scenario 14: Confidence formula = 100 - ((column_index - 1) * 5) / 100
    /// </summary>
    public async Task<PhoneSearchResult> FindByPhoneAsync(
        string normalizedPhone,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(normalizedPhone))
            throw new ArgumentException("Normalized phone cannot be empty", nameof(normalizedPhone));

        // ====================================================================
        // PRIORITY-BASED SEARCH: Phone1 → Phone2 → ... → Phone10 → NormalizedPhone
        // Searches in priority order to determine which column matched.
        // Each column has an index, so EF Core generates efficient queries.
        // Returns immediately upon first match (BDD Scenario 4: highest confidence first)
        // ====================================================================

        ConsumerEnrichment? entity;

        // Phone1 - 100% confidence
        entity = await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.Phone1 == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
            return PhoneSearchResult.CreateMatch(entity, 1);

        // Phone2 - 95% confidence
        entity = await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.Phone2 == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
            return PhoneSearchResult.CreateMatch(entity, 2);

        // Phone3 - 90% confidence
        entity = await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.Phone3 == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
            return PhoneSearchResult.CreateMatch(entity, 3);

        // Phone4 - 85% confidence
        entity = await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.Phone4 == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
            return PhoneSearchResult.CreateMatch(entity, 4);

        // Phone5 - 80% confidence
        entity = await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.Phone5 == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
            return PhoneSearchResult.CreateMatch(entity, 5);

        // Phone6 - 75% confidence
        entity = await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.Phone6 == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
            return PhoneSearchResult.CreateMatch(entity, 6);

        // Phone7 - 70% confidence
        entity = await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.Phone7 == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
            return PhoneSearchResult.CreateMatch(entity, 7);

        // Phone8 - 65% confidence
        entity = await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.Phone8 == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
            return PhoneSearchResult.CreateMatch(entity, 8);

        // Phone9 - 60% confidence
        entity = await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.Phone9 == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
            return PhoneSearchResult.CreateMatch(entity, 9);

        // Phone10 - 55% confidence
        entity = await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.Phone10 == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
            return PhoneSearchResult.CreateMatch(entity, 10);

        // NormalizedPhone - Legacy fallback (uses entity's stored confidence)
        // NOTE: Backward compatibility during migration
        // TODO (Slice 5): After populating Phone1-Phone10, remove this fallback
        entity = await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.NormalizedPhone == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
            return PhoneSearchResult.CreateLegacyMatch(entity);

        // No match found - BDD Scenario 3
        return PhoneSearchResult.CreateNoMatch();
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
