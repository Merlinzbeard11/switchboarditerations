using System.Security.Cryptography;
using System.Text;
using EquifaxEnrichmentAPI.Domain.Entities;

namespace EquifaxEnrichmentAPI.Infrastructure.Persistence;

/// <summary>
/// Seeds database with buyer accounts for production use.
/// BDD Feature: API Key Authentication (feature-2.1-api-key-authentication.feature)
///
/// SECURITY NOTES:
/// - API keys generated with cryptographic randomness (32 bytes)
/// - Keys stored as SHA-256 hashes only (never plaintext)
/// - Plaintext keys logged ONCE on first seed, then never recoverable
/// </summary>
public static class BuyerSeeder
{
    /// <summary>
    /// Seeds the database with buyer accounts.
    /// Idempotent: Can be run multiple times safely (checks for existing data).
    /// </summary>
    public static async Task SeedAsync(EnrichmentDbContext context)
    {
        // Check if buyers already exist
        if (context.Buyers.Any())
        {
            Console.WriteLine("Buyers already seeded. Skipping buyer seed operation.");
            return;
        }

        Console.WriteLine("Seeding buyers...");

        // ====================================================================
        // BUYER 1: Data Bridge Studio
        // Initial production buyer for API testing and development
        // ====================================================================

        // Generate secure API key (32 random bytes = 256 bits)
        var apiKey = GenerateApiKey();
        var apiKeyHash = ComputeSha256Hash(apiKey);

        var dataBridgeStudio = Buyer.Create(
            apiKeyHash: apiKeyHash,
            name: "Data Bridge Studio",
            isActive: true
        );

        await context.Buyers.AddAsync(dataBridgeStudio);
        await context.SaveChangesAsync();

        // SECURITY: Log plaintext API key ONCE for initial setup
        // After this, the key cannot be recovered (only hash stored)
        Console.WriteLine("====================================================================");
        Console.WriteLine("✅ BUYER CREATED: Data Bridge Studio");
        Console.WriteLine("====================================================================");
        Console.WriteLine($"Buyer ID: {dataBridgeStudio.Id}");
        Console.WriteLine($"API Key:  {apiKey}");
        Console.WriteLine($"");
        Console.WriteLine("⚠️  IMPORTANT: Save this API key securely!");
        Console.WriteLine("⚠️  This is the ONLY time the plaintext key will be shown.");
        Console.WriteLine("⚠️  The database stores only the SHA-256 hash.");
        Console.WriteLine("====================================================================");
    }

    /// <summary>
    /// Generates a cryptographically secure API key.
    /// Format: 32 random bytes encoded as Base64 (43 characters).
    /// </summary>
    private static string GenerateApiKey()
    {
        var keyBytes = new byte[32]; // 256 bits
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(keyBytes);
        return Convert.ToBase64String(keyBytes);
    }

    /// <summary>
    /// Computes SHA-256 hash of API key and returns Base64-encoded string.
    /// BDD Scenario 8: Store API key as SHA-256 hash in database
    /// </summary>
    private static string ComputeSha256Hash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}
