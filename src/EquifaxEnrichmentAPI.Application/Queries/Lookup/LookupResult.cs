namespace EquifaxEnrichmentAPI.Application.Queries.Lookup;

/// <summary>
/// Application-layer result model for enrichment lookup query.
/// Maps to LookupResponseDto in API layer.
/// </summary>
public class LookupResult
{
    /// <summary>
    /// Result status: "success" or "error"
    /// </summary>
    public string Response { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable message describing the result
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Enrichment data payload (null for error responses)
    /// </summary>
    public EnrichmentData? Data { get; init; }

    /// <summary>
    /// Response metadata for tracking and monitoring
    /// </summary>
    public ResponseMetadata Metadata { get; init; } = new();
}

/// <summary>
/// Enrichment data structure containing consumer information.
/// </summary>
public class EnrichmentData
{
    public string? ConsumerKey { get; init; }
    public object? PersonalInfo { get; init; }
    public object[]? Addresses { get; init; }
    public object[]? Phones { get; init; }
    public object? Financial { get; init; }
}

/// <summary>
/// Response metadata for tracking, monitoring, and auditing.
/// </summary>
public class ResponseMetadata
{
    /// <summary>
    /// Match confidence score (0.0 to 1.0)
    /// </summary>
    public double MatchConfidence { get; init; }

    /// <summary>
    /// Type of match performed (e.g., "phone_only", "phone_with_name")
    /// </summary>
    public string MatchType { get; init; } = string.Empty;

    /// <summary>
    /// Date when the data was last updated
    /// </summary>
    public DateTime DataFreshnessDate { get; init; }

    /// <summary>
    /// Timestamp when the query was executed (UTC)
    /// </summary>
    public DateTime QueryTimestamp { get; init; }

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public int ResponseTimeMs { get; init; }

    /// <summary>
    /// Server-generated request ID (UUID format)
    /// </summary>
    public string RequestId { get; init; } = string.Empty;

    /// <summary>
    /// Client-provided unique ID (echoed from request)
    /// </summary>
    public string? UniqueId { get; init; }

    /// <summary>
    /// Total number of fields returned (null for basic, 398 for full)
    /// </summary>
    public int? TotalFieldsReturned { get; init; }
}
