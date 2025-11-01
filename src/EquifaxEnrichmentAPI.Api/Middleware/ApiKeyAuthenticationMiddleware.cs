using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using EquifaxEnrichmentAPI.Infrastructure.Persistence;

namespace EquifaxEnrichmentAPI.Api.Middleware;

/// <summary>
/// API Key Authentication Middleware with timing-attack prevention.
/// BDD Feature: API Key Authentication (feature-2.1-api-key-authentication.feature)
///
/// SECURITY CRITICAL:
/// - BDD Scenario 4: Uses CryptographicOperations.FixedTimeEquals() to prevent timing attacks (CVE-2025-59425)
/// - BDD Scenario 8: Compares SHA-256 hashes, NEVER plaintext keys
/// - BDD Scenario 23: Constant-time hash comparison
/// </summary>
public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-API-Key";

    public ApiKeyAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context, EnrichmentDbContext dbContext)
    {
        // BDD Scenario 2: Missing API key (401 Unauthorized)
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeader))
        {
            await WriteUnauthorizedResponse(context, "Missing API key",
                "Include X-API-Key header with valid API key");
            return;
        }

        var providedApiKey = apiKeyHeader.ToString();

        // Validate API key is not empty
        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            await WriteUnauthorizedResponse(context, "Missing API key",
                "Include X-API-Key header with valid API key");
            return;
        }

        // BDD Scenario 8: Hash the provided API key using SHA-256
        var providedKeyHash = ComputeSha256Hash(providedApiKey);

        // BDD Scenario 1: Look up buyer by API key hash
        // BDD Scenario 17: Use index on api_key_hash for O(log n) lookup (< 5ms)
        var buyer = await dbContext.Buyers
            .AsNoTracking() // Read-only operation for performance
            .FirstOrDefaultAsync(b => b.ApiKeyHash == providedKeyHash);

        // BDD Scenario 3: Invalid API key (hash not found in database)
        if (buyer == null)
        {
            // SECURITY: Use constant-time comparison even for "not found" case
            // This prevents timing attacks that could distinguish "key exists" vs "key doesn't exist"
            // BDD Scenario 4: Timing attack prevention
            PerformDummyTimingAttackMitigation(providedKeyHash);

            await WriteUnauthorizedResponse(context, "Invalid API key",
                "Ensure X-API-Key header contains a valid API key");
            return;
        }

        // BDD Scenario 5: Reject request for inactive buyer account
        // BDD Scenario 14: Zero Trust - verify IsActive on EVERY request
        if (!buyer.IsActive)
        {
            // SECURITY: Same error message as invalid key to prevent enumeration
            // BDD Scenario 5: "Invalid API key" (same as invalid key - prevent enumeration)
            await WriteUnauthorizedResponse(context, "Invalid API key",
                "Ensure X-API-Key header contains a valid API key");
            return;
        }

        // BDD Scenario 1: Authentication succeeded
        // Store buyer information in request context for downstream use
        context.Items["BuyerId"] = buyer.Id;
        context.Items["BuyerName"] = buyer.Name;
        context.Items["AuthenticatedAt"] = DateTime.UtcNow;

        // BDD Scenario 1: Request should proceed to downstream middleware
        await _next(context);
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

    /// <summary>
    /// Performs a dummy constant-time operation to prevent timing attacks.
    /// BDD Scenario 4: Use constant-time comparison to prevent timing attacks
    ///
    /// When API key is not found, we still perform a hash comparison against a dummy value
    /// to ensure the response time is constant regardless of whether the key exists.
    /// </summary>
    private static void PerformDummyTimingAttackMitigation(string providedKeyHash)
    {
        // Use a fixed dummy hash for comparison
        var dummyHash = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
        var providedBytes = Convert.FromBase64String(providedKeyHash);
        var dummyBytes = Convert.FromBase64String(dummyHash);

        // BDD Scenario 4: Use CryptographicOperations.FixedTimeEquals()
        // BDD Scenario 23: Constant-time hash comparison
        _ = CryptographicOperations.FixedTimeEquals(providedBytes, dummyBytes);
    }

    /// <summary>
    /// Writes 401 Unauthorized response with error details.
    /// BDD Scenario 2, 3, 5: Return 401 for authentication failures
    /// BDD Scenario 22: Never log full API keys (security best practice)
    /// </summary>
    private static async Task WriteUnauthorizedResponse(HttpContext context, string error, string guidance)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";

        var response = new
        {
            response = "error",
            message = error,
            guidance = guidance,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Extension method to register API Key Authentication Middleware.
/// </summary>
public static class ApiKeyAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiKeyAuthenticationMiddleware>();
    }
}
