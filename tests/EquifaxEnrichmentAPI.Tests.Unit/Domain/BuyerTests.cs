using EquifaxEnrichmentAPI.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EquifaxEnrichmentAPI.Tests.Unit.Domain;

/// <summary>
/// TDD Unit Tests for Buyer Entity
/// BDD Feature: API Key Authentication (feature-2.1-api-key-authentication.feature)
///
/// TESTS WRITTEN FIRST (TDD Red-Green-Refactor)
/// These tests will FAIL until Buyer entity is implemented
/// </summary>
public class BuyerTests
{
    // ====================================================================
    // Factory Method Tests - Buyer.Create()
    // ====================================================================

    [Fact]
    public void Create_ValidInputs_ShouldCreateBuyerWithCorrectProperties()
    {
        // Arrange
        var apiKeyHash = "jw6ejF19jqC8VPwbLBXNnGSCFKLxGTJ0fMT7RzwQDIk=";
        var name = "Test Buyer";
        var isActive = true;

        // Act
        var buyer = Buyer.Create(apiKeyHash, name, isActive);

        // Assert
        buyer.Should().NotBeNull();
        buyer.Id.Should().NotBeEmpty();
        buyer.ApiKeyHash.Should().Be(apiKeyHash);
        buyer.Name.Should().Be(name);
        buyer.IsActive.Should().BeTrue();
        buyer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        buyer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_DefaultIsActiveTrue_ShouldCreateActiveBuyer()
    {
        // Arrange
        var apiKeyHash = "jw6ejF19jqC8VPwbLBXNnGSCFKLxGTJ0fMT7RzwQDIk=";
        var name = "Test Buyer";

        // Act
        var buyer = Buyer.Create(apiKeyHash, name);

        // Assert
        buyer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_NullApiKeyHash_ShouldThrowArgumentException()
    {
        // Arrange
        string? apiKeyHash = null;
        var name = "Test Buyer";

        // Act
        Action act = () => Buyer.Create(apiKeyHash!, name);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*API key hash cannot be empty*");
    }

    [Fact]
    public void Create_EmptyApiKeyHash_ShouldThrowArgumentException()
    {
        // Arrange
        var apiKeyHash = "";
        var name = "Test Buyer";

        // Act
        Action act = () => Buyer.Create(apiKeyHash, name);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*API key hash cannot be empty*");
    }

    [Fact]
    public void Create_WhitespaceApiKeyHash_ShouldThrowArgumentException()
    {
        // Arrange
        var apiKeyHash = "   ";
        var name = "Test Buyer";

        // Act
        Action act = () => Buyer.Create(apiKeyHash, name);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*API key hash cannot be empty*");
    }

    [Fact]
    public void Create_NullName_ShouldThrowArgumentException()
    {
        // Arrange
        var apiKeyHash = "jw6ejF19jqC8VPwbLBXNnGSCFKLxGTJ0fMT7RzwQDIk=";
        string? name = null;

        // Act
        Action act = () => Buyer.Create(apiKeyHash, name!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Buyer name cannot be empty*");
    }

    [Fact]
    public void Create_EmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var apiKeyHash = "jw6ejF19jqC8VPwbLBXNnGSCFKLxGTJ0fMT7RzwQDIk=";
        var name = "";

        // Act
        Action act = () => Buyer.Create(apiKeyHash, name);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Buyer name cannot be empty*");
    }

    // ====================================================================
    // Deactivate() Tests - BDD Scenario 21: Key revocation (soft delete)
    // ====================================================================

    [Fact]
    public void Deactivate_ActiveBuyer_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var buyer = Buyer.Create("hash", "Test Buyer", isActive: true);
        var originalUpdatedAt = buyer.UpdatedAt;

        // Act
        Thread.Sleep(10); // Ensure UpdatedAt changes
        buyer.Deactivate();

        // Assert
        buyer.IsActive.Should().BeFalse();
        buyer.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Deactivate_InactiveBuyer_ShouldRemainInactive()
    {
        // Arrange
        var buyer = Buyer.Create("hash", "Test Buyer", isActive: false);

        // Act
        buyer.Deactivate();

        // Assert
        buyer.IsActive.Should().BeFalse();
    }

    // ====================================================================
    // Activate() Tests
    // ====================================================================

    [Fact]
    public void Activate_InactiveBuyer_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var buyer = Buyer.Create("hash", "Test Buyer", isActive: false);
        var originalUpdatedAt = buyer.UpdatedAt;

        // Act
        Thread.Sleep(10); // Ensure UpdatedAt changes
        buyer.Activate();

        // Assert
        buyer.IsActive.Should().BeTrue();
        buyer.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Activate_ActiveBuyer_ShouldRemainActive()
    {
        // Arrange
        var buyer = Buyer.Create("hash", "Test Buyer", isActive: true);

        // Act
        buyer.Activate();

        // Assert
        buyer.IsActive.Should().BeTrue();
    }

    // ====================================================================
    // RotateApiKey() Tests - BDD Scenario 9: Automated API key rotation
    // ====================================================================

    [Fact]
    public void RotateApiKey_ValidHash_ShouldUpdateApiKeyHashAndUpdatedAt()
    {
        // Arrange
        var originalHash = "jw6ejF19jqC8VPwbLBXNnGSCFKLxGTJ0fMT7RzwQDIk=";
        var buyer = Buyer.Create(originalHash, "Test Buyer");
        var originalUpdatedAt = buyer.UpdatedAt;
        var newHash = "NEW_HASH_AFTER_ROTATION_90_DAYS";

        // Act
        Thread.Sleep(10); // Ensure UpdatedAt changes
        buyer.RotateApiKey(newHash);

        // Assert
        buyer.ApiKeyHash.Should().Be(newHash);
        buyer.ApiKeyHash.Should().NotBe(originalHash);
        buyer.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void RotateApiKey_NullHash_ShouldThrowArgumentException()
    {
        // Arrange
        var buyer = Buyer.Create("hash", "Test Buyer");
        string? newHash = null;

        // Act
        Action act = () => buyer.RotateApiKey(newHash!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*New API key hash cannot be empty*");
    }

    [Fact]
    public void RotateApiKey_EmptyHash_ShouldThrowArgumentException()
    {
        // Arrange
        var buyer = Buyer.Create("hash", "Test Buyer");
        var newHash = "";

        // Act
        Action act = () => buyer.RotateApiKey(newHash);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*New API key hash cannot be empty*");
    }

    [Fact]
    public void RotateApiKey_WhitespaceHash_ShouldThrowArgumentException()
    {
        // Arrange
        var buyer = Buyer.Create("hash", "Test Buyer");
        var newHash = "   ";

        // Act
        Action act = () => buyer.RotateApiKey(newHash);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*New API key hash cannot be empty*");
    }
}
