# 398-COLUMN SCHEMA IMPLEMENTATION ROADMAP
## Equifax API Database - 100% Data Mirroring

**Decision**: 100% mirror of 398-column Equifax source schema
**Rationale**: Data is an asset - cannot recreate lost fields
**Status**: In Progress - Entity Model Complete
**Date**: 2025-11-03

---

## EXECUTIVE DECISION

User directive: *"we need a 100% mirror of the 398 columns in our api database. data is an asset and we need 100% of the data in our api database no questions asked."*

This is NON-NEGOTIABLE. Schema must mirror all 398 Equifax columns with zero data loss.

---

## IMPLEMENTATION STATUS

### ✅ COMPLETED

1. **Schema Analysis** (docs/analysis/equifax-complete-column-assessment.md)
   - Documented all 398 columns with categories
   - Identified encryption patterns (26% encrypted)
   - Corrected financial data availability (14 fields exist)
   - Created field mapping from CSV to API schema

2. **Entity Model Generation** (src/EquifaxEnrichmentAPI.Domain/Entities/ConsumerEnrichment.cs)
   - Generated 398-column entity class (1359 lines)
   - Auto-generated from schema.sql using Python script
   - Preserves all Equifax fields with proper C# types
   - Includes backward compatibility fields (NormalizedPhone, MatchConfidence)

3. **Entity Generator Tool** (tools/generate-consumer-enrichment-entity.py)
   - Automated generation from schema.sql
   - Prevents manual errors in 398-column entity
   - Repeatable for future schema updates

### 🔄 IN PROGRESS

4. **Entity Configuration Update** (src/EquifaxEnrichmentAPI.Infrastructure/Persistence/Configurations/ConsumerEnrichmentConfiguration.cs)
   - Need to map all 398 columns in Fluent API
   - Configure indexes for performance
   - Set column types and constraints

### ⏳ PENDING

5. **EF Core Migration Generation**
   ```bash
   dotnet ef migrations add MirrorEquifax398ColumnSchema \
       --context EnrichmentDbContext \
       --project src/EquifaxEnrichmentAPI.Infrastructure \
       --startup-project src/EquifaxEnrichmentAPI.Api
   ```

6. **Repository Updates** (src/EquifaxEnrichmentAPI.Infrastructure/Repositories/EnrichmentRepository.cs)
   - Update phone search to use MobilePhone1, MobilePhone2, Phone1-5
   - Add email search capability (15 emails)
   - Remove JSON parsing logic (data now in native columns)

7. **DTO Updates** (src/EquifaxEnrichmentAPI.Api/DTOs/)
   - Expand LookupResponseDto to expose new fields
   - Add Email DTOs (15 email addresses)
   - Add Alternate Name DTOs (5 name sets)
   - Add Financial DTOs (14 financial fields)
   - Add Credit Score DTOs (123 Vantage scores)

8. **CSV Import Pipeline** (tools/csv-importer/)
   - Build AES-GCM decryption module
   - Parse 10,814 CSV files from S3
   - Map CSV columns to entity properties
   - Batch insert with performance monitoring

9. **Decryption Key Configuration**
   - Locate AES-GCM encryption keys
   - Configure key storage (AWS Secrets Manager?)
   - Test decryption on sample encrypted fields

10. **Database Migration Execution**
    ```bash
    dotnet ef database update --context EnrichmentDbContext
    ```

---

## DETAILED IMPLEMENTATION PLAN

### PHASE 1: ENTITY CONFIGURATION (NEXT STEP)

**File**: `src/EquifaxEnrichmentAPI.Infrastructure/Persistence/Configurations/ConsumerEnrichmentConfiguration.cs`

