# EQUIFAX API DATABASE SCHEMA MIGRATION PLAN
## From 22-Column Simplified to 398-Column Mirrored Design

**Date**: 2025-11-03
**Purpose**: Restructure API database to mirror Equifax source data
**Impact**: Major architectural change - preserves 95% more data

---

## PHASE 1: SCHEMA DESIGN

### Current Schema (22 columns - 95% data loss)
```sql
CREATE TABLE consumer_enrichments (
    id UUID PRIMARY KEY,
    consumer_key VARCHAR(100),           -- Just identifier
    normalized_phone VARCHAR(10),        -- Search optimization

    -- Collapsed data (loses structure)
    Phone1 TEXT,
    Phone2 TEXT,
    Phone3 TEXT,
    Phone4 TEXT,
    Phone5 TEXT,
    Phone6 TEXT,
    Phone7 TEXT,
    Phone8 TEXT,
    Phone9 TEXT,
    Phone10 TEXT,
    phones_json TEXT,                    -- Redundant with Phone1-10
    personal_info_json TEXT,             -- 11 fields collapsed
    addresses_json TEXT,                 -- 162 fields collapsed
    financial_json TEXT,                 -- 14 fields collapsed

    -- Metadata
    match_confidence DOUBLE PRECISION,
    match_type VARCHAR(50),
    data_freshness_date TIMESTAMPTZ,
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ
);
```

**Problems**:
- NO email addresses (15 available in source)
- NO alternate names (30 fields lost)
- NO credit scores (123 Vantage scores lost)
- NO device identifiers (7 fields lost)
- Personal/address data hidden in JSON (complex queries)
- Financial data expected but source has 14 real fields

---

