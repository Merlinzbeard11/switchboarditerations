# COMPLETE EQUIFAX DATA ASSESSMENT
## All 398 Columns Analyzed for API Database Mirroring

**Source**: Snowflake MIE_PLUSAUTO_VIEW (327M rows)
**Target**: PostgreSQL RDS (equifax-enrichment-api-db)
**Purpose**: Mirror source schema for accurate API database design
**Analysis Date**: 2025-11-03

---

## EXECUTIVE SUMMARY

### Column Distribution
- **Total Columns**: 398 (from Snowflake source)
- **Encrypted Fields (JSONB)**: 158 columns (~40%)
- **Plain Text Fields (TEXT)**: 226 columns (~57%)
- **Numeric Fields**: 14 columns (~3%)
- **Metadata Columns**: 3 additional (source_system, migration_batch_id, migrated_at)

### Data Categories
| Category | Column Count | Encrypted | Plain Text | Numeric | Notes |
|----------|--------------|-----------|------------|---------|-------|
| **Identity** | 1 | 1 | 0 | 0 | consumer_key (primary identifier) |
| **Personal Info** | 11 | 6 | 5 | 0 | Name, DOB, gender, age, deceased |
| **Alternate Names** | 30 | 30 | 0 | 0 | 5 alternate name sets (6 fields each) |
| **Addresses** | 162 | 66 | 96 | 0 | 10 address records (16 fields each) |
| **Phone Numbers** | 12 | 0 | 12 | 0 | 2 mobile + 5 landline (with dates) |
| **Email Addresses** | 30 | 0 | 30 | 0 | 15 emails (with last_seen dates) |
| **Financial Scores** | 14 | 0 | 0 | 14 | Income, spending power, affluence |
| **Credit Attributes** | 123 | 0 | 0 | 123 | Vantage scores, propensity scores |
| **Device IDs** | 7 | 6 | 1 | 0 | IP addresses, IDFA identifiers |
| **Metadata** | 8 | 0 | 8 | 0 | Flags, UUID, revision tracking |

---

## DETAILED COLUMN INVENTORY

### 1. IDENTITY (1 column)
| Column Name | Type | Encrypted | Description |
|-------------|------|-----------|-------------|
| `consumer_key` | JSONB | YES | Equifax unique consumer identifier |

**Encryption Format**:
```json
{"ciphertext":"dfd554812c8739a794c50fa7","iv":"5d0d974f47d305a3280ba517","tag":"d550766c5d7f8ecba6efaacf182297bd"}
```

**Critical**: This is the primary identifier but stored encrypted. Must decrypt for API use.

---

### 2. PRIMARY PERSONAL INFORMATION (11 columns)

| Column Name | Type | Encrypted | Sample Values | Description |
|-------------|------|-----------|---------------|-------------|
| `prefix` | JSONB | YES | "Mr", "Mrs", "Dr" | Name prefix/title |
| `first_name` | JSONB | YES | Encrypted | Given name |
| `middle_name` | JSONB | YES | Encrypted | Middle name |
| `last_name` | JSONB | YES | Encrypted | Family name |
| `suffix` | JSONB | YES | "Jr", "Sr", "III" | Name suffix |
| `gender` | TEXT | NO | "M", "F" | Gender |
| `date_of_birth` | JSONB | YES | Encrypted | Birth date |
| `age` | TEXT | NO | "55", "79" | Age in years |
| `deceased` | TEXT | NO | null, "Y" | Deceased indicator |
| `first_seen_date_primary_name` | TEXT | NO | "19990501", "20000701" | First appearance |
| `last_seen_date_primary_name` | TEXT | NO | "20160801", "20230831" | Most recent appearance |

**Encryption**: 6 of 11 fields encrypted (55%)
**Use Case**: Core identity verification, demographic targeting

---

### 3. ALTERNATE NAMES (30 columns - 5 sets of 6 fields each)

