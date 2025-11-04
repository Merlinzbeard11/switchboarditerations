using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CsvImporter.Services;

/// <summary>
/// Decrypts AES-256-GCM encrypted fields from Equifax CSV data.
///
/// Encrypted Format (JSON string in CSV field):
/// {
///   "ciphertext": "hex_encoded_encrypted_data",
///   "iv": "hex_encoded_initialization_vector",
///   "tag": "hex_encoded_authentication_tag"
/// }
///
/// 26% of fields are encrypted: SSN, DOB, driver license numbers, etc.
/// </summary>
public class AesGcmDecryptor
{
    private readonly byte[] _key;
    private readonly bool _isKeyValid;

    public AesGcmDecryptor(string aesKeyHex)
    {
        if (string.IsNullOrWhiteSpace(aesKeyHex) || aesKeyHex.Contains("PLACEHOLDER"))
        {
            Console.WriteLine("⚠️  WARNING: AES encryption key not configured!");
            Console.WriteLine("   Encrypted fields will be skipped during import.");
            Console.WriteLine("   Set Encryption:AesKeyHex in appsettings.json or AWS_AES_KEY environment variable.");
            _key = Array.Empty<byte>();
            _isKeyValid = false;
            return;
        }

        try
        {
            _key = Convert.FromHexString(aesKeyHex);
            _isKeyValid = true;
            Console.WriteLine($"✅ AES-256-GCM decryption key loaded ({_key.Length * 8} bits)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR: Invalid AES key hex string: {ex.Message}");
            _key = Array.Empty<byte>();
            _isKeyValid = false;
        }
    }

    /// <summary>
    /// Decrypt an encrypted field value.
    /// Returns null if decryption fails or key not configured.
    /// </summary>
    public string? Decrypt(string? encryptedJson)
    {
        if (string.IsNullOrWhiteSpace(encryptedJson))
            return null;

        if (!_isKeyValid)
            return null; // Skip decryption if key not configured

        try
        {
            var encrypted = JsonSerializer.Deserialize<EncryptedValue>(encryptedJson);
            if (encrypted == null)
                return null;

            using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);

            var ciphertext = Convert.FromHexString(encrypted.Ciphertext);
            var iv = Convert.FromHexString(encrypted.Iv);
            var tag = Convert.FromHexString(encrypted.Tag);
            var plaintext = new byte[ciphertext.Length];

            aes.Decrypt(iv, ciphertext, tag, plaintext);
            return Encoding.UTF8.GetString(plaintext);
        }
        catch (Exception ex)
        {
            // Log decryption failure but continue processing
            // Don't throw - encrypted field will be null
            Console.WriteLine($"⚠️  Decryption failed: {ex.Message} | Data: {encryptedJson?.Substring(0, Math.Min(50, encryptedJson.Length))}...");
            return null;
        }
    }

    /// <summary>
    /// Check if decryption is enabled (key configured).
    /// </summary>
    public bool IsDecryptionEnabled => _isKeyValid;

    private class EncryptedValue
    {
        public string Ciphertext { get; set; } = string.Empty;
        public string Iv { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
    }
}
