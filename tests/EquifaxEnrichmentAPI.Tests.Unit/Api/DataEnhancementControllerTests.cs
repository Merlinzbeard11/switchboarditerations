using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MediatR;
using EquifaxEnrichmentAPI.Api.Controllers;
using EquifaxEnrichmentAPI.Api.DTOs;
using EquifaxEnrichmentAPI.Api.Services;
using EquifaxEnrichmentAPI.Application.Queries.Lookup;

namespace EquifaxEnrichmentAPI.Tests.Unit.Api;

/// <summary>
/// Tests for Feature 1.1: REST API Endpoint - DataEnhancementController
/// BDD Scenarios: features/phase1/feature-1.1-rest-api-endpoint.feature
/// Scenarios 1-4: Basic endpoint functionality with mock responses
///
/// Slice 3: Updated to test thin controller with mocked MediatR.
/// Business logic testing moved to LookupQueryHandlerTests.
/// </summary>
public class DataEnhancementControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IAuditLoggingService> _auditServiceMock;
    private readonly DataEnhancementController _controller;

    public DataEnhancementControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _auditServiceMock = new Mock<IAuditLoggingService>();

        // Setup smart mock that dynamically responds based on query
        // Simulates handler behavior to test controller mapping
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<LookupQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LookupQuery query, CancellationToken ct) =>
            {
                // Simulate "no match" for 5559999999 (same as handler)
                if (query.Phone == "5559999999")
                {
                    return new LookupResult
                    {
                        Response = "error",
                        Message = "Unable to find record for phone number",
                        Data = new EnrichmentData
                        {
                            ConsumerKey = "5559999999",
                            PersonalInfo = new { phone = "5559999999", match_attempted = true, match_confidence = 0.0 },
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
                            ResponseTimeMs = 10,
                            RequestId = Guid.NewGuid().ToString(),
                            UniqueId = query.UniqueId,
                            TotalFieldsReturned = null
                        }
                    };
                }

                // Calculate match confidence (same logic as handler)
                var confidence = 0.75;
                if (!string.IsNullOrWhiteSpace(query.FirstName)) confidence += 0.05;
                if (!string.IsNullOrWhiteSpace(query.LastName)) confidence += 0.05;
                if (!string.IsNullOrWhiteSpace(query.PostalCode)) confidence += 0.05;
                if (!string.IsNullOrWhiteSpace(query.State)) confidence += 0.03;
                if (!string.IsNullOrWhiteSpace(query.IpAddress)) confidence += 0.02;
                confidence = Math.Min(confidence, 1.0);

                // Determine match type (same logic as handler)
                var hasName = !string.IsNullOrWhiteSpace(query.FirstName) || !string.IsNullOrWhiteSpace(query.LastName);
                var hasAddress = !string.IsNullOrWhiteSpace(query.PostalCode) || !string.IsNullOrWhiteSpace(query.State);
                var matchType = (hasName, hasAddress) switch
                {
                    (true, true) => "phone_with_name_and_address",
                    (true, false) => "phone_with_name",
                    (false, true) => "phone_with_address",
                    _ => "phone_only"
                };

                // Success response
                return new LookupResult
                {
                    Response = "success",
                    Message = confidence > 0.90 ? "Record found with high confidence" : "Record found with moderate confidence",
                    Data = new EnrichmentData
                    {
                        ConsumerKey = $"EQF_{Guid.NewGuid():N}",
                        PersonalInfo = new { },
                        Addresses = new object[] { },
                        Phones = new object[] { },
                        Financial = new { }
                    },
                    Metadata = new ResponseMetadata
                    {
                        MatchConfidence = confidence,
                        MatchType = matchType,
                        DataFreshnessDate = DateTime.UtcNow.AddDays(-7),
                        QueryTimestamp = DateTime.UtcNow,
                        ResponseTimeMs = 10,
                        RequestId = Guid.NewGuid().ToString(),
                        UniqueId = query.UniqueId,
                        TotalFieldsReturned = query.Fields?.ToLower() == "full" ? 398 : null
                    }
                };
            });

        _controller = new DataEnhancementController(_mediatorMock.Object, _auditServiceMock.Object);

        // Setup HttpContext for BuyerId access (set by ApiKeyAuthenticationMiddleware)
        _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };
        _controller.HttpContext.Items["BuyerId"] = Guid.NewGuid();
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

    // ============================================================================
    // FEATURE 2.3: FCRA AUDIT LOGGING - CONTROLLER INTEGRATION TESTS
    // BDD Feature: features/phase2/feature-2.3-fcra-audit-logging.feature
    // ============================================================================

    /// <summary>
    /// TDD Test for Feature 2.3: Verify AuditLoggingService is called after successful lookup
    /// BDD Scenario 1 (Lines 19-38): Log every API query with comprehensive audit trail
    /// </summary>
    [Fact]
    public async Task Lookup_SuccessfulRequest_CallsAuditLoggingService()
    {
        // Arrange
        var auditServiceMock = new Mock<IAuditLoggingService>();
        var controller = new DataEnhancementController(_mediatorMock.Object, auditServiceMock.Object);

        // Setup HttpContext for BuyerId access
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };
        controller.HttpContext.Items["BuyerId"] = Guid.NewGuid();

        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = "insurance_underwriting",
            IpAddress = "192.168.1.100"
        };

        // Act
        var result = await controller.Lookup(request);

        // Assert
        // BDD Scenario 1: Audit log entry should be created (Line 24)
        auditServiceMock.Verify(
            x => x.LogRequestAsync(It.IsAny<AuditLogEntry>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "Audit logging service must be called exactly once per API request (FCRA § 607(b) requirement)"
        );
    }

    /// <summary>
    /// TDD Test for Feature 2.3: Verify correct phone hash is logged
    /// BDD Scenario 1 (Line 27): phone_number_queried_hash should be SHA-256 hash
    /// </summary>
    [Fact]
    public async Task Lookup_SuccessfulRequest_LogsCorrectPhoneHash()
    {
        // Arrange
        var auditServiceMock = new Mock<IAuditLoggingService>();
        var controller = new DataEnhancementController(_mediatorMock.Object, auditServiceMock.Object);

        // Setup HttpContext for BuyerId access
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };
        controller.HttpContext.Items["BuyerId"] = Guid.NewGuid();

        var phone = "8015551234";
        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = phone,
            PermissiblePurpose = "insurance_underwriting"
        };

        // Act
        await controller.Lookup(request);

        // Assert
        // BDD Scenario 1: Phone should be passed to audit service for hashing (Line 38)
        // NOTE: PhoneHash is computed by AuditLoggingService, not the controller
        auditServiceMock.Verify(
            x => x.LogRequestAsync(
                It.Is<AuditLogEntry>(entry => entry.Phone == phone),
                It.IsAny<CancellationToken>()
            ),
            Times.Once,
            "Phone number must be passed to audit logging service for FCRA compliance"
        );
    }

    /// <summary>
    /// TDD Test for Feature 2.3: Verify permissible purpose is logged
    /// BDD Scenario 1 (Line 30): permissible_purpose should be logged
    /// </summary>
    [Fact]
    public async Task Lookup_SuccessfulRequest_LogsPermissiblePurpose()
    {
        // Arrange
        var auditServiceMock = new Mock<IAuditLoggingService>();
        var controller = new DataEnhancementController(_mediatorMock.Object, auditServiceMock.Object);

        // Setup HttpContext for BuyerId access
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };
        controller.HttpContext.Items["BuyerId"] = Guid.NewGuid();

        var permissiblePurpose = "insurance_underwriting";
        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = permissiblePurpose
        };

        // Act
        await controller.Lookup(request);

        // Assert
        // BDD Scenario 1: Permissible purpose must be logged (Line 30)
        auditServiceMock.Verify(
            x => x.LogRequestAsync(
                It.Is<AuditLogEntry>(entry => entry.PermissiblePurpose == permissiblePurpose),
                It.IsAny<CancellationToken>()
            ),
            Times.Once,
            "Permissible purpose must be logged for every query (FCRA § 604 requirement)"
        );
    }

    /// <summary>
    /// TDD Test for Feature 2.3: Verify IP address is logged
    /// BDD Scenario 1 (Line 31): ip_address should be logged
    /// </summary>
    [Fact]
    public async Task Lookup_SuccessfulRequest_LogsIpAddress()
    {
        // Arrange
        var auditServiceMock = new Mock<IAuditLoggingService>();
        var controller = new DataEnhancementController(_mediatorMock.Object, auditServiceMock.Object);

        // Setup HttpContext for BuyerId access
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };
        controller.HttpContext.Items["BuyerId"] = Guid.NewGuid();

        var ipAddress = "192.168.1.100";
        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = "insurance_underwriting",
            IpAddress = ipAddress
        };

        // Act
        await controller.Lookup(request);

        // Assert
        // BDD Scenario 1: IP address should be logged (Line 31)
        auditServiceMock.Verify(
            x => x.LogRequestAsync(
                It.Is<AuditLogEntry>(entry => entry.IpAddress == ipAddress),
                It.IsAny<CancellationToken>()
            ),
            Times.Once,
            "IP address must be logged for fraud detection and audit trail"
        );
    }

    /// <summary>
    /// TDD Test for Feature 2.3: Verify response type is logged
    /// BDD Scenario 1 (Line 29): match_found should be logged
    /// </summary>
    [Fact]
    public async Task Lookup_SuccessfulMatch_LogsSuccessResponse()
    {
        // Arrange
        var auditServiceMock = new Mock<IAuditLoggingService>();
        var controller = new DataEnhancementController(_mediatorMock.Object, auditServiceMock.Object);

        // Setup HttpContext for BuyerId access
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };
        controller.HttpContext.Items["BuyerId"] = Guid.NewGuid();

        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234", // Will return success from mock
            PermissiblePurpose = "insurance_underwriting"
        };

        // Act
        await controller.Lookup(request);

        // Assert
        // BDD Scenario 1: Response should be "success" for matches (Line 29)
        auditServiceMock.Verify(
            x => x.LogRequestAsync(
                It.Is<AuditLogEntry>(entry =>
                    entry.Response == "success" &&
                    entry.StatusCode == 200),
                It.IsAny<CancellationToken>()
            ),
            Times.Once,
            "Successful lookups must be logged with response='success' and status_code=200"
        );
    }

    /// <summary>
    /// TDD Test for Feature 2.3: Verify no-match responses are logged
    /// BDD Scenario 4: No Match Found - should still log audit entry
    /// </summary>
    [Fact]
    public async Task Lookup_NoMatchFound_StillLogsAuditEntry()
    {
        // Arrange
        var auditServiceMock = new Mock<IAuditLoggingService>();
        var controller = new DataEnhancementController(_mediatorMock.Object, auditServiceMock.Object);

        // Setup HttpContext for BuyerId access
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };
        controller.HttpContext.Items["BuyerId"] = Guid.NewGuid();

        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "5559999999", // Mock returns no match for this phone
            PermissiblePurpose = "insurance_underwriting"
        };

        // Act
        await controller.Lookup(request);

        // Assert
        // BDD Scenario: No-match queries MUST still be audited (FCRA requirement)
        auditServiceMock.Verify(
            x => x.LogRequestAsync(
                It.Is<AuditLogEntry>(entry =>
                    entry.Response == "error" &&
                    entry.Phone == "5559999999"),
                It.IsAny<CancellationToken>()
            ),
            Times.Once,
            "No-match queries must STILL be logged for FCRA compliance (every query attempt must be audited)"
        );
    }
}
