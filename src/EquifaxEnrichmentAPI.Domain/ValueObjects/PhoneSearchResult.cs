namespace EquifaxEnrichmentAPI.Domain.ValueObjects;

/// <summary>
/// Value object representing the result of a multi-column phone search.
/// Feature 1.3 Slice 4: Confidence Scoring Based on Matched Column
/// BDD File: features/phase1/feature-1.3-database-query-multi-phone.feature
///
/// Encapsulates:
/// - The matched entity (if found)
/// - Which column matched (Phone1-Phone10 or NormalizedPhone)
/// - Calculated confidence based on matched column
///
/// BDD Scenario 14: Confidence formula = 100 - ((column_index - 1) * 5)
/// - Phone1 (index 1) = 1.00 (100%)
/// - Phone2 (index 2) = 0.95 (95%)
/// - ...
/// - Phone10 (index 10) = 0.55 (55%)
/// - NormalizedPhone (legacy) = uses entity's stored confidence
/// </summary>
public class PhoneSearchResult
{
    /// <summary>
    /// The matched consumer enrichment entity (null if no match)
    /// </summary>
    public Entities.ConsumerEnrichment? Entity { get; private set; }

    /// <summary>
    /// Which column matched (1-10 for Phone1-Phone10, null for NormalizedPhone or no match)
    /// BDD Scenario 8: System determines matched column accurately
    /// </summary>
    public int? MatchedColumn { get; private set; }

    /// <summary>
    /// Calculated confidence based on matched column
    /// BDD Scenario 14: Formula-based confidence scoring
    /// </summary>
    public double Confidence { get; private set; }

    /// <summary>
    /// Name of the matched column (e.g., "Phone1", "Phone2", "NormalizedPhone")
    /// Used for logging per BDD Scenario 1 (Line 26): "Phone match found: phone_1, Confidence: 100%"
    /// </summary>
    public string? MatchedColumnName { get; private set; }

    private PhoneSearchResult()
    {
        // Private constructor for factory methods
    }

    /// <summary>
    /// Creates a result for a successful match in one of the Phone1-Phone10 columns.
    /// Calculates confidence using formula: 100 - ((column_index - 1) * 5) / 100
    /// </summary>
    public static PhoneSearchResult CreateMatch(
        Entities.ConsumerEnrichment entity,
        int matchedColumnIndex)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (matchedColumnIndex < 1 || matchedColumnIndex > 10)
            throw new ArgumentOutOfRangeException(
                nameof(matchedColumnIndex),
                "Matched column index must be between 1 and 10");

        // BDD Scenario 14: Confidence formula
        var confidencePercentage = 100 - ((matchedColumnIndex - 1) * 5);
        var confidence = confidencePercentage / 100.0;

        return new PhoneSearchResult
        {
            Entity = entity,
            MatchedColumn = matchedColumnIndex,
            Confidence = confidence,
            MatchedColumnName = $"Phone{matchedColumnIndex}"
        };
    }

    /// <summary>
    /// Creates a result for a match in the legacy NormalizedPhone column.
    /// Uses entity's stored MatchConfidence (backward compatibility during migration).
    /// </summary>
    public static PhoneSearchResult CreateLegacyMatch(Entities.ConsumerEnrichment entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return new PhoneSearchResult
        {
            Entity = entity,
            MatchedColumn = null, // Legacy column has no index
            Confidence = entity.match_confidence,
            MatchedColumnName = "NormalizedPhone"
        };
    }

    /// <summary>
    /// Creates a result for no match found.
    /// BDD Scenario 3: No match returns null entity with 0.0 confidence
    /// </summary>
    public static PhoneSearchResult CreateNoMatch()
    {
        return new PhoneSearchResult
        {
            Entity = null,
            MatchedColumn = null,
            Confidence = 0.0,
            MatchedColumnName = null
        };
    }

    /// <summary>
    /// Indicates whether a match was found.
    /// </summary>
    public bool IsMatch => Entity != null;
}
