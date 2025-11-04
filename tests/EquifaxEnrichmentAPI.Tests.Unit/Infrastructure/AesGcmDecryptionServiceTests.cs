using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using EquifaxEnrichmentAPI.Application.Configuration;
using EquifaxEnrichmentAPI.Infrastructure.Services;
using Xunit;

namespace EquifaxEnrichmentAPI.Tests.Unit.Infrastructure;

/// <summary>
/// Unit tests for AES-GCM decryption service.
/// Tests all BDD scenarios from feature-1.4-aes-gcm-decryption.feature
///
/// BDD Feature: AES-GCM Decryption Service for Equifax Encrypted Fields
/// BDD File: features/phase1/feature-1.4-aes-gcm-decryption.feature
/// </summary>
public class AesGcmDecryptionServiceTests
{
    // Test encryption key (32 bytes = 256 bits)
    // This is the actual Equifax key for development/testing
    private const string TestKeyHex = "4142505A44514850594D493346463758424F59505A45424D4E345931364E5A50";

    private readonly AesGcmDecryptionService _service;

    public AesGcmDecryptionServiceTests()
    {
        var options = Options.Create(new EncryptionOptions
        {
            EquifaxKeyHex = TestKeyHex
        });

        _service = new AesGcmDecryptionService(options);
    }

    /// <summary>
    /// Helper method to encrypt plaintext for testing decryption.
    /// Mirrors the Equifax encryption format.
    /// </summary>
    private string EncryptForTesting(string plaintext)
    {
        var key = Convert.FromHexString(TestKeyHex);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce = new byte[12]; // 12-byte nonce
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[16]; // 16-byte tag

        // Generate random nonce
        RandomNumberGenerator.Fill(nonce);

        // Encrypt
        using var aes = new AesGcm(key, 16);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        // Return JSON format matching Equifax structure
        return JsonSerializer.Serialize(new
        {
            ciphertext = Convert.ToHexString(ciphertext).ToLower(),
            iv = Convert.ToHexString(nonce).ToLower(),
            tag = Convert.ToHexString(tag).ToLower()
        });
    }

    [Fact]
    public void Decrypt_ValidEncryptedConsumerKey_ReturnsDecryptedValue()
    {
        // Arrange - BDD Scenario 1: Successfully decrypt valid encrypted consumer_key
        var plaintext = "ABC123XYZ789CONSUMER";
        var encryptedJson = EncryptForTesting(plaintext);

        // Act
        var result = _service.Decrypt(encryptedJson);

        // Assert
        result.Should().Be(plaintext);
    }

    [Fact]
    public void Decrypt_ValidEncryptedName_ReturnsDecryptedValue()
    {
        // Arrange - BDD Scenario 2: Successfully decrypt encrypted name field
        var firstName = "John";
        var encryptedJson = EncryptForTesting(firstName);

        // Act
        var result = _service.Decrypt(encryptedJson);

        // Assert
        result.Should().Be(firstName);
    }

    [Fact]
    public void Decrypt_ValidEncryptedAddressWithSpecialCharacters_PreservesCharacters()
    {
        // Arrange - BDD Scenario 3: Successfully decrypt encrypted address field with special characters
        var streetName = "O'Malley St. #123-B";
        var encryptedJson = EncryptForTesting(streetName);

        // Act
        var result = _service.Decrypt(encryptedJson);

        // Assert
        result.Should().Be(streetName);
    }

