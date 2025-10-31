using Xunit;
using FluentAssertions;
using EquifaxEnrichmentAPI.Application.Queries.Lookup;

namespace EquifaxEnrichmentAPI.Tests.Unit.Application;

/// <summary>
/// Tests for Feature 1.1 Slice 3: CQRS Query Handler
/// BDD Scenarios: features/phase1/feature-1.1-rest-api-endpoint.feature
/// Tests business logic moved to Application layer following Clean Architecture
/// </summary>
public class LookupQueryHandlerTests
{
    private readonly LookupQueryHandler _handler;

    public LookupQueryHandlerTests()
    {
        _handler = new LookupQueryHandler();
    }

    // ============================================================================
    // SCENARIO 1: Successful Phone Lookup with Basic Fields (Happy Path)
    // BDD Lines 15-62
    // ============================================================================

    [Fact]
    public async Task Handle_ValidQuery_ReturnsSuccessResult()
    {
        // Arrange
        var query = new LookupQuery
        {
            Phone = "8015551234",
            Fields = "basic"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Response.Should().Be("success");
        result.Message.Should().NotBeNullOrEmpty();
        result.Data.Should().NotBeNull();
        result.Metadata.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsMetadataWithCorrectStructure()
    {
        // Arrange
        var query = new LookupQuery
        {
            Phone = "8015551234"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert - BDD Scenario 12: Metadata requirements (Lines 285-306)
        result.Metadata.MatchConfidence.Should().BeInRange(0.0, 1.0);
        result.Metadata.MatchType.Should().NotBeNullOrEmpty();
        result.Metadata.DataFreshnessDate.Should().NotBe(default(DateTime));
        result.Metadata.QueryTimestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Metadata.ResponseTimeMs.Should().BeGreaterThanOrEqualTo(0);
        result.Metadata.RequestId.Should().NotBeNullOrEmpty();

        // BDD Scenario 11: Default fields should have null total_fields_returned
        result.Metadata.TotalFieldsReturned.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UniqueIdProvided_ReturnsUniqueIdInMetadata()
    {
        // Arrange
        var uniqueId = "test-tracking-id-123";
        var query = new LookupQuery
        {
            Phone = "8015551234",
            UniqueId = uniqueId
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert - BDD Scenario 12: unique_id should be echoed (Line 306)
        result.Metadata.UniqueId.Should().Be(uniqueId);
    }

    // ============================================================================
    // SCENARIO 2: Successful Phone Lookup with Full Fields (398 Columns)
    // BDD Lines 67-85
    // ============================================================================

    [Fact]
    public async Task Handle_FullFieldsRequested_ReturnsTotalFieldsReturned()
    {
        // Arrange
        var query = new LookupQuery
        {
            Phone = "8015551234",
            Fields = "full"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert - BDD Scenario 2: Full dataset should show total_fields_returned = 398 (Line 83)
        result.Metadata.TotalFieldsReturned.Should().Be(398);
    }

    // ============================================================================
    // SCENARIO 3: Phone Lookup with Optional Fields (Improved Match Confidence)
    // BDD Lines 90-112
    // ============================================================================

    [Fact]
    public async Task Handle_OptionalFieldsProvided_ReturnsHigherMatchConfidence()
    {
        // Arrange
        var query = new LookupQuery
        {
            Phone = "8015551234",
            FirstName = "Bob",
            LastName = "Barker",
            PostalCode = "84010",
            State = "UT",
            IpAddress = "192.168.1.100",
            UniqueId = "b4c9f530-5461-11ef-8f6f-8ffb313ceb02"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert - BDD Scenario 3: match_confidence should be > 0.90 with optional fields (Line 109)
        result.Metadata.MatchConfidence.Should().BeGreaterThan(0.90);
    }

    [Fact]
    public async Task Handle_OptionalFieldsProvided_ReturnsCorrectMatchType()
    {
        // Arrange
        var query = new LookupQuery
        {
            Phone = "8015551234",
            FirstName = "Bob",
            LastName = "Barker",
            PostalCode = "84010",
            State = "UT"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Metadata.MatchType.Should().Be("phone_with_name_and_address");
    }

    [Fact]
    public async Task Handle_OnlyNameProvided_ReturnsPhoneWithNameMatchType()
    {
        // Arrange
        var query = new LookupQuery
        {
            Phone = "8015551234",
            FirstName = "Bob"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Metadata.MatchType.Should().Be("phone_with_name");
    }

    [Fact]
    public async Task Handle_OnlyAddressProvided_ReturnsPhoneWithAddressMatchType()
    {
        // Arrange
        var query = new LookupQuery
        {
            Phone = "8015551234",
            PostalCode = "84010"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Metadata.MatchType.Should().Be("phone_with_address");
    }

    [Fact]
    public async Task Handle_NoOptionalFields_ReturnsPhoneOnlyMatchType()
    {
        // Arrange
        var query = new LookupQuery
        {
            Phone = "8015551234"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Metadata.MatchType.Should().Be("phone_only");
        result.Metadata.MatchConfidence.Should().Be(0.75); // Base confidence
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
    public async Task Handle_DifferentPhoneFormats_NormalizesAndReturnsSuccess(string phoneFormat)
    {
        // Arrange
        var query = new LookupQuery
        {
            Phone = phoneFormat
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Response.Should().Be("success");

        // BDD Scenario 10: Phone should be normalized before query (Line 260)
        // Actual normalization tested in PhoneNumber value object tests
    }

    // ============================================================================
    // SCENARIO 4: No Match Found (Valid Request, Phone Not in Database)
    // BDD Lines 117-141
    // ============================================================================

    [Fact]
    public async Task Handle_PhoneNotFound_ReturnsErrorResponse()
    {
        // Arrange - Using mock phone number that doesn't exist
        var query = new LookupQuery
        {
            Phone = "5559999999" // Non-existent phone
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert - BDD Scenario 4: Response should be "error" (Line 130)
        result.Response.Should().Be("error");

        // BDD Scenario 4: Message should indicate not found (Line 131)
        result.Message.Should().Contain("Unable to find record");

        // BDD Scenario 4: Data should include phone and match info (Lines 133-138)
        result.Data.Should().NotBeNull();
        result.Metadata.MatchConfidence.Should().Be(0.0);
        result.Metadata.MatchType.Should().Be("no_match");
    }

    // ============================================================================
    // INVALID PHONE HANDLING
    // ============================================================================

    [Theory]
    [InlineData("123")] // Too short
    [InlineData("abcdefghij")] // Alphabetic
    [InlineData("055-555-1234")] // Invalid NANP
    public async Task Handle_InvalidPhone_ReturnsErrorResponse(string invalidPhone)
    {
        // Arrange
        var query = new LookupQuery
        {
            Phone = invalidPhone
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Response.Should().Be("error");
        result.Message.Should().Contain("Invalid phone number");
    }

    // ============================================================================
    // METADATA VALIDATION
    // ============================================================================

    [Fact]
    public async Task Handle_ValidQuery_GeneratesUniqueRequestId()
    {
        // Arrange
        var query = new LookupQuery
        {
            Phone = "8015551234"
        };

        // Act
        var result1 = await _handler.Handle(query, CancellationToken.None);
        var result2 = await _handler.Handle(query, CancellationToken.None);

        // Assert - Each request should have a unique request_id
        result1.Metadata.RequestId.Should().NotBe(result2.Metadata.RequestId);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsNonNullData()
    {
        // Arrange
        var query = new LookupQuery
        {
            Phone = "8015551234"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data!.ConsumerKey.Should().NotBeNullOrEmpty();
    }
}
