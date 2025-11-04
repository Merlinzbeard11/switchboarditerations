using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using EquifaxEnrichmentAPI.Infrastructure.Persistence;

namespace EquifaxEnrichmentAPI.Tests.Integration.Infrastructure;

/// <summary>
/// Integration tests for database seed data validation.
/// Feature 1.3 Slice 5: Verify Phone1-Phone10 columns are populated
/// BDD File: features/phase1/feature-1.3-database-query-multi-phone.feature
///
/// TDD RED Phase: These tests validate seed data has Phone1-Phone10 populated.
/// Tests will FAIL until DatabaseSeeder is updated.
/// </summary>
public class SeedDataValidationTests : IAsyncLifetime
{
    private EnrichmentDbContext _context = null!;

    public async Task InitializeAsync()
    {
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<EnrichmentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new EnrichmentDbContext(options);

        // Run seeder to populate test data
        await DatabaseSeeder.SeedAsync(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    // ====================================================================
    // Feature 1.3 Slice 5: Validate Phone1-Phone10 Population
    // BDD Scenario: Seed data should have phones distributed across columns
    // ====================================================================

    [Fact]
    public async Task SeedData_Consumer1_ShouldHavePhoneInPhone1Column()
    {
        // Arrange - BDD: Primary test record (8015551234) should have phone in Phone1
        var expectedPhone = "8015551234";

        // Act - Find consumer by normalized_phone (client-side evaluation - computed property)
        var consumers = await _context.ConsumerEnrichments.ToListAsync();
        var consumer = consumers.FirstOrDefault(e => e.normalized_phone == expectedPhone);

        // Assert - mobile_phone_1 should be populated
        consumer.Should().NotBeNull("seed data should contain 8015551234");

        // Use reflection to access private mobile_phone_1 property
        var phone1Property = typeof(Domain.Entities.ConsumerEnrichment).GetProperty("mobile_phone_1");
        var phone1Value = phone1Property!.GetValue(consumer) as string;

        phone1Value.Should().Be(expectedPhone,
            "primary test record should have phone in mobile_phone_1 column for 100% confidence");
    }

    [Fact]
    public async Task SeedData_Consumer2_ShouldHavePhoneInPhone2Column()
    {
        // Arrange - BDD: Second test record (3105552000) should have phone in Phone2
        var expectedPhone = "3105552000";

        // Act - Client-side evaluation for computed property
        var consumers = await _context.ConsumerEnrichments.ToListAsync();
        var consumer = consumers.FirstOrDefault(e => e.normalized_phone == expectedPhone);

        // Assert - mobile_phone_2 should be populated
        consumer.Should().NotBeNull("seed data should contain 3105552000");

        var phone2Property = typeof(Domain.Entities.ConsumerEnrichment).GetProperty("mobile_phone_2");
        var phone2Value = phone2Property!.GetValue(consumer) as string;

        phone2Value.Should().Be(expectedPhone,
            "second test record should have phone in mobile_phone_2 column for 95% confidence");
    }

    [Fact]
    public async Task SeedData_Consumer3_ShouldHavePhoneInPhone5Column()
    {
        // Arrange - BDD: Third test record (2125553000) should have phone in phone_5
        var expectedPhone = "2125553000";

        // Act - Client-side evaluation for computed property
        var consumers = await _context.ConsumerEnrichments.ToListAsync();
        var consumer = consumers.FirstOrDefault(e => e.normalized_phone == expectedPhone);

        // Assert - phone_5 should be populated (70% confidence - index 7)
        consumer.Should().NotBeNull("seed data should contain 2125553000");

        var phone5Property = typeof(Domain.Entities.ConsumerEnrichment).GetProperty("phone_5");
        var phone5Value = phone5Property!.GetValue(consumer) as string;

        phone5Value.Should().Be(expectedPhone,
            "third test record should have phone in phone_5 column for 70% confidence");
    }

    [Fact]
    public async Task SeedData_AllConsumers_ShouldHaveAtLeastOnePhoneColumnPopulated()
    {
        // Arrange & Act - Get all seeded consumers
        var consumers = await _context.ConsumerEnrichments.ToListAsync();

        // Assert - Should have 3 seeded consumers
        consumers.Should().HaveCount(3, "DatabaseSeeder seeds 3 test records");

        // Verify each consumer has at least one phone column populated (mobile_phone_1-2 or phone_1-5)
        foreach (var consumer in consumers)
        {
            var hasPhoneColumn = false;
            var phoneProperties = new[] { "mobile_phone_1", "mobile_phone_2", "phone_1", "phone_2", "phone_3", "phone_4", "phone_5" };

            foreach (var propName in phoneProperties)
            {
                var property = typeof(Domain.Entities.ConsumerEnrichment).GetProperty(propName);
                var value = property!.GetValue(consumer) as string;

                if (!string.IsNullOrEmpty(value))
                {
                    hasPhoneColumn = true;
                    break;
                }
            }

            hasPhoneColumn.Should().BeTrue(
                $"Consumer {consumer.consumer_key} should have at least one phone column populated");
        }
    }

    [Fact]
    public async Task SeedData_BackwardCompatibility_NormalizedPhoneShouldStillBePopulated()
    {
        // Arrange & Act - Verify NormalizedPhone still populated for backward compatibility
        var consumers = await _context.ConsumerEnrichments.ToListAsync();

        // Assert - All consumers should still have NormalizedPhone
        consumers.Should().HaveCount(3);

        foreach (var consumer in consumers)
        {
            consumer.normalized_phone.Should().NotBeNullOrEmpty(
                $"Consumer {consumer.consumer_key} should have normalized_phone for backward compatibility");
            consumer.normalized_phone.Should().HaveLength(10,
                "normalized_phone should be 10 digits");
        }
    }

    [Fact]
    public async Task SeedData_DiverseColumnDistribution_ShouldCoverMultipleColumns()
    {
        // Arrange & Act - Verify phones are distributed across different columns
        var consumers = await _context.ConsumerEnrichments.ToListAsync();
        var populatedColumns = new HashSet<string>();

        // Assert - Check which columns are populated across all consumers
        var phoneColumns = new[] { "mobile_phone_1", "mobile_phone_2", "phone_1", "phone_2", "phone_3", "phone_4", "phone_5" };

        foreach (var consumer in consumers)
        {
            foreach (var columnName in phoneColumns)
            {
                var property = typeof(Domain.Entities.ConsumerEnrichment).GetProperty(columnName);
                var value = property!.GetValue(consumer) as string;

                if (!string.IsNullOrEmpty(value))
                {
                    populatedColumns.Add(columnName);
                }
            }
        }

        populatedColumns.Should().HaveCountGreaterThanOrEqualTo(3,
            "Seed data should use at least 3 different phone columns for test coverage");

        populatedColumns.Should().Contain("mobile_phone_1", "Should have mobile_phone_1 (100% confidence) test case");
        populatedColumns.Should().Contain("phone_5", "Should have phone_5 (70% confidence) test case");
    }
}
