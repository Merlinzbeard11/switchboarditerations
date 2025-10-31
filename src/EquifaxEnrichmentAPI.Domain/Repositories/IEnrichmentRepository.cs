using EquifaxEnrichmentAPI.Domain.Entities;

namespace EquifaxEnrichmentAPI.Domain.Repositories;

/// <summary>
/// Repository interface for consumer enrichment data.
/// Defined in Domain layer following Dependency Inversion Principle.
/// Implementation will be in Infrastructure layer with EF Core.
///
/// BDD Feature: REST API Endpoint for Phone Number Enrichment
/// BDD File: features/phase1/feature-1.1-rest-api-endpoint.feature
/// </summary>
public interface IEnrichmentRepository
{
    /// <summary>
    /// Finds enrichment data by normalized phone number.
    /// BDD Scenario 1-3: Successful lookup
    /// BDD Scenario 4: No match found
    /// </summary>
    /// <param name="normalizedPhone">Normalized 10-digit phone number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enrichment data if found, null otherwise</returns>
    Task<ConsumerEnrichment?> FindByPhoneAsync(string normalizedPhone, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds new enrichment record.
    /// Used for seeding test data and future data ingestion.
    /// </summary>
    /// <param name="enrichment">Enrichment entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(ConsumerEnrichment enrichment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if enrichment data exists for phone.
    /// Optimized query for existence check without loading full entity.
    /// </summary>
    /// <param name="normalizedPhone">Normalized 10-digit phone number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsAsync(string normalizedPhone, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes to the database.
    /// Required for Unit of Work pattern with EF Core.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of state entries written to database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
