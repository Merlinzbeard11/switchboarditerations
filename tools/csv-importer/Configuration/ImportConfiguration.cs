namespace CsvImporter.Configuration;

/// <summary>
/// Configuration settings for CSV import process.
/// Loaded from appsettings.json with environment variable overrides.
/// </summary>
public class ImportConfiguration
{
    public ImportSettings Import { get; set; } = new();
    public DatabaseSettings Database { get; set; } = new();
    public EncryptionSettings Encryption { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
}

public class ImportSettings
{
    public string S3Bucket { get; set; } = string.Empty;
    public string S3Prefix { get; set; } = string.Empty;
    public string S3Region { get; set; } = "us-east-1";
    public int WorkerCount { get; set; } = 16;
    public int BatchSize { get; set; } = 10000;
    public int RetryAttempts { get; set; } = 3;
    public bool EnableProgressMonitoring { get; set; } = true;
    public int ProgressUpdateIntervalSeconds { get; set; } = 10;
}

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeoutSeconds { get; set; } = 600;
    public bool EnableOptimizations { get; set; } = true;
    public bool DropIndexesBeforeImport { get; set; } = true;
    public bool UseUnloggedTable { get; set; } = true;
    public int MaintenanceWorkMemMB { get; set; } = 2048;
    public int WorkMemMB { get; set; } = 256;
}

public class EncryptionSettings
{
    public string AesKeyHex { get; set; } = string.Empty;
    public string[] EncryptedFields { get; set; } = Array.Empty<string>();
}

public class LoggingSettings
{
    public string ErrorLogPath { get; set; } = "./import_errors.log";
    public string ProgressLogPath { get; set; } = "./import_progress.log";
    public bool VerboseLogging { get; set; } = true;
}