**Changes Required**:
```csharp
public class ConsumerEnrichmentConfiguration : IEntityTypeConfiguration<ConsumerEnrichment>
{
    public void Configure(EntityTypeBuilder<ConsumerEnrichment> builder)
    {
        builder.ToTable("consumer_enrichments");
        builder.HasKey(e => e.Id);

        // ====================================================================
        // IDENTITY
        // ====================================================================
        builder.Property(e => e.ConsumerKey)
            .HasColumnName("consumer_key")
            .HasColumnType("text")
            .IsRequired();

        builder.HasIndex(e => e.ConsumerKey)
            .HasDatabaseName("ix_consumer_enrichments_consumer_key");

        // ====================================================================
        // PRIMARY PERSONAL INFO (11 columns)
        // ====================================================================
        builder.Property(e => e.Prefix).HasColumnName("prefix").HasColumnType("text");
        builder.Property(e => e.FirstName).HasColumnName("first_name").HasColumnType("text");
        builder.Property(e => e.MiddleName).HasColumnName("middle_name").HasColumnType("text");
        builder.Property(e => e.LastName).HasColumnName("last_name").HasColumnType("text");
        builder.Property(e => e.Suffix).HasColumnName("suffix").HasColumnType("text");
        builder.Property(e => e.Gender).HasColumnName("gender").HasColumnType("text");
        builder.Property(e => e.DateOfBirth).HasColumnName("date_of_birth").HasColumnType("text");
        builder.Property(e => e.Age).HasColumnName("age").HasColumnType("text");
        builder.Property(e => e.Deceased).HasColumnName("deceased").HasColumnType("text");
        builder.Property(e => e.FirstSeenDatePrimaryName).HasColumnName("first_seen_date_primary_name").HasColumnType("text");
        builder.Property(e => e.LastSeenDatePrimaryName).HasColumnName("last_seen_date_primary_name").HasColumnType("text");

        // ... ALL 398 COLUMNS MAPPED ...

        // ====================================================================
        // PHONE NUMBERS (12 columns)
        // ====================================================================
        builder.Property(e => e.MobilePhone1).HasColumnName("mobile_phone_1").HasColumnType("text");
        builder.Property(e => e.MobilePhone2).HasColumnName("mobile_phone_2").HasColumnType("text");
        builder.Property(e => e.Phone1).HasColumnName("phone_1").HasColumnType("text");
        // ... all phone fields

        // ====================================================================
        // INDEXES FOR PERFORMANCE
        // ====================================================================

        // Phone search indexes
        builder.HasIndex(e => e.MobilePhone1)
            .HasDatabaseName("ix_consumer_enrichments_mobile_phone_1");
        builder.HasIndex(e => e.Phone1)
            .HasDatabaseName("ix_consumer_enrichments_phone_1");

        // Email search indexes
        builder.HasIndex(e => e.Email1)
            .HasDatabaseName("ix_consumer_enrichments_email_1");

        // Computed column for backward compatibility
        builder.Property(e => e.NormalizedPhone)
            .HasComputedColumnSql("COALESCE(mobile_phone_1, phone_1)", stored: true);

        builder.HasIndex(e => e.NormalizedPhone)
            .HasDatabaseName("ix_consumer_enrichments_normalized_phone");
    }
}
```

**Estimated Time**: 2-3 hours (map all 398 columns)

---

### PHASE 2: MIGRATION GENERATION

**Command**:
```bash
cd src/EquifaxEnrichmentAPI.Infrastructure
dotnet ef migrations add MirrorEquifax398ColumnSchema \
    --context EnrichmentDbContext \
    --startup-project ../EquifaxEnrichmentAPI.Api \
    --verbose
```

**Expected Output**:
- Migration file: `Migrations/[timestamp]_MirrorEquifax398ColumnSchema.cs`
- Model snapshot update: `Migrations/EnrichmentDbContextModelSnapshot.cs`

**Migration Actions**:
- Drop old columns: Phone1-10, PersonalInfoJson, AddressesJson, PhonesJson, FinancialJson
- Add 398 new columns from Equifax schema
- Create indexes on phone, email, consumer_key columns
- Add computed column for NormalizedPhone

**Estimated Time**: 30 minutes

---

### PHASE 3: REPOSITORY UPDATES

**File**: `src/EquifaxEnrichmentAPI.Infrastructure/Repositories/EnrichmentRepository.cs`

