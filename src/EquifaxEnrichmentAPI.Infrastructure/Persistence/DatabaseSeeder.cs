using EquifaxEnrichmentAPI.Domain.Entities;
using EquifaxEnrichmentAPI.Domain.ValueObjects;
using System.Reflection;

namespace EquifaxEnrichmentAPI.Infrastructure.Persistence;

/// <summary>
/// Seeds database with test data for BDD scenarios.
/// Provides known test data for integration testing and development.
///
/// 398-COLUMN SCHEMA - Updated for individual field structure
/// Uses reflection to set private properties for test data.
///
/// BDD Test Data Requirements:
/// - Phone 8015551234: Successful lookup (Scenarios 1-3, 10)
/// - Phone 5559999999: No match (Scenario 4) - NOT SEEDED
/// - Various match confidence levels for testing
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds the database with test consumer enrichment records.
    /// Idempotent: Can be run multiple times safely (checks for existing data).
    /// </summary>
    public static async Task SeedAsync(EnrichmentDbContext context)
    {
        // Check if data already exists
        if (context.ConsumerEnrichments.Any())
        {
            Console.WriteLine("Database already seeded. Skipping seed operation.");
            return;
        }

        Console.WriteLine("Seeding database with 398-column test data...");

        // ====================================================================
        // BDD SCENARIO 1-3, 10: Bob Barker - 8015551234
        // High-quality consumer record for testing successful enrichment
        // mobile_phone_1 (100% confidence - index 1)
        // ====================================================================
        var consumer1 = ConsumerEnrichment.CreateFromEquifaxCsv(
            consumerKey: "EQF_" + Guid.NewGuid().ToString("N").Substring(0, 24).ToUpper(),
            matchConfidence: 1.0, // 100% confidence
            matchType: "phone_only"
        );

        // Set personal info fields
        SetProperty(consumer1, "first_name", "Bob");
        SetProperty(consumer1, "last_name", "Barker");
        SetProperty(consumer1, "middle_name", "W");
        SetProperty(consumer1, "date_of_birth", "1950-12-25");
        SetProperty(consumer1, "gender", "M");
        SetProperty(consumer1, "age", "73");

        // Set phone field - mobile_phone_1 (100% confidence)
        SetProperty(consumer1, "mobile_phone_1", "8015551234");

        // Set address fields (current address)
        SetProperty(consumer1, "address_1", "123 Main St");
        SetProperty(consumer1, "city_name_1", "Bountiful");
        SetProperty(consumer1, "state_abbreviation_1", "UT");
        SetProperty(consumer1, "zip_1", "84010");

        // Set data freshness
        SetProperty(consumer1, "data_freshness_date", DateTimeOffset.UtcNow.AddDays(-7));

        await context.ConsumerEnrichments.AddAsync(consumer1);

        // ====================================================================
        // ADDITIONAL TEST DATA: Alice Smith - 3105552000
        // High confidence record with recent data
        // mobile_phone_2 (95% confidence - index 2)
        // ====================================================================
        var consumer2 = ConsumerEnrichment.CreateFromEquifaxCsv(
            consumerKey: "EQF_" + Guid.NewGuid().ToString("N").Substring(0, 24).ToUpper(),
            matchConfidence: 0.95, // 95% confidence
            matchType: "phone_with_name_and_address"
        );

        // Set personal info fields
        SetProperty(consumer2, "first_name", "Alice");
        SetProperty(consumer2, "last_name", "Smith");
        SetProperty(consumer2, "age", "42");

        // Set phone field - mobile_phone_2 (95% confidence)
        SetProperty(consumer2, "mobile_phone_2", "3105552000");

        // Set address fields
        SetProperty(consumer2, "address_1", "789 Pine Rd");
        SetProperty(consumer2, "city_name_1", "Los Angeles");
        SetProperty(consumer2, "state_abbreviation_1", "CA");
        SetProperty(consumer2, "zip_1", "90001");

        // Set data freshness
        SetProperty(consumer2, "data_freshness_date", DateTimeOffset.UtcNow.AddDays(-1));

        await context.ConsumerEnrichments.AddAsync(consumer2);

        // ====================================================================
        // ADDITIONAL TEST DATA: John Doe - 2125553000
        // Lower confidence record with older data
        // phone_5 (70% confidence - index 7: mobile_1, mobile_2, phone_1-4, phone_5)
        // ====================================================================
        var consumer3 = ConsumerEnrichment.CreateFromEquifaxCsv(
            consumerKey: "EQF_" + Guid.NewGuid().ToString("N").Substring(0, 24).ToUpper(),
            matchConfidence: 0.70, // 70% confidence
            matchType: "phone_only"
        );

        // Set personal info fields
        SetProperty(consumer3, "first_name", "John");
        SetProperty(consumer3, "last_name", "Doe");

        // Set phone field - phone_5 (70% confidence - index 7)
        SetProperty(consumer3, "phone_5", "2125553000");

        // Set address fields
        SetProperty(consumer3, "city_name_1", "New York");
        SetProperty(consumer3, "state_abbreviation_1", "NY");
        SetProperty(consumer3, "zip_1", "10001");

        // Set data freshness
        SetProperty(consumer3, "data_freshness_date", DateTimeOffset.UtcNow.AddDays(-90));

        await context.ConsumerEnrichments.AddAsync(consumer3);

        // Save all changes
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Seeded 3 test consumer enrichment records (398-column schema)");
        Console.WriteLine($"   - 8015551234: Bob Barker (mobile_phone_1 = 100% confidence)");
        Console.WriteLine($"   - 3105552000: Alice Smith (mobile_phone_2 = 95% confidence)");
        Console.WriteLine($"   - 2125553000: John Doe (phone_5 = 70% confidence)");
        Console.WriteLine($"   - 5559999999: NOT SEEDED (for no-match scenario testing)");
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
            Console.WriteLine($"⚠️  Warning: Property '{propertyName}' not found on ConsumerEnrichment");
            return; // Gracefully skip missing properties (398 fields, some may not be in test entity yet)
        }

        property.SetValue(entity, value);
    }
}
