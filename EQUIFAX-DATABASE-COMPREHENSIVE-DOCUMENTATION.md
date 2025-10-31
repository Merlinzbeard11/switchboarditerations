# EQUIFAX DATABASE COMPREHENSIVE DOCUMENTATION

**Generated:** October 31, 2025
**Purpose:** Complete reference for Equifax data integration
**Status:** Production-ready schema with 326.7M records

---

## TABLE OF CONTENTS

1. [Database Schema](#database-schema)
2. [AWS Credentials & Connection](#aws-credentials--connection)
3. [PII Encryption & Decryption](#pii-encryption--decryption)
4. [Connection Examples](#connection-examples)
5. [Security & Compliance](#security--compliance)

---

## DATABASE SCHEMA

### Table: `equifax_staging_all_text`
- **Total Columns:** 398
- **Total Records:** 326,718,517 (326.7 million)
- **Storage Size:** ~1.03 TiB (1,052 GB)
- **Data Format:** All columns stored as TEXT in staging table

### COMPLETE COLUMN LIST (All 398 Columns)

#### Personal Information (8 columns)
```
consumer_key          TEXT  -- Unique identifier for consumer
prefix                TEXT  -- Name prefix (Mr., Mrs., Dr., etc.)
first_name            TEXT  -- First name
middle_name           TEXT  -- Middle name
last_name             TEXT  -- Last name
suffix                TEXT  -- Name suffix (Jr., Sr., III, etc.)
gender                TEXT  -- Gender
date_of_birth         TEXT  -- Date of birth (YYYY-MM-DD)
age                   TEXT  -- Age in years
deceased              TEXT  -- Deceased flag
first_seen_date_primary_name  TEXT  -- First seen date for primary name
last_seen_date_primary_name   TEXT  -- Last seen date for primary name
```

#### Alternate Names (30 columns - 5 sets of 6 fields each)
```
-- Alternate Name Set 1
alternate_name_1              TEXT
alternate_prefix_1            TEXT
alternate_first_name_1        TEXT
alternate_middle_name_1       TEXT
alternate_last_name_1         TEXT
alternate_suffix_1            TEXT

-- Alternate Name Set 2
alternate_name_2              TEXT
alternate_prefix_2            TEXT
alternate_first_name_2        TEXT
alternate_middle_name_2       TEXT
alternate_last_name_2         TEXT
alternate_suffix_2            TEXT

-- Alternate Name Set 3
alternate_name_3              TEXT
alternate_prefix_3            TEXT
alternate_first_name_3        TEXT
alternate_middle_name_3       TEXT
alternate_last_name_3         TEXT
alternate_suffix_3            TEXT

-- Alternate Name Set 4
alternate_name_4              TEXT
alternate_prefix_4            TEXT
alternate_first_name_4        TEXT
alternate_middle_name_4       TEXT
alternate_last_name_4         TEXT
alternate_suffix_4            TEXT

-- Alternate Name Set 5
alternate_name_5              TEXT
alternate_prefix_5            TEXT
alternate_first_name_5        TEXT
alternate_middle_name_5       TEXT
alternate_last_name_5         TEXT
alternate_suffix_5            TEXT
```

#### Address Information (170 columns - 10 addresses × 17 fields each)
```
-- Address 1
address_1                     TEXT  -- Full address
house_number_1                TEXT
predirectional_1              TEXT  -- N, S, E, W, etc.
street_name_1                 TEXT
street_suffix_1               TEXT  -- St, Ave, Blvd, etc.
post_direction_1              TEXT
unit_type_1                   TEXT  -- Apt, Suite, etc.
unit_number_1                 TEXT
city_name_1                   TEXT
state_abbreviation_1          TEXT  -- CA, NY, TX, etc.
zip_1                         TEXT  -- ZIP code
z4_1                          TEXT  -- ZIP+4 extension
delivery_point_code_1         TEXT
delivery_point_validation_1   TEXT
carrier_route_1               TEXT
fips_code_1                   TEXT
z4_type_1                     TEXT
transaction_date_1            TEXT

-- Address 2
address_2                     TEXT
house_number_2                TEXT
predirectional_2              TEXT
street_name_2                 TEXT
street_suffix_2               TEXT
post_direction_2              TEXT
unit_type_2                   TEXT
unit_number_2                 TEXT
city_name_2                   TEXT
state_abbreviation_2          TEXT
zip_2                         TEXT
z4_2                          TEXT
delivery_point_code_2         TEXT
delivery_point_validation_2   TEXT
carrier_route_2               TEXT
fips_code_2                   TEXT
z4_type_2                     TEXT
transaction_date_2            TEXT

-- Addresses 3-10 follow same pattern (17 fields each)
-- Total: 10 addresses × 17 fields = 170 columns
```

#### Phone Numbers (12 columns)
```
mobile_phone_1        TEXT
mobile_phone_2        TEXT
phone_1               TEXT
last_seen_date_phone_1 TEXT
phone_2               TEXT
last_seen_date_phone_2 TEXT
phone_3               TEXT
last_seen_date_phone_3 TEXT
phone_4               TEXT
last_seen_date_phone_4 TEXT
phone_5               TEXT
last_seen_date_phone_5 TEXT
```

#### Email Addresses (30 columns - 15 emails × 2 fields each)
```
email_1               TEXT
last_seen_date_email_1 TEXT
email_2               TEXT
last_seen_date_email_2 TEXT
email_3               TEXT
last_seen_date_email_3 TEXT
email_4               TEXT
last_seen_date_email_4 TEXT
email_5               TEXT
last_seen_date_email_5 TEXT
email_6               TEXT
last_seen_date_email_6 TEXT
email_7               TEXT
last_seen_date_email_7 TEXT
email_8               TEXT
last_seen_date_email_8 TEXT
email_9               TEXT
last_seen_date_email_9 TEXT
email_10              TEXT
last_seen_date_email_10 TEXT
email_11              TEXT
last_seen_date_email_11 TEXT
email_12              TEXT
last_seen_date_email_12 TEXT
email_13              TEXT
last_seen_date_email_13 TEXT
email_14              TEXT
last_seen_date_email_14 TEXT
email_15              TEXT
last_seen_date_email_15 TEXT
```

#### Financial & Economic Data (6 columns)
```
income360_complete            BIGINT  -- Total income
income360_salary              BIGINT  -- Salary income
income360_non_salary          BIGINT  -- Non-salary income
economiccohortscode           TEXT    -- Economic cohort classification
financialdurabilityindex      BIGINT  -- Financial durability score
financialdurabilityscore      BIGINT  -- Financial durability index
```

#### Credit & Financial Scores (3 columns)
```
spending_power                        BIGINT
affluence_index                       BIGINT
balance_auto_finance_loan_accounts    BIGINT
percent_balance_to_high_auto_finance_credit BIGINT
vantage_score_neighborhood_risk_score BIGINT
```

#### Automotive Data (3 columns)
```
automotive_response_intent_indicator  BIGINT
auto_in_market_propensity_score       BIGINT
```

#### Proprietary Scores (123 "v" columns - vaa through vdq)
```
vds    BIGINT
vf     BIGINT
vr     BIGINT
vg     BIGINT
vh     BIGINT
vj     BIGINT
vac    BIGINT
vy     BIGINT
vs     BIGINT
vt     BIGINT
vc     BIGINT
vdo    BIGINT
vcw    BIGINT
vda    BIGINT
vcz    BIGINT
vbs    BIGINT
vby    BIGINT
vbr    BIGINT
vbw    BIGINT
vbv    BIGINT
vbt    BIGINT
vbx    BIGINT
vbu    BIGINT
vdp    BIGINT
vcv    BIGINT
vcy    BIGINT
vcx    BIGINT
vcl    BIGINT
vck    BIGINT
vbz    BIGINT
vch    BIGINT
vcf    BIGINT
vca    BIGINT
vcc    BIGINT
vce    BIGINT
vci    BIGINT
vcj    BIGINT
vcd    BIGINT
vcm    BIGINT
vcb    BIGINT
vcg    BIGINT
vdq    BIGINT
vct    BIGINT
vcs    BIGINT
vco    BIGINT
vcp    BIGINT
vcn    BIGINT
vcr    BIGINT
vcq    BIGINT
vdr    BIGINT
vdd    BIGINT
vdc    BIGINT
vdb    BIGINT
vcu    BIGINT
vde    BIGINT
vad    BIGINT
vbi    BIGINT
vbd    BIGINT
vay    BIGINT
vbm    BIGINT
vbk    BIGINT
vag    BIGINT
vaz    BIGINT
vbc    BIGINT
vbo    BIGINT
vbp    BIGINT
vae    BIGINT
vbq    BIGINT
vba    BIGINT
vbl    BIGINT
vaf    BIGINT
vbb    BIGINT
vbg    BIGINT
vai    BIGINT
vbe    BIGINT
vaj    BIGINT
vbf    BIGINT
vah    BIGINT
vbn    BIGINT
vbh    BIGINT
vak    BIGINT
vbj    BIGINT
vaw    BIGINT
vdn    BIGINT
vaq    BIGINT
vau    BIGINT
vam    BIGINT
val    BIGINT
van    BIGINT
vas    BIGINT
vao    BIGINT
vat    BIGINT
vax    BIGINT
vav    BIGINT
vap    BIGINT
vdg    BIGINT
vdk    BIGINT
vdi    BIGINT
vdf    BIGINT
vdj    BIGINT
vdh    BIGINT
vdl    BIGINT
vdm    BIGINT
vaa    BIGINT
vk     BIGINT
vl     BIGINT
vm     BIGINT
vn     BIGINT
vo     BIGINT
vp     BIGINT
vq     BIGINT
```

#### Device & Marketing Data (6 columns)
```
idfa1                 TEXT  -- Apple IDFA 1
idfa2                 TEXT  -- Apple IDFA 2
idfa3                 TEXT  -- Apple IDFA 3
idfa4                 TEXT  -- Apple IDFA 4
idfa5                 TEXT  -- Apple IDFA 5
ipaddress1            TEXT  -- IP Address 1
ipaddress2            TEXT  -- IP Address 2
marketing_email_flag  TEXT  -- Marketing email opt-in flag
```

#### Metadata (2 columns)
```
meta_revision         TEXT  -- Data revision timestamp
uuid                  TEXT  -- Universal unique identifier
```

---

## AWS CREDENTIALS & CONNECTION

### PostgreSQL RDS Database

**Endpoint:** `sb-marketing-postgres.cu9k2siys4p8.us-east-1.rds.amazonaws.com`
**Port:** `5432`
**Database:** `postgres`
**User:** `sbadmin`
**Password:** `kQwd5Z33U8yvLPtFcBLjWqwOjn5Y8E`
**Region:** `us-east-1`

### Snowflake Source (Read-Only View)

**Account:** `VXB91017`
**User:** `JEFF`
**Password:** `Itajubabrazil1!`
**Warehouse:** `SNOWFLAKE_LEARNING_WH`
**Database:** `EQUIFAX_AWS_USWEST2_SWITCHBOARD_SHARE`
**Schema:** `DATASET`
**View:** `MIE_PLUSAUTO_VIEW`

### AWS S3 Storage

**Bucket:** `sb-marketing-migration`
**Prefix:** `equifax-export/full-export/`
**Region:** `us-east-1`
**Files:** 10,814 CSV files
**Format:** Pipe-delimited (|), UTF-8 encoding, no header

---

## PII ENCRYPTION & DECRYPTION

### Encryption Algorithm

**Algorithm:** AES-256-GCM (Advanced Encryption Standard, Galois/Counter Mode)
**Key Length:** 256 bits (32 bytes)
**IV Length:** 128 bits (16 bytes)
**Authentication Tag:** 128 bits (16 bytes) - prevents tampering

### Environment Variables Required

```bash
# Required in .env or environment
PII_ENCRYPTION_KEY=<base64-encoded-32-byte-key>
PII_ENCRYPTION_KEY_ID=<key-identifier-for-rotation>
```

### Generate New Encryption Key

```typescript
import { PIIEncryptionService } from '@/app/lib/pii-encryption';

// Generate a new 256-bit encryption key
const newKey = PIIEncryptionService.generateKey();
console.log('Add to .env.local:');
console.log(`PII_ENCRYPTION_KEY=${newKey}`);
console.log(`PII_ENCRYPTION_KEY_ID=key-v2-${Date.now()}`);
```

### Encryption Process

1. **Input:** PII data as JSON object
2. **Generate:** Random 16-byte IV (initialization vector)
3. **Encrypt:** JSON data using AES-256-GCM
4. **Output:** `iv:authTag:encryptedData` (base64-encoded, colon-separated)

**Encrypted Data Format:**
```
[16-byte-iv-base64]:[16-byte-auth-tag-base64]:[encrypted-data-base64]
```

### Decryption Process

1. **Input:** Encrypted string from database
2. **Split:** Extract IV, auth tag, and encrypted data
3. **Verify:** Check authentication tag (prevents tampering)
4. **Decrypt:** Using AES-256-GCM with original key
5. **Output:** Original PII data as JSON object

### Encryption Example (TypeScript)

```typescript
import { getPIIEncryptionService } from '@/app/lib/pii-encryption';

const encryptionService = getPIIEncryptionService();

// PII data to encrypt
const piiData = {
  first_name: 'John',
  last_name: 'Doe',
  date_of_birth: '1985-03-15',
  phone_1: '555-123-4567',
  email_1: 'john.doe@example.com',
  zip_1: '90210'
};

// Encrypt
const encrypted = encryptionService.encrypt(piiData);

console.log(encrypted);
// {
//   encrypted_data: "base64iv:base64auth:base64encrypted",
//   key_id: "default-key-v1",
//   encrypted_at: "2025-01-26T10:30:00.000Z",
//   algorithm: "aes-256-gcm"
// }

// Store in database
await db.query(
  `INSERT INTO equifax_records_full (encrypted_pii, encryption_key_id, encrypted_at)
   VALUES ($1, $2, $3)`,
  [encrypted.encrypted_data, encrypted.key_id, encrypted.encrypted_at]
);
```

### Decryption Example (TypeScript)

```typescript
// Retrieve from database
const row = await db.query('SELECT * FROM equifax_records_full WHERE id = $1', [recordId]);

const encryptedPII = {
  encrypted_data: row.encrypted_pii,
  key_id: row.encryption_key_id,
  encrypted_at: row.encrypted_at,
  algorithm: 'aes-256-gcm'
};

// FCRA-compliant audit information (REQUIRED)
const auditInfo = {
  equifax_record_id: row.id,
  user_id: currentUser.id,
  fields_accessed: ['first_name', 'last_name', 'phone_1'],
  permissible_purpose: 'insurance_quote', // FCRA permissible purpose
  accessed_at: new Date().toISOString()
};

// Decrypt
const piiData = encryptionService.decrypt(encryptedPII, auditInfo);

console.log(piiData.first_name); // 'John'
console.log(piiData.last_name);  // 'Doe'
console.log(piiData.phone_1);    // '555-123-4567'
```

### Selective Field Decryption (Minimize Exposure)

```typescript
// Decrypt ONLY specific fields (best practice)
const partialPII = encryptionService.decryptFields(
  encryptedPII,
  ['phone_1', 'email_1'], // Only these fields
  {
    equifax_record_id: row.id,
    user_id: currentUser.id,
    permissible_purpose: 'contact_verification',
    accessed_at: new Date().toISOString()
  }
);

console.log(partialPII.phone_1);  // '555-123-4567'
console.log(partialPII.email_1);  // 'john.doe@example.com'
console.log(partialPII.ssn);      // undefined (not requested)
```

### Key Rotation

```typescript
// When rotating encryption keys
const oldKeyBase64 = process.env.PII_ENCRYPTION_KEY_OLD;

const reEncrypted = encryptionService.rotateKey(
  encryptedPII,
  oldKeyBase64,
  auditInfo
);

// Update database with newly encrypted data
await db.query(
  `UPDATE equifax_records_full
   SET encrypted_pii = $1, encryption_key_id = $2, encrypted_at = $3
   WHERE id = $4`,
  [reEncrypted.encrypted_data, reEncrypted.key_id, reEncrypted.encrypted_at, recordId]
);
```

---

## CONNECTION EXAMPLES

### PostgreSQL Connection (Python)

```python
import psycopg2
import os
from dotenv import load_dotenv

load_dotenv()

# Connect to PostgreSQL RDS
conn = psycopg2.connect(
    host='sb-marketing-postgres.cu9k2siys4p8.us-east-1.rds.amazonaws.com',
    port=5432,
    user='sbadmin',
    password=os.getenv('RDS_PASSWORD', 'kQwd5Z33U8yvLPtFcBLjWqwOjn5Y8E'),
    database='postgres',
    connect_timeout=30,
    # TCP keepalive (prevents connection timeout)
    keepalives=1,
    keepalives_idle=60,
    keepalives_interval=10,
    keepalives_count=5
)

# Query example
cursor = conn.cursor()
cursor.execute("SELECT COUNT(*) FROM equifax_staging_all_text")
count = cursor.fetchone()[0]
print(f"Total records: {count:,}")

cursor.close()
conn.close()
```

### PostgreSQL Connection (Node.js / TypeScript)

```typescript
import { Pool } from 'pg';

const pool = new Pool({
  host: 'sb-marketing-postgres.cu9k2siys4p8.us-east-1.rds.amazonaws.com',
  port: 5432,
  user: 'sbadmin',
  password: process.env.RDS_PASSWORD || 'kQwd5Z33U8yvLPtFcBLjWqwOjn5Y8E',
  database: 'postgres',
  max: 20, // Maximum connections in pool
  idleTimeoutMillis: 30000,
  connectionTimeoutMillis: 10000,
});

// Query example
const result = await pool.query('SELECT COUNT(*) FROM equifax_staging_all_text');
console.log(`Total records: ${result.rows[0].count}`);

// Remember to close pool when done
await pool.end();
```

### Snowflake Connection (Python)

```python
import snowflake.connector
import os
from dotenv import load_dotenv

load_dotenv()

# Connect to Snowflake
conn = snowflake.connector.connect(
    user=os.getenv('SNOWFLAKE_USER', 'JEFF'),
    password=os.getenv('SNOWFLAKE_PASSWORD', 'Itajubabrazil1!'),
    account=os.getenv('SNOWFLAKE_ACCOUNT', 'VXB91017'),
    warehouse='SNOWFLAKE_LEARNING_WH',
    database='EQUIFAX_AWS_USWEST2_SWITCHBOARD_SHARE',
    schema='DATASET'
)

# Query example
cursor = conn.cursor()
cursor.execute("SELECT COUNT(*) FROM MIE_PLUSAUTO_VIEW")
count = cursor.fetchone()[0]
print(f"Total records in Snowflake view: {count:,}")

cursor.close()
conn.close()
```

### AWS CLI Configuration

```bash
# Configure AWS CLI for S3 access
aws configure set region us-east-1

# List files in S3 bucket
aws s3 ls s3://sb-marketing-migration/equifax-export/full-export/ --recursive

# Download a file from S3
aws s3 cp s3://sb-marketing-migration/equifax-export/full-export/data_0_0_0.csv.gz ./

# Upload a file to S3
aws s3 cp local-file.csv s3://sb-marketing-migration/uploads/
```

---

## SECURITY & COMPLIANCE

### FCRA Compliance Requirements

1. **Permissible Purpose:** All PII access MUST have documented FCRA permissible purpose
2. **Access Logging:** All PII decryption MUST be logged with audit trail
3. **Minimal Exposure:** Only decrypt fields actually needed for the purpose
4. **User Attribution:** Track WHO accessed WHAT and WHY
5. **Timestamp Tracking:** Record WHEN data was accessed

### FCRA Permissible Purposes

Valid reasons for accessing Equifax PII (per Fair Credit Reporting Act):

- `credit_application` - Consumer applying for credit
- `employment_screening` - Background check for employment
- `insurance_underwriting` - Insurance application or renewal
- `legitimate_business_need` - Account review, collections
- `court_order` - Legal subpoena or court order
- `written_consumer_consent` - Consumer provided explicit consent

### Audit Logging

Every PII access MUST be logged:

```sql
CREATE TABLE equifax_pii_access_log (
    id                      BIGSERIAL PRIMARY KEY,
    equifax_record_id       VARCHAR(255) NOT NULL,
    user_id                 VARCHAR(255),
    fields_accessed         TEXT[], -- Array of field names
    permissible_purpose     VARCHAR(100) NOT NULL,
    accessed_at             TIMESTAMPTZ NOT NULL,
    ip_address              INET,
    user_agent              TEXT
);
```

### Data Retention

- **Raw Data:** 7 years minimum (FCRA requirement)
- **Access Logs:** 25 months minimum (FCRA requirement)
- **Encryption Keys:** Maintain old keys for 12 months after rotation

### Security Best Practices

1. **Encryption at Rest:** All PII encrypted using AES-256-GCM
2. **Encryption in Transit:** TLS 1.2+ for all database connections
3. **Key Rotation:** Rotate encryption keys every 90 days
4. **Access Control:** Role-based access control (RBAC) for PII
5. **Network Security:** VPC isolation, security groups, no public access
6. **Monitoring:** Alert on suspicious access patterns
7. **Backup Security:** Encrypted backups with separate encryption keys

---

## APPENDIX: QUICK REFERENCE

### Database Sizes

| Metric | Value |
|--------|-------|
| Total Records | 326,718,517 |
| Total Columns | 398 |
| Storage Size | ~1.03 TiB |
| CSV Files (S3) | 10,814 |
| Avg File Size | ~100 MB |

### Connection Strings

**PostgreSQL:**
```
postgresql://sbadmin:kQwd5Z33U8yvLPtFcBLjWqwOjn5Y8E@sb-marketing-postgres.cu9k2siys4p8.us-east-1.rds.amazonaws.com:5432/postgres
```

**Snowflake:**
```
VXB91017.snowflakecomputing.com
User: JEFF
Password: Itajubabrazil1!
Warehouse: SNOWFLAKE_LEARNING_WH
Database: EQUIFAX_AWS_USWEST2_SWITCHBOARD_SHARE
Schema: DATASET
```

### Environment Variable Template

```bash
# PostgreSQL RDS
RDS_ENDPOINT=sb-marketing-postgres.cu9k2siys4p8.us-east-1.rds.amazonaws.com
RDS_PORT=5432
RDS_USER=sbadmin
RDS_PASSWORD=kQwd5Z33U8yvLPtFcBLjWqwOjn5Y8E
RDS_DATABASE=postgres

# Snowflake
SNOWFLAKE_ACCOUNT=VXB91017
SNOWFLAKE_USER=JEFF
SNOWFLAKE_PASSWORD=Itajubabrazil1!
SNOWFLAKE_WAREHOUSE=SNOWFLAKE_LEARNING_WH
SNOWFLAKE_DATABASE=EQUIFAX_AWS_USWEST2_SWITCHBOARD_SHARE
SNOWFLAKE_SCHEMA=DATASET

# PII Encryption
PII_ENCRYPTION_KEY=<generate-with-PIIEncryptionService.generateKey()>
PII_ENCRYPTION_KEY_ID=key-v1-$(date +%s)

# AWS
AWS_REGION=us-east-1
S3_BUCKET=sb-marketing-migration
```

---

## SUPPORT & CONTACT

**Project:** SwitchBoard Marketing - Equifax Integration
**Database Owner:** sbadmin
**Region:** us-east-1
**Last Updated:** October 31, 2025

**CRITICAL:** This document contains sensitive credentials. Store securely and restrict access.