### Proposed Mirrored Schema (398 columns - 0% data loss)
```sql
CREATE TABLE consumer_enrichments (
    -- PostgreSQL Primary Key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),

    -- IDENTITY (1 column) - DECRYPTED
    consumer_key TEXT NOT NULL,  -- Decrypted from JSONB

    -- PRIMARY PERSONAL INFO (11 columns) - MIXED ENCRYPTION
    prefix TEXT,                 -- Decrypted
    first_name TEXT,             -- Decrypted
    middle_name TEXT,            -- Decrypted
    last_name TEXT,              -- Decrypted
    suffix TEXT,                 -- Decrypted
    gender TEXT,                 -- Plain text
    date_of_birth TEXT,          -- Decrypted
    age TEXT,                    -- Plain text
    deceased TEXT,               -- Plain text
    first_seen_date_primary_name TEXT,
    last_seen_date_primary_name TEXT,

    -- ALTERNATE NAMES (30 columns) - ALL ENCRYPTED → DECRYPTED
    alternate_name_1 TEXT,
    alternate_prefix_1 TEXT,
    alternate_first_name_1 TEXT,
    alternate_middle_name_1 TEXT,
    alternate_last_name_1 TEXT,
    alternate_suffix_1 TEXT,
    -- ... repeat for alternate_name_2 through alternate_name_5

    -- ADDRESSES (162 columns) - MIXED ENCRYPTION
    -- Address 1
    address_1 TEXT,              -- Decrypted full address
    house_number_1 TEXT,         -- Decrypted
    predirectional_1 TEXT,       -- Decrypted
    street_name_1 TEXT,          -- Decrypted
    street_suffix_1 TEXT,        -- Decrypted
    post_direction_1 TEXT,       -- Decrypted
    unit_type_1 TEXT,            -- Plain text
    unit_number_1 TEXT,          -- Plain text
    city_name_1 TEXT,            -- Plain text
    state_abbreviation_1 TEXT,   -- Plain text
    zip_1 TEXT,                  -- Plain text
    z4_1 TEXT,                   -- Plain text
    delivery_point_code_1 TEXT,
    delivery_point_validation_1 TEXT,
    carrier_route_1 TEXT,
    fips_code_1 TEXT,
    z4_type_1 TEXT,
    transaction_date_1 TEXT,
    -- ... repeat for address_2 through address_10 (total 162 cols)

    -- PHONES (12 columns) - ALL PLAIN TEXT
    mobile_phone_1 TEXT,
    mobile_phone_2 TEXT,
    phone_1 TEXT,
    last_seen_date_phone_1 TEXT,
    phone_2 TEXT,
    last_seen_date_phone_2 TEXT,
    phone_3 TEXT,
    last_seen_date_phone_3 TEXT,
    phone_4 TEXT,
    last_seen_date_phone_4 TEXT,
    phone_5 TEXT,
    last_seen_date_phone_5 TEXT,

    -- EMAILS (30 columns) - ALL PLAIN TEXT - **NEW**
    email_1 TEXT,
    last_seen_date_email_1 TEXT,
    email_2 TEXT,
    last_seen_date_email_2 TEXT,
    email_3 TEXT,
    last_seen_date_email_3 TEXT,
    -- ... repeat through email_15

    -- FINANCIAL ATTRIBUTES (14 columns) - ALL NUMERIC - **NEW**
    income360_complete NUMERIC,
    income360_salary NUMERIC,
    income360_non_salary NUMERIC,
    economiccohortscode TEXT,
    financialdurabilityindex NUMERIC,
    financialdurabilityscore NUMERIC,
    spending_power NUMERIC,
    affluence_index NUMERIC,
    balance_auto_finance_loan_accounts NUMERIC,
    percent_balance_to_high_auto_finance_credit NUMERIC,
    vantage_score_neighborhood_risk_score NUMERIC,
    automotive_response_intent_indicator NUMERIC,
    auto_in_market_propensity_score NUMERIC,
    vds NUMERIC,

    -- CREDIT ATTRIBUTES (123 columns) - ALL NUMERIC - **NEW**
    vf NUMERIC, vr NUMERIC, vg NUMERIC, vh NUMERIC, vj NUMERIC,
    vac NUMERIC, vy NUMERIC, vs NUMERIC, vt NUMERIC, vc NUMERIC,
    -- ... all 123 Vantage score fields

    -- DEVICE IDENTIFIERS (7 columns) - ENCRYPTED → DECRYPTED - **NEW**
    ipaddress1 TEXT,
    ipaddress2 TEXT,
    idfa1 TEXT,
    idfa2 TEXT,
    idfa3 TEXT,
    idfa4 TEXT,
    idfa5 TEXT,

    -- METADATA (8 columns) - PLAIN TEXT
    marketing_email_flag TEXT,
    uuid TEXT,
    meta_revision TEXT,
    source_system VARCHAR(50) DEFAULT 'EQUIFAX_SNOWFLAKE',
    migration_batch_id UUID,
    migrated_at TIMESTAMPTZ,

    -- COMPUTED COLUMNS (backward compatibility)
    normalized_phone VARCHAR(10) GENERATED ALWAYS AS (
        CASE
            WHEN mobile_phone_1 IS NOT NULL THEN mobile_phone_1
            WHEN phone_1 IS NOT NULL THEN phone_1
            ELSE NULL
        END
    ) STORED,

    -- SEARCH OPTIMIZATION
    phone_search_vector TSVECTOR GENERATED ALWAYS AS (
        to_tsvector('simple',
            COALESCE(mobile_phone_1, '') || ' ' ||
            COALESCE(mobile_phone_2, '') || ' ' ||
            COALESCE(phone_1, '') || ' ' ||
            COALESCE(phone_2, '') || ' ' ||
            COALESCE(phone_3, '') || ' ' ||
            COALESCE(phone_4, '') || ' ' ||
            COALESCE(phone_5, '')
        )
    ) STORED,

    email_search_vector TSVECTOR GENERATED ALWAYS AS (
        to_tsvector('simple',
            COALESCE(email_1, '') || ' ' ||
            COALESCE(email_2, '') || ' ' ||
            COALESCE(email_3, '') || ' ' ||
            -- ... all 15 emails
        )
    ) STORED
);

-- INDEXES
CREATE INDEX idx_consumer_key ON consumer_enrichments(consumer_key);
CREATE INDEX idx_normalized_phone ON consumer_enrichments(normalized_phone);
CREATE INDEX idx_phone_search ON consumer_enrichments USING gin(phone_search_vector);
CREATE INDEX idx_email_search ON consumer_enrichments USING gin(email_search_vector);
CREATE INDEX idx_uuid ON consumer_enrichments(uuid);

-- Multi-column phone search for any phone match
CREATE INDEX idx_all_phones ON consumer_enrichments (
    mobile_phone_1, mobile_phone_2,
    phone_1, phone_2, phone_3, phone_4, phone_5
);

-- Email lookup index
CREATE INDEX idx_primary_email ON consumer_enrichments(email_1);

-- Financial/credit score indexes for filtering
CREATE INDEX idx_income_complete ON consumer_enrichments(income360_complete);
CREATE INDEX idx_spending_power ON consumer_enrichments(spending_power);
CREATE INDEX idx_auto_propensity ON consumer_enrichments(auto_in_market_propensity_score);
```

