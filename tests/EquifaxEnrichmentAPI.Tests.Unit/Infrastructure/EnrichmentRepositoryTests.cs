using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using EquifaxEnrichmentAPI.Infrastructure.Repositories;
using EquifaxEnrichmentAPI.Infrastructure.Persistence;
using EquifaxEnrichmentAPI.Domain.Entities;
using EquifaxEnrichmentAPI.Domain.ValueObjects;

namespace EquifaxEnrichmentAPI.Tests.Unit.Infrastructure;

/// <summary>
/// Unit tests for EnrichmentRepository
/// Feature 1.3: Multi-Phone Search Database Enhancement
/// BDD File: features/phase1/feature-1.3-database-query-multi-phone.feature
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
    // Feature 1.3 Slice 3: Multi-Column Phone Search
    // TDD RED: These tests should FAIL before repository implementation
    // ================================================================

    [Fact]
    public async Task FindByPhoneAsync_PhoneInPhone1Column_ShouldFindRecord()
    {
        // Arrange - BDD Scenario 1: Primary phone match (100% confidence)
        var phone = PhoneNumber.Create("8015551234").Value;
        var consumer = ConsumerEnrichment.Create(
            phone,
            "EQF_test_123",
            1.0,
            "phone_only",
            DateTime.UtcNow.AddDays(-7),
            "{\"first_name\": \"Bob\", \"last_name\": \"Barker\"}",
            "[]",
            "[]",
            "{}");

        // Manually set Phone1 using reflection (since entity has private setters)
        var phone1Property = typeof(ConsumerEnrichment).GetProperty("Phone1");
        phone1Property!.SetValue(consumer, "8015551234");

        _context.ConsumerEnrichments.Add(consumer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindByPhoneAsync("8015551234");

        // Assert - Should find record when phone is in Phone1
        // BDD Scenario 1: Phone1 match returns 100% confidence (Line 22)
        result.Should().NotBeNull();
        result!.IsMatch.Should().BeTrue("phone found in Phone1 column");
        result.Entity.Should().NotBeNull();
        result.Entity!.ConsumerKey.Should().Be("EQF_test_123");
        result.MatchedColumn.Should().Be(1, "phone was found in Phone1 column");
        result.Confidence.Should().Be(1.00, "Phone1 match should be 100% confidence per BDD");
        result.MatchedColumnName.Should().Be("Phone1");
    }

    [Fact]
    public async Task FindByPhoneAsync_PhoneInPhone2Column_ShouldFindRecord()
    {
        // Arrange - BDD Scenario 2: Secondary phone match (95% confidence)
        var phone = PhoneNumber.Create("8015551234").Value;
        var consumer = ConsumerEnrichment.Create(
            phone,
            "EQF_test_456",
            0.95,
            "phone_only",
            DateTime.UtcNow.AddDays(-7));

        // Set phone in Phone2 (not Phone1)
        var phone2Property = typeof(ConsumerEnrichment).GetProperty("Phone2");
        phone2Property!.SetValue(consumer, "8015551234");

        _context.ConsumerEnrichments.Add(consumer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindByPhoneAsync("8015551234");

        // Assert - Should find record when phone is in Phone2
        // BDD Scenario 2: Phone2 match returns 95% confidence (Line 42)
        result.Should().NotBeNull("repository should search Phone2 column");
        result!.IsMatch.Should().BeTrue("phone found in Phone2 column");
        result.Entity.Should().NotBeNull();
        result.Entity!.ConsumerKey.Should().Be("EQF_test_456");
        result.MatchedColumn.Should().Be(2, "phone was found in Phone2 column");
        result.Confidence.Should().Be(0.95, "Phone2 match should be 95% confidence per BDD");
        result.MatchedColumnName.Should().Be("Phone2");
    }

    [Fact]
    public async Task FindByPhoneAsync_PhoneInPhone10Column_ShouldFindRecord()
    {
        // Arrange - BDD Scenario 2: Least recent phone match (55% confidence)
        var phone = PhoneNumber.Create("8015551234").Value;
        var consumer = ConsumerEnrichment.Create(
            phone,
            "EQF_test_789",
            0.55,
            "phone_only",
            DateTime.UtcNow.AddDays(-7));

        // Set phone in Phone10 (last column)
        var phone10Property = typeof(ConsumerEnrichment).GetProperty("Phone10");
        phone10Property!.SetValue(consumer, "8015551234");

        _context.ConsumerEnrichments.Add(consumer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindByPhoneAsync("8015551234");

        // Assert - Should find record even in Phone10
        // BDD Scenario 2: Phone10 match returns 55% confidence (Line 50)
        result.Should().NotBeNull("repository should search Phone10 column");
        result!.IsMatch.Should().BeTrue("phone found in Phone10 column");
        result.Entity.Should().NotBeNull();
        result.Entity!.ConsumerKey.Should().Be("EQF_test_789");
        result.MatchedColumn.Should().Be(10, "phone was found in Phone10 column");
        result.Confidence.Should().Be(0.55, "Phone10 match should be 55% confidence per BDD");
        result.MatchedColumnName.Should().Be("Phone10");
    }

    [Fact]
    public async Task FindByPhoneAsync_PhoneNotInAnyColumn_ShouldReturnNull()
    {
        // Arrange - BDD Scenario 3: No match found
        // Create consumer with DIFFERENT phone in NormalizedPhone AND all Phone columns
        var differentPhone = PhoneNumber.Create("8015559999").Value;
        var consumer = ConsumerEnrichment.Create(
            differentPhone,  // NormalizedPhone will be 8015559999
            "EQF_different",
            0.75,
            "phone_only",
            DateTime.UtcNow.AddDays(-7));

        // Set different phone in Phone1 as well
        var phone1Property = typeof(ConsumerEnrichment).GetProperty("Phone1");
        phone1Property!.SetValue(consumer, "8015559999");

        _context.ConsumerEnrichments.Add(consumer);
        await _context.SaveChangesAsync();

        // Act - Search for phone that doesn't exist in ANY column
        var result = await _repository.FindByPhoneAsync("8015551234");

        // Assert - Should return no match result when phone not in any column
        // BDD Scenario 3: No match found returns null entity with 0.0 confidence (Line 60)
        result.Should().NotBeNull("repository should return PhoneSearchResult even when no match");
        result!.IsMatch.Should().BeFalse("phone 8015551234 is not in NormalizedPhone, Phone1, or any other column");
        result.Entity.Should().BeNull();
        result.Confidence.Should().Be(0.0, "no match should have 0% confidence per BDD");
        result.MatchedColumn.Should().BeNull();
        result.MatchedColumnName.Should().BeNull();
    }

    [Fact]
    public async Task FindByPhoneAsync_DuplicatePhoneAcrossColumns_ShouldReturnFirstMatch()
    {
        // Arrange - BDD Scenario 4: Duplicate phone returns highest confidence
        var phone = PhoneNumber.Create("8015551234").Value;

        // Create two consumers with same phone in different columns
        var consumer1 = ConsumerEnrichment.Create(
            phone,
            "EQF_first",
            1.0,
            "phone_only",
            DateTime.UtcNow.AddDays(-7));
        var phone1Property = typeof(ConsumerEnrichment).GetProperty("Phone1");
        phone1Property!.SetValue(consumer1, "8015551234");

        var consumer2 = ConsumerEnrichment.Create(
            phone,
            "EQF_second",
            0.90,
            "phone_only",
            DateTime.UtcNow.AddDays(-8));
        var phone3Property = typeof(ConsumerEnrichment).GetProperty("Phone3");
        phone3Property!.SetValue(consumer2, "8015551234");

        _context.ConsumerEnrichments.AddRange(consumer1, consumer2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindByPhoneAsync("8015551234");

        // Assert - Should return first match (Phone1 has priority)
        // BDD Scenario 4: Duplicate phone returns first match with highest confidence (Line 77)
        result.Should().NotBeNull();
        result!.IsMatch.Should().BeTrue();
        result.Entity.Should().NotBeNull();
        result.Entity!.ConsumerKey.Should().Be("EQF_first",
            "Phone1 match should be returned first (highest confidence)");
        result.MatchedColumn.Should().Be(1, "first match was in Phone1 column");
        result.Confidence.Should().Be(1.00, "Phone1 match has 100% confidence");
        result.MatchedColumnName.Should().Be("Phone1");
    }
}
