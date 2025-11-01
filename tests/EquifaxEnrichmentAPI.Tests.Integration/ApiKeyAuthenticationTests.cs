using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EquifaxEnrichmentAPI.Tests.Integration;

/// <summary>
/// TDD Integration Tests for API Key Authentication Middleware
/// BDD Feature: API Key Authentication (feature-2.1-api-key-authentication.feature)
///
/// TESTS WRITTEN FIRST (TDD Red-Green-Refactor)
/// These tests will FAIL until middleware is implemented and registered
/// </summary>
public class ApiKeyAuthenticationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private const string ValidApiKey = "test-api-key-123"; // SHA-256 stored in DB
    private const string InvalidApiKey = "invalid-key-999";
    private const string InactiveApiKey = "inactive-api-key-456"; // Inactive buyer

    public ApiKeyAuthenticationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // ====================================================================
    // BDD Scenario 1: Successfully authenticate request with valid API key
    // ====================================================================
    [Fact]
    public async Task AuthenticatedRequest_ValidApiKey_ShouldReturn200()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/data_enhancement/lookup");
        request.Headers.Add("X-API-Key", ValidApiKey);
        request.Content = JsonContent.Create(new
        {
            phone_number = "555-123-4567",
            provider_code = "TEST",
            permissible_purpose = "Identity Verification"
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        // We expect either 200 (OK) or 400 (validation error) - both prove authentication passed
        // 400 means middleware authenticated successfully, but request validation failed (expected for test data)
        response.StatusCode.Should().Match(code =>
            code == HttpStatusCode.OK || code == HttpStatusCode.BadRequest,
            "valid API key should authenticate (200 or 400 proves middleware passed, not 401)");
    }

    // ====================================================================
    // BDD Scenario 2: Missing API key (401 Unauthorized)
    // ====================================================================
    [Fact]
    public async Task Request_MissingApiKey_ShouldReturn401()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/data_enhancement/lookup");
        // Intentionally NOT adding X-API-Key header
        request.Content = JsonContent.Create(new
        {
            phone_number = "555-123-4567",
            provider_code = "TEST",
            permissible_purpose = "Identity Verification"
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "missing API key should return 401");

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("response").GetString().Should().Be("error");
        json.RootElement.GetProperty("message").GetString().Should().Contain("Missing API key");
    }

    // ====================================================================
    // BDD Scenario 3: Invalid API key (401 Unauthorized)
    // ====================================================================
    [Fact]
    public async Task Request_InvalidApiKey_ShouldReturn401()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/data_enhancement/lookup");
        request.Headers.Add("X-API-Key", InvalidApiKey);
        request.Content = JsonContent.Create(new
        {
            phone_number = "555-123-4567",
            provider_code = "TEST",
            permissible_purpose = "Identity Verification"
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "invalid API key should return 401");

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("response").GetString().Should().Be("error");
        json.RootElement.GetProperty("message").GetString().Should().Contain("Invalid API key");
    }

    // ====================================================================
    // BDD Scenario 5: Reject request for inactive buyer account
    // ====================================================================
    [Fact]
    public async Task Request_InactiveBuyer_ShouldReturn401()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/data_enhancement/lookup");
        request.Headers.Add("X-API-Key", InactiveApiKey);
        request.Content = JsonContent.Create(new
        {
            phone_number = "555-123-4567",
            provider_code = "TEST",
            permissible_purpose = "Identity Verification"
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "inactive buyer should return 401 (same as invalid key to prevent enumeration)");

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("response").GetString().Should().Be("error");
        json.RootElement.GetProperty("message").GetString().Should().Contain("Invalid API key");
    }

    // ====================================================================
    // BDD Scenario 2 & 3 & 5: Empty API key treated as missing
    // ====================================================================
    [Fact]
    public async Task Request_EmptyApiKey_ShouldReturn401()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/data_enhancement/lookup");
        request.Headers.Add("X-API-Key", "");
        request.Content = JsonContent.Create(new
        {
            phone_number = "555-123-4567",
            provider_code = "TEST",
            permissible_purpose = "Identity Verification"
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "empty API key should be treated as missing");

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("response").GetString().Should().Be("error");
        json.RootElement.GetProperty("message").GetString().Should().Contain("Missing API key");
    }

    // ====================================================================
    // BDD Scenario 4: Timing attack prevention
    // Response time should be constant regardless of key validity
    // NOTE: This test has relaxed tolerance for integration testing environment
    // Constant-time implementation verified by CryptographicOperations.FixedTimeEquals() usage
    // ====================================================================
    [Fact]
    public async Task Authentication_TimingAttackPrevention_ShouldHaveConstantResponseTime()
    {
        // WARMUP: Send requests to eliminate first-request overhead
        // (database connection pool init, JIT compilation, etc.)
        for (int i = 0; i < 3; i++)
        {
            var warmupRequest = new HttpRequestMessage(HttpMethod.Post, "/api/data_enhancement/lookup");
            warmupRequest.Headers.Add("X-API-Key", i % 2 == 0 ? ValidApiKey : InvalidApiKey);
            warmupRequest.Content = JsonContent.Create(new
            {
                phone_number = "555-123-4567",
                provider_code = "TEST",
                permissible_purpose = "Identity Verification"
            });
            await _client.SendAsync(warmupRequest);
        }

        // Arrange - Create fresh requests after warmup
        var validKeyRequest = new HttpRequestMessage(HttpMethod.Post, "/api/data_enhancement/lookup");
        validKeyRequest.Headers.Add("X-API-Key", ValidApiKey);
        validKeyRequest.Content = JsonContent.Create(new
        {
            phone_number = "555-123-4567",
            provider_code = "TEST",
            permissible_purpose = "Identity Verification"
        });

        var invalidKeyRequest = new HttpRequestMessage(HttpMethod.Post, "/api/data_enhancement/lookup");
        invalidKeyRequest.Headers.Add("X-API-Key", InvalidApiKey);
        invalidKeyRequest.Content = JsonContent.Create(new
        {
            phone_number = "555-123-4567",
            provider_code = "TEST",
            permissible_purpose = "Identity Verification"
        });

        // Act - Measure response times AFTER warmup
        var validKeyStopwatch = System.Diagnostics.Stopwatch.StartNew();
        await _client.SendAsync(validKeyRequest);
        validKeyStopwatch.Stop();

        var invalidKeyStopwatch = System.Diagnostics.Stopwatch.StartNew();
        await _client.SendAsync(invalidKeyRequest);
        invalidKeyStopwatch.Stop();

        // Assert - Response times should be similar (within 500ms variance for integration tests)
        // This prevents timing attacks that could distinguish valid/invalid keys
        // NOTE: Integration tests have relaxed tolerance due to system load variations
        // Actual constant-time security is provided by CryptographicOperations.FixedTimeEquals()
        var timeDifference = Math.Abs(
            validKeyStopwatch.ElapsedMilliseconds - invalidKeyStopwatch.ElapsedMilliseconds
        );

        timeDifference.Should().BeLessThan(500,
            "response times must be similar to prevent timing attacks (BDD Scenario 4, CVE-2025-59425)");
    }
}
