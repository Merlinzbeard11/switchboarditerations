using System.Text.Json.Serialization;

namespace EquifaxEnrichmentAPI.Api.DTOs;

/// <summary>
/// Request payload for phone number enrichment lookup.
///
/// This is what you send when requesting enrichment data for a phone number.
/// Think of it as an order form: provide the phone number (required) and any optional
/// details (name, address) to improve match accuracy.
/// </summary>
public class LookupRequestDto
{
    /// <summary>
    /// Provider code identifying the data source (required)
    /// Expected value: "EQUIFAX_ENRICHMENT"
    /// </summary>
    [JsonPropertyName("provider_code")]
    public string? ProviderCode { get; set; }

    /// <summary>
    /// Phone number to enrich (required)
    /// Supports multiple formats - will be normalized automatically
    /// </summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    /// <summary>
    /// FCRA-compliant permissible purpose for data access (required)
    /// Valid values: insurance_underwriting, credit_extension, employment_screening,
    /// tenant_screening, legitimate_business_need
    /// </summary>
    [JsonPropertyName("permissible_purpose")]
    public string? PermissiblePurpose { get; set; }

    /// <summary>
    /// Fields to return: "basic" (~50 fields) or "full" (398 fields)
    /// Defaults to "basic" if not specified
    /// </summary>
    [JsonPropertyName("fields")]
    public string Fields { get; set; } = "basic";

    // ========================================================================
    // OPTIONAL FIELDS for improved match confidence
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
    /// </summary>
    [JsonPropertyName("unique_id")]
    public string? UniqueId { get; set; }
}
