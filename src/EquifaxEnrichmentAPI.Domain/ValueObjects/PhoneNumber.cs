using System.Text;
using System.Text.RegularExpressions;
using EquifaxEnrichmentAPI.Domain.Common;

namespace EquifaxEnrichmentAPI.Domain.ValueObjects;

/// <summary>
/// Phone Number value object following NANP (North American Numbering Plan) rules.
/// Immutable, self-validating, with rich domain behavior.
/// BDD Scenarios: features/phase1/feature-1.2-phone-number-normalization.feature
/// </summary>
public sealed class PhoneNumber : IEquatable<PhoneNumber>
{
    private static readonly Regex DigitOnlyRegex = new(@"\d", RegexOptions.Compiled);
    private static readonly Regex NonNumericRegex = new(@"[^\d\s\-\(\)\.\+]", RegexOptions.Compiled);

    public string NormalizedValue { get; }
    public string AreaCode => NormalizedValue.Substring(0, 3);
    public string Exchange => NormalizedValue.Substring(3, 3);
    public string LineNumber => NormalizedValue.Substring(6, 4);

    private PhoneNumber(string normalizedValue)
    {
        NormalizedValue = normalizedValue;
    }

    /// <summary>
    /// Factory method to create and validate a phone number.
    /// Returns Result<PhoneNumber> to avoid throwing exceptions for validation errors.
    /// </summary>
    public static Result<PhoneNumber> Create(string? input)
    {
        // ============================================================================
        // VALIDATION 1: Null/Empty/Whitespace Check
        // BDD Scenario 5: Null, Empty, and Whitespace Handling
        // ============================================================================
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result<PhoneNumber>.Failure("Phone number cannot be null or empty");
        }

        input = input.Trim();

        // ============================================================================
        // VALIDATION 2: Check for Invalid Non-Numeric Characters (Before Processing)
        // BDD Scenario 6: Non-Numeric Characters
        // Allowed: digits, spaces, hyphens, parentheses, dots, plus sign
        // ============================================================================
        if (NonNumericRegex.IsMatch(input))
        {
            return Result<PhoneNumber>.Failure(
                "Invalid phone number format: contains unsupported characters. " +
                "Only digits, spaces, hyphens, parentheses, dots, and plus signs are allowed.");
        }

        // ============================================================================
        // NORMALIZATION: Extract only digits
        // BDD Scenario 1: Basic US Phone Number Formats
        // ============================================================================
        var digits = ExtractDigits(input);

        // ============================================================================
        // NORMALIZATION: Remove US country code (1) if present
        // BDD Scenario 1: E.164 formats with +1 prefix
        // ============================================================================
        if (digits.Length == 11 && digits[0] == '1')
        {
            digits = digits.Substring(1);
        }

        // Handle international format with 00 prefix
        if (digits.Length == 13 && digits.StartsWith("001"))
        {
            digits = digits.Substring(3);
        }

        // ============================================================================
        // VALIDATION 3: Length Check (Must be exactly 10 digits)
        // BDD Scenario 4: Invalid Phone Number Lengths
        // ============================================================================
        if (digits.Length != 10)
        {
            return Result<PhoneNumber>.Failure(
                $"Invalid phone number length: expected length is 10 digits, " +
                $"actual length is {digits.Length} digits");
        }

        // ============================================================================
        // VALIDATION 4: NANP First Digit Rule (Area Code)
        // BDD Scenario 3: NANP First Digit Rule (Must Be 2-9)
        // Area code first digit must be 2-9 (not 0 or 1)
        // ============================================================================
        char areaCodeFirstDigit = digits[0];
        if (areaCodeFirstDigit < '2' || areaCodeFirstDigit > '9')
        {
            return Result<PhoneNumber>.Failure(
                $"First digit must be 2-9: area code first digit is '{areaCodeFirstDigit}' " +
                $"(area code: {digits.Substring(0, 3)})");
        }

        // ============================================================================
        // VALIDATION 5: NANP N11 Rule (Area Code)
        // BDD Scenario 2: NANP N11 Rule Validation
        // Area code second digit cannot be 1 (N11 format reserved)
        // Examples: 211, 311, 411, 511, 611, 711, 811, 911
        // ============================================================================
        if (digits[1] == '1' && digits[2] == '1')
        {
            return Result<PhoneNumber>.Failure(
                $"N11 format reserved for special services: " +
                $"area code {digits.Substring(0, 3)} is not valid for regular phone numbers");
        }

        // ============================================================================
        // VALIDATION 6: NANP First Digit Rule (Exchange)
        // BDD Scenario 3: NANP First Digit Rule (Must Be 2-9)
        // Exchange first digit must be 2-9 (not 0 or 1)
        // ============================================================================
        char exchangeFirstDigit = digits[3];
        if (exchangeFirstDigit < '2' || exchangeFirstDigit > '9')
        {
            return Result<PhoneNumber>.Failure(
                $"First digit must be 2-9: exchange first digit is '{exchangeFirstDigit}' " +
                $"(exchange: {digits.Substring(3, 3)})");
        }

        // ============================================================================
        // VALIDATION 7: NANP N11 Rule (Exchange)
        // BDD Scenario 2: NANP N11 Rule Validation
        // Exchange second digit cannot be 1 (N11 format reserved)
        // Examples: 211, 311, 411, 511, 611, 711, 811, 911
        // ============================================================================
        if (digits[4] == '1' && digits[5] == '1')
        {
            return Result<PhoneNumber>.Failure(
                $"N11 format reserved for special services: " +
                $"exchange {digits.Substring(3, 3)} is not valid for regular phone numbers");
        }

        // ============================================================================
        // VALIDATION 8: Check for both area code AND exchange being N11 (edge case)
        // BDD Scenario 2: N11 in both area code AND exchange
        // ============================================================================
        bool areaCodeIsN11 = digits[1] == '1' && digits[2] == '1';
        bool exchangeIsN11 = digits[4] == '1' && digits[5] == '1';
        if (areaCodeIsN11 && exchangeIsN11)
        {
            return Result<PhoneNumber>.Failure(
                $"N11 format reserved for special services: " +
                $"both area code ({digits.Substring(0, 3)}) and " +
                $"exchange ({digits.Substring(3, 3)}) cannot be N11 format");
        }

        // All validations passed - create PhoneNumber value object
        return Result<PhoneNumber>.Success(new PhoneNumber(digits));
    }

    /// <summary>
    /// Extracts only digit characters from input string.
    /// Optimized for performance (<1ms requirement).
    /// </summary>
    private static string ExtractDigits(string input)
    {
        // Performance optimization: use StringBuilder for efficient string building
        // Avoids string concatenation overhead
        var matches = DigitOnlyRegex.Matches(input);
        if (matches.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder(matches.Count);
        foreach (Match match in matches)
        {
            sb.Append(match.Value);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Formats phone number for display (optional)
    /// </summary>
    public string ToFormattedString() =>
        $"({AreaCode}) {Exchange}-{LineNumber}";

    // ============================================================================
    // Value Object Equality
    // Two phone numbers are equal if their normalized values are equal
    // ============================================================================

    public bool Equals(PhoneNumber? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return NormalizedValue == other.NormalizedValue;
    }

    public override bool Equals(object? obj) =>
        obj is PhoneNumber other && Equals(other);

    public override int GetHashCode() =>
        NormalizedValue.GetHashCode();

    public static bool operator ==(PhoneNumber? left, PhoneNumber? right) =>
        Equals(left, right);

    public static bool operator !=(PhoneNumber? left, PhoneNumber? right) =>
        !Equals(left, right);

    public override string ToString() => NormalizedValue;
}
