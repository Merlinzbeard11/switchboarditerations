using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using EquifaxEnrichmentAPI.Api.Services;
using Microsoft.Extensions.Logging;

namespace EquifaxEnrichmentAPI.Tests.Unit.Services;

public class AuditLoggingServiceTests
{
    [Fact]
    public async Task LogRequest_HashesPhoneNumber_WithSHA256()
    {
        // Arrange - BDD Scenario 1: Phone hashing for privacy
        var logger = new Mock<ILogger<AuditLoggingService>>();
        var serviceProvider = new Mock<IServiceProvider>();
        var service = new AuditLoggingService(logger.Object, serviceProvider.Object);

        var entry = new AuditLogEntry
        {
            BuyerId = "buyer-123",
            Phone = "8015551234",
            PermissiblePurpose = "Credit evaluation",
            IpAddress = "192.168.1.1",
            Response = "success",
            StatusCode = 200
        };

        // Act
        await service.LogRequestAsync(entry, CancellationToken.None);

        // Assert
        entry.PhoneHash.Should().NotBeNullOrEmpty("phone should be hashed");
        entry.PhoneHash.Should().NotBe("8015551234", "hash should not be plaintext");

        // Verify it's a valid SHA-256 hash (64 hex characters)
        entry.PhoneHash.Should().HaveLength(64, "SHA-256 produces 32 bytes = 64 hex chars");
        entry.PhoneHash.Should().MatchRegex("^[0-9A-F]+$", "should be hexadecimal");

        // Verify hash is deterministic (same input = same hash)
        var expectedHash = ComputeSHA256Hash("8015551234");
        entry.PhoneHash.Should().Be(expectedHash, "hash should be deterministic");
    }

    [Fact]
    public async Task FireAndForget_ReturnsImmediately_WithoutBlockingOnDatabaseWrite()
    {
        // Arrange - BDD Scenario 2: Fire-and-forget async logging (< 1ms)
        var logger = new Mock<ILogger<AuditLoggingService>>();
        var serviceProvider = new Mock<IServiceProvider>();
        var service = new AuditLoggingService(logger.Object, serviceProvider.Object);

        var entry = new AuditLogEntry
        {
            BuyerId = "buyer-123",
            Phone = "8015551234",
            PermissiblePurpose = "Credit evaluation",
            IpAddress = "192.168.1.1",
            Response = "success",
            StatusCode = 200
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        await service.LogRequestAsync(entry, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10,
            "LogRequestAsync should return immediately (fire-and-forget pattern via Channel)");
    }

    [Fact]
    public async Task MultipleEnqueueOperations_DoNotBlock_UsingBoundedChannel()
    {
        // Arrange - BDD Scenario: Channel<T> async processing
        var logger = new Mock<ILogger<AuditLoggingService>>();
        var serviceProvider = new Mock<IServiceProvider>();
        var service = new AuditLoggingService(logger.Object, serviceProvider.Object);

        // Act - Enqueue multiple entries rapidly
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 50; i++)
        {
            await service.LogRequestAsync(new AuditLogEntry
            {
                BuyerId = $"buyer-{i}",
                Phone = $"80155512{i:D2}",
                PermissiblePurpose = "Credit evaluation",
                IpAddress = "192.168.1.1",
                Response = "success",
                StatusCode = 200
            }, CancellationToken.None);
        }
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100,
            "50 enqueue operations should complete quickly with Channel<T> bounded buffer");
    }

    /// <summary>
    /// Helper to compute SHA-256 hash for verification
    /// </summary>
    private static string ComputeSHA256Hash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}
