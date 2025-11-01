using MediatR;
using EquifaxEnrichmentAPI.Domain.ValueObjects;
using EquifaxEnrichmentAPI.Domain.Repositories;
using System.Diagnostics;

namespace EquifaxEnrichmentAPI.Application.Queries.Lookup;

/// <summary>
/// CQRS Query Handler for phone number enrichment lookup.
/// Implements business logic for enrichment lookup following Clean Architecture.
///
/// BDD Scenarios Implemented:
/// - Scenario 1: Successful lookup with basic fields
/// - Scenario 2: Full dataset request (398 columns)
/// - Scenario 3: Enhanced matching with optional fields
/// - Scenario 4: No match found
/// - Scenario 10: Phone normalization handling
/// - Scenario 12: Metadata requirements
///
/// Slice 4: Integrated with repository for actual database lookups.
/// </summary>
public class LookupQueryHandler : IRequestHandler<LookupQuery, LookupResult>
{
    private readonly IEnrichmentRepository _repository;

    public LookupQueryHandler(IEnrichmentRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<LookupResult> Handle(LookupQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        // ====================================================================
        // PHONE NUMBER NORMALIZATION
        // BDD Scenario 10: Normalize phone before lookup (Lines 246-269)
        // ====================================================================
        var phoneResult = PhoneNumber.Create(request.Phone);
        if (phoneResult.IsFailure)
        {
            // This should have been caught by validation, but defensive check
            return CreateErrorResult(
                $"Invalid phone number: {phoneResult.Error}",
                request.UniqueId,
                stopwatch);
        }

        var normalizedPhone = phoneResult.Value.NormalizedValue;

        // ====================================================================
        // DATABASE LOOKUP
        // Feature 1.3 Slice 4: Repository returns PhoneSearchResult with column-based confidence
        // BDD Scenario 4: No match found (PhoneSearchResult.IsMatch = false)
        // BDD Scenarios 1-3: Successful match (PhoneSearchResult.IsMatch = true)
        // ====================================================================
        var searchResult = await _repository.FindByPhoneAsync(normalizedPhone, cancellationToken);

        if (!searchResult.IsMatch)
        {
            return await HandleNoMatchFoundAsync(request, normalizedPhone, stopwatch, cancellationToken);
        }

        // ====================================================================
        // SUCCESSFUL MATCH - MAP ENTITY TO RESULT
        // BDD Scenarios 1-3: Return enrichment data from database
        // Use column-based confidence from repository (no optional field boost needed)
        // ====================================================================
        return await MapEntityToResultAsync(searchResult, request, stopwatch, cancellationToken);
    }

    /// <summary>
    /// Maps database search result to application result.
    /// Feature 1.3 Slice 4: Uses column-based confidence from PhoneSearchResult
    /// BDD Scenario 1: Basic fields response (~50 fields)
    /// BDD Scenario 2: Full fields response (398 fields)
    /// BDD Scenario 3: Enhances base match with optional fields from query
    /// BDD Scenario 14: Confidence from matched column (Phone1=1.00, Phone2=0.95, etc.)
    /// </summary>
    private async Task<LookupResult> MapEntityToResultAsync(
        Domain.ValueObjects.PhoneSearchResult searchResult,
        LookupQuery request,
        Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask; // Async signature for consistency

        var enrichment = searchResult.Entity!; // Entity guaranteed non-null when IsMatch=true

        // ====================================================================
        // USE COLUMN-BASED CONFIDENCE FROM REPOSITORY
        // Feature 1.3 Slice 4: Confidence calculated from matched column
        // BDD Scenario 14: Phone1=1.00, Phone2=0.95, ..., Phone10=0.55
        // Optional fields may still enhance confidence slightly
        // ====================================================================
        var baseConfidence = searchResult.Confidence; // From matched column
        var enhancedConfidence = CalculateEnhancedConfidence(baseConfidence, request);
        var enhancedMatchType = DetermineMatchType(request);

        // ====================================================================
        // MAP ENTITY TO RESULT
        // Use data from database entity with column-based confidence
        // ====================================================================
        return new LookupResult
        {
            Response = "success",
            Message = enhancedConfidence > 0.90
                ? "Record found with high confidence"
                : "Record found with moderate confidence",
            Data = new EnrichmentData
            {
                ConsumerKey = enrichment.ConsumerKey,
                PersonalInfo = System.Text.Json.JsonSerializer.Deserialize<object>(enrichment.PersonalInfoJson),
                Addresses = System.Text.Json.JsonSerializer.Deserialize<object[]>(enrichment.AddressesJson),
                Phones = System.Text.Json.JsonSerializer.Deserialize<object[]>(enrichment.PhonesJson),
                Financial = System.Text.Json.JsonSerializer.Deserialize<object>(enrichment.FinancialJson)
            },
            Metadata = new ResponseMetadata
            {
                MatchConfidence = enhancedConfidence,
                MatchType = enhancedMatchType,
                DataFreshnessDate = enrichment.DataFreshnessDate,
                QueryTimestamp = DateTime.UtcNow,
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                RequestId = Guid.NewGuid().ToString(),
                UniqueId = request.UniqueId, // Echo client's tracking ID
                TotalFieldsReturned = request.Fields?.ToLower() == "full" ? 398 : null
            }
        };
    }

    /// <summary>
    /// Enhances base database confidence with optional field boost.
    /// BDD Scenario 3: Optional fields improve match confidence (Lines 90-112)
    /// </summary>
    private static double CalculateEnhancedConfidence(double baseConfidence, LookupQuery request)
    {
        var confidence = baseConfidence;

        // Name fields boost confidence
        if (!string.IsNullOrWhiteSpace(request.FirstName))
            confidence += 0.10;
        if (!string.IsNullOrWhiteSpace(request.LastName))
            confidence += 0.05;

        // Address fields boost confidence
        if (!string.IsNullOrWhiteSpace(request.PostalCode))
            confidence += 0.07;
        if (!string.IsNullOrWhiteSpace(request.State))
            confidence += 0.03;

        // IP address for fraud detection
        if (!string.IsNullOrWhiteSpace(request.IpAddress))
            confidence += 0.02;

        // Cap at 1.0
        return Math.Min(confidence, 1.0);
    }

    /// <summary>
    /// Determines match type based on optional fields provided.
    /// BDD Scenario 3: Match type indicates which fields were used (Lines 110-112)
    /// </summary>
    private static string DetermineMatchType(LookupQuery request)
    {
        var hasName = !string.IsNullOrWhiteSpace(request.FirstName) ||
                      !string.IsNullOrWhiteSpace(request.LastName);
        var hasAddress = !string.IsNullOrWhiteSpace(request.PostalCode) ||
                         !string.IsNullOrWhiteSpace(request.State);

        if (hasName && hasAddress)
            return "phone_with_name_and_address";
        if (hasName)
            return "phone_with_name";
        if (hasAddress)
            return "phone_with_address";

        return "phone_only";
    }

    /// <summary>
    /// Handles no match found scenario.
    /// BDD Scenario 4: Phone not in database (Lines 117-141)
    /// </summary>
    private async Task<LookupResult> HandleNoMatchFoundAsync(
        LookupQuery request,
        string normalizedPhone,
        Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask; // Simulate async operation

        // BDD Scenario 4: Status should still be 200 with error response (Line 129)
        return new LookupResult
        {
            Response = "error",
            Message = "Unable to find record for phone number",
            Data = new EnrichmentData
            {
                ConsumerKey = normalizedPhone,
                PersonalInfo = new
                {
                    phone = normalizedPhone,
                    match_attempted = true,
                    match_confidence = 0.0
                },
                Addresses = null,
                Phones = null,
                Financial = null
            },
            Metadata = new ResponseMetadata
            {
                MatchConfidence = 0.0,
                MatchType = "no_match",
                DataFreshnessDate = DateTime.UtcNow,
                QueryTimestamp = DateTime.UtcNow,
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                RequestId = Guid.NewGuid().ToString(),
                UniqueId = request.UniqueId,
                TotalFieldsReturned = null
            }
        };
    }

    /// <summary>
    /// Creates error result for validation failures.
    /// </summary>
    private static LookupResult CreateErrorResult(string message, string? uniqueId, Stopwatch stopwatch)
    {
        return new LookupResult
        {
            Response = "error",
            Message = message,
            Data = null,
            Metadata = new ResponseMetadata
            {
                MatchConfidence = 0.0,
                MatchType = "validation_error",
                DataFreshnessDate = DateTime.UtcNow,
                QueryTimestamp = DateTime.UtcNow,
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                RequestId = Guid.NewGuid().ToString(),
                UniqueId = uniqueId,
                TotalFieldsReturned = null
            }
        };
    }
}
