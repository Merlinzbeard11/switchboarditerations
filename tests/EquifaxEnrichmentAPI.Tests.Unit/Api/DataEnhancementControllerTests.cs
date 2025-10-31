using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using EquifaxEnrichmentAPI.Api.Controllers;
using EquifaxEnrichmentAPI.Api.DTOs;

namespace EquifaxEnrichmentAPI.Tests.Unit.Api;

/// <summary>
/// Tests for Feature 1.1: REST API Endpoint - DataEnhancementController
/// BDD Scenarios: features/phase1/feature-1.1-rest-api-endpoint.feature
/// Scenarios 1-4: Basic endpoint functionality with mock responses
/// </summary>
public class DataEnhancementControllerTests
{
    private readonly DataEnhancementController _controller;

    public DataEnhancementControllerTests()
    {
        _controller = new DataEnhancementController();
    }

    // ============================================================================
    // SCENARIO 1: Successful Phone Lookup with Basic Fields (Happy Path)
    // BDD Lines 15-62
    // ============================================================================

    [Fact]
    public async Task Lookup_ValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ApiKey = "157659ac293445df00772760e6114ac4",
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = "insurance_underwriting",
            Fields = "basic"
        };

        // Act
        var result = await _controller.Lookup(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<LookupResponseDto>().Subject;
        response.Response.Should().Be("success");
        response.Message.Should().NotBeNullOrEmpty();
        response.Data.Should().NotBeNull();
        response.Metadata.Should().NotBeNull();
    }

    [Fact]
    public async Task Lookup_ValidRequest_ReturnsMetadataWithCorrectStructure()
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ApiKey = "157659ac293445df00772760e6114ac4",
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = "insurance_underwriting"
        };

        // Act
        var result = await _controller.Lookup(request);

        // Assert
        var okResult = (OkObjectResult)result;
        var response = (LookupResponseDto)okResult.Value!;

        // BDD Scenario 12: Metadata requirements (Lines 285-306)
        response.Metadata.MatchConfidence.Should().BeInRange(0.0, 1.0);
        response.Metadata.MatchType.Should().NotBeNullOrEmpty();
        response.Metadata.DataFreshnessDate.Should().NotBe(default(DateTime));
        response.Metadata.QueryTimestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        response.Metadata.ResponseTimeMs.Should().BeGreaterThanOrEqualTo(0);
        response.Metadata.RequestId.Should().NotBeNullOrEmpty();

        // BDD Scenario 11: Default fields should have null total_fields_returned
        response.Metadata.TotalFieldsReturned.Should().BeNull();
    }

    [Fact]
    public async Task Lookup_UniqueIdProvided_ReturnsUniqueIdInMetadata()
    {
        // Arrange
        var uniqueId = "test-tracking-id-123";
        var request = new LookupRequestDto
        {
            ApiKey = "157659ac293445df00772760e6114ac4",
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = "insurance_underwriting",
            UniqueId = uniqueId
        };

        // Act
        var result = await _controller.Lookup(request);

        // Assert
        var okResult = (OkObjectResult)result;
        var response = (LookupResponseDto)okResult.Value!;

        // BDD Scenario 12: unique_id should be echoed (Line 306)
        response.Metadata.UniqueId.Should().Be(uniqueId);
    }

    // ============================================================================
    // SCENARIO 2: Successful Phone Lookup with Full Fields (398 Columns)
    // BDD Lines 67-85
    // ============================================================================

    [Fact]
    public async Task Lookup_FullFieldsRequested_ReturnsTotalFieldsReturned()
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ApiKey = "157659ac293445df00772760e6114ac4",
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = "insurance_underwriting",
            Fields = "full"
        };

        // Act
        var result = await _controller.Lookup(request);

        // Assert
        var okResult = (OkObjectResult)result;
        var response = (LookupResponseDto)okResult.Value!;

        // BDD Scenario 2: Full dataset should show total_fields_returned = 398 (Line 83)
        response.Metadata.TotalFieldsReturned.Should().Be(398);
    }

    // ============================================================================
    // SCENARIO 3: Phone Lookup with Optional Fields (Improved Match Confidence)
    // BDD Lines 90-112
    // ============================================================================

    [Fact]
    public async Task Lookup_OptionalFieldsProvided_ReturnsHigherMatchConfidence()
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ApiKey = "157659ac293445df00772760e6114ac4",
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            FirstName = "Bob",
            LastName = "Barker",
            PostalCode = "84010",
            State = "UT",
            IpAddress = "192.168.1.100",
            PermissiblePurpose = "insurance_underwriting",
            UniqueId = "b4c9f530-5461-11ef-8f6f-8ffb313ceb02"
        };

        // Act
        var result = await _controller.Lookup(request);

        // Assert
        var okResult = (OkObjectResult)result;
        var response = (LookupResponseDto)okResult.Value!;

        // BDD Scenario 3: match_confidence should be > 0.90 with optional fields (Line 109)
        response.Metadata.MatchConfidence.Should().BeGreaterThan(0.90);
    }

    // ============================================================================
    // SCENARIO 10: Phone Number Normalization Handling
    // BDD Lines 246-269
    // ============================================================================

    [Theory]
    [InlineData("8015551234")]
    [InlineData("(801) 555-1234")]
    [InlineData("801-555-1234")]
    [InlineData("1-801-555-1234")]
    [InlineData("+1 (801) 555-1234")]
    public async Task Lookup_DifferentPhoneFormats_NormalizesAndReturnsSuccess(string phoneFormat)
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ApiKey = "157659ac293445df00772760e6114ac4",
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = phoneFormat,
            PermissiblePurpose = "insurance_underwriting"
        };

        // Act
        var result = await _controller.Lookup(request);

        // Assert
        var okResult = (OkObjectResult)result;
        okResult.StatusCode.Should().Be(200);

        var response = (LookupResponseDto)okResult.Value!;
        response.Response.Should().Be("success");

        // BDD Scenario 10: Phone should be normalized before query (Line 260)
        // For now, just verify success - actual normalization tested in PhoneNumber value object
    }

    // ============================================================================
    // SCENARIO 4: No Match Found (Valid Request, Phone Not in Database)
    // BDD Lines 117-141
    // ============================================================================

    [Fact]
    public async Task Lookup_PhoneNotFound_ReturnsSuccessWithErrorMessage()
    {
        // Arrange - Using a phone number that doesn't exist
        var request = new LookupRequestDto
        {
            ApiKey = "157659ac293445df00772760e6114ac4",
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "5559999999", // Non-existent phone
            PermissiblePurpose = "insurance_underwriting"
        };

        // Act
        var result = await _controller.Lookup(request);

        // Assert
        // BDD Scenario 4: Status should still be 200 (Line 129)
        var okResult = (OkObjectResult)result;
        okResult.StatusCode.Should().Be(200);

        var response = (LookupResponseDto)okResult.Value!;

        // BDD Scenario 4: Response should be "error" (Line 130)
        response.Response.Should().Be("error");

        // BDD Scenario 4: Message should indicate not found (Line 131)
        response.Message.Should().Contain("Unable to find record");

        // BDD Scenario 4: Data should include phone and match info (Lines 133-138)
        response.Data.Should().NotBeNull();
    }

    // ============================================================================
    // Basic Response Structure Tests
    // ============================================================================

    [Fact]
    public async Task Lookup_ValidRequest_ReturnsNonNullData()
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ApiKey = "157659ac293445df00772760e6114ac4",
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = "insurance_underwriting"
        };

        // Act
        var result = await _controller.Lookup(request);

        // Assert
        var okResult = (OkObjectResult)result;
        var response = (LookupResponseDto)okResult.Value!;

        response.Data.Should().NotBeNull();
        response.Data!.ConsumerKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Lookup_ValidRequest_GeneratesUniqueRequestId()
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ApiKey = "157659ac293445df00772760e6114ac4",
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = "insurance_underwriting"
        };

        // Act
        var result1 = await _controller.Lookup(request);
        var result2 = await _controller.Lookup(request);

        // Assert
        var response1 = ((LookupResponseDto)((OkObjectResult)result1).Value!);
        var response2 = ((LookupResponseDto)((OkObjectResult)result2).Value!);

        // Each request should have a unique request_id
        response1.Metadata.RequestId.Should().NotBe(response2.Metadata.RequestId);
    }
}
