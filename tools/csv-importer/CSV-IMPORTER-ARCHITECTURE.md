# CSV Importer Architecture

## Overview

High-performance CSV importer for importing 327M Equifax consumer enrichment records from S3 to PostgreSQL using Npgsql binary COPY command.

## Performance Strategy

Based on authoritative documentation research:

### 1. Native PostgreSQL COPY Command (100x faster than INSERT)
- **Technology**: Npgsql `BeginBinaryImport()` with `COPY FROM STDIN (FORMAT BINARY)`
- **Performance**: 100x improvement over standard INSERT statements
- **Benchmark**: Binary mode is most efficient format for bulk data transfer

### 2. PostgreSQL Database Optimizations
- **UNLOGGED tables**: 50-200% performance improvement during import
- **Drop indexes**: Rebuild in single pass after import (much faster)
- **Memory tuning**: Increase `maintenance_work_mem` for faster index creation
- **Disable commits**: Set `synchronous_commit=off` during import

### 3. Parallel Processing
- **Multiple workers**: Process CSV files in parallel
- **Connection pooling**: Npgsql connection pool for parallel workers
- **File-level parallelism**: Each worker processes different S3 files

## Architecture Components

### Configuration
- `ImportConfiguration.cs`: Worker count, batch size, S3 bucket, connection strings

### Services
- `NpgsqlBulkImporter.cs`: Native COPY binary import (core performance)
- `DatabaseOptimizer.cs`: Pre/post-import database tuning
- `AesGcmDecryptor.cs`: Decrypt 26% of encrypted fields
- `CsvRowParser.cs`: Parse pipe-delimited 398-column CSV
- `S3FileStreamer.cs`: Stream CSV files from S3 bucket

### Workers
- `ParallelCsvWorker.cs`: Coordinate parallel file processing

## Import Workflow

### Phase 1: Database Preparation (5-10 minutes)
1. Backup current database (optional, recommended)
2. Drop all indexes on `consumer_enrichments` table
3. Convert table to UNLOGGED (disable WAL)
4. Increase PostgreSQL memory settings:
   - `maintenance_work_mem = 2GB`
   - `work_mem = 256MB`
5. Set `synchronous_commit = off`

### Phase 2: Parallel CSV Import (40-50 hours estimated)
1. List all 10,814 CSV files from S3 bucket
2. Distribute files across N parallel workers (configurable, recommend 8-16)
3. Each worker:
   - Download CSV file from S3 (streaming, not full download)
   - Parse each row (398 pipe-delimited fields)
   - Decrypt encrypted fields using AES-256-GCM
   - Use Npgsql binary COPY to import batches
4. Progress monitoring: Log file count, record count, errors
5. Error handling: Retry failed files, log problematic records

### Phase 3: Post-Import Optimization (4-6 hours)
1. Convert table to LOGGED (enable WAL)
2. Rebuild all indexes in parallel
3. Run ANALYZE to update statistics
4. Verify record count (expect 326,718,517 records)
5. Reset PostgreSQL settings to defaults

## Performance Estimates

**Source Data:**
- Files: 10,814 CSV files
- Records: 326,718,517 total
- Columns: 398 per record
- Encrypted: 26% of fields (AES-256-GCM)

**With Npgsql Binary COPY + Optimizations:**
- Import speed: ~150,000-200,000 rows/minute (conservative with decryption overhead)
- Total import time: 27-36 hours (import only)
- Database prep: 5-10 minutes
- Index rebuild: 4-6 hours
- **Total runtime: 32-42 hours** (vs 54-60 hours with EF Core)

## Data Integrity Safeguards

1. **Transaction-based batches**: Rollback on error within batch
2. **Checksum validation**: Verify record counts match source
3. **Duplicate detection**: Use `consumer_key` unique constraint
4. **Error logging**: Log all failed records to `import_errors.log`
5. **Retry mechanism**: Retry failed files up to 3 times
6. **Validation queries**: Run sample queries to verify data integrity

## Configuration

### appsettings.json
```json
{
  "Import": {
    "S3Bucket": "sb-marketing-migration",
    "S3Prefix": "equifax-export/full-export/",
    "WorkerCount": 16,
    "BatchSize": 10000,
    "RetryAttempts": 3
  },
  "Database": {
    "ConnectionString": "Host=localhost;Port=5432;Database=equifax_enrichment_api_dev;Username=postgres;Password=postgres"
  },
  "Encryption": {
    "AesKeyHex": "REPLACE_WITH_ACTUAL_KEY"
  }
}
```

## Monitoring Dashboard Output

```
╔═══════════════════════════════════════════════════════════╗
║ Equifax CSV Import - Real-time Progress                  ║
╠═══════════════════════════════════════════════════════════╣
║ Phase: Importing Data                                     ║
║ Workers: 16 active                                        ║
║ Files Processed: 1,234 / 10,814 (11.4%)                  ║
║ Records Imported: 37,823,451 / 326,718,517 (11.6%)       ║
║ Import Speed: 187,234 records/min                         ║
║ ETA: 26.4 hours                                           ║
║ Errors: 23 records (logged to import_errors.log)         ║
╚═══════════════════════════════════════════════════════════╝
```

## Error Handling

### Retry Strategy
- File download failures: Retry up to 3 times with exponential backoff
- Parse errors: Log problematic record, continue with next
- Database errors: Rollback batch, retry entire file
- Decryption errors: Log record, mark as encrypted data invalid

### Error Logging Format
```
[2025-11-04 02:30:15] ERROR | File: part-00123.csv | Row: 4,567 | Error: Decryption failed for field 'ssn' | Raw: {...}
```

## Post-Import Verification

1. **Record count**: `SELECT COUNT(*) FROM consumer_enrichments;` (expect 326,718,517)
2. **Phone distribution**: Verify phones distributed across mobile_phone_1, mobile_phone_2, etc.
3. **Sample queries**: Test API endpoint lookups for known phone numbers
4. **Index verification**: Confirm all indexes rebuilt successfully
5. **Data integrity**: Run spot-check queries for data quality

## Known Gotchas (from Research)

### EF Core Bulk Insert
- ❌ **Avoid**: EF Core `AddRangeAsync` - too slow for 327M records
- ❌ **Avoid**: EFCore.BulkExtensions - 5x improvement but still slower than native COPY
- ✅ **Use**: Npgsql binary COPY command (100x faster)

### PostgreSQL Index Management
- ❌ **Don't**: Keep indexes enabled during import (dramatic slowdown)
- ✅ **Do**: Drop indexes, import, rebuild in one pass

### UNLOGGED Tables
- ⚠️ **Warning**: UNLOGGED tables are NOT crash-safe
- ✅ **Mitigation**: Convert back to LOGGED immediately after import
- ℹ️ **Note**: Can use `ALTER TABLE ... SET LOGGED` in PostgreSQL 9.5+

### Connection Pooling
- ❌ **Don't**: Create new connection for each batch
- ✅ **Do**: Use Npgsql connection pooling (configured in connection string)
- ℹ️ **Setting**: `Pooling=true;MinPoolSize=8;MaxPoolSize=32`