**Changes**:
```csharp
public async Task<PhoneSearchResult> FindByPhoneAsync(
    string normalizedPhone,
    CancellationToken cancellationToken = default)
{
    // Search across MobilePhone1, MobilePhone2, Phone1-5 (7 phones from CSV)
    var entity = await _context.ConsumerEnrichments
        .AsNoTracking()
        .Where(e =>
            e.MobilePhone1 == normalizedPhone ||
            e.MobilePhone2 == normalizedPhone ||
            e.Phone1 == normalizedPhone ||
            e.Phone2 == normalizedPhone ||
            e.Phone3 == normalizedPhone ||
            e.Phone4 == normalizedPhone ||
            e.Phone5 == normalizedPhone)
        .FirstOrDefaultAsync(cancellationToken);

    if (entity != null)
    {
        // Determine which column matched for confidence scoring
        int columnIndex = DetermineMatchedColumn(entity, normalizedPhone);
        return PhoneSearchResult.CreateMatch(entity, columnIndex);
    }

    return PhoneSearchResult.CreateNoMatch();
}

// NEW: Email search capability
public async Task<ConsumerEnrichment?> FindByEmailAsync(
    string email,
    CancellationToken cancellationToken = default)
{
    return await _context.ConsumerEnrichments
        .AsNoTracking()
        .Where(e =>
            e.Email1 == email ||
            e.Email2 == email ||
            e.Email3 == email ||
            // ... through Email15
        )
        .FirstOrDefaultAsync(cancellationToken);
}
```

**Estimated Time**: 2 hours

---

### PHASE 4: DTO UPDATES

**New DTOs Required**:

1. **AlternateNameDto.cs** (5 name variations)
```csharp
public class AlternateNameDto
{
    public string? Prefix { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Suffix { get; set; }
}
```

2. **EmailDto.cs** (15 emails with dates)
```csharp
public class EmailDto
{
    public string? Address { get; set; }
    public string? LastSeenDate { get; set; }
}
```

3. **FinancialAttributesDto.cs** (14 fields)
```csharp
public class FinancialAttributesDto
{
    public decimal? Income360Complete { get; set; }
    public decimal? Income360Salary { get; set; }
    public decimal? Income360NonSalary { get; set; }
    public decimal? SpendingPower { get; set; }
    public decimal? AffluenceIndex { get; set; }
    // ... all 14 fields
}
```

4. **CreditScoresDto.cs** (key Vantage scores)
```csharp
public class CreditScoresDto
{
    public decimal? VantageDataScore { get; set; }
    public decimal? NeighborhoodRiskScore { get; set; }
    public decimal? AutoInMarketPropensityScore { get; set; }
    // Expose 20-30 most important scores, not all 123
}
```

5. **LookupResponseDto.cs** (updated)
```csharp
public class LookupResponseDto
{
    public string ConsumerKey { get; set; }

    // Personal Info (native fields, not JSON)
    public string? Prefix { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Gender { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Age { get; set; }
    public string? Deceased { get; set; }

    // NEW: Alternate names
    public List<AlternateNameDto>? AlternateNames { get; set; }

    // Addresses (structured from native columns)
    public List<AddressDto>? Addresses { get; set; }

    // Phones (from native columns, not JSON)
    public List<PhoneDto>? Phones { get; set; }

    // NEW: Emails
    public List<EmailDto>? Emails { get; set; }

    // NEW: Financial attributes
    public FinancialAttributesDto? FinancialAttributes { get; set; }

    // NEW: Credit scores
    public CreditScoresDto? CreditScores { get; set; }

    // Metadata
    public double MatchConfidence { get; set; }
    public string MatchType { get; set; }
}
```

**Estimated Time**: 3 hours

---

### PHASE 5: CSV IMPORT PIPELINE

**File Structure**:
```
tools/csv-importer/
├── Program.cs                      # Main entry point
├── Services/
│   ├── AesGcmDecryptor.cs         # AES-GCM decryption
│   ├── CsvParser.cs               # Parse pipe-delimited CSV
│   ├── EquifaxImporter.cs         # Main import logic
│   └── S3CsvLoader.cs             # Load CSV files from S3
├── Models/
│   └── EquifaxCsvRow.cs           # Represents CSV row
└── appsettings.json               # Configuration
```