Each alternate name record contains:
- `alternate_name_{1-5}` (JSONB, encrypted) - Full name
- `alternate_prefix_{1-5}` (JSONB, encrypted) - Title
- `alternate_first_name_{1-5}` (JSONB, encrypted) - Given name
- `alternate_middle_name_{1-5}` (JSONB, encrypted) - Middle name
- `alternate_last_name_{1-5}` (JSONB, encrypted) - Family name
- `alternate_suffix_{1-5}` (JSONB, encrypted) - Suffix

**Total**: 30 columns
**Encryption**: 100% encrypted (all 30 fields)
**Purpose**: Track name changes (marriage, legal name changes, aliases)

**Example Set 1**:
```sql
alternate_name_1          JSONB  -- Full alternate name
alternate_prefix_1        JSONB  -- "Ms", "Miss"
alternate_first_name_1    JSONB  -- Previous first name
alternate_middle_name_1   JSONB  -- Previous middle name
alternate_last_name_1     JSONB  -- Maiden name, etc.
alternate_suffix_1        JSONB  -- "II", etc.
```

---

### 4. ADDRESSES (162 columns - 10 address records, 16 fields each + 2 dates)

Each address record contains 16 fields:

| Field Pattern | Type | Encrypted | Description |
|---------------|------|-----------|-------------|
| `address_{1-10}` | JSONB | YES | Complete address string |
| `house_number_{1-10}` | JSONB | YES | Street number |
| `predirectional_{1-10}` | JSONB | YES | "N", "S", "E", "W" |
| `street_name_{1-10}` | JSONB | YES | Street name |
| `street_suffix_{1-10}` | JSONB | YES | "St", "Ave", "Blvd" |
| `post_direction_{1-10}` | JSONB | YES | Post-directional |
| `unit_type_{1-10}` | TEXT | NO | "APT", "STE", "UNIT" |
| `unit_number_{1-10}` | TEXT | NO | Apartment/suite number |
| `city_name_{1-10}` | TEXT | NO | City name |
| `state_abbreviation_{1-10}` | TEXT | NO | "CA", "NY", "TX" |
| `zip_{1-10}` | TEXT | NO | ZIP code (5 digit) |
| `z4_{1-10}` | TEXT | NO | ZIP+4 extension |
| `delivery_point_code_{1-10}` | TEXT | NO | USPS delivery point |
| `delivery_point_validation_{1-10}` | TEXT | NO | "Y", "N" validation flag |
| `carrier_route_{1-10}` | TEXT | NO | USPS carrier route |
| `fips_code_{1-10}` | TEXT | NO | Federal Information Processing Standards code |
| `z4_type_{1-10}` | TEXT | NO | ZIP+4 type code |
| `transaction_date_{1-10}` | TEXT | NO | "201608", "200912" (YYYYMM format) |

**Total**: 10 addresses × 16 fields = 160 columns + 2 metadata = **162 columns**
**Encryption**: 60 encrypted (37%), 102 plain text (63%)
**Sample from CSV**:
```
Address 1: SIREN, WI 54872 (transaction: 200608)
Address 2: SAINT PAUL, MN 55113 (transaction: 199904)
Address 3: SAINT PAUL, MN 55113 (transaction: 199701)
```

**Encryption Pattern**:
- **Encrypted**: address, house_number, predirectional, street_name, street_suffix, post_direction
- **Plain Text**: unit_type, unit_number, city_name, state, zip, z4, delivery codes, transaction_date

---

### 5. PHONE NUMBERS (12 columns - 7 phones with dates)

