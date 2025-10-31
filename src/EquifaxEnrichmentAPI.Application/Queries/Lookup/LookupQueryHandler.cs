using MediatR;
using EquifaxEnrichmentAPI.Domain.ValueObjects;
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
/// NOTE: This is Slice 3 implementation with MOCK responses.
/// Future slices will add: database lookup, FCRA audit logging.
/// </summary>
public class LookupQueryHandler : IRequestHandler<LookupQuery, LookupResult>
{
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
        // MOCK DATA LOOKUP
        // BDD Scenario 4: Simulate "no match" for phone 5559999999
        // BDD Scenarios 1-3: Simulate successful match for other phones
        // ====================================================================
        if (normalizedPhone == "5559999999")
        {
            return await HandleNoMatchFoundAsync(request, normalizedPhone, stopwatch, cancellationToken);
        }

        // ====================================================================
        // SUCCESSFUL MATCH - MOCK RESPONSE
        // BDD Scenarios 1-3: Return mock enrichment data
        // ====================================================================
        return await HandleSuccessfulMatchAsync(request, normalizedPhone, stopwatch, cancellationToken);
    }

    /// <summary>
    /// Handles successful match scenario with mock data.
    /// BDD Scenario 1: Basic fields response (~50 fields)
    /// BDD Scenario 2: Full fields response (398 fields)
    /// BDD Scenario 3: Enhanced match confidence with optional fields
    /// </summary>
    private async Task<LookupResult> HandleSuccessfulMatchAsync(
        LookupQuery request,
        string normalizedPhone,
        Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask; // Simulate async operation

        // ====================================================================
        // CALCULATE MATCH CONFIDENCE
        // BDD Scenario 3: Higher confidence with optional fields (Lines 90-112)
        // ====================================================================
        var matchConfidence = CalculateMatchConfidence(request);

        // ====================================================================
        // DETERMINE MATCH TYPE
        // ====================================================================
        var matchType = DetermineMatchType(request);

        // ====================================================================
        // BUILD RESPONSE
        // ====================================================================
        return new LookupResult
        {
            Response = "success",
            Message = matchConfidence > 0.90
                ? "Record found with high confidence"
                : "Record found with moderate confidence",
            Data = new EnrichmentData
            {
                ConsumerKey = $"EQF_{Guid.NewGuid():N}",
                PersonalInfo = new { /* TODO: Expand in future slice */ },
                Addresses = new object[] { /* TODO: Expand in future slice */ },
                Phones = new object[] { /* TODO: Expand in future slice */ },
                Financial = new { /* TODO: Expand in future slice */ }
            },
            Metadata = new ResponseMetadata
            {
                MatchConfidence = matchConfidence,
                MatchType = matchType,
                DataFreshnessDate = DateTime.UtcNow.AddDays(-7), // Mock: 7 days old
                QueryTimestamp = DateTime.UtcNow,
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                RequestId = Guid.NewGuid().ToString(),
                UniqueId = request.UniqueId, // Echo client's tracking ID
                TotalFieldsReturned = request.Fields?.ToLower() == "full" ? 398 : null
            }
        };
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

    /// <summary>
    /// Calculates match confidence based on available fields.
    /// BDD Scenario 3: > 0.90 with optional fields (Line 109)
    /// </summary>
    private static double CalculateMatchConfidence(LookupQuery request)
    {
        // Base confidence for phone-only match
        var confidence = 0.75;

        // Increase confidence with optional fields
        if (!string.IsNullOrWhiteSpace(request.FirstName)) confidence += 0.05;
        if (!string.IsNullOrWhiteSpace(request.LastName)) confidence += 0.05;
        if (!string.IsNullOrWhiteSpace(request.PostalCode)) confidence += 0.05;
        if (!string.IsNullOrWhiteSpace(request.State)) confidence += 0.03;
        if (!string.IsNullOrWhiteSpace(request.IpAddress)) confidence += 0.02;

        return Math.Min(confidence, 1.0); // Cap at 1.0
    }

    /// <summary>
    /// Determines match type based on available fields.
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
}