**AesGcmDecryptor.cs**:
```csharp
public class AesGcmDecryptor
{
    private readonly byte[] _key;

    public AesGcmDecryptor(byte[] key)
    {
        _key = key ?? throw new ArgumentNullException(nameof(key));
    }

    public string? Decrypt(string encryptedJson)
    {
        if (string.IsNullOrWhiteSpace(encryptedJson))
            return null;

        // Parse {"ciphertext":"...","iv":"...","tag":"..."}
        var encrypted = JsonSerializer.Deserialize<EncryptedValue>(encryptedJson);
        if (encrypted == null) return null;

        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);

        var ciphertext = Convert.FromHexString(encrypted.Ciphertext);
        var iv = Convert.FromHexString(encrypted.Iv);
        var tag = Convert.FromHexString(encrypted.Tag);
        var plaintext = new byte[ciphertext.Length];

        aes.Decrypt(iv, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }

    private class EncryptedValue
    {
        [JsonPropertyName("ciphertext")]
        public string Ciphertext { get; set; } = string.Empty;

        [JsonPropertyName("iv")]
        public string Iv { get; set; } = string.Empty;

        [JsonPropertyName("tag")]
        public string Tag { get; set; } = string.Empty;
    }
}
```

**EquifaxImporter.cs** (Main Logic):
```csharp
public class EquifaxImporter
{
    private readonly EnrichmentDbContext _context;
    private readonly AesGcmDecryptor _decryptor;

    public async Task ImportCsvFilesAsync(
        string[] csvFilePaths,
        CancellationToken cancellationToken)
    {
        int totalImported = 0;

        foreach (var csvPath in csvFilePaths)
        {
            Console.WriteLine($"Processing: {csvPath}");

            using var reader = new StreamReader(csvPath);
            var batch = new List<ConsumerEnrichment>();

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(line)) continue;

                var fields = line.Split('|');
                if (fields.Length < 398)
                {
                    Console.WriteLine($"⚠️ Skipping row with {fields.Length} fields (expected 398)");
                    continue;
                }

                var consumer = MapCsvRowToEntity(fields);
                batch.Add(consumer);

                // Batch insert every 10,000 records
                if (batch.Count >= 10000)
                {
                    await _context.ConsumerEnrichments.AddRangeAsync(batch, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    totalImported += batch.Count;
                    Console.WriteLine($"✅ Imported {totalImported:N0} records...");
                    batch.Clear();
                }
            }

            // Insert remaining records
            if (batch.Any())
            {
                await _context.ConsumerEnrichments.AddRangeAsync(batch, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                totalImported += batch.Count;
            }
        }

        Console.WriteLine($"🎉 Import complete: {totalImported:N0} total records");
    }

    private ConsumerEnrichment MapCsvRowToEntity(string[] fields)
    {
        // Field mapping based on schema_comparison.md
        return ConsumerEnrichment.CreateFromEquifaxCsv(
            consumerKey: _decryptor.Decrypt(fields[0]) ?? throw new Exception("consumer_key required"),
            prefix: _decryptor.Decrypt(fields[1]),
            firstName: _decryptor.Decrypt(fields[2]),
            middleName: _decryptor.Decrypt(fields[3]),
            lastName: _decryptor.Decrypt(fields[4]),
            suffix: _decryptor.Decrypt(fields[5]),
            gender: fields[6],
            dateOfBirth: _decryptor.Decrypt(fields[7]),
            age: fields[8],
            deceased: fields[9]
            // ... ALL 398 fields mapped
        );
    }
}
```

**Estimated Time**: 5 hours

---

### PHASE 6: DECRYPTION KEY CONFIGURATION

**Steps**:
1. Locate encryption keys in AWS environment
2. Potential locations:
   - AWS Secrets Manager: `equifax/encryption-keys`
   - S3: `s3://sb-marketing-migration/keys/`
   - Snowflake: Contact data provider
3. Test decryption on sample encrypted field
4. Store keys securely in configuration

**Command to Check AWS Secrets**:
```bash
aws secretsmanager list-secrets --region us-east-1 | grep -i equifax
aws secretsmanager get-secret-value --secret-id <secret-name> --region us-east-1
```

**Estimated Time**: 2-4 hours (depending on key location)

---

### PHASE 7: DATABASE MIGRATION EXECUTION

**Pre-Migration Checklist**:
- [ ] Backup production database
- [ ] Test migration on dev environment
- [ ] Verify no breaking changes to existing APIs
- [ ] Confirm rollback plan ready

**Execute Migration**:
```bash
# Test on development first
dotnet ef database update --context EnrichmentDbContext --connection "Host=localhost;Database=equifax_dev;..."

# Production (after testing)
dotnet ef database update --context EnrichmentDbContext --connection "Host=equifax-enrichment-api-db...;Database=equifax_enrichment_api;..."
```

**Expected Duration**: 30-60 minutes (depending on database size)

