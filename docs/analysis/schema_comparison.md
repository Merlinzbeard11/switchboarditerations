# EQUIFAX SCHEMA COMPARISON

## API Database vs CSV/S3 Source

### **API Database: consumer_enrichments (22 columns)**
```
equifax-enrichment-api-db.cu9k2siys4p8.us-east-1.rds.amazonaws.com
Database: equifax_enrichment_api
```

### **CSV/S3 Source: equifax_records_full (281+ columns)**
```
S3: s3://sb-marketing-migration/equifax-export/full-export/
Backup DB: equifax-enrichment-backup.cu9k2siys4p8.us-east-1.rds.amazonaws.com
```

---

## FIELD MAPPING COMPARISON

| API Field | Type | CSV Source Column(s) | CSV Column # | Encryption | Notes |
|-----------|------|---------------------|--------------|------------|-------|
| **IDENTITY** |
| `id` | uuid | (generated) | - | No | Primary key, generated |
| `consumer_key` | varchar(100) | `consumer_key` | Col 3 | **YES** (AES-GCM) | Equifax consumer identifier |
| **PHONES** |
| `normalized_phone` | varchar(10) | `mobile_phone_1` OR `phone_1` | Col 225, 227 | No | Primary search field |
| `Phone1` | text | `mobile_phone_1` | Col 225 | No | 100% confidence match |
| `Phone2` | text | `mobile_phone_2` | Col 226 | No | 95% confidence match |
| `Phone3` | text | `phone_1` | Col 227 | No | 90% confidence match |
| `Phone4` | text | `phone_2` | Col 229 | No | 85% confidence match |
| `Phone5` | text | `phone_3` | Col 231 | No | 80% confidence match |
| `Phone6` | text | `phone_4` | Col 233 | No | 75% confidence match |
| `Phone7` | text | `phone_5` | Col 235 | No | 70% confidence match |
| `Phone8` | text | ❌ NOT IN CSV | - | - | No source data |
| `Phone9` | text | ❌ NOT IN CSV | - | - | No source data |
| `Phone10` | text | ❌ NOT IN CSV | - | - | No source data |
| `phones_json` | text | Multiple phone cols | 225-236 | Mixed | JSON array of all phones |
| **PERSONAL INFO** |
| `personal_info_json` | text | 43+ columns | 4-42 | **YES** (most) | first_name, last_name, DOB, etc. |
| **ADDRESSES** |
| `addresses_json` | text | 62+ columns | 45-150 | **YES** (partial) | Up to 7 address records |
| **FINANCIAL** |
| `financial_json` | text | ❌ NOT IN CSV | - | - | No financial data in CSV |
| **METADATA** |
| `match_confidence` | double | (calculated) | - | No | Derived from Phone column |
| `match_type` | varchar(50) | (calculated) | - | No | phone_only, phone_with_name |
| `data_freshness_date` | timestamptz | `last_seen_date_phone_1` | Col 228 | No | Most recent phone update |
| `created_at` | timestamptz | (generated) | - | No | Record creation time |
| `updated_at` | timestamptz | (generated) | - | No | Record update time |

---

## KEY DIFFERENCES

### ✅ AVAILABLE IN CSV
- **Personal Info**: 43 fields (first_name, last_name, DOB, gender, age, 5 alternate names)
- **Phones**: 7 phone numbers (mobile_phone_1, mobile_phone_2, phone_1-5) + dates
- **Addresses**: 62 fields (up to 7 address records with full details)
- **Other**: 164 additional fields (demographics, email, etc.)

### ❌ MISSING FROM CSV
- **Financial Data**: credit_score, income, home_value (API expects but CSV doesn't have)
- **Phone8-10**: API has slots for 10 phones, CSV only has 7
- **Match Metadata**: Calculated fields (confidence, match_type)

### 🔐 ENCRYPTION STATUS
- **Encrypted in CSV** (~40% of fields):
  - consumer_key
  - first_name, last_name, middle_name
  - Most address components (house_number, street_name)
  - Some alternate names
- **Plain Text in CSV** (~60% of fields):
  - All phone numbers ✅
  - gender, age, deceased
  - city_name, state, zip
  - dates (last_seen, transaction_date)

---

## TRANSFORMATION REQUIREMENTS

### 1. **Phone Number Extraction**
```
CSV Column 225 (mobile_phone_1) → normalized_phone + Phone1
CSV Column 226 (mobile_phone_2) → Phone2
CSV Column 227 (phone_1) → Phone3
CSV Column 229 (phone_2) → Phone4
CSV Column 231 (phone_3) → Phone5
CSV Column 233 (phone_4) → Phone6
CSV Column 235 (phone_5) → Phone7
Phone8-10 → NULL (no source data)
```

### 2. **Personal Info JSON Assembly**
Decrypt and combine 43+ encrypted/plain fields:
```json
{
  "first_name": decrypt(col_5),
  "last_name": decrypt(col_7),
  "middle_initial": decrypt(col_6),
  "date_of_birth": decrypt(col_10),
  "gender": col_9,
  "age": col_11,
  "deceased": col_12
}
```

### 3. **Addresses JSON Assembly**
Decrypt and structure 62 address fields into array:
```json
[
  {
    "address_type": "current",
    "street": decrypt(col_45),
    "city": col_53,
    "state": col_54,
    "postal_code": col_55,
    "transaction_date": col_62
  },
  // ... addresses 2-7
]
```

### 4. **Financial JSON**
❌ **PROBLEM**: API expects financial data but CSV doesn't have it
```json
{
  "credit_score": null,
  "estimated_income": null,
  "homeowner": null
}
```

---

## CRITICAL ISSUES

### 🔴 Issue #1: Financial Data Missing
- API schema expects `financial_json` with credit scores, income, home values
- CSV has NO financial data
- **Solution**: Populate with null/empty JSON or remove requirement

### 🔴 Issue #2: Decryption Required
- 40% of CSV fields encrypted with AES-GCM
- Need decryption keys from `encryption_keys` table (currently empty)
- **Solution**: Get keys from Snowflake or AWS KMS

### 🔴 Issue #3: Consumer Key Encrypted
- `consumer_key` is encrypted in CSV but used as identifier in API
- Must decrypt to populate API database
- **Solution**: Decrypt during ETL or use different identifier

### ⚠️ Issue #4: Phone Count Mismatch
- API supports 10 phones, CSV only has 7
- Phone8-10 will always be NULL
- **Solution**: Leave Phone8-10 as NULL, not a blocker

---

## SUMMARY

| Aspect | API Database | CSV Source | Status |
|--------|-------------|------------|--------|
| Total Columns | 22 | 281+ | ✅ CSV has more data |
| Phone Fields | 10 + 1 JSON | 7 plain text | ⚠️ Can map 7 of 10 |
| Personal Info | 1 JSON field | 43 fields | ✅ CSV has all needed |
| Addresses | 1 JSON field | 62 fields | ✅ CSV has all needed |
| Financial | 1 JSON field | 0 fields | ❌ **MISSING** |
| Encryption | None | 40% encrypted | ❌ **NEED KEYS** |

**Bottom Line**: CSV has MORE than enough data for the API (except financial), but requires decryption and JSON assembly transformation.
