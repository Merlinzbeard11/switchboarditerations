using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using EquifaxEnrichmentAPI.Infrastructure.Repositories;
using EquifaxEnrichmentAPI.Infrastructure.Persistence;
using EquifaxEnrichmentAPI.Domain.Entities;
using EquifaxEnrichmentAPI.Domain.ValueObjects;
using System.Reflection;

namespace EquifaxEnrichmentAPI.Tests.Unit.Infrastructure;

/// <summary>
/// Unit tests for EnrichmentRepository
/// Feature 1.3: Multi-Phone Search Database Enhancement
/// BDD File: features/phase1/feature-1.3-database-query-multi-phone.feature
///
/// 398-COLUMN SCHEMA - Updated for snake_case properties
/// Uses reflection to set properties for test data
///
/// TDD approach: Write failing tests first, then implement multi-column search
/// </summary>
public class EnrichmentRepositoryTests : IDisposable
{
    private readonly EnrichmentDbContext _context;
    private readonly EnrichmentRepository _repository;

    public EnrichmentRepositoryTests()
    {
        // Use in-memory database for unit testing
        var options = new DbContextOptionsBuilder<EnrichmentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new EnrichmentDbContext(options);
        _repository = new EnrichmentRepository(_context);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    // ================================================================
    // Feature 1.3: Multi-Column Phone Search (398-column schema)
    // Phone fields: mobile_phone_1-2, phone_1-5 (7 total)
    // ================================================================

    [Fact]
    public async Task FindByPhoneAsync_PhoneInMobilePhone1Column_ShouldFindRecord()
    {
        // Arrange - BDD Scenario 1: Primary phone match (100% confidence)
        var consumer = ConsumerEnrichment.CreateFromEquifaxCsv(
            consumerKey: "EQF_test_123",
            matchConfidence: 1.0,
            matchType: "phone_only");

        SetProperty(consumer, "mobile_phone_1", "8015551234");
        SetProperty(consumer, "first_name", "Bob");
        SetProperty(consumer, "last_name", "Barker");

        _context.ConsumerEnrichments.Add(consumer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindByPhoneAsync("8015551234");

        // Assert - Should find record when phone is in mobile_phone_1
        result.Should().NotBeNull();
        result!.IsMatch.Should().BeTrue("phone found in mobile_phone_1 column");
        result.Entity.Should().NotBeNull();
        result.Entity!.consumer_key.Should().Be("EQF_test_123");
        result.MatchedColumn.Should().Be(1, "phone was found in mobile_phone_1 column (index 1)");
        result.Confidence.Should().Be(1.00, "mobile_phone_1 match should be 100% confidence per BDD");
    }

    [Fact]
    public async Task FindByPhoneAsync_PhoneInMobilePhone2Column_ShouldFindRecord()
    {
        // Arrange - BDD Scenario 2: Secondary phone match (95% confidence)
        var consumer = ConsumerEnrichment.CreateFromEquifaxCsv(
            consumerKey: "EQF_test_456",
            matchConfidence: 0.95,
            matchType: "phone_only");

        // Set phone in mobile_phone_2 (not mobile_phone_1)
        SetProperty(consumer, "mobile_phone_2", "8015551234");

        _context.ConsumerEnrichments.Add(consumer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindByPhoneAsync("8015551234");

        // Assert - Should find record when phone is in mobile_phone_2
        result.Should().NotBeNull("repository should search mobile_phone_2 column");
        result!.IsMatch.Should().BeTrue("phone found in mobile_phone_2 column");
        result.Entity.Should().NotBeNull();
        result.Entity!.consumer_key.Should().Be("EQF_test_456");
        result.MatchedColumn.Should().Be(2, "phone was found in mobile_phone_2 column (index 2)");
        result.Confidence.Should().Be(0.95, "mobile_phone_2 match should be 95% confidence per BDD");
    }

    [Fact]
    public async Task FindByPhoneAsync_PhoneInPhone5Column_ShouldFindRecord()
    {
        // Arrange - BDD Scenario 2: Lower confidence phone match (70% confidence - index 7)
        var consumer = ConsumerEnrichment.CreateFromEquifaxCsv(
            consumerKey: "EQF_test_789",
            matchConfidence: 0.70,
            matchType: "phone_only");

        // Set phone in phone_5 (index 7: mobile_1, mobile_2, phone_1-4, phone_5)
        SetProperty(consumer, "phone_5", "8015551234");

        _context.ConsumerEnrichments.Add(consumer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindByPhoneAsync("8015551234");

        // Assert - Should find record even in phone_5
        result.Should().NotBeNull("repository should search phone_5 column");
        result!.IsMatch.Should().BeTrue("phone found in phone_5 column");
        result.Entity.Should().NotBeNull();
        result.Entity!.consumer_key.Should().Be("EQF_test_789");
        result.MatchedColumn.Should().Be(7, "phone was found in phone_5 column (index 7)");
        result.Confidence.Should().Be(0.70, "phone_5 match should be 70% confidence per BDD");
    }

    [Fact]
    public async Task FindByPhoneAsync_PhoneNotInAnyColumn_ShouldReturnNoMatch()
    {
        // Arrange - BDD Scenario 3: No match found
        // Create consumer with DIFFERENT phone
        var consumer = ConsumerEnrichment.CreateFromEquifaxCsv(
            consumerKey: "EQF_different",
            matchConfidence: 0.75,
            matchType: "phone_only");

        // Set different phone in mobile_phone_1
        SetProperty(consumer, "mobile_phone_1", "8015559999");

        _context.ConsumerEnrichments.Add(consumer);
        await _context.SaveChangesAsync();

        // Act - Search for phone that doesn't exist in ANY column
        var result = await _repository.FindByPhoneAsync("8015551234");

        // Assert - Should return no match result
        result.Should().NotBeNull("repository should return PhoneSearchResult even when no match");
        result!.IsMatch.Should().BeFalse("phone 8015551234 is not in any column");
        result.Entity.Should().BeNull();
        result.Confidence.Should().Be(0.0, "no match should have 0% confidence per BDD");
        result.MatchedColumn.Should().BeNull();
    }

    [Fact]
    public async Task FindByPhoneAsync_DuplicatePhoneAcrossColumns_ShouldReturnFirstMatch()
    {
        // Arrange - BDD Scenario 4: Duplicate phone returns highest confidence
        // Create two consumers with same phone in different columns
        var consumer1 = ConsumerEnrichment.CreateFromEquifaxCsv(
            consumerKey: "EQF_first",
            matchConfidence: 1.0,
            matchType: "phone_only");
        SetProperty(consumer1, "mobile_phone_1", "8015551234");

        var consumer2 = ConsumerEnrichment.CreateFromEquifaxCsv(
            consumerKey: "EQF_second",
            matchConfidence: 0.90,
            matchType: "phone_only");
        SetProperty(consumer2, "phone_3", "8015551234");

        _context.ConsumerEnrichments.AddRange(consumer1, consumer2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindByPhoneAsync("8015551234");

        // Assert - Should return first match (mobile_phone_1 has priority)
        result.Should().NotBeNull();
        result!.IsMatch.Should().BeTrue();
        result.Entity.Should().NotBeNull();
        result.Entity!.consumer_key.Should().Be("EQF_first",
            "mobile_phone_1 match should be returned first (highest confidence)");
        result.MatchedColumn.Should().Be(1, "first match was in mobile_phone_1 column");
        result.Confidence.Should().Be(1.00, "mobile_phone_1 match has 100% confidence");
    }

    /// <summary>
    /// Helper method to set properties using reflection.
    /// Required because ConsumerEnrichment has private setters for data integrity.
    /// </summary>
    private static void SetProperty<T>(ConsumerEnrichment entity, string propertyName, T value)
    {
        var property = typeof(ConsumerEnrichment).GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (property == null)
        {
            throw new InvalidOperationException($"Property '{propertyName}' not found on ConsumerEnrichment");
        }

        property.SetValue(entity, value);
    }
}