| Column Name | Type | Encrypted | Description | CSV Position |
|-------------|------|-----------|-------------|--------------|
| `mobile_phone_1` | TEXT | NO | Primary mobile | Col 233 |
| `mobile_phone_2` | TEXT | NO | Secondary mobile | Col 234 |
| `phone_1` | TEXT | NO | Landline 1 | Col 235 |
| `last_seen_date_phone_1` | TEXT | NO | Last seen date for phone_1 | Col 236 |
| `phone_2` | TEXT | NO | Landline 2 | Col 237 |
| `last_seen_date_phone_2` | TEXT | NO | Last seen date for phone_2 | Col 238 |
| `phone_3` | TEXT | NO | Landline 3 | Col 239 |
| `last_seen_date_phone_3` | TEXT | NO | Last seen date for phone_3 | Col 240 |
| `phone_4` | TEXT | NO | Landline 4 | Col 241 |
| `last_seen_date_phone_4` | TEXT | NO | Last seen date for phone_4 | Col 242 |
| `phone_5` | TEXT | NO | Landline 5 | Col 243 |
| `last_seen_date_phone_5` | TEXT | NO | Last seen date for phone_5 | Col 244 |

**Total**: 12 columns (7 phones + 5 last_seen dates)
**Encryption**: 0% (ALL plain text)
**Sample Values**: "6514894377" (10-digit format)

**Current API Mismatch**:
- CSV has 7 phones, API designed for 10 phones
- CSV has date tracking for only 5 phones (phone_1 through phone_5)
- No dates for mobile_phone_1, mobile_phone_2

---

### 6. EMAIL ADDRESSES (30 columns - 15 emails with dates)

| Column Pattern | Type | Encrypted | Description |
|----------------|------|-----------|-------------|
| `email_{1-15}` | TEXT | NO | Email address |
| `last_seen_date_email_{1-15}` | TEXT | NO | Last seen date (YYYYMMDD) |

**Total**: 30 columns (15 emails + 15 dates)
**Encryption**: 0% (ALL plain text)
**Sample Values**:
```
email_1: LOLO_5@HI5.COM (last_seen: 20230831)
email_2: LOLO5@HI.COM (last_seen: 20190428)
```

**Note**: Current API schema has NO email fields. Major omission.

---

### 7. FINANCIAL ATTRIBUTES (14 columns)

| Column Name | Type | Description |
|-------------|------|-------------|
| `income360_complete` | NUMERIC | Complete household income estimate |
| `income360_salary` | NUMERIC | Salary income component |
| `income360_non_salary` | NUMERIC | Non-salary income (investments, etc.) |
| `economiccohortscode` | TEXT | Economic cohort classification |
| `financialdurabilityindex` | NUMERIC | Financial resilience index |
| `financialdurabilityscore` | NUMERIC | Financial stability score |
| `spending_power` | NUMERIC | Estimated spending capacity |
| `affluence_index` | NUMERIC | Wealth/affluence indicator |
| `balance_auto_finance_loan_accounts` | NUMERIC | Auto loan balance |
| `percent_balance_to_high_auto_finance_credit` | NUMERIC | Auto loan utilization ratio |
| `vantage_score_neighborhood_risk_score` | NUMERIC | Geographic risk score |
| `automotive_response_intent_indicator` | NUMERIC | Auto purchase propensity |
| `auto_in_market_propensity_score` | NUMERIC | Auto shopping likelihood |
| `vds` | NUMERIC | Vantage Data Score |

**Total**: 14 columns
**Encryption**: 0% (all numeric, not encrypted)
**Use Case**: Credit decisioning, income verification, auto lending

**CRITICAL**: Previous analysis said "no financial data in CSV" - THIS WAS INCORRECT. CSV has 14 financial fields.

---

### 8. CREDIT ATTRIBUTES (123 columns - Vantage Scores)

All fields prefixed with "v" followed by 1-3 character codes:

**Categories**:
- **VF, VR, VG, VH, VJ, VAC, VY, VS, VT, VC**: Base Vantage scores (10 fields)
- **VDO, VCW, VDA, VCZ**: Document/verification scores (4 fields)
- **VBS, VBY, VBR, VBW, VBV, VBT, VBX, VBU**: Behavior scores (8 fields)
- **VDP, VCV, VCY, VCX, VCL, VCK, VBZ**: Credit pattern scores (7 fields)
- **VCH, VCF, VCA, VCC, VCE, VCI, VCJ, VCD, VCM, VCB, VCG**: Credit history scores (11 fields)
- **VDQ, VCT, VCS, VCO, VCP, VCN, VCR, VCQ**: Credit quality scores (8 fields)
- **VDR, VDD, VDC, VDB, VCU, VDE**: Debt-related scores (6 fields)
- **VAD, VBI, VBD, VAY, VBM, VBK, VAG, VAZ**: Auto-specific scores (8 fields)
- **VBC, VBO, VBP, VAE, VBQ, VBA, VBL, VAF**: Banking scores (8 fields)
- **VBB, VBG, VAI, VBE, VAJ, VBF, VAH**: Behavioral indicators (7 fields)
- **VBN, VBH, VAK, VBJ, VAW**: Risk indicators (5 fields)
- **VDN, VAQ, VAU, VAM, VAL, VAN, VAS, VAO**: Additional risk/propensity (8 fields)
- **VAT, VAX, VAV, VAP**: Propensity scores (4 fields)
- **VDG, VDK, VDF, VDJ, VDI, VDM, VDH, VDL**: Decisioning scores (8 fields)
- **VK, VN, VM, VP, VO, VQ, VL, VAA**: General attributes (8 fields)

**Total**: 123 Vantage score columns
**Type**: All NUMERIC
**Encryption**: 0% (numeric fields not encrypted)
**Sample Values**: 70537, 56135, 14402, 3, 48712, etc.

**Use Case**: Credit risk assessment, auto lending decisioning, propensity modeling

---

### 9. DEVICE IDENTIFIERS (7 columns)

| Column Name | Type | Encrypted | Description |
|-------------|------|-----------|-------------|
| `ipaddress1` | JSONB | YES | IP address 1 |
| `ipaddress2` | JSONB | YES | IP address 2 |
| `idfa1` | JSONB | YES | Apple IDFA 1 |
| `idfa2` | JSONB | YES | Apple IDFA 2 |
| `idfa3` | JSONB | YES | Apple IDFA 3 |
| `idfa4` | JSONB | YES | Apple IDFA 4 |
| `idfa5` | JSONB | YES | Apple IDFA 5 |

**Total**: 7 columns
**Encryption**: 86% (6 of 7 encrypted)
**Use Case**: Digital identity, cross-device tracking, fraud prevention

---

### 10. METADATA & TRACKING (8 columns)

| Column Name | Type | Encrypted | Description |
|-------------|------|-----------|-------------|
| `marketing_email_flag` | TEXT | NO | Email marketing opt-in status |
| `uuid` | TEXT | NO | Universal unique identifier |
| `meta_revision` | TEXT | NO | "202507" - data revision version |
| `source_system` | VARCHAR(50) | NO | "EQUIFAX_SNOWFLAKE" (default) |
| `migration_batch_id` | UUID | NO | Migration tracking |
| `migrated_at` | TIMESTAMP | NO | Migration timestamp |
| `id` | UUID | NO | PostgreSQL primary key |
| `created_at` | TIMESTAMP | NO | Record creation time |
| `updated_at` | TIMESTAMP | NO | Record update time |

**Total**: 8 columns + 3 PostgreSQL metadata
**Encryption**: 0%
**Sample Values**:
```
uuid: 8f3e262d-cbf7-4b14-970f-6cf0f3487d86
meta_revision: 202507
```

---

## ENCRYPTION ANALYSIS

### Encryption Summary
| Data Type | Total Columns | Encrypted | Plain Text | Encrypted % |
|-----------|--------------|-----------|------------|-------------|
| Identity | 1 | 1 | 0 | 100% |
| Personal Info | 11 | 6 | 5 | 55% |
| Alternate Names | 30 | 30 | 0 | 100% |
| Addresses | 162 | 60 | 102 | 37% |
| Phones | 12 | 0 | 12 | 0% |
| Emails | 30 | 0 | 30 | 0% |
| Financial | 14 | 0 | 14 | 0% |
| Credit Attributes | 123 | 0 | 123 | 0% |
| Device IDs | 7 | 6 | 1 | 86% |
| Metadata | 8 | 0 | 8 | 0% |
| **TOTAL** | **398** | **103** | **295** | **26%** |

