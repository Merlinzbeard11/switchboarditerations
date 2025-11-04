using Xunit;
using FluentAssertions;
using EquifaxEnrichmentAPI.Domain.ValueObjects;
using EquifaxEnrichmentAPI.Domain.Entities;

namespace EquifaxEnrichmentAPI.Tests.Unit.Domain;

/// <summary>
/// TDD Unit tests for PhoneSearchResult value object
/// Feature 1.3 Slice 4: Confidence Scoring Based on Matched Column
/// BDD File: features/phase1/feature-1.3-database-query-multi-phone.feature
///
/// BDD Scenario 14: Confidence score formula testing (Lines 230-247)
/// Tests confidence calculation: 100 - ((column_index - 1) * 5) / 100
/// </summary>
public class PhoneSearchResultTests
{
    // ================================================================
    // BDD Scenario 14: Confidence Formula Testing
    // Formula: 100 - ((column_index - 1) * 5) as percentage, divided by 100 for decimal
    // ================================================================

    [Theory]
    [InlineData(1, 1.00)]  // Phone1: 100 - ((1-1)*5) = 100% = 1.00
    [InlineData(2, 0.95)]  // Phone2: 100 - ((2-1)*5) = 95% = 0.95
    [InlineData(3, 0.90)]  // Phone3: 100 - ((3-1)*5) = 90% = 0.90
    [InlineData(4, 0.85)]  // Phone4: 100 - ((4-1)*5) = 85% = 0.85
    [InlineData(5, 0.80)]  // Phone5: 100 - ((5-1)*5) = 80% = 0.80
    [InlineData(6, 0.75)]  // Phone6: 100 - ((6-1)*5) = 75% = 0.75
    [InlineData(7, 0.70)]  // Phone7: 100 - ((7-1)*5) = 70% = 0.70
    [InlineData(8, 0.65)]  // Phone8: 100 - ((8-1)*5) = 65% = 0.65
    [InlineData(9, 0.60)]  // Phone9: 100 - ((9-1)*5) = 60% = 0.60
    [InlineData(10, 0.55)] // Phone10: 100 - ((10-1)*5) = 55% = 0.55
    public void CreateMatch_CalculatesConfidenceUsingFormula_ForEachColumn(int columnIndex, double expectedConfidence)
    {
        // Arrange - Create test entity (398-column schema)
        var entity = ConsumerEnrichment.CreateFromEquifaxCsv(
            consumerKey: "EQF_test",
            matchConfidence: 0.50, // This will be IGNORED - confidence comes from column index
            matchType: "phone_only");

        // Act - Create result with specific column match
        var result = PhoneSearchResult.CreateMatch(entity, columnIndex);

        // Assert - Verify confidence matches BDD formula
        result.Confidence.Should().Be(expectedConfidence,
            $"Phone{columnIndex} should have confidence {expectedConfidence} per BDD Scenario 14");
        result.MatchedColumn.Should().Be(columnIndex);
        result.MatchedColumnName.Should().Be($"Phone{columnIndex}");
        result.IsMatch.Should().BeTrue();
    }

    [Fact]
    public void CreateMatch_Phone1Match_Returns100PercentConfidence()
    {
        // Arrange - BDD Scenario 1: Phone1 match = 100% confidence (Line 22)
        var entity = ConsumerEnrichment.CreateFromEquifaxCsv(
            consumerKey: "EQF_test",
            matchConfidence: 0.5,
            matchType: "phone_only");

        // Act
        var result = PhoneSearchResult.CreateMatch(entity, 1);

        // Assert
        result.Confidence.Should().Be(1.00, "Phone1 match should be 100% confidence");
        result.MatchedColumn.Should().Be(1);
        result.MatchedColumnName.Should().Be("Phone1");
    }

    [Fact]
    public void CreateMatch_Phone10Match_Returns55PercentConfidence()
    {
        // Arrange - BDD Scenario 2: Phone10 match = 55% confidence (Line 50)
        var entity = ConsumerEnrichment.CreateFromEquifaxCsv(
            consumerKey: "EQF_test",
            matchConfidence: 0.5,
            matchType: "phone_only");

        // Act
        var result = PhoneSearchResult.CreateMatch(entity, 10);

        // Assert
        result.Confidence.Should().Be(0.55, "Phone10 match should be 55% confidence");
        result.MatchedColumn.Should().Be(10);
        result.MatchedColumnName.Should().Be("Phone10");
    }

