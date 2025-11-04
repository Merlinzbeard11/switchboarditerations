using Microsoft.Extensions.Configuration;
using CsvImporter.Configuration;
using CsvImporter.Services;

/// <summary>
/// Equifax CSV Bulk Importer
///
/// High-performance importer for 327M Equifax consumer enrichment records.
/// Uses Npgsql binary COPY command (100x faster than INSERT statements).
///
/// 3-Phase Import Process:
/// Phase 1: Database Preparation (5-10 min)
///   - Drop indexes
///   - Convert to UNLOGGED table
///   - Increase memory settings
///
/// Phase 2: Parallel CSV Import (32-42 hours)
///   - Download CSV files from S3
///   - Parse pipe-delimited 398 columns
///   - Decrypt encrypted fields (AES-256-GCM)
///   - Import using binary COPY (150K-200K rows/min)
///
/// Phase 3: Database Restoration (4-6 hours)
///   - Rebuild indexes
///   - Convert to LOGGED table
///   - Update statistics
///   - Verify record count
///
/// Usage:
///   dotnet run --project tools/csv-importer/CsvImporter.csproj
///
/// Configuration:
///   - appsettings.json: Import settings, database connection, S3 bucket
///   - Environment variables: AWS credentials, encryption keys
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.Clear();
        PrintBanner();

        try
        {
            // Load configuration
            var config = LoadConfiguration();
            ValidateConfiguration(config);

            // Initialize services
            var optimizer = new DatabaseOptimizer(config.Database.ConnectionString);
            var decryptor = new AesGcmDecryptor(config.Encryption.AesKeyHex);
            var parser = new CsvRowParser(decryptor, config.Encryption.EncryptedFields);
            var importer = new NpgsqlBulkImporter(config.Database.ConnectionString);
            var s3Streamer = new S3FileStreamer(
                config.Import.S3Bucket,
                config.Import.S3Prefix,
                config.Import.S3Region
            );

            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("Configuration Loaded:");
            Console.WriteLine($"  S3 Bucket: s3://{config.Import.S3Bucket}/{config.Import.S3Prefix}");
            Console.WriteLine($"  Workers: {config.Import.WorkerCount} parallel");
            Console.WriteLine($"  Batch Size: {config.Import.BatchSize:N0} records");
            Console.WriteLine($"  Database: {ExtractDatabaseName(config.Database.ConnectionString)}");
            Console.WriteLine($"  Decryption: {(decryptor.IsDecryptionEnabled ? "ENABLED" : "DISABLED (encrypted fields will be null)")}");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine();

            // Confirm before starting
            Console.Write("⚠️  This will import ~327M records to PostgreSQL. Continue? (y/N): ");
            var confirm = Console.ReadLine();
            if (!string.Equals(confirm, "y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("❌ Import cancelled by user.");
                return 1;
            }

            var overallStart = DateTime.UtcNow;

            // PHASE 1: Prepare Database
            Console.WriteLine();
            await optimizer.PrepareForImportAsync();

            // PHASE 2: Import CSV Files
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║ Phase 2: Importing CSV Files from S3                     ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // List all CSV files
            var csvFiles = await s3Streamer.ListCsvFilesAsync();
            Console.WriteLine($"📊 Total files to process: {csvFiles.Count:N0}");
            Console.WriteLine();

            // Process files in parallel
            Console.WriteLine($"🚀 Starting import with {config.Import.WorkerCount} parallel workers...");
            Console.WriteLine();

            var importResults = await s3Streamer.ProcessFilesInParallelAsync(
                csvFiles,
                importer,
                parser,
                maxParallelism: config.Import.WorkerCount
            );

            // PHASE 3: Restore Database
            Console.WriteLine();
            await optimizer.RestoreAfterImportAsync();

            // Print Summary
            PrintSummary(importResults, overallStart);

            Console.WriteLine();
            Console.WriteLine("✅ Import completed successfully!");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("❌ FATAL ERROR:");
            Console.WriteLine($"   {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Stack Trace:");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("═══════════════════════════════════════════════════════════");

            return 1;
        }
    }

    static ImportConfiguration LoadConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables(prefix: "EQUIFAX_")
            .Build();

        var config = new ImportConfiguration();
        configuration.Bind(config);

        return config;
    }

    static void ValidateConfiguration(ImportConfiguration config)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(config.Database.ConnectionString))
            errors.Add("Database connection string is required");

        if (string.IsNullOrWhiteSpace(config.Import.S3Bucket))
            errors.Add("S3 bucket name is required");

        if (config.Import.WorkerCount < 1 || config.Import.WorkerCount > 64)
            errors.Add("Worker count must be between 1 and 64");

        if (errors.Any())
        {
            Console.WriteLine("❌ Configuration Errors:");
            foreach (var error in errors)
            {
                Console.WriteLine($"   - {error}");
            }
            throw new InvalidOperationException("Invalid configuration. See errors above.");
        }
    }

    static void PrintBanner()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                                                           ║");
        Console.WriteLine("║        Equifax CSV Bulk Importer                          ║");
        Console.WriteLine("║        High-Performance PostgreSQL Import Tool            ║");
        Console.WriteLine("║                                                           ║");
        Console.WriteLine("║        Target: 326,718,517 records (327M)                 ║");
        Console.WriteLine("║        Method: Npgsql Binary COPY (100x faster)           ║");
        Console.WriteLine("║        Estimated: 32-42 hours                             ║");
        Console.WriteLine("║                                                           ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        Console.WriteLine();
    }

    static void PrintSummary(List<ImportResult> results, DateTime overallStart)
    {
        var successCount = results.Count(r => r.IsSuccess);
        var failureCount = results.Count(r => !r.IsSuccess);
        var totalRows = results.Sum(r => r.ImportedRows);
        var totalSkipped = results.Sum(r => r.SkippedRows);
        var overallDuration = DateTime.UtcNow - overallStart;
        var avgRate = totalRows / overallDuration.TotalMinutes;

        Console.WriteLine();
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║ Import Summary                                            ║");
        Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║ Files Processed: {results.Count:N0}                           ");
        Console.WriteLine($"║   - Successful: {successCount:N0}                              ");
        Console.WriteLine($"║   - Failed: {failureCount:N0}                                  ");
        Console.WriteLine($"║                                                           ║");
        Console.WriteLine($"║ Records Imported: {totalRows:N0}                         ");
        Console.WriteLine($"║ Records Skipped: {totalSkipped:N0}                       ");
        Console.WriteLine($"║                                                           ║");
        Console.WriteLine($"║ Total Duration: {overallDuration.TotalHours:F1} hours             ");
        Console.WriteLine($"║ Average Rate: {avgRate:N0} rows/minute                   ");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");

        if (failureCount > 0)
        {
            Console.WriteLine();
            Console.WriteLine("⚠️  Failed Files:");
            foreach (var failure in results.Where(r => !r.IsSuccess))
            {
                Console.WriteLine($"   - {failure.FileName}: {failure.ErrorMessage}");
            }
        }
    }

    static string ExtractDatabaseName(string connectionString)
    {
        var match = System.Text.RegularExpressions.Regex.Match(connectionString, @"Database=([^;]+)");
        return match.Success ? match.Groups[1].Value : "unknown";
    }
}
