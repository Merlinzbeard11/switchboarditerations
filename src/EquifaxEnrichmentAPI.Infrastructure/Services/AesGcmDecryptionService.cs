using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using EquifaxEnrichmentAPI.Application.Configuration;
using EquifaxEnrichmentAPI.Application.Interfaces;

namespace EquifaxEnrichmentAPI.Infrastructure.Services;

/// <summary>
/// AES-256-GCM decryption service for Equifax encrypted fields.
/// Implements enterprise-grade decryption with security best practices.
///
/// BDD Feature: AES-GCM Decryption Service for Equifax Encrypted Fields
/// BDD File: features/phase1/feature-1.4-aes-gcm-decryption.feature
///
/// SECURITY IMPLEMENTATION (Based on Microsoft Learn and authoritative sources):
/// ✅ Uses AesGcm(key, tagSize) constructor (not deprecated AesGcm(key) - SYSLIB0053)
/// ✅ 16-byte (128-bit) authentication tags for cross-platform compatibility (macOS requirement)
/// ✅ Decrypt() method automatically verifies authentication tag BEFORE returning plaintext
/// ✅ IDisposable pattern with using statement for proper resource cleanup
/// ✅ IOptions&lt;T&gt; pattern for secure configuration management
/// ✅ Scoped lifetime (not singleton) - AesGcm must be disposed per operation
/// ✅ Returns null for null input (graceful handling)
/// ✅ Specific exceptions for crypto failures (authentication, format, corruption)
///
/// CRITICAL GOTCHAS ADDRESSED:
/// 1. Tag truncation attacks: Uses new constructor requiring explicit tag size
/// 2. macOS compatibility: 16-byte tags (CryptoKit limitation)
/// 3. Premature data use: Decrypt() enforces tag verification before returning
/// 4. Nonce validation: Verifies 12-byte IV size (AES-GCM requirement)
/// 5. Tag array reuse: Never reuses tag arrays (allocates per operation)
///
/// Equifax Encrypted Data Format:
/// {
///   "ciphertext": "hex_encoded_encrypted_data",
///   "iv": "hex_encoded_12_byte_nonce",
///   "tag": "hex_encoded_16_byte_authentication_tag"
/// }
/// </summary>
public class AesGcmDecryptionService : IAesGcmDecryptionService
{
    private readonly byte[] _encryptionKey;
    private const int TagSizeBytes = 16;  // 128-bit for cross-platform (macOS requirement)
    private const int NonceSizeBytes = 12; // 96-bit (AES-GCM standard)

    /// <summary>
    /// Initializes the decryption service with encryption key from configuration.
    /// </summary>
    /// <param name="options">Encryption options from IOptions pattern (appsettings.json or Key Vault)</param>
    /// <exception cref="ArgumentNullException">Thrown if options is null</exception>
    /// <exception cref="InvalidOperationException">Thrown if encryption key is not configured or invalid</exception>
    public AesGcmDecryptionService(IOptions<EncryptionOptions> options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        var encryptionOptions = options.Value;

        // Validate configuration
        encryptionOptions.Validate();

        // Convert hex key to byte array (32 bytes = 256 bits for AES-256)
        try
        {
            _encryptionKey = Convert.FromHexString(encryptionOptions.EquifaxKeyHex);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "Failed to parse encryption key from hexadecimal format.", ex);
        }

