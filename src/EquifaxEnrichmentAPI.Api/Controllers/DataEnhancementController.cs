using Microsoft.AspNetCore.Mvc;
using MediatR;
using EquifaxEnrichmentAPI.Api.DTOs;
using EquifaxEnrichmentAPI.Application.Queries.Lookup;

namespace EquifaxEnrichmentAPI.Api.Controllers;

/// <summary>
/// Data Enhancement API Controller for phone number enrichment.
/// BDD Feature: REST API Endpoint for Phone Number Enrichment
/// BDD File: features/phase1/feature-1.1-rest-api-endpoint.feature
///
/// NOTE: This is Slice 3 implementation with CQRS/MediatR pattern.
/// Controller is thin - all business logic in LookupQueryHandler.
/// Future slices will add: authentication, database lookup, FCRA audit logging, rate limiting.
/// </summary>
[ApiController]
[Route("api/data_enhancement")]
public class DataEnhancementController : ControllerBase
{
    private readonly IMediator _mediator;

    public DataEnhancementController(IMediator mediator)
    {
        _mediator = mediator;
    }
    /// <summary>
    /// Enrich phone number with comprehensive consumer data
    /// </summary>
    /// <remarks>
    /// Returns detailed consumer information associated with a phone number, including personal data,
    /// address history, phone history, and financial indicators.
    ///
    /// ## Request Fields
    ///
    /// **Required:**
    /// - `phone` (string): Phone number to enrich (formats: "555-123-4567", "5551234567", "(555) 123-4567")
    ///
    /// **Optional (Improves Match Confidence):**
    /// - `first_name` (string): Consumer first name
    /// - `last_name` (string): Consumer last name
    /// - `postal_code` (string): 5-digit ZIP code
    /// - `state` (string): 2-letter state code (e.g., "CA", "NY")
    /// - `ip_address` (string): Client IP address for audit logging
    /// - `unique_id` (string): Your tracking identifier (echoed back in response)
    /// - `fields` (string): "basic" (default, ~50 fields) or "full" (398 fields)
    ///
    /// ## Response Fields
    ///
    /// ### Success Response (`response: "success"`)
    ///
    /// **data.consumer_key** (string): Unique Equifax consumer identifier
    ///
    /// **data.personal_info** (object):
    /// - `first_name`: Legal first name
    /// - `middle_name`: Middle name or initial
    /// - `last_name`: Legal last name
    /// - `name_suffix`: Suffix (Jr., Sr., III, etc.)
    /// - `age`: Current age
    /// - `date_of_birth`: Date of birth (YYYY-MM-DD)
    /// - `gender`: Gender (M/F)
    /// - `marital_status`: Marital status
    /// - `education_level`: Highest education level
    /// - `email_addresses`: Array of known email addresses
    ///
    /// **data.addresses** (array): Address history (most recent first)
    /// - `street_address`: Street address
    /// - `unit`: Apartment/unit number
    /// - `city`: City name
    /// - `state`: 2-letter state code
    /// - `postal_code`: 5-digit ZIP code
    /// - `postal_code_plus4`: ZIP+4 code
    /// - `county`: County name
    /// - `country`: Country code
    /// - `address_type`: Type (Current, Previous, Mailing, etc.)
    /// - `residence_type`: Type (Single Family, Apartment, Condo, etc.)
    /// - `move_in_date`: Date moved in (YYYY-MM-DD)
    /// - `move_out_date`: Date moved out (YYYY-MM-DD, null if current)
    /// - `length_of_residence_months`: Months at this address
    /// - `ownership_status`: Own, Rent, Unknown
    ///
    /// **data.phones** (array): Phone number history
    /// - `phone_number`: Phone number (normalized format)
    /// - `phone_type`: Type (Mobile, Landline, VoIP, etc.)
    /// - `carrier`: Telecom carrier name
    /// - `is_current`: Active phone number (true/false)
    /// - `first_seen_date`: First association date
    /// - `last_seen_date`: Last verification date
    /// - `line_type`: Wireless, Wireline, VoIP, etc.
    /// - `do_not_call_registered`: DNC registry status
    ///
    /// **data.financial** (object): Financial indicators
    /// - `estimated_income_range`: Income bracket
    /// - `credit_score_range`: Credit score range (e.g., "700-749")
    /// - `homeowner`: Homeownership status (true/false)
    /// - `estimated_home_value`: Estimated property value
    /// - `mortgage_amount`: Outstanding mortgage balance
    /// - `vehicle_ownership`: Number of vehicles owned
    /// - `occupation_category`: Job category
    /// - `employment_status`: Employment status
    ///
    /// **metadata** (object): Response metadata
    /// - `match_confidence` (0.0-1.0): Confidence score (> 0.7 recommended)
    /// - `match_type`: Type of match performed
    /// - `data_freshness_date`: When data was last updated
    /// - `query_timestamp`: Request timestamp (UTC)
    /// - `response_time_ms`: Response time in milliseconds
    /// - `request_id`: Server-generated UUID
    /// - `unique_id`: Your tracking ID (if provided)
    /// - `total_fields_returned`: Total fields in response
    ///
    /// ### Error Response (`response: "error"`)
    ///
    /// **Common Error Messages:**
    /// - "No consumer record found" - Phone number has no matching data
    /// - "Invalid phone number format" - Phone format not recognized
    /// - "Missing required field: phone" - Phone parameter not provided
    /// - "Invalid API key" - Authentication failed
    /// - "Rate limit exceeded" - Too many requests
    ///
    /// ## Performance
    ///
    /// - Basic request: &lt; 200ms average response time
    /// - Full request: &lt; 300ms average response time
    /// - Match confidence &gt; 0.90 with first_name, last_name, postal_code
    /// - Match confidence 0.60-0.80 with phone only
    ///
    /// ## Best Practices
    ///
    /// 1. Always include first_name, last_name, postal_code for highest match confidence
    /// 2. Use unique_id to correlate requests with your internal systems
    /// 3. Monitor match_confidence scores - investigate scores &lt; 0.7
    /// 4. Handle "no match" responses gracefully - not all numbers have data
    /// 5. Use "basic" fields for faster responses unless you need complete dataset
    /// </remarks>
    /// <param name="request">Lookup request with phone and optional matching fields</param>
    /// <returns>Enrichment data with consumer information, or error response if no match found</returns>
    /// <response code="200">Successfully enriched phone number with consumer data</response>
    /// <response code="400">Invalid request format or missing required fields</response>
    /// <response code="401">Invalid or missing API key</response>
    [HttpPost("lookup")]
    [ProducesResponseType(typeof(LookupResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Lookup([FromBody] LookupRequestDto request)
    {
        // TODO (Future Slice): Validate API key (BDD Scenario 5)
        // TODO (Future Slice): Check rate limiting (BDD Scenario 9)
        // TODO (Future Slice): Log FCRA audit entry (BDD Scenarios 1, 4)

        // ====================================================================
        // MAP API DTO TO APPLICATION QUERY
        // ====================================================================
        var query = new LookupQuery
        {
            Phone = request.Phone,
            Fields = request.Fields,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PostalCode = request.PostalCode,
            State = request.State,
            IpAddress = request.IpAddress,
            UniqueId = request.UniqueId
        };

        // ====================================================================
        // SEND QUERY VIA MEDIATR (CQRS PATTERN)
        // Business logic handled by LookupQueryHandler
        // ====================================================================
        var result = await _mediator.Send(query);

        // ====================================================================
        // MAP APPLICATION RESULT TO API DTO
        // ====================================================================
        var response = new LookupResponseDto
        {
            Response = result.Response,
            Message = result.Message,
            Data = result.Data == null ? null : new EnrichmentDataDto
            {
                ConsumerKey = result.Data.ConsumerKey,
                PersonalInfo = result.Data.PersonalInfo,
                Addresses = result.Data.Addresses,
                Phones = result.Data.Phones,
                Financial = result.Data.Financial
            },
            Metadata = new ResponseMetadataDto
            {
                MatchConfidence = result.Metadata.MatchConfidence,
                MatchType = result.Metadata.MatchType,
                DataFreshnessDate = result.Metadata.DataFreshnessDate,
                QueryTimestamp = result.Metadata.QueryTimestamp,
                ResponseTimeMs = result.Metadata.ResponseTimeMs,
                RequestId = result.Metadata.RequestId,
                UniqueId = result.Metadata.UniqueId,
                TotalFieldsReturned = result.Metadata.TotalFieldsReturned
            }
        };

        return Ok(response);
    }
}
