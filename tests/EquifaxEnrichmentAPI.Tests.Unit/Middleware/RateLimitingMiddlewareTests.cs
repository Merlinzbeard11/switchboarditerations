using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using EquifaxEnrichmentAPI.Api.Middleware;
using StackExchange.Redis;

namespace EquifaxEnrichmentAPI.Tests.Unit.Middleware;

public class RateLimitingMiddlewareTests
{
    [Fact]
    public async Task AllowRequest_WhenWithinRateLimit_Returns200WithHeaders()
    {
        // Arrange - BDD Scenario 2: Requests within rate limit
        var mockMultiplexer = new Mock<IConnectionMultiplexer>();
        var mockDatabase = new Mock<IDatabase>();
        var mockLogger = new Mock<ILogger<RateLimitingMiddleware>>();

        // Mock Lua script to return: allowed=1, consumed=951, remaining=49
        var luaResult = RedisResult.Create(new RedisValue[] { 1, 951, 49 });
        mockDatabase
            .Setup(db => db.ScriptEvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<RedisKey[]>(),
                It.IsAny<RedisValue[]>(),
                CommandFlags.None))
            .ReturnsAsync(luaResult);

        mockMultiplexer
            .Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(mockDatabase.Object);

        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new RateLimitingMiddleware(next, mockMultiplexer.Object, mockLogger.Object);

        // Use DefaultHttpContext (Microsoft official pattern)
        var context = new DefaultHttpContext();
        context.Request.Headers["X-API-Key"] = "test-api-key";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue("request should proceed when within rate limit");
        context.Response.Headers["X-RateLimit-Limit"].ToString().Should().Be("1000");
        context.Response.Headers["X-RateLimit-Remaining"].ToString().Should().Be("49");
        context.Response.Headers.Should().ContainKey("X-RateLimit-Reset");
    }

    [Fact]
    public async Task RejectRequest_WhenRateLimitExceeded_Returns429WithOverageTracking()
    {
        // Arrange - BDD Scenario 3: Rate limit exceeded
        var mockMultiplexer = new Mock<IConnectionMultiplexer>();
        var mockDatabase = new Mock<IDatabase>();
        var mockLogger = new Mock<ILogger<RateLimitingMiddleware>>();

        // Mock Lua script to return: allowed=0, consumed=1000, remaining=0
        var luaResult = RedisResult.Create(new RedisValue[] { 0, 1000, 0 });
        mockDatabase
            .Setup(db => db.ScriptEvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<RedisKey[]>(),
                It.IsAny<RedisValue[]>(),
                CommandFlags.None))
            .ReturnsAsync(luaResult);

        // Mock overage tracking
        mockDatabase
            .Setup(db => db.StringIncrementAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<long>(),
                CommandFlags.None))
            .ReturnsAsync(1);

        mockDatabase
            .Setup(db => db.KeyExpireAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<ExpireWhen>(),
                CommandFlags.None))
            .ReturnsAsync(true);

        mockMultiplexer
            .Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(mockDatabase.Object);

        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new RateLimitingMiddleware(next, mockMultiplexer.Object, mockLogger.Object);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-API-Key"] = "test-api-key";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeFalse("request should NOT proceed when rate limit exceeded");
        context.Response.StatusCode.Should().Be(429, "should return 429 Too Many Requests");
        context.Response.Headers["X-RateLimit-Remaining"].ToString().Should().Be("0");

        // Verify overage tracking was called
        mockDatabase.Verify(db => db.StringIncrementAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<long>(),
            CommandFlags.None), Times.Once, "should track overage for billing");
    }

    [Fact]
    public async Task GracefulDegradation_WhenRedisUnavailable_AllowsRequest()
    {
        // Arrange - BDD Scenario: Graceful degradation (fail-open)
        var mockMultiplexer = new Mock<IConnectionMultiplexer>();
        var mockDatabase = new Mock<IDatabase>();
        var mockLogger = new Mock<ILogger<RateLimitingMiddleware>>();

        // Simulate Redis failure
        mockDatabase
            .Setup(db => db.ScriptEvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<RedisKey[]>(),
                It.IsAny<RedisValue[]>(),
                CommandFlags.None))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Redis unavailable"));

        mockMultiplexer
            .Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(mockDatabase.Object);

        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new RateLimitingMiddleware(next, mockMultiplexer.Object, mockLogger.Object);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-API-Key"] = "test-api-key";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue("should fail open and allow request when Redis unavailable");
        context.Response.Headers["X-RateLimit-Limit"].ToString().Should().Be("1000");
        context.Response.Headers["X-RateLimit-Remaining"].ToString().Should().Be("1000", "should show full limit when Redis unavailable");
    }
}
