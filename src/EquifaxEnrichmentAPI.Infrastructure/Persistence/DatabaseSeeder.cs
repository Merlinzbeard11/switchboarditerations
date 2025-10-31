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
        // ====================================================================
        var testPhone1 = PhoneNumber.Create("8015551234").Value;
        var consumer1 = ConsumerEnrichment.Create(
            testPhone1,
            "EQF_" + Guid.NewGuid().ToString("N").Substring(0, 24).ToUpper(), // 24-char key
            0.75, // Base confidence (will be enhanced by optional fields in tests)
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

        await context.ConsumerEnrichments.AddAsync(consumer1);

        // ====================================================================
        // ADDITIONAL TEST DATA: Various scenarios
        // ====================================================================

        // High confidence record with recent data
        var testPhone2 = PhoneNumber.Create("3105552000").Value;
        var consumer2 = ConsumerEnrichment.Create(
            testPhone2,
            "EQF_" + Guid.NewGuid().ToString("N").Substring(0, 24).ToUpper(),
            0.95, // High base confidence
            "phone_with_name_and_address",
            DateTime.UtcNow.AddDays(-1), // Very fresh data
            personalInfoJson: @"{""first_name"": ""Alice"", ""last_name"": ""Smith"", ""age"": 42}",
            addressesJson: @"[{""street"": ""789 Pine Rd"", ""city"": ""Los Angeles"", ""state"": ""CA"", ""postal_code"": ""90001""}]",
            phonesJson: @"[{""phone"": ""3105552000"", ""phone_type"": ""mobile"", ""is_active"": true}]",
            financialJson: @"{""credit_score"": 800, ""estimated_income"": 120000}"
        );

        await context.ConsumerEnrichments.AddAsync(consumer2);

        // Lower confidence record with older data
        var testPhone3 = PhoneNumber.Create("2125553000").Value;
        var consumer3 = ConsumerEnrichment.Create(
            testPhone3,
            "EQF_" + Guid.NewGuid().ToString("N").Substring(0, 24).ToUpper(),
            0.60, // Lower confidence
            "phone_only",
            DateTime.UtcNow.AddDays(-90), // Older data (3 months)
            personalInfoJson: @"{""first_name"": ""John"", ""last_name"": ""Doe""}",
            addressesJson: @"[{""city"": ""New York"", ""state"": ""NY"", ""postal_code"": ""10001""}]",
            phonesJson: @"[{""phone"": ""2125553000"", ""phone_type"": ""landline""}]",
            financialJson: @"{""credit_score"": 650}"
        );

        await context.ConsumerEnrichments.AddAsync(consumer3);

        // Save all changes
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Seeded {3} test consumer enrichment records");
        Console.WriteLine($"   - 8015551234: Bob Barker (base confidence 0.75)");
        Console.WriteLine($"   - 3105552000: Alice Smith (high confidence 0.95)");
        Console.WriteLine($"   - 2125553000: John Doe (low confidence 0.60)");
        Console.WriteLine($"   - 5559999999: NOT SEEDED (for no-match scenario testing)");
    }
}
