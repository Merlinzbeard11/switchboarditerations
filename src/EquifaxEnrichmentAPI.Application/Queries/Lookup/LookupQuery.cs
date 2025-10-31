using MediatR;

namespace EquifaxEnrichmentAPI.Application.Queries.Lookup;

/// <summary>
/// CQRS Query for phone number enrichment lookup.
/// BDD Feature: REST API Endpoint for Phone Number Enrichment
/// BDD File: features/phase1/feature-1.1-rest-api-endpoint.feature
///
/// This query encapsulates all input parameters for enrichment lookup,
/// following CQRS pattern to separate read operations from commands.
/// </summary>
public class LookupQuery : IRequest<LookupResult>
{
    /// <summary>
    /// Phone number to enrich (required, will be normalized)
    /// </summary>
    public string Phone { get; init; } = string.Empty;

    /// <summary>
    /// Fields to return: "basic" (~50 fields) or "full" (398 fields)
    /// Defaults to "basic" if not specified
    /// </summary>
    public string Fields { get; init; } = "basic";

    // ====================================================================
    // OPTIONAL FIELDS FOR IMPROVED MATCH CONFIDENCE
    // BDD Scenario 3: Optional fields increase match_confidence > 0.90
    // ====================================================================

    /// <summary>
    /// Consumer first name (optional, improves match confidence)
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// Consumer last name (optional, improves match confidence)
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// 5-digit US postal code (optional, improves match confidence)
    /// </summary>
    public string? PostalCode { get; init; }

    /// <summary>
    /// 2-letter US state code (optional, improves match confidence)
    /// </summary>
    public string? State { get; init; }

    /// <summary>
    /// IPv4 address (optional, improves match confidence)
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Client-provided tracking identifier (optional, echoed in response metadata)
    /// BDD Scenario 3, 12: Used for request correlation
    /// </summary>
    public string? UniqueId { get; init; }
}
