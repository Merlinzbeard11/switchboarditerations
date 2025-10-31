using System.Text.Json.Serialization;

namespace EquifaxEnrichmentAPI.Api.DTOs;

/// <summary>
/// Request DTO for phone number enrichment lookup.
/// Maps to BDD Scenario 1-3: Basic fields, full fields, and optional fields for improved matching.
/// BDD File: features/phase1/feature-1.1-rest-api-endpoint.feature
/// </summary>
public class LookupRequestDto
{
    /// <summary>
    /// API key for authentication (required)
    /// BDD Scenario 5: Invalid API key validation
    /// </summary>
    [JsonPropertyName("api_key")]
    public string? ApiKey { get; set; }

    /// <summary>
    /// Provider code identifying the data source (required)
    /// Expected value: "EQUIFAX_ENRICHMENT"
    /// </summary>
    [JsonPropertyName("provider_code")]
    public string? ProviderCode { get; set; }

    /// <summary>
    /// Phone number to enrich (required)
    /// Supports multiple formats - will be normalized using PhoneNumber value object
    /// BDD Scenario 7: Phone number format validation
    /// BDD Scenario 10: Phone number normalization
    /// </summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    /// <summary>
    /// FCRA-compliant permissible purpose for data access (required)
    /// Valid values: insurance_underwriting, credit_extension, employment_screening,
    /// tenant_screening, legitimate_business_need
    /// BDD Scenario 8: Permissible purpose validation
    /// </summary>
    [JsonPropertyName("permissible_purpose")]
    public string? PermissiblePurpose { get; set; }

    /// <summary>
    /// Fields to return: "basic" (~50 fields) or "full" (398 fields)
    /// Defaults to "basic" if not specified
    /// BDD Scenario 2: Full dataset request
    /// BDD Scenario 11: Default fields selection
    /// </summary>
    [JsonPropertyName("fields")]
    public string Fields { get; set; } = "basic";

    // ========================================================================
    // OPTIONAL FIELDS for improved match confidence
    // BDD Scenario 3: Optional fields increase match_confidence above 0.90
    // ========================================================================

    /// <summary>
    /// Consumer first name (optional, improves match confidence)
    /// </summary>
    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Consumer last name (optional, improves match confidence)
    /// </summary>
    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    /// <summary>
    /// 5-digit US postal code (optional, improves match confidence)
    /// </summary>
    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; set; }

    /// <summary>
    /// 2-letter US state code (optional, improves match confidence)
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }

    /// <summary>
    /// IPv4 address (optional, for additional context)
    /// </summary>
    [JsonPropertyName("ip_address")]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Client-provided unique identifier for request tracking (optional)
    /// Returned in response metadata for correlation
    /// BDD Scenario 12: Metadata completeness
    /// </summary>
    [JsonPropertyName("unique_id")]
    public string? UniqueId { get; set; }
}
