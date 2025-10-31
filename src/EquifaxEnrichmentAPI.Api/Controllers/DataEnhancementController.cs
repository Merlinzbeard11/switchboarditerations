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
    /// Lookup phone number enrichment data.
    /// BDD Scenario 1: Successful lookup with basic fields
    /// BDD Scenario 2: Full dataset request (398 columns)
    /// BDD Scenario 3: Enhanced matching with optional fields
    /// BDD Scenario 4: No match found
    /// BDD Scenario 10: Phone normalization handling
    ///
    /// Slice 3: Thin controller following CQRS pattern.
    /// All business logic delegated to LookupQueryHandler via MediatR.
    /// </summary>
    /// <param name="request">Lookup request with phone and optional matching fields</param>
    /// <returns>Enrichment data or error response</returns>
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
