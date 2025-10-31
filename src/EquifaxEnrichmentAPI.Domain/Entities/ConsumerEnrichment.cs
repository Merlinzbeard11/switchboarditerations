using EquifaxEnrichmentAPI.Domain.ValueObjects;

namespace EquifaxEnrichmentAPI.Domain.Entities;

/// <summary>
/// Aggregate root representing enriched consumer data from Equifax.
/// BDD Feature: REST API Endpoint for Phone Number Enrichment
/// BDD File: features/phase1/feature-1.1-rest-api-endpoint.feature
///
/// This is a rich domain model following DDD principles.
/// Encapsulates business rules and invariants for consumer enrichment data.
/// </summary>
public class ConsumerEnrichment
{
    /// <summary>
    /// Unique identifier (primary key)
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Equifax consumer key identifier
    /// </summary>
    public string ConsumerKey { get; private set; } = string.Empty;

    /// <summary>
    /// Normalized phone number (10 digits, no formatting)
    /// BDD Scenario 10: Phone normalization handling
    /// </summary>
    public string NormalizedPhone { get; private set; } = string.Empty;

    /// <summary>
    /// Match confidence score (0.0 to 1.0)
    /// BDD Scenario 3: Higher confidence with optional fields
    /// </summary>
    public double MatchConfidence { get; private set; }

    /// <summary>
    /// Type of match performed
    /// Examples: "phone_only", "phone_with_name", "phone_with_name_and_address"
    /// </summary>
    public string MatchType { get; private set; } = string.Empty;

    /// <summary>
    /// Date when the enrichment data was last updated
    /// BDD Scenario 12: Data freshness tracking
    /// </summary>
    public DateTime DataFreshnessDate { get; private set; }

    /// <summary>
    /// Personal information (JSON serialized for MVP)
    /// TODO: Expand to proper value object in future slice
    /// </summary>
    public string PersonalInfoJson { get; private set; } = "{}";

    /// <summary>
    /// Address history (JSON serialized for MVP)
    /// TODO: Expand to proper value objects in future slice
    /// </summary>
    public string AddressesJson { get; private set; } = "[]";

    /// <summary>
    /// Phone number history (JSON serialized for MVP)
    /// TODO: Expand to proper value objects in future slice
    /// </summary>
    public string PhonesJson { get; private set; } = "[]";

    /// <summary>
    /// Financial information (JSON serialized for MVP)
    /// TODO: Expand to proper value object in future slice
    /// </summary>
    public string FinancialJson { get; private set; } = "{}";

    /// <summary>
    /// When this record was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// When this record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    // Private parameterless constructor for EF Core
    private ConsumerEnrichment()
    {
    }

    /// <summary>
    /// Factory method to create new enrichment record.
    /// Encapsulates creation logic and enforces invariants.
    /// </summary>
    public static ConsumerEnrichment Create(
        PhoneNumber phone,
        string consumerKey,
        double matchConfidence,
        string matchType,
        DateTime dataFreshnessDate,
        string? personalInfoJson = null,
        string? addressesJson = null,
        string? phonesJson = null,
        string? financialJson = null)
    {
        // Validate invariants
        if (string.IsNullOrWhiteSpace(consumerKey))
            throw new ArgumentException("Consumer key cannot be empty", nameof(consumerKey));

        if (matchConfidence < 0.0 || matchConfidence > 1.0)
            throw new ArgumentOutOfRangeException(nameof(matchConfidence), "Match confidence must be between 0.0 and 1.0");

        if (string.IsNullOrWhiteSpace(matchType))
            throw new ArgumentException("Match type cannot be empty", nameof(matchType));

        return new ConsumerEnrichment
        {
            Id = Guid.NewGuid(),
            ConsumerKey = consumerKey,
            NormalizedPhone = phone.NormalizedValue,
            MatchConfidence = matchConfidence,
            MatchType = matchType,
            DataFreshnessDate = dataFreshnessDate,
            PersonalInfoJson = personalInfoJson ?? "{}",
            AddressesJson = addressesJson ?? "[]",
            PhonesJson = phonesJson ?? "[]",
            FinancialJson = financialJson ?? "{}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates match metadata.
    /// Business rule: Can only update if new confidence is higher.
    /// </summary>
    public void UpdateMatchMetadata(double newConfidence, string newMatchType)
    {
        if (newConfidence < 0.0 || newConfidence > 1.0)
            throw new ArgumentOutOfRangeException(nameof(newConfidence), "Match confidence must be between 0.0 and 1.0");

        // Business rule: Only update if confidence improved
        if (newConfidence > MatchConfidence)
        {
            MatchConfidence = newConfidence;
            MatchType = newMatchType;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Updates data freshness date.
    /// Business rule: Cannot set freshness to future date.
    /// </summary>
    public void UpdateDataFreshness(DateTime newFreshnessDate)
    {
        if (newFreshnessDate > DateTime.UtcNow)
            throw new ArgumentException("Data freshness date cannot be in the future", nameof(newFreshnessDate));

        DataFreshnessDate = newFreshnessDate;
        UpdatedAt = DateTime.UtcNow;
    }
}