---

## PHASE 2: ENTITY FRAMEWORK MIGRATION

### Generate Migration
```bash
cd src/EquifaxEnrichmentAPI.Infrastructure
dotnet ef migrations add MirrorEquifaxFullSchema \
    --context EnrichmentDbContext \
    --output-dir Migrations

dotnet ef database update --context EnrichmentDbContext
```

### Migration File Structure
```csharp
public partial class MirrorEquifaxFullSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Drop old JSON columns
        migrationBuilder.DropColumn(name: "phones_json", table: "consumer_enrichments");
        migrationBuilder.DropColumn(name: "personal_info_json", table: "consumer_enrichments");
        migrationBuilder.DropColumn(name: "addresses_json", table: "consumer_enrichments");
        migrationBuilder.DropColumn(name: "financial_json", table: "consumer_enrichments");

        // Drop old individual phone columns (Phone1-10)
        for (int i = 1; i <= 10; i++)
        {
            migrationBuilder.DropColumn(name: $"Phone{i}", table: "consumer_enrichments");
        }

        // Add all 398 new columns

        // IDENTITY
        migrationBuilder.AlterColumn<string>(
            name: "consumer_key",
            table: "consumer_enrichments",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(100)");

        // PRIMARY PERSONAL INFO (11 columns)
        migrationBuilder.AddColumn<string>(
            name: "prefix", table: "consumer_enrichments", type: "text", nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "first_name", table: "consumer_enrichments", type: "text", nullable: true);
        // ... all 11 fields

        // ALTERNATE NAMES (30 columns)
        for (int i = 1; i <= 5; i++)
        {
            migrationBuilder.AddColumn<string>(
                name: $"alternate_name_{i}", table: "consumer_enrichments", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: $"alternate_prefix_{i}", table: "consumer_enrichments", type: "text", nullable: true);
            // ... 6 fields per alternate name
        }

        // ADDRESSES (162 columns)
        for (int i = 1; i <= 10; i++)
        {
            migrationBuilder.AddColumn<string>(
                name: $"address_{i}", table: "consumer_enrichments", type: "text", nullable: true);
            // ... 16 fields per address
        }

        // PHONES (12 columns)
        migrationBuilder.AddColumn<string>(
            name: "mobile_phone_1", table: "consumer_enrichments", type: "text", nullable: true);
        // ... all phone fields

        // EMAILS (30 columns) - NEW
        for (int i = 1; i <= 15; i++)
        {
            migrationBuilder.AddColumn<string>(
                name: $"email_{i}", table: "consumer_enrichments", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: $"last_seen_date_email_{i}", table: "consumer_enrichments", type: "text", nullable: true);
        }

        // FINANCIAL (14 columns) - NEW
        migrationBuilder.AddColumn<decimal>(
            name: "income360_complete", table: "consumer_enrichments", type: "numeric", nullable: true);
        // ... all 14 financial fields

        // CREDIT ATTRIBUTES (123 columns) - NEW
        migrationBuilder.AddColumn<decimal>(
            name: "vf", table: "consumer_enrichments", type: "numeric", nullable: true);
        // ... all 123 Vantage scores

        // DEVICE IDS (7 columns) - NEW
        migrationBuilder.AddColumn<string>(
            name: "ipaddress1", table: "consumer_enrichments", type: "text", nullable: true);
        // ... all 7 device fields

        // METADATA (8 columns)
        migrationBuilder.AddColumn<string>(
            name: "marketing_email_flag", table: "consumer_enrichments", type: "text", nullable: true);
        // ... all metadata fields

        // COMPUTED COLUMNS
        migrationBuilder.Sql(@"
            ALTER TABLE consumer_enrichments
            ADD COLUMN normalized_phone VARCHAR(10)
            GENERATED ALWAYS AS (
                CASE
                    WHEN mobile_phone_1 IS NOT NULL THEN mobile_phone_1
                    WHEN phone_1 IS NOT NULL THEN phone_1
                    ELSE NULL
                END
            ) STORED;
        ");

        migrationBuilder.Sql(@"
            ALTER TABLE consumer_enrichments
            ADD COLUMN phone_search_vector TSVECTOR
            GENERATED ALWAYS AS (
                to_tsvector('simple',
                    COALESCE(mobile_phone_1, '') || ' ' ||
                    COALESCE(mobile_phone_2, '') || ' ' ||
                    COALESCE(phone_1, '') || ' ' ||
                    COALESCE(phone_2, '') || ' ' ||
                    COALESCE(phone_3, '') || ' ' ||
                    COALESCE(phone_4, '') || ' ' ||
                    COALESCE(phone_5, '')
                )
            ) STORED;
        ");

        // INDEXES
        migrationBuilder.CreateIndex(
            name: "idx_normalized_phone",
            table: "consumer_enrichments",
            column: "normalized_phone");

        migrationBuilder.CreateIndex(
            name: "idx_phone_search",
            table: "consumer_enrichments",
            column: "phone_search_vector")
            .Annotation("Npgsql:IndexMethod", "gin");

        migrationBuilder.CreateIndex(
            name: "idx_primary_email",
            table: "consumer_enrichments",
            column: "email_1");

        migrationBuilder.CreateIndex(
            name: "idx_income_complete",
            table: "consumer_enrichments",
            column: "income360_complete");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Reverse all changes
        // ... drop all new columns, restore old schema
    }
}
```

