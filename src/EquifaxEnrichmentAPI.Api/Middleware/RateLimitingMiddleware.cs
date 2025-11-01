using Microsoft.AspNetCore.Http;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace EquifaxEnrichmentAPI.Api.Middleware;

/// <summary>
/// Distributed rate limiting middleware using Redis with Lua scripts for atomic operations.
/// Implements sliding window algorithm to prevent boundary burst issues.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    // Rate limit configuration (per buyer)
    private const int LIMIT_PER_MINUTE = 1000;
    private const int LIMIT_PER_HOUR = 60000;
    private const int LIMIT_PER_DAY = 1000000;
    private const decimal OVERAGE_COST = 0.035m;

    // Lua script for atomic rate limit check and increment (Gotcha #1: Prevents race conditions)
    private const string LUA_SLIDING_WINDOW_SCRIPT = @"
        local key = KEYS[1]
        local limit = tonumber(ARGV[1])
        local window = tonumber(ARGV[2])
        local now = tonumber(ARGV[3])

        -- Remove expired entries (sliding window)
        redis.call('ZREMRANGEBYSCORE', key, 0, now - window)

        -- Count current entries
        local current = redis.call('ZCARD', key)

        if current < limit then
            -- Add new entry with current timestamp
            redis.call('ZADD', key, now, now)
            redis.call('EXPIRE', key, window)
            return {1, current + 1, limit - current - 1}
        else
            -- Rate limit exceeded
            return {0, current, 0}
        end
    ";

    public RateLimitingMiddleware(RequestDelegate next, IConnectionMultiplexer redis, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Constructor for testing without RequestDelegate
    public RateLimitingMiddleware(IConnectionMultiplexer redis)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = null!;
        _next = null!;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract API key from request
        if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
        {
            await _next(context);
            return;
        }

        var buyerId = apiKey.ToString(); // In production, would lookup buyer ID from API key

        // Check rate limits using sliding window algorithm (Gotcha #2: Prevents 2x burst at boundaries)
        var (allowed, consumed, remaining) = await CheckRateLimitAsync(buyerId);

        // Add rate limit headers to response
        context.Response.Headers["X-RateLimit-Limit"] = LIMIT_PER_MINUTE.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds().ToString();

        if (!allowed)
        {
            // Track overage for billing
            await TrackOverageAsync(buyerId);

            context.Response.StatusCode = 429; // Too Many Requests
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                limit = LIMIT_PER_MINUTE,
                reset = DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds()
            });
            return;
        }

        await _next(context);
    }

    /// <summary>
    /// Check rate limit using Lua script for atomic operations.
    /// Implements sliding window algorithm to prevent boundary burst issues.
    /// </summary>
    private async Task<(bool allowed, int consumed, int remaining)> CheckRateLimitAsync(string buyerId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"ratelimit:{buyerId}:minute";
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var windowSeconds = 60;

            // Execute Lua script atomically (Gotcha #1: Prevents race conditions in distributed environment)
            var result = await db.ScriptEvaluateAsync(
                LUA_SLIDING_WINDOW_SCRIPT,
                new RedisKey[] { key },
                new RedisValue[] { LIMIT_PER_MINUTE, windowSeconds, now }
            );

            var values = (RedisValue[])result;
            var allowed = (int)values[0] == 1;
            var consumed = (int)values[1];
            var remaining = (int)values[2];

            return (allowed, consumed, remaining);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Rate limit check failed for buyer {BuyerId}", buyerId);

            // Fail open: allow request if Redis unavailable (Gotcha #5: Graceful degradation)
            return (true, 0, LIMIT_PER_MINUTE);
        }
    }

    /// <summary>
    /// Track overage requests for billing purposes ($0.035 per call over quota).
    /// </summary>
    private async Task<bool> TrackOverageAsync(string buyerId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var overageKey = $"overage:{buyerId}:{DateTimeOffset.UtcNow:yyyyMMdd}";

            // Increment overage counter
            await db.StringIncrementAsync(overageKey);

            // Set expiration to 90 days (billing cycle + retention)
            await db.KeyExpireAsync(overageKey, TimeSpan.FromDays(90));

            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to track overage for buyer {BuyerId}", buyerId);
            return false;
        }
    }
}