    [Fact]
    public void CreateMatch_WithNullEntity_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => PhoneSearchResult.CreateMatch(null!, 1);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("entity");
    }

    [Theory]
    [InlineData(0)]   // Below valid range
    [InlineData(11)]  // Above valid range
    [InlineData(-1)]  // Negative
    [InlineData(100)] // Way out of range
    public void CreateMatch_WithInvalidColumnIndex_ThrowsArgumentOutOfRangeException(int invalidColumnIndex)
    {
        // Arrange
        var entity = ConsumerEnrichment.CreateFromEquifaxCsv(
            consumerKey: "EQF_test",
            matchConfidence: 0.5,
            matchType: "phone_only");

        // Act & Assert
        var act = () => PhoneSearchResult.CreateMatch(entity, invalidColumnIndex);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("matchedColumnIndex")
            .WithMessage("*must be between 1 and 10*");
    }

    [Fact]
    public void CreateLegacyMatch_UsesEntityStoredConfidence()
    {
        // Arrange - Legacy NormalizedPhone match uses stored confidence
        var entity = ConsumerEnrichment.CreateFromEquifaxCsv(
            consumerKey: "EQF_test",
            matchConfidence: 0.87, // This confidence WILL be used for legacy matches
            matchType: "phone_only");

        // Act
        var result = PhoneSearchResult.CreateLegacyMatch(entity);

        // Assert - Legacy match uses entity's confidence, not formula-based
        result.Confidence.Should().Be(0.87, "Legacy match should use entity's stored confidence");
        result.MatchedColumn.Should().BeNull("Legacy column has no index");
        result.MatchedColumnName.Should().Be("NormalizedPhone");
        result.IsMatch.Should().BeTrue();
    }

    [Fact]
    public void CreateLegacyMatch_WithNullEntity_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => PhoneSearchResult.CreateLegacyMatch(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("entity");
    }

    [Fact]
    public void CreateNoMatch_ReturnsZeroConfidenceWithNullEntity()
    {
        // Arrange & Act - BDD Scenario 3: No match found (Line 60)
        var result = PhoneSearchResult.CreateNoMatch();

        // Assert
        result.Entity.Should().BeNull("No match means no entity");
        result.Confidence.Should().Be(0.0, "No match should have 0% confidence");
        result.MatchedColumn.Should().BeNull("No match means no column");
        result.MatchedColumnName.Should().BeNull("No match means no column name");
        result.IsMatch.Should().BeFalse();
    }

    [Fact]
    public void IsMatch_WhenEntityExists_ReturnsTrue()
    {
        // Arrange
        var entity = ConsumerEnrichment.CreateFromEquifaxCsv(
            consumerKey: "EQF_test",
            matchConfidence: 0.5,
            matchType: "phone_only");

        // Act
        var result = PhoneSearchResult.CreateMatch(entity, 3);

        // Assert
        result.IsMatch.Should().BeTrue();
    }

    [Fact]
    public void IsMatch_WhenNoMatch_ReturnsFalse()
    {
        // Arrange & Act
        var result = PhoneSearchResult.CreateNoMatch();

        // Assert
        result.IsMatch.Should().BeFalse();
    }

    // ================================================================
    // BDD Scenario 8: Match Column Determination Logic (Lines 127-155)
    // Verifies MatchedColumnName property is set correctly
    // ================================================================

    [Theory]
    [InlineData(1, "Phone1")]
    [InlineData(2, "Phone2")]
    [InlineData(3, "Phone3")]
    [InlineData(4, "Phone4")]
    [InlineData(5, "Phone5")]
    [InlineData(6, "Phone6")]
    [InlineData(7, "Phone7")]
    [InlineData(8, "Phone8")]
    [InlineData(9, "Phone9")]
    [InlineData(10, "Phone10")]
    public void CreateMatch_SetsMatchedColumnNameCorrectly(int columnIndex, string expectedName)
    {
        // Arrange
        var entity = ConsumerEnrichment.CreateFromEquifaxCsv(
            consumerKey: "EQF_test",
            matchConfidence: 0.5,
            matchType: "phone_only");

        // Act
        var result = PhoneSearchResult.CreateMatch(entity, columnIndex);

        // Assert - BDD Scenario 8: System determines which column matched
        result.MatchedColumnName.Should().Be(expectedName,
            $"Column {columnIndex} should have name '{expectedName}' for logging");
    }
}