---

## PHASE 3: DOMAIN MODEL UPDATE

### Current ConsumerEnrichment Entity
```csharp
// src/EquifaxEnrichmentAPI.Domain/Entities/ConsumerEnrichment.cs
public class ConsumerEnrichment
{
    public Guid Id { get; set; }
    public string ConsumerKey { get; set; } = string.Empty;
    public string? NormalizedPhone { get; set; }

    // Individual phone columns
    public string? Phone1 { get; set; }
    // ... Phone2-10

    // JSON fields
    public string? PhonesJson { get; set; }
    public string? PersonalInfoJson { get; set; }
    public string? AddressesJson { get; set; }
    public string? FinancialJson { get; set; }

    // Metadata
    public double MatchConfidence { get; set; }
    public string MatchType { get; set; } = string.Empty;
    public DateTimeOffset? DataFreshnessDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
```

### Proposed Mirrored Entity
```csharp
// src/EquifaxEnrichmentAPI.Domain/Entities/ConsumerEnrichment.cs
public class ConsumerEnrichment
{
    // PostgreSQL Primary Key
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // IDENTITY
    public string ConsumerKey { get; set; } = string.Empty;

    // PRIMARY PERSONAL INFO (11 fields)
    public string? Prefix { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Suffix { get; set; }
    public string? Gender { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Age { get; set; }
    public string? Deceased { get; set; }
    public string? FirstSeenDatePrimaryName { get; set; }
    public string? LastSeenDatePrimaryName { get; set; }

    // ALTERNATE NAMES (30 fields)
    public string? AlternateName1 { get; set; }
    public string? AlternatePrefix1 { get; set; }
    public string? AlternateFirstName1 { get; set; }
    public string? AlternateMiddleName1 { get; set; }
    public string? AlternateLastName1 { get; set; }
    public string? AlternateSuffix1 { get; set; }
    // ... repeat for AlternateName2-5

    // ADDRESSES (162 fields) - Show pattern for Address 1
    public string? Address1 { get; set; }
    public string? HouseNumber1 { get; set; }
    public string? Predirectional1 { get; set; }
    public string? StreetName1 { get; set; }
    public string? StreetSuffix1 { get; set; }
    public string? PostDirection1 { get; set; }
    public string? UnitType1 { get; set; }
    public string? UnitNumber1 { get; set; }
    public string? CityName1 { get; set; }
    public string? StateAbbreviation1 { get; set; }
    public string? Zip1 { get; set; }
    public string? Z41 { get; set; }
    public string? DeliveryPointCode1 { get; set; }
    public string? DeliveryPointValidation1 { get; set; }
    public string? CarrierRoute1 { get; set; }
    public string? FipsCode1 { get; set; }
    public string? Z4Type1 { get; set; }
    public string? TransactionDate1 { get; set; }
    // ... repeat for Address2-10

    // PHONES (12 fields)
    public string? MobilePhone1 { get; set; }
    public string? MobilePhone2 { get; set; }
    public string? Phone1 { get; set; }
    public string? LastSeenDatePhone1 { get; set; }
    public string? Phone2 { get; set; }
    public string? LastSeenDatePhone2 { get; set; }
    public string? Phone3 { get; set; }
    public string? LastSeenDatePhone3 { get; set; }
    public string? Phone4 { get; set; }
    public string? LastSeenDatePhone4 { get; set; }
    public string? Phone5 { get; set; }
    public string? LastSeenDatePhone5 { get; set; }

    // EMAILS (30 fields) - NEW
    public string? Email1 { get; set; }
    public string? LastSeenDateEmail1 { get; set; }
    public string? Email2 { get; set; }
    public string? LastSeenDateEmail2 { get; set; }
    // ... repeat through Email15

    // FINANCIAL ATTRIBUTES (14 fields) - NEW
    public decimal? Income360Complete { get; set; }
    public decimal? Income360Salary { get; set; }
    public decimal? Income360NonSalary { get; set; }
    public string? EconomicCohortsCode { get; set; }
    public decimal? FinancialDurabilityIndex { get; set; }
    public decimal? FinancialDurabilityScore { get; set; }
    public decimal? SpendingPower { get; set; }
    public decimal? AffluenceIndex { get; set; }
    public decimal? BalanceAutoFinanceLoanAccounts { get; set; }
    public decimal? PercentBalanceToHighAutoFinanceCredit { get; set; }
    public decimal? VantageScoreNeighborhoodRiskScore { get; set; }
    public decimal? AutomotiveResponseIntentIndicator { get; set; }
    public decimal? AutoInMarketPropensityScore { get; set; }
    public decimal? Vds { get; set; }

    // CREDIT ATTRIBUTES (123 fields) - NEW
    public decimal? Vf { get; set; }
    public decimal? Vr { get; set; }
    public decimal? Vg { get; set; }
    // ... all 123 Vantage scores

    // DEVICE IDENTIFIERS (7 fields) - NEW
    public string? IpAddress1 { get; set; }
    public string? IpAddress2 { get; set; }
    public string? Idfa1 { get; set; }
    public string? Idfa2 { get; set; }
    public string? Idfa3 { get; set; }
    public string? Idfa4 { get; set; }
    public string? Idfa5 { get; set; }

    // METADATA (8 fields)
    public string? MarketingEmailFlag { get; set; }
    public string? Uuid { get; set; }
    public string? MetaRevision { get; set; }
    public string SourceSystem { get; set; } = "EQUIFAX_SNOWFLAKE";
    public Guid? MigrationBatchId { get; set; }
    public DateTimeOffset? MigratedAt { get; set; }

    // COMPUTED COLUMN (read-only)
    public string? NormalizedPhone { get; set; }

    // NAVIGATION PROPERTIES (optional - for related entities)
    // public virtual ICollection<LookupHistory> LookupHistory { get; set; }
}
```

