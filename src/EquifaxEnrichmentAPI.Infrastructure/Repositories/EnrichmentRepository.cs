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
/// 398-COLUMN SCHEMA - Updated for Equifax field structure
/// Phone fields: mobile_phone_1-2, phone_1-5 (7 total)
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
    /// Searches mobile_phone_1-2 and phone_1-5 in priority order.
    /// Mobile phones prioritized (higher confidence).
    /// PostgreSQL indexes on each column enable efficient sequential search.
    ///
    /// BDD Scenario 1: Phone match (100%-70% confidence based on column)
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
        // PRIORITY-BASED SEARCH: mobile_phone_1-2 → phone_1-5 → normalized_phone
        // Searches in priority order to determine which column matched.
        // Each indexed column enables efficient query.
        // Returns immediately upon first match (highest confidence first)
        // ====================================================================

        ConsumerEnrichment? entity;

        // mobile_phone_1 - 100% confidence (index 1)
        entity = await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.mobile_phone_1 == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
            return PhoneSearchResult.CreateMatch(entity, 1);

        // mobile_phone_2 - 95% confidence (index 2)
        entity = await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.mobile_phone_2 == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
            return PhoneSearchResult.CreateMatch(entity, 2);

        // phone_1 - 90% confidence (index 3)
        entity = await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.phone_1 == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
            return PhoneSearchResult.CreateMatch(entity, 3);

        // phone_2 - 85% confidence (index 4)
        entity = await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.phone_2 == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
            return PhoneSearchResult.CreateMatch(entity, 4);

        // phone_3 - 80% confidence (index 5)
        entity = await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.phone_3 == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
            return PhoneSearchResult.CreateMatch(entity, 5);

        // phone_4 - 75% confidence (index 6)
        entity = await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.phone_4 == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
            return PhoneSearchResult.CreateMatch(entity, 6);

        // phone_5 - 70% confidence (index 7)
        entity = await _context.ConsumerEnrichments
            .AsNoTracking()
            .Where(e => e.phone_5 == normalizedPhone)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity != null)
            return PhoneSearchResult.CreateMatch(entity, 7);

        // NOTE: normalized_phone legacy fallback commented out for InMemory database compatibility
        // EF Core InMemory provider cannot translate computed C# properties in queries
        // All phones are searched via explicit columns above (mobile_phone_1-2, phone_1-5)
        // For PostgreSQL, add computed column in migration if legacy fallback needed

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
            .AnyAsync(e => e.normalized_phone == normalizedPhone, cancellationToken);
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