        if (_encryptionKey.Length != 32)
        {
            throw new InvalidOperationException(
                $"Invalid encryption key size. Expected 32 bytes (256 bits), got {_encryptionKey.Length} bytes.");
        }
    }

    /// <summary>
    /// Decrypts an AES-GCM encrypted field from Equifax CSV data.
    ///
    /// BDD Scenarios Implemented:
    /// - Successfully decrypt valid encrypted consumer_key
    /// - Successfully decrypt encrypted name field
    /// - Successfully decrypt encrypted address field with special characters
    /// - Handle null or empty encrypted value gracefully (returns null)
    /// - Handle invalid JSON format (throws JsonException)
    /// - Handle corrupt ciphertext gracefully (throws CryptographicException)
    /// - Handle wrong encryption key (throws CryptographicException with tag mismatch)
    /// </summary>
    /// <param name="encryptedJson">Encrypted JSON with ciphertext, iv, and tag</param>
    /// <returns>Decrypted UTF-8 string, or null if input is null/empty</returns>
    /// <exception cref="JsonException">Invalid JSON format or missing required fields</exception>
    /// <exception cref="CryptographicException">Authentication tag mismatch, corrupted data, or wrong key</exception>
    /// <exception cref="FormatException">Invalid hexadecimal encoding</exception>
    public string? Decrypt(string? encryptedJson)
    {
        // BDD Scenario 4: Handle null or empty encrypted value gracefully
        if (string.IsNullOrWhiteSpace(encryptedJson))
            return null;

        // Parse encrypted JSON structure
        EncryptedValue? encrypted;
        try
        {
            encrypted = JsonSerializer.Deserialize<EncryptedValue>(encryptedJson);
        }
        catch (JsonException ex)
        {
            // BDD Scenario 5: Handle invalid JSON format
            throw new JsonException(
                "Invalid encrypted data format. Expected JSON with ciphertext, iv, and tag fields.", ex);
        }

        if (encrypted == null)
        {
            throw new JsonException(
                "Failed to deserialize encrypted data. JSON structure is invalid.");
        }

        // Validate required fields exist
        if (string.IsNullOrWhiteSpace(encrypted.Ciphertext) ||
            string.IsNullOrWhiteSpace(encrypted.Iv) ||
            string.IsNullOrWhiteSpace(encrypted.Tag))
        {
            throw new JsonException(
                "Encrypted data is missing required fields. Must have ciphertext, iv, and tag.");
        }

        // Convert hex strings to byte arrays
        byte[] ciphertext;
        byte[] nonce;
        byte[] tag;

        try
        {
            ciphertext = Convert.FromHexString(encrypted.Ciphertext);
            nonce = Convert.FromHexString(encrypted.Iv);
            tag = Convert.FromHexString(encrypted.Tag);
        }
        catch (FormatException ex)
        {
            throw new FormatException(
                "Invalid hexadecimal encoding in encrypted data (ciphertext, iv, or tag).", ex);
        }

        // Validate sizes
        if (nonce.Length != NonceSizeBytes)
        {
            throw new CryptographicException(
                $"Invalid nonce (IV) size. Expected {NonceSizeBytes} bytes (96 bits), got {nonce.Length} bytes.");
        }

        if (tag.Length != TagSizeBytes)
        {
            throw new CryptographicException(
                $"Invalid authentication tag size. Expected {TagSizeBytes} bytes (128 bits), got {tag.Length} bytes. " +
                "Note: macOS only supports 128-bit tags.");
        }

        // Decrypt using AES-GCM
        // CRITICAL: Uses new AesGcm(key, tagSize) constructor (SYSLIB0053 compliant)
        // CRITICAL: Decrypt() automatically verifies authentication tag BEFORE returning plaintext
        var plaintext = new byte[ciphertext.Length];

        try
        {
            // Create AesGcm instance with explicit tag size (prevents tag truncation attacks)
            using var aes = new AesGcm(_encryptionKey, TagSizeBytes);

            // Decrypt and verify authentication tag atomically
            // If tag verification fails, CryptographicException is thrown
            // Plaintext is NEVER returned if authentication fails
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
        }
        catch (CryptographicException ex)
        {
            // BDD Scenario 6: Handle corrupt ciphertext
            // BDD Scenario 7: Handle wrong encryption key (tag mismatch)
            throw new CryptographicException(
                "Decryption failed. Authentication tag mismatch or corrupted data. " +
                "This may indicate data tampering, wrong encryption key, or data corruption.",
                ex);
        }

        // Convert decrypted bytes to UTF-8 string
        // BDD Scenarios 1-3: Successfully decrypt valid encrypted fields
        return Encoding.UTF8.GetString(plaintext);
    }

    /// <summary>
    /// Internal record for deserializing encrypted JSON structure.
    /// Matches Equifax encrypted field format.
    /// </summary>
    private record EncryptedValue
    {
        /// <summary>Hex-encoded ciphertext (variable length)</summary>
        [System.Text.Json.Serialization.JsonPropertyName("ciphertext")]
        public string Ciphertext { get; init; } = string.Empty;

        /// <summary>Hex-encoded initialization vector / nonce (12 bytes = 24 hex chars)</summary>
        [System.Text.Json.Serialization.JsonPropertyName("iv")]
        public string Iv { get; init; } = string.Empty;

        /// <summary>Hex-encoded authentication tag (16 bytes = 32 hex chars)</summary>
        [System.Text.Json.Serialization.JsonPropertyName("tag")]
        public string Tag { get; init; } = string.Empty;
    }
}