---

## PHASE 4: REPOSITORY UPDATE

### Current Repository Query (JSON Parsing)
```csharp
// Searches Phone1-10 individual columns
var consumer = await _context.ConsumerEnrichments
    .Where(c =>
        c.Phone1 == normalizedPhone ||
        c.Phone2 == normalizedPhone ||
        // ... Phone3-10
    )
    .FirstOrDefaultAsync(cancellationToken);
```

### Proposed Repository Query (Native Columns)
```csharp
// Search all phone fields natively
var consumer = await _context.ConsumerEnrichments
    .Where(c =>
        c.MobilePhone1 == normalizedPhone ||
        c.MobilePhone2 == normalizedPhone ||
        c.Phone1 == normalizedPhone ||
        c.Phone2 == normalizedPhone ||
        c.Phone3 == normalizedPhone ||
        c.Phone4 == normalizedPhone ||
        c.Phone5 == normalizedPhone
    )
    .FirstOrDefaultAsync(cancellationToken);

// OR use full-text search on computed vector column
var consumer = await _context.ConsumerEnrichments
    .FromSqlRaw(@"
        SELECT * FROM consumer_enrichments
        WHERE phone_search_vector @@ to_tsquery('simple', {0})
        LIMIT 1
    ", normalizedPhone)
    .FirstOrDefaultAsync(cancellationToken);

// Email search (new capability)
var consumerByEmail = await _context.ConsumerEnrichments
    .Where(c =>
        c.Email1 == searchEmail ||
        c.Email2 == searchEmail ||
        c.Email3 == searchEmail
        // ... through Email15
    )
    .FirstOrDefaultAsync(cancellationToken);
```

