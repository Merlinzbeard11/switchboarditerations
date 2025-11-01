using Xunit;
using FluentAssertions;
using EquifaxEnrichmentAPI.Api.DTOs;
using FluentValidation.TestHelper;

namespace EquifaxEnrichmentAPI.Tests.Unit.Api;

/// <summary>
/// Tests for Feature 1.1: REST API Endpoint - Request Validation
/// BDD Scenarios: features/phase1/feature-1.1-rest-api-endpoint.feature
/// Scenarios 6, 7, 8 - Validation of required fields, phone format, permissible purpose
/// </summary>
public class LookupRequestValidationTests
{
    private readonly LookupRequestDtoValidator _validator;

    public LookupRequestValidationTests()
    {
        _validator = new LookupRequestDtoValidator();
    }

    // ============================================================================
    // SCENARIO 6: Missing Required Fields (400 Bad Request)
    // BDD Lines 165-177
    // NOTE: API key authentication is now handled by middleware (feature-2.1) using
    // X-API-Key header - no api_key field in request body
    // ============================================================================

    [Fact(Skip = "MVP Phase 1 - Provider code validation deferred to future slice (BDD Scenario 6)")]
    public void Validate_MissingProviderCode_ReturnsValidationError()
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ProviderCode = null, // Missing required field
            Phone = "8015551234",
            PermissiblePurpose = "insurance_underwriting"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProviderCode)
            .WithErrorMessage("provider_code is required");
    }

    [Fact]
    public void Validate_MissingPhone_ReturnsValidationError()
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = null, // Missing required field
            PermissiblePurpose = "insurance_underwriting"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage("phone is required");
    }

    [Fact(Skip = "MVP Phase 1 - Permissible purpose validation deferred to future slice (BDD Scenario 8)")]
    public void Validate_MissingPermissiblePurpose_ReturnsValidationError()
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = null // Missing required field
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PermissiblePurpose)
            .WithErrorMessage("permissible_purpose is required");
    }

    // ============================================================================
    // SCENARIO 7: Invalid Phone Number Format (400 Bad Request)
    // BDD Lines 182-202
    // ============================================================================

    [Theory]
    [InlineData("123")] // Only 3 digits
    [InlineData("abcdefghij")] // Alphabetic
    [InlineData("055-555-1234")] // Starts with 0 (invalid NANP)
    [InlineData("(555) 123-456")] // Too short
    [InlineData("555-1234")] // Only 7 digits
    public void Validate_InvalidPhoneFormat_ReturnsValidationError(string invalidPhone)
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = invalidPhone,
            PermissiblePurpose = "insurance_underwriting"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage("Invalid phone number format");
    }

    [Theory]
    [InlineData("8015551234")] // Valid 10-digit
    [InlineData("(801) 555-1234")] // Valid with formatting
    [InlineData("801-555-1234")] // Valid with hyphens
    [InlineData("1-801-555-1234")] // Valid with country code
    [InlineData("+1 (801) 555-1234")] // Valid E.164 format
    public void Validate_ValidPhoneFormat_PassesValidation(string validPhone)
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = validPhone,
            PermissiblePurpose = "insurance_underwriting"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    // ============================================================================
    // SCENARIO 8: Invalid Permissible Purpose (400 Bad Request)
    // BDD Lines 207-220
    // ============================================================================

    [Fact(Skip = "MVP Phase 1 - Permissible purpose validation deferred to future slice (BDD Scenario 8)")]
    public void Validate_InvalidPermissiblePurpose_ReturnsValidationError()
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = "invalid_purpose"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PermissiblePurpose)
            .WithErrorMessage("Invalid permissible purpose. Valid values: insurance_underwriting, credit_extension, employment_screening, tenant_screening, legitimate_business_need");
    }

    [Theory]
    [InlineData("insurance_underwriting")]
    [InlineData("credit_extension")]
    [InlineData("employment_screening")]
    [InlineData("tenant_screening")]
    [InlineData("legitimate_business_need")]
    public void Validate_ValidPermissiblePurpose_PassesValidation(string validPurpose)
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = validPurpose
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PermissiblePurpose);
    }

    // ============================================================================
    // SCENARIO 11: Default Fields Selection
    // BDD Lines 273-280
    // ============================================================================

    [Fact]
    public void Validate_MissingFieldsParameter_DefaultsToBasic()
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = "insurance_underwriting"
            // Fields not specified - should default to "basic"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        // Note: Default value will be set in DTO constructor
    }

    [Theory]
    [InlineData("basic")]
    [InlineData("full")]
    public void Validate_ValidFieldsParameter_PassesValidation(string fields)
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = "insurance_underwriting",
            Fields = fields
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Fields);
    }

    [Fact]
    public void Validate_InvalidFieldsParameter_ReturnsValidationError()
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = "insurance_underwriting",
            Fields = "invalid_value"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Fields)
            .WithErrorMessage("Invalid fields parameter. Valid values: basic, full");
    }

    // ============================================================================
    // Optional Fields Validation (Scenario 3)
    // BDD Lines 90-112
    // ============================================================================

    [Fact]
    public void Validate_AllOptionalFieldsProvided_PassesValidation()
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = "insurance_underwriting",
            FirstName = "Bob",
            LastName = "Barker",
            PostalCode = "84010",
            State = "UT",
            IpAddress = "192.168.1.100",
            UniqueId = "b4c9f530-5461-11ef-8f6f-8ffb313ceb02"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("8401")] // Too short
    [InlineData("840101234")] // Too long
    [InlineData("abcde")] // Non-numeric
    public void Validate_InvalidPostalCode_ReturnsValidationError(string invalidPostalCode)
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = "insurance_underwriting",
            PostalCode = invalidPostalCode
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PostalCode);
    }

    [Theory]
    [InlineData("ABC")] // Too long
    [InlineData("A")] // Too short
    [InlineData("12")] // Numeric
    public void Validate_InvalidState_ReturnsValidationError(string invalidState)
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = "insurance_underwriting",
            State = invalidState
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.State);
    }

    [Theory]
    [InlineData("192.168.1.256")] // Invalid octet
    [InlineData("not.an.ip.address")] // Non-numeric
    [InlineData("192.168.1")] // Incomplete
    public void Validate_InvalidIpAddress_ReturnsValidationError(string invalidIp)
    {
        // Arrange
        var request = new LookupRequestDto
        {
            ProviderCode = "EQUIFAX_ENRICHMENT",
            Phone = "8015551234",
            PermissiblePurpose = "insurance_underwriting",
            IpAddress = invalidIp
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IpAddress);
    }
}
