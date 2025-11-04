namespace CsvImporter.Services;

/// <summary>
/// Parses pipe-delimited (|) CSV rows with 398 columns.
///
/// Equifax CSV Format:
/// - Delimiter: | (pipe)
/// - Columns: 398 (matches ConsumerEnrichment entity exactly)
/// - Header row: First row contains column names
/// - Encoding: UTF-8
/// - Line endings: \n or \r\n
/// - Null values: Empty string between delimiters (||)
///
/// Column Order (from EQUIFAX-DATABASE-COMPREHENSIVE-DOCUMENTATION.md):
/// 1-8: Personal Info (consumer_key, first_name, last_name, etc.)
/// 9-38: Alternate Names (30 columns)
/// 39-208: Addresses (170 columns: address_1-address_50, city_name_1-city_name_50, etc.)
/// 209-220: Phones (12 columns: mobile_phone_1-2, phone_1-10)
/// 221-250: Emails (30 columns)
/// 251-373: Credit Scores (123 columns)
/// 374-398: Financial Indicators (14 columns) + metadata
/// </summary>
public class CsvRowParser
{
    private const char Delimiter = '|';
    private const int ExpectedColumnCount = 398;
    private readonly AesGcmDecryptor _decryptor;
    private readonly string[] _encryptedFieldNames;

    public CsvRowParser(AesGcmDecryptor decryptor, string[] encryptedFieldNames)
    {
        _decryptor = decryptor;
        _encryptedFieldNames = encryptedFieldNames;
    }

    /// <summary>
    /// Parse a single CSV row into 398 field values.
    /// Returns null if row is invalid or doesn't have expected column count.
    /// </summary>
    public string[]? ParseRow(string csvLine, int lineNumber)
    {
        if (string.IsNullOrWhiteSpace(csvLine))
            return null;

        // Split by pipe delimiter
        var fields = csvLine.Split(Delimiter);

        // Validate column count
        if (fields.Length != ExpectedColumnCount)
        {
            Console.WriteLine($"⚠️  Line {lineNumber}: Expected {ExpectedColumnCount} columns, found {fields.Length}. Skipping row.");
            return null;
        }

        // Process fields (trim, handle nulls, decrypt if needed)
        for (int i = 0; i < fields.Length; i++)
        {
            // Trim whitespace
            fields[i] = fields[i].Trim();

            // Convert empty strings to null
            if (string.IsNullOrEmpty(fields[i]))
            {
                fields[i] = null!;
                continue;
            }

            // Decrypt if this field is in encrypted list
            // Note: Actual field name mapping would require column header mapping
            // For now, we'll decrypt fields that look like JSON encrypted format
            if (_decryptor.IsDecryptionEnabled && IsEncryptedFormat(fields[i]))
            {
                var decrypted = _decryptor.Decrypt(fields[i]);
                fields[i] = decrypted ?? fields[i]; // Keep original if decryption fails
            }
        }

        return fields;
    }

    /// <summary>
    /// Check if a field value looks like encrypted JSON format.
    /// Encrypted format: {"ciphertext":"...","iv":"...","tag":"..."}
    /// </summary>
    private bool IsEncryptedFormat(string value)
    {
        return value.StartsWith("{") &&
               value.Contains("\"ciphertext\"") &&
               value.Contains("\"iv\"") &&
               value.Contains("\"tag\"");
    }

    /// <summary>
    /// Parse CSV header row to get column names.
    /// Returns array of 398 column names in order.
    /// </summary>
    public string[]? ParseHeader(string headerLine)
    {
        if (string.IsNullOrWhiteSpace(headerLine))
            return null;

        var columns = headerLine.Split(Delimiter);

        if (columns.Length != ExpectedColumnCount)
        {
            Console.WriteLine($"❌ ERROR: CSV header has {columns.Length} columns, expected {ExpectedColumnCount}");
            return null;
        }

        // Trim column names
        for (int i = 0; i < columns.Length; i++)
        {
            columns[i] = columns[i].Trim();
        }

        return columns;
    }
}