---

## PHASE 5: API DTO UPDATE

### Current Response DTO (22 fields)
```csharp
public class LookupResponseDto
{
    public string ConsumerKey { get; set; }
    public PersonalInfoDto? PersonalInfo { get; set; }  // Parsed from JSON
    public List<AddressDto>? Addresses { get; set; }    // Parsed from JSON
    public List<string>? Phones { get; set; }           // From Phone1-10
    public FinancialInfoDto? FinancialInfo { get; set; } // Parsed from JSON
    public double MatchConfidence { get; set; }
    public string MatchType { get; set; }
}
```

### Proposed Response DTO (398 fields available)
```csharp
public class LookupResponseDto
{
    // IDENTITY
    public string ConsumerKey { get; set; }

    // PERSONAL INFO (native fields, not JSON)
    public string? Prefix { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Suffix { get; set; }
    public string? Gender { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Age { get; set; }
    public string? Deceased { get; set; }

    // ALTERNATE NAMES (new)
    public List<AlternateNameDto>? AlternateNames { get; set; }

    // ADDRESSES (structured, not JSON)
    public List<AddressDto>? Addresses { get; set; }

    // PHONES (7 phones + dates, not 10)
    public List<PhoneDto>? Phones { get; set; }

    // EMAILS (new - 15 emails)
    public List<EmailDto>? Emails { get; set; }

    // FINANCIAL (native fields, not JSON)
    public FinancialAttributesDto? FinancialAttributes { get; set; }

    // CREDIT SCORES (new - 123 scores)
    public CreditScoresDto? CreditScores { get; set; }

    // DEVICE IDS (new)
    public List<string>? DeviceIdentifiers { get; set; }

    // METADATA
    public string? MarketingEmailFlag { get; set; }
    public string? Uuid { get; set; }
    public string? MetaRevision { get; set; }
}

// Supporting DTOs
public class AlternateNameDto
{
    public string? FullName { get; set; }
    public string? Prefix { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Suffix { get; set; }
}

public class PhoneDto
{
    public string? Number { get; set; }
    public string? Type { get; set; }  // "mobile" or "landline"
    public string? LastSeenDate { get; set; }
}

public class EmailDto
{
    public string? Address { get; set; }
    public string? LastSeenDate { get; set; }
}

public class FinancialAttributesDto
{
    public decimal? IncomeComplete { get; set; }
    public decimal? IncomeSalary { get; set; }
    public decimal? IncomeNonSalary { get; set; }
    public decimal? SpendingPower { get; set; }
    public decimal? AffluenceIndex { get; set; }
    public decimal? AutoLoanBalance { get; set; }
    public decimal? AutoInMarketScore { get; set; }
}

public class CreditScoresDto
{
    public decimal? VantageDataScore { get; set; }  // vds
    public decimal? NeighborhoodRiskScore { get; set; }
    public decimal? AutoPropensityScore { get; set; }
    // ... expose key scores (not all 123)
}
```

---

## PHASE 6: DATA MIGRATION / ETL