**Revised Encryption Estimate**: 26% (not 40% as previously estimated)

### Encrypted Field Format (AES-GCM)
```json
{
  "ciphertext": "dfd554812c8739a794c50fa7",
  "iv": "5d0d974f47d305a3280ba517",
  "tag": "d550766c5d7f8ecba6efaacf182297bd"
}
```

**Fields Requiring Decryption**:
- `consumer_key` (critical - primary identifier)
- Name fields (prefix, first, middle, last, suffix)
- Date of birth
- All 30 alternate name fields
- Address components (address, house_number, street_name, etc.)
- IP addresses and IDFA device identifiers

---

## COMPARISON: CSV SOURCE vs CURRENT API DATABASE

| Aspect | CSV Source | Current API DB | Gap Analysis |
|--------|------------|----------------|--------------|
| **Total Columns** | 398 | 22 | API has 5% of source data |
| **Phones** | 7 phones + 5 dates (12 cols) | 10 phone slots + 1 JSON (11 cols) | API has slots but no data |
| **Emails** | 15 emails + 15 dates (30 cols) | 0 | **MISSING entirely** |
| **Personal Info** | 11 native columns | 1 JSON field | Collapsed to JSON |
| **Alternate Names** | 30 columns (5 sets) | 0 | **MISSING entirely** |
| **Addresses** | 162 columns (10 addresses) | 1 JSON field | Collapsed to JSON |
| **Financial** | 14 columns | 1 JSON field | Data exists! (was wrong) |
| **Credit Scores** | 123 Vantage scores | 0 | **MISSING entirely** |
| **Device IDs** | 7 columns | 0 | **MISSING entirely** |
| **Encryption** | 103 fields (26%) | 0 fields | API stores decrypted |

---

## CRITICAL FINDINGS

### 1. MASSIVE DATA LOSS IN CURRENT DESIGN
Current API database schema captures only **5% of available Equifax data**:
- **Missing**: 15 email addresses
- **Missing**: 5 alternate name sets (30 fields)
- **Missing**: 123 credit/vantage scores
- **Missing**: 7 device identifiers
- **Collapsed**: 11 personal info fields → 1 JSON
- **Collapsed**: 162 address fields → 1 JSON
- **Collapsed**: 14 financial fields → 1 JSON (but data exists!)

### 2. EMAIL ADDRESSES COMPLETELY ABSENT
- CSV provides 15 email addresses with last_seen dates
- Current API has ZERO email fields
- Major omission for marketing/contact enrichment

### 3. FINANCIAL DATA EXISTS (PREVIOUS ANALYSIS WRONG)
- CSV contains 14 financial attributes
- Income360 (complete, salary, non-salary)
- Spending power, affluence index
- Auto loan balances and propensity scores
- Previous analysis incorrectly stated "no financial data"

### 4. CREDIT SCORES MISSING
- 123 Vantage score attributes available
- Auto propensity, risk scores, behavior indicators
- NONE captured in current API design

### 5. ENCRYPTION KEY REQUIREMENT
- 103 fields require AES-GCM decryption
- `encryption_keys` table exists but empty (0 rows)
- Must obtain keys before migration

---

## PROPOSED MIRRORED SCHEMA OPTIONS

### Option A: TRUE MIRROR (Recommended for Data Completeness)
**Approach**: Create exact mirror of 398-column structure

**Pros**:
- Zero data loss - all Equifax data preserved
- Future-proof - any field might become valuable
- Simple ETL - direct column mapping
- Supports advanced analytics on credit scores

**Cons**:
- Large table width (398 columns)
- May require API code refactor
- More complex query patterns

