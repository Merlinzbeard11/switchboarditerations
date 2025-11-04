namespace EquifaxEnrichmentAPI.Application.Configuration;

/// <summary>
/// Configuration options for AES-GCM encryption/decryption.
/// Loaded via IOptions pattern from appsettings.json or Azure Key Vault.
///
/// BDD Feature: AES-GCM Decryption Service for Equifax Encrypted Fields
/// BDD File: features/phase1/feature-1.4-aes-gcm-decryption.feature
///
/// SECURITY: Never hardcode encryption keys in source code.
/// Use configuration management (appsettings, environment variables, Key Vault).
/// </summary>
public class EncryptionOptions
{
    /// <summary>
    /// Configuration section name for binding.
    /// Usage: builder.Services.Configure&lt;EncryptionOptions&gt;(config.GetSection(EncryptionOptions.SectionName))
    /// </summary>
    public const string SectionName = "Encryption";

    /// <summary>
    /// AES-256-GCM encryption key in hexadecimal format.
    /// Must be exactly 64 hex characters (32 bytes = 256 bits).
    ///
    /// Example: "4142505A44514850594D493346463758424F59505A45424D4E345931364E5A50"
    /// Decodes to: "ABPZDQHPYMI3FF7XBOYPZEBMN4Y16NZP" (32 bytes)
    ///
    /// CRITICAL SECURITY: This is the Equifax decryption key.
    /// - Production: Load from Azure Key Vault
    /// - Development: Load from User Secrets or appsettings.Development.json
    /// - NEVER commit actual key to source control
    /// </summary>
    public string EquifaxKeyHex { get; set; } = string.Empty;

    /// <summary>
    /// Validates that the encryption key is properly configured.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if key is missing or invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(EquifaxKeyHex))
        {
            throw new InvalidOperationException(
                "Encryption key not configured. Set Encryption:EquifaxKeyHex in configuration.");
        }

        if (EquifaxKeyHex.Length != 64)
        {
            throw new InvalidOperationException(
                $"Invalid encryption key length. Expected 64 hex characters (32 bytes), got {EquifaxKeyHex.Length}.");
        }

        // Validate it's valid hexadecimal
        try
        {
            _ = Convert.FromHexString(EquifaxKeyHex);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException(
                "Invalid encryption key format. Must be valid hexadecimal string.");
        }
    }
}
