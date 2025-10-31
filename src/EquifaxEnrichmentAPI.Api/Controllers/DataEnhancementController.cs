using Microsoft.AspNetCore.Mvc;
using EquifaxEnrichmentAPI.Api.DTOs;
using EquifaxEnrichmentAPI.Domain.ValueObjects;
using System.Diagnostics;

namespace EquifaxEnrichmentAPI.Api.Controllers;

/// <summary>
/// Data Enhancement API Controller for phone number enrichment.
/// BDD Feature: REST API Endpoint for Phone Number Enrichment
/// BDD File: features/phase1/feature-1.1-rest-api-endpoint.feature
///
/// NOTE: This is Slice 2 implementation with MOCK responses.
/// Future slices will add: authentication, database lookup, FCRA audit logging, rate limiting.
/// </summary>
[ApiController]
[Route("api/data_enhancement")]
public class DataEnhancementController : ControllerBase
{
    /// <summary>
    /// Lookup phone number enrichment data.
    /// BDD Scenario 1: Successful lookup with basic fields
    /// BDD Scenario 2: Full dataset request (398 columns)
    /// BDD Scenario 3: Enhanced matching with optional fields
    /// BDD Scenario 4: No match found
    /// BDD Scenario 10: Phone normalization handling
    /// </summary>
    /// <param name="request">Lookup request with phone and optional matching fields</param>
    /// <returns>Enrichment data or error response</returns>
    [HttpPost("lookup")]
    [ProducesResponseType(typeof(LookupResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Lookup([FromBody] LookupRequestDto request)
    {
        var stopwatch = Stopwatch.StartNew();

        // TODO (Future Slice): Validate API key (BDD Scenario 5)
        // TODO (Future Slice): Check rate limiting (BDD Scenario 9)
        // TODO (Future Slice): Log FCRA audit entry (BDD Scenarios 1, 4)

        // ====================================================================
        // PHONE NUMBER NORMALIZATION
        // BDD Scenario 10: Normalize phone before lookup (Lines 246-269)
        // ====================================================================
        var phoneResult = PhoneNumber.Create(request.Phone);
        if (phoneResult.IsFailure)
        {
            // This should have been caught by FluentValidation, but defensive check
            return BadRequest(new ErrorResponseDto
            {
                Response = "error",
                Message = $"Invalid phone number: {phoneResult.Error}",
                Data = null,
                Metadata = null
            });
        }

        var normalizedPhone = phoneResult.Value.NormalizedValue;

        // ====================================================================
        // MOCK DATA LOOKUP
        // BDD Scenario 4: Simulate "no match" for phone 5559999999
        // BDD Scenarios 1-3: Simulate successful match for other phones
        // ====================================================================
        if (normalizedPhone == "5559999999")
        {
            return await HandleNoMatchFound(request, normalizedPhone, stopwatch);
        }

        // ====================================================================
        // SUCCESSFUL MATCH - MOCK RESPONSE
        // BDD Scenarios 1-3: Return mock enrichment data
        // ====================================================================
        return await HandleSuccessfulMatch(request, normalizedPhone, stopwatch);
    }

    /// <summary>
    /// Handles successful match scenario with mock data.
    /// BDD Scenario 1: Basic fields response (~50 fields)
    /// BDD Scenario 2: Full fields response (398 fields)
    /// BDD Scenario 3: Enhanced match confidence with optional fields
    /// </summary>
    private async Task<IActionResult> HandleSuccessfulMatch(
        LookupRequestDto request,
        string normalizedPhone,
        Stopwatch stopwatch)
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
        var response = new LookupResponseDto
        {
            Response = "success",
            Message = matchConfidence > 0.90
                ? "Record found with high confidence"
                : "Record found with moderate confidence",
            Data = new EnrichmentDataDto
            {
                ConsumerKey = $"EQF_{Guid.NewGuid():N}",
                PersonalInfo = new { /* TODO: Expand in future slice */ },
                Addresses = new object[] { /* TODO: Expand in future slice */ },
                Phones = new object[] { /* TODO: Expand in future slice */ },
                Financial = new { /* TODO: Expand in future slice */ }
            },
            Metadata = new ResponseMetadataDto
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

        return Ok(response);
    }

    /// <summary>
    /// Handles no match found scenario.
    /// BDD Scenario 4: Phone not in database (Lines 117-141)
    /// </summary>
    private async Task<IActionResult> HandleNoMatchFound(
        LookupRequestDto request,
        string normalizedPhone,
        Stopwatch stopwatch)
    {
        await Task.CompletedTask; // Simulate async operation

        // BDD Scenario 4: Status 200 with error response (Line 129)
        var response = new LookupResponseDto
        {
            Response = "error",
            Message = "Unable to find record for phone number",
            Data = new EnrichmentDataDto
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
            Metadata = new ResponseMetadataDto
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

        return Ok(response);
    }

    /// <summary>
    /// Calculates match confidence based on available fields.
    /// BDD Scenario 3: > 0.90 with optional fields (Line 109)
    /// </summary>
    private static double CalculateMatchConfidence(LookupRequestDto request)
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
    private static string DetermineMatchType(LookupRequestDto request)
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
