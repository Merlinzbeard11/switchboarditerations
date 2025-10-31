using System.Text.Json.Serialization;

namespace EquifaxEnrichmentAPI.Api.DTOs;

/// <summary>
/// Response DTO for successful phone number enrichment lookup.
/// BDD Scenarios 1, 2, 3: Success responses with data
/// BDD File: features/phase1/feature-1.1-rest-api-endpoint.feature
/// </summary>
public class LookupResponseDto
{
    /// <summary>
    /// Response status: "success" or "error"
    /// </summary>
    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable message describing the result
    /// Example: "Record found with high confidence"
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Enrichment data payload (null for error responses)
    /// BDD Scenario 1: Contains ~50 fields for "basic"
    /// BDD Scenario 2: Contains 398 fields for "full"
    /// </summary>
    [JsonPropertyName("data")]
    public EnrichmentDataDto? Data { get; set; }

    /// <summary>
    /// Response metadata for tracking and monitoring
    /// BDD Scenario 12: Complete metadata requirements
    /// </summary>
    [JsonPropertyName("_metadata")]
    public ResponseMetadataDto Metadata { get; set; } = new();
}

/// <summary>
/// Enrichment data structure containing consumer information.
/// BDD Scenario 1: Basic structure with personal info, addresses, phones, financial
/// </summary>
public class EnrichmentDataDto
{
    /// <summary>
    /// Equifax consumer key identifier
    /// </summary>
    [JsonPropertyName("consumer_key")]
    public string? ConsumerKey { get; set; }

    /// <summary>
    /// Personal information (name, age, etc.)
    /// Placeholder for now - will be expanded in later iterations
    /// </summary>
    [JsonPropertyName("personal_info")]
    public object? PersonalInfo { get; set; }

    /// <summary>
    /// Address history
    /// Placeholder for now - will be expanded in later iterations
    /// </summary>
    [JsonPropertyName("addresses")]
    public object[]? Addresses { get; set; }

    /// <summary>
    /// Phone number history
    /// Placeholder for now - will be expanded in later iterations
    /// </summary>
    [JsonPropertyName("phones")]
    public object[]? Phones { get; set; }

    /// <summary>
    /// Financial information
    /// Placeholder for now - will be expanded in later iterations
    /// </summary>
    [JsonPropertyName("financial")]
    public object? Financial { get; set; }
}

/// <summary>
/// Response metadata for tracking, monitoring, and auditing.
/// BDD Scenario 12: Complete metadata requirements (Lines 285-306)
/// </summary>
public class ResponseMetadataDto
{
    /// <summary>
    /// Match confidence score (0.0 to 1.0)
    /// BDD Scenario 3: Should be > 0.90 with optional fields
    /// </summary>
    [JsonPropertyName("match_confidence")]
    public double MatchConfidence { get; set; }

    /// <summary>
    /// Type of match performed (e.g., "phone_only", "phone_with_name")
    /// </summary>
    [JsonPropertyName("match_type")]
    public string MatchType { get; set; } = string.Empty;

    /// <summary>
    /// Date when the data was last updated (ISO 8601 format)
    /// BDD Scenario 12: Valid ISO 8601 date
    /// </summary>
    [JsonPropertyName("data_freshness_date")]
    public DateTime DataFreshnessDate { get; set; }

    /// <summary>
    /// Timestamp when the query was executed (UTC)
    /// BDD Scenario 12: Current UTC timestamp
    /// </summary>
    [JsonPropertyName("query_timestamp")]
    public DateTime QueryTimestamp { get; set; }

    /// <summary>
    /// Response time in milliseconds
    /// BDD Scenario 13: Should be < 200ms for basic, < 300ms for full
    /// </summary>
    [JsonPropertyName("response_time_ms")]
    public int ResponseTimeMs { get; set; }

    /// <summary>
    /// Server-generated request ID (UUID format)
    /// BDD Scenario 12: Non-empty UUID format
    /// </summary>
    [JsonPropertyName("request_id")]
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Client-provided unique ID (echoed from request)
    /// BDD Scenario 3, 12: Optional tracking identifier
    /// </summary>
    [JsonPropertyName("unique_id")]
    public string? UniqueId { get; set; }

    /// <summary>
    /// Total number of fields returned (null for basic, 398 for full)
    /// BDD Scenario 2: Should show 398 for full dataset
    /// BDD Scenario 11: null for basic (default)
    /// </summary>
    [JsonPropertyName("total_fields_returned")]
    public int? TotalFieldsReturned { get; set; }
}

/// <summary>
/// Error response DTO for failed lookups.
/// BDD Scenario 4: No match found
/// BDD Scenario 5: Invalid API key
/// BDD Scenario 6: Missing required fields
/// BDD Scenario 7: Invalid phone format
/// BDD Scenario 8: Invalid permissible purpose
/// </summary>
public class ErrorResponseDto
{
    /// <summary>
    /// Response status: always "error"
    /// </summary>
    [JsonPropertyName("response")]
    public string Response { get; set; } = "error";

    /// <summary>
    /// Error message describing what went wrong
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional error details (e.g., phone number attempted for no-match)
    /// BDD Scenario 4: Includes phone, match_attempted, match_confidence
    /// </summary>
    [JsonPropertyName("data")]
    public object? Data { get; set; }

    /// <summary>
    /// Metadata (present for some errors, absent for others)
    /// BDD Scenario 4: No match includes metadata
    /// BDD Scenario 5: Invalid API key does NOT include metadata
    /// </summary>
    [JsonPropertyName("_metadata")]
    public ResponseMetadataDto? Metadata { get; set; }
}