    [Fact]
    public void Decrypt_NullInput_ReturnsNull()
    {
        // Arrange - BDD Scenario 4: Handle null or empty encrypted value gracefully
        string? encryptedJson = null;

        // Act
        var result = _service.Decrypt(encryptedJson);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Decrypt_EmptyInput_ReturnsNull()
    {
        // Arrange - BDD Scenario 4: Handle null or empty encrypted value gracefully
        var encryptedJson = string.Empty;

        // Act
        var result = _service.Decrypt(encryptedJson);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Decrypt_WhitespaceInput_ReturnsNull()
    {
        // Arrange - BDD Scenario 4: Handle null or empty encrypted value gracefully
        var encryptedJson = "   ";

        // Act
        var result = _service.Decrypt(encryptedJson);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Decrypt_InvalidJsonFormat_ThrowsJsonException()
    {
        // Arrange - BDD Scenario 5: Handle invalid JSON format
        var invalidJson = "not valid json at all";

        // Act
        Action act = () => _service.Decrypt(invalidJson);

        // Assert
        act.Should().Throw<JsonException>()
            .WithMessage("*Invalid encrypted data format*");
    }

    [Fact]
    public void Decrypt_MissingCiphertextField_ThrowsJsonException()
    {
        // Arrange - BDD Scenario 5: Handle invalid JSON format (missing ciphertext)
        var invalidJson = JsonSerializer.Serialize(new
        {
            iv = "a1b2c3d4e5f6a7b8c9d0e1f2",
            tag = "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6"
        });

        // Act
        Action act = () => _service.Decrypt(invalidJson);

        // Assert
        act.Should().Throw<JsonException>()
            .WithMessage("*missing required fields*");
    }

    [Fact]
    public void Decrypt_CorruptCiphertext_ThrowsCryptographicException()
    {
        // Arrange - BDD Scenario 6: Handle corrupt ciphertext gracefully
        var plaintext = "TestData";
        var encryptedJson = EncryptForTesting(plaintext);

        // Corrupt the ciphertext by changing first hex character in ciphertext value
        var json = JsonSerializer.Deserialize<Dictionary<string, string>>(encryptedJson);
        var ciphertext = json!["ciphertext"];
        var corruptedCiphertext = "ff" + ciphertext.Substring(2); // Change first byte
        json["ciphertext"] = corruptedCiphertext;
        var corrupted = JsonSerializer.Serialize(json);

        // Act
        Action act = () => _service.Decrypt(corrupted);

        // Assert
        act.Should().Throw<CryptographicException>()
            .WithMessage("*Decryption failed*authentication tag mismatch*");
    }

    [Fact]
    public void Decrypt_WrongEncryptionKey_ThrowsCryptographicException()
    {
        // Arrange - BDD Scenario 7: Handle wrong encryption key
        var plaintext = "TestData";

        // Encrypt with different key
        var wrongKey = "0000000000000000000000000000000000000000000000000000000000000000";
        var wrongKeyBytes = Convert.FromHexString(wrongKey);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce = new byte[12];
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[16];

        RandomNumberGenerator.Fill(nonce);

        using (var aes = new AesGcm(wrongKeyBytes, 16))
        {
            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);
        }

        var encryptedJson = JsonSerializer.Serialize(new
        {
            ciphertext = Convert.ToHexString(ciphertext).ToLower(),
            iv = Convert.ToHexString(nonce).ToLower(),
            tag = Convert.ToHexString(tag).ToLower()
        });

        // Act - Try to decrypt with correct key (but data encrypted with wrong key)
        Action act = () => _service.Decrypt(encryptedJson);

        // Assert
        act.Should().Throw<CryptographicException>()
            .WithMessage("*authentication tag mismatch*");
    }

    [Fact]
    public void Decrypt_InvalidHexEncoding_ThrowsFormatException()
    {
        // Arrange - Invalid hex characters in ciphertext
        var invalidJson = JsonSerializer.Serialize(new
        {
            ciphertext = "not-hex-characters!!!",
            iv = "a1b2c3d4e5f6a7b8c9d0e1f2",
            tag = "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6"
        });

        // Act
        Action act = () => _service.Decrypt(invalidJson);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("*Invalid hexadecimal encoding*");
    }

    [Fact]
    public void Decrypt_InvalidNonceSize_ThrowsCryptographicException()
    {
        // Arrange - Nonce not 12 bytes
        var invalidJson = JsonSerializer.Serialize(new
        {
            ciphertext = "a1b2c3d4",
            iv = "a1b2c3",  // Only 3 bytes instead of 12
            tag = "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6"
        });

        // Act
        Action act = () => _service.Decrypt(invalidJson);

        // Assert
        act.Should().Throw<CryptographicException>()
            .WithMessage("*Invalid nonce (IV) size*12 bytes*");
    }

    [Fact]
    public void Decrypt_InvalidTagSize_ThrowsCryptographicException()
    {
        // Arrange - Tag not 16 bytes
        var invalidJson = JsonSerializer.Serialize(new
        {
            ciphertext = "a1b2c3d4",
            iv = "a1b2c3d4e5f6a7b8c9d0e1f2",
            tag = "a1b2c3"  // Only 3 bytes instead of 16
        });

        // Act
        Action act = () => _service.Decrypt(invalidJson);

        // Assert
        act.Should().Throw<CryptographicException>()
            .WithMessage("*Invalid authentication tag size*16 bytes*");
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        IOptions<EncryptionOptions>? options = null;

        // Act
        Action act = () => new AesGcmDecryptionService(options!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void Constructor_InvalidKeyLength_ThrowsInvalidOperationException()
    {
        // Arrange - Key not 32 bytes
        var options = Options.Create(new EncryptionOptions
        {
            EquifaxKeyHex = "ABCD1234"  // Too short
        });

        // Act
        Action act = () => new AesGcmDecryptionService(options);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Invalid encryption key length*64 hex characters*");
    }

    [Fact]
    public void Constructor_EmptyKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new EncryptionOptions
        {
            EquifaxKeyHex = string.Empty
        });

        // Act
        Action act = () => new AesGcmDecryptionService(options);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Encryption key not configured*");
    }

    [Fact]
    public void Decrypt_LargePayload_DecryptsSuccessfully()
    {
        // Arrange - BDD Scenario 8: Performance test (large payload)
        var largePlaintext = new string('A', 10000); // 10KB payload
        var encryptedJson = EncryptForTesting(largePlaintext);

        // Act
        var result = _service.Decrypt(encryptedJson);

        // Assert
        result.Should().Be(largePlaintext);
    }

    [Fact]
    public void Decrypt_UnicodeCharacters_PreservesEncoding()
    {
        // Arrange - Test UTF-8 encoding preservation
        var unicodePlaintext = "Hello 世界 🌍 Emoji";
        var encryptedJson = EncryptForTesting(unicodePlaintext);

        // Act
        var result = _service.Decrypt(encryptedJson);

        // Assert
        result.Should().Be(unicodePlaintext);
    }
}