### Decryption Function (AES-GCM)
```csharp
public class AesGcmDecryptor
{
    private readonly byte[] _key;

    public string? Decrypt(string encryptedJson)
    {
        if (string.IsNullOrWhiteSpace(encryptedJson))
            return null;

        var encrypted = JsonSerializer.Deserialize<EncryptedValue>(encryptedJson);
        if (encrypted == null) return null;

        using var aes = new AesGcm(_key);

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

### CSV Import Script
```csharp
public class EquifaxCsvImporter
{
    private readonly EnrichmentDbContext _context;
    private readonly AesGcmDecryptor _decryptor;

    public async Task ImportCsvAsync(string csvPath, CancellationToken ct)
    {
        using var reader = new StreamReader(csvPath);
        var batch = new List<ConsumerEnrichment>();

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line)) continue;

            var fields = line.Split('|');
            if (fields.Length < 398) continue;

            var consumer = new ConsumerEnrichment
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,

                // IDENTITY - DECRYPT
                ConsumerKey = _decryptor.Decrypt(fields[0]) ?? string.Empty,

                // PERSONAL INFO - MIXED
                Prefix = _decryptor.Decrypt(fields[1]),
                FirstName = _decryptor.Decrypt(fields[2]),
                MiddleName = _decryptor.Decrypt(fields[3]),
                LastName = _decryptor.Decrypt(fields[4]),
                Suffix = _decryptor.Decrypt(fields[5]),
                Gender = fields[6],  // Plain text
                DateOfBirth = _decryptor.Decrypt(fields[7]),
                Age = fields[8],
                Deceased = fields[9],
                FirstSeenDatePrimaryName = fields[10],
                LastSeenDatePrimaryName = fields[11],

                // ALTERNATE NAMES - ALL ENCRYPTED
                AlternateName1 = _decryptor.Decrypt(fields[12]),
                AlternatePrefix1 = _decryptor.Decrypt(fields[13]),
                // ... all 30 alternate name fields

                // ADDRESS 1 - MIXED ENCRYPTION
                Address1 = _decryptor.Decrypt(fields[42]),
                HouseNumber1 = _decryptor.Decrypt(fields[43]),
                Predirectional1 = _decryptor.Decrypt(fields[44]),
                StreetName1 = _decryptor.Decrypt(fields[45]),
                StreetSuffix1 = _decryptor.Decrypt(fields[46]),
                PostDirection1 = _decryptor.Decrypt(fields[47]),
                UnitType1 = fields[48],  // Plain
                UnitNumber1 = fields[49],
                CityName1 = fields[50],
                StateAbbreviation1 = fields[51],
                Zip1 = fields[52],
                // ... all 10 addresses

                // PHONES - ALL PLAIN TEXT
                MobilePhone1 = fields[232],
                MobilePhone2 = fields[233],
                Phone1 = fields[234],
                LastSeenDatePhone1 = fields[235],
                Phone2 = fields[236],
                // ... all phone fields

                // EMAILS - ALL PLAIN TEXT
                Email1 = fields[244],
                LastSeenDateEmail1 = fields[245],
                // ... all 15 emails

                // FINANCIAL - ALL NUMERIC
                Income360Complete = ParseDecimal(fields[274]),
                Income360Salary = ParseDecimal(fields[275]),
                // ... all 14 financial fields

                // CREDIT ATTRIBUTES - ALL NUMERIC
                Vf = ParseDecimal(fields[288]),
                Vr = ParseDecimal(fields[289]),
                // ... all 123 Vantage scores

                // DEVICE IDS - ENCRYPTED
                IpAddress1 = _decryptor.Decrypt(fields[399]),
                IpAddress2 = _decryptor.Decrypt(fields[400]),
                // ... all device fields

                // METADATA
                MarketingEmailFlag = fields[398],
                Uuid = fields[406],
                MetaRevision = fields[407],
                SourceSystem = "EQUIFAX_SNOWFLAKE",
                MigrationBatchId = Guid.NewGuid(),
                MigratedAt = DateTimeOffset.UtcNow
            };

            batch.Add(consumer);

            // Batch insert every 10,000 records
            if (batch.Count >= 10000)
            {
                await _context.ConsumerEnrichments.AddRangeAsync(batch, ct);
                await _context.SaveChangesAsync(ct);
                batch.Clear();
                Console.WriteLine($"Imported {_context.ConsumerEnrichments.Count()} records...");
            }
        }

        // Insert remaining records
        if (batch.Any())
        {
            await _context.ConsumerEnrichments.AddRangeAsync(batch, ct);
            await _context.SaveChangesAsync(ct);
        }
    }

    private decimal? ParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return decimal.TryParse(value, out var result) ? result : null;
    }
}
```

---

## PHASE 7: PERFORMANCE TESTING

### Expected Query Performance (vs Current 6207ms)
```sql
-- Phone lookup (indexed)
SELECT * FROM consumer_enrichments
WHERE normalized_phone = '9253245459';
-- Expected: <10ms

