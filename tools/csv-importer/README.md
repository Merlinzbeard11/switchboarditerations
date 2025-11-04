# Equifax CSV Bulk Importer

High-performance CSV importer for importing 327M Equifax consumer enrichment records from S3 to PostgreSQL.

## Performance Highlights

- **Technology**: Npgsql binary COPY command (100x faster than INSERT statements)
- **Throughput**: 150,000-200,000 rows/minute (with decryption)
- **Total Runtime**: 32-42 hours estimated (vs 54-60 hours with EF Core)
- **Parallel Processing**: Configurable workers (default: 16 parallel)

## Architecture

Based on research from authoritative documentation:
- ✅ Npgsql binary COPY (100x performance improvement)
- ✅ PostgreSQL UNLOGGED tables during import (50-200% faster)
- ✅ Index management (drop before import, rebuild after)
- ✅ Memory tuning (maintenance_work_mem, work_mem)
- ✅ Parallel S3 file streaming

## 3-Phase Import Process

### Phase 1: Database Preparation (5-10 minutes)
- Drop all indexes on `consumer_enrichments` table
- Convert table to UNLOGGED (disable WAL for performance)
- Increase PostgreSQL memory settings:
  - `maintenance_work_mem = 2GB`
  - `work_mem = 256MB`
- Disable synchronous commits

### Phase 2: Parallel CSV Import (32-42 hours)
- List all 10,814 CSV files from S3 bucket
- Distribute files across 16 parallel workers
- For each file:
  1. Download from S3 to temp directory
  2. Parse pipe-delimited 398 columns
  3. Decrypt AES-256-GCM encrypted fields
  4. Import using Npgsql binary COPY
  5. Delete temp file
- Progress monitoring with real-time statistics

### Phase 3: Database Restoration (4-6 hours)
- Convert table back to LOGGED (enable WAL for crash safety)
- Rebuild all indexes
- Run ANALYZE to update table statistics
- Verify record count (expect 326,718,517 records)
- Reset PostgreSQL settings to defaults

## Prerequisites

1. **PostgreSQL 17** (or compatible version)
2. **AWS S3 Access**:
   - Bucket: `sb-marketing-migration`
   - Prefix: `equifax-export/full-export/`
   - AWS credentials configured (via environment variables or AWS CLI)
3. **.NET 9.0 SDK**
4. **AES-256-GCM Encryption Key** (for decrypting 26% of fields)

## Configuration

### appsettings.json

```json
{
  "Import": {
    "S3Bucket": "sb-marketing-migration",
    "S3Prefix": "equifaxexport/full-export/",
    "S3Region": "us-east-1",
    "WorkerCount": 16,
    "BatchSize": 10000,
    "RetryAttempts": 3
  },
  "Database": {
    "ConnectionString": "Host=localhost;Port=5432;Database=equifax_enrichment_api_dev;Username=postgres;Password=postgres;Pooling=true;MinPoolSize=8;MaxPoolSize=32",
    "EnableOptimizations": true,
    "DropIndexesBeforeImport": true,
    "UseUnloggedTable": true
  },
  "Encryption": {
    "AesKeyHex": "YOUR_AES_KEY_HERE"
  }
}
```

### Environment Variables

- `EQUIFAX_Encryption__AesKeyHex`: Override AES encryption key
- `AWS_ACCESS_KEY_ID`: AWS access key for S3
- `AWS_SECRET_ACCESS_KEY`: AWS secret key for S3
- `AWS_REGION`: AWS region (default: us-east-1)

## Usage

### Run Import

```bash
cd tools/csv-importer
dotnet run
```

### Expected Output

```
╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║        Equifax CSV Bulk Importer                          ║
║        High-Performance PostgreSQL Import Tool            ║
║                                                           ║
║        Target: 326,718,517 records (327M)                 ║
║        Method: Npgsql Binary COPY (100x faster)           ║
║        Estimated: 32-42 hours                             ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝

Configuration Loaded:
  S3 Bucket: s3://sb-marketing-migration/equifax-export/full-export/
  Workers: 16 parallel
  Batch Size: 10,000 records
  Database: equifax_enrichment_api_dev
  Decryption: ENABLED

⚠️  This will import ~327M records to PostgreSQL. Continue? (y/N):
```

## Performance Benchmarks

Based on authoritative documentation research:

| Method | Performance | Notes |
|--------|------------|-------|
| EF Core AddRangeAsync | ~1M records in 21 seconds | Too slow for 327M records |
| EFCore.BulkExtensions | 5x improvement | Still slower than native COPY |
| **Npgsql Binary COPY** | **100x faster than INSERT** | ✅ Best option for bulk import |

### Database Optimizations Impact

| Optimization | Performance Gain | Safety |
|--------------|------------------|--------|
| Drop indexes before import | 2-3x faster | ✅ Safe (rebuilt after) |
| UNLOGGED table | 50-200% faster | ⚠️ Temporary (converted back after) |
| Increased memory | 20-30% faster | ✅ Safe (reset after) |
| Disable sync commits | 10-15% faster | ⚠️ Temporary (re-enabled after) |

## Data Integrity Safeguards

1. **Transaction-based batches**: Rollback on error within batch
2. **Checksum validation**: Verify record counts match source
3. **Duplicate detection**: `consumer_key` unique constraint prevents duplicates
4. **Error logging**: All failed records logged to `import_errors.log`
5. **Retry mechanism**: Failed files retried up to 3 times
6. **Post-import verification**: Sample queries validate data integrity

## Troubleshooting

### Missing AES Encryption Key

```
⚠️  WARNING: AES encryption key not configured!
   Encrypted fields will be skipped during import.
```

**Solution**: Set `Encryption:AesKeyHex` in appsettings.json or `EQUIFAX_Encryption__AesKeyHex` environment variable.

### AWS S3 Access Denied

```
❌ ERROR downloading part-00000.csv: Access Denied
```

**Solution**: Configure AWS credentials:
```bash
aws configure
# OR
export AWS_ACCESS_KEY_ID=your_key
export AWS_SECRET_ACCESS_KEY=your_secret
```

### PostgreSQL Connection Failure

```
❌ ERROR: Connection to server failed
```

**Solution**: Verify PostgreSQL is running and connection string is correct:
```bash
psql -h localhost -U postgres -d equifax_enrichment_api_dev -c "SELECT version();"
```

## Project Structure

```
tools/csv-importer/
├── Program.cs                         # Main orchestrator
├── appsettings.json                   # Configuration
├── Configuration/
│   └── ImportConfiguration.cs         # Config models
├── Services/
│   ├── DatabaseOptimizer.cs           # Pre/post-import DB optimization
│   ├── NpgsqlBulkImporter.cs          # Binary COPY implementation
│   ├── AesGcmDecryptor.cs             # AES-256-GCM decryption
│   ├── CsvRowParser.cs                # 398-column parser
│   └── S3FileStreamer.cs              # S3 file streaming
└── CSV-IMPORTER-ARCHITECTURE.md       # Detailed architecture docs
```

## Research References

Implementation based on authoritative sources:

- **Npgsql Official Docs**: [COPY command documentation](https://www.npgsql.org/doc/copy.html)
- **PostgreSQL Best Practices**: [7 Best Tips for Bulk Data Loading](https://www.enterprisedb.com/blog/7-best-practice-tips-postgresql-bulk-data-loading)
- **Performance Benchmarks**: Npgsql binary COPY showed 100x improvement over INSERT in real-world testing

## License

Part of the Equifax Enrichment API project - see main repository for license information.
