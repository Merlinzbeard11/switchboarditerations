using FluentValidation;
using EquifaxEnrichmentAPI.Api.DTOs;
using EquifaxEnrichmentAPI.Domain.ValueObjects;
using System.Text.RegularExpressions;

namespace EquifaxEnrichmentAPI.Api.DTOs;

/// <summary>
/// FluentValidation validator for LookupRequestDto.
/// Implements validation rules from BDD Scenarios 6, 7, 8, 11.
/// BDD File: features/phase1/feature-1.1-rest-api-endpoint.feature
/// </summary>
public partial class LookupRequestDtoValidator : AbstractValidator<LookupRequestDto>
{
    // FCRA-compliant permissible purposes
    private static readonly string[] ValidPermissiblePurposes = new[]
    {
        "insurance_underwriting",
        "credit_extension",
        "employment_screening",
        "tenant_screening",
        "legitimate_business_need"
    };

    private static readonly string[] ValidFieldsOptions = new[] { "basic", "full" };

    public LookupRequestDtoValidator()
    {
        // ====================================================================
        // REQUIRED FIELDS VALIDATION
        // BDD Scenario 6: Missing Required Fields (400 Bad Request)
        // Lines 165-177
        //
        // MVP PHASE 1: Only phone is required.
        // Authentication is now handled by API Key Authentication Middleware (feature-2.1)
        // using X-API-Key header - no api_key field in request body.
        // ProviderCode and PermissiblePurpose will be enforced in future slices.
        // TODO (Future Slice): Uncomment PermissiblePurpose validation for BDD Scenario 8
        // ====================================================================

        // Slice 9: Provider Code validation
        RuleFor(x => x.ProviderCode)
            .NotEmpty()
            .WithMessage("provider_code is required");

        // MVP PHASE 1: Phone is REQUIRED
        RuleFor(x => x.Phone)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("phone is required")
            .Must(BeValidPhoneNumber)
            .WithMessage("Invalid phone number format");

        // FUTURE: Permissible Purpose validation (BDD Scenario 8)
        // RuleFor(x => x.PermissiblePurpose)
        //     .Cascade(CascadeMode.Stop)
        //     .NotEmpty()
        //     .WithMessage("permissible_purpose is required")
        //     .Must(BeValidPermissiblePurpose)
        //     .WithMessage($"Invalid permissible purpose. Valid values: {string.Join(", ", ValidPermissiblePurposes)}");

        // ====================================================================
        // OPTIONAL FIELDS VALIDATION
        // BDD Scenario 11: Default Fields Selection
        // Lines 273-280
        // ====================================================================

        RuleFor(x => x.Fields)
            .Must(BeValidFieldsOption)
            .When(x => !string.IsNullOrWhiteSpace(x.Fields))
            .WithMessage($"Invalid fields parameter. Valid values: {string.Join(", ", ValidFieldsOptions)}");

        // ====================================================================
        // ENHANCED MATCHING FIELDS VALIDATION
        // BDD Scenario 3: Optional fields for improved match confidence
        // Lines 90-112
        // ====================================================================

        RuleFor(x => x.PostalCode)
            .Matches(@"^\d{5}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PostalCode))
            .WithMessage("Postal code must be 5 digits");

        RuleFor(x => x.State)
            .Length(2)
            .Matches(@"^[A-Z]{2}$")
            .When(x => !string.IsNullOrWhiteSpace(x.State))
            .WithMessage("State must be 2-letter uppercase code (e.g., UT, CA, NY)");

        RuleFor(x => x.IpAddress)
            .Must(BeValidIpAddress)
            .When(x => !string.IsNullOrWhiteSpace(x.IpAddress))
            .WithMessage("Invalid IP address format");
    }

    /// <summary>
    /// Validates phone number using PhoneNumber value object.
    /// BDD Scenario 7: Invalid Phone Number Format
    /// BDD Scenario 10: Phone normalization handling
    /// Lines 182-269
    /// </summary>
    private static bool BeValidPhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        var result = PhoneNumber.Create(phone);
        return result.IsSuccess;
    }

    /// <summary>
    /// Validates permissible purpose against FCRA-compliant list.
    /// BDD Scenario 8: Invalid Permissible Purpose
    /// Lines 207-220
    /// </summary>
    private static bool BeValidPermissiblePurpose(string? purpose)
    {
        if (string.IsNullOrWhiteSpace(purpose))
            return false;

        return ValidPermissiblePurposes.Contains(purpose, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates fields parameter against allowed values.
    /// BDD Scenario 2: Full dataset request
    /// BDD Scenario 11: Default fields selection
    /// </summary>
    private static bool BeValidFieldsOption(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
            return true; // null/empty is valid (defaults to "basic")

        return ValidFieldsOptions.Contains(fields, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates IPv4 address format.
    /// BDD Scenario 3: Optional fields validation
    /// Requires exactly 4 octets (xxx.xxx.xxx.xxx)
    /// </summary>
    private static bool BeValidIpAddress(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        // Use .NET's built-in IP address parsing
        if (!System.Net.IPAddress.TryParse(ipAddress, out var parsedIp))
            return false;

        // Must be IPv4 (InterNetwork)
        if (parsedIp.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            return false;

        // Ensure exactly 4 octets (reject "192.168.1" which .NET parses as valid)
        var parts = ipAddress.Split('.');
        return parts.Length == 4;
    }
}
