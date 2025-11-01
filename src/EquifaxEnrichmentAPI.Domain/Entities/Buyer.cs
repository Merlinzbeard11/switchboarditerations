namespace EquifaxEnrichmentAPI.Domain.Entities;

/// <summary>
/// Buyer entity representing API clients with authentication credentials.
/// BDD Feature: API Key Authentication (feature-2.1-api-key-authentication.feature)
///
/// SECURITY NOTES:
/// - API keys are NEVER stored in plaintext (only SHA-256 hashes)
/// - Hashes compared using constant-time operations (timing-attack resistant)
/// - BDD Scenario 4: Timing attack prevention is CRITICAL (CVE-2025-59425)
/// - BDD Scenario 8: Only hashes stored, plaintext returned ONCE on generation
/// </summary>
public class Buyer
{
    /// <summary>
    /// Unique buyer identifier (primary key).
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// SHA-256 hash of the API key (Base64-encoded).
    /// SECURITY: Plaintext API keys are NEVER stored in database.
    /// BDD Scenario 8: Store API key as SHA-256 hash in database
    /// </summary>
    public string ApiKeyHash { get; private set; } = string.Empty;

    /// <summary>
    /// Indicates whether the buyer account is active.
    /// BDD Scenario 5: Inactive buyers cannot authenticate (returns 401)
    /// BDD Scenario 14: Zero Trust - verify on EVERY request (no caching)
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Buyer company name or identifier.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// When the buyer account was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// When the buyer account was last updated (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    // Private parameterless constructor for EF Core
    private Buyer() { }

    /// <summary>
    /// Factory method to create a new Buyer with API key hash.
    /// BDD Scenario 1: Valid API key authentication
    /// BDD Scenario 8: Store SHA-256 hash, not plaintext
    /// </summary>
    /// <param name="apiKeyHash">SHA-256 hash of the API key (Base64-encoded)</param>
    /// <param name="name">Buyer name/company</param>
    /// <param name="isActive">Whether account is active (defaults to true)</param>
    public static Buyer Create(string apiKeyHash, string name, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(apiKeyHash))
            throw new ArgumentException("API key hash cannot be empty", nameof(apiKeyHash));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Buyer name cannot be empty", nameof(name));

        var now = DateTime.UtcNow;

        return new Buyer
        {
            Id = Guid.NewGuid(),
            ApiKeyHash = apiKeyHash,
            Name = name,
            IsActive = isActive,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Deactivates the buyer account.
    /// BDD Scenario 21: Key revocation (soft delete with audit trail)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the buyer account.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the API key hash (for key rotation).
    /// BDD Scenario 9: Automated API key rotation (90-day cycle)
    /// </summary>
    /// <param name="newApiKeyHash">New SHA-256 hash of rotated API key</param>
    public void RotateApiKey(string newApiKeyHash)
    {
        if (string.IsNullOrWhiteSpace(newApiKeyHash))
            throw new ArgumentException("New API key hash cannot be empty", nameof(newApiKeyHash));

        ApiKeyHash = newApiKeyHash;
        UpdatedAt = DateTime.UtcNow;
    }
}