-- Email lookup (new capability)
SELECT * FROM consumer_enrichments
WHERE email_1 = 'test@example.com';
-- Expected: <10ms

-- Multi-phone search (full-text)
SELECT * FROM consumer_enrichments
WHERE phone_search_vector @@ to_tsquery('simple', '9253245459');
-- Expected: <50ms

-- Credit score filtering (new capability)
SELECT * FROM consumer_enrichments
WHERE income360_complete > 100000
AND auto_in_market_propensity_score > 50;
-- Expected: <100ms with partial index
```

---

## IMPLEMENTATION TIMELINE

| Phase | Task | Duration | Dependencies |
|-------|------|----------|--------------|
| **1** | Obtain decryption keys | 1-2 days | AWS Secrets Manager / Snowflake access |
| **2** | Generate EF migration | 1 day | Decryption keys located |
| **3** | Update domain models | 1 day | Migration generated |
| **4** | Update repository queries | 1 day | Domain models updated |
| **5** | Update API DTOs | 1 day | Repository updated |
| **6** | Build CSV import script | 2 days | All code changes complete |
| **7** | Test with sample data | 1 day | Import script ready |
| **8** | Import production CSV files | 1-2 days | Testing successful |
| **9** | Performance validation | 1 day | Data imported |
| **10** | Deploy to production | 1 day | Performance validated |

**Total Estimated Time**: 11-14 days

---

## RISKS & MITIGATION

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Decryption keys unavailable** | Blocker | Escalate to Snowflake/AWS support immediately |
| **Migration breaks existing API** | High | Add backward-compatible computed columns |
| **Performance degradation** | Medium | Extensive indexing, query optimization |
| **CSV import errors** | Medium | Validate sample files first, implement retries |
| **Data type mismatches** | Low | Test with actual CSV data before production |
| **API breaking changes** | High | Version API (v2 endpoint) with new schema |

---

## ROLLBACK PLAN

If migration fails:

1. **Revert Migration**:
   ```bash
   dotnet ef database update PreviousMigrationName
   ```

2. **Restore from Snapshot**:
   - RDS snapshot taken before migration
   - Restore takes ~30 minutes

3. **Redeploy Previous Code**:
   - Git revert to previous commit
   - Redeploy API service

4. **Data Preserved**:
   - Both old and new schema versions in git history
   - S3 CSV files untouched (source of truth)

---

## SUCCESS METRICS

| Metric | Current | Target | Measurement |
|--------|---------|--------|-------------|
| **Data Coverage** | 5% of Equifax data | 100% | All 398 columns present |
| **Query Performance** | 6207ms | <100ms | 99th percentile response time |
| **Phone Lookup Success** | 0% (empty DB) | 95%+ | Match rate on test dataset |
| **Email Lookup** | N/A (no emails) | 80%+ | Match rate on email queries |
| **API Uptime** | 99.9% | 99.9% | No degradation post-migration |
| **Import Speed** | N/A | 100K rows/min | CSV import throughput |

---

## NEXT STEPS

1. **Review & Approve** this migration plan
2. **Locate Decryption Keys** (blocking step)
3. **Create Migration Branch**: `feature/mirror-equifax-schema`
4. **Generate EF Migration** with all 398 columns
5. **Update Domain Models** and repository
6. **Build CSV Import Script** with decryption
7. **Test with Sample Data** (100 rows from CSV)
8. **Import Production Data** (10,814 files)
9. **Performance Validation** (query benchmarks)
10. **Deploy to Production** (with rollback plan ready)
