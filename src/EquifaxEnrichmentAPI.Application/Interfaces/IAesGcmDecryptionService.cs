namespace EquifaxEnrichmentAPI.Application.Interfaces;

/// <summary>
/// Service for decrypting AES-256-GCM encrypted fields from Equifax CSV data.
/// Handles decryption of 103 encrypted fields (26% of 398 total columns).
///
/// BDD Feature: AES-GCM Decryption Service for Equifax Encrypted Fields
/// BDD File: features/phase1/feature-1.4-aes-gcm-decryption.feature
///
/// Encrypted Format (JSON):
/// {
///   "ciphertext": "hex_encoded_ciphertext",
///   "iv": "hex_encoded_12_byte_nonce",
///   "tag": "hex_encoded_16_byte_authentication_tag"
/// }
///
/// Encrypted Fields (103 total):
/// - consumer_key (1)
/// - Personal names: prefix, first_name, middle_name, last_name, suffix, date_of_birth (6)
/// - Alternate names: 5 sets of 6 fields each (30)
/// - Address components: 10 addresses with street details (60)
/// - Device IDs: ipaddress1, ipaddress2, idfa1-5 (7)
///
/// Plain Text Fields (295 total):
/// - Phones: mobile_phone_1, mobile_phone_2, phone_1-5 (12)
/// - Emails: email_1-15 with last_seen dates (30)
/// - Financial: income360_complete, spending_power, etc. (14)
/// - Credit Scores: Vantage scores and propensity models (123)
/// - Metadata: uuid, marketing flags, revision (116)
///
/// SECURITY NOTES:
/// - Uses AES-256-GCM (Galois/Counter Mode) for authenticated encryption
/// - 12-byte nonce (IV) must be unique per encryption operation
/// - 16-byte authentication tag verifies data integrity
/// - Decrypt() method automatically verifies tag before returning plaintext
/// - NEVER use decrypted data before tag verification (security critical)
/// </summary>
public interface IAesGcmDecryptionService
{
    /// <summary>
    /// Decrypts an AES-GCM encrypted field from Equifax CSV data.
    ///
    /// BDD Scenarios:
    /// - Scenario 1: Successfully decrypt valid encrypted consumer_key
    /// - Scenario 2: Successfully decrypt encrypted name field
    /// - Scenario 3: Successfully decrypt encrypted address field with special characters
    /// - Scenario 4: Handle null or empty encrypted value gracefully (returns null)
    /// - Scenario 5: Handle invalid JSON format (throws exception)
    /// - Scenario 6: Handle corrupt ciphertext gracefully (throws exception)
    /// - Scenario 7: Handle wrong encryption key (throws exception with tag mismatch)
    ///
    /// Encrypted JSON Format:
    /// {
    ///   "ciphertext": "a1b2c3d4...",  // Hex-encoded encrypted data
    ///   "iv": "e5f6a7b8...",            // Hex-encoded 12-byte nonce
    ///   "tag": "c9d0e1f2..."            // Hex-encoded 16-byte authentication tag
    /// }
    /// </summary>
    /// <param name="encryptedJson">
    /// Encrypted field in JSON format with ciphertext, iv (nonce), and tag.
    /// Can be null or empty (returns null).
    /// </param>
    /// <returns>
    /// Decrypted UTF-8 string, or null if input is null/empty.
    /// </returns>
    /// <exception cref="System.Text.Json.JsonException">
    /// Thrown if encryptedJson is not valid JSON or missing required fields.
    /// </exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">
    /// Thrown if:
    /// - Authentication tag verification fails (data tampered or wrong key)
    /// - Ciphertext is corrupted
    /// - IV (nonce) is invalid size (must be 12 bytes)
    /// - Tag is invalid size (must be 16 bytes for cross-platform compatibility)
    /// </exception>
    /// <exception cref="FormatException">
    /// Thrown if hex strings in JSON are not valid hexadecimal.
    /// </exception>
    string? Decrypt(string? encryptedJson);
}
