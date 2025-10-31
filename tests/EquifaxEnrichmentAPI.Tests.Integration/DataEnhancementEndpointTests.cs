using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;

namespace EquifaxEnrichmentAPI.Tests.Integration;

/// <summary>
/// Integration tests for Data Enhancement API endpoint.
/// Tests the entire stack: Controller → MediatR → Handler → Repository → PostgreSQL Database.
///
/// BDD Feature: REST API Endpoint for Phone Number Enrichment
/// BDD File: features/phase1/feature-1.1-rest-api-endpoint.feature
///
/// These tests verify end-to-end functionality with real database.
/// Database must be seeded with test data before running (see tools/seed-data.sql).
/// </summary>
public class DataEnhancementEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DataEnhancementEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // ====================================================================
    // BDD SCENARIO 1: Successful Phone Lookup with Basic Fields
    // BDD Lines 15-62
    // ====================================================================

    [Fact]
    public async Task POST_Lookup_WithValidPhone_ReturnsSuccessResponse()
    {
        // Arrange - BDD Scenario 1 (Lines 15-62)
        var request = new
        {
            phone = "8015551234",
            fields = "basic"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/data_enhancement/lookup", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // BDD Line 54: Response should be "success"
        result.RootElement.GetProperty("response").GetString().Should().Be("success");

        // BDD Line 55: Message should not be empty
        result.RootElement.GetProperty("message").GetString().Should().NotBeNullOrEmpty();

        // BDD Lines 56-58: Data section should exist and contain enrichment
        result.RootElement.TryGetProperty("data", out var data).Should().BeTrue();
        data.TryGetProperty("consumer_key", out _).Should().BeTrue();
        data.TryGetProperty("personal_info", out _).Should().BeTrue();
        data.TryGetProperty("addresses", out _).Should().BeTrue();
        data.TryGetProperty("phones", out _).Should().BeTrue();

        // BDD Lines 59-62: Metadata should exist with required fields
        result.RootElement.TryGetProperty("metadata", out var metadata).Should().BeTrue();
        metadata.TryGetProperty("match_confidence", out _).Should().BeTrue();
        metadata.TryGetProperty("match_type", out _).Should().BeTrue();
        metadata.TryGetProperty("query_timestamp", out _).Should().BeTrue();
    }

    [Fact]
    public async Task POST_Lookup_WithValidPhone_ReturnsExpectedConsumerData()
    {
        // Arrange - Verify specific consumer data from seeded database
        var request = new
        {
            phone = "8015551234"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/data_enhancement/lookup", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // Verify consumer data matches seed data
        var data = result.RootElement.GetProperty("data");
        var personalInfo = data.GetProperty("personal_info");

        // Seeded data has Bob Barker
        personalInfo.GetProperty("first_name").GetString().Should().Be("Bob");
        personalInfo.GetProperty("last_name").GetString().Should().Be("Barker");
    }

    // ====================================================================
    // BDD SCENARIO 2: Full Fields Request (398 Columns)
    // BDD Lines 67-85
    // ====================================================================

    [Fact]
    public async Task POST_Lookup_WithFullFields_ReturnsTotalFieldsReturned()
    {
        // Arrange - BDD Scenario 2 (Lines 67-85)
        var request = new
        {
            phone = "8015551234",
            fields = "full"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/data_enhancement/lookup", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // BDD Line 83: total_fields_returned should be 398 for full dataset
        var metadata = result.RootElement.GetProperty("metadata");
        metadata.GetProperty("total_fields_returned").GetInt32().Should().Be(398);
    }

    // ====================================================================
    // BDD SCENARIO 3: Enhanced Matching with Optional Fields
    // BDD Lines 90-112
    // ====================================================================

    [Fact]
    public async Task POST_Lookup_WithOptionalFields_ReturnsHigherMatchConfidence()
    {
        // Arrange - BDD Scenario 3 (Lines 90-112)
        var request = new
        {
            phone = "8015551234",
            first_name = "Bob",
            last_name = "Barker",
            postal_code = "84010",
            state = "UT",
            ip_address = "192.168.1.100",
            unique_id = "b4c9f530-5461-11ef-8f6f-8ffb313ceb02"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/data_enhancement/lookup", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        var metadata = result.RootElement.GetProperty("metadata");

        // BDD Line 109: match_confidence should be > 0.90 with optional fields
        // Base confidence 0.75 + 0.10 (first) + 0.05 (last) + 0.07 (postal) + 0.03 (state) + 0.02 (IP) = 1.02 → capped at 1.0
        metadata.GetProperty("match_confidence").GetDouble().Should().BeGreaterThan(0.90);

        // BDD Line 110: match_type should reflect optional fields used
        metadata.GetProperty("match_type").GetString().Should().Be("phone_with_name_and_address");

        // BDD Line 111: unique_id should be echoed back
        metadata.GetProperty("unique_id").GetString().Should().Be("b4c9f530-5461-11ef-8f6f-8ffb313ceb02");
    }

    // ====================================================================
    // BDD SCENARIO 4: No Match Found
    // BDD Lines 117-141
    // ====================================================================

    [Fact]
    public async Task POST_Lookup_WithNonExistentPhone_ReturnsErrorResponse()
    {
        // Arrange - BDD Scenario 4 (Lines 117-141)
        // Phone 5559999999 is NOT seeded in database
        var request = new
        {
            phone = "5559999999"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/data_enhancement/lookup", request);

        // Assert - BDD Line 129: Status code should still be 200 OK (business error, not HTTP error)
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // BDD Line 130: Response should be "error"
        result.RootElement.GetProperty("response").GetString().Should().Be("error");

        // BDD Line 131: Message should indicate record not found
        result.RootElement.GetProperty("message").GetString().Should().Contain("Unable to find record");

        // BDD Lines 133-138: Data section should still exist with phone number
        var data = result.RootElement.GetProperty("data");
        data.TryGetProperty("consumer_key", out _).Should().BeTrue();

        // BDD Line 139: Metadata should show zero confidence
        var metadata = result.RootElement.GetProperty("metadata");
        metadata.GetProperty("match_confidence").GetDouble().Should().Be(0.0);
        metadata.GetProperty("match_type").GetString().Should().Be("no_match");
    }

    // ====================================================================
    // BDD SCENARIO 10: Phone Number Normalization
    // BDD Lines 246-269
    // ====================================================================

    [Theory]
    [InlineData("8015551234")]       // Already normalized
    [InlineData("(801) 555-1234")]   // With parentheses and spaces
    [InlineData("801-555-1234")]     // With dashes
    [InlineData("1-801-555-1234")]   // With country code and dashes
    [InlineData("+1 (801) 555-1234")] // International format
    public async Task POST_Lookup_WithDifferentPhoneFormats_NormalizesAndFindsMatch(string phoneFormat)
    {
        // Arrange - BDD Scenario 10 (Lines 246-269)
        var request = new
        {
            phone = phoneFormat
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/data_enhancement/lookup", request);

        // Assert - All formats should normalize to 8015551234 and find match
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        // BDD Line 260: All formats should result in successful lookup
        result.RootElement.GetProperty("response").GetString().Should().Be("success");

        // Verify same consumer found regardless of format
        var data = result.RootElement.GetProperty("data");
        var personalInfo = data.GetProperty("personal_info");
        personalInfo.GetProperty("first_name").GetString().Should().Be("Bob");
        personalInfo.GetProperty("last_name").GetString().Should().Be("Barker");
    }

    // ====================================================================
    // BDD SCENARIO 12: Metadata Requirements
    // BDD Lines 285-306
    // ====================================================================

    [Fact]
    public async Task POST_Lookup_ValidRequest_ReturnsCompleteMetadata()
    {
        // Arrange - BDD Scenario 12 (Lines 285-306)
        var uniqueId = "test-metadata-id-789";
        var request = new
        {
            phone = "8015551234",
            unique_id = uniqueId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/data_enhancement/lookup", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        var metadata = result.RootElement.GetProperty("metadata");

        // BDD Lines 294-306: Verify all required metadata fields
        metadata.GetProperty("match_confidence").GetDouble().Should().BeInRange(0.0, 1.0);
        metadata.GetProperty("match_type").GetString().Should().NotBeNullOrEmpty();
        metadata.TryGetProperty("data_freshness_date", out _).Should().BeTrue();
        metadata.TryGetProperty("query_timestamp", out _).Should().BeTrue();
        metadata.GetProperty("response_time_ms").GetInt32().Should().BeGreaterThanOrEqualTo(0);
        metadata.GetProperty("request_id").GetString().Should().NotBeNullOrEmpty();

        // BDD Line 306: unique_id should be echoed back
        metadata.GetProperty("unique_id").GetString().Should().Be(uniqueId);
    }

    // ====================================================================
    // VALIDATION SCENARIOS
    // ====================================================================

    [Theory]
    [InlineData("")]              // Empty string
    [InlineData("   ")]           // Whitespace
    [InlineData("123")]           // Too short
    [InlineData("abcdefghij")]    // Alphabetic
    public async Task POST_Lookup_WithInvalidPhone_ReturnsBadRequest(string invalidPhone)
    {
        // Arrange
        var request = new
        {
            phone = invalidPhone
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/data_enhancement/lookup", request);

        // Assert - FluentValidation should return 400 Bad Request
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Lookup_WithMissingPhone_ReturnsBadRequest()
    {
        // Arrange - Request without phone field
        var request = new
        {
            fields = "basic"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/data_enhancement/lookup", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
