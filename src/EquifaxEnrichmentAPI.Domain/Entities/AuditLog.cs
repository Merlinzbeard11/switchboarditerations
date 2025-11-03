namespace EquifaxEnrichmentAPI.Domain.Entities;

/// <summary>
/// Audit log entity for FCRA compliance tracking.
/// BDD Feature: Audit Log Database Persistence (feature-2.4-audit-log-persistence.feature)
///
/// COMPLIANCE NOTES:
/// - Tracks every enrichment API request for FCRA § 607(b) compliance
/// - Phone numbers stored as SHA-256 hashes (privacy protection)
/// - Immutable records (append-only, no updates/deletes)
/// - Permissible purpose validation required for all queries
/// - Supports consumer access requests (FCRA § 609)
///
/// PERFORMANCE NOTES:
/// - Uses UUIDv7 for optimal index performance (configured in EF Core)
/// - Partitioned by month for scalability (326M+ records expected)
/// - TIMESTAMPTZ for timezone-aware audit trail (configured in EF Core)
///
/// GOTCHAS ADDRESSED:
/// - Gotcha #1: No FCRA-mandated retention period (business decision: 24 months)
/// - Gotcha #4: Table partitioning required for performance at scale
/// - Gotcha #5: TIMESTAMPTZ mandatory for accurate audit timeline
/// - Gotcha #6: UUIDv7 reduces index fragmentation vs UUID v4
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Unique audit log identifier (primary key).
    /// PERFORMANCE: Uses UUIDv7 (PostgreSQL 18+) for sequential ordering and reduced fragmentation.
    /// Gotcha #6: UUIDv7 provides time-based ordering unlike UUID v4 (random)
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// When the API request occurred (UTC, stored as TIMESTAMPTZ).
    /// GOTCHA #5: Must use TIMESTAMPTZ to preserve timezone information for compliance audits.
    /// Gotcha #7: Use database NOW() to avoid clock skew between app server and database.
    /// </summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>
    /// Buyer identifier who made the request.
    /// References Buyer.Id for buyer account tracking.
    /// </summary>
    public Guid BuyerId { get; private set; }

    /// <summary>
    /// SHA-256 hash of the phone number queried.
    /// PRIVACY: Phone numbers are NEVER stored in plaintext (FCRA privacy requirement).
    /// BDD Scenario: Phone hashing for privacy protection
    /// </summary>
    public string PhoneHash { get; private set; } = string.Empty;

    /// <summary>
    /// FCRA-compliant permissible purpose for the query.
    /// Valid values: insurance_underwriting, credit_extension, employment_screening,
    /// tenant_screening, legitimate_business_need
    /// </summary>
    public string PermissiblePurpose { get; private set; } = string.Empty;

    /// <summary>
    /// IP address of the API client (optional, for fraud detection).
    /// Supports IPv4 (max 15 chars) and IPv6 (max 45 chars).
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// Response type: "success", "no_match", "error"
    /// Tracks query outcome for compliance reporting.
    /// </summary>
    public string Response { get; private set; } = string.Empty;

    /// <summary>
    /// HTTP status code returned to client.
    /// Examples: 200 (success), 404 (no match), 400 (bad request), 401 (unauthorized)
    /// </summary>
    public int StatusCode { get; private set; }

    /// <summary>
    /// When the audit log was created in the database (UTC, stored as TIMESTAMPTZ).
    /// Gotcha #7: Set by database NOW() to avoid clock skew issues.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    // Private parameterless constructor for EF Core
    private AuditLog() { }

    /// <summary>
    /// Factory method to create a new AuditLog entry.
    /// BDD Scenario: Log all enrichment requests with full context
    /// </summary>
    /// <param name="buyerId">Buyer who made the request</param>
    /// <param name="phoneHash">SHA-256 hash of the phone number</param>
    /// <param name="permissiblePurpose">FCRA-compliant purpose</param>
    /// <param name="response">Response type (success, no_match, error)</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="ipAddress">Client IP address (optional)</param>
    /// <param name="timestamp">Request timestamp (defaults to UtcNow, but database will override with NOW())</param>
    public static AuditLog Create(
        Guid buyerId,
        string phoneHash,
        string permissiblePurpose,
        string response,
        int statusCode,
        string? ipAddress = null,
        DateTime? timestamp = null)
    {
        if (buyerId == Guid.Empty)
            throw new ArgumentException("Buyer ID cannot be empty", nameof(buyerId));

        if (string.IsNullOrWhiteSpace(phoneHash))
            throw new ArgumentException("Phone hash cannot be empty", nameof(phoneHash));

        if (phoneHash.Length != 64)
            throw new ArgumentException("Phone hash must be 64 characters (SHA-256)", nameof(phoneHash));

        if (string.IsNullOrWhiteSpace(permissiblePurpose))
            throw new ArgumentException("Permissible purpose cannot be empty", nameof(permissiblePurpose));

        if (string.IsNullOrWhiteSpace(response))
            throw new ArgumentException("Response cannot be empty", nameof(response));

        if (statusCode < 100 || statusCode >= 600)
            throw new ArgumentException("Status code must be valid HTTP status (100-599)", nameof(statusCode));

        // NOTE: Id will be generated as UUIDv7 by database (configured in EF Core)
        // NOTE: Timestamp and CreatedAt will be set by database NOW() (configured in EF Core)
        return new AuditLog
        {
            Id = Guid.NewGuid(), // Placeholder - database will override with UUIDv7
            Timestamp = timestamp ?? DateTime.UtcNow, // Placeholder - database will override with NOW()
            BuyerId = buyerId,
            PhoneHash = phoneHash,
            PermissiblePurpose = permissiblePurpose,
            IpAddress = ipAddress,
            Response = response,
            StatusCode = statusCode,
            CreatedAt = DateTime.UtcNow // Placeholder - database will override with NOW()
        };
    }
}