---

### PHASE 8: CSV DATA IMPORT

**Import Process**:
```bash
cd tools/csv-importer

# Import from S3 bucket (10,814 files)
dotnet run -- \
    --s3-bucket sb-marketing-migration \
    --s3-prefix equifax-export/full-export/ \
    --batch-size 10000 \
    --connection-string "Host=equifax-enrichment-api-db..."
```

**Expected Import Speed**: ~100,000 rows/minute
**Total Records**: 327M rows
**Estimated Time**: 54-60 hours

**Monitoring**:
- Progress logging every 10,000 records
- Error tracking for malformed rows
- Performance metrics (rows/sec, MB/sec)

---

## ROLLBACK PLAN

If migration fails:

1. **Database Rollback**:
   ```bash
   # Revert to previous migration
   dotnet ef database update <PreviousMigrationName> --context EnrichmentDbContext

   # OR restore from RDS snapshot
   aws rds restore-db-instance-from-db-snapshot \
       --db-instance-identifier equifax-enrichment-api-db-restored \
       --db-snapshot-identifier <snapshot-id>
   ```

2. **Code Rollback**:
   ```bash
   # Restore old entity
   git checkout HEAD~1 -- src/EquifaxEnrichmentAPI.Domain/Entities/ConsumerEnrichment.cs

   # Redeploy API service
   dotnet publish -c Release
   # Deploy to ECS
   ```

3. **Data Preserved**:
   - S3 CSV files untouched (source of truth)
   - Can re-import anytime
   - Backup database has full 398-column schema already

---

## SUCCESS METRICS

| Metric | Current | Target | Status |
|--------|---------|--------|--------|
| **Columns in API DB** | 22 | 398 | 🔄 In Progress |
| **Data Coverage** | 5% | 100% | 🔄 In Progress |
| **Phone Lookup Success** | 0% (empty) | 95%+ | ⏳ Pending Import |
| **Email Lookup** | N/A | 80%+ | ⏳ Pending |
| **Query Performance** | 6207ms | <100ms | ⏳ Pending |
| **Records Imported** | 0 | 327M | ⏳ Pending |

---

## NEXT IMMEDIATE STEPS

1. ✅ **Entity Model Created** (1359 lines, all 398 columns)
2. 🔄 **Update Entity Configuration** (map all columns in Fluent API)
3. ⏳ **Generate EF Migration** (create database schema change)
4. ⏳ **Build CSV Importer** (with AES-GCM decryption)
5. ⏳ **Locate Decryption Keys** (AWS Secrets Manager / Snowflake)
6. ⏳ **Test on Dev Environment** (validate schema and import)
7. ⏳ **Execute Production Migration** (apply schema to production DB)
8. ⏳ **Import 10,814 CSV Files** (populate 327M records)
9. ⏳ **Verify API Functionality** (phone/email lookups work)
10. ⏳ **Performance Testing** (confirm <100ms queries)

---

## TIMELINE ESTIMATE

| Phase | Duration | Dependencies |
|-------|----------|--------------|
| Entity Configuration | 2-3 hours | Entity model complete ✅ |
| Migration Generation | 30 min | Configuration complete |
| Repository Updates | 2 hours | Migration generated |
| DTO Updates | 3 hours | Repository updated |
| CSV Importer | 5 hours | All code changes done |
| Decryption Keys | 2-4 hours | Access to AWS/Snowflake |
| Test Migration | 1 hour | Keys located |
| Production Migration | 1 hour | Testing successful |
| CSV Import | 54-60 hours | Migration applied |
| **TOTAL** | **70-80 hours** | **~2 weeks** |

**Actual calendar time**: 10-14 days (with testing and validation)

---

## BLOCKERS & RISKS

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| **Decryption keys unavailable** | Medium | Blocker | Escalate to Snowflake/AWS immediately |
| **Migration breaks API** | Low | High | Computed columns for backward compat |
| **Import performance** | Medium | Medium | Batch processing, parallel workers |
| **Data type mismatches** | Low | Medium | Validate with sample data first |

---

## NOTES

- Entity model is GENERATED, not hand-written (prevents errors)
- Backward compatibility maintained with computed columns
- All 398 columns preserved - zero data loss
- CSV source remains in S3 (can re-import if needed)
- Data is an asset - cannot recreate missing fields later
