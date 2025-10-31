using Xunit;
using FluentAssertions;
using System.Diagnostics;
using EquifaxEnrichmentAPI.Domain.ValueObjects;

namespace EquifaxEnrichmentAPI.Tests.Unit;

/// <summary>
/// Tests for Feature 1.2: Phone Number Normalization
/// BDD Scenarios: features/phase1/feature-1.2-phone-number-normalization.feature
/// </summary>
public class PhoneNumberNormalizationTests
{
    // ============================================================================
    // SCENARIO 1: Basic US Phone Number Formats (Common Cases)
    // ============================================================================

    [Theory]
    [InlineData("5552345678", "5552345678")] // Already normalized
    [InlineData("(555) 234-5678", "5552345678")] // Parentheses and hyphens
    [InlineData("555-234-5678", "5552345678")] // Hyphens only
    [InlineData("555.234.5678", "5552345678")] // Dots as separators
    [InlineData("555 234 5678", "5552345678")] // Spaces as separators
    [InlineData("1-555-234-5678", "5552345678")] // US country code prefix
    [InlineData("+1 555 234 5678", "5552345678")] // E.164 with country code
    [InlineData("+1 (555) 234-5678", "5552345678")] // E.164 with formatting
    [InlineData("+15552345678", "5552345678")] // E.164 compact format
    [InlineData("1 (555) 234-5678", "5552345678")] // Country code with parens
    [InlineData("1.555.234.5678", "5552345678")] // Country code with dots
    public void Normalize_CommonUSFormats_Returns10DigitNumber(string input, string expected)
    {
        // Act
        var result = PhoneNumber.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.NormalizedValue.Should().Be(expected);
        result.Value.NormalizedValue.Should().HaveLength(10);
        result.Value.NormalizedValue.Should().MatchRegex("^[0-9]{10}$"); // Only numeric
        // Note: Performance is tested separately in dedicated performance test methods
    }

    // ============================================================================
    // SCENARIO 2: NANP N11 Rule Validation (Reserved Codes)
    // ============================================================================

    [Theory]
    [InlineData("211-555-2345", "area code")] // N11 area code
    [InlineData("311-555-2345", "area code")] // N11 area code
    [InlineData("411-555-2345", "area code")] // Directory assistance
    [InlineData("511-555-2345", "area code")] // Traffic/weather
    [InlineData("611-555-2345", "area code")] // Repair service
    [InlineData("711-555-2345", "area code")] // TDD relay service
    [InlineData("811-555-2345", "area code")] // Call before you dig
    [InlineData("911-555-2345", "area code")] // Emergency services
    [InlineData("555-211-2345", "exchange")] // N11 exchange
    [InlineData("555-411-2345", "exchange")] // Directory assistance exchange
    [InlineData("555-911-2345", "exchange")] // Emergency services exchange
    [InlineData("411-411-2345", "area code")] // N11 in area code (reports first violation)
    public void Normalize_N11Formats_ReturnsValidationError(string input, string n11Type)
    {
        // Act
        var result = PhoneNumber.Create(input);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("N11 format reserved for special services");
        result.Error.Should().Contain(n11Type);
    }

    // ============================================================================
    // SCENARIO 3: NANP First Digit Rule (Must Be 2-9)
    // ============================================================================

    [Theory]
    [InlineData("055-234-5678", "area code")] // Area code starts with 0
    [InlineData("155-234-5678", "area code")] // Area code starts with 1
    [InlineData("555-023-4567", "exchange")] // Exchange starts with 0
    [InlineData("555-234-5678", null)] // Valid (baseline)
    public void Normalize_FirstDigitValidation_EnforcesNANPRules(string input, string? invalidPart)
    {
        // Act
        var result = PhoneNumber.Create(input);

        // Assert
        if (invalidPart == null)
        {
            result.IsSuccess.Should().BeTrue();
        }
        else
        {
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("First digit must be 2-9");
            result.Error.Should().Contain(invalidPart);
        }
    }

    [Theory]
    [InlineData("255-234-5678")] // Area code starts with 2 (valid)
    [InlineData("955-234-5678")] // Area code starts with 9 (valid)
    [InlineData("555-223-4567")] // Exchange starts with 2 (valid)
    [InlineData("555-923-4567")] // Exchange starts with 9 (valid)
    public void Normalize_ValidFirstDigits_Succeeds(string input)
    {
        // Act
        var result = PhoneNumber.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.NormalizedValue.Should().HaveLength(10);
    }

