using System.Text.Json.Serialization;

namespace EquifaxEnrichmentAPI.Api.DTOs;

/// <summary>
/// Successful response returned when enrichment data is found.
///
/// This is the complete package you receive back: includes the enrichment data,
/// a success message, and metadata showing match quality and performance metrics.
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
    /// Contains ~50 fields for "basic" or 398 fields for "full"
    /// </summary>
    [JsonPropertyName("data")]
    public EnrichmentDataDto? Data { get; set; }

    /// <summary>
    /// Response metadata for tracking and monitoring
    /// </summary>
    [JsonPropertyName("metadata")]
    public ResponseMetadataDto Metadata { get; set; } = new();
}

/// <summary>
/// The actual enrichment data - this is what you're paying for!
///
/// Contains detailed consumer information including personal details, address history,
/// phone numbers, and financial indicators. This is the core product delivered by the API.
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
/// Quality report and tracking information for the response.
///
/// Shows match confidence (how sure we are it's the right person), response time,
/// data freshness, and tracking IDs. Think of it as the receipt and quality guarantee
/// label attached to your order.
/// </summary>
public class ResponseMetadataDto
{
    /// <summary>
    /// Match confidence score (0.0 to 1.0)
    /// Higher scores indicate better match quality
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
    /// </summary>
    [JsonPropertyName("data_freshness_date")]
    public DateTime DataFreshnessDate { get; set; }

    /// <summary>
    /// Timestamp when the query was executed (UTC)
    /// </summary>
    [JsonPropertyName("query_timestamp")]
    public DateTime QueryTimestamp { get; set; }

    /// <summary>
    /// Response time in milliseconds
    /// Typically &lt; 200ms for basic requests, &lt; 300ms for full dataset
    /// </summary>
    [JsonPropertyName("response_time_ms")]
    public int ResponseTimeMs { get; set; }

    /// <summary>
    /// Server-generated request ID (UUID format)
    /// </summary>
    [JsonPropertyName("request_id")]
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Client-provided unique ID (echoed from request)
    /// Optional tracking identifier for request correlation
    /// </summary>
    [JsonPropertyName("unique_id")]
    public string? UniqueId { get; set; }

    /// <summary>
    /// Total number of fields returned (null for basic, 398 for full)
    /// </summary>
    [JsonPropertyName("total_fields_returned")]
    public int? TotalFieldsReturned { get; set; }
}

/// <summary>
/// Error response returned when something goes wrong.
///
/// Explains what went wrong in plain language. Common errors include:
/// - No match found for the phone number
/// - Invalid or missing API key
/// - Missing required fields
/// - Invalid phone number format
/// - Invalid permissible purpose
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
    /// </summary>
    [JsonPropertyName("data")]
    public object? Data { get; set; }

    /// <summary>
    /// Metadata (present for some errors, absent for others)
    /// </summary>
    [JsonPropertyName("_metadata")]
    public ResponseMetadataDto? Metadata { get; set; }
}
