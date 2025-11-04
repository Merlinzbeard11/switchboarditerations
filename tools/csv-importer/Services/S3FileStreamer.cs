using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace CsvImporter.Services;

/// <summary>
/// Streams CSV files from AWS S3 bucket for bulk import.
///
/// Source: S3 bucket "sb-marketing-migration/equifax-export/full-export/"
/// Files: 10,814 CSV files (part-00000.csv through part-10813.csv)
/// Total size: ~1 TB compressed
///
/// Streaming Strategy:
/// - List all CSV files in S3 prefix
/// - Download one file at a time (avoid storing 1TB locally)
/// - Stream directly to parser/importer
/// - Clean up local temp file after import
///
/// Performance:
/// - AWS SDK async streaming for memory efficiency
/// - Download to local temp file first (faster than streaming parse)
/// - Parallel workers can download different files simultaneously
/// </summary>
public class S3FileStreamer
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _prefix;

    public S3FileStreamer(string bucketName, string prefix, string region)
    {
        _bucketName = bucketName;
        _prefix = prefix;

        var regionEndpoint = RegionEndpoint.GetBySystemName(region);
        _s3Client = new AmazonS3Client(regionEndpoint);

        Console.WriteLine($"✅ S3 client initialized");
        Console.WriteLine($"   Bucket: {bucketName}");
        Console.WriteLine($"   Prefix: {prefix}");
        Console.WriteLine($"   Region: {region}");
    }

    /// <summary>
    /// List all CSV files in the S3 bucket/prefix.
    /// Returns list of S3 object keys.
    /// </summary>
    public async Task<List<string>> ListCsvFilesAsync(CancellationToken cancellationToken = default)
    {
        Console.Write($"📋 Listing CSV files in s3://{_bucketName}/{_prefix}... ");

        var csvFiles = new List<string>();
        var request = new ListObjectsV2Request
        {
            BucketName = _bucketName,
            Prefix = _prefix
        };

        ListObjectsV2Response response;
        do
        {
            response = await _s3Client.ListObjectsV2Async(request, cancellationToken);

            foreach (var obj in response.S3Objects)
            {
                // Only include .csv files (skip directories, metadata files, etc.)
                if (obj.Key.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    csvFiles.Add(obj.Key);
                }
            }

            request.ContinuationToken = response.NextContinuationToken;

        } while (response.IsTruncated == true);

        Console.WriteLine($"Found {csvFiles.Count:N0} CSV files");
        return csvFiles;
    }

    /// <summary>
    /// Download a single CSV file from S3 to local temp directory.
    /// Returns path to local temp file.
    /// Caller is responsible for deleting temp file after use.
    /// </summary>
    public async Task<string> DownloadCsvFileAsync(string s3Key, CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(s3Key);
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"equifax-import-{fileName}");

        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = s3Key
            };

            using var response = await _s3Client.GetObjectAsync(request, cancellationToken);
            await using var responseStream = response.ResponseStream;
            await using var fileStream = File.Create(tempFilePath);

            await responseStream.CopyToAsync(fileStream, cancellationToken);

            return tempFilePath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR downloading {s3Key}: {ex.Message}");

            // Clean up partial download
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }

            throw;
        }
    }

    /// <summary>
    /// Get metadata for a specific S3 object (file size, last modified, etc.).
    /// </summary>
    public async Task<S3FileMetadata> GetFileMetadataAsync(string s3Key, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = s3Key
            };

            var response = await _s3Client.GetObjectMetadataAsync(request, cancellationToken);

            return new S3FileMetadata
            {
                Key = s3Key,
                SizeBytes = response.ContentLength,
                LastModified = response.LastModified ?? DateTime.UtcNow,
                ETag = response.ETag ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  WARNING: Could not get metadata for {s3Key}: {ex.Message}");
            return new S3FileMetadata { Key = s3Key };
        }
    }

    /// <summary>
    /// Download and process multiple CSV files in parallel.
    /// Each file is downloaded, processed, and deleted before next download.
    /// </summary>
    public async Task<List<ImportResult>> ProcessFilesInParallelAsync(
        List<string> s3Keys,
        NpgsqlBulkImporter importer,
        CsvRowParser parser,
        int maxParallelism = 8,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ImportResult>();
        var semaphore = new SemaphoreSlim(maxParallelism);

        var tasks = s3Keys.Select(async s3Key =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                return await ProcessSingleFileAsync(s3Key, importer, parser, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        });

        var importResults = await Task.WhenAll(tasks);
        return importResults.ToList();
    }

    /// <summary>
    /// Process a single S3 file: download, import, cleanup.
    /// </summary>
    private async Task<ImportResult> ProcessSingleFileAsync(
        string s3Key,
        NpgsqlBulkImporter importer,
        CsvRowParser parser,
        CancellationToken cancellationToken)
    {
        string? tempFilePath = null;

        try
        {
            // Download from S3
            Console.WriteLine($"📥 Downloading: {Path.GetFileName(s3Key)}");
            tempFilePath = await DownloadCsvFileAsync(s3Key, cancellationToken);

            // Import to database
            Console.WriteLine($"💾 Importing: {Path.GetFileName(s3Key)}");
            var result = await importer.ImportCsvFileAsync(tempFilePath, parser, cancellationToken: cancellationToken);

            Console.WriteLine($"✅ Completed: {Path.GetFileName(s3Key)} - {result.ImportedRows:N0} rows in {result.Duration.TotalMinutes:F1} min");

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed: {Path.GetFileName(s3Key)} - {ex.Message}");
            return new ImportResult
            {
                FileName = Path.GetFileName(s3Key),
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
        finally
        {
            // Clean up temp file
            if (tempFilePath != null && File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️  WARNING: Could not delete temp file {tempFilePath}: {ex.Message}");
                }
            }
        }
    }
}

/// <summary>
/// Metadata about an S3 file.
/// </summary>
public class S3FileMetadata
{
    public string Key { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime LastModified { get; set; }
    public string ETag { get; set; } = string.Empty;

    public double SizeMB => SizeBytes / (1024.0 * 1024.0);
    public double SizeGB => SizeBytes / (1024.0 * 1024.0 * 1024.0);
}
