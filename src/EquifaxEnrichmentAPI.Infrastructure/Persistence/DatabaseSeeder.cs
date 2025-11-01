using EquifaxEnrichmentAPI.Domain.Entities;
using EquifaxEnrichmentAPI.Domain.ValueObjects;

namespace EquifaxEnrichmentAPI.Infrastructure.Persistence;

/// <summary>
/// Seeds database with test data for BDD scenarios.
/// Provides known test data for integration testing and development.
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

        Console.WriteLine("Seeding database with test data...");

        // ====================================================================
        // BDD SCENARIO 1-3, 10: Successful lookup for phone 8015551234
        // High-quality consumer record for testing successful enrichment
        // Feature 1.3 Slice 5: Populate Phone1 column for 100% confidence testing
        // ====================================================================
        var testPhone1 = PhoneNumber.Create("8015551234").Value;
        var consumer1 = ConsumerEnrichment.Create(
            testPhone1,
            "EQF_" + Guid.NewGuid().ToString("N").Substring(0, 24).ToUpper(), // 24-char key
            0.75, // Base confidence (ignored - will use Phone1 = 100% confidence)
            "phone_only",
            DateTime.UtcNow.AddDays(-7), // Data refreshed 7 days ago
            personalInfoJson: @"{
                ""first_name"": ""Bob"",
                ""last_name"": ""Barker"",
                ""middle_initial"": ""W"",
                ""date_of_birth"": ""1950-12-25"",
                ""ssn_last_4"": ""1234"",
                ""gender"": ""M"",
                ""age"": 73
            }",
            addressesJson: @"[
                {
                    ""address_type"": ""current"",
                    ""street"": ""123 Main St"",
                    ""unit"": """",
                    ""city"": ""Bountiful"",
                    ""state"": ""UT"",
                    ""postal_code"": ""84010"",
                    ""county"": ""Davis"",
                    ""country"": ""USA"",
                    ""residence_type"": ""single_family"",
                    ""ownership"": ""owner"",
                    ""move_in_date"": ""2015-03-15"",
                    ""confidence"": 0.95
                },
                {
                    ""address_type"": ""previous"",
                    ""street"": ""456 Oak Ave"",
                    ""city"": ""Salt Lake City"",
                    ""state"": ""UT"",
                    ""postal_code"": ""84101"",
                    ""move_in_date"": ""2010-06-01"",
                    ""move_out_date"": ""2015-03-14"",
                    ""confidence"": 0.85
                }
            ]",
            phonesJson: @"[
                {
                    ""phone"": ""8015551234"",
                    ""phone_type"": ""mobile"",
                    ""carrier"": ""Verizon Wireless"",
                    ""is_active"": true,
                    ""first_seen"": ""2015-03-15"",
                    ""last_verified"": ""2025-10-24"",
                    ""confidence"": 0.98
                },
                {
                    ""phone"": ""8015559876"",
                    ""phone_type"": ""landline"",
                    ""carrier"": ""CenturyLink"",
                    ""is_active"": false,
                    ""first_seen"": ""2010-06-01"",
                    ""last_verified"": ""2015-03-14"",
                    ""confidence"": 0.75
                }
            ]",
            financialJson: @"{
                ""credit_score"": 720,
                ""credit_score_date"": ""2025-10-01"",
                ""credit_score_provider"": ""Equifax"",
                ""estimated_income"": 85000,
                ""income_source"": ""modeled"",
                ""homeowner"": true,
                ""estimated_home_value"": 450000,
                ""mortgage_balance"": 280000,
                ""num_open_credit_accounts"": 5,
                ""num_derogatory_marks"": 0,
                ""bankruptcy_indicator"": false,
                ""foreclosure_indicator"": false
            }"
        );

        // Feature 1.3 Slice 5: Populate Phone1 using reflection (100% confidence)
        SetPhoneColumn(consumer1, 1, "8015551234");

        await context.ConsumerEnrichments.AddAsync(consumer1);

        // ====================================================================
        // ADDITIONAL TEST DATA: Various scenarios
        // ====================================================================

        // High confidence record with recent data
        // Feature 1.3 Slice 5: Populate Phone2 for 95% confidence testing
        var testPhone2 = PhoneNumber.Create("3105552000").Value;
        var consumer2 = ConsumerEnrichment.Create(
            testPhone2,
            "EQF_" + Guid.NewGuid().ToString("N").Substring(0, 24).ToUpper(),
            0.95, // High base confidence (ignored - will use Phone2 = 95% confidence)
            "phone_with_name_and_address",
            DateTime.UtcNow.AddDays(-1), // Very fresh data
            personalInfoJson: @"{""first_name"": ""Alice"", ""last_name"": ""Smith"", ""age"": 42}",
            addressesJson: @"[{""street"": ""789 Pine Rd"", ""city"": ""Los Angeles"", ""state"": ""CA"", ""postal_code"": ""90001""}]",
            phonesJson: @"[{""phone"": ""3105552000"", ""phone_type"": ""mobile"", ""is_active"": true}]",
            financialJson: @"{""credit_score"": 800, ""estimated_income"": 120000}"
        );

        // Feature 1.3 Slice 5: Populate Phone2 using reflection (95% confidence)
        SetPhoneColumn(consumer2, 2, "3105552000");

        await context.ConsumerEnrichments.AddAsync(consumer2);

        // Lower confidence record with older data
        // Feature 1.3 Slice 5: Populate Phone10 for 55% confidence testing (lowest)
        var testPhone3 = PhoneNumber.Create("2125553000").Value;
        var consumer3 = ConsumerEnrichment.Create(
            testPhone3,
            "EQF_" + Guid.NewGuid().ToString("N").Substring(0, 24).ToUpper(),
            0.60, // Lower confidence (ignored - will use Phone10 = 55% confidence)
            "phone_only",
            DateTime.UtcNow.AddDays(-90), // Older data (3 months)
            personalInfoJson: @"{""first_name"": ""John"", ""last_name"": ""Doe""}",
            addressesJson: @"[{""city"": ""New York"", ""state"": ""NY"", ""postal_code"": ""10001""}]",
            phonesJson: @"[{""phone"": ""2125553000"", ""phone_type"": ""landline""}]",
            financialJson: @"{""credit_score"": 650}"
        );

        // Feature 1.3 Slice 5: Populate Phone10 using reflection (55% confidence - lowest)
        SetPhoneColumn(consumer3, 10, "2125553000");

        await context.ConsumerEnrichments.AddAsync(consumer3);

        // Save all changes
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Seeded {3} test consumer enrichment records");
        Console.WriteLine($"   - 8015551234: Bob Barker (Phone1 = 100% confidence)");
        Console.WriteLine($"   - 3105552000: Alice Smith (Phone2 = 95% confidence)");
        Console.WriteLine($"   - 2125553000: John Doe (Phone10 = 55% confidence)");
        Console.WriteLine($"   - 5559999999: NOT SEEDED (for no-match scenario testing)");
        Console.WriteLine($"   Feature 1.3 Slice 5: Phone1-Phone10 columns populated for confidence testing");
    }

    /// <summary>
    /// Helper method to set Phone1-Phone10 using reflection.
    /// Feature 1.3 Slice 5: ConsumerEnrichment has private setters, so we use reflection.
    /// </summary>
    private static void SetPhoneColumn(ConsumerEnrichment entity, int columnIndex, string phoneValue)
    {
        if (columnIndex < 1 || columnIndex > 10)
            throw new ArgumentOutOfRangeException(nameof(columnIndex), "Column index must be 1-10");

        var propertyName = $"Phone{columnIndex}";
        var property = typeof(ConsumerEnrichment).GetProperty(propertyName);

        if (property == null)
            throw new InvalidOperationException($"Property {propertyName} not found on ConsumerEnrichment");

        property.SetValue(entity, phoneValue);
    }
}