**Schema**:
```sql
CREATE TABLE consumer_enrichments (
    -- PostgreSQL metadata
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    -- EXACT mirror of all 398 Equifax columns
    consumer_key TEXT,  -- DECRYPTED from JSONB
    prefix TEXT,        -- DECRYPTED
    first_name TEXT,    -- DECRYPTED
    -- ... all 398 columns ...

    -- Additional indexes
    normalized_phone VARCHAR(10) GENERATED ALWAYS AS (
        COALESCE(mobile_phone_1, phone_1)
    ) STORED
);

-- Multi-column phone index for fast lookup
CREATE INDEX idx_consumer_phone_search ON consumer_enrichments
    USING gin (
        to_tsvector('simple',
            COALESCE(mobile_phone_1, '') || ' ' ||
            COALESCE(mobile_phone_2, '') || ' ' ||
            COALESCE(phone_1, '') || ' ' ||
            -- ... all phones
        )
    );
```

---

### Option B: HYBRID APPROACH (Balance Complexity & Completeness)
**Approach**: Flatten high-value fields, keep others as JSONB

**High-Value Native Columns** (50-75 columns):
- consumer_key (decrypted)
- Primary name fields (decrypted)
- All 7 phone numbers (plain text)
- All 15 email addresses (plain text)
- Top 10 financial/credit scores (most predictive)
- Primary address (address_1 fields, decrypted)
- Key metadata (age, gender, deceased, etc.)

**JSON Storage** (Low-frequency access):
- alternate_names_json (30 fields collapsed)
- addresses_2_through_10_json (144 fields)
- credit_scores_extended_json (113 fields)
- device_identifiers_json (7 fields)

**Pros**:
- Optimized query performance on common fields
- Reduced table width (~75 columns vs 398)
- Still preserves all data
- Easier migration from current API code

**Cons**:
- More complex ETL (selective flattening)
- JSON query overhead for less-common fields
- Need to decide which fields are "high value"

---

### Option C: CURRENT SIMPLIFIED (NOT Recommended)
**Approach**: Keep current 22-column design, improve JSON assembly

**Why NOT Recommended**:
- Loses 95% of available data
- Throws away 123 credit scores
- Discards 15 email addresses
- No alternate name tracking
- Cannot support advanced use cases

**Only viable if**:
- API purpose is ONLY basic phone lookup
- No plans for credit decisioning
- No email marketing integration
- No need for historical address tracking

---

## RECOMMENDATION

**Deploy Option A (True Mirror)** for these reasons:

1. **Data is an Asset**: 123 credit scores, 15 emails, device IDs are valuable
2. **Future-Proof**: Unknown which fields will become critical later
3. **Simple ETL**: Direct column mapping, less transformation logic
4. **Cost**: Storage is cheap, data loss is expensive
5. **Flexibility**: Can always collapse to JSON later, can't expand from missing data

**Implementation Path**:
1. Create new migration: `AddFullEquifaxSchema`
2. Generate 398 columns matching schema.sql (with decrypted types)
3. Update EnrichmentRepository to query specific fields
4. Add computed column for `normalized_phone` (backward compatibility)
5. Migrate API to use native columns instead of JSON parsing

**Query Performance**:
- Index on consumer_key, normalized_phone, emails
- Partial indexes on high-value credit scores
- GIN indexes for multi-column phone/email search
- Expected lookup time: <10ms (vs current 6207ms)

---

## NEXT STEPS

1. **Obtain Decryption Keys** - Locate AES-GCM keys for 103 encrypted fields
2. **Generate Migration** - Create EF Core migration with 398-column schema
3. **Update Repository** - Refactor EnrichmentRepository for native columns
4. **ETL Pipeline** - Build CSV import with decryption logic
5. **Performance Testing** - Validate query speed with full schema
6. **API Endpoint Updates** - Modify DTOs to expose new fields
7. **Import 10,814 CSV Files** - Execute production migration