    // ============================================================================
    // SCENARIO 4: Invalid Phone Number Lengths
    // ============================================================================

    [Theory]
    [InlineData("234", 3)] // Only 3 digits
    [InlineData("555-2345", 7)] // Only 7 digits (missing area code)
    [InlineData("555-234-567", 9)] // Only 9 digits
    [InlineData("555-234-56789", 11)] // 11 digits (extra digit, after stripping country code check)
    [InlineData("23456789012345", 14)] // 14 digits (way too long)
    public void Normalize_InvalidLength_ReturnsValidationError(string input, int actualDigits)
    {
        // Act
        var result = PhoneNumber.Create(input);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid phone number length");
        result.Error.Should().Contain("expected length is 10 digits");
        result.Error.Should().Contain($"actual length is {actualDigits} digits");
    }

    // ============================================================================
    // SCENARIO 5: Null, Empty, and Whitespace Handling
    // ============================================================================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalize_NullOrWhitespace_ReturnsValidationError(string? input)
    {
        // Act
        var result = PhoneNumber.Create(input);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Phone number cannot be null or empty");
    }

    // ============================================================================
    // SCENARIO 6: Non-Numeric Characters (Edge Cases)
    // ============================================================================

    [Theory]
    [InlineData("abc-def-ghij")] // Alphabetic characters
    [InlineData("555-ABC-1234")] // Mixed alphanumeric
    [InlineData("555@123#4567")] // Special characters (not allowed)
    public void Normalize_NonNumericCharacters_ReturnsValidationError(string input)
    {
        // Act
        var result = PhoneNumber.Create(input);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid phone number format");
    }

    // ============================================================================
    // SCENARIO 7: Performance Requirement (<1ms)
    // ============================================================================

    [Fact]
    public void Normalize_Performance_CompletesInLessThan1ms()
    {
        // Arrange
        var input = "+1 (555) 234-5678";
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = PhoneNumber.Create(input);

        // Assert
        stopwatch.Stop();
        result.IsSuccess.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1,
            "phone normalization must complete in < 1ms for performance SLA");
    }

    [Fact]
    public void Normalize_Performance_100Iterations_AverageLessThan1ms()
    {
        // Arrange
        var inputs = new[]
        {
            "(555) 234-5678",
            "+1 555 234 5678",
            "555-234-5678",
            "5552345678"
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            var input = inputs[i % inputs.Length];
            var result = PhoneNumber.Create(input);
            result.IsSuccess.Should().BeTrue();
        }
        stopwatch.Stop();

        // Assert
        var averageMs = stopwatch.Elapsed.TotalMilliseconds / 100;
        averageMs.Should().BeLessThan(1,
            $"average normalization time was {averageMs:F3}ms, expected < 1ms");
    }

    // ============================================================================
    // SCENARIO 8: Idempotency (Normalizing Already-Normalized Number)
    // ============================================================================

    [Fact]
    public void Normalize_AlreadyNormalized_ReturnsUnchanged()
    {
        // Arrange
        var input = "5552345678";

        // Act
        var result1 = PhoneNumber.Create(input);
        var result2 = PhoneNumber.Create(result1.Value.NormalizedValue);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.NormalizedValue.Should().Be(result2.Value.NormalizedValue);
        result1.Value.NormalizedValue.Should().Be("5552345678");
    }

    // ============================================================================
    // SCENARIO 9: Edge Cases - Leading/Trailing Whitespace
    // ============================================================================

    [Theory]
    [InlineData("  5552345678  ", "5552345678")]
    [InlineData("\t(555) 234-5678\t", "5552345678")]
    [InlineData("\n555-234-5678\n", "5552345678")]
    public void Normalize_LeadingTrailingWhitespace_TrimsAndNormalizes(string input, string expected)
    {
        // Act
        var result = PhoneNumber.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.NormalizedValue.Should().Be(expected);
    }

    // ============================================================================
    // SCENARIO 10: Country Code Removal (US +1)
    // ============================================================================

    [Theory]
    [InlineData("+1555234567 8", "5552345678")] // E.164 with spaces (should handle gracefully)
    [InlineData("001-555-234-5678", "5552345678")] // International format with 00 prefix
    public void Normalize_InternationalFormats_RemovesCountryCode(string input, string expected)
    {
        // Act
        var result = PhoneNumber.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.NormalizedValue.Should().Be(expected);
    }
}
