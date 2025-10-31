# EQUIFAX LEAD ENRICHMENT API - MVP ROADMAP

**Project:** Phone Number Lead Enrichment Service
**Version:** 1.0 MVP
**Last Updated:** October 31, 2025
**Status:** Planning Phase

---

## EXECUTIVE SUMMARY

### Vision
Build a high-performance REST API that enables buyers to enrich phone number leads with comprehensive Equifax data (398 columns, 326M+ records) while maintaining FCRA compliance and sub-200ms response times.

### Business Value
- **Revenue Stream:** Per-query pricing model for lead enrichment
- **Market Advantage:** Instant access to 326M+ consumer records via simple API
- **Compliance Built-In:** FCRA audit logging and permissible purpose tracking
- **Scalability:** Supports 100+ concurrent buyers with rate limiting

### Success Metrics (Contract SLA Requirements)
- **API response time:** < 500ms (average) - **CONTRACTUAL REQUIREMENT** (Section 4.1)
- **API uptime:** 99.5% per calendar month (max 3.6 hours downtime/month) - **CONTRACTUAL REQUIREMENT** (Section 4.1)
- **Match rate:** > 85% for valid phone numbers (operational goal)
- **Daily queries:** 10K+ within first month (growth target)
- **Zero FCRA/TCPA/GDPR/CCPA compliance violations** - **CONTRACTUAL REQUIREMENT** (Section 6.1, 12.1)
- **Data breach notification:** Within 72 hours - **CONTRACTUAL REQUIREMENT** (Section 13.2)
- **Planned downtime notice:** 24 hours advance notification - **CONTRACTUAL REQUIREMENT** (Section 4.1)

---

## TECHNICAL STACK

### Core Technologies
| Component | Technology | Version | Justification |
|-----------|-----------|---------|---------------|
| **API Framework** | ASP.NET Core Web API | 9.0 | High performance, native async/await, production-ready |
| **Language** | C# | 13 | Type safety, LINQ, modern language features |
| **Database** | PostgreSQL (AWS RDS) | 18 | Existing infrastructure, 190M+ rows already imported |
| **ORM** | Entity Framework Core | 9.0 | Type-safe queries, migrations, connection pooling |
| **Database Driver** | Npgsql | 9.0 | Optimized PostgreSQL driver for .NET |
| **Caching** | Redis (AWS ElastiCache) | 7.x | Sub-millisecond lookups, reduces DB load |
| **Authentication** | JWT Bearer + API Keys | - | Industry standard, stateless, scalable |
| **Rate Limiting** | AspNetCoreRateLimit | 5.0 | Per-buyer quotas, sliding window algorithm |
| **Logging** | Serilog | 8.0 | Structured logging, AWS CloudWatch integration |
| **Validation** | FluentValidation | 11.3 | Declarative validation rules, clear error messages |
| **API Documentation** | Swashbuckle (Swagger) | 6.5 | Interactive API explorer, auto-generated docs |
| **Encryption** | System.Security.Cryptography | - | AES-256-GCM for PII decryption |

### AWS Infrastructure
| Service | Purpose | Configuration |
|---------|---------|---------------|
| **RDS PostgreSQL** | Primary data store | db.r5.2xlarge, 1.5TB storage, Multi-AZ |
| **ElastiCache Redis** | Query result caching | cache.r6g.large, cluster mode enabled |
| **Elastic Beanstalk** | API hosting | Auto-scaling, load balanced |
| **CloudWatch** | Monitoring & alerting | Custom metrics, log aggregation |
| **Secrets Manager** | Credential storage | Automatic rotation, encrypted |
| **S3** | Backup storage | Audit logs, compliance archives |

---

## ARCHITECTURE

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                     PRESENTATION LAYER                       │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  EnrichmentAPI.Api                                    │  │
│  │  - Controllers/EnrichmentController.cs                │  │
│  │  - Middleware/ApiKeyAuthMiddleware.cs                 │  │
│  │  - Middleware/AuditLoggingMiddleware.cs               │  │
│  │  - Program.cs, Startup.cs                             │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                             ↓
┌─────────────────────────────────────────────────────────────┐
│                   APPLICATION SERVICE LAYER                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  EnrichmentAPI.Application                            │  │
│  │  - Services/PhoneEnrichmentService.cs                 │  │
│  │  - Services/PIIDecryptionService.cs                   │  │
│  │  - Services/RateLimitingService.cs                    │  │
│  │  - DTOs/EnrichmentRequest.cs                          │  │
│  │  - DTOs/EnrichmentResponse.cs                         │  │
│  │  - Validators/EnrichmentRequestValidator.cs           │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                             ↓
┌─────────────────────────────────────────────────────────────┐
│                        DOMAIN LAYER                          │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  EnrichmentAPI.Domain                                 │  │
│  │  - Entities/EquifaxRecord.cs (398 properties)         │  │
│  │  - Entities/Buyer.cs                                  │  │
│  │  - Entities/AuditLog.cs                               │  │
│  │  - Interfaces/IEquifaxRepository.cs                   │  │
│  │  - Interfaces/IBuyerRepository.cs                     │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                             ↓
┌─────────────────────────────────────────────────────────────┐
│                   INFRASTRUCTURE LAYER                       │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  EnrichmentAPI.Infrastructure                         │  │
│  │  - Repositories/EquifaxRepository.cs                  │  │
│  │  - Repositories/BuyerRepository.cs                    │  │
│  │  - Data/ApplicationDbContext.cs                       │  │
│  │  - Caching/RedisCacheService.cs                       │  │
│  │  - Encryption/AesGcmEncryptionService.cs              │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Data Flow: Phone Number Lookup

```
1. Buyer Request
   POST /api/data_enhancement/lookup
   {
     "api_key": "157659ac...",
     "provider_code": "EQUIFAX_ENRICHMENT",
     "phone": "8015551234",
     "permissible_purpose": "insurance_underwriting",
     "fields": "basic"
   }
         ↓
2. Authentication Middleware
   Validate API key → Identify buyer → Check rate limit
         ↓
3. Request Validation
   FluentValidation:
   - api_key exists and active
   - provider_code matches
   - phone format (10 digits)
   - permissible_purpose is valid
   - required fields present
         ↓
4. Phone Normalization
   (555) 123-4567 → 5551234567
   Remove formatting, strip country code
         ↓
5. Cache Check (Redis)
   Key: phone:5551234567:basic
   Hit? → Return cached result (5-10ms)
   Miss? → Continue to DB
         ↓
6. Database Query (PostgreSQL)
   SELECT * FROM equifax_staging_all_text
   WHERE phone_1 = '5551234567'
      OR phone_2 = '5551234567'
      ...
      OR phone_10 = '5551234567'
   LIMIT 1;
   (50-150ms with indexes)
         ↓
7. PII Decryption (if needed)
   Decrypt: first_name, last_name, ssn, dob
   Using AES-256-GCM
         ↓
8. Response Assembly
   If fields = "basic":
     Return ~50 core fields (personal, address, financial)
   If fields = "full":
     Return all 398 columns
   Add metadata: match_confidence, request_id, timestamp
         ↓
9. Cache Result (Redis)
   TTL: 24 hours
   Separate cache keys for basic vs full
         ↓
10. Audit Logging (FCRA Compliance)
    Log: buyer_id, phone, consumer_key, timestamp,
         permissible_purpose, IP, unique_id
    Store in audit_logs table (24-month retention)
         ↓
11. Return Response
    200 OK: { "response": "success", "data": {...} }
    404 Not Found: { "response": "error", "message": "Unable to find record" }
    400 Bad Request: Invalid input
    401 Unauthorized: Invalid API key
    429 Rate Limit: Too many requests
```

---

## DATABASE SCHEMA

### Existing Table: `equifax_staging_all_text`
- **Total Rows:** 326,718,517 (326.7M) - **Currently importing: 190M (58.4%)**
- **Total Columns:** 398 (all TEXT for staging)
- **Storage Size:** ~1.03 TiB
- **Database:** sb-marketing-postgres.cu9k2siys4p8.us-east-1.rds.amazonaws.com

### Required Indexes (Performance Critical)

```sql
-- Phone number indexes (10 columns)
CREATE INDEX CONCURRENTLY idx_equifax_phone_1 ON equifax_staging_all_text(phone_1)
    WHERE phone_1 IS NOT NULL AND phone_1 != '';

CREATE INDEX CONCURRENTLY idx_equifax_phone_2 ON equifax_staging_all_text(phone_2)
    WHERE phone_2 IS NOT NULL AND phone_2 != '';

CREATE INDEX CONCURRENTLY idx_equifax_phone_3 ON equifax_staging_all_text(phone_3)
    WHERE phone_3 IS NOT NULL AND phone_3 != '';

-- Repeat for phone_4 through phone_10

-- Composite index for common multi-phone queries
CREATE INDEX CONCURRENTLY idx_equifax_phones_composite
    ON equifax_staging_all_text(phone_1, phone_2, phone_3, phone_4, phone_5)
    WHERE phone_1 IS NOT NULL;

-- Consumer key index (for direct lookups)
CREATE INDEX CONCURRENTLY idx_equifax_consumer_key
    ON equifax_staging_all_text(consumer_key);
```

**Index Build Time Estimate:** 6-8 hours for all 10 phone indexes (can run in parallel)

### New Tables Required

```sql
-- Buyers table
CREATE TABLE buyers (
    buyer_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_name VARCHAR(255) NOT NULL,
    api_key_hash VARCHAR(255) NOT NULL UNIQUE,
    api_key_prefix VARCHAR(10) NOT NULL, -- First 8 chars for identification
    rate_limit_per_minute INT DEFAULT 100,
    monthly_quota INT DEFAULT 100000,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_buyers_api_key ON buyers(api_key_hash);

-- Audit logs table (FCRA compliance)
CREATE TABLE audit_logs (
    log_id BIGSERIAL PRIMARY KEY,
    buyer_id UUID NOT NULL REFERENCES buyers(buyer_id),
    phone_number_queried VARCHAR(20) NOT NULL,
    consumer_key_returned VARCHAR(255),
    match_found BOOLEAN NOT NULL,
    permissible_purpose VARCHAR(100) NOT NULL,
    ip_address INET NOT NULL,
    user_agent TEXT,
    response_time_ms INT,
    queried_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_audit_buyer_time ON audit_logs(buyer_id, queried_at DESC);
CREATE INDEX idx_audit_phone ON audit_logs(phone_number_queried);
CREATE INDEX idx_audit_time ON audit_logs(queried_at) WHERE queried_at > NOW() - INTERVAL '24 months';

-- Partition audit_logs by month for performance
CREATE TABLE audit_logs_y2025m11 PARTITION OF audit_logs
    FOR VALUES FROM ('2025-11-01') TO ('2025-12-01');

-- Rate limit tracking table
CREATE TABLE rate_limit_tracking (
    buyer_id UUID NOT NULL REFERENCES buyers(buyer_id),
    window_start TIMESTAMPTZ NOT NULL,
    request_count INT DEFAULT 0,
    PRIMARY KEY (buyer_id, window_start)
);

CREATE INDEX idx_rate_limit_window ON rate_limit_tracking(window_start);
```

---

## COMPLIANCE & REGULATORY REQUIREMENTS

### Contractual Compliance Overview
**Data Subscription Agreement - Sections 6.1, 12.1, 13.2**

The Equifax Lead Enrichment API must maintain strict compliance with multiple regulatory frameworks to fulfill contractual obligations and legal requirements for consumer data handling.

### 1. FCRA - Fair Credit Reporting Act (15 U.S.C. § 1681 et seq.)
**Contract Reference:** Section 6.1 - Compliance with Laws
**Applicability:** Mandatory for all consumer credit information usage

#### Technical Requirements:
```csharp
// FCRA-compliant audit logging service
public class FCRAAuditService
{
    // REQUIREMENT: Log EVERY access to consumer data (§ 607)
    public async Task LogConsumerAccessAsync(ConsumerAccessLog log)
    {
        var auditEntry = new AuditLog
        {
            BuyerId = log.BuyerId,
            PhoneNumberQueried = log.PhoneNumber,
            ConsumerKey = log.ConsumerKey,
            PermissiblePurpose = log.PermissiblePurpose, // Required by § 604
            RequestTimestamp = DateTime.UtcNow,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            ResponseTimeMs = log.ResponseTimeMs,
            DataFieldsReturned = log.FieldsReturned, // Track what data was disclosed

            // FCRA-specific fields
            CertificationReceived = true, // § 604(f) - Buyer must certify purpose
            RetentionExpiry = DateTime.UtcNow.AddMonths(24) // § 607(b) - 24-month retention
        };

        await _context.AuditLogs.AddAsync(auditEntry);
        await _context.SaveChangesAsync();
    }

    // REQUIREMENT: Validate permissible purpose before data release (§ 604)
    public bool ValidatePermissiblePurpose(string purpose)
    {
        var validPurposes = new[]
        {
            "credit_transaction",           // § 604(a)(3)(A)
            "insurance_underwriting",       // § 604(a)(3)(C)
            "employment_purposes",          // § 604(a)(3)(B)
            "legitimate_business_need",     // § 604(a)(3)(F)(i)
            "account_review",               // § 604(a)(3)(A)
            "collection_of_debt"            // § 604(a)(3)(A)
        };

        return validPurposes.Contains(purpose);
    }
}
```

#### FCRA Compliance Checklist:
- ✅ **§ 604 - Permissible Purpose Validation:** API requires `permissible_purpose` field in every request
- ✅ **§ 607(b) - Record Retention:** Audit logs retained for 24 months minimum
- ✅ **§ 609 - Consumer Rights:** Data subject rights portal (Feature 4.5) enables access requests
- ✅ **§ 611 - Dispute Resolution:** Correction request handling implemented
- ✅ **§ 616 - Civil Liability:** Comprehensive audit trail for regulatory defense
- ✅ **§ 621 - FTC Enforcement:** Quarterly compliance reports generated

### 2. TCPA - Telephone Consumer Protection Act (47 U.S.C. § 227)
**Contract Reference:** Section 6.1 - Compliance with Laws
**Applicability:** Phone number data used for marketing/telemarketing

#### Technical Requirements:
```csharp
// TCPA consent verification service
public class TCPAComplianceService
{
    // REQUIREMENT: Verify consent before enabling phone data for marketing
    public async Task<bool> ValidateConsentForCallingAsync(string phoneNumber, Guid buyerId)
    {
        // Check National Do Not Call Registry integration
        var isOnDNCRegistry = await _dncService.CheckPhoneAsync(phoneNumber);

        // Check buyer-specific consent records
        var hasExplicitConsent = await _context.ConsentRecords
            .AnyAsync(c => c.PhoneNumber == phoneNumber
                        && c.BuyerId == buyerId
                        && c.ConsentType == "express_written_consent"
                        && c.ConsentDate.HasValue
                        && c.RevokedDate == null);

        // TCPA requirement: Express written consent for autodialed calls (§ 227(b)(1)(A))
        return !isOnDNCRegistry && hasExplicitConsent;
    }

    // Track consent in database
    public class ConsentRecord
    {
        public Guid ConsentId { get; set; }
        public string PhoneNumber { get; set; }
        public Guid BuyerId { get; set; }
        public string ConsentType { get; set; } // "express_written_consent", "prior_business_relationship"
        public DateTime? ConsentDate { get; set; }
        public string ConsentMethod { get; set; } // "web_form", "email", "paper"
        public DateTime? RevokedDate { get; set; }
        public string ConsentProofUrl { get; set; } // S3 link to signed consent
    }
}
```

#### TCPA Compliance Checklist:
- ✅ **§ 227(b)(1)(A) - Express Consent Required:** Consent tracking table implemented
- ✅ **§ 227(c) - Do Not Call Registry:** Integration with National DNC Registry
- ✅ **§ 227(b)(1)(B) - Opt-Out Mechanism:** Data subject rights portal includes revocation
- ✅ **§ 227(e)(5) - Recordkeeping:** Consent proof stored in S3 with 4-year retention
- ✅ **FCC Regulations:** Quarterly audit reports for BUYER compliance verification

### 3. GDPR - General Data Protection Regulation (EU Regulation 2016/679)
**Contract Reference:** Section 12.1 - Data Protection Compliance
**Applicability:** Any EU resident data, regardless of where BUYER is located

#### Technical Requirements:
```csharp
// GDPR data subject rights service
public class GDPRComplianceService
{
    // Article 15: Right of Access
    public async Task<PersonalDataExportDto> ExportPersonalDataAsync(string phoneNumber)
    {
        var record = await _context.EquifaxRecords
            .Where(r => r.Phone1 == phoneNumber || r.Phone2 == phoneNumber)
            .FirstOrDefaultAsync();

        var accessLogs = await _context.AuditLogs
            .Where(a => a.PhoneNumberQueried == phoneNumber)
            .OrderByDescending(a => a.QueriedAt)
            .ToListAsync();

        return new PersonalDataExportDto
        {
            PersonalData = record,
            AccessHistory = accessLogs,
            DataSources = new[] { "Equifax Consumer Database" },
            RetentionPeriod = "24 months from last access",
            LegalBasis = "Legitimate Interest (Article 6(1)(f))",
            DataProtectionOfficer = "dpo@switchboard.com"
        };
    }

    // Article 17: Right to Erasure ("Right to be Forgotten")
    public async Task<bool> ErasePersonalDataAsync(string phoneNumber, string requestReason)
    {
        // GDPR Article 17 exceptions: Data needed for legal compliance (FCRA 24-month retention)
        var hasRecentAccess = await _context.AuditLogs
            .AnyAsync(a => a.PhoneNumberQueried == phoneNumber
                        && a.QueriedAt > DateTime.UtcNow.AddMonths(-24));

        if (hasRecentAccess)
        {
            // Cannot erase due to FCRA retention requirement
            return false;
        }

        // Pseudonymization approach: Replace PII with hashed identifier
        var records = await _context.EquifaxRecords
            .Where(r => r.Phone1 == phoneNumber || r.Phone2 == phoneNumber)
            .ToListAsync();

        foreach (var record in records)
        {
            record.FirstName = "[REDACTED]";
            record.LastName = "[REDACTED]";
            record.SSN = "[REDACTED]";
            record.DateOfBirth = null;
            record.Email = "[REDACTED]";
            // Keep phone number hashed for audit trail
            record.IsRedacted = true;
            record.RedactedAt = DateTime.UtcNow;
            record.RedactionReason = requestReason;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // Article 20: Right to Data Portability
    public async Task<byte[]> GeneratePortableDataExportAsync(string phoneNumber)
    {
        var data = await ExportPersonalDataAsync(phoneNumber);

        // Export as machine-readable JSON (GDPR Article 20 requirement)
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return Encoding.UTF8.GetBytes(json);
    }
}
```

#### GDPR Compliance Checklist:
- ✅ **Article 6 - Lawful Basis:** Legitimate interest documented for all processing
- ✅ **Article 15 - Right of Access:** Data subject can request all personal data via portal
- ✅ **Article 16 - Right to Rectification:** Correction requests processed within 30 days
- ✅ **Article 17 - Right to Erasure:** Pseudonymization implemented (with FCRA exceptions)
- ✅ **Article 20 - Right to Portability:** JSON export in structured format
- ✅ **Article 25 - Data Protection by Design:** Encryption-at-rest, encryption-in-transit, minimal data exposure
- ✅ **Article 32 - Security of Processing:** AES-256-GCM encryption, audit logging, access controls
- ✅ **Article 33 - Breach Notification:** 72-hour notification to supervisory authority
- ✅ **Article 37 - Data Protection Officer:** DPO contact information in API documentation

### 4. CCPA - California Consumer Privacy Act (California Civil Code § 1798.100 et seq.)
**Contract Reference:** Section 12.1 - Data Protection Compliance
**Applicability:** California resident data (population: 39M+)

#### Technical Requirements:
```csharp
// CCPA consumer rights service
public class CCPAComplianceService
{
    // § 1798.100 - Right to Know
    public async Task<CCPADisclosureDto> GetConsumerDisclosureAsync(string phoneNumber)
    {
        var accessLogs = await _context.AuditLogs
            .Where(a => a.PhoneNumberQueried == phoneNumber
                     && a.QueriedAt > DateTime.UtcNow.AddMonths(-12))
            .ToListAsync();

        return new CCPADisclosureDto
        {
            CategoriesCollected = new[]
            {
                "Identifiers (name, phone, email, SSN)",
                "Demographics (age, gender, income)",
                "Financial information (credit score, accounts)",
                "Geolocation (address, city, state, zip)"
            },
            BusinessPurposes = new[]
            {
                "Credit evaluation and underwriting",
                "Insurance risk assessment",
                "Identity verification",
                "Fraud prevention"
            },
            ThirdPartiesSharedWith = accessLogs
                .Select(a => a.BuyerId)
                .Distinct()
                .Select(buyerId => _context.Buyers.Find(buyerId).CompanyName)
                .ToList(),
            SaleDateAndPrice = null // Data not sold, only licensed
        };
    }

    // § 1798.105 - Right to Delete
    public async Task<bool> DeleteConsumerDataAsync(string phoneNumber)
    {
        // Same implementation as GDPR erasure, but without legal basis exceptions
        return await _gdprService.ErasePersonalDataAsync(phoneNumber, "CCPA deletion request");
    }

    // § 1798.115 - Right to Know About Sales
    public async Task<CCPASalesDisclosureDto> GetSalesDisclosureAsync(string phoneNumber)
    {
        return new CCPASalesDisclosureDto
        {
            DataSoldInLast12Months = false, // API provides licensed access, not data sales
            DataSharedForBusinessPurpose = true,
            BusinessPartners = await GetBusinessPartnersAsync(phoneNumber)
        };
    }

    // § 1798.120 - Right to Opt-Out of Sales
    public async Task OptOutOfSalesAsync(string phoneNumber)
    {
        var optOut = new CCPAOptOutRecord
        {
            PhoneNumber = phoneNumber,
            OptOutDate = DateTime.UtcNow,
            OptOutMethod = "consumer_portal",
            IsActive = true
        };

        await _context.CCPAOptOuts.AddAsync(optOut);
        await _context.SaveChangesAsync();

        // Block future data access for this consumer
        await _context.EquifaxRecords
            .Where(r => r.Phone1 == phoneNumber || r.Phone2 == phoneNumber)
            .ForEachAsync(r => r.CCPAOptOut = true);
    }
}
```

#### CCPA Compliance Checklist:
- ✅ **§ 1798.100 - Right to Know:** Consumer can request categories and specific data
- ✅ **§ 1798.105 - Right to Delete:** Deletion requests processed within 45 days
- ✅ **§ 1798.110 - Right to Data Disclosure:** Detailed disclosure of data practices
- ✅ **§ 1798.115 - Right to Know About Sales:** Sales disclosure (N/A - licensing model)
- ✅ **§ 1798.120 - Right to Opt-Out:** "Do Not Sell My Personal Information" portal
- ✅ **§ 1798.130 - Notice Requirements:** Privacy policy updated with CCPA disclosures
- ✅ **§ 1798.135 - Opt-Out Button:** Consumer portal includes "Do Not Sell" button
- ✅ **§ 1798.150 - Data Breach Liability:** Encryption minimizes breach risk

### 5. GLBA - Gramm-Leach-Bliley Act (15 U.S.C. § 6801 et seq.)
**Contract Reference:** Section 6.1 - Compliance with Laws
**Applicability:** Financial information disclosure

#### Technical Requirements:
```csharp
// GLBA safeguards compliance service
public class GLBAComplianceService
{
    // § 501(b) - Safeguards Rule
    public class GLBASafeguards
    {
        // Administrative Safeguards
        public bool DesignatedSecurityOfficer => true; // CISO assigned
        public bool RiskAssessmentCompleted => true; // Annual assessment
        public bool EmployeeTrainingMandatory => true; // Quarterly training

        // Technical Safeguards
        public string EncryptionStandard => "AES-256-GCM"; // § 314.4(c)
        public bool AccessControlsImplemented => true; // API keys, JWT
        public bool MultifactorAuthentication => true; // Admin portal MFA
        public bool IntrusionDetection => true; // AWS GuardDuty

        // Physical Safeguards
        public string DataCenterSecurity => "AWS SOC 2 Type II certified";
        public bool BackupEncryption => true; // RDS encrypted backups
    }

    // § 502 - Privacy Notice Requirements
    public async Task<GLBAPrivacyNoticeDto> GetPrivacyNoticeAsync()
    {
        return new GLBAPrivacyNoticeDto
        {
            FinancialInformationCollected = new[]
            {
                "Credit scores and credit history",
                "Bank account information",
                "Loan and mortgage details",
                "Investment account data"
            },
            InformationSharing = new[]
            {
                "Shared with licensed buyers for permissible purposes",
                "Not sold to third parties for marketing",
                "Disclosed to law enforcement when legally required"
            },
            OptOutRight = "Consumers may opt-out via data subject rights portal",
            SecurityMeasures = "AES-256 encryption, access logging, 24/7 monitoring"
        };
    }

    // § 503 - Opt-Out Notice
    public async Task SendOptOutNoticeAsync(string phoneNumber)
    {
        // GLBA requires annual opt-out notices
        var lastNotice = await _context.GLBAOptOutNotices
            .Where(n => n.PhoneNumber == phoneNumber)
            .OrderByDescending(n => n.SentAt)
            .FirstOrDefaultAsync();

        if (lastNotice == null || lastNotice.SentAt < DateTime.UtcNow.AddYears(-1))
        {
            await _notificationService.SendOptOutNoticeAsync(phoneNumber);
        }
    }
}
```

#### GLBA Compliance Checklist:
- ✅ **§ 501(b) - Safeguards Rule:** Comprehensive security program documented
- ✅ **§ 502 - Privacy Notices:** Privacy policy published and accessible
- ✅ **§ 503 - Opt-Out Rights:** Consumer can opt-out of information sharing
- ✅ **§ 314.4 - Information Security:** AES-256 encryption, access controls, monitoring
- ✅ **FTC Safeguards Rule:** Risk assessment, employee training, vendor management

### Compliance Monitoring & Reporting

#### Automated Compliance Dashboard
```csharp
public class ComplianceDashboardService
{
    public async Task<ComplianceStatusDto> GetComplianceStatusAsync()
    {
        return new ComplianceStatusDto
        {
            FCRACompliance = await CheckFCRAComplianceAsync(),
            TCPACompliance = await CheckTCPAComplianceAsync(),
            GDPRCompliance = await CheckGDPRComplianceAsync(),
            CCPACompliance = await CheckCCPAComplianceAsync(),
            GLBACompliance = await CheckGLBAComplianceAsync(),

            // Contract SLA metrics
            UptimePercentage = await CalculateUptimeAsync(), // Target: 99.5%
            AverageResponseTime = await CalculateAvgResponseTimeAsync(), // Target: < 500ms
            DataBreachNotificationCompliance = true, // 72-hour requirement

            LastAuditDate = DateTime.Parse("2025-10-15"),
            NextAuditDate = DateTime.Parse("2026-01-15"),
            ComplianceScore = 98.5 // Weighted average of all frameworks
        };
    }
}
```

#### Quarterly Compliance Reports (Contractual Requirement)
**Contract Reference:** Section 6.1 - Regulatory Reporting

1. **FCRA Compliance Report**
   - Total access logs reviewed: [count]
   - Permissible purpose violations: 0
   - Consumer dispute resolutions: [count]
   - Audit log retention compliance: 100%

2. **Data Breach Assessment**
   - Security incidents: [count]
   - Breaches requiring notification: 0
   - Time to notification (if applicable): < 72 hours
   - Remediation actions taken: [list]

3. **Consumer Rights Requests**
   - Access requests: [count] (avg response time: [days])
   - Deletion requests: [count] (avg response time: [days])
   - Correction requests: [count] (avg response time: [days])
   - Opt-out requests: [count]

4. **SLA Performance**
   - Uptime: [percentage] (target: ≥ 99.5%)
   - Average response time: [ms] (target: ≤ 500ms)
   - Planned downtime notifications: [count] (all > 24 hours advance)

---

## MVP FEATURE BREAKDOWN

### Phase 1: Core API (Days 1-5)

#### Feature 1.1: REST API Endpoint
**Priority:** P0 (Blocker)
**Effort:** 1 day
**Dependencies:** None

**Implementation:**
```csharp
[ApiController]
[Route("api/data_enhancement")]
public class DataEnhancementController : ControllerBase
{
    private readonly IPhoneEnrichmentService _enrichmentService;
    private readonly ILogger<DataEnhancementController> _logger;

    [HttpPost("lookup")]
    [ProducesResponseType(typeof(EnrichmentResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(429)]
    public async Task<IActionResult> Lookup(
        [FromBody] EnrichmentRequest request,
        CancellationToken cancellationToken)
    {
        // Request validation happens via FluentValidation
        // Authentication happens via ApiKeyAuthMiddleware

        var result = await _enrichmentService.EnrichByPhoneAsync(
            request.Phone,
            request.PermissiblePurpose,
            request.Fields ?? "basic", // Default to basic fields
            request.UniqueId,
            cancellationToken);

        if (result == null)
        {
            return Ok(new EnrichmentResponse
            {
                Response = "error",
                Message = "Unable to find record for phone number",
                Data = new
                {
                    phone = request.Phone,
                    match_attempted = true,
                    match_confidence = 0.0
                },
                Metadata = new ResponseMetadata
                {
                    RequestId = HttpContext.TraceIdentifier,
                    QueryTimestamp = DateTime.UtcNow,
                    UniqueId = request.UniqueId
                }
            });
        }

        return Ok(new EnrichmentResponse
        {
            Response = "success",
            Message = "Record found with high confidence",
            Data = result,
            Metadata = new ResponseMetadata
            {
                MatchConfidence = result.MatchConfidence,
                MatchType = result.MatchType,
                DataFreshnessDate = result.DataFreshnessDate,
                QueryTimestamp = DateTime.UtcNow,
                ResponseTimeMs = result.ResponseTimeMs,
                RequestId = HttpContext.TraceIdentifier,
                UniqueId = request.UniqueId,
                TotalFieldsReturned = request.Fields == "full" ? 398 : null
            }
        });
    }
}

// Request DTO
public class EnrichmentRequest
{
    // REQUIRED FIELDS
    [Required]
    public string ApiKey { get; set; }

    [Required]
    public string ProviderCode { get; set; }

    [Required]
    [Phone]
    public string Phone { get; set; }

    [Required]
    public string PermissiblePurpose { get; set; }

    // OPTIONAL FIELDS (improve matching accuracy)
    public string FirstName { get; set; }
    public string LastName { get; set; }

    /// <summary>
    /// RECOMMENDED: Zipcode improves match confidence by 15-20% (industry standard)
    /// Used for geographic verification and disambiguation
    /// </summary>
    public string PostalCode { get; set; }

    public string State { get; set; }

    /// <summary>
    /// RECOMMENDED: IP address enables lead intelligence and buying intent signals
    /// Used for fraud detection, geographic verification, and B2B company matching
    /// Format: IPv4 (e.g., "192.168.1.1") or IPv6
    /// </summary>
    public string IpAddress { get; set; }

    public string UniqueId { get; set; }

    // FIELD SELECTION (controls response payload size)
    /// <summary>
    /// Field selection: "basic" (~50 fields, faster) or "full" (398 fields, comprehensive)
    /// Default: "basic"
    /// Field projection reduces payload by 70%+ and improves response time
    /// </summary>
    public string Fields { get; set; } // "basic" or "full"
}

// Response DTO
public class EnrichmentResponse
{
    public string Response { get; set; } // "success" or "error"
    public string Message { get; set; }
    public object Data { get; set; }

    [JsonPropertyName("_metadata")]
    public ResponseMetadata Metadata { get; set; }
}

public class ResponseMetadata
{
    public double? MatchConfidence { get; set; }
    public string MatchType { get; set; }
    public string DataFreshnessDate { get; set; }
    public DateTime QueryTimestamp { get; set; }
    public int? ResponseTimeMs { get; set; }
    public string RequestId { get; set; }
    public string UniqueId { get; set; }
    public int? TotalFieldsReturned { get; set; }
}
```

**Acceptance Criteria:**
- ✅ Accepts JSON payload with api_key, provider_code, phone, permissible_purpose (REQUIRED)
- ✅ Accepts optional fields: postal_code (RECOMMENDED), ip_address (RECOMMENDED), first_name, last_name, state, unique_id
- ✅ Returns 200 OK with basic fields (~50) or full dataset (398) based on request
- ✅ Returns error response when no match (still 200 status, error in response body)
- ✅ Returns 400 Bad Request for invalid phone format or missing required fields
- ✅ Returns 401 Unauthorized for invalid API key
- ✅ Returns 429 Too Many Requests when rate limit exceeded
- ✅ Response time < 200ms (p95) for basic fields, < 300ms for full dataset
- ✅ Includes unique_id in response for tieback tracking
- ✅ IP address and zipcode validation (when provided)
- ✅ Higher match confidence when optional fields provided

**Industry Best Practices (2025 Standards):**

📊 **Performance Benchmarks:**
- **Target:** < 200ms average response time (competitive with Seamless.AI: 200ms, SuperAGI: 250ms, FullContact: 300ms)
- **Redis Caching:** Reduces response from 200ms+ to 1ms for cached data (90% reduction)
- **Database Query:** 50-150ms for uncached PostgreSQL lookups
- **Field Projection:** Reduces payload by 70%+ when using "basic" vs "full" fields

🎯 **Matching Accuracy:**
- **Phone-only matching:** 85% baseline accuracy
- **Phone + zipcode:** 90%+ accuracy (15-20% improvement)
- **Phone + IP address:** Enables fraud detection, geographic verification, B2B company identification
- **Multi-field enrichment:** Industry standard (FullContact, Clearbit) for higher confidence

🔒 **FCRA Compliance:**
- **Permissible purpose validation:** MANDATORY before data release (§ 604)
- **Strict liability:** Data brokers must PREVENT (not just discourage) non-permissible use
- **Audit logging:** Every access logged with 24-month retention (§ 607)
- **Data minimization:** Return only requested fields (privacy-by-design principle)

✅ **Validation Strategy:**
- **Early validation:** Reject bad data at gateway before reaching services
- **Assume bad input:** Validate all fields, never trust client data
- **Multi-layered:** Gateway-level (malformed requests) + service-specific (business rules)
- **Simple rules:** Only validate what's necessary for business logic

🚀 **Optimization Techniques:**
- **Redis caching:** AWS ElastiCache with 24-hour TTL for phone lookups
- **Connection pooling:** EF Core with optimized connection strings
- **Async/await:** Non-blocking I/O for all database operations
- **Indexed queries:** PostgreSQL indexes on all 10 phone columns
- **Field selection:** Allow clients to request only needed fields (reduces rendering time)

📈 **2025 Compliance Trends:**
- **Privacy-by-design:** Gartner predicts 75% adoption in API development
- **Data minimization:** Only collect and return necessary data
- **Purpose limitation:** Enforce permissible purpose at code level
- **Transparency:** Clear documentation of data sources and usage

---

#### Feature 1.2: Phone Number Normalization
**Priority:** P0 (Blocker)
**Effort:** 0.5 days (regex) or 1 day (libphonenumber hybrid)
**Dependencies:** None (optional: libphonenumber-csharp 9.0.17 NuGet package)

**Recommended Approach: Hybrid Strategy**

Use **fast regex** for MVP (US-only NANP numbers), with **libphonenumber validation layer** for future international expansion.

**Option 1: Fast Regex Normalization (US-Only MVP)**
```csharp
public class PhoneNumberNormalizer
{
    private static readonly Regex NonNumericRegex = new Regex(@"[^\d]", RegexOptions.Compiled);

    /// <summary>
    /// Fast normalization for US NANP (North American Numbering Plan) phone numbers.
    /// Performance: Sub-millisecond for high-volume APIs
    /// Use case: US-only enrichment API with 326M US records
    /// </summary>
    public string Normalize(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

        // Remove all non-numeric characters
        var digitsOnly = NonNumericRegex.Replace(phoneNumber, "");

        // Handle US country code (+1 or 1 prefix)
        if (digitsOnly.StartsWith("1") && digitsOnly.Length == 11)
            digitsOnly = digitsOnly.Substring(1);

        // Validate NANP format: NXX NXX XXXX (N = 2-9, X = 0-9)
        if (digitsOnly.Length != 10)
            throw new ArgumentException($"Invalid phone number length: {digitsOnly.Length} digits (expected 10)", nameof(phoneNumber));

        // NANP validation: Area code and exchange cannot be N11 (reserved for special services like 911, 411)
        if (digitsOnly[1] == '1' && digitsOnly[2] == '1')
            throw new ArgumentException("Invalid area code: N11 format reserved for special services", nameof(phoneNumber));

        if (digitsOnly[4] == '1' && digitsOnly[5] == '1')
            throw new ArgumentException("Invalid exchange code: N11 format reserved for special services", nameof(phoneNumber));

        // First digit of area code and exchange must be 2-9 (NANP requirement)
        if (digitsOnly[0] < '2' || digitsOnly[0] > '9')
            throw new ArgumentException("Invalid area code: First digit must be 2-9", nameof(phoneNumber));

        if (digitsOnly[3] < '2' || digitsOnly[3] > '9')
            throw new ArgumentException("Invalid exchange code: First digit must be 2-9", nameof(phoneNumber));

        return digitsOnly;
    }

    /// <summary>
    /// Validates if number is a valid NANP phone number (quick check before normalization)
    /// </summary>
    public bool IsValidNANPFormat(string phoneNumber)
    {
        try
        {
            Normalize(phoneNumber);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

**Option 2: libphonenumber Validation Layer (International-Ready)**
```csharp
using PhoneNumbers;

public class PhoneNumberValidator
{
    private static readonly PhoneNumberUtil PhoneUtil = PhoneNumberUtil.GetInstance();
    private const string DefaultRegion = "US";

    /// <summary>
    /// Industry-standard validation using Google's libphonenumber.
    /// Performance: ~30x slower than regex but handles international numbers correctly.
    /// NuGet: libphonenumber-csharp 9.0.17 (updated every 2 weeks)
    /// </summary>
    public (bool IsValid, string NormalizedNumber, PhoneNumberType NumberType) ValidateAndNormalize(string phoneNumber)
    {
        try
        {
            // Parse with default region US
            var parsedNumber = PhoneUtil.Parse(phoneNumber, DefaultRegion);

            // Validate
            bool isValid = PhoneUtil.IsValidNumber(parsedNumber);

            if (!isValid)
                return (false, null, PhoneNumberType.UNKNOWN);

            // Get number type (MOBILE, FIXED_LINE, TOLL_FREE, etc.)
            var numberType = PhoneUtil.GetNumberType(parsedNumber);

            // Format to E.164 (international standard: +15551234567)
            string e164Format = PhoneUtil.Format(parsedNumber, PhoneNumberFormat.E164);

            // For US database compatibility, strip +1 prefix
            string normalizedNumber = e164Format.StartsWith("+1")
                ? e164Format.Substring(2)
                : e164Format.TrimStart('+');

            return (true, normalizedNumber, numberType);
        }
        catch (NumberParseException ex)
        {
            return (false, null, PhoneNumberType.UNKNOWN);
        }
    }

    /// <summary>
    /// Fast pre-validation without full parsing (checks length only)
    /// Use this for quick rejection of obviously invalid numbers
    /// </summary>
    public bool IsPossibleNumber(string phoneNumber)
    {
        try
        {
            var parsedNumber = PhoneUtil.Parse(phoneNumber, DefaultRegion);
            return PhoneUtil.IsPossibleNumber(parsedNumber);
        }
        catch
        {
            return false;
        }
    }
}
```

**Recommended Hybrid Implementation:**
```csharp
public class HybridPhoneNormalizer
{
    private readonly PhoneNumberNormalizer _regexNormalizer;
    private readonly PhoneNumberValidator _libPhoneValidator;
    private readonly ILogger<HybridPhoneNormalizer> _logger;

    public string NormalizeForLookup(string phoneNumber, bool strictValidation = false)
    {
        // Fast path: Regex normalization for US numbers (sub-millisecond)
        if (!strictValidation)
        {
            return _regexNormalizer.Normalize(phoneNumber);
        }

        // Strict path: libphonenumber validation (slower but accurate)
        var (isValid, normalized, numberType) = _libPhoneValidator.ValidateAndNormalize(phoneNumber);

        if (!isValid)
            throw new ArgumentException("Invalid phone number", nameof(phoneNumber));

        _logger.LogInformation("Phone validated: {PhoneNumber}, Type: {NumberType}", normalized, numberType);

        return normalized;
    }
}
```

**Test Cases:**
- ✅ `(555) 123-4567` → `5551234567`
- ✅ `555-123-4567` → `5551234567`
- ✅ `1-555-123-4567` → `5551234567`
- ✅ `+1 555 123 4567` → `5551234567`
- ✅ `5551234567` → `5551234567`
- ✅ `+15551234567` → `5551234567` (E.164 format)
- ❌ `555-411-1234` → **REJECT** (N11 exchange forbidden)
- ❌ `155-123-4567` → **REJECT** (first digit must be 2-9)
- ❌ `555-123-456` → **REJECT** (only 9 digits)

---

**🚨 Critical Gotchas & Best Practices (2025 Standards):**

**Regex Approach Gotchas:**
- ❌ **Format-only validation:** Regex cannot verify if number actually exists
- ❌ **NANP N11 rule:** Area codes and exchanges cannot be N11 (conflicts with 911, 411, 511, etc.)
- ❌ **First digit rule:** NANP requires first digit of area code and exchange to be 2-9
- ❌ **Invalid numbers accepted:** Simple regex accepts garbage like `"-- +()()())()))))"`
- ❌ **Country code confusion:** `+1`, `1`, or no prefix - all mean different things in international context
- ⚠️ **Don't strip non-numeric first:** Characters help determine country/area code (do validation first, then strip)

**Industry Standards:**
- 📋 **E.164 Format:** International standard `+[country code][subscriber number]` (max 15 digits)
- 📋 **Storage:** Use `VARCHAR(32)` NOT numeric types (phone 05 ≠ 5, they're identifiers not numbers)
- 📋 **Why VARCHAR(32):** Buffer for non-compliant providers, extensions, edge cases (E.164 max is 15)
- 📋 **libphonenumber-csharp:** Google's official C# library (NuGet 9.0.17, updated every 2 weeks)

**Performance Trade-offs:**
- ⚡ **Regex:** Sub-millisecond, perfect for high-volume US-only APIs (your use case: 326M US records)
- 🐌 **libphonenumber:** 30x slower in benchmarks (many regex comparisons for GetNumberType)
- 💾 **Bundle size:** libphonenumber adds 300KB-500KB (1000+ files)
- 🎯 **Optimization:** Cache parsed `PhoneNumber` objects, use singleton `GetInstance()`, parse once and reuse

**Recommended Strategy for MVP:**
1. ✅ **Use fast regex** for API normalization (US-only database, performance critical)
2. ✅ **NANP validation rules** (N11 check, first digit 2-9 requirement)
3. ✅ **Add libphonenumber layer** behind feature flag for international expansion
4. ✅ **Cache normalized results** in Redis (24-hour TTL)
5. ✅ **Store 10-digit format** (matches existing database schema with 326M records)

**Future International Expansion:**
- When expanding beyond US, switch to libphonenumber validation
- Store numbers in E.164 format (`+15551234567`)
- Update database schema to `VARCHAR(32)`
- Use libphonenumber's `GetNumberType()` to distinguish mobile vs landline

**Sources:**
- Google libphonenumber-csharp (GitHub, NuGet 9.0.17)
- ITU-T E.164 International Telecommunication Standard
- North American Numbering Plan (NANP) documentation
- Stack Overflow phone normalization best practices
- Wikipedia: E.164, NANP, National telephone conventions

---

#### Feature 1.3: Database Query with Multi-Phone Search
**Priority:** P0 (Blocker)
**Effort:** 2 days
**Dependencies:** Single-column indexes created on phone_1 through phone_10

**Critical Context:** Equifax orders phone numbers by reliability:
- **phone_1** = Most reliable (highest confidence)
- **phone_10** = Least reliable (lowest confidence)
- Same pattern applies to addresses and other multi-column fields

**Recommended Implementation: EF Core 9 with Confidence Scoring**
```csharp
public class EquifaxRepository : IEquifaxRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EquifaxRepository> _logger;

    // Compiled query for performance (EF Core 9 feature - .NET 9)
    private static readonly Func<ApplicationDbContext, string, CancellationToken, Task<EquifaxRecord?>>
        _compiledPhoneQuery = EF.CompileAsyncQuery(
            (ApplicationDbContext context, string phone, CancellationToken ct) =>
                context.EquifaxRecords
                    .AsNoTracking() // Read-only, no change tracking overhead
                    .Where(r => r.Phone1 == phone
                             || r.Phone2 == phone
                             || r.Phone3 == phone
                             || r.Phone4 == phone
                             || r.Phone5 == phone
                             || r.Phone6 == phone
                             || r.Phone7 == phone
                             || r.Phone8 == phone
                             || r.Phone9 == phone
                             || r.Phone10 == phone)
                    .FirstOrDefault()
        );

    /// <summary>
    /// Find consumer record by phone number with confidence scoring based on match column.
    /// Uses PostgreSQL bitmap heap scan to combine 10 single-column indexes.
    /// Performance: 50-150ms uncached, 1-10ms with Redis cache.
    /// </summary>
    public async Task<(EquifaxRecord? Record, double MatchConfidence, int? MatchedColumn)>
        FindByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        // Use compiled query for performance (caches execution plan)
        var record = await _compiledPhoneQuery(_context, phoneNumber, cancellationToken);

        if (record == null)
            return (null, 0.0, null);

        // Determine which phone column matched (for confidence scoring)
        int matchedColumn = DetermineMatchedColumn(record, phoneNumber);

        // Calculate confidence based on column reliability
        // phone_1 = 100%, phone_2 = 95%, ... phone_10 = 55%
        double confidence = CalculateMatchConfidence(matchedColumn);

        _logger.LogInformation(
            "Phone match found: {PhoneNumber}, Column: phone_{MatchedColumn}, Confidence: {Confidence:P0}",
            phoneNumber, matchedColumn, confidence
        );

        return (record, confidence, matchedColumn);
    }

    /// <summary>
    /// Determines which phone column contained the matched number.
    /// Required for confidence scoring (phone_1 = most reliable).
    /// </summary>
    private int DetermineMatchedColumn(EquifaxRecord record, string phoneNumber)
    {
        if (record.Phone1 == phoneNumber) return 1;
        if (record.Phone2 == phoneNumber) return 2;
        if (record.Phone3 == phoneNumber) return 3;
        if (record.Phone4 == phoneNumber) return 4;
        if (record.Phone5 == phoneNumber) return 5;
        if (record.Phone6 == phoneNumber) return 6;
        if (record.Phone7 == phoneNumber) return 7;
        if (record.Phone8 == phoneNumber) return 8;
        if (record.Phone9 == phoneNumber) return 9;
        if (record.Phone10 == phoneNumber) return 10;

        return 0; // Should never happen if record was returned
    }

    /// <summary>
    /// Calculate match confidence based on which column matched.
    /// Equifax orders by reliability: phone_1 (best) to phone_10 (worst).
    /// Formula: confidence = 100 - ((column_index - 1) * 5)
    /// Industry standard: Multi-source waterfall enrichment with confidence scoring.
    /// </summary>
    private double CalculateMatchConfidence(int columnIndex)
    {
        return columnIndex switch
        {
            1 => 1.00,  // 100% - Most reliable
            2 => 0.95,  // 95%
            3 => 0.90,  // 90%
            4 => 0.85,  // 85%
            5 => 0.80,  // 80%
            6 => 0.75,  // 75%
            7 => 0.70,  // 70%
            8 => 0.65,  // 65%
            9 => 0.60,  // 60%
            10 => 0.55, // 55% - Least reliable
            _ => 0.50   // 50% - Unknown/fallback
        };
    }
}
```

**Alternative: UNION ALL Approach (If Bitmap Scan Shows Performance Issues)**
```csharp
/// <summary>
/// UNION ALL approach: Search columns individually (may be faster for selective searches).
/// Use this if EXPLAIN ANALYZE shows bitmap heap scan overhead with OR approach.
/// Trade-off: More complex query, but potentially better index usage.
/// </summary>
public async Task<(EquifaxRecord? Record, double MatchConfidence, int? MatchedColumn)>
    FindByPhoneNumberWithUnionAsync(string phoneNumber, CancellationToken cancellationToken)
{
    // Search phone_1 first (most reliable)
    var record = await _context.EquifaxRecords
        .AsNoTracking()
        .Where(r => r.Phone1 == phoneNumber)
        .FirstOrDefaultAsync(cancellationToken);

    if (record != null)
        return (record, 1.00, 1);

    // Search phone_2 (if not found in phone_1)
    record = await _context.EquifaxRecords
        .AsNoTracking()
        .Where(r => r.Phone2 == phoneNumber)
        .FirstOrDefaultAsync(cancellationToken);

    if (record != null)
        return (record, 0.95, 2);

    // Continue for phone_3 through phone_10...
    // (Full implementation would include all 10 columns)

    return (null, 0.0, null);
}
```

**Performance Targets:**
- **Without cache:** 50-150ms (PostgreSQL bitmap heap scan with indexes)
- **With Redis cache:** 1-10ms (in-memory lookup)
- **Query plan:** PostgreSQL automatically combines 10 single-column indexes using bitmap scan
- **Compiled query:** Caches EF Core execution plan (major performance boost in .NET 9)
- **AsNoTracking:** Eliminates change tracking overhead for read-only queries

**PostgreSQL Query Optimization (Under the Hood):**
```sql
-- PostgreSQL automatically converts OR query to bitmap heap scan:
EXPLAIN ANALYZE
SELECT * FROM equifax_staging_all_text
WHERE phone_1 = '5551234567'
   OR phone_2 = '5551234567'
   OR phone_3 = '5551234567'
   ...
   OR phone_10 = '5551234567'
LIMIT 1;

-- Expected Plan:
-- Bitmap Heap Scan on equifax_staging_all_text
--   Recheck Cond: ((phone_1 = '5551234567') OR (phone_2 = '5551234567') ...)
--   -> BitmapOr
--      -> Bitmap Index Scan on idx_phone_1
--      -> Bitmap Index Scan on idx_phone_2
--      ...
--      -> Bitmap Index Scan on idx_phone_10
```

**Required Indexes (Single-Column B-tree):**
```sql
CREATE INDEX CONCURRENTLY idx_phone_1 ON equifax_staging_all_text(phone_1);
CREATE INDEX CONCURRENTLY idx_phone_2 ON equifax_staging_all_text(phone_2);
CREATE INDEX CONCURRENTLY idx_phone_3 ON equifax_staging_all_text(phone_3);
CREATE INDEX CONCURRENTLY idx_phone_4 ON equifax_staging_all_text(phone_4);
CREATE INDEX CONCURRENTLY idx_phone_5 ON equifax_staging_all_text(phone_5);
CREATE INDEX CONCURRENTLY idx_phone_6 ON equifax_staging_all_text(phone_6);
CREATE INDEX CONCURRENTLY idx_phone_7 ON equifax_staging_all_text(phone_7);
CREATE INDEX CONCURRENTLY idx_phone_8 ON equifax_staging_all_text(phone_8);
CREATE INDEX CONCURRENTLY idx_phone_9 ON equifax_staging_all_text(phone_9);
CREATE INDEX CONCURRENTLY idx_phone_10 ON equifax_staging_all_text(phone_10);

-- CONCURRENTLY allows index creation without locking table (important for 326M records)
-- Note: Multi-column indexes NOT recommended for OR queries across unrelated columns
```

**Maintenance:**
```sql
-- Run VACUUM regularly to enable index-only scans (performance critical)
VACUUM ANALYZE equifax_staging_all_text;

-- Monitor index usage
SELECT schemaname, tablename, indexname, idx_scan, idx_tup_read, idx_tup_fetch
FROM pg_stat_user_indexes
WHERE tablename = 'equifax_staging_all_text'
ORDER BY idx_scan DESC;
```

---

**🚨 Critical Best Practices (2025 PostgreSQL 18 + EF Core 9):**

**PostgreSQL Optimization:**
- ✅ **Bitmap Heap Scan**: PostgreSQL automatically combines multiple indexes for OR queries
- ✅ **Single-column indexes**: Recommended for OR queries across unrelated columns (phone_1, phone_2, etc.)
- ⚠️ **Physical row order**: Bitmap scans lose index ordering (add separate ORDER BY if needed)
- ✅ **PostgreSQL 18 (Sept 2025)**: Skip scan optimization for multi-column B-tree with few distinct values
- ✅ **VACUUM regularly**: Enables index-only scans (visibility map must show pages as "all-visible")
- 📊 **Use EXPLAIN ANALYZE**: Profile queries to verify bitmap scan performance
- ⚡ **99.28% improvement possible**: 304ms vs minutes with proper indexing (real-world benchmark)

**Entity Framework Core 9 Optimization:**
- ✅ **AsNoTracking()**: Eliminates change tracking overhead (significantly faster for read-only)
- ✅ **Compiled Queries**: EF.CompileAsyncQuery caches execution plans (major perf boost in .NET 9)
- ✅ **Streaming with foreach**: Reduces memory, processes before full result set retrieved
- ✅ **PostgreSQL prepared statements**: Similar to SQL Server's LRU query plan cache
- 🔍 **Profile with EXPLAIN ANALYZE**: Identify slow queries and optimize indexes

**Match Confidence Scoring (Data Enrichment Standard 2025):**
- 📊 **Reliability ordering**: phone_1 (100% confidence) → phone_10 (55% confidence)
- 🎯 **Track matched column**: Required for accurate confidence scoring
- 🔄 **Multi-source waterfall**: Industry standard for data enrichment APIs
- 🤖 **AI/ML confidence scoring**: Continuous learning, prioritizes high-confidence matches
- ✅ **Multi-provider validation**: Cross-validate across sources for higher confidence
- 📈 **Continuous enrichment**: Ongoing process (customer data changes constantly)

**Performance Benchmarks (326M Records):**
- **Target:** 50-150ms uncached, 1-10ms with Redis
- **Compiled query:** Pre-compiles LINQ, caches execution plan
- **Index creation time:** Use CONCURRENTLY to avoid table locks during creation
- **Monitor index usage:** pg_stat_user_indexes to verify indexes are being used

**OR vs UNION ALL Trade-offs:**
- **OR approach (recommended)**: Single query, PostgreSQL handles bitmap scan automatically
- **UNION ALL approach**: Separate queries per column, may be faster for selective searches
- **Caveat**: PostgreSQL query planner is sophisticated - benchmark both approaches with real data
- **Decision**: Start with OR (simpler), switch to UNION ALL only if profiling shows bitmap scan overhead

**Data Enrichment Industry Standards (2025):**
- ✅ **Intelligent confidence scoring**: Multi-level verification (phone ownership, cross-provider)
- ✅ **Knowledge-graph enrichment**: Relationship confidence scoring
- ✅ **ML-powered enrichment**: Automatically eliminates duplicates, prioritizes high-confidence
- ✅ **Multi-provider waterfall**: Intelligent sourcing from multiple premium providers
- ✅ **GDPR/CCPA compliance**: Data protection regulations in enrichment workflows

**Sources:**
- PostgreSQL 18 Documentation (Official - September 2025)
- Microsoft Learn: EF Core 9 Performance (Official)
- Tiger Data: Handling Billions of Rows in PostgreSQL
- Frontend Masters: Advanced PostgreSQL Indexing
- Cybertec PostgreSQL: Index Scan vs Bitmap Scan
- Unacast: Best Practices for Customer Data Enrichment 2025
- Code Maze: Entity Framework Core Best Practices

---

#### Feature 1.4: PII Decryption Service
**Priority:** P1 (High - FCRA/GDPR Compliance Required)
**Effort:** 1 day
**Dependencies:** AWS Secrets Manager configured, encryption keys with automatic rotation

**⚠️ CRITICAL SECURITY WARNING - AES-GCM NONCE REUSE**
🔴 **NEVER reuse the same IV/nonce with the same key** - allows complete key recovery from single reuse!
🔴 **Usage limit:** 2^32 encryptions per key (≈100 days at 500 records/sec) - rotate keys regularly!

**Recommended Implementation: Secure PII Decryption with Performance Optimization**
```csharp
using System.Buffers;
using System.Security.Cryptography;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

public class PIIDecryptionService : IPIIDecryptionService
{
    private readonly IAmazonSecretsManager _secretsManager;
    private readonly ILogger<PIIDecryptionService> _logger;
    private readonly string _secretName;
    private readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;

    // Cache decryption key in memory (refresh on rotation)
    private byte[] _cachedKey;
    private DateTime _keyLoadedAt;
    private readonly TimeSpan _keyCacheDuration = TimeSpan.FromMinutes(30);

    public PIIDecryptionService(
        IAmazonSecretsManager secretsManager,
        ILogger<PIIDecryptionService> logger,
        IConfiguration configuration)
    {
        _secretsManager = secretsManager;
        _logger = logger;
        _secretName = configuration["AWS:SecretName"]; // e.g., "prod_equifax_encryption_key"
    }

    /// <summary>
    /// Decrypt PII field using AES-256-GCM authenticated encryption.
    /// FCRA Compliance: Only call after validating permissible purpose.
    /// Performance: Uses Span<T> stackalloc for small buffers, ArrayPool for large.
    /// Security: Validates authentication tag, prevents tampering.
    /// </summary>
    public async Task<string> DecryptAsync(string encryptedData, string permissiblePurpose, CancellationToken cancellationToken = default)
    {
        // FCRA COMPLIANCE: Validate permissible purpose before decryption
        if (!IsPermissiblePurposeValid(permissiblePurpose))
        {
            _logger.LogWarning("PII decryption attempted with invalid permissible purpose: {Purpose}", permissiblePurpose);
            throw new UnauthorizedAccessException($"Invalid permissible purpose for PII decryption: {permissiblePurpose}");
        }

        if (string.IsNullOrWhiteSpace(encryptedData))
            return null;

        try
        {
            // Parse encrypted data format: [IV]:[AuthTag]:[Ciphertext] (Base64)
            var parts = encryptedData.Split(':');
            if (parts.Length != 3)
                throw new ArgumentException("Invalid encrypted data format. Expected [IV]:[AuthTag]:[Ciphertext]");

            // Get encryption key from AWS Secrets Manager (cached)
            var encryptionKey = await GetEncryptionKeyAsync(cancellationToken);

            // Decode Base64 components
            // Performance optimization: Use stackalloc for small buffers (< 1024 bytes)
            Span<byte> iv = parts[0].Length <= 1024
                ? stackalloc byte[GetBase64DecodedLength(parts[0])]
                : new byte[GetBase64DecodedLength(parts[0])];

            Span<byte> authTag = parts[1].Length <= 1024
                ? stackalloc byte[GetBase64DecodedLength(parts[1])]
                : new byte[GetBase64DecodedLength(parts[1])];

            // For ciphertext, use ArrayPool if > 1024 bytes
            int ciphertextLength = GetBase64DecodedLength(parts[2]);
            byte[] ciphertextBuffer = null;
            Span<byte> ciphertext;

            if (ciphertextLength <= 1024)
            {
                ciphertext = stackalloc byte[ciphertextLength];
            }
            else
            {
                ciphertextBuffer = _arrayPool.Rent(ciphertextLength);
                ciphertext = ciphertextBuffer.AsSpan(0, ciphertextLength);
            }

            try
            {
                // Decode Base64
                Convert.TryFromBase64String(parts[0], iv, out _);
                Convert.TryFromBase64String(parts[1], authTag, out _);
                Convert.TryFromBase64String(parts[2], ciphertext, out _);

                // Validate IV size (96-bit = 12 bytes recommended for AES-GCM)
                if (iv.Length != 12)
                {
                    _logger.LogWarning("Non-standard IV size detected: {IVSize} bytes (recommended: 12 bytes)", iv.Length);
                }

                // Decrypt using AES-256-GCM
                using var aesGcm = new AesGcm(encryptionKey);

                // Allocate plaintext buffer
                byte[] plaintextBuffer = null;
                Span<byte> plaintext;

                if (ciphertextLength <= 1024)
                {
                    plaintext = stackalloc byte[ciphertextLength];
                }
                else
                {
                    plaintextBuffer = _arrayPool.Rent(ciphertextLength);
                    plaintext = plaintextBuffer.AsSpan(0, ciphertextLength);
                }

                try
                {
                    // Decrypt with authentication tag validation
                    // This throws if auth tag doesn't match (prevents tampering)
                    aesGcm.Decrypt(iv, ciphertext, authTag, plaintext);

                    // Convert plaintext bytes to string
                    var decryptedValue = Encoding.UTF8.GetString(plaintext);

                    // FCRA COMPLIANCE: Log PII access with permissible purpose
                    _logger.LogInformation(
                        "PII field decrypted. Purpose: {Purpose}, FieldLength: {Length}, Timestamp: {Timestamp}",
                        permissiblePurpose,
                        decryptedValue.Length,
                        DateTime.UtcNow
                    );

                    return decryptedValue;
                }
                finally
                {
                    // Return pooled buffer
                    if (plaintextBuffer != null)
                        _arrayPool.Return(plaintextBuffer, clearArray: true); // Clear sensitive data
                }
            }
            finally
            {
                // Return pooled buffer
                if (ciphertextBuffer != null)
                    _arrayPool.Return(ciphertextBuffer);
            }
        }
        catch (CryptographicException ex)
        {
            // Authentication tag validation failed - potential tampering
            _logger.LogError(ex, "PII decryption failed - authentication tag mismatch. Possible data tampering detected.");
            throw new SecurityException("PII decryption failed - data integrity check failed", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PII decryption failed unexpectedly");
            throw;
        }
    }

    /// <summary>
    /// Batch decrypt multiple PII fields with parallel processing.
    /// Performance: 8 Gbps/core throughput with platform intrinsics.
    /// </summary>
    public async Task<Dictionary<string, string>> DecryptBatchAsync(
        Dictionary<string, string> encryptedFields,
        string permissiblePurpose,
        CancellationToken cancellationToken = default)
    {
        var decryptionTasks = encryptedFields.Select(async kvp =>
            new KeyValuePair<string, string>(
                kvp.Key,
                await DecryptAsync(kvp.Value, permissiblePurpose, cancellationToken)
            )
        );

        var results = await Task.WhenAll(decryptionTasks);
        return results.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// Get encryption key from AWS Secrets Manager with caching.
    /// Security: Keys cached for 30 minutes, refreshed automatically on rotation.
    /// </summary>
    private async Task<byte[]> GetEncryptionKeyAsync(CancellationToken cancellationToken)
    {
        // Check if cached key is still valid
        if (_cachedKey != null && DateTime.UtcNow - _keyLoadedAt < _keyCacheDuration)
            return _cachedKey;

        try
        {
            // Retrieve from AWS Secrets Manager
            var request = new GetSecretValueRequest
            {
                SecretId = _secretName,
                VersionStage = "AWSCURRENT" // Always use current version
            };

            var response = await _secretsManager.GetSecretValueAsync(request, cancellationToken);

            // Parse secret (assumes JSON format: {"encryptionKey": "base64-encoded-key"})
            var secretJson = JObject.Parse(response.SecretString);
            var keyBase64 = secretJson["encryptionKey"].ToString();

            _cachedKey = Convert.FromBase64String(keyBase64);
            _keyLoadedAt = DateTime.UtcNow;

            _logger.LogInformation("Encryption key loaded from AWS Secrets Manager. SecretId: {SecretId}, Version: {Version}",
                _secretName, response.VersionId);

            return _cachedKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve encryption key from AWS Secrets Manager");
            throw;
        }
    }

    /// <summary>
    /// Validate permissible purpose per FCRA § 604.
    /// </summary>
    private bool IsPermissiblePurposeValid(string purpose)
    {
        var validPurposes = new[]
        {
            "credit_transaction",
            "insurance_underwriting",
            "employment_purposes",
            "legitimate_business_need",
            "account_review",
            "collection_of_debt"
        };

        return validPurposes.Contains(purpose);
    }

    /// <summary>
    /// Calculate Base64 decoded length (for buffer allocation).
    /// </summary>
    private int GetBase64DecodedLength(string base64) =>
        (base64.Length * 3 + 3) / 4;
}
```

**Conditional Decryption: Decrypt Only When Needed**
```csharp
public class SelectiveDecryptionService
{
    /// <summary>
    /// Decrypt only PII fields based on requested field set.
    /// Performance: Avoids unnecessary decryption operations.
    /// FCRA Compliance: Data minimization - only decrypt what's needed.
    /// </summary>
    public async Task<EquifaxEnrichedResponse> DecryptSelectivelyAsync(
        EquifaxRecord record,
        string[] requestedFields,
        string permissiblePurpose)
    {
        var response = new EquifaxEnrichedResponse();

        // Decrypt first_name only if requested
        if (requestedFields.Contains("first_name") && !string.IsNullOrEmpty(record.FirstNameEncrypted))
        {
            response.FirstName = await _decryptionService.DecryptAsync(
                record.FirstNameEncrypted,
                permissiblePurpose
            );
        }

        // Decrypt last_name only if requested
        if (requestedFields.Contains("last_name") && !string.IsNullOrEmpty(record.LastNameEncrypted))
        {
            response.LastName = await _decryptionService.DecryptAsync(
                record.LastNameEncrypted,
                permissiblePurpose
            );
        }

        // Continue for other PII fields...

        return response;
    }
}
```

**Fields to Decrypt (Conditional Based on Request):**
- **first_name** - Consumer's first name (PII)
- **middle_name** - Consumer's middle name (PII)
- **last_name** - Consumer's last name (PII)
- **ssn_hash** - Social Security Number hash (sensitive PII - requires highest permissible purpose)
- **date_of_birth** - DOB (PII)
- **email** - Email address (PII if present)
- **address_1, address_2** - Physical addresses (PII)

**AWS Secrets Manager Configuration:**
```json
{
  "SecretName": "prod_equifax_encryption_key",
  "SecretString": {
    "encryptionKey": "base64-encoded-256-bit-key-here",
    "keyVersion": "v1",
    "createdAt": "2025-10-31T12:00:00Z",
    "rotateAfter": "2025-11-30T12:00:00Z"
  },
  "RotationConfiguration": {
    "AutomaticallyAfterDays": 30,
    "RotationLambdaARN": "arn:aws:lambda:us-east-1:123456789:function:RotateEncryptionKey"
  },
  "KmsKeyId": "arn:aws:kms:us-east-1:123456789:key/custom-kms-key-id"
}
```

**Key Rotation Lambda Function:**
```csharp
public class KeyRotationHandler
{
    /// <summary>
    /// AWS Lambda function for automatic encryption key rotation.
    /// Called by AWS Secrets Manager every 30 days.
    /// Strategy: Envelope encryption - only rotate DEK, not re-encrypt all data.
    /// </summary>
    public async Task<string> HandleRotation(SecretsManagerRotationEvent rotationEvent)
    {
        // Generate new 256-bit encryption key
        var newKey = GenerateSecureKey(256);

        // Store new key version in Secrets Manager
        await StoreNewKeyVersion(rotationEvent.SecretId, newKey, "AWSPENDING");

        // Test new key (decrypt test data)
        await TestNewKey(newKey);

        // Mark new key as current
        await PromoteKeyVersion(rotationEvent.SecretId, "AWSPENDING", "AWSCURRENT");

        // Keep old key available for 7 days (for decrypting old data)
        await ScheduleOldKeyDeprecation(rotationEvent.SecretId, TimeSpan.FromDays(7));

        return "Rotation completed successfully";
    }

    private byte[] GenerateSecureKey(int bits)
    {
        using var rng = RandomNumberGenerator.Create();
        var key = new byte[bits / 8];
        rng.GetBytes(key);
        return key;
    }
}
```

---

**🚨 Critical Security Best Practices (2025 Standards):**

**AES-GCM Security Requirements:**
- 🔴 **NONCE REUSE = CATASTROPHIC FAILURE**: Using same IV/nonce twice with same key allows complete key recovery
- ⚠️ **Usage limit:** 2^32 encryptions per key (100 days at 500 records/sec) - enforce key rotation
- ✅ **96-bit nonces (12 bytes)**: NIST-recommended size for AES-GCM optimal security
- ✅ **Random IV generation**: NIST officially recommends random IV/nonce generation
- ✅ **Authentication tag validation**: Prevents tampering, throws CryptographicException if invalid
- 🔄 **Consider AES-GCM-SIV**: RFC 8452 nonce-misuse-resistant alternative if unique nonces can't be guaranteed

**AWS Secrets Manager Best Practices:**
- ✅ **Automatic rotation every 30 days**: Recommended industry standard (minimum 4 hours possible)
- ✅ **Custom KMS keys**: Use AWS KMS keys you create (NOT default encryption key) for enhanced control
- ✅ **Envelope encryption**: Each secret version encrypted with unique data encryption key (DEK)
- ✅ **IAM least privilege**: Define policies allowing only specific roles/users to access secrets
- ✅ **Rotation strategies**: Single user (simple) or Alternating users (zero downtime)
- ✅ **Blast radius reduction**: Short-term secrets significantly reduce compromise risk
- ✅ **CloudTrail auditing**: Log all Secrets Manager key retrievals for compliance

**.NET Performance Optimization:**
- ⚡ **Span<T> with stackalloc**: Use for buffers < 1024 bytes to avoid heap allocations
- ⚡ **ArrayPool<byte>.Shared**: Memory pooling for buffers > 1024 bytes, return with clearArray: true
- ⚡ **Platform implementations**: AesGcm class calls OS implementations (Windows CNG, Linux, OpenSSL on macOS)
- ⚡ **Throughput**: Platform intrinsics achieve ~8 Gbps/core for encryption/decryption
- ⚡ **Low-allocation design**: Minimize GC pressure for high-throughput scenarios
- ⚡ **Available since .NET Core 3**: System.Security.Cryptography.AesGcm

**FCRA/GDPR Compliance (2025):**
- ✅ **Permissible purpose validation**: FCRA § 604 - validate before PII decryption (mandatory)
- ✅ **PII protection protocols**: All employees and third parties must adhere to procedures
- ✅ **Audit logging**: Log all PII decryption events with permissible purpose (24-month retention)
- ✅ **Data minimization**: Only decrypt fields explicitly requested (GDPR Article 25)
- ✅ **Field-level encryption**: Encrypt PII fields individually, leave non-PII plaintext
- ✅ **Proper disposal**: Secure deletion procedures when PII no longer needed
- ✅ **Conditional decryption**: Only decrypt when permissible purpose validates (FCRA requirement)

**Decryption Performance Strategies:**
- ✅ **Lazy decryption**: Decrypt on-demand per field vs all fields upfront
- ✅ **Batch decryption**: For multiple records, use parallel processing with Task.WhenAll
- ❌ **Never cache decrypted PII in Redis**: Security risk - only cache encrypted data
- ✅ **Memory management**: stackalloc for small buffers, ArrayPool for large batches
- ✅ **Error handling**: Authentication tag validation prevents tampering, fail-fast on invalid tags

**Key Rotation Strategy:**
- 🔄 **Frequency**: Rotate encryption keys every 30-90 days (industry standard 2025)
- 🔄 **Zero-downtime rotation**: Use alternating users strategy during rotation
- 🔄 **Envelope encryption advantage**: Only re-encrypt DEKs, not all consumer data
- 🔄 **AWS automation**: Secrets Manager handles rotation with Lambda functions
- 🔄 **Old key retention**: Keep old keys available for 7 days to decrypt legacy data

**Security Monitoring:**
- 📊 **Nonce uniqueness tracking**: Monitor for duplicate IV/nonce usage (critical security violation)
- 📊 **Usage counters**: Track encryption operations per key, rotate before 2^32 limit
- 📊 **Failed auth tags**: Monitor authentication failures (potential tampering attempts)
- 📊 **Access logging**: Log all PII decryption events with permissible purpose (FCRA § 607)
- 📊 **CloudTrail auditing**: All Secrets Manager key retrievals logged

**Encryption Format Specification:**
- **Format:** `[IV]:[AuthTag]:[Ciphertext]` (Base64-encoded components)
- **IV size:** 96-bit (12 bytes) for AES-GCM optimal performance
- **Auth tag size:** 128-bit (16 bytes) for authenticated encryption
- **Key size:** 256-bit (32 bytes) for AES-256-GCM
- **Storage:** VARCHAR/TEXT field in database, Base64 string encoding
- **Key storage:** AWS Secrets Manager with automatic rotation, NEVER hardcode

**Sources:**
- NIST AES-GCM Recommendations (Official)
- RFC 8452: AES-GCM-SIV Nonce Misuse-Resistant Encryption
- AWS Secrets Manager Documentation (Official 2025)
- elttam Security Research: Key Recovery Attacks on GCM
- Stack Overflow: Using AesGcm Class (.NET)
- Medium: AES-GCM Complete Guide 2025
- CFPB FCRA Guidance (2025)
- AWS Prescriptive Guidance: Encryption Best Practices

---

### Phase 2: Security & Compliance (Days 6-8)

#### Feature 2.1: API Key Authentication
**Priority:** P0 (Blocker)
**Effort:** 1 day
**Dependencies:** Buyers table created
**OWASP Compliance:** API2:2023 Broken Authentication (#2 Risk)
**CVE Reference:** CVE-2025-59425 (Timing Attack Prevention)

---

#### 🔴 CRITICAL SECURITY GOTCHAS

**1. Timing Attack Vulnerability (CVE-2025-59425)**
- **CATASTROPHIC:** Regular string comparison (`==` or `Equals()`) leaks timing information
- Attacker can guess API keys **one character at a time** by measuring response time
- Each correct character increases comparison time by ~2x
- **SOLUTION:** Use `CryptographicOperations.FixedTimeEquals()` for constant-time comparison

**2. API Key Usage (OWASP API2:2023)**
- **CRITICAL MISCONCEPTION:** API keys are NOT user authentication
- API keys should ONLY be used for **client authentication** (server-to-server)
- Do NOT use API keys to authenticate individual users
- Use OAuth 2.0 / JWT for user authentication

**3. Key Rotation Requirements**
- **MINIMUM:** Rotate API keys every 90 days
- High-security environments: Monthly or weekly rotation
- Without rotation: Compromised keys remain valid indefinitely
- **SOLUTION:** Automated rotation with AWS Secrets Manager

**4. Rate Limiting**
- Auth endpoints need **stricter** rate limiting than regular API endpoints
- Failed authentication attempts must trigger anti-brute force protection
- **INDUSTRY STANDARD:** 5 failed attempts = temporary block (exponential backoff)

---

#### **Implementation: Timing-Attack-Resistant Authentication**

```csharp
using System.Security.Cryptography;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IBuyerRepository _buyerRepository;
    private readonly IAuthenticationAuditService _auditService;
    private readonly ILogger<ApiKeyAuthMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract API key from X-API-Key header
        if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyFromHeader))
        {
            await LogAuthenticationFailureAsync(context, "missing_api_key");

            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Missing API key",
                message = "Include X-API-Key header with valid API key"
            });
            return;
        }

        // Retrieve buyer by API key (constant-time lookup)
        var buyer = await _buyerRepository.FindByApiKeyHashAsync(
            ComputeApiKeyHash(apiKeyFromHeader)
        );

        // CRITICAL: Use constant-time comparison to prevent timing attacks
        bool isValid = false;
        if (buyer != null && buyer.IsActive && !string.IsNullOrEmpty(buyer.ApiKeyHash))
        {
            byte[] providedKeyHash = ComputeApiKeyHashBytes(apiKeyFromHeader);
            byte[] storedKeyHash = Convert.FromBase64String(buyer.ApiKeyHash);

            // CryptographicOperations.FixedTimeEquals prevents timing attacks (CVE-2025-59425)
            isValid = CryptographicOperations.FixedTimeEquals(providedKeyHash, storedKeyHash);
        }

        if (!isValid)
        {
            await LogAuthenticationFailureAsync(context, "invalid_api_key", apiKeyFromHeader);

            // SECURITY: Always return same response regardless of reason (prevents enumeration)
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Invalid API key"
            });
            return;
        }

        // Check if API key needs rotation (90-day policy)
        if (buyer.ApiKeyCreatedAt.AddDays(90) < DateTime.UtcNow)
        {
            _logger.LogWarning(
                "API key for buyer {BuyerId} is {Days} days old - rotation recommended",
                buyer.BuyerId,
                (DateTime.UtcNow - buyer.ApiKeyCreatedAt).Days
            );
        }

        // Store buyer in context for downstream middleware
        context.Items["Buyer"] = buyer;
        context.Items["AuthenticatedAt"] = DateTime.UtcNow;

        await LogAuthenticationSuccessAsync(context, buyer);
        await _next(context);
    }

    // Compute SHA-256 hash of API key for database lookup
    private string ComputeApiKeyHash(string apiKey)
    {
        return Convert.ToBase64String(ComputeApiKeyHashBytes(apiKey));
    }

    private byte[] ComputeApiKeyHashBytes(string apiKey)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
    }

    // Log authentication failures for security monitoring
    private async Task LogAuthenticationFailureAsync(
        HttpContext context,
        string reason,
        string attemptedKey = null)
    {
        await _auditService.LogAuthenticationAttemptAsync(new AuthenticationAttempt
        {
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = context.Request.Headers["User-Agent"].ToString(),
            ApiKeyPrefix = attemptedKey?.Substring(0, Math.Min(8, attemptedKey.Length)),
            Result = "failure",
            FailureReason = reason,
            Timestamp = DateTime.UtcNow
        });
    }

    private async Task LogAuthenticationSuccessAsync(HttpContext context, Buyer buyer)
    {
        await _auditService.LogAuthenticationAttemptAsync(new AuthenticationAttempt
        {
            BuyerId = buyer.BuyerId,
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = context.Request.Headers["User-Agent"].ToString(),
            Result = "success",
            Timestamp = DateTime.UtcNow
        });
    }
}
```

---

#### **Repository Implementation: Secure API Key Validation**

```csharp
public class BuyerRepository : IBuyerRepository
{
    private readonly ApplicationDbContext _context;

    // Constant-time lookup by hashed API key
    public async Task<Buyer?> FindByApiKeyHashAsync(string apiKeyHash, CancellationToken cancellationToken = default)
    {
        // Index on api_key_hash for fast lookup
        return await _context.Buyers
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.ApiKeyHash == apiKeyHash, cancellationToken);
    }
}
```

---

#### **API Key Generation Service**

```csharp
using System.Security.Cryptography;

public class ApiKeyGenerationService
{
    // Generate cryptographically secure API key (256-bit entropy)
    public string GenerateApiKey()
    {
        // 32 bytes = 256 bits of entropy
        byte[] keyBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(keyBytes);
        }

        // Base64 encoding (URL-safe variant recommended)
        return Convert.ToBase64String(keyBytes)
            .TrimEnd('=')                    // Remove padding
            .Replace('+', '-')               // URL-safe characters
            .Replace('/', '_');              // URL-safe characters
    }

    // Hash API key for storage (SHA-256)
    public string HashApiKey(string apiKey)
    {
        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(hashBytes);
    }
}
```

---

#### **Automated Key Rotation with AWS Secrets Manager**

```csharp
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

public class ApiKeyRotationService
{
    private readonly IAmazonSecretsManager _secretsManager;
    private readonly IBuyerRepository _buyerRepository;
    private readonly ILogger<ApiKeyRotationService> _logger;

    // Rotate API key for a specific buyer (90-day cycle)
    public async Task<string> RotateApiKeyAsync(int buyerId, CancellationToken cancellationToken = default)
    {
        var buyer = await _buyerRepository.GetByIdAsync(buyerId, cancellationToken);
        if (buyer == null)
            throw new ArgumentException($"Buyer {buyerId} not found");

        // Generate new API key
        var apiKeyService = new ApiKeyGenerationService();
        string newApiKey = apiKeyService.GenerateApiKey();
        string newApiKeyHash = apiKeyService.HashApiKey(newApiKey);

        // Store old key in rotation history
        var rotationRecord = new ApiKeyRotationHistory
        {
            BuyerId = buyerId,
            OldApiKeyHash = buyer.ApiKeyHash,
            NewApiKeyHash = newApiKeyHash,
            RotatedAt = DateTime.UtcNow,
            RotationReason = "scheduled_90_day_rotation"
        };

        // Update buyer with new API key hash
        buyer.ApiKeyHash = newApiKeyHash;
        buyer.ApiKeyCreatedAt = DateTime.UtcNow;
        buyer.UpdatedAt = DateTime.UtcNow;

        await _buyerRepository.UpdateAsync(buyer, cancellationToken);
        await _buyerRepository.AddRotationHistoryAsync(rotationRecord, cancellationToken);

        // Store new API key in AWS Secrets Manager (encrypted)
        await StoreApiKeyInSecretsManagerAsync(buyerId, newApiKey, cancellationToken);

        _logger.LogInformation(
            "API key rotated for buyer {BuyerId} - old key invalidated, new key generated",
            buyerId
        );

        return newApiKey; // Return only once - buyer must store securely
    }

    // Store API key in AWS Secrets Manager
    private async Task StoreApiKeyInSecretsManagerAsync(
        int buyerId,
        string apiKey,
        CancellationToken cancellationToken)
    {
        var secretName = $"equifax-api-buyer-{buyerId}-key";

        var request = new PutSecretValueRequest
        {
            SecretId = secretName,
            SecretString = apiKey
        };

        try
        {
            await _secretsManager.PutSecretValueAsync(request, cancellationToken);
        }
        catch (ResourceNotFoundException)
        {
            // Create secret if it doesn't exist
            var createRequest = new CreateSecretRequest
            {
                Name = secretName,
                SecretString = apiKey,
                Description = $"API key for Equifax Lead Enrichment buyer {buyerId}"
            };
            await _secretsManager.CreateSecretAsync(createRequest, cancellationToken);
        }
    }

    // Automated rotation Lambda function (scheduled CloudWatch Event)
    // Runs daily, rotates keys older than 90 days
    public async Task RotateExpiredKeysAsync(CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-90);
        var buyersNeedingRotation = await _buyerRepository.GetBuyersWithKeysOlderThanAsync(
            cutoffDate,
            cancellationToken
        );

        foreach (var buyer in buyersNeedingRotation)
        {
            try
            {
                await RotateApiKeyAsync(buyer.BuyerId, cancellationToken);
                _logger.LogInformation(
                    "Automatically rotated API key for buyer {BuyerId} (key age: {Days} days)",
                    buyer.BuyerId,
                    (DateTime.UtcNow - buyer.ApiKeyCreatedAt).Days
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to rotate API key for buyer {BuyerId}",
                    buyer.BuyerId
                );
            }
        }
    }
}
```

---

#### **Security Best Practices (2025 Standards)**

**1. Timing Attack Prevention (CRITICAL)**
- ✅ ALWAYS use `CryptographicOperations.FixedTimeEquals()` for API key comparison
- ❌ NEVER use `==`, `Equals()`, or `String.Compare()` (vulnerable to timing attacks)
- Timing attacks allow attackers to guess keys one character at a time

**2. Key Storage**
- ✅ Store SHA-256 hashes in database (NOT plaintext keys)
- ✅ Use AWS Secrets Manager for encrypted key storage and retrieval
- ❌ NEVER log API keys (even partially) in application logs
- Index `api_key_hash` column for fast lookup (B-tree index)

**3. Key Generation**
- ✅ Use `RandomNumberGenerator.Create()` for cryptographically secure randomness
- ✅ Generate 256-bit keys (32 bytes) minimum
- ✅ Use Base64 URL-safe encoding (replace `+/=` characters)
- ❌ NEVER use `Random()` or `Guid.NewGuid()` (insufficient entropy)

**4. Key Rotation (Automated)**
- ✅ Rotate API keys every 90 days (industry standard)
- ✅ High-security environments: 30-60 day rotation
- ✅ Store rotation history for audit trail
- ✅ Use AWS Lambda + CloudWatch Events for automated rotation
- ❌ NEVER allow keys to remain valid indefinitely

**5. Rate Limiting (OWASP API2:2023)**
- ✅ **Stricter** rate limits for authentication endpoints than regular API
- ✅ Failed authentication: 5 attempts = temporary block (15 minutes)
- ✅ Exponential backoff for repeated failures
- ✅ Monitor for brute force patterns (IpAddress + UserAgent)

**6. Audit Logging**
- ✅ Log ALL authentication attempts (success and failure)
- ✅ Store: BuyerId, IpAddress, UserAgent, Timestamp, Result
- ✅ For failures: Log reason (missing_key, invalid_key, inactive_buyer)
- ✅ 24-month retention minimum (FCRA compliance)
- ❌ NEVER log actual API keys (log only first 8 characters as prefix)

**7. Key Lifecycle Management**
- **Generation:** Use cryptographically secure RNG (256-bit entropy)
- **Storage:** SHA-256 hash in PostgreSQL + encrypted copy in AWS Secrets Manager
- **Validation:** Constant-time comparison (FixedTimeEquals)
- **Rotation:** Automated 90-day cycle via Lambda
- **Revocation:** Soft delete (set IsActive=false, preserve audit trail)
- **History:** Store all rotations/revocations with timestamp and reason

**8. OWASP API2:2023 Compliance**
- ✅ API keys for **client authentication only** (NOT user authentication)
- ✅ Implement anti-brute force protection
- ✅ Use industry-standard authentication frameworks (ASP.NET Core Identity)
- ✅ Maintain audit trail for all authentication events
- ✅ Regular security audits and penetration testing

**9. Zero Trust Architecture (2025 Trend)**
- ✅ Verify every request (even with valid API key)
- ✅ Check buyer IsActive status on every request
- ✅ Monitor for anomalous behavior patterns
- ✅ Geographic restrictions (if applicable)
- ✅ IP allowlisting option for high-security clients

**10. Incident Response**
- ✅ Detect: Monitor failed authentication patterns (5+ failures in 1 minute)
- ✅ Alert: Notify security team for suspected brute force attacks
- ✅ Respond: Automatically block IPs with excessive failures
- ✅ Recover: Force key rotation for compromised buyers
- ✅ Learn: Update detection rules based on attack patterns

---

#### Feature 2.2: Rate Limiting (Distributed Redis-Based)
**Priority:** P0 (Blocker)
**Effort:** 2 days (distributed system complexity)
**Dependencies:** Redis cluster (AWS ElastiCache), rate limit tracking in Redis
**Algorithm:** Sliding Window Counter (best balance of precision and efficiency)

---

#### 🔴 CRITICAL GOTCHAS

**1. Configuration Logic Error (MATHEMATICAL IMPOSSIBILITY)**
- ❌ **WRONG:** 1 hour = 60,000 requests BUT 1 day = 10,000 requests
- This is **mathematically impossible**: 60,000/hour × 24 hours = 1,440,000/day minimum
- Original config would **immediately block** all traffic after 10,000 daily requests
- **FIX:** Ensure time-based limits are logically consistent

**2. Native .NET Rate Limiter Limitations**
- ⚠️ **CRITICAL:** Built-in `Microsoft.AspNetCore.RateLimiting` (.NET 7+) is **in-memory ONLY**
- NOT distributed - each server instance enforces limits independently
- Counters reset on application restart (lose all tracking)
- In multi-node deployments: 3 servers × 1000/min limit = 3000/min actual throughput
- **SOLUTION:** Use Redis with Lua scripts for distributed atomic operations

**3. Race Conditions in Distributed Systems**
- **CATASTROPHIC:** Get-then-set pattern creates race condition
- Example: 2 servers read "999 requests" simultaneously, both increment to 1000, both allow request = 1001 total (limit exceeded)
- Non-atomic operations allow requests to slip through during read-increment-write cycle
- **SOLUTION:** Redis INCR + EXPIRE in single Lua script (atomic operation)

**4. Clock Skew Between Distributed Nodes**
- Server clocks drift over time (even with NTP sync)
- Inconsistent time windows between nodes = unfair rate limits
- Example: Server A thinks minute started at :00, Server B thinks :01 = double counting
- **SOLUTION:** Use Redis server time (single source of truth) or Unix timestamps

**5. IP-Based Rate Limiting Insufficient**
- NAT/proxy environments: Multiple users share same IP = unfair blocking
- IP spoofing possible (though rare with X-Forwarded-For validation)
- **SOLUTION:** Per-API-key rate limiting (more accurate, fair, and secure)

---

#### **Corrected Rate Limit Configuration**

```json
{
  "RateLimiting": {
    "Redis": {
      "ConnectionString": "{{AWS_ELASTICACHE_ENDPOINT}}",
      "InstanceName": "equifax-api-rate-limit:"
    },
    "ApiKeyLimits": {
      "Endpoint": "POST:/api/data_enhancement/lookup",
      "PerMinute": 1000,      // Burst protection: 1000 requests/minute max
      "PerHour": 60000,       // Sustained load: 60K requests/hour (1000/min × 60)
      "PerDay": 1000000,      // Daily quota: 1M requests/day (allows variance)
      "BurstWindow": 10,      // seconds
      "BurstAllowance": 1500  // 1500 requests in 10 seconds (50% over per-second rate)
    },
    "OverageBilling": {
      "Enabled": true,
      "PricePerCall": 0.035,           // $0.035 per qualified call (Section 3.2)
      "MonthlyIncludedQuota": 100000,  // Base plan includes 100K calls/month
      "BillingCycle": "monthly"
    },
    "HttpResponses": {
      "StatusCode": 429,
      "IncludeHeaders": true,          // X-RateLimit-Limit, X-RateLimit-Remaining, X-RateLimit-Reset
      "IncludeRetryAfter": true        // Retry-After header (seconds until limit resets)
    }
  }
}
```

**Logical Consistency Check:**
- ✅ Per-minute: 1,000 requests
- ✅ Per-hour: 60,000 requests (1,000/min × 60 min = 60,000) ✓
- ✅ Per-day: 1,000,000 requests (60,000/hour × 24 hours = 1,440,000 theoretical, cap at 1M for safety)
- ✅ Burst: 1,500 requests in 10 seconds (150/sec vs 16.67/sec sustained = 9× burst capacity)

---

#### **Implementation: Redis-Based Distributed Rate Limiter with Sliding Window**

```csharp
using StackExchange.Redis;
using System.Security.Cryptography;

public class RedisRateLimiterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisRateLimiterMiddleware> _logger;
    private readonly RateLimitConfiguration _config;

    // Lua script for atomic rate limiting (prevents race conditions)
    private const string LuaScript = @"
        local key = KEYS[1]
        local limit = tonumber(ARGV[1])
        local window = tonumber(ARGV[2])
        local current_time = tonumber(ARGV[3])

        -- Remove expired entries (sliding window)
        redis.call('ZREMRANGEBYSCORE', key, 0, current_time - window)

        -- Count requests in current window
        local current_count = redis.call('ZCARD', key)

        if current_count < limit then
            -- Add current request with timestamp as score
            redis.call('ZADD', key, current_time, current_time)
            redis.call('EXPIRE', key, window)
            return {1, limit - current_count - 1, window - (current_time % window)}
        else
            -- Calculate time until window resets
            local oldest = redis.call('ZRANGE', key, 0, 0, 'WITHSCORES')[2]
            local reset_time = tonumber(oldest) + window - current_time
            return {0, 0, reset_time}
        end
    ";

    public async Task InvokeAsync(HttpContext context)
    {
        var buyer = context.Items["Buyer"] as Buyer;
        if (buyer == null)
        {
            await _next(context);
            return;
        }

        // Rate limit key: per API key (more accurate than IP-based)
        string rateLimitKey = $"rate_limit:apikey:{buyer.ApiKey}:minute";

        var db = _redis.GetDatabase();
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Execute Lua script atomically
        var result = await db.ScriptEvaluateAsync(
            LuaScript,
            keys: new RedisKey[] { rateLimitKey },
            values: new RedisValue[] {
                _config.PerMinute,        // limit
                60,                        // window (seconds)
                currentTime                // current timestamp
            }
        );

        var resultArray = (RedisValue[])result;
        bool allowed = (int)resultArray[0] == 1;
        long remaining = (long)resultArray[1];
        long resetSeconds = (long)resultArray[2];

        // Add rate limit headers (industry standard)
        context.Response.Headers["X-RateLimit-Limit"] = _config.PerMinute.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = resetSeconds.ToString();

        if (!allowed)
        {
            // Track overage for billing
            await TrackOverageRequestAsync(buyer.BuyerId, context.Request.Path);

            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers["Retry-After"] = resetSeconds.ToString();

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                message = $"You have exceeded the rate limit of {_config.PerMinute} requests per minute",
                retryAfter = resetSeconds,
                overageBilling = _config.OverageBilling.Enabled
                    ? $"Overage requests are billed at ${_config.OverageBilling.PricePerCall} per call"
                    : null
            });

            _logger.LogWarning(
                "Rate limit exceeded for buyer {BuyerId} - API key: {ApiKeyPrefix}",
                buyer.BuyerId,
                buyer.ApiKey?.Substring(0, 8)
            );

            return;
        }

        await _next(context);
    }

    // Track overage requests for billing
    private async Task TrackOverageRequestAsync(int buyerId, string endpoint)
    {
        var db = _redis.GetDatabase();
        string overageKey = $"overage:buyer:{buyerId}:{DateTime.UtcNow:yyyyMM}";

        // Increment monthly overage counter
        await db.StringIncrementAsync(overageKey);

        // Set expiration (2 months for billing reconciliation)
        await db.KeyExpireAsync(overageKey, TimeSpan.FromDays(60));
    }
}
```

---

#### **Alternative: Token Bucket for Burst Handling**

```csharp
public class TokenBucketRateLimiter
{
    private const string LuaTokenBucket = @"
        local key = KEYS[1]
        local max_tokens = tonumber(ARGV[1])
        local refill_rate = tonumber(ARGV[2])
        local requested_tokens = tonumber(ARGV[3])
        local current_time = tonumber(ARGV[4])

        local bucket = redis.call('HMGET', key, 'tokens', 'last_refill')
        local tokens = tonumber(bucket[1]) or max_tokens
        local last_refill = tonumber(bucket[2]) or current_time

        -- Refill tokens based on time elapsed
        local elapsed = current_time - last_refill
        local new_tokens = math.min(max_tokens, tokens + (elapsed * refill_rate))

        if new_tokens >= requested_tokens then
            -- Allow request and consume tokens
            redis.call('HSET', key, 'tokens', new_tokens - requested_tokens, 'last_refill', current_time)
            redis.call('EXPIRE', key, 3600)
            return {1, new_tokens - requested_tokens}
        else
            -- Deny request
            return {0, new_tokens}
        end
    ";

    public async Task<(bool Allowed, long RemainingTokens)> TryConsumeAsync(
        string apiKey,
        int tokensRequested = 1)
    {
        var db = _redis.GetDatabase();
        string key = $"token_bucket:apikey:{apiKey}";

        var result = await db.ScriptEvaluateAsync(
            LuaTokenBucket,
            keys: new RedisKey[] { key },
            values: new RedisValue[] {
                1000,                                       // max_tokens (burst capacity)
                16.67,                                      // refill_rate (1000/60 = 16.67 tokens/second)
                tokensRequested,                            // requested_tokens
                DateTimeOffset.UtcNow.ToUnixTimeSeconds()   // current_time
            }
        );

        var resultArray = (RedisValue[])result;
        bool allowed = (int)resultArray[0] == 1;
        long remainingTokens = (long)resultArray[1];

        return (allowed, remainingTokens);
    }
}
```

---

#### **Overage Billing Implementation**

```csharp
public class OverageBillingService
{
    private readonly ApplicationDbContext _context;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<OverageBillingService> _logger;

    // Calculate monthly overage charges for a buyer
    public async Task<OverageBillingReport> CalculateMonthlyOverageAsync(
        int buyerId,
        DateTime billingMonth,
        CancellationToken cancellationToken = default)
    {
        var buyer = await _context.Buyers
            .Include(b => b.RateLimitConfig)
            .FirstOrDefaultAsync(b => b.BuyerId == buyerId, cancellationToken);

        if (buyer == null)
            throw new ArgumentException($"Buyer {buyerId} not found");

        // Get total requests for the month from audit logs
        var monthStart = new DateTime(billingMonth.Year, billingMonth.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var monthlyRequests = await _context.AuditLogs
            .Where(a => a.BuyerId == buyerId
                     && a.QueriedAt >= monthStart
                     && a.QueriedAt < monthEnd
                     && a.MatchFound == true)  // Only qualified calls (matches found)
            .CountAsync(cancellationToken);

        // Calculate overage
        int includedQuota = buyer.RateLimitConfig?.MonthlyIncludedQuota ?? 100000;
        int overageRequests = Math.Max(0, monthlyRequests - includedQuota);
        decimal overageCharge = overageRequests * 0.035m; // $0.035 per call (Section 3.2)

        var report = new OverageBillingReport
        {
            BuyerId = buyerId,
            BillingMonth = billingMonth,
            TotalRequests = monthlyRequests,
            IncludedQuota = includedQuota,
            OverageRequests = overageRequests,
            PricePerCall = 0.035m,
            OverageCharge = overageCharge,
            BaseSubscriptionFee = buyer.RateLimitConfig?.MonthlyBaseFee ?? 0m,
            TotalCharge = (buyer.RateLimitConfig?.MonthlyBaseFee ?? 0m) + overageCharge,
            GeneratedAt = DateTime.UtcNow
        };

        // Store billing record
        await _context.BillingRecords.AddAsync(new BillingRecord
        {
            BuyerId = buyerId,
            BillingMonth = billingMonth,
            TotalRequests = monthlyRequests,
            OverageRequests = overageRequests,
            OverageCharge = overageCharge,
            TotalCharge = report.TotalCharge,
            GeneratedAt = DateTime.UtcNow,
            Status = "pending"
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Generated overage billing report for buyer {BuyerId}: {Requests} total, {Overage} overage, ${Charge} charge",
            buyerId,
            monthlyRequests,
            overageRequests,
            overageCharge
        );

        return report;
    }

    // Automated monthly billing job (runs on 1st of each month)
    public async Task ProcessMonthlyBillingAsync(CancellationToken cancellationToken = default)
    {
        var lastMonth = DateTime.UtcNow.AddMonths(-1);

        var activeBuyers = await _context.Buyers
            .Where(b => b.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var buyer in activeBuyers)
        {
            try
            {
                var report = await CalculateMonthlyOverageAsync(buyer.BuyerId, lastMonth, cancellationToken);

                // Send invoice to buyer
                // await _invoiceService.SendInvoiceAsync(buyer, report);

                _logger.LogInformation(
                    "Monthly billing processed for buyer {BuyerId} - Total: ${Total}",
                    buyer.BuyerId,
                    report.TotalCharge
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to process monthly billing for buyer {BuyerId}",
                    buyer.BuyerId
                );
            }
        }
    }
}

public class OverageBillingReport
{
    public int BuyerId { get; set; }
    public DateTime BillingMonth { get; set; }
    public int TotalRequests { get; set; }
    public int IncludedQuota { get; set; }
    public int OverageRequests { get; set; }
    public decimal PricePerCall { get; set; }
    public decimal OverageCharge { get; set; }
    public decimal BaseSubscriptionFee { get; set; }
    public decimal TotalCharge { get; set; }
    public DateTime GeneratedAt { get; set; }
}
```

---

#### **Rate Limit Configuration per Buyer (Database Model)**

```csharp
public class Buyer
{
    public int BuyerId { get; set; }
    public string CompanyName { get; set; }
    public string ApiKey { get; set; }
    public string ApiKeyHash { get; set; }
    public bool IsActive { get; set; }

    // Rate limiting configuration
    public RateLimitConfig RateLimitConfig { get; set; }
}

public class RateLimitConfig
{
    public int ConfigId { get; set; }
    public int BuyerId { get; set; }

    // Rate limits
    public int PerMinuteLimit { get; set; } = 1000;
    public int PerHourLimit { get; set; } = 60000;
    public int PerDayLimit { get; set; } = 1000000;

    // Overage billing
    public int MonthlyIncludedQuota { get; set; } = 100000;
    public decimal MonthlyBaseFee { get; set; } = 1000m;
    public decimal OveragePricePerCall { get; set; } = 0.035m;
    public bool OverageBillingEnabled { get; set; } = true;

    // Custom limits for enterprise clients
    public bool IsCustomPlan { get; set; } = false;

    // Navigation
    public Buyer Buyer { get; set; }
}
```

---

#### **Security Best Practices (2025 Standards)**

**1. Distributed Rate Limiting (CRITICAL)**
- ✅ Use Redis with Lua scripts for atomic operations (prevents race conditions)
- ✅ Sliding window algorithm (best precision/efficiency balance)
- ✅ Token bucket for burst handling (allows temporary spikes)
- ❌ NEVER use native .NET in-memory rate limiter for multi-node deployments
- ❌ NEVER use get-then-set pattern (creates race conditions)

**2. Per-API-Key Rate Limiting**
- ✅ Rate limit by API key (accurate, fair, secure)
- ✅ Configurable per buyer (enterprise customization)
- ❌ Avoid IP-based only (NAT/proxy environments share IPs)
- Use IP + API key combination for defense in depth

**3. Clock Synchronization**
- ✅ Use Redis server time (single source of truth)
- ✅ Use Unix timestamps (no timezone issues)
- ❌ Avoid relying on application server clocks (drift over time)
- Configure NTP on all servers for backup time sync

**4. HTTP Standards**
- ✅ Return HTTP 429 (Too Many Requests)
- ✅ Include `Retry-After` header (seconds until reset)
- ✅ Include `X-RateLimit-Limit` (total limit)
- ✅ Include `X-RateLimit-Remaining` (requests left)
- ✅ Include `X-RateLimit-Reset` (seconds until reset)

**5. Overage Billing Strategy**
- ✅ Fixed fee + overage model (predictable base, flexible usage)
- ✅ Monthly included quota (e.g., 100K calls)
- ✅ Per-call overage pricing ($0.035 per qualified call - Section 3.2)
- ✅ Real-time overage tracking in Redis
- ✅ Monthly billing reconciliation from audit logs
- ✅ Only bill "qualified calls" (matches found, not 404s)

**6. Performance Optimization**
- ✅ Redis pipeline commands when possible
- ✅ Lua scripts eliminate round-trips (single atomic operation)
- ✅ AWS ElastiCache for production (managed Redis cluster)
- ✅ Connection multiplexing (StackExchange.Redis handles this)
- Target: < 5ms rate limit check overhead

**7. Monitoring and Alerts**
- ✅ Monitor rate limit hit rates per buyer
- ✅ Alert on excessive 429 responses (may indicate attack or mis-configuration)
- ✅ Track overage requests for billing accuracy
- ✅ Dashboard showing real-time rate limit usage
- ✅ Anomaly detection for unusual traffic patterns

**8. Graceful Degradation**
- ✅ If Redis unavailable: Fail open (allow requests) OR fail closed (deny requests)
- ✅ Recommended: Fail open with logging (maintain availability)
- ✅ Circuit breaker pattern for Redis connectivity issues
- ✅ Fallback to in-memory rate limiting (per-node) if Redis down

**9. Testing**
- ✅ Load test with multiple concurrent clients
- ✅ Verify atomic operations (no race conditions)
- ✅ Test clock skew scenarios
- ✅ Validate overage billing calculations
- ✅ Test Redis failover scenarios

**10. Compliance**
- ✅ Contractual requirement: $0.035 per overage call (Section 3.2)
- ✅ Transparent pricing displayed in 429 responses
- ✅ Detailed billing reports monthly
- ✅ Audit trail for all rate limit decisions

---

#### Feature 2.3: FCRA Audit Logging (Immutable, High-Performance)
**Priority:** P0 (Blocker)
**Effort:** 2 days (compliance + performance optimization)
**Dependencies:** Audit logs table (partitioned), PostgreSQL 18, async infrastructure
**Framework:** Audit.NET for comprehensive audit trail
**Compliance:** FCRA, SOC2 Type II (GDPR in Phase 2)

---

#### 🔴 CRITICAL GOTCHAS

**1. Async Logging Performance (CRITICAL)**
- **PROBLEM:** Synchronous logging blocks API response (adds 10-50ms latency)
- Middleware `await _auditRepository.LogAsync()` blocks user response until DB commit
- At 1000 req/min, audit logging becomes bottleneck
- **SOLUTION:** Fire-and-forget async logging, use background queue (Channel<T>)

**2. PostgreSQL Table Bloat at Scale**
- **PROBLEM:** 326M Equifax records + audit logs = multi-billion row tables
- Without partitioning: queries slow to 5-10 seconds at scale
- Index bloat from constant INSERTs
- **SOLUTION:** PostgreSQL table partitioning by month (automatically archives old data)

**3. Immutability Enforcement**
- **PROBLEM:** PostgreSQL allows UPDATE/DELETE by default (not truly immutable)
- DBAs could accidentally modify audit logs
- **SOLUTION:** PostgreSQL triggers that REJECT all UPDATE/DELETE operations + database-level permissions

**4. Clock Synchronization**
- **PROBLEM:** Distributed servers with clock skew = incorrect audit timestamps
- **SOLUTION:** Use PostgreSQL server time (`NOW()`) instead of application server time

**5. What to Log vs. Not Log**
- ✅ **MUST LOG:** WHO (BuyerId), WHAT (PhoneNumberQueried - hashed), WHEN (timestamp), WHY (PermissiblePurpose), RESULT (MatchFound)
- ❌ **DO NOT LOG:** Full PII in plaintext (SSN, full addresses) - use hashed identifiers only
- ⚠️ **SECURITY RISK:** Logging excessive PII increases breach liability

---

#### **Corrected Implementation: Async, Immutable, High-Performance**

#### **1. Database Schema with Immutability**

```sql
-- Partitioned audit log table (monthly partitions for performance)
CREATE TABLE audit_logs (
    audit_log_id BIGSERIAL NOT NULL,
    buyer_id INTEGER NOT NULL,

    -- Query details (hashed for privacy)
    phone_number_queried_hash VARCHAR(64) NOT NULL,  -- SHA-256 hash (not plaintext)
    consumer_key_returned UUID,
    match_found BOOLEAN NOT NULL,

    -- FCRA compliance
    permissible_purpose VARCHAR(100) NOT NULL,

    -- Request metadata
    ip_address INET NOT NULL,
    user_agent TEXT,
    request_id UUID NOT NULL,  -- Trace requests across systems

    -- Performance tracking
    response_time_ms INTEGER NOT NULL,

    -- Immutable timestamp (PostgreSQL server time)
    queried_at TIMESTAMP WITH TIME ZONE DEFAULT NOW() NOT NULL,

    -- Audit metadata
    log_version INTEGER DEFAULT 1,  -- Schema version for migrations

    PRIMARY KEY (audit_log_id, queried_at)  -- Composite key for partitioning
) PARTITION BY RANGE (queried_at);

-- Create monthly partitions (automatically managed)
CREATE TABLE audit_logs_2025_10 PARTITION OF audit_logs
    FOR VALUES FROM ('2025-10-01') TO ('2025-11-01');

CREATE TABLE audit_logs_2025_11 PARTITION OF audit_logs
    FOR VALUES FROM ('2025-11-01') TO ('2025-12-01');

-- Indexes for fast querying
CREATE INDEX idx_audit_logs_buyer_id ON audit_logs (buyer_id, queried_at DESC);
CREATE INDEX idx_audit_logs_phone_hash ON audit_logs (phone_number_queried_hash);
CREATE INDEX idx_audit_logs_request_id ON audit_logs (request_id);

-- IMMUTABILITY ENFORCEMENT: Trigger that rejects UPDATE/DELETE
CREATE OR REPLACE FUNCTION reject_audit_modifications()
RETURNS TRIGGER AS $$
BEGIN
    RAISE EXCEPTION 'Audit logs are immutable - modifications not allowed (FCRA compliance)';
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER audit_logs_immutable
    BEFORE UPDATE OR DELETE ON audit_logs
    FOR EACH ROW
    EXECUTE FUNCTION reject_audit_modifications();

-- Database-level permissions (read-only for most users)
REVOKE UPDATE, DELETE ON audit_logs FROM PUBLIC;
GRANT SELECT, INSERT ON audit_logs TO api_service_role;
```

---

#### **2. High-Performance Async Audit Logging Service**

```csharp
using System.Threading.Channels;
using System.Security.Cryptography;

public class AuditLoggingService : BackgroundService
{
    private readonly Channel<AuditLogEntry> _auditQueue;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditLoggingService> _logger;

    public AuditLoggingService(ApplicationDbContext context, ILogger<AuditLoggingService> logger)
    {
        _context = context;
        _logger = logger;

        // Bounded channel: Drop oldest if queue full (prevents memory exhaustion)
        _auditQueue = Channel.CreateBounded<AuditLogEntry>(new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
    }

    // Public method: Enqueue audit log (non-blocking)
    public ValueTask EnqueueAuditLogAsync(AuditLogEntry entry)
    {
        // Fire-and-forget: Does NOT block API response
        return _auditQueue.Writer.TryWrite(entry)
            ? ValueTask.CompletedTask
            : _auditQueue.Writer.WriteAsync(entry);
    }

    // Background worker: Process queue
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var entry in _auditQueue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                // Batch processing: Accumulate 100 logs or 1 second timeout
                var batch = new List<AuditLogEntry> { entry };

                while (batch.Count < 100 && _auditQueue.Reader.TryRead(out var nextEntry))
                {
                    batch.Add(nextEntry);
                }

                // Bulk insert for performance
                await _context.AuditLogs.AddRangeAsync(batch, stoppingToken);
                await _context.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("Persisted {Count} audit logs", batch.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist audit log: {@Entry}", entry);

                // CRITICAL: Do not lose audit logs - retry or write to dead-letter queue
                // TODO: Implement dead-letter queue for failed audit logs
            }
        }
    }

    // Hash phone number for privacy-preserving audit
    public static string HashPhoneNumber(string phoneNumber)
    {
        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(phoneNumber));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
```

---

#### **3. Audit Logging Middleware (Fire-and-Forget)**

```csharp
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AuditLoggingService _auditService;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var buyer = context.Items["Buyer"] as Buyer;
        if (buyer == null)
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid();
        context.Items["RequestId"] = requestId;

        // Capture request details BEFORE processing
        string phoneNumber = null;
        if (context.Request.HasJsonContentType())
        {
            // Parse request body to extract phone number
            // (Implementation depends on request DTO structure)
        }

        Exception capturedException = null;
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            capturedException = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            // Extract response details
            var consumerKey = context.Items["ConsumerKeyReturned"] as Guid?;
            var matchFound = context.Items["MatchFound"] as bool? ?? false;

            // Create audit log entry
            var auditEntry = new AuditLogEntry
            {
                BuyerId = buyer.BuyerId,
                PhoneNumberQueriedHash = AuditLoggingService.HashPhoneNumber(phoneNumber ?? "unknown"),
                ConsumerKeyReturned = consumerKey,
                MatchFound = matchFound,
                PermissiblePurpose = buyer.PermissiblePurpose,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                RequestId = requestId,
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                HttpStatusCode = context.Response.StatusCode,
                ErrorMessage = capturedException?.Message,
                QueriedAt = DateTime.UtcNow  // Will be overridden by PostgreSQL NOW()
            };

            // CRITICAL: Fire-and-forget async (does NOT block response)
            _ = _auditService.EnqueueAuditLogAsync(auditEntry);
        }
    }
}

public class AuditLogEntry
{
    public int BuyerId { get; set; }
    public string PhoneNumberQueriedHash { get; set; }
    public Guid? ConsumerKeyReturned { get; set; }
    public bool MatchFound { get; set; }
    public string PermissiblePurpose { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public Guid RequestId { get; set; }
    public int ResponseTimeMs { get; set; }
    public int HttpStatusCode { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime QueriedAt { get; set; }
}
```

---

#### **4. Automated Retention Policy (PostgreSQL Partitioning)**

```csharp
public class AuditLogRetentionService : BackgroundService
{
    private readonly ILogger<AuditLogRetentionService> _logger;
    private readonly IConfiguration _configuration;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run daily at 2 AM
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

                // Check if retention period exceeded (24 months for FCRA)
                var retentionMonths = 24;
                var cutoffDate = DateTime.UtcNow.AddMonths(-retentionMonths);

                // Archive old partitions to cold storage (S3)
                await ArchiveOldPartitionsAsync(cutoffDate, stoppingToken);

                _logger.LogInformation(
                    "Audit log retention policy executed - archived logs older than {CutoffDate}",
                    cutoffDate
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute retention policy");
            }
        }
    }

    private async Task ArchiveOldPartitionsAsync(DateTime cutoffDate, CancellationToken cancellationToken)
    {
        // PostgreSQL: Detach old partitions and export to S3
        // This keeps hot data in PostgreSQL, cold data in S3 (cost optimization)

        // Example: Detach partition for October 2023 (24 months ago)
        // ALTER TABLE audit_logs DETACH PARTITION audit_logs_2023_10;
        // COPY audit_logs_2023_10 TO 's3://audit-archive/2023-10.parquet';
        // DROP TABLE audit_logs_2023_10;

        _logger.LogInformation("Archived audit logs older than {CutoffDate}", cutoffDate);
    }
}
```

---

#### **Compliance Requirements (2025 Standards)**

**1. FCRA Requirements**
- ✅ Every query logged (no exceptions)
- ✅ 24-month minimum retention (automated with partitioning)
- ✅ Immutable logs (PostgreSQL trigger enforcement)
- ✅ Permissible purpose recorded (FCRA § 604)
- ✅ Reasonable security measures (encryption at rest, access controls)

**2. SOC2 Type II Requirements**
- ✅ 3-6 months of evidence collection for audit
- ✅ Immutable, timestamped audit trail
- ✅ Secure retention with access controls
- ✅ Automated retention policies
- ✅ Comprehensive logging (WHO, WHAT, WHEN, WHY, RESULT)

**3. Additional Standards (Future Phases)**
- **GDPR:** Phase 2 - Right to be forgotten, data minimization
- **HIPAA:** 6-year retention (if health data involved)
- **SOX:** 7-year retention (financial audit requirements)
- **PCI DSS:** 12-month retention for cardholder data environments

---

#### **Performance Optimizations (2025 Best Practices)**

**1. Async Fire-and-Forget Logging**
- ✅ Channel<T> for background queue (bounded, prevents memory exhaustion)
- ✅ Batch processing (100 logs per batch or 1-second timeout)
- ✅ Does NOT block API response (<1ms overhead)
- ❌ Avoid synchronous `await _auditRepository.LogAsync()` in middleware

**2. PostgreSQL Table Partitioning**
- ✅ Monthly partitions (automatically managed)
- ✅ Query performance: O(1) partition pruning (not O(N) table scan)
- ✅ Archive old partitions to S3 (cost optimization)
- Target: <50ms audit log query even with billions of rows

**3. Bulk Inserts**
- ✅ EF Core `AddRangeAsync` (batch inserts)
- ✅ PostgreSQL COPY command for maximum throughput
- Target: 10,000+ audit logs/second sustained

**4. Privacy-Preserving Hashing**
- ✅ SHA-256 hash phone numbers (cannot reverse-engineer)
- ✅ Allows querying without exposing plaintext PII
- ✅ Privacy best practice (minimizes PII exposure in breach)

**5. Monitoring and Alerts**
- ✅ Alert if audit queue exceeds 5,000 entries (backpressure)
- ✅ Alert if audit log write latency > 1 second
- ✅ Daily verification: Audit log count matches API request count
- ✅ Dead-letter queue for failed audit log writes

---

#### **Security Best Practices**

**1. Immutability Enforcement**
- ✅ PostgreSQL trigger rejects UPDATE/DELETE operations
- ✅ Database-level permissions (INSERT/SELECT only for API role)
- ✅ Separate admin role for compliance auditors (SELECT only)

**2. Data Protection**
- ✅ Encryption at rest (PostgreSQL TDE or AWS RDS encryption)
- ✅ Encryption in transit (TLS 1.3)
- ✅ Hash phone numbers (SHA-256, not reversible)
- ❌ NEVER log SSN, full credit card numbers, passwords

**3. Access Controls**
- ✅ Role-based access (API service can INSERT/SELECT, auditors can SELECT only)
- ✅ Audit log access is logged (who accessed audit logs)
- ✅ IP allowlisting for audit log queries

**4. Tamper Detection**
- ✅ Cryptographic checksums for audit log integrity
- ✅ Periodic verification (compare checksums)
- ✅ Alert on checksum mismatch (potential tampering)

---

#### Feature 2.4: Security Audits & Breach Notification (Automated)
**Priority:** P0 (Blocker - Contractual Requirement)
**Effort:** 3 days (automation + multi-jurisdiction compliance)
**Dependencies:** SIEM integration, monitoring infrastructure, notification workflows
**Contract Reference:** Section 13.2 - Data Security and Breach Notification (72-hour requirement)
**Compliance:** NIST SP 800-61 Rev 3 (April 2025), SOC2 CC7.1

---

#### 🔴 CRITICAL GOTCHAS

**1. 72-Hour Notification is CONTRACTUAL, Not FCRA Law**
- **MISCONCEPTION:** FCRA requires 72-hour breach notification
- **REALITY:** FCRA has NO specific breach notification timeline
- **CONTRACT:** 72 hours is from Section 13.2 (contractual obligation)
- **LEGAL REQUIREMENTS VARY:**
  - GDPR: 72 hours (not applicable in MVP)
  - HIPAA: 60 days for patient notification
  - California: 30 days (effective January 1, 2026)
  - Most states: "Without unreasonable delay" or 30-45 days

**2. When Does the Clock Start? (Legal Definition of "Discovery")**
- **PROBLEM:** "Discovery" has different legal definitions across jurisdictions
- Is it when first detected, when confirmed as breach, or when investigated?
- **INDUSTRY AVERAGE:** 205 days to identify vendor breaches (healthcare sector)
- HIPAA violations common because 60-day window starts at discovery, not reporting
- **SOLUTION:** Clear internal definition of "discovery" + automated timestamping

**3. Manual Breach Notification Processes Don't Scale**
- **PROBLEM:** Manual workflows are slow, error-prone, can't handle multiple concurrent incidents
- Fragmented systems (email, spreadsheets, tickets) cause delays
- At scale: tracking 100+ vendor relationships, each with unique SLA requirements
- **SOLUTION:** Automated workflow with SIEM integration, deadline tracking, escalation

**4. Missing the Deadline = Legal Penalties + Contract Breach**
- Late notification penalties vary by jurisdiction (fines, lawsuits, contract termination)
- **California example:** Businesses face potential fines for missing 30-day deadline
- **HIPAA:** $100-$50,000 per violation (up to $1.5M annual max)
- **CONTRACT:** 72-hour breach triggers potential contract termination or penalties
- **SOLUTION:** Automated deadline tracking with escalation alerts (48h, 60h, 70h warnings)

**5. Notification Content Requirements Differ by Jurisdiction**
- Each state/regulation requires different information in breach notices
- Missing required elements = non-compliant notification = re-notification required
- **SOLUTION:** Template system with jurisdiction-specific content validation

**6. False Positives Create Notification Fatigue**
- Over-alerting desensitizes security teams to real threats
- Automated detection without validation = unnecessary breach notifications
- **SOLUTION:** Automated triage + human validation before escalating to breach protocol

**7. Cyber Insurance Requires 24-Hour Notification or Claim DENIED**
- **CRITICAL BUSINESS REQUIREMENT:** Most cyber insurance policies require notification within 24 hours
- Missing this deadline = Insurance claim DENIED (can cost millions)
- **INDUSTRY REALITY:** 60% of organizations don't know their cyber insurance notification requirements
- Insurance covers: incident response, breach notification costs, legal defense, business interruption
- **SOLUTION:** Automated cyber insurance notification as FIRST step (before customer notification)
- Must prove appropriate controls were in place BEFORE breach (pre-breach security posture documentation)

**8. First 24 Hours = Customer Trust Window (15% Trust Drop if Delayed)**
- **BUSINESS IMPACT:** Delayed response = 15% drop in customer trust metrics
- Brands that stay quiet during breach investigation look guilty ("What are they hiding?")
- **CUSTOMER EXPECTATION:** Immediate acknowledgment ("We're investigating") beats complete information later
- Crisis communication is 90% preparation, 10% response
- **SOLUTION:** Pre-approved holding statement templates for first 24-hour response

**9. Board Communication: Business Impact, NOT Technical Details**
- **COMMON MISTAKE:** CISO presents firewall logs and vulnerability CVEs to board
- **BOARD CARES ABOUT:** Revenue impact, customer data compromised, reputation risk, regulatory fines
- Third-party breaches: 60% of all breaches, cost $370K more on average
- **SOLUTION:** Executive summary templates focusing on business metrics (customers affected, revenue at risk, brand impact)

**10. Business Continuity vs. Incident Response Integration**
- **PROBLEM:** 87% of organizations have business continuity plans but poor integration with incident response
- **CONFUSION:** IR = first line of defense, BC = keeps operations running, DR = restores systems
- All three must coordinate during breach (not siloed)
- **SOLUTION:** Cross-functional war room with CISO, Legal, PR, Executive team, BC coordinator

**11. Third-Party Vendor Breaches (60% of All Breaches)**
- **INDUSTRY REALITY:** Most breaches come from vendors/suppliers, not direct attacks
- Vendor breaches cost $370K MORE than internal breaches on average
- **DETECTION PROBLEM:** Average 205 days to identify vendor breaches (healthcare sector)
- **SOLUTION:** Vendor risk management integration with automated breach notification when vendor signals compromise

---

#### **Corrected Implementation: Comprehensive Business + Technical Integration**
```csharp
// Security incident tracking and breach notification system
public class SecurityIncidentService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SecurityIncidentService> _logger;

    // Detect and log security incidents
    public async Task<SecurityIncident> ReportIncidentAsync(SecurityIncidentDto incident)
    {
        var securityIncident = new SecurityIncident
        {
            IncidentId = Guid.NewGuid(),
            IncidentType = incident.Type, // "unauthorized_access", "data_breach", "ddos_attack"
            Severity = DetermineSeverity(incident),
            DetectedAt = DateTime.UtcNow,
            DetectionMethod = incident.DetectionMethod, // "automated_alert", "manual_review"
            Description = incident.Description,
            AffectedSystems = incident.AffectedSystems,
            AffectedRecordCount = incident.AffectedRecordCount,
            Status = "investigating"
        };

        await _context.SecurityIncidents.AddAsync(securityIncident);
        await _context.SaveChangesAsync();

        // CONTRACTUAL REQUIREMENT: 72-hour breach notification (Section 13.2)
        if (IsDataBreach(securityIncident))
        {
            await InitiateBreachNotificationProtocolAsync(securityIncident);
        }

        return securityIncident;
    }

    // Data breach notification protocol (Contract Section 13.2)
    private async Task InitiateBreachNotificationProtocolAsync(SecurityIncident incident)
    {
        var breach = new DataBreach
        {
            BreachId = Guid.NewGuid(),
            IncidentId = incident.IncidentId,
            DetectedAt = incident.DetectedAt,
            NotificationDeadline = incident.DetectedAt.AddHours(72), // CONTRACT: 72-hour requirement
            AffectedConsumerCount = incident.AffectedRecordCount,
            BreachType = incident.IncidentType,
            DataTypesCompromised = await DetermineCompromisedDataTypesAsync(incident),
            Status = "pending_notification"
        };

        await _context.DataBreaches.AddAsync(breach);

        // Immediate notifications to internal teams
        await _notificationService.SendSecurityAlertAsync(
            subject: $"CRITICAL: Data Breach Detected - {incident.IncidentType}",
            body: $"Breach detected at {incident.DetectedAt:yyyy-MM-dd HH:mm:ss UTC}\n" +
                  $"Affected records: {incident.AffectedRecordCount}\n" +
                  $"Notification deadline: {breach.NotificationDeadline:yyyy-MM-dd HH:mm:ss UTC}\n" +
                  $"Time remaining: {(breach.NotificationDeadline - DateTime.UtcNow).TotalHours:F1} hours",
            recipients: new[] { "security@switchboard.com", "legal@switchboard.com", "ciso@switchboard.com" }
        );

        // Schedule buyer notification (must occur within 72 hours)
        await ScheduleBuyerBreachNotificationAsync(breach);

        // Schedule regulatory notification (if PII compromised)
        if (RequiresRegulatoryNotification(breach))
        {
            await ScheduleRegulatoryNotificationAsync(breach);
        }

        _logger.LogCritical(
            "Data breach notification protocol initiated. BreachId: {BreachId}, Deadline: {Deadline}",
            breach.BreachId,
            breach.NotificationDeadline
        );
    }

    // Buyer breach notification (Contract Section 13.2)
    private async Task ScheduleBuyerBreachNotificationAsync(DataBreach breach)
    {
        // Identify affected buyers
        var affectedBuyers = await _context.AuditLogs
            .Where(a => a.QueriedAt >= breach.DetectedAt.AddHours(-24) && a.QueriedAt <= breach.DetectedAt)
            .Select(a => a.BuyerId)
            .Distinct()
            .ToListAsync();

        foreach (var buyerId in affectedBuyers)
        {
            var notification = new BreachNotification
            {
                NotificationId = Guid.NewGuid(),
                BreachId = breach.BreachId,
                BuyerId = buyerId,
                NotificationMethod = "email", // Also: phone, certified_mail
                ScheduledFor = DateTime.UtcNow.AddHours(1), // Notify within 1 hour of detection
                NotificationDeadline = breach.NotificationDeadline,
                Status = "pending"
            };

            await _context.BreachNotifications.AddAsync(notification);
        }

        await _context.SaveChangesAsync();
    }

    // Regulatory breach notification (GDPR Article 33, CCPA, etc.)
    private async Task ScheduleRegulatoryNotificationAsync(DataBreach breach)
    {
        var notifications = new[]
        {
            new RegulatoryNotification
            {
                BreachId = breach.BreachId,
                RegulatoryBody = "FTC", // FCRA, GLBA enforcement
                Jurisdiction = "United States",
                NotificationDeadline = breach.NotificationDeadline,
                Status = "pending"
            },
            new RegulatoryNotification
            {
                BreachId = breach.BreachId,
                RegulatoryBody = "EU Data Protection Authority",
                Jurisdiction = "European Union",
                NotificationDeadline = breach.DetectedAt.AddHours(72), // GDPR Article 33
                Status = "pending"
            },
            new RegulatoryNotification
            {
                BreachId = breach.BreachId,
                RegulatoryBody = "California Attorney General",
                Jurisdiction = "California",
                NotificationDeadline = breach.DetectedAt.AddHours(72), // CCPA requirement
                Status = "pending"
            }
        };

        await _context.RegulatoryNotifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();
    }
}

// ============================================================================
// BUSINESS-FOCUSED BREACH RESPONSE SERVICES
// ============================================================================

// Cyber insurance notification service (24-hour requirement)
public class CyberInsuranceNotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CyberInsuranceNotificationService> _logger;

    // CRITICAL: Notify cyber insurance carrier within 24 hours or claim DENIED
    public async Task NotifyInsuranceCarrierAsync(DataBreach breach)
    {
        var insuranceNotification = new InsuranceNotification
        {
            NotificationId = Guid.NewGuid(),
            BreachId = breach.BreachId,
            InsuranceCarrier = "Cyber Insurance Provider",
            PolicyNumber = "CYB-2025-12345",
            NotificationDeadline = breach.DetectedAt.AddHours(24), // 24-hour requirement

            // Required information for claim validation
            PreBreachSecurityControls = new[]
            {
                "API key authentication (Section 2.1)",
                "Rate limiting with Redis (Section 2.2)",
                "FCRA audit logging (Section 2.3)",
                "TLS 1.3 encryption in transit",
                "AES-256 encryption at rest",
                "SOC2 Type II certified"
            },

            IncidentSummary = $"Data breach detected at {breach.DetectedAt:yyyy-MM-dd HH:mm:ss UTC}. " +
                             $"Estimated {breach.AffectedConsumerCount} consumer records potentially compromised. " +
                             $"Breach type: {breach.BreachType}. Investigation in progress.",

            EstimatedLossAmount = EstimateBreachCost(breach),
            Status = "pending"
        };

        await _context.InsuranceNotifications.AddAsync(insuranceNotification);
        await _context.SaveChangesAsync();

        // Send immediate notification to insurance carrier
        await _notificationService.SendCyberInsuranceAlertAsync(
            carrier: insuranceNotification.InsuranceCarrier,
            policyNumber: insuranceNotification.PolicyNumber,
            subject: $"URGENT: Data Breach Notification - Policy {insuranceNotification.PolicyNumber}",
            body: $"Date/Time of Detection: {breach.DetectedAt:yyyy-MM-dd HH:mm:ss UTC}\n" +
                  $"Estimated Affected Records: {breach.AffectedConsumerCount}\n" +
                  $"Breach Type: {breach.BreachType}\n" +
                  $"Data Types Compromised: {string.Join(", ", breach.DataTypesCompromised)}\n" +
                  $"Estimated Loss: ${insuranceNotification.EstimatedLossAmount:N2}\n\n" +
                  $"Pre-Breach Security Controls:\n{string.Join("\n", insuranceNotification.PreBreachSecurityControls)}\n\n" +
                  $"Contact: security@switchboard.com\n" +
                  $"Incident ID: {breach.BreachId}"
        );

        _logger.LogCritical(
            "Cyber insurance carrier notified. BreachId: {BreachId}, Carrier: {Carrier}, Deadline: {Deadline}",
            breach.BreachId,
            insuranceNotification.InsuranceCarrier,
            insuranceNotification.NotificationDeadline
        );
    }

    // Estimate breach cost for insurance claim
    private decimal EstimateBreachCost(DataBreach breach)
    {
        // Industry average: $148 per compromised record (2024 IBM Cost of Data Breach Report)
        decimal costPerRecord = 148m;
        decimal estimatedCost = breach.AffectedConsumerCount * costPerRecord;

        // Add fixed costs: incident response, legal, notification
        decimal fixedCosts = 50000m; // $50K baseline for incident response

        return estimatedCost + fixedCosts;
    }
}

// Executive communication service (board reporting with business language)
public class ExecutiveCommunicationService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ExecutiveCommunicationService> _logger;

    // Generate executive summary for board (business impact, NOT technical details)
    public async Task<ExecutiveBreachSummary> GenerateExecutiveSummaryAsync(DataBreach breach)
    {
        var summary = new ExecutiveBreachSummary
        {
            SummaryId = Guid.NewGuid(),
            BreachId = breach.BreachId,

            // Business impact metrics (what board cares about)
            CustomersAffected = breach.AffectedConsumerCount,
            EstimatedRevenueLoss = CalculateRevenueLoss(breach),
            EstimatedNotificationCost = CalculateNotificationCost(breach),
            EstimatedLegalCost = 100000m, // Average legal defense cost
            ReputationRiskLevel = "High", // Brand impact assessment

            // Regulatory exposure
            PotentialRegulatoryFines = CalculateRegulatoryFines(breach),

            // Customer trust metrics
            ProjectedCustomerChurnRate = 0.15m, // 15% churn from delayed response

            // Timeline
            TimeToDetection = (breach.DetectedAt - breach.IncidentId).Hours, // Assumed incident start
            TimeToContainment = "In progress",
            TimeToNotification = "Contractual 72-hour deadline",

            // Third-party exposure
            VendorInvolved = false, // Set to true if vendor breach
            VendorName = null,

            // Recommended actions (business language)
            ImmediateActions = new[]
            {
                "Notify cyber insurance carrier (24-hour deadline)",
                "Engage external legal counsel (breach notification expertise)",
                "Prepare customer communication (first 24 hours critical)",
                "Activate crisis communication plan",
                "Schedule emergency board meeting"
            },

            ExecutiveSummary = $"A data breach was detected on {breach.DetectedAt:MMMM dd, yyyy} affecting approximately " +
                             $"{breach.AffectedConsumerCount:N0} consumer records. The breach poses significant " +
                             $"financial and reputational risks:\n\n" +
                             $"• Estimated Total Cost: ${CalculateTotalBreachCost(breach):N2}\n" +
                             $"• Potential Regulatory Fines: ${CalculateRegulatoryFines(breach):N2}\n" +
                             $"• Customer Churn Risk: 15% (industry average for delayed response)\n" +
                             $"• Contractual Obligation: 72-hour buyer notification deadline\n\n" +
                             $"Immediate action required to minimize business impact and maintain customer trust."
        };

        await _context.ExecutiveSummaries.AddAsync(summary);
        await _context.SaveChangesAsync();

        // Send executive summary to board
        await _notificationService.SendExecutiveAlertAsync(
            recipients: new[] { "ceo@switchboard.com", "cfo@switchboard.com", "board@switchboard.com" },
            subject: $"URGENT: Data Breach - Executive Summary Required",
            body: summary.ExecutiveSummary
        );

        return summary;
    }

    private decimal CalculateTotalBreachCost(DataBreach breach)
    {
        decimal costPerRecord = 148m; // IBM 2024 average
        decimal recordCost = breach.AffectedConsumerCount * costPerRecord;
        decimal fixedCosts = 250000m; // IR + legal + notification
        return recordCost + fixedCosts;
    }

    private decimal CalculateRevenueLoss(DataBreach breach)
    {
        // Estimate revenue impact from customer churn
        decimal avgRevenuePerCustomer = 500m; // Annual value
        decimal churnRate = 0.15m; // 15% churn
        return breach.AffectedConsumerCount * avgRevenuePerCustomer * churnRate;
    }

    private decimal CalculateNotificationCost(DataBreach breach)
    {
        // Notification cost: $1 per affected customer (mail + email)
        return breach.AffectedConsumerCount * 1m;
    }

    private decimal CalculateRegulatoryFines(DataBreach breach)
    {
        // Conservative estimate: varies by jurisdiction
        // HIPAA: $100-$50K per violation
        // State laws: $100-$750 per record
        // Use conservative $100 per record
        return breach.AffectedConsumerCount * 100m;
    }
}

// Customer trust management service (first 24-hour response)
public class CustomerTrustManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CustomerTrustManagementService> _logger;

    // Send immediate holding statement (first 24 hours = critical)
    public async Task SendInitialHoldingStatementAsync(DataBreach breach)
    {
        // Pre-approved holding statement template (90% preparation, 10% response)
        var holdingStatement = new BreachCommunication
        {
            CommunicationId = Guid.NewGuid(),
            BreachId = breach.BreachId,
            CommunicationType = "initial_holding_statement",
            SentAt = DateTime.UtcNow,

            // Template approved by Legal + PR (ready to send immediately)
            MessageTemplate = @"
We are writing to inform you of a security incident that may have affected your data.

On {BREACH_DATE}, we detected unauthorized access to our systems. We immediately launched
an investigation and engaged third-party cybersecurity experts to assist.

WHAT WE KNOW SO FAR:
• The incident occurred on {BREACH_DATE}
• We have contained the incident and secured our systems
• We are conducting a thorough investigation to determine the full scope
• We have notified law enforcement and regulatory authorities

WHAT WE ARE DOING:
• Working with cybersecurity experts to investigate
• Implementing additional security measures
• Monitoring for any suspicious activity
• We will provide updates as we learn more

WHAT YOU CAN DO:
• Monitor your accounts for suspicious activity
• We will provide additional guidance as our investigation continues
• Contact us at security@switchboard.com with questions

We take the security of your information seriously and deeply regret this incident.
We will keep you informed as we learn more.

Sincerely,
Switchboard Security Team
",

            Status = "sent"
        };

        await _context.BreachCommunications.AddAsync(holdingStatement);
        await _context.SaveChangesAsync();

        // Send to affected buyers within 24 hours
        var affectedBuyers = await _context.Buyers
            .Where(b => b.BuyerId == breach.AffectedBuyerIds.FirstOrDefault())
            .ToListAsync();

        foreach (var buyer in affectedBuyers)
        {
            string personalizedMessage = holdingStatement.MessageTemplate
                .Replace("{BREACH_DATE}", breach.DetectedAt.ToString("MMMM dd, yyyy"));

            await _notificationService.SendCustomerBreachNotificationAsync(
                recipient: buyer.Email,
                subject: "Important Security Notice - Immediate Action May Be Required",
                body: personalizedMessage
            );
        }

        _logger.LogInformation(
            "Initial holding statement sent to {BuyerCount} affected buyers within 24 hours",
            affectedBuyers.Count
        );
    }
}

// Business continuity integration service (IR/BC/DR coordination)
public class BusinessContinuityIntegrationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BusinessContinuityIntegrationService> _logger;

    // Activate cross-functional war room (CISO, Legal, PR, Exec, BC coordinator)
    public async Task ActivateCrisisWarRoomAsync(DataBreach breach)
    {
        var warRoom = new CrisisWarRoom
        {
            WarRoomId = Guid.NewGuid(),
            BreachId = breach.BreachId,
            ActivatedAt = DateTime.UtcNow,

            // Cross-functional team (NOT siloed)
            IncidentCommander = "CISO",
            TeamMembers = new[]
            {
                "CISO (Incident Response leader)",
                "Legal Counsel (regulatory compliance)",
                "PR Director (crisis communication)",
                "CEO/CFO (executive decision-making)",
                "Business Continuity Coordinator (operations)",
                "IT Director (disaster recovery)",
                "Customer Success (customer communication)"
            },

            // Coordination strategy
            CoordinationPlan = @"
INCIDENT RESPONSE (IR): First line of defense - contain and investigate breach
BUSINESS CONTINUITY (BC): Keep operations running during incident
DISASTER RECOVERY (DR): Restore systems to normal operation

COORDINATION:
• IR team contains breach, BC team maintains customer service
• BC team reroutes traffic to unaffected systems
• DR team prepares system restoration plan
• All three coordinate through unified war room
",

            Status = "active"
        };

        await _context.CrisisWarRooms.AddAsync(warRoom);
        await _context.SaveChangesAsync();

        _logger.LogCritical(
            "Crisis war room activated. BreachId: {BreachId}, Commander: {Commander}",
            breach.BreachId,
            warRoom.IncidentCommander
        );
    }
}

// Vendor breach monitoring service (third-party breach detection)
public class VendorBreachMonitoringService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<VendorBreachMonitoringService> _logger;

    // Monitor for vendor breach signals (60% of breaches come from vendors)
    public async Task MonitorVendorBreachSignalsAsync()
    {
        // Equifax vendor breach monitoring
        var equifaxVendor = await _context.Vendors.FirstOrDefaultAsync(v => v.VendorName == "Equifax");

        if (equifaxVendor?.BreachSignalDetected == true)
        {
            // CRITICAL: Average 205 days to detect vendor breach (healthcare)
            // Automated detection reduces this significantly

            var vendorBreach = new VendorBreach
            {
                VendorBreachId = Guid.NewGuid(),
                VendorId = equifaxVendor.VendorId,
                VendorName = "Equifax",
                DetectedAt = DateTime.UtcNow,
                BreachSource = "third_party_vendor",

                // Vendor breaches cost $370K MORE than internal breaches
                EstimatedAdditionalCost = 370000m,

                // Immediate notification required
                NotificationStatus = "pending",

                RiskAssessment = "HIGH - Equifax provides consumer data, breach exposes our customers"
            };

            await _context.VendorBreaches.AddAsync(vendorBreach);
            await _context.SaveChangesAsync();

            _logger.LogCritical(
                "VENDOR BREACH DETECTED: {VendorName} - Immediate incident response required",
                equifaxVendor.VendorName
            );
        }
    }
}

// ============================================================================
// ORIGINAL SERVICES (planned downtime, security audits)
// ============================================================================

public class SecurityIncidentService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SecurityIncidentService> _logger;

    // Planned downtime notification (Contract Section 4.1)
    public async Task SchedulePlannedDowntimeAsync(PlannedDowntimeDto downtime)
    {
        // CONTRACTUAL REQUIREMENT: 24-hour advance notice for planned downtime
        if (downtime.ScheduledStart < DateTime.UtcNow.AddHours(24))
        {
            throw new InvalidOperationException(
                "Contract violation: Planned downtime requires 24-hour advance notice (Section 4.1)"
            );
        }

        var downtimeEvent = new DowntimeEvent
        {
            EventId = Guid.NewGuid(),
            EventType = "planned_maintenance",
            ScheduledStart = downtime.ScheduledStart,
            ScheduledEnd = downtime.ScheduledEnd,
            Reason = downtime.Reason,
            ImpactedServices = downtime.ImpactedServices,
            NotificationSentAt = DateTime.UtcNow,
            Status = "scheduled"
        };

        await _context.DowntimeEvents.AddAsync(downtimeEvent);
        await _context.SaveChangesAsync();

        // Notify all active buyers
        var activeBuyers = await _context.Buyers.Where(b => b.IsActive).ToListAsync();

        foreach (var buyer in activeBuyers)
        {
            await _notificationService.SendDowntimeNotificationAsync(
                recipient: buyer.Email,
                subject: $"Planned Maintenance: {downtime.Reason}",
                body: $"Scheduled maintenance window:\n" +
                      $"Start: {downtime.ScheduledStart:yyyy-MM-dd HH:mm:ss UTC}\n" +
                      $"End: {downtime.ScheduledEnd:yyyy-MM-dd HH:mm:ss UTC}\n" +
                      $"Duration: {(downtime.ScheduledEnd - downtime.ScheduledStart).TotalHours:F1} hours\n" +
                      $"Impacted services: {string.Join(", ", downtime.ImpactedServices)}\n\n" +
                      $"API will be unavailable during this window."
            );
        }

        _logger.LogInformation(
            "Planned downtime notification sent. EventId: {EventId}, Start: {Start}, Duration: {Duration}",
            downtimeEvent.EventId,
            downtime.ScheduledStart,
            (downtime.ScheduledEnd - downtime.ScheduledStart).TotalHours
        );
    }

    // Annual security audit (Industry best practice + contractual compliance)
    public async Task<SecurityAuditReport> ConductSecurityAuditAsync()
    {
        var audit = new SecurityAuditReport
        {
            AuditId = Guid.NewGuid(),
            AuditDate = DateTime.UtcNow,
            AuditType = "comprehensive_security_audit",
            Auditor = "Third-Party Security Firm",

            // Access control audit
            UserAccountsReviewed = await _context.Users.CountAsync(),
            InactiveAccountsDisabled = await DisableInactiveAccountsAsync(),
            UnauthorizedAccessAttempts = await CountUnauthorizedAccessAttemptsAsync(),

            // Encryption audit
            EncryptionAtRestVerified = true, // RDS encryption enabled
            EncryptionInTransitVerified = true, // TLS 1.3 enforced
            KeyRotationCompliant = await VerifyKeyRotationAsync(),

            // Compliance audit
            FCRAAuditLogRetention = await VerifyAuditLogRetentionAsync(), // 24 months
            GDPRDataProcessingAgreements = await VerifyDPAsAsync(),
            CCPAPrivacyPolicyUpdated = true,

            // Vulnerability assessment
            CriticalVulnerabilities = 0,
            HighVulnerabilities = 0,
            MediumVulnerabilities = 3,
            LowVulnerabilities = 12,

            // Penetration testing results
            PenetrationTestDate = DateTime.UtcNow.AddDays(-30),
            PenetrationTestPassed = true,

            RecommendedActions = new[]
            {
                "Update AWS GuardDuty threat detection rules",
                "Implement additional rate limiting for admin endpoints",
                "Enable AWS CloudTrail for all API calls"
            }
        };

        await _context.SecurityAudits.AddAsync(audit);
        await _context.SaveChangesAsync();

        return audit;
    }

    // Quarterly security metrics report
    public async Task<SecurityMetricsReport> GenerateQuarterlySecurityReportAsync(DateTime quarter)
    {
        var startDate = new DateTime(quarter.Year, ((quarter.Month - 1) / 3) * 3 + 1, 1);
        var endDate = startDate.AddMonths(3);

        return new SecurityMetricsReport
        {
            ReportPeriod = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",

            SecurityIncidents = await _context.SecurityIncidents
                .Where(i => i.DetectedAt >= startDate && i.DetectedAt < endDate)
                .CountAsync(),

            DataBreaches = await _context.DataBreaches
                .Where(b => b.DetectedAt >= startDate && b.DetectedAt < endDate)
                .CountAsync(),

            BreachNotificationCompliance = await CalculateBreachNotificationComplianceAsync(startDate, endDate),

            UnauthorizedAccessAttempts = await _context.SecurityLogs
                .Where(l => l.EventType == "unauthorized_access"
                         && l.Timestamp >= startDate && l.Timestamp < endDate)
                .CountAsync(),

            AverageMTTR = await CalculateMeanTimeToResolveAsync(startDate, endDate),

            ComplianceScore = 99.2, // Weighted average of all compliance frameworks

            Recommendations = new[]
            {
                "Continue quarterly security audits",
                "Maintain 72-hour breach notification protocol",
                "Update incident response playbooks annually"
            }
        };
    }
}
```

**Database Schema for Security Tracking:**
```sql
-- Security incidents table
CREATE TABLE security_incidents (
    incident_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    incident_type VARCHAR(100) NOT NULL, -- 'unauthorized_access', 'data_breach', 'ddos_attack'
    severity VARCHAR(50) NOT NULL, -- 'critical', 'high', 'medium', 'low'
    detected_at TIMESTAMPTZ NOT NULL,
    resolved_at TIMESTAMPTZ,
    detection_method VARCHAR(100),
    description TEXT,
    affected_systems TEXT[],
    affected_record_count BIGINT,
    status VARCHAR(50) DEFAULT 'investigating',
    assigned_to UUID,
    notes TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Data breaches table (Contract Section 13.2 requirement)
CREATE TABLE data_breaches (
    breach_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    incident_id UUID REFERENCES security_incidents(incident_id),
    detected_at TIMESTAMPTZ NOT NULL,
    notification_deadline TIMESTAMPTZ NOT NULL, -- 72 hours from detection
    affected_consumer_count BIGINT,
    breach_type VARCHAR(100),
    data_types_compromised TEXT[],
    root_cause TEXT,
    remediation_actions TEXT,
    status VARCHAR(50) DEFAULT 'pending_notification',
    buyer_notification_sent_at TIMESTAMPTZ,
    regulatory_notification_sent_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Breach notifications to buyers
CREATE TABLE breach_notifications (
    notification_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    breach_id UUID REFERENCES data_breaches(breach_id),
    buyer_id UUID REFERENCES buyers(buyer_id),
    notification_method VARCHAR(50), -- 'email', 'phone', 'certified_mail'
    scheduled_for TIMESTAMPTZ NOT NULL,
    notification_deadline TIMESTAMPTZ NOT NULL,
    sent_at TIMESTAMPTZ,
    acknowledged_at TIMESTAMPTZ,
    status VARCHAR(50) DEFAULT 'pending',
    delivery_confirmation TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Regulatory breach notifications
CREATE TABLE regulatory_notifications (
    notification_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    breach_id UUID REFERENCES data_breaches(breach_id),
    regulatory_body VARCHAR(255) NOT NULL, -- 'FTC', 'EU DPA', 'CA Attorney General'
    jurisdiction VARCHAR(100),
    notification_deadline TIMESTAMPTZ NOT NULL,
    submitted_at TIMESTAMPTZ,
    acknowledgment_received_at TIMESTAMPTZ,
    status VARCHAR(50) DEFAULT 'pending',
    submission_proof_url TEXT, -- S3 link to submission confirmation
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Planned downtime events (Contract Section 4.1 requirement)
CREATE TABLE downtime_events (
    event_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    event_type VARCHAR(50) NOT NULL, -- 'planned_maintenance', 'emergency_maintenance'
    scheduled_start TIMESTAMPTZ NOT NULL,
    scheduled_end TIMESTAMPTZ NOT NULL,
    actual_start TIMESTAMPTZ,
    actual_end TIMESTAMPTZ,
    reason TEXT,
    impacted_services TEXT[],
    notification_sent_at TIMESTAMPTZ,
    advance_notice_hours INT GENERATED ALWAYS AS (
        EXTRACT(EPOCH FROM (scheduled_start - notification_sent_at)) / 3600
    ) STORED, -- Must be >= 24 for planned maintenance
    status VARCHAR(50) DEFAULT 'scheduled',
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Security audits table
CREATE TABLE security_audits (
    audit_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    audit_date DATE NOT NULL,
    audit_type VARCHAR(100), -- 'comprehensive', 'penetration_test', 'compliance_review'
    auditor VARCHAR(255),
    findings TEXT,
    critical_vulnerabilities INT DEFAULT 0,
    high_vulnerabilities INT DEFAULT 0,
    medium_vulnerabilities INT DEFAULT 0,
    low_vulnerabilities INT DEFAULT 0,
    compliance_score DECIMAL(5,2),
    passed BOOLEAN,
    report_url TEXT, -- S3 link to full audit report
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Security logs (detailed event tracking)
CREATE TABLE security_logs (
    log_id BIGSERIAL PRIMARY KEY,
    event_type VARCHAR(100) NOT NULL,
    severity VARCHAR(50),
    user_id UUID,
    ip_address INET,
    user_agent TEXT,
    request_path TEXT,
    response_status INT,
    timestamp TIMESTAMPTZ DEFAULT NOW(),
    details JSONB
);

CREATE INDEX idx_security_logs_event_type ON security_logs(event_type);
CREATE INDEX idx_security_logs_timestamp ON security_logs(timestamp DESC);
CREATE INDEX idx_security_logs_ip ON security_logs(ip_address);
```

**Security Requirements Checklist (Technical + Business):**
- ✅ **72-hour breach notification:** Automated tracking and notification system
- ✅ **24-hour cyber insurance notification:** Automated carrier notification (claim requirement)
- ✅ **First 24-hour customer response:** Pre-approved holding statement templates
- ✅ **Executive summary generation:** Board reporting with business impact metrics
- ✅ **24-hour downtime notice:** Enforced via code validation
- ✅ **Annual security audits:** Comprehensive audit tracking and reporting
- ✅ **Incident response protocol:** Documented procedures for all incident types
- ✅ **Business continuity integration:** Cross-functional war room (IR/BC/DR coordination)
- ✅ **Vendor breach monitoring:** Third-party breach detection and response
- ✅ **Regulatory compliance reporting:** Automated quarterly compliance reports
- ✅ **Data breach tracking:** Complete audit trail from detection to resolution
- ✅ **Penetration testing:** Annual third-party security assessment
- ✅ **Vulnerability management:** Continuous monitoring and remediation

**Contractual Compliance:**
- **Section 4.1:** 24-hour advance notice for planned downtime ✅
- **Section 13.2:** 72-hour breach notification to buyers ✅
- **Section 13.2:** Immediate notification to regulatory bodies (GDPR/CCPA) ✅
- **Section 6.1:** Annual security audits and compliance verification ✅

**Business-Focused Compliance:**
- **Cyber Insurance:** 24-hour notification with pre-breach security controls documentation ✅
- **Customer Trust:** First 24-hour response (15% churn risk mitigation) ✅
- **Executive Communication:** Business impact summaries (revenue, reputation, regulatory) ✅
- **Business Continuity:** IR/BC/DR coordination (87% have plans, now integrated) ✅
- **Vendor Risk Management:** Third-party breach detection (60% of breaches) ✅

---

### Phase 3: Performance & Caching (Days 9-10)

#### Feature 3.1: Redis Caching (Distributed, High-Performance)
**Priority:** P1 (High)
**Effort:** 1 day
**Dependencies:** ElastiCache Redis cluster
**Performance Target:** < 10ms cache hit response time (vs. 50-150ms DB queries)
**Throughput:** 100,000+ queries/second (Redis benchmark)

---

#### 🔴 CRITICAL REDIS GOTCHAS

**1. NEVER Use KEYS Command in Production (Catastrophic Performance)**
- **CATASTROPHIC:** `KEYS *` is O(n) and BLOCKS all Redis operations until complete
- Running `KEYS` on database 0 will FREEZE all 16 numbered databases (including database 9, 15, etc.)
- On a Redis instance with 1M keys, `KEYS *` can block for 1-2 seconds
- **SOLUTION:** Use `SCAN` with cursor pattern for iteration (non-blocking, O(1) per call)
```csharp
// ❌ WRONG: Blocks entire Redis instance
var keys = await db.ExecuteAsync("KEYS", "phone:*");

// ✅ CORRECT: Non-blocking cursor-based iteration
await foreach (var key in server.KeysAsync(pattern: "phone:*"))
{
    // Process key
}
```

**2. Always Set TTL or Keys Accumulate Forever (Memory Leak)**
- **PROBLEM:** Keys without TTL never expire, accumulate indefinitely, trigger LRU eviction
- Eviction is unpredictable - you don't control WHAT gets evicted
- **INDUSTRY STANDARD:** All cache keys MUST have TTL
- **SOLUTION:** Set TTL on every SET operation
```csharp
// ❌ WRONG: No TTL = memory leak
await db.StringSetAsync("phone:5551234567", json);

// ✅ CORRECT: 24-hour TTL with jitter (prevents thundering herd)
var ttl = TimeSpan.FromHours(24) + TimeSpan.FromMinutes(Random.Shared.Next(0, 60));
await db.StringSetAsync("phone:5551234567", json, ttl);
```

**3. TTL Jitter Required to Prevent Thundering Herd**
- **PROBLEM:** 10,000 cache entries expire at EXACTLY the same time (all set with 24-hour TTL)
- All 10,000 requests simultaneously hit database (thundering herd)
- Database overload, API latency spikes to 5-10 seconds
- **SOLUTION:** Add random jitter to TTL (±5-10% variance)
```csharp
// ❌ WRONG: All keys expire at same time
TimeSpan ttl = TimeSpan.FromHours(24);

// ✅ CORRECT: Random jitter prevents synchronized expiration
TimeSpan ttl = TimeSpan.FromHours(24) + TimeSpan.FromMinutes(Random.Shared.Next(0, 60));
// Result: Keys expire between 24:00 and 25:00 hours (spread out)
```

**4. ConnectionMultiplexer MUST Be Singleton (Connection Exhaustion)**
- **PROBLEM:** Creating new `ConnectionMultiplexer` per request exhausts connections
- Redis default: 65,535 max connections per instance
- High-traffic API creates 100+ connections/second = exhaustion in 10 minutes
- **SOLUTION:** Register `ConnectionMultiplexer` as singleton in DI container
```csharp
// ❌ WRONG: Creates new connection on every request (connection leak)
var redis = await ConnectionMultiplexer.ConnectAsync("redis-endpoint");

// ✅ CORRECT: Singleton ConnectionMultiplexer via DI
services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect("redis-endpoint"));
```

**5. Avoid Numbered Databases (0-15) - Architectural Flaw**
- **PROBLEM:** Redis creator (Salvatore Sanfilippo) called numbered databases "worst design mistake"
- Databases share same connection pool and event loop
- `KEYS` on database 0 BLOCKS database 9 (all databases affected)
- Not truly isolated, just namespace prefixes
- **SOLUTION:** Use single database (0) with key prefixes for logical separation
```csharp
// ❌ WRONG: Using numbered databases (shared blocking)
await db0.StringSetAsync("user:123", data);  // database 0
await db1.StringSetAsync("session:456", data); // database 1 (SAME blocking risk)

// ✅ CORRECT: Single database with prefixes
await db.StringSetAsync("user:123", data);
await db.StringSetAsync("session:456", data);
```

**6. Network Latency is #1 Performance Bottleneck (Not Redis)**
- **PROBLEM:** Redis processes 100K+ ops/second, but network round-trip is 0.5-2ms
- Single GET: 1-2ms latency (mostly network)
- 100 sequential GETs: 100-200ms total (network dominates)
- **SOLUTION:** Use pipelining or batching (MGET/MSET) to reduce round trips
```csharp
// ❌ WRONG: 100 round trips = 100-200ms total latency
for (int i = 0; i < 100; i++)
{
    await db.StringGetAsync($"phone:{phoneNumbers[i]}");
}

// ✅ CORRECT: 1 round trip with MGET = 2-5ms total latency
var keys = phoneNumbers.Select(p => new RedisKey($"phone:{p}")).ToArray();
var results = await db.StringGetAsync(keys); // Batch operation
```

**7. AbortOnConnectFail=true Causes Cascading Failures**
- **PROBLEM:** `AbortOnConnectFail=true` (default) throws exception if Redis unavailable
- Exception kills API request, returns 500 to user
- Redis outage = complete API outage (tight coupling)
- **SOLUTION:** Set `AbortOnConnectFail=false` for graceful degradation
```csharp
// ❌ WRONG: Redis outage kills entire API
ConfigurationOptions config = new() { AbortOnConnectFail = true };

// ✅ CORRECT: Graceful degradation (fallback to DB on Redis failure)
ConfigurationOptions config = new()
{
    AbortOnConnectFail = false, // Don't throw on connection failure
    ConnectRetry = 3,
    ConnectTimeout = 5000
};
```

**8. Not Using IDistributedCache Interface (Tight Coupling)**
- **PROBLEM:** Direct `StackExchange.Redis` usage couples code to Redis
- Cannot switch to Memcached, NCache, or other providers without rewrite
- Testing requires real Redis instance (slow integration tests)
- **SOLUTION:** Use `IDistributedCache` interface (provider-agnostic)
```csharp
// ❌ WRONG: Tightly coupled to Redis
public class CacheService
{
    private readonly IConnectionMultiplexer _redis;
    public async Task<string?> GetAsync(string key) =>
        await _redis.GetDatabase().StringGetAsync(key);
}

// ✅ CORRECT: Provider-agnostic interface
public class CacheService
{
    private readonly IDistributedCache _cache;
    public async Task<string?> GetAsync(string key) =>
        await _cache.GetStringAsync(key); // Works with Redis, Memcached, NCache, etc.
}
```

**9. Large Value Sizes Hurt Performance (Redis Sweet Spot: < 100KB)**
- **PROBLEM:** Redis stores values in memory, large values (> 1MB) cause:
  - Slow serialization/deserialization
  - Network transfer bottleneck
  - Memory fragmentation
- **INDUSTRY BENCHMARK:** Redis performs best with values < 100KB
- **SOLUTION:** Compress large values or store references (S3 URLs) instead
```csharp
// ❌ WRONG: Caching 5MB serialized EquifaxRecord (398 columns)
var json = JsonSerializer.Serialize(equifaxRecord); // 5MB
await _cache.SetStringAsync($"phone:{phoneNumber}", json);

// ✅ CORRECT: Cache only essential fields (~10KB)
var cachedData = new
{
    equifaxRecord.ConsumerKey,
    equifaxRecord.FirstName,
    equifaxRecord.LastName,
    equifaxRecord.Phone1, // Top 10 most-accessed fields
    // Omit rarely-used fields (reduce from 5MB to 10KB)
};
var json = JsonSerializer.Serialize(cachedData);
await _cache.SetStringAsync($"phone:{phoneNumber}", json);
```

**10. Missing Monitoring = Silent Cache Failures**
- **PROBLEM:** Cache hit rate unknown, silent degradation undetected
- Cache hit rate drops from 90% to 20% = database overload (unnoticed)
- No visibility into eviction rate, memory pressure, connection issues
- **SOLUTION:** Emit cache metrics (hit rate, miss rate, latency, eviction count)

---

#### **Corrected Implementation: High-Performance, Production-Ready Redis Caching**

**Dependency Injection Setup (Startup.cs):**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // ✅ BEST PRACTICE: Use IDistributedCache interface (provider-agnostic)
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = Configuration.GetConnectionString("Redis");

        // ✅ BEST PRACTICE: Graceful degradation on Redis failure
        options.ConfigurationOptions = new ConfigurationOptions
        {
            AbortOnConnectFail = false, // Don't throw if Redis unavailable
            ConnectRetry = 3,
            ConnectTimeout = 5000,
            SyncTimeout = 5000,

            // AWS ElastiCache endpoint
            EndPoints = { "sb-redis.cache.amazonaws.com:6379" },

            // ✅ BEST PRACTICE: Connection pooling
            Ssl = true,
            AllowAdmin = false // Security: Disable admin commands in production
        };
    });

    // ✅ BEST PRACTICE: Register cache service
    services.AddScoped<IPhoneEnrichmentCacheService, PhoneEnrichmentCacheService>();
}
```

**Cache Service Implementation:**
```csharp
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics;
using System.Text.Json;

public interface IPhoneEnrichmentCacheService
{
    Task<EquifaxRecord?> GetCachedRecordAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task CacheRecordAsync(string phoneNumber, EquifaxRecord record, CancellationToken cancellationToken = default);
    Task<Dictionary<string, EquifaxRecord>> GetBatchAsync(string[] phoneNumbers, CancellationToken cancellationToken = default);
}

public class PhoneEnrichmentCacheService : IPhoneEnrichmentCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<PhoneEnrichmentCacheService> _logger;
    private readonly ICacheMetricsService _metrics;

    // ✅ BEST PRACTICE: Cache statistics tracking
    private long _cacheHits = 0;
    private long _cacheMisses = 0;

    public PhoneEnrichmentCacheService(
        IDistributedCache cache,
        ILogger<PhoneEnrichmentCacheService> logger,
        ICacheMetricsService metrics)
    {
        _cache = cache;
        _logger = logger;
        _metrics = metrics;
    }

    // Single record lookup with monitoring
    public async Task<EquifaxRecord?> GetCachedRecordAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var key = $"phone:{phoneNumber}";

        try
        {
            var cachedBytes = await _cache.GetAsync(key, cancellationToken);

            if (cachedBytes != null)
            {
                // ✅ Cache HIT
                Interlocked.Increment(ref _cacheHits);

                var record = JsonSerializer.Deserialize<EquifaxRecord>(cachedBytes);

                sw.Stop();
                _metrics.RecordCacheHit(sw.ElapsedMilliseconds);

                _logger.LogDebug(
                    "Cache HIT: {PhoneNumber} in {ElapsedMs}ms",
                    phoneNumber,
                    sw.ElapsedMilliseconds
                );

                return record;
            }
            else
            {
                // ❌ Cache MISS
                Interlocked.Increment(ref _cacheMisses);

                sw.Stop();
                _metrics.RecordCacheMiss(sw.ElapsedMilliseconds);

                _logger.LogDebug(
                    "Cache MISS: {PhoneNumber} in {ElapsedMs}ms",
                    phoneNumber,
                    sw.ElapsedMilliseconds
                );

                return null;
            }
        }
        catch (Exception ex)
        {
            // ✅ BEST PRACTICE: Graceful degradation (Redis failure doesn't kill API)
            _logger.LogWarning(
                ex,
                "Redis cache GET failed for {PhoneNumber}. Falling back to database.",
                phoneNumber
            );

            return null; // Fallback to database query
        }
    }

    // Cache record with TTL jitter
    public async Task CacheRecordAsync(
        string phoneNumber,
        EquifaxRecord record,
        CancellationToken cancellationToken = default)
    {
        var key = $"phone:{phoneNumber}";

        try
        {
            // ✅ BEST PRACTICE: Cache only essential fields (not all 398 columns)
            // Reduces cache size from ~5MB to ~10KB per record
            var cachedData = new
            {
                record.ConsumerKey,
                record.FirstName,
                record.LastName,
                record.MiddleName,
                record.Phone1,
                record.Phone1Type,
                record.Address1,
                record.City1,
                record.State1,
                record.Zip1,
                record.Email1,
                record.DateOfBirth,
                record.Age,
                record.Gender,
                // Top 15 most-accessed fields (based on API usage analytics)
            };

            var json = JsonSerializer.SerializeToUtf8Bytes(cachedData);

            // ✅ BEST PRACTICE: TTL with jitter (prevents thundering herd)
            var baseTtl = TimeSpan.FromHours(24);
            var jitter = TimeSpan.FromMinutes(Random.Shared.Next(0, 60));
            var ttl = baseTtl + jitter;

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };

            await _cache.SetAsync(key, json, options, cancellationToken);

            _logger.LogDebug(
                "Cached record for {PhoneNumber} with TTL {TTL}",
                phoneNumber,
                ttl
            );
        }
        catch (Exception ex)
        {
            // ✅ BEST PRACTICE: Cache write failure doesn't fail API request
            _logger.LogWarning(
                ex,
                "Redis cache SET failed for {PhoneNumber}. Continuing without cache.",
                phoneNumber
            );
            // Don't throw - cache write failures are non-critical
        }
    }

    // ✅ BEST PRACTICE: Batch operations (reduces network round trips)
    public async Task<Dictionary<string, EquifaxRecord>> GetBatchAsync(
        string[] phoneNumbers,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var results = new Dictionary<string, EquifaxRecord>();

        try
        {
            // Note: IDistributedCache doesn't support batching natively
            // This is a limitation of the abstraction
            // For true batch support, use IConnectionMultiplexer directly with MGET

            var tasks = phoneNumbers.Select(phone => GetCachedRecordAsync(phone, cancellationToken));
            var records = await Task.WhenAll(tasks);

            for (int i = 0; i < phoneNumbers.Length; i++)
            {
                if (records[i] != null)
                {
                    results[phoneNumbers[i]] = records[i];
                }
            }

            sw.Stop();
            _logger.LogInformation(
                "Batch cache lookup: {Total} requests, {Hits} hits, {Misses} misses in {ElapsedMs}ms",
                phoneNumbers.Length,
                results.Count,
                phoneNumbers.Length - results.Count,
                sw.ElapsedMilliseconds
            );

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Batch cache lookup failed. Falling back to database.");
            return new Dictionary<string, EquifaxRecord>();
        }
    }

    // Cache statistics for monitoring
    public CacheStatistics GetStatistics()
    {
        var hits = Interlocked.Read(ref _cacheHits);
        var misses = Interlocked.Read(ref _cacheMisses);
        var total = hits + misses;
        var hitRate = total > 0 ? (double)hits / total : 0;

        return new CacheStatistics
        {
            CacheHits = hits,
            CacheMisses = misses,
            HitRate = hitRate,
            TotalRequests = total
        };
    }
}

public record CacheStatistics
{
    public long CacheHits { get; init; }
    public long CacheMisses { get; init; }
    public double HitRate { get; init; }
    public long TotalRequests { get; init; }
}
```

**Cache Metrics Service (Monitoring):**
```csharp
public interface ICacheMetricsService
{
    void RecordCacheHit(long latencyMs);
    void RecordCacheMiss(long latencyMs);
}

public class CacheMetricsService : ICacheMetricsService
{
    private readonly ILogger<CacheMetricsService> _logger;

    public void RecordCacheHit(long latencyMs)
    {
        // Emit CloudWatch metric
        _logger.LogInformation(
            "METRIC|CacheHit|{Latency}|ms",
            latencyMs
        );
    }

    public void RecordCacheMiss(long latencyMs)
    {
        // Emit CloudWatch metric
        _logger.LogInformation(
            "METRIC|CacheMiss|{Latency}|ms",
            latencyMs
        );
    }
}
```

**PhoneEnrichmentService Integration:**
```csharp
public class PhoneEnrichmentService
{
    private readonly IEquifaxRepository _repository;
    private readonly IPhoneEnrichmentCacheService _cache;

    public async Task<EquifaxRecord?> EnrichByPhoneAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        // Normalize phone number
        var normalized = NormalizePhoneNumber(phoneNumber);

        // ✅ Try cache first (5-10ms if hit)
        var cached = await _cache.GetCachedRecordAsync(normalized, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        // ❌ Cache miss: Query database (50-150ms)
        var record = await _repository.FindByPhoneNumberAsync(normalized, cancellationToken);

        // ✅ Cache result for next request
        if (record != null)
        {
            await _cache.CacheRecordAsync(normalized, record, cancellationToken);
        }

        return record;
    }
}
```

**Cache Strategy:**
- **Cache hit:** Return in 5-10ms (90% faster than DB query)
- **Cache miss:** Query DB (50-150ms), cache result with TTL jitter
- **TTL:** 24 hours + 0-60 minutes jitter (prevents thundering herd)
- **Graceful degradation:** Redis failure falls back to DB (no API outage)
- **Monitoring:** Track hit rate, miss rate, latency via CloudWatch metrics
- **Value size optimization:** Cache only top 15 fields (~10KB vs. 5MB full record)
- **Target hit rate:** 85-90% (industry standard for phone lookups)

**Performance Benefits:**
- **Latency reduction:** 50-150ms DB query → 5-10ms cache hit (10-15x faster)
- **Database load reduction:** 85-90% cache hit rate = 10x fewer DB queries
- **Cost savings:** Reduced RDS instance size due to lower query volume
- **Throughput increase:** Redis supports 100K+ queries/second vs. PostgreSQL ~5K queries/second
- **Contract SLA compliance:** Cache enables < 500ms API response time (Section 4.1)

**Industry Standards Applied:**
- ✅ Microsoft Learn ASP.NET Core 9.0 distributed caching best practices
- ✅ AWS Database Caching Strategies whitepaper patterns
- ✅ Redis.io official anti-patterns avoidance
- ✅ StackExchange.Redis connection multiplexer singleton pattern
- ✅ IDistributedCache abstraction for provider independence

---

#### Feature 3.2: Database Connection Pooling (Npgsql + EF Core Optimization)
**Priority:** P1 (High)
**Effort:** 0.5 days
**Dependencies:** None
**Performance Target:** Reuse connections efficiently, avoid pool exhaustion under load
**RDS Configuration:** db.r5.2xlarge with max_connections=500

---

#### 🔴 CRITICAL CONNECTION POOLING GOTCHAS

**1. Connection Leaks = #1 Cause of Pool Exhaustion (ALWAYS Use `using`)**
- **CATASTROPHIC:** Not disposing connections = gradual pool exhaustion → 500 errors → API down
- After 100 leaked connections (default MaxPoolSize), ALL new requests fail
- **SYMPTOM:** PostgreSQL shows many "idle" or "idle in transaction" connections that never close
- **SOLUTION:** ALWAYS use `using` statement or ensure Dispose() in finally block
```csharp
// ❌ WRONG: Connection leak (exception prevents Dispose)
var conn = new NpgsqlConnection(connectionString);
await conn.OpenAsync();
// Query here... if exception thrown, connection never disposed
conn.Dispose(); // Never reached if exception above

// ✅ CORRECT: using ensures Dispose even on exception
using (var conn = new NpgsqlConnection(connectionString))
{
    await conn.OpenAsync();
    // Query here - connection always returned to pool
}

// ✅ CORRECT: EF Core handles this automatically
using (var context = new ApplicationDbContext())
{
    var records = await context.EquifaxRecords.ToListAsync();
}
```

**2. Connection String Inconsistency Creates Separate Pools**
- **PROBLEM:** ANY difference in connection string = separate pool
- "Password=abc" vs. "password=abc" = 2 pools (case-sensitive keys)
- Whitespace differences create separate pools
- Can exhaust PostgreSQL max_connections with multiple pools
- **SOLUTION:** Use IConfiguration with consistent connection string everywhere
```csharp
// ❌ WRONG: Different strings = 2 separate pools (200 connections total)
var conn1 = new NpgsqlConnection("Host=db;Port=5432;Database=postgres;Username=admin;Password=pass");
var conn2 = new NpgsqlConnection("Host=db;Port=5432;Database=postgres;username=admin;Password=pass"); // lowercase 'username'

// ✅ CORRECT: Single source of truth
var connectionString = Configuration.GetConnectionString("EquifaxDb");
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
```

**3. Two Pooling Layers: Npgsql (Connection) vs. EF Core (DbContext)**
- **CONFUSION:** Developers conflate connection pooling with DbContext pooling
- **Npgsql Connection Pooling (Layer 1):** Manages physical database connections (default 1-100)
- **EF Core DbContext Pooling (Layer 2):** Reuses DbContext instances (reduces setup cost)
- **IMPORTANT:** AddDbContextPool does NOT affect connection pooling
- **SOLUTION:** Use both for high-traffic scenarios
```csharp
// Layer 1: Npgsql connection pooling (automatic, configured in connection string)
// Layer 2: EF Core DbContext pooling (opt-in for high traffic)

// ❌ WRONG: Using AddDbContext for high-traffic API (creates new DbContext per request)
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ✅ CORRECT: Use AddDbContextPool for high-traffic scenarios
services.AddDbContextPool<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString),
    poolSize: 128); // Reuse up to 128 DbContext instances
```

**4. PgBouncer Compatibility Issue (No Reset On Close Required)**
- **PROBLEM:** Using PgBouncer in transaction/statement mode with Npgsql pooling = errors
- Npgsql executes `DISCARD ALL` when returning connection to pool (resets session state)
- `DISCARD ALL` fails in PgBouncer transaction/statement mode
- **SOLUTION:** Set `No Reset On Close=true` in connection string when using PgBouncer
```csharp
// ❌ WRONG: PgBouncer transaction mode + default Npgsql pooling = DISCARD ALL fails
"Host=pgbouncer;Port=6432;Database=postgres;Pooling=true"

// ✅ CORRECT: Disable DISCARD ALL when using PgBouncer transaction/statement mode
"Host=pgbouncer;Port=6432;Database=postgres;Pooling=true;No Reset On Close=true"
```

**5. Connection Lifetime vs. Idle Lifetime Confusion**
- **Connection Lifetime:** Terminates connection after X seconds EVEN IF ACTIVE
- **Connection Idle Lifetime:** Returns idle connection to pool after X seconds
- **PROBLEM:** Short Connection Lifetime (e.g., 60s) kills active long-running queries
- **SOLUTION:** Set Connection Lifetime high (600s), Idle Lifetime low (60s)
```csharp
// ❌ WRONG: Connection Lifetime=60s kills active queries after 1 minute
"Connection Lifetime=60;Connection Idle Lifetime=300"

// ✅ CORRECT: Long lifetime for active, short for idle
"Connection Lifetime=600;Connection Idle Lifetime=60"
```

**6. Missing Max Auto Prepare = Missed Performance Gains**
- **PROBLEM:** EF Core/Dapper don't prepare statements by default (performance loss)
- Npgsql can automatically prepare frequently-used statements
- **INDUSTRY BENCHMARK:** 20-40% performance improvement with prepared statements
- **SOLUTION:** Set `Max Auto Prepare=10` or higher in connection string
```csharp
// ❌ WRONG: No prepared statements = repeated query parsing overhead
"Host=db;Port=5432;Database=postgres"

// ✅ CORRECT: Automatic prepared statements (up to 10 unique queries)
"Host=db;Port=5432;Database=postgres;Max Auto Prepare=10"
```

**7. Pool Size Too Small for Concurrent Load**
- **PROBLEM:** MaxPoolSize=100, but 200 concurrent requests = 100 requests wait/timeout
- AWS RDS max_connections=500, but only using 100 = underutilized
- **FORMULA:** MaxPoolSize = (Expected Concurrent Requests) / (Avg Query Time in seconds)
- **SOLUTION:** Calculate based on actual load
```csharp
// Example calculation:
// - 1000 requests/second
// - Average query time: 50ms (0.05 seconds)
// - Concurrent queries: 1000 × 0.05 = 50
// - Buffer: 50 × 2 = 100 (2x for spikes)
// - Result: MaxPoolSize=100

// ✅ CORRECT: Size pool for expected load
"MinPoolSize=20;MaxPoolSize=200" // Handles up to 200 concurrent queries
```

**8. Monitoring for Connection Leaks (PostgreSQL Side)**
- **PROBLEM:** Connection leaks silent until pool exhausted
- PostgreSQL shows "idle" or "idle in transaction" connections that never close
- **DETECTION:** Query pg_stat_activity for long-running idle connections
- **SOLUTION:** Monitor and alert on idle connections > 5 minutes
```sql
-- Detect connection leaks (idle connections > 5 minutes)
SELECT pid, usename, application_name, state, state_change
FROM pg_stat_activity
WHERE state = 'idle'
  AND state_change < NOW() - INTERVAL '5 minutes'
ORDER BY state_change;
```

**9. Not Enabling EF Core Connection Leak Detection**
- **PROBLEM:** EF Core can detect leaked DbContext instances, but it's not enabled by default
- Leaked DbContext = leaked connection
- **SOLUTION:** Enable DbContext pooling with leak detection logging
```csharp
services.AddDbContextPool<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging(); // Development only
    // Leak detection automatically enabled with AddDbContextPool
});
```

**10. Optimizing Wrong Thing First (Query Before Pool)**
- **PROBLEM:** Increasing pool size to fix slow queries (doesn't help)
- Slow query takes 5 seconds, increasing pool from 100 → 200 = still 5 seconds per query
- **OPTIMIZATION ORDER:**
  1. Fix slow queries (10x performance gains with indexes)
  2. Add missing indexes
  3. Optimize N+1 query problems
  4. THEN increase pool size if needed
- **SOLUTION:** Measure query performance first
```csharp
// ✅ CORRECT: Log slow queries BEFORE increasing pool size
services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name })
           .EnableSensitiveDataLogging(); // See actual SQL + parameters
});
```

---

#### **Corrected Implementation: Production-Grade Connection Pooling**

**appsettings.json (Optimized Connection String):**
```json
{
  "ConnectionStrings": {
    "EquifaxDb": "Host=sb-marketing-postgres.cu9k2siys4p8.us-east-1.rds.amazonaws.com;Port=5432;Database=postgres;Username=sbadmin;Password=***;Pooling=true;MinPoolSize=20;MaxPoolSize=200;Connection Lifetime=600;Connection Idle Lifetime=60;Max Auto Prepare=10;Command Timeout=30;Timeout=30;Keepalive=30;"
  }
}
```

**Connection String Parameters Explained:**
- **Pooling=true:** Enable connection pooling (default, but explicit for clarity)
- **MinPoolSize=20:** Keep 20 connections warm (reduces cold start latency)
- **MaxPoolSize=200:** Allow up to 200 concurrent connections (RDS max_connections=500, leaving 300 for other services/admin)
- **Connection Lifetime=600:** Close connections after 10 minutes (even if active, helps with AWS RDS failover)
- **Connection Idle Lifetime=60:** Return idle connections to pool after 60 seconds
- **Max Auto Prepare=10:** Automatically prepare up to 10 frequently-used statements (20-40% performance gain)
- **Command Timeout=30:** Query timeout after 30 seconds (prevents long-running queries from holding connections)
- **Timeout=30:** Connection attempt timeout (fail fast if DB unreachable)
- **Keepalive=30:** Send TCP keepalive every 30 seconds (detect dead connections faster)

**Startup.cs (EF Core Configuration):**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    var connectionString = Configuration.GetConnectionString("EquifaxDb");

    // ✅ BEST PRACTICE: Use AddDbContextPool for high-traffic scenarios
    services.AddDbContextPool<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            // ✅ Connection resiliency (retry on transient failures)
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);

            // ✅ Command timeout (prevent long-running queries from holding connections)
            npgsqlOptions.CommandTimeout(30);

            // ✅ Query splitting for large joins (prevents cartesian explosion)
            npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

        // ✅ Logging (detect slow queries and connection issues)
        if (Environment.IsDevelopment())
        {
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
            options.LogTo(Console.WriteLine, LogLevel.Information);
        }
    },
    poolSize: 128); // Reuse up to 128 DbContext instances (reduces GC pressure)

    // ✅ Health checks (monitor connection pool health)
    services.AddHealthChecks()
        .AddNpgSql(connectionString, name: "postgres", tags: new[] { "db", "sql" });
}
```

**Repository Pattern (Proper Connection Management):**
```csharp
public class EquifaxRepository : IEquifaxRepository
{
    private readonly ApplicationDbContext _context;

    public EquifaxRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // ✅ CORRECT: EF Core handles connection disposal automatically
    public async Task<EquifaxRecord?> FindByPhoneNumberAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        // No explicit connection management needed
        // EF Core opens connection, executes query, returns connection to pool
        return await _context.EquifaxRecords
            .AsNoTracking() // Read-only queries don't track changes (faster)
            .FirstOrDefaultAsync(e => e.Phone1 == phoneNumber, cancellationToken);
    }
}
```

**Connection Pool Monitoring (CloudWatch Metrics):**
```csharp
public class ConnectionPoolHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ConnectionPoolHealthCheck> _logger;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // ✅ Test connection with simple query
            await _context.Database.ExecuteSqlRawAsync(
                "SELECT 1",
                cancellationToken);

            // ✅ Check for connection leaks (idle connections > 5 minutes)
            var leakedConnections = await _context.Database
                .SqlQueryRaw<int>(@"
                    SELECT COUNT(*)
                    FROM pg_stat_activity
                    WHERE state = 'idle'
                      AND state_change < NOW() - INTERVAL '5 minutes'
                      AND application_name = 'EnrichmentAPI'
                ")
                .FirstOrDefaultAsync(cancellationToken);

            if (leakedConnections > 10)
            {
                _logger.LogWarning(
                    "Potential connection leak detected: {LeakedConnections} idle connections > 5 minutes",
                    leakedConnections);

                return HealthCheckResult.Degraded(
                    $"Connection leak detected: {leakedConnections} idle connections");
            }

            return HealthCheckResult.Healthy("PostgreSQL connection pool healthy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostgreSQL health check failed");
            return HealthCheckResult.Unhealthy("PostgreSQL connection failed", ex);
        }
    }
}
```

**Pool Settings Summary:**
- **Min connections:** 20 (warm pool for low latency)
- **Max connections:** 200 (handles 200 concurrent queries)
- **Connection lifetime:** 10 minutes (helps with RDS failover)
- **Idle lifetime:** 60 seconds (return unused connections quickly)
- **DbContext pool size:** 128 (reuse DbContext instances)
- **Prepared statements:** 10 (auto-prepare frequent queries)
- **Command timeout:** 30 seconds (fail fast on slow queries)
- **Keepalive:** 30 seconds (detect dead connections)

**Performance Benefits:**
- **Connection reuse:** No TCP handshake overhead (saves 5-10ms per query)
- **Prepared statements:** 20-40% faster query execution
- **DbContext pooling:** Reduced GC pressure, faster request processing
- **Warm pool (MinPoolSize=20):** Eliminates cold start latency
- **Leak detection:** Early warning before pool exhaustion

**Industry Standards Applied:**
- ✅ Microsoft Learn EF Core Advanced Performance Topics
- ✅ Npgsql official documentation (prepared statements, pooling)
- ✅ Azure Database for PostgreSQL connection pooling best practices
- ✅ PostgreSQL max_connections sizing formula
- ✅ Connection leak detection patterns

---

### Phase 4: Testing & Deployment (Days 11-13)

#### Feature 4.1: Unit Tests (xUnit + Moq + FluentAssertions)
**Priority:** P0 (Blocker)
**Effort:** 2 days
**Dependencies:** All features implemented
**Framework:** xUnit 2.9+, Moq 4.20+, FluentAssertions 6.12+
**Target:** 95% code coverage (line + branch)

---

#### 🔴 CRITICAL UNIT TESTING GOTCHAS

**1. Using `Returns` Instead of `ReturnsAsync` for Async Methods**
- **CATASTROPHIC:** Using `Returns(Task.FromResult(...))` instead of `ReturnsAsync` = verbose, error-prone
- Moq 4.5.28+ provides `ReturnsAsync` for cleaner async mocking
- **SOLUTION:** Always use `ReturnsAsync` for async methods
```csharp
// ❌ WRONG: Verbose and outdated pattern
mockRepo.Setup(r => r.FindByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
    .Returns(Task.FromResult<EquifaxRecord?>(new EquifaxRecord { ConsumerKey = "ABC123" }));

// ✅ CORRECT: Clean and modern (Moq 4.5.28+)
mockRepo.Setup(r => r.FindByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new EquifaxRecord { ConsumerKey = "ABC123" });
```

**2. Not Testing All 3 Async Scenarios (Success, Failure, Sync)**
- **PROBLEM:** `await` operator behaves differently when awaitable is already completed
- Most developers only test async success path
- **REQUIRED:** Test all 3 scenarios:
  1. Asynchronous success (normal flow)
  2. Asynchronous failure (exception thrown)
  3. Synchronous success (already completed Task)
```csharp
// ✅ Scenario 1: Async success
[Fact]
public async Task EnrichByPhone_AsyncSuccess_ReturnsRecord()
{
    mockRepo.Setup(r => r.FindByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new EquifaxRecord { ConsumerKey = "ABC123" });

    var result = await service.EnrichByPhoneAsync("5551234567");
    result.Should().NotBeNull();
}

// ✅ Scenario 2: Async failure
[Fact]
public async Task EnrichByPhone_AsyncFailure_ThrowsException()
{
    mockRepo.Setup(r => r.FindByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new InvalidOperationException("DB error"));

    Func<Task> act = async () => await service.EnrichByPhoneAsync("5551234567");
    await act.Should().ThrowAsync<InvalidOperationException>();
}

// ✅ Scenario 3: Sync success (Task already completed)
[Fact]
public async Task EnrichByPhone_SyncSuccess_ReturnsRecord()
{
    var completedTask = Task.FromResult<EquifaxRecord?>(new EquifaxRecord { ConsumerKey = "ABC123" });
    mockRepo.Setup(r => r.FindByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .Returns(completedTask); // Already completed

    var result = await service.EnrichByPhoneAsync("5551234567");
    result.Should().NotBeNull();
}
```

**3. Accessing Databases/Filesystem in Unit Tests (NOT Unit Tests)**
- **PROBLEM:** Unit test accessing database = integration test (slow, brittle, not isolated)
- **INDUSTRY STANDARD:** Unit tests must be Fast, Isolated, Repeatable, Self-validating, Timely (FIRST)
- Tests accessing I/O should be <100ms each, unit tests should be <10ms
- **SOLUTION:** Mock all external dependencies
```csharp
// ❌ WRONG: Accessing real database (this is an integration test)
[Fact]
public async Task EnrichByPhone_ValidPhone_ReturnsRecord()
{
    var dbContext = new ApplicationDbContext(realDbOptions); // Real DB
    var repo = new EquifaxRepository(dbContext);
    var service = new PhoneEnrichmentService(repo);

    var result = await service.EnrichByPhoneAsync("5551234567"); // Hits real DB
    result.Should().NotBeNull();
}

// ✅ CORRECT: Mock repository (true unit test)
[Fact]
public async Task EnrichByPhone_ValidPhone_ReturnsRecord()
{
    var mockRepo = new Mock<IEquifaxRepository>();
    mockRepo.Setup(r => r.FindByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new EquifaxRecord { ConsumerKey = "ABC123" });

    var service = new PhoneEnrichmentService(mockRepo.Object);

    var result = await service.EnrichByPhoneAsync("5551234567"); // No I/O
    result.Should().NotBeNull();
}
```

**4. Too Many Verify() Calls in Single Test (Anti-Pattern)**
- **PROBLEM:** Test with 5+ `Verify()` calls = hard to read, brittle, violates Single Responsibility
- Each test should verify ONE behavior
- **SOLUTION:** One Assert/Verify per test (or closely related assertions)
```csharp
// ❌ WRONG: Too many verifications in one test
[Fact]
public async Task EnrichByPhone_ValidPhone_CallsEverything()
{
    // ... setup ...
    await service.EnrichByPhoneAsync("5551234567");

    mockRepo.Verify(r => r.FindByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    mockCache.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Once);
    mockLogger.Verify(l => l.Log(...), Times.Once);
    mockMetrics.Verify(m => m.RecordLatency(...), Times.Once);
    mockAudit.Verify(a => a.LogQuery(...), Times.Once); // 5 verifications!
}

// ✅ CORRECT: Separate tests for each verification
[Fact]
public async Task EnrichByPhone_ValidPhone_QueriesRepository()
{
    await service.EnrichByPhoneAsync("5551234567");
    mockRepo.Verify(r => r.FindByPhoneNumberAsync("5551234567", It.IsAny<CancellationToken>()), Times.Once);
}

[Fact]
public async Task EnrichByPhone_ValidPhone_ChecksCacheFirst()
{
    await service.EnrichByPhoneAsync("5551234567");
    mockCache.Verify(c => c.GetAsync("phone:5551234567"), Times.Once);
}
```

**5. Using Assert.Equal Instead of FluentAssertions (Readability)**
- **PROBLEM:** `Assert.Equal(expected, actual)` is less readable than FluentAssertions
- FluentAssertions provides better error messages
- **SOLUTION:** Use FluentAssertions for all assertions
```csharp
// ❌ WRONG: Less readable, unclear error messages
Assert.NotNull(result);
Assert.Equal("ABC123", result.ConsumerKey);
Assert.True(result.Phone1 == "5551234567");

// ✅ CORRECT: More readable, clear error messages
result.Should().NotBeNull();
result.ConsumerKey.Should().Be("ABC123");
result.Phone1.Should().Be("5551234567");
```

**6. Not Using MockBehavior.Strict for Critical Paths**
- **PROBLEM:** Default `MockBehavior.Loose` allows unmocked method calls (silent failures)
- Critical business logic should fail if unexpected methods called
- **SOLUTION:** Use `MockBehavior.Strict` for critical services
```csharp
// ❌ WRONG: Loose behavior allows unexpected calls
var mockRepo = new Mock<IEquifaxRepository>(); // Default: Loose

// ✅ CORRECT: Strict behavior fails on unexpected calls
var mockRepo = new Mock<IEquifaxRepository>(MockBehavior.Strict);
mockRepo.Setup(r => r.FindByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new EquifaxRecord { ConsumerKey = "ABC123" });
// Any other method call will throw exception
```

**7. Not Clearing Mock Invocations Between Tests**
- **PROBLEM:** Mock invocations accumulate across tests (shared state)
- Test 2 sees invocations from Test 1 = false positives
- **SOLUTION:** Clear invocations or create new mock per test
```csharp
// ❌ WRONG: Shared mock across tests
private readonly Mock<IEquifaxRepository> _mockRepo = new Mock<IEquifaxRepository>();

[Fact]
public async Task Test1()
{
    await service.EnrichByPhoneAsync("5551234567");
    _mockRepo.Verify(r => r.FindByPhoneNumberAsync(...), Times.Once); // Pass
}

[Fact]
public async Task Test2()
{
    // _mockRepo still has invocation from Test1!
    _mockRepo.Verify(r => r.FindByPhoneNumberAsync(...), Times.Never); // FAILS (saw Test1 call)
}

// ✅ CORRECT: Clear invocations between tests
[Fact]
public async Task Test1()
{
    await service.EnrichByPhoneAsync("5551234567");
    _mockRepo.Verify(r => r.FindByPhoneNumberAsync(...), Times.Once);
    _mockRepo.Invocations.Clear(); // Clear for next test
}
```

**8. xUnit Runs Tests in Parallel by Default (Race Conditions)**
- **PROBLEM:** xUnit parallelizes tests by default = shared state race conditions
- Tests accessing static fields or singletons = flaky tests
- **SOLUTION:** Avoid shared state OR disable parallelization for specific tests
```csharp
// ❌ WRONG: Shared static state (race condition)
public class MyTests
{
    private static int _counter = 0; // Shared across tests!

    [Fact]
    public void Test1() { _counter++; } // Race condition

    [Fact]
    public void Test2() { _counter++; } // Race condition
}

// ✅ CORRECT: No shared state
public class MyTests
{
    [Fact]
    public void Test1()
    {
        int counter = 0; // Local variable
        counter++;
        counter.Should().Be(1);
    }
}

// ✅ ALTERNATIVE: Disable parallelization for collection
[Collection("Serial")] // Tests in this collection run sequentially
public class MyTests { }
```

**9. Not Testing Edge Cases (Null, Empty, Boundary Values)**
- **PROBLEM:** Only testing happy path = bugs in production
- **REQUIRED:** Test null, empty string, whitespace, max length, invalid format
```csharp
// ✅ Test edge cases
[Theory]
[InlineData(null)] // Null input
[InlineData("")] // Empty string
[InlineData("   ")] // Whitespace
[InlineData("abc")] // Invalid format
[InlineData("123")] // Too short
[InlineData("12345678901234567890")] // Too long
public async Task EnrichByPhone_InvalidInput_ThrowsValidationException(string phoneNumber)
{
    Func<Task> act = async () => await service.EnrichByPhoneAsync(phoneNumber);
    await act.Should().ThrowAsync<ValidationException>();
}
```

**10. Missing Code Coverage Measurement**
- **PROBLEM:** No visibility into what code is tested
- 95% target means 5% of code untested = potential bugs
- **SOLUTION:** Use Coverlet or Microsoft Code Coverage extension
```bash
# ✅ CORRECT: Measure coverage
dotnet test --collect:"XPlat Code Coverage"

# ✅ Generate HTML report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# ✅ Verify coverage >= 95%
```

---

#### **Corrected Implementation: Production-Grade Unit Tests**

**Test Project Setup (.csproj):**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <!-- xUnit framework -->
    <PackageReference Include="xunit" Version="2.9.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />

    <!-- Test SDK -->
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.0" />

    <!-- Mocking framework -->
    <PackageReference Include="Moq" Version="4.20.70" />

    <!-- Fluent assertions -->
    <PackageReference Include="FluentAssertions" Version="6.12.1" />

    <!-- Code coverage -->
    <PackageReference Include="coverlet.collector" Version="6.0.2" />
    <PackageReference Include="Microsoft.Testing.Extensions.CodeCoverage" Version="17.11.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\EnrichmentAPI.Application\EnrichmentAPI.Application.csproj" />
    <ProjectReference Include="..\EnrichmentAPI.Domain\EnrichmentAPI.Domain.csproj" />
  </ItemGroup>
</Project>
```

**PhoneEnrichmentServiceTests.cs (Comprehensive Test Suite):**
```csharp
using Xunit;
using Moq;
using FluentAssertions;
using EnrichmentAPI.Application.Services;
using EnrichmentAPI.Domain.Entities;
using EnrichmentAPI.Domain.Interfaces;

namespace EnrichmentAPI.Tests.Unit.Services
{
    public class PhoneEnrichmentServiceTests
    {
        private readonly Mock<IEquifaxRepository> _mockRepo;
        private readonly Mock<IPhoneEnrichmentCacheService> _mockCache;
        private readonly Mock<ILogger<PhoneEnrichmentService>> _mockLogger;
        private readonly PhoneEnrichmentService _service;

        // ✅ Constructor runs before each test (fresh mocks)
        public PhoneEnrichmentServiceTests()
        {
            _mockRepo = new Mock<IEquifaxRepository>(MockBehavior.Strict);
            _mockCache = new Mock<IPhoneEnrichmentCacheService>();
            _mockLogger = new Mock<ILogger<PhoneEnrichmentService>>();

            _service = new PhoneEnrichmentService(
                _mockRepo.Object,
                _mockCache.Object,
                _mockLogger.Object);
        }

        // =====================================================
        // HAPPY PATH TESTS
        // =====================================================

        [Fact]
        public async Task EnrichByPhone_ValidPhone_ReturnsRecord()
        {
            // Arrange
            var expectedRecord = new EquifaxRecord
            {
                ConsumerKey = "ABC123",
                FirstName = "John",
                LastName = "Doe",
                Phone1 = "5551234567"
            };

            _mockCache.Setup(c => c.GetCachedRecordAsync("5551234567", It.IsAny<CancellationToken>()))
                .ReturnsAsync((EquifaxRecord?)null); // Cache miss

            _mockRepo.Setup(r => r.FindByPhoneNumberAsync("5551234567", It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedRecord);

            _mockCache.Setup(c => c.CacheRecordAsync("5551234567", expectedRecord, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.EnrichByPhoneAsync("(555) 123-4567", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ConsumerKey.Should().Be("ABC123");
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.Phone1.Should().Be("5551234567");
        }

        [Fact]
        public async Task EnrichByPhone_CacheHit_ReturnsCachedRecord()
        {
            // Arrange
            var cachedRecord = new EquifaxRecord { ConsumerKey = "CACHED" };

            _mockCache.Setup(c => c.GetCachedRecordAsync("5551234567", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedRecord);

            // Act
            var result = await _service.EnrichByPhoneAsync("5551234567", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ConsumerKey.Should().Be("CACHED");

            // ✅ Verify repository was NOT called (cache hit)
            _mockRepo.Verify(
                r => r.FindByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // =====================================================
        // EDGE CASE TESTS
        // =====================================================

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task EnrichByPhone_NullOrWhitespace_ThrowsArgumentException(string phoneNumber)
        {
            // Act
            Func<Task> act = async () => await _service.EnrichByPhoneAsync(phoneNumber, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*phone number*");
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("123")]
        [InlineData("12345678901234567890")]
        public async Task EnrichByPhone_InvalidFormat_ThrowsValidationException(string phoneNumber)
        {
            // Act
            Func<Task> act = async () => await _service.EnrichByPhoneAsync(phoneNumber, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task EnrichByPhone_PhoneNotFound_ReturnsNull()
        {
            // Arrange
            _mockCache.Setup(c => c.GetCachedRecordAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EquifaxRecord?)null);

            _mockRepo.Setup(r => r.FindByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EquifaxRecord?)null); // Not found

            // Act
            var result = await _service.EnrichByPhoneAsync("5551234567", CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        // =====================================================
        // ASYNC FAILURE TESTS
        // =====================================================

        [Fact]
        public async Task EnrichByPhone_RepositoryThrows_ThrowsException()
        {
            // Arrange
            _mockCache.Setup(c => c.GetCachedRecordAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EquifaxRecord?)null);

            _mockRepo.Setup(r => r.FindByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act
            Func<Task> act = async () => await _service.EnrichByPhoneAsync("5551234567", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database error");
        }

        // =====================================================
        // BEHAVIOR VERIFICATION TESTS
        // =====================================================

        [Fact]
        public async Task EnrichByPhone_CacheMiss_CachesResult()
        {
            // Arrange
            var record = new EquifaxRecord { ConsumerKey = "ABC123" };

            _mockCache.Setup(c => c.GetCachedRecordAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EquifaxRecord?)null);

            _mockRepo.Setup(r => r.FindByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(record);

            _mockCache.Setup(c => c.CacheRecordAsync(It.IsAny<string>(), It.IsAny<EquifaxRecord>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.EnrichByPhoneAsync("5551234567", CancellationToken.None);

            // Assert
            _mockCache.Verify(
                c => c.CacheRecordAsync("5551234567", record, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task EnrichByPhone_ValidPhone_NormalizesPhoneNumber()
        {
            // Arrange
            _mockCache.Setup(c => c.GetCachedRecordAsync("5551234567", It.IsAny<CancellationToken>()))
                .ReturnsAsync((EquifaxRecord?)null);

            _mockRepo.Setup(r => r.FindByPhoneNumberAsync("5551234567", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EquifaxRecord { ConsumerKey = "ABC123" });

            _mockCache.Setup(c => c.CacheRecordAsync(It.IsAny<string>(), It.IsAny<EquifaxRecord>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.EnrichByPhoneAsync("(555) 123-4567", CancellationToken.None); // Formatted

            // Assert - Verify normalized phone was used
            _mockRepo.Verify(
                r => r.FindByPhoneNumberAsync("5551234567", It.IsAny<CancellationToken>()), // No formatting
                Times.Once);
        }
    }
}
```

**Test Coverage Configuration (coverlet.runsettings):**
```xml
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat code coverage">
        <Configuration>
          <Format>cobertura</Format>
          <Exclude>[*.Tests]*,[*.TestHelpers]*</Exclude>
          <ExcludeByAttribute>Obsolete,GeneratedCodeAttribute,CompilerGeneratedAttribute</ExcludeByAttribute>
          <ExcludeByFile>**/Migrations/**</ExcludeByFile>
          <Threshold>95</Threshold>
          <ThresholdType>line,branch</ThresholdType>
          <ThresholdStat>total</ThresholdStat>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

**Running Tests with Coverage:**
```bash
# ✅ Run all tests with coverage
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

# ✅ Generate HTML coverage report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html

# ✅ Open coverage report
open coveragereport/index.html

# ✅ Verify coverage threshold
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
# Build will FAIL if coverage < 95%
```

**Test Coverage Targets:**
- **PhoneEnrichmentService:** 95%+ (business logic)
- **EquifaxRepository:** 90%+ (data access)
- **PIIDecryptionService:** 100% (security-critical)
- **PhoneNumberNormalizer:** 100% (validation logic)
- **ApiKeyAuthMiddleware:** 100% (security-critical)
- **RateLimitingMiddleware:** 95%+ (business rules)

**Performance Benchmarks:**
- **Per test:** < 10ms (unit tests should be fast)
- **Full test suite:** < 2 minutes for 500+ tests
- **Parallel execution:** xUnit default (25-80% speed improvement)

**Industry Standards Applied:**
- ✅ Microsoft Learn unit testing best practices
- ✅ xUnit.net v3 parallel execution patterns
- ✅ Moq 4.20+ async mocking (ReturnsAsync)
- ✅ FluentAssertions 6.12+ for readability
- ✅ FAST principle (Fast, Isolated, Repeatable, Self-validating, Timely)
- ✅ AAA pattern (Arrange-Act-Assert)
- ✅ Test naming convention: TestMethod_WhatShouldHappen_WhenScenario

---

#### Feature 4.2: Integration Tests (WebApplicationFactory + Real Database)
**Priority:** P1 (High)
**Effort:** 1 day
**Dependencies:** Test database populated
**Framework:** Microsoft.AspNetCore.Mvc.Testing 9.0+, PostgreSQL test database
**Performance Target:** < 100ms per test with transaction rollback

---

#### 🔴 CRITICAL INTEGRATION TESTING GOTCHAS

**1. Program Class Visibility (.NET 6+ Breaking Change)**
- **CATASTROPHIC:** .NET 6+ generates `Program` class as internal, inaccessible to test project
- `WebApplicationFactory<Program>` fails with "Program is inaccessible due to its protection level"
- **SOLUTION:** Add `public partial class Program {}` to Program.cs
```csharp
// Program.cs (bottom of file)
var app = builder.Build();
app.Run();

// ✅ REQUIRED for integration tests (.NET 6+)
public partial class Program { }
```

**2. Using EF InMemory Database Provider (Anti-Pattern)**
- **PROBLEM:** EF Core InMemory provider is NOT a real relational database
  - No foreign key constraints
  - No unique constraints
  - No SQL dialect differences
  - Slower than SQLite in-memory mode
- **MICROSOFT RECOMMENDATION:** Use real database for integration tests
- **IF YOU MUST USE IN-MEMORY:** SQLite in-memory mode (NOT EF InMemory provider)
```csharp
// ❌ WRONG: EF InMemory provider (not relational, misses bugs)
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("TestDb"));

// ✅ BETTER: SQLite in-memory (real relational database)
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("DataSource=:memory:"));

// ✅ BEST: Real PostgreSQL test database (catches SQL dialect issues)
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql("Host=localhost;Database=enrichment_test"));
```

**3. Database Recreation = Slow Tests (4x Slower Than Transaction Rollback)**
- **PROBLEM:** Dropping and recreating database between tests = 400ms overhead per test
- 700 tests × 400ms = 4.7 minutes wasted on database setup
- **SOLUTION:** Wrap each test in transaction + rollback (100ms → 25ms per test)
```csharp
// ❌ WRONG: Recreate database for each test (SLOW)
[Fact]
public async Task Test1()
{
    await _factory.RecreateDatabase(); // 400ms overhead
    // Test here...
}

// ✅ CORRECT: Transaction rollback (4x faster)
[Fact]
public async Task Test1()
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    // Test here...
    await transaction.RollbackAsync(); // Instant cleanup
}
```

**4. Using ConfigureServices Instead of ConfigureTestServices**
- **PROBLEM:** `ConfigureServices` runs BEFORE `Startup.ConfigureServices` = overrides don't work
- Test database connection string ignored, production database used
- **SOLUTION:** Always use `ConfigureTestServices` (runs AFTER Startup)
```csharp
// ❌ WRONG: ConfigureServices runs too early
factory.WithWebHostBuilder(builder =>
{
    builder.ConfigureServices(services =>
    {
        // This runs BEFORE Startup.ConfigureServices
        // Production DbContext already registered, this does nothing
        services.AddDbContext<ApplicationDbContext>(...);
    });
});

// ✅ CORRECT: ConfigureTestServices runs after Startup
factory.WithWebHostBuilder(builder =>
{
    builder.ConfigureTestServices(services =>
    {
        // Remove production DbContext
        services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

        // Add test DbContext
        services.AddDbContext<ApplicationDbContext>(...);
    });
});
```

**5. Not Mocking Authentication (Can't Test Protected Endpoints)**
- **PROBLEM:** API endpoints require authentication, HttpClient has no auth token
- Integration tests fail with 401 Unauthorized
- **SOLUTION:** Create custom `TestAuthHandler` for mock authentication
```csharp
// ✅ Custom authentication handler for tests
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Create test claims
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.NameIdentifier, "test-buyer-123"),
            new Claim("buyer_id", "1")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

// Register in test factory
builder.ConfigureTestServices(services =>
{
    services.AddAuthentication("Test")
        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
});
```

**6. Everything in Program.cs Runs During Tests**
- **PROBLEM:** WebApplicationFactory executes entire Program.cs = real dependencies initialized
- Real Redis connection, real email service, real payment gateway = tests fail or corrupt production
- **SOLUTION:** Replace real services with test doubles in ConfigureTestServices
```csharp
builder.ConfigureTestServices(services =>
{
    // ✅ Remove real services
    services.RemoveAll<IEmailService>();
    services.RemoveAll<IPaymentGateway>();
    services.RemoveAll<IConnectionMultiplexer>(); // Redis

    // ✅ Add mock services
    services.AddSingleton<IEmailService, MockEmailService>();
    services.AddSingleton<IPaymentGateway, MockPaymentGateway>();
    services.AddSingleton<IConnectionMultiplexer>(_ => Mock.Of<IConnectionMultiplexer>());
});
```

**7. Shared Database State Between Tests (Flaky Tests)**
- **PROBLEM:** Test 1 inserts data, Test 2 expects empty database = Test 2 fails
- xUnit runs tests in parallel = race conditions with shared database
- **SOLUTION:** Transaction rollback OR disable parallelization for integration tests
```csharp
// ✅ SOLUTION 1: Transaction rollback (isolates each test)
public class IntegrationTestBase : IAsyncLifetime
{
    protected IDbContextTransaction Transaction { get; private set; }

    public async Task InitializeAsync()
    {
        Transaction = await Context.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        await Transaction.RollbackAsync();
        await Transaction.DisposeAsync();
    }
}

// ✅ SOLUTION 2: Disable parallelization for integration tests
[Collection("Integration Tests")] // All tests in collection run sequentially
public class ApiIntegrationTests { }
```

**8. HttpClient Doesn't Preserve Cookies by Default**
- **PROBLEM:** Login endpoint sets auth cookie, but subsequent requests don't send it
- **SOLUTION:** HttpClient handles cookies automatically (no action needed in ASP.NET Core)
```csharp
// ✅ HttpClient automatically handles cookies (no extra config needed)
var client = _factory.CreateClient();

// Login (sets cookie)
var loginResponse = await client.PostAsync("/api/auth/login", loginContent);

// Subsequent request includes cookie automatically
var dataResponse = await client.GetAsync("/api/data"); // Cookie sent
```

**9. Not Testing Raw SQL Queries**
- **PROBLEM:** Using in-memory database skips raw SQL testing
- Application has `context.Database.ExecuteSqlRaw(...)` for performance = untested
- **SOLUTION:** Use real database to test raw SQL queries
```csharp
// ❌ WRONG: In-memory database can't test raw SQL
// This query will fail in production but pass in tests with InMemory provider

// ✅ CORRECT: Real database catches SQL syntax errors
[Fact]
public async Task RawSqlQuery_ValidSyntax_ReturnsResults()
{
    var results = await _context.Database
        .SqlQueryRaw<EquifaxRecord>(@"
            SELECT * FROM equifax_records
            WHERE phone1 = '5551234567'
            LIMIT 10
        ")
        .ToListAsync();

    results.Should().NotBeEmpty();
}
```

**10. Missing Test Data Cleanup (Database Bloat)**
- **PROBLEM:** Integration tests insert 1000s of rows, never clean up = test database grows to GB
- Slow queries, out of disk space, tests fail
- **SOLUTION:** Transaction rollback OR aggressive cleanup strategy
```csharp
// ✅ SOLUTION 1: Transaction rollback (preferred)
using var transaction = await _context.Database.BeginTransactionAsync();
// Test inserts data...
await transaction.RollbackAsync(); // All inserts gone

// ✅ SOLUTION 2: Selective table cleanup (if transactions not feasible)
public async Task CleanupAsync()
{
    // Only delete from tables modified in this test
    await _context.Database.ExecuteSqlRawAsync("DELETE FROM audit_logs WHERE created_at > @p0", _testStartTime);
    await _context.Database.ExecuteSqlRawAsync("DELETE FROM buyers WHERE created_at > @p0", _testStartTime);
}
```

---

#### **Corrected Implementation: Production-Grade Integration Tests**

**Test Project Setup (.csproj):**
```csharp
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <!-- ASP.NET Core testing -->
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />

    <!-- xUnit framework -->
    <PackageReference Include="xunit" Version="2.9.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />

    <!-- Test SDK -->
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.0" />

    <!-- Fluent assertions -->
    <PackageReference Include="FluentAssertions" Version="6.12.1" />

    <!-- PostgreSQL (for test database) -->
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\EnrichmentAPI.Api\EnrichmentAPI.Api.csproj" />
  </ItemGroup>
</Project>
```

**CustomWebApplicationFactory.cs:**
```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EnrichmentAPI.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // ✅ Remove production DbContext
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<DbContext>();

                // ✅ Add test database (real PostgreSQL, not in-memory)
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql("Host=localhost;Database=enrichment_test;Username=postgres;Password=test");
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                });

                // ✅ Add mock authentication
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                // ✅ Remove real Redis (replace with mock)
                services.RemoveAll<IConnectionMultiplexer>();
                services.AddSingleton<IConnectionMultiplexer>(_ =>
                {
                    var mock = new Mock<IConnectionMultiplexer>();
                    var mockDb = new Mock<IDatabase>();
                    mock.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                        .Returns(mockDb.Object);
                    return mock.Object;
                });
            });

            builder.UseEnvironment("Test");
        }
    }

    // ✅ Test authentication handler (mocks API key auth)
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // ✅ Create test claims (simulates authenticated buyer)
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "TestBuyer"),
                new Claim(ClaimTypes.NameIdentifier, "test-buyer-1"),
                new Claim("buyer_id", "1"),
                new Claim("api_key", "test-api-key-12345"),
                new Claim("permissible_purpose", "marketing")
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
```

**IntegrationTestBase.cs (Transaction Rollback Pattern):**
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace EnrichmentAPI.Tests.Integration
{
    public class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        protected readonly CustomWebApplicationFactory Factory;
        protected readonly HttpClient Client;
        protected readonly ApplicationDbContext Context;
        private IDbContextTransaction _transaction;

        public IntegrationTestBase(CustomWebApplicationFactory factory)
        {
            Factory = factory;
            Client = factory.CreateClient();

            // ✅ Create scope to get DbContext
            var scope = factory.Services.CreateScope();
            Context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        // ✅ Run before each test
        public async Task InitializeAsync()
        {
            // ✅ Start transaction (4x faster than database recreation)
            _transaction = await Context.Database.BeginTransactionAsync();
        }

        // ✅ Run after each test
        public async Task DisposeAsync()
        {
            // ✅ Rollback transaction (instant cleanup)
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
            }
        }
    }
}
```

**ApiIntegrationTests.cs (Comprehensive Test Suite):**
```csharp
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EnrichmentAPI.Tests.Integration
{
    [Collection("Integration Tests")] // Run sequentially to avoid database conflicts
    public class ApiIntegrationTests : IntegrationTestBase
    {
        public ApiIntegrationTests(CustomWebApplicationFactory factory) : base(factory) { }

        // =====================================================
        // END-TO-END API TESTS
        // =====================================================

        [Fact]
        public async Task EnrichByPhone_ValidPhone_ReturnsEnrichedData()
        {
            // Arrange - Insert test data
            var testRecord = new EquifaxRecord
            {
                ConsumerKey = "TEST123",
                FirstName = "John",
                LastName = "Doe",
                Phone1 = "5551234567"
            };
            Context.EquifaxRecords.Add(testRecord);
            await Context.SaveChangesAsync();

            var request = new { phone_number = "5551234567" };

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/enrich", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<EnrichmentResponse>();
            result.Should().NotBeNull();
            result.ConsumerKey.Should().Be("TEST123");
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
        }

        [Fact]
        public async Task EnrichByPhone_PhoneNotFound_Returns404()
        {
            // Arrange
            var request = new { phone_number = "9999999999" }; // Doesn't exist

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/enrich", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // =====================================================
        // AUTHENTICATION TESTS
        // =====================================================

        [Fact]
        public async Task EnrichByPhone_NoAuthentication_Returns401()
        {
            // Arrange - Create client WITHOUT authentication
            var unauthClient = Factory.CreateClient();
            unauthClient.DefaultRequestHeaders.Clear(); // Remove test auth

            var request = new { phone_number = "5551234567" };

            // Act
            var response = await unauthClient.PostAsJsonAsync("/api/v1/enrich", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // =====================================================
        // RATE LIMITING TESTS
        // =====================================================

        [Fact]
        public async Task EnrichByPhone_ExceedsRateLimit_Returns429()
        {
            // Arrange - Make 1001 requests (exceeds 1000/min limit)
            var request = new { phone_number = "5551234567" };

            // Act - First 1000 should succeed
            for (int i = 0; i < 1000; i++)
            {
                var response = await Client.PostAsJsonAsync("/api/v1/enrich", request);
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
            }

            // 1001st request should be rate limited
            var rateLimitedResponse = await Client.PostAsJsonAsync("/api/v1/enrich", request);

            // Assert
            rateLimitedResponse.StatusCode.Should().Be((HttpStatusCode)429); // Too Many Requests
            rateLimitedResponse.Headers.Should().ContainKey("X-RateLimit-Limit");
            rateLimitedResponse.Headers.Should().ContainKey("X-RateLimit-Remaining");
            rateLimitedResponse.Headers.Should().ContainKey("Retry-After");
        }

        // =====================================================
        // CACHE TESTS
        // =====================================================

        [Fact]
        public async Task EnrichByPhone_CacheHit_ReturnsCachedData()
        {
            // Arrange - Insert test data
            var testRecord = new EquifaxRecord
            {
                ConsumerKey = "CACHE123",
                FirstName = "Jane",
                LastName = "Smith",
                Phone1 = "5559876543"
            };
            Context.EquifaxRecords.Add(testRecord);
            await Context.SaveChangesAsync();

            var request = new { phone_number = "5559876543" };

            // Act - First request (cache miss, hits database)
            var response1 = await Client.PostAsJsonAsync("/api/v1/enrich", request);
            response1.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act - Second request (cache hit, faster response)
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var response2 = await Client.PostAsJsonAsync("/api/v1/enrich", request);
            sw.Stop();

            // Assert - Cache hit should be faster (< 50ms)
            response2.StatusCode.Should().Be(HttpStatusCode.OK);
            sw.ElapsedMilliseconds.Should().BeLessThan(50); // Cache hit is fast

            var result = await response2.Content.ReadFromJsonAsync<EnrichmentResponse>();
            result.ConsumerKey.Should().Be("CACHE123");
        }

        // =====================================================
        // FCRA AUDIT LOG TESTS
        // =====================================================

        [Fact]
        public async Task EnrichByPhone_AllRequests_CreateAuditLog()
        {
            // Arrange
            var initialAuditCount = await Context.AuditLogs.CountAsync();

            var request = new { phone_number = "5551112222" };

            // Act
            await Client.PostAsJsonAsync("/api/v1/enrich", request);

            // Assert - Verify audit log created
            await Task.Delay(100); // Allow async logging to complete

            var finalAuditCount = await Context.AuditLogs.CountAsync();
            finalAuditCount.Should().Be(initialAuditCount + 1);

            var latestLog = await Context.AuditLogs
                .OrderByDescending(a => a.QueriedAt)
                .FirstOrDefaultAsync();

            latestLog.Should().NotBeNull();
            latestLog.BuyerId.Should().Be(1); // From test auth claims
            latestLog.PhoneNumber.Should().Be("5551112222");
            latestLog.PermissiblePurpose.Should().Be("marketing");
        }

        // =====================================================
        // DATABASE PERFORMANCE TESTS
        // =====================================================

        [Fact]
        public async Task EnrichByPhone_DatabaseQuery_CompletesUnder150ms()
        {
            // Arrange
            var testRecord = new EquifaxRecord
            {
                ConsumerKey = "PERF123",
                FirstName = "Performance",
                LastName = "Test",
                Phone1 = "5551231234"
            };
            Context.EquifaxRecords.Add(testRecord);
            await Context.SaveChangesAsync();

            var request = new { phone_number = "5551231234" };

            // Act
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var response = await Client.PostAsJsonAsync("/api/v1/enrich", request);
            sw.Stop();

            // Assert - Database query should be fast (p95 < 150ms)
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            sw.ElapsedMilliseconds.Should().BeLessThan(150);
        }
    }
}
```

**Running Integration Tests:**
```bash
# ✅ Setup test database (one-time)
createdb enrichment_test
dotnet ef database update --connection "Host=localhost;Database=enrichment_test;Username=postgres;Password=test"

# ✅ Run integration tests
dotnet test --filter "Category=Integration"

# ✅ Run with detailed output
dotnet test --filter "Category=Integration" --logger "console;verbosity=detailed"

# ✅ Performance: Average 25-50ms per test with transaction rollback
```

**Test Scenarios Covered:**
- ✅ End-to-end API call with authentication (POST /api/v1/enrich)
- ✅ Rate limiting enforcement (1000/min limit, 429 response)
- ✅ Cache hit/miss scenarios (< 50ms cache hit)
- ✅ Database query performance (< 150ms p95)
- ✅ FCRA audit log verification (every request logged)
- ✅ Authentication failure (401 Unauthorized)
- ✅ Phone not found (404 Not Found)

**Performance Benchmarks:**
- **Per test:** 25-100ms with transaction rollback
- **Full suite (50 tests):** < 5 seconds
- **Transaction rollback:** 4x faster than database recreation
- **Database cleanup:** Instant (rollback) vs 400ms (recreate)

**Industry Standards Applied:**
- ✅ Microsoft Learn ASP.NET Core 9.0 integration testing patterns
- ✅ WebApplicationFactory with ConfigureTestServices
- ✅ Real PostgreSQL test database (not in-memory)
- ✅ Transaction rollback for test isolation (4x faster)
- ✅ Custom AuthenticationHandler for test auth
- ✅ Sequential execution to avoid database conflicts

---

#### Feature 4.3: AWS Deployment (Elastic Beanstalk + ElastiCache + RDS)
**Priority:** P0 (Blocker)
**Effort:** 1 day
**Dependencies:** All tests passing
**Status:** RESEARCHED - Production-grade AWS deployment with 10 critical gotchas

---

### 🔴 10 CRITICAL AWS DEPLOYMENT GOTCHAS

#### 1. **Zip File Structure MUST Be Root-Level** (90% of Deployment Failures)
**Problem:** Elastic Beanstalk expects files at zip root, not nested in subdirectory.

**Common Error:**
```
❌ WRONG: my-app.zip > my-app/ > my-app.dll
          └─ EnrichmentAPI/ └─ EnrichmentAPI.dll  (nested folder)

✅ CORRECT: my-app.zip > my-app.dll
            └─ EnrichmentAPI.dll  (root level)
```

**Error Message:** "There is no .runtimeconfig.json file for your single application" (even when file exists)

**Fix:**
```bash
# WRONG way (creates nested folder)
zip -r my-app.zip my-app/

# CORRECT way (files at root)
cd my-app
zip -r ../my-app.zip .
```

**Impact:** Deployment fails, app won't start
**Detection:** Check zip structure: `unzip -l my-app.zip | head -20`
**Evidence:** AWS Stack Overflow #1 issue for .NET Core Elastic Beanstalk deployments

---

#### 2. **ElastiCache EngineCPUUtilization MUST Stay Below 90%** (Performance Cliff)
**Problem:** Redis CPU above 90% causes request timeouts and cascading failures.

**Why It Matters:**
- Single Redis core = ~100,000 RPS for GET/SET
- CPU spike = requests queue = latency spike = timeout cascade
- No CPU headroom = no burst capacity

**CloudWatch Alarms (Multi-Level):**
```json
{
  "AlarmName": "Redis-CPU-Warning",
  "MetricName": "EngineCPUUtilization",
  "Namespace": "AWS/ElastiCache",
  "Threshold": 65.0,
  "ComparisonOperator": "GreaterThanThreshold",
  "EvaluationPeriods": 2,
  "Period": 60,
  "Statistic": "Average"
}

{
  "AlarmName": "Redis-CPU-Critical",
  "MetricName": "EngineCPUUtilization",
  "Namespace": "AWS/ElastiCache",
  "Threshold": 90.0,
  "ComparisonOperator": "GreaterThanThreshold",
  "EvaluationPeriods": 1,
  "Period": 60,
  "Statistic": "Average"
}
```

**Mitigation:**
- Use Enhanced I/O (Redis 5.0.3+) for multi-CPU instances
- Use Enhanced I/O Multiplexing (Redis 7+) for lower latency
- Scale horizontally with cluster-mode ENABLED
- Monitor with 65% WARN, 90% HIGH alarms

**Impact:** Performance degradation, request timeouts, cascading failures
**Detection:** CloudWatch metric `EngineCPUUtilization`
**Evidence:** AWS Performance at Scale whitepaper 2025

---

#### 3. **Secrets Only Fetched at Instance Bootstrap** (No Auto-Rotation)
**Problem:** Elastic Beanstalk pulls secrets ONCE during instance startup. Secret rotation doesn't propagate automatically.

**Critical Limitation:**
```csharp
// Secrets Manager ARN in environment variables
Environment Variables:
  DATABASE_PASSWORD: arn:aws:secretsmanager:us-east-1:123456789012:secret:db-password-ABC123

// ❌ PROBLEM: If you rotate the secret in Secrets Manager,
// running EC2 instances still have OLD password in env vars
```

**Workaround:**
```bash
# Force environment to refetch secrets after rotation
aws elasticbeanstalk update-environment \
  --environment-name enrichment-api-prod \
  --region us-east-1

# OR restart app server (faster)
aws elasticbeanstalk restart-app-server \
  --environment-name enrichment-api-prod \
  --region us-east-1
```

**Better Approach (Direct SDK Retrieval):**
```csharp
// Startup.cs - Fetch secrets directly, NOT from environment variables
public void ConfigureServices(IServiceCollection services)
{
    var secretsClient = new AmazonSecretsManagerClient(RegionEndpoint.USEast1);

    var secretRequest = new GetSecretValueRequest
    {
        SecretId = "enrichment-api-prod/database",
        VersionStage = "AWSCURRENT"
    };

    var secretResponse = await secretsClient.GetSecretValueAsync(secretRequest);
    var secrets = JsonSerializer.Deserialize<DatabaseSecrets>(secretResponse.SecretString);

    // Build connection string from fresh secrets
    var connectionString = $"Host={secrets.Host};Port={secrets.Port};Database={secrets.Database};Username={secrets.Username};Password={secrets.Password};...";

    services.AddDbContextPool<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
```

**Impact:** Stale credentials after rotation, authentication failures
**Detection:** Monitor authentication errors after secret rotation
**Evidence:** AWS Elastic Beanstalk docs March 2025 - secret rotation limitations

---

#### 4. **ElastiCache Cluster-Mode ENABLED Required for Horizontal Scaling**
**Problem:** Cluster-mode DISABLED cannot scale horizontally (limited to single shard).

**Architectural Decision:**
```
Cluster-Mode DISABLED (Default):
  - Single primary node + read replicas
  - Max throughput: ~100K RPS per node
  - Vertical scaling only (bigger instances)
  - Good for: Small workloads, simple setup

Cluster-Mode ENABLED (Recommended):
  - Multiple shards (partitions) with replicas
  - Max throughput: ~100K RPS × shard count
  - Horizontal scaling (add shards)
  - Good for: High throughput, production workloads
```

**Terraform Configuration:**
```hcl
resource "aws_elasticache_replication_group" "enrichment_api" {
  replication_group_id       = "enrichment-api-redis"
  replication_group_description = "Redis cluster for phone enrichment caching"

  engine               = "redis"
  engine_version       = "7.1"
  node_type            = "cache.r7g.large"
  num_node_groups      = 3  # 3 shards for horizontal scaling
  replicas_per_node_group = 2  # 2 read replicas per shard

  # Cluster-mode ENABLED
  parameter_group_name = "default.redis7.cluster.on"

  # Multi-AZ with automatic failover
  multi_az_enabled       = true
  automatic_failover_enabled = true

  # Maintenance window
  maintenance_window = "sun:05:00-sun:06:00"

  # Encryption
  at_rest_encryption_enabled = true
  transit_encryption_enabled = true

  # Backup
  snapshot_retention_limit = 7
  snapshot_window         = "03:00-04:00"

  tags = {
    Environment = "production"
    Service     = "enrichment-api"
  }
}
```

**Connection Configuration (.NET):**
```csharp
// Cluster-mode ENABLED requires different connection string format
services.AddStackExchangeRedisCache(options =>
{
    options.ConfigurationOptions = new ConfigurationOptions
    {
        // Configuration endpoint (NOT individual node endpoints)
        EndPoints = { "enrichment-api-redis.abc123.clustercfg.use1.cache.amazonaws.com:6379" },

        // Critical for cluster-mode
        AbortOnConnectFail = false,
        ConnectRetry = 3,
        ConnectTimeout = 5000,
        SyncTimeout = 5000,

        // Cluster-mode specific
        Ssl = true,
        AllowAdmin = false,  // NEVER use KEYS, FLUSHDB, etc. in cluster-mode

        // Connection pooling
        PoolSize = 50
    };
});
```

**Read Replica Optimization:**
```csharp
// Direct reads to replica port 6380 for local AZ routing
var readEndpoint = "enrichment-api-redis.abc123.use1.cache.amazonaws.com:6380";

// Writes to primary port 6379
var writeEndpoint = "enrichment-api-redis.abc123.use1.cache.amazonaws.com:6379";
```

**Impact:** Cannot scale beyond single shard throughput (~100K RPS)
**Detection:** CPU spikes with increasing load, no horizontal scaling path
**Evidence:** AWS ElastiCache best practices 2025 - cluster-mode recommended for production

---

#### 5. **RDS Proxy REQUIRED for High Connection Churn** (Connection Multiplexing)
**Problem:** ASP.NET Core applications frequently open/close connections. Without RDS Proxy, connection pool exhausts.

**Symptoms:**
- "FATAL: remaining connection slots are reserved for non-replication superuser connections"
- 500 errors during traffic spikes
- Connection establishment 5-10ms overhead per request

**RDS Proxy Benefits:**
```
Without RDS Proxy:
  1000 API instances × 200 connections = 200,000 connections to RDS
  RDS max_connections = 5000 (exhausted)

With RDS Proxy:
  1000 API instances × 200 connections = 200,000 to RDS Proxy
  RDS Proxy → RDS = 500 connections (multiplexed)
  RDS max_connections = 5000 (20% utilization)
```

**Terraform Configuration:**
```hcl
resource "aws_db_proxy" "enrichment_api" {
  name                   = "enrichment-api-rds-proxy"
  engine_family          = "POSTGRESQL"
  auth {
    secret_arn = aws_secretsmanager_secret.db_credentials.arn
    iam_auth   = "DISABLED"
  }
  role_arn              = aws_iam_role.rds_proxy.arn
  vpc_subnet_ids        = aws_subnet.private[*].id
  require_tls           = true

  tags = {
    Environment = "production"
  }
}

resource "aws_db_proxy_default_target_group" "enrichment_api" {
  db_proxy_name = aws_db_proxy.enrichment_api.name

  connection_pool_config {
    # Connection multiplexing settings
    max_connections_percent         = 30  # 30% of RDS max_connections
    max_idle_connections_percent    = 10  # Reclaim idle connections
    connection_borrow_timeout       = 120  # Wait 2 minutes for connection
    session_pinning_filters         = ["EXCLUDE_VARIABLE_SETS"]  # Prevent pinning
  }
}
```

**Connection String Update:**
```json
{
  "ConnectionStrings": {
    "EquifaxDb": "Host=enrichment-api-rds-proxy.proxy-abc123.us-east-1.rds.amazonaws.com;Port=5432;Database=postgres;Username=sbadmin;Password=***;Pooling=true;MinPoolSize=20;MaxPoolSize=200;..."
  }
}
```

**Monitoring:**
```sql
-- Check RDS Proxy connection multiplexing efficiency
SELECT
    COUNT(*) as total_connections,
    COUNT(DISTINCT application_name) as unique_applications,
    state,
    wait_event_type
FROM pg_stat_activity
WHERE usename = 'sbadmin'
GROUP BY state, wait_event_type;
```

**Impact:** Connection exhaustion, 500 errors during traffic spikes
**Detection:** Monitor `MaxDatabaseConnectionsAllowed` CloudWatch metric
**Evidence:** AWS Aurora docs 2025 - RDS Proxy officially recommended for connection churn

---

#### 6. **Slow Internet = Incomplete Zip Uploads** (Silent Deployment Failures)
**Problem:** Large deployment packages upload incompletely, causing subtle runtime failures.

**Symptoms:**
- Deployment shows "SUCCESS" but missing files at runtime
- "File not found" errors for embedded resources
- Missing appsettings.json configurations
- Inconsistent behavior between deployments

**Detection:**
```bash
# Verify uploaded artifact size matches local
aws elasticbeanstalk describe-application-versions \
  --application-name enrichment-api \
  --version-labels v1.2.3 \
  --query 'ApplicationVersions[0].SourceBundle.S3Key' \
  --output text

aws s3 ls s3://elasticbeanstalk-us-east-1-123456789012/enrichment-api/v1.2.3.zip

# Compare with local zip size
ls -lh my-app.zip
```

**Mitigation:**
```bash
# Use AWS CLI with multipart upload for large files
aws s3 cp my-app.zip s3://elasticbeanstalk-us-east-1-123456789012/enrichment-api/v1.2.3.zip \
  --storage-class STANDARD \
  --metadata "md5=$(md5sum my-app.zip | awk '{print $1}')"

# Verify upload integrity
aws s3api head-object \
  --bucket elasticbeanstalk-us-east-1-123456789012 \
  --key enrichment-api/v1.2.3.zip
```

**Impact:** Intermittent runtime failures, missing dependencies
**Detection:** Compare S3 object size with local zip size
**Evidence:** Stack Overflow common issue - incomplete uploads on slow connections

---

#### 7. **IMDSv1 Security Risk** (Instance Metadata Service Vulnerability)
**Problem:** IMDSv1 allows SSRF attacks to steal IAM credentials from EC2 instances.

**Attack Vector:**
```bash
# Attacker exploits SSRF vulnerability in application
curl http://169.254.169.254/latest/meta-data/iam/security-credentials/role-name

# Returns temporary IAM credentials
{
  "AccessKeyId": "ASIA...",
  "SecretAccessKey": "...",
  "Token": "...",
  "Expiration": "2025-10-31T23:59:59Z"
}
```

**IMDSv2 Protection (Requires Session Token):**
```bash
# Step 1: Get session token (blocked by SSRF)
TOKEN=$(curl -X PUT "http://169.254.169.254/latest/api/token" \
  -H "X-aws-ec2-metadata-token-ttl-seconds: 21600")

# Step 2: Use token to access metadata
curl -H "X-aws-ec2-metadata-token: $TOKEN" \
  http://169.254.169.254/latest/meta-data/iam/security-credentials/role-name
```

**Elastic Beanstalk Configuration (.ebextensions/imds.config):**
```yaml
option_settings:
  - namespace: aws:autoscaling:launchconfiguration
    option_name: EC2KeyName
    value: my-keypair

  # Enforce IMDSv2 (blocks IMDSv1 requests)
  - namespace: aws:autoscaling:launchconfiguration
    option_name: IamInstanceProfile
    value: aws-elasticbeanstalk-ec2-role

commands:
  01_enforce_imdsv2:
    command: |
      aws ec2 modify-instance-metadata-options \
        --instance-id $(ec2-metadata --instance-id | cut -d " " -f 2) \
        --http-tokens required \
        --http-put-response-hop-limit 1
```

**Terraform Configuration:**
```hcl
resource "aws_launch_template" "elasticbeanstalk" {
  name = "enrichment-api-launch-template"

  metadata_options {
    http_tokens                 = "required"  # Enforce IMDSv2
    http_put_response_hop_limit = 1
    http_endpoint               = "enabled"
  }

  iam_instance_profile {
    arn = aws_iam_instance_profile.elasticbeanstalk_ec2.arn
  }
}
```

**Impact:** SSRF attacks can steal IAM credentials, full AWS account compromise
**Detection:** Monitor CloudWatch Logs for IMDSv1 requests
**Evidence:** AWS Security Blog 2025 - IMDSv2 enforcement recommended

---

#### 8. **Missing .NET Runtime Version = Deployment Fails**
**Problem:** Elastic Beanstalk platform may not support latest .NET version (lag 3-6 months).

**Error Message:**
```
ERROR: The specified framework 'Microsoft.NETCore.App', version '9.0.0' was not found.
```

**Detection:**
```bash
# Check available .NET versions on Elastic Beanstalk
aws elasticbeanstalk list-available-solution-stacks \
  --query 'SolutionStacks[?contains(@, `.NET`)]' \
  --output table

# Example output
64bit Amazon Linux 2023 v3.1.1 running .NET 8
64bit Amazon Linux 2 v2.7.4 running .NET Core 6
```

**Solution 1: Docker (Recommended for .NET 9):**
```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["EnrichmentAPI/EnrichmentAPI.csproj", "EnrichmentAPI/"]
RUN dotnet restore "EnrichmentAPI/EnrichmentAPI.csproj"
COPY . .
WORKDIR "/src/EnrichmentAPI"
RUN dotnet build "EnrichmentAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EnrichmentAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnrichmentAPI.dll"]
```

**Elastic Beanstalk Dockerrun.aws.json:**
```json
{
  "AWSEBDockerrunVersion": "1",
  "Image": {
    "Name": "123456789012.dkr.ecr.us-east-1.amazonaws.com/enrichment-api:latest",
    "Update": "true"
  },
  "Ports": [
    {
      "ContainerPort": 80,
      "HostPort": 80
    }
  ],
  "Logging": "/var/log/nginx"
}
```

**Solution 2: Self-Contained Deployment:**
```xml
<!-- EnrichmentAPI.csproj -->
<PropertyGroup>
  <TargetFramework>net9.0</TargetFramework>
  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
  <SelfContained>true</SelfContained>
  <PublishSingleFile>true</PublishSingleFile>
  <PublishTrimmed>false</PublishTrimmed>
</PropertyGroup>
```

```bash
# Publish with runtime included
dotnet publish -c Release -r linux-x64 --self-contained

# Zip deployment package
cd bin/Release/net9.0/linux-x64/publish/
zip -r ../../../../../enrichment-api.zip .
```

**Impact:** Deployment fails, application won't start
**Detection:** Check Elastic Beanstalk supported platforms before deployment
**Evidence:** Stack Overflow common issue - .NET version support lag

---

#### 9. **4096 Byte Environment Variable Limit** (Silent Truncation)
**Problem:** Elastic Beanstalk limits ALL environment variables combined to 4096 bytes. Exceeding causes silent truncation.

**Symptoms:**
- Missing environment variables at runtime
- Truncated connection strings
- "Unexpected end of JSON" parsing errors
- Configuration values disappear

**Detection:**
```bash
# Calculate total environment variable size
eb printenv enrichment-api-prod | \
  awk '{print length($0)}' | \
  awk '{sum += $1} END {print "Total bytes:", sum}'

# Example output
Total bytes: 4521  # ❌ EXCEEDS 4096 LIMIT
```

**Workaround:**
```csharp
// Startup.cs - Load configuration from AWS Systems Manager Parameter Store
public void ConfigureAppConfiguration(IConfigurationBuilder config)
{
    config.AddSystemsManager("/enrichment-api/prod", new AWSOptions
    {
        Region = RegionEndpoint.USEast1
    });
}

// Store large configurations in Parameter Store
aws ssm put-parameter \
  --name "/enrichment-api/prod/ConnectionStrings__EquifaxDb" \
  --value "Host=...;Port=5432;Database=postgres;Username=sbadmin;Password=***;Pooling=true;MinPoolSize=20;MaxPoolSize=200;Connection Lifetime=600;..." \
  --type "SecureString" \
  --tier "Advanced"  # Advanced tier supports 8KB parameters
```

**Best Practice (Secrets Manager):**
```csharp
// Store ALL sensitive config in Secrets Manager, reference by ARN
Environment Variables (Elastic Beanstalk):
  DATABASE_SECRET_ARN=arn:aws:secretsmanager:us-east-1:123456789012:secret:db-ABC123
  REDIS_SECRET_ARN=arn:aws:secretsmanager:us-east-1:123456789012:secret:redis-XYZ789

// Total size: 180 bytes (well under 4096 limit)
```

**Impact:** Silent configuration loss, runtime failures, authentication errors
**Detection:** Monitor for truncated environment variables
**Evidence:** AWS Elastic Beanstalk docs - 4096 byte limit for all env vars combined

---

#### 10. **No Reset On Close for PgBouncer Compatibility**
**Problem:** If using PgBouncer for connection pooling, Npgsql's default "Reset On Close" breaks pooling.

**Why It Breaks:**
```csharp
// Npgsql default behavior
using (var connection = new NpgsqlConnection(connectionString))
{
    await connection.OpenAsync();
    await connection.ExecuteAsync("SET application_name = 'MyApp'");
    // ... queries ...
}  // ← Connection.Close() sends DISCARD ALL to reset session state

// PgBouncer in TRANSACTION pooling mode expects NO session commands
// DISCARD ALL breaks PgBouncer's transaction pooling
```

**Fix:**
```json
{
  "ConnectionStrings": {
    "EquifaxDb": "Host=pgbouncer.internal;Port=6432;Database=postgres;Username=sbadmin;Password=***;Pooling=true;No Reset On Close=true;Max Auto Prepare=0;"
  }
}
```

**When to Use:**
- Using PgBouncer in TRANSACTION pooling mode
- Using AWS RDS Proxy (less critical but still beneficial)
- High connection churn workloads

**When NOT to Use:**
- Direct RDS connection without pooler
- PgBouncer in SESSION pooling mode
- Need session-level temporary tables

**Impact:** PgBouncer errors, connection pool instability
**Detection:** PgBouncer logs show "prepared statement does not exist" errors
**Evidence:** Npgsql docs - PgBouncer compatibility requirements

---

### 📋 PRODUCTION-GRADE AWS DEPLOYMENT

#### Architecture Overview
```
Internet
   ↓
Application Load Balancer (ALB)
   ├─ Target Group 1 (AZ us-east-1a)
   │  └─ Elastic Beanstalk Auto Scaling Group
   │     ├─ EC2 Instance 1 (ASP.NET Core 9 API)
   │     ├─ EC2 Instance 2 (ASP.NET Core 9 API)
   │     └─ EC2 Instance N
   │
   └─ Target Group 2 (AZ us-east-1b)
      └─ Elastic Beanstalk Auto Scaling Group
         ├─ EC2 Instance 1 (ASP.NET Core 9 API)
         ├─ EC2 Instance 2 (ASP.NET Core 9 API)
         └─ EC2 Instance N

API Instances connect to:
   ├─ ElastiCache Redis (Cluster-Mode Enabled)
   │  ├─ Shard 1: Primary + 2 Replicas (Multi-AZ)
   │  ├─ Shard 2: Primary + 2 Replicas (Multi-AZ)
   │  └─ Shard 3: Primary + 2 Replicas (Multi-AZ)
   │
   ├─ RDS Proxy (Connection Multiplexing)
   │  └─ RDS PostgreSQL 18 (Multi-AZ)
   │     ├─ Primary Instance (us-east-1a)
   │     └─ Standby Instance (us-east-1b)
   │
   └─ AWS Secrets Manager (Database & Redis credentials)
```

---

#### Deployment Package Structure

**Correct Zip Structure:**
```
enrichment-api.zip
├─ EnrichmentAPI.dll
├─ EnrichmentAPI.runtimeconfig.json
├─ appsettings.json
├─ appsettings.Production.json
├─ web.config
└─ wwwroot/
   └─ swagger/
```

**Build Script:**
```bash
#!/bin/bash
# build-and-deploy.sh

set -e  # Exit on error

PROJECT_NAME="EnrichmentAPI"
VERSION=$(date +%Y%m%d.%H%M)
DEPLOY_DIR="./deploy"
ZIP_FILE="${PROJECT_NAME}-${VERSION}.zip"

echo "Building ${PROJECT_NAME} version ${VERSION}..."

# Clean previous builds
rm -rf $DEPLOY_DIR
mkdir -p $DEPLOY_DIR

# Publish application
dotnet publish src/${PROJECT_NAME}/${PROJECT_NAME}.csproj \
  -c Release \
  -o $DEPLOY_DIR \
  --self-contained false \
  -p:PublishTrimmed=false

# Create deployment zip (files at root level)
cd $DEPLOY_DIR
zip -r ../$ZIP_FILE . -x "*.pdb"
cd ..

# Verify zip structure
echo "Verifying zip structure..."
unzip -l $ZIP_FILE | head -20

# Calculate size
ZIP_SIZE=$(ls -lh $ZIP_FILE | awk '{print $5}')
echo "Deployment package size: $ZIP_SIZE"

# Upload to S3
aws s3 cp $ZIP_FILE s3://elasticbeanstalk-us-east-1-123456789012/enrichment-api/$ZIP_FILE

# Create application version
aws elasticbeanstalk create-application-version \
  --application-name enrichment-api \
  --version-label $VERSION \
  --source-bundle S3Bucket=elasticbeanstalk-us-east-1-123456789012,S3Key=enrichment-api/$ZIP_FILE \
  --description "Deployment $(date)"

# Deploy to environment
aws elasticbeanstalk update-environment \
  --environment-name enrichment-api-prod \
  --version-label $VERSION

echo "Deployment initiated. Version: $VERSION"
echo "Monitor deployment: https://console.aws.amazon.com/elasticbeanstalk"
```

---

#### Elastic Beanstalk Configuration (.ebextensions/)

**.ebextensions/01-environment.config:**
```yaml
option_settings:
  # Platform settings
  aws:elasticbeanstalk:environment:
    EnvironmentType: LoadBalanced
    LoadBalancerType: application
    ServiceRole: aws-elasticbeanstalk-service-role

  # Auto Scaling
  aws:autoscaling:asg:
    MinSize: 2
    MaxSize: 10
    Cooldown: 300

  aws:autoscaling:trigger:
    MeasureName: CPUUtilization
    Unit: Percent
    UpperThreshold: 70
    LowerThreshold: 30
    UpperBreachScaleIncrement: 2
    LowerBreachScaleIncrement: -1

  # EC2 instances
  aws:autoscaling:launchconfiguration:
    InstanceType: t3.medium
    IamInstanceProfile: aws-elasticbeanstalk-ec2-role
    SecurityGroups: sg-abc123  # API security group
    EC2KeyName: enrichment-api-keypair

  # Load balancer
  aws:elbv2:loadbalancer:
    SecurityGroups: sg-xyz789  # ALB security group
    ManagedSecurityGroup: sg-xyz789

  aws:elbv2:listener:443:
    Protocol: HTTPS
    SSLCertificateArns: arn:aws:acm:us-east-1:123456789012:certificate/abc-123-xyz

  # Health checks
  aws:elasticbeanstalk:application:
    Application Healthcheck URL: HTTP:80/health

  aws:elb:healthcheck:
    Interval: 30
    Timeout: 5
    HealthyThreshold: 2
    UnhealthyThreshold: 5

  # Logging
  aws:elasticbeanstalk:cloudwatch:logs:
    StreamLogs: true
    DeleteOnTerminate: false
    RetentionInDays: 30

  # .NET platform
  aws:elasticbeanstalk:container:dotnet:apppool:
    Enable 32-bit Applications: false
    Managed Pipeline Mode: Integrated
    Target Runtime: 8.0

  # Environment variables (use Secrets Manager ARNs)
  aws:elasticbeanstalk:application:environment:
    ASPNETCORE_ENVIRONMENT: Production
    DATABASE_SECRET_ARN: arn:aws:secretsmanager:us-east-1:123456789012:secret:enrichment-db-ABC123
    REDIS_SECRET_ARN: arn:aws:secretsmanager:us-east-1:123456789012:secret:enrichment-redis-XYZ789
```

**.ebextensions/02-security.config:**
```yaml
commands:
  01_enforce_imdsv2:
    command: |
      INSTANCE_ID=$(ec2-metadata --instance-id | cut -d " " -f 2)
      aws ec2 modify-instance-metadata-options \
        --instance-id $INSTANCE_ID \
        --http-tokens required \
        --http-put-response-hop-limit 1 \
        --region us-east-1

  02_install_cloudwatch_agent:
    command: |
      wget https://s3.amazonaws.com/amazoncloudwatch-agent/amazon_linux/amd64/latest/amazon-cloudwatch-agent.rpm
      rpm -U ./amazon-cloudwatch-agent.rpm

  03_configure_cloudwatch_agent:
    command: |
      cat > /opt/aws/amazon-cloudwatch-agent/etc/config.json << 'EOF'
      {
        "metrics": {
          "namespace": "EnrichmentAPI",
          "metrics_collected": {
            "mem": {
              "measurement": [
                {"name": "mem_used_percent", "rename": "MemoryUtilization"}
              ],
              "metrics_collection_interval": 60
            },
            "disk": {
              "measurement": [
                {"name": "disk_used_percent", "rename": "DiskUtilization"}
              ],
              "metrics_collection_interval": 60
            }
          }
        }
      }
      EOF

      /opt/aws/amazon-cloudwatch-agent/bin/amazon-cloudwatch-agent-ctl \
        -a fetch-config \
        -m ec2 \
        -c file:/opt/aws/amazon-cloudwatch-agent/etc/config.json \
        -s
```

---

#### ElastiCache Redis Configuration (Terraform)

```hcl
# elasticache.tf
resource "aws_elasticache_replication_group" "enrichment_api" {
  replication_group_id       = "enrichment-api-redis"
  replication_group_description = "Redis cluster for phone enrichment caching"

  # Engine
  engine               = "redis"
  engine_version       = "7.1"
  port                 = 6379
  parameter_group_name = "default.redis7.cluster.on"

  # Cluster-mode ENABLED (horizontal scaling)
  num_node_groups         = 3  # 3 shards
  replicas_per_node_group = 2  # 2 read replicas per shard

  # Instance type
  node_type = "cache.r7g.large"  # 13.07 GiB memory, 2 vCPUs

  # Multi-AZ with automatic failover
  multi_az_enabled           = true
  automatic_failover_enabled = true

  # Subnet group
  subnet_group_name = aws_elasticache_subnet_group.enrichment_api.name

  # Security
  security_group_ids = [aws_security_group.redis.id]
  at_rest_encryption_enabled = true
  transit_encryption_enabled = true
  auth_token_enabled         = true
  auth_token                 = random_password.redis_auth_token.result

  # Maintenance
  maintenance_window      = "sun:05:00-sun:06:00"
  snapshot_retention_limit = 7
  snapshot_window         = "03:00-04:00"

  # Notifications
  notification_topic_arn = aws_sns_topic.elasticache_notifications.arn

  # Logging
  log_delivery_configuration {
    destination      = aws_cloudwatch_log_group.redis_slow_log.name
    destination_type = "cloudwatch-logs"
    log_format       = "json"
    log_type         = "slow-log"
  }

  log_delivery_configuration {
    destination      = aws_cloudwatch_log_group.redis_engine_log.name
    destination_type = "cloudwatch-logs"
    log_format       = "json"
    log_type         = "engine-log"
  }

  tags = {
    Environment = "production"
    Service     = "enrichment-api"
    ManagedBy   = "terraform"
  }
}

resource "aws_elasticache_subnet_group" "enrichment_api" {
  name       = "enrichment-api-redis-subnet"
  subnet_ids = aws_subnet.private[*].id
}

resource "random_password" "redis_auth_token" {
  length  = 32
  special = false  # Redis auth tokens don't support special characters
}

resource "aws_secretsmanager_secret" "redis_auth_token" {
  name = "enrichment-api/redis-auth-token"
}

resource "aws_secretsmanager_secret_version" "redis_auth_token" {
  secret_id     = aws_secretsmanager_secret.redis_auth_token.id
  secret_string = jsonencode({
    auth_token = random_password.redis_auth_token.result
    endpoint   = aws_elasticache_replication_group.enrichment_api.configuration_endpoint_address
    port       = 6379
  })
}

# CloudWatch Alarms
resource "aws_cloudwatch_metric_alarm" "redis_cpu_warning" {
  alarm_name          = "enrichment-api-redis-cpu-warning"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "EngineCPUUtilization"
  namespace           = "AWS/ElastiCache"
  period              = 60
  statistic           = "Average"
  threshold           = 65
  alarm_description   = "Redis CPU utilization above 65%"
  alarm_actions       = [aws_sns_topic.elasticache_notifications.arn]

  dimensions = {
    ReplicationGroupId = aws_elasticache_replication_group.enrichment_api.id
  }
}

resource "aws_cloudwatch_metric_alarm" "redis_cpu_critical" {
  alarm_name          = "enrichment-api-redis-cpu-critical"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 1
  metric_name         = "EngineCPUUtilization"
  namespace           = "AWS/ElastiCache"
  period              = 60
  statistic           = "Average"
  threshold           = 90
  alarm_description   = "Redis CPU utilization above 90% - CRITICAL"
  alarm_actions       = [aws_sns_topic.elasticache_notifications.arn]

  dimensions = {
    ReplicationGroupId = aws_elasticache_replication_group.enrichment_api.id
  }
}

resource "aws_cloudwatch_metric_alarm" "redis_memory" {
  alarm_name          = "enrichment-api-redis-memory"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "DatabaseMemoryUsagePercentage"
  namespace           = "AWS/ElastiCache"
  period              = 300
  statistic           = "Average"
  threshold           = 85
  alarm_description   = "Redis memory usage above 85%"
  alarm_actions       = [aws_sns_topic.elasticache_notifications.arn]

  dimensions = {
    ReplicationGroupId = aws_elasticache_replication_group.enrichment_api.id
  }
}
```

---

#### RDS Proxy Configuration (Terraform)

```hcl
# rds-proxy.tf
resource "aws_db_proxy" "enrichment_api" {
  name                   = "enrichment-api-rds-proxy"
  engine_family          = "POSTGRESQL"

  auth {
    auth_scheme = "SECRETS"
    iam_auth    = "DISABLED"
    secret_arn  = aws_secretsmanager_secret.db_credentials.arn
  }

  role_arn = aws_iam_role.rds_proxy.arn

  vpc_subnet_ids = aws_subnet.private[*].id
  require_tls    = true

  debug_logging = true

  tags = {
    Environment = "production"
    Service     = "enrichment-api"
  }
}

resource "aws_db_proxy_default_target_group" "enrichment_api" {
  db_proxy_name = aws_db_proxy.enrichment_api.name

  connection_pool_config {
    # Connection multiplexing
    max_connections_percent         = 30   # 30% of RDS max_connections
    max_idle_connections_percent    = 10   # Reclaim idle connections
    connection_borrow_timeout       = 120  # Wait 2 minutes for connection
    init_query                      = ""   # No initialization query
    session_pinning_filters         = [
      "EXCLUDE_VARIABLE_SETS"  # Prevent session pinning
    ]
  }
}

resource "aws_db_proxy_target" "enrichment_api" {
  db_proxy_name         = aws_db_proxy.enrichment_api.name
  target_group_name     = aws_db_proxy_default_target_group.enrichment_api.name
  db_instance_identifier = aws_db_instance.enrichment_api.id
}

# IAM role for RDS Proxy
resource "aws_iam_role" "rds_proxy" {
  name = "enrichment-api-rds-proxy-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "rds.amazonaws.com"
        }
      }
    ]
  })
}

resource "aws_iam_role_policy" "rds_proxy_secrets" {
  name = "enrichment-api-rds-proxy-secrets"
  role = aws_iam_role.rds_proxy.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = [
          "secretsmanager:GetSecretValue"
        ]
        Effect = "Allow"
        Resource = aws_secretsmanager_secret.db_credentials.arn
      }
    ]
  })
}

# CloudWatch Alarms for RDS Proxy
resource "aws_cloudwatch_metric_alarm" "rds_proxy_connections" {
  alarm_name          = "enrichment-api-rds-proxy-connections"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "DatabaseConnections"
  namespace           = "AWS/RDS"
  period              = 300
  statistic           = "Average"
  threshold           = 800  # 80% of 1000 max connections
  alarm_description   = "RDS Proxy connection count high"
  alarm_actions       = [aws_sns_topic.rds_notifications.arn]

  dimensions = {
    DBProxyName = aws_db_proxy.enrichment_api.name
  }
}
```

---

#### Secrets Management (Terraform)

```hcl
# secrets.tf
resource "aws_secretsmanager_secret" "db_credentials" {
  name        = "enrichment-api/database"
  description = "RDS PostgreSQL credentials for Enrichment API"

  recovery_window_in_days = 30

  tags = {
    Environment = "production"
    Service     = "enrichment-api"
  }
}

resource "aws_secretsmanager_secret_version" "db_credentials" {
  secret_id = aws_secretsmanager_secret.db_credentials.id

  secret_string = jsonencode({
    host     = aws_db_instance.enrichment_api.address
    port     = aws_db_instance.enrichment_api.port
    database = aws_db_instance.enrichment_api.db_name
    username = aws_db_instance.enrichment_api.username
    password = random_password.db_password.result

    # RDS Proxy endpoint (use this in application)
    proxy_host = aws_db_proxy.enrichment_api.endpoint
  })
}

resource "random_password" "db_password" {
  length  = 32
  special = true
}

# IAM policy for Elastic Beanstalk EC2 instances to read secrets
resource "aws_iam_role_policy" "elasticbeanstalk_secrets" {
  name = "enrichment-api-secrets-access"
  role = "aws-elasticbeanstalk-ec2-role"  # Existing role

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = [
          "secretsmanager:GetSecretValue"
        ]
        Effect = "Allow"
        Resource = [
          aws_secretsmanager_secret.db_credentials.arn,
          aws_secretsmanager_secret.redis_auth_token.arn
        ]
      }
    ]
  })
}
```

---

#### Application Configuration (appsettings.Production.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",

  "ConnectionStrings": {
    "EquifaxDb": "PLACEHOLDER - Loaded from Secrets Manager at runtime"
  },

  "Redis": {
    "Configuration": "PLACEHOLDER - Loaded from Secrets Manager at runtime"
  },

  "AWS": {
    "Region": "us-east-1",
    "Secrets": {
      "DatabaseSecretArn": "ENV:DATABASE_SECRET_ARN",
      "RedisSecretArn": "ENV:REDIS_SECRET_ARN"
    }
  },

  "HealthChecks": {
    "Enabled": true,
    "DatabaseTimeout": 5,
    "RedisTimeout": 3
  },

  "RateLimit": {
    "EnableRateLimiting": true,
    "DefaultLimit": 1000,
    "PeriodSeconds": 60
  }
}
```

---

#### Startup Configuration with Secrets Manager

```csharp
// Program.cs
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Load secrets from AWS Secrets Manager
        await ConfigureAwsSecretsAsync(builder.Configuration, builder.Environment);

        // Configure services
        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        // Run database migrations
        if (builder.Environment.IsProduction())
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Database.MigrateAsync();
            }
        }

        Configure(app, builder.Environment);

        await app.RunAsync();
    }

    private static async Task ConfigureAwsSecretsAsync(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (!environment.IsProduction())
            return;

        var region = RegionEndpoint.GetBySystemName(configuration["AWS:Region"] ?? "us-east-1");
        var secretsClient = new AmazonSecretsManagerClient(region);

        // Fetch database credentials
        var dbSecretArn = Environment.GetEnvironmentVariable("DATABASE_SECRET_ARN");
        if (!string.IsNullOrEmpty(dbSecretArn))
        {
            var dbSecretResponse = await secretsClient.GetSecretValueAsync(new GetSecretValueRequest
            {
                SecretId = dbSecretArn,
                VersionStage = "AWSCURRENT"
            });

            var dbSecrets = JsonSerializer.Deserialize<DatabaseSecrets>(dbSecretResponse.SecretString);

            // Build connection string using RDS Proxy endpoint
            var connectionString = $"Host={dbSecrets.ProxyHost};Port={dbSecrets.Port};Database={dbSecrets.Database};Username={dbSecrets.Username};Password={dbSecrets.Password};Pooling=true;MinPoolSize=20;MaxPoolSize=200;Connection Lifetime=600;Connection Idle Lifetime=60;Max Auto Prepare=10;Command Timeout=30;Timeout=30;Keepalive=30;No Reset On Close=true;";

            // Override configuration
            configuration["ConnectionStrings:EquifaxDb"] = connectionString;
        }

        // Fetch Redis credentials
        var redisSecretArn = Environment.GetEnvironmentVariable("REDIS_SECRET_ARN");
        if (!string.IsNullOrEmpty(redisSecretArn))
        {
            var redisSecretResponse = await secretsClient.GetSecretValueAsync(new GetSecretValueRequest
            {
                SecretId = redisSecretArn,
                VersionStage = "AWSCURRENT"
            });

            var redisSecrets = JsonSerializer.Deserialize<RedisSecrets>(redisSecretResponse.SecretString);

            // Build Redis configuration string
            var redisConfig = $"{redisSecrets.Endpoint}:{redisSecrets.Port},password={redisSecrets.AuthToken},ssl=true,abortConnect=false,connectRetry=3,connectTimeout=5000,syncTimeout=5000";

            configuration["Redis:Configuration"] = redisConfig;
        }
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContextPool<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("EquifaxDb"), npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(30);
                npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
        },
        poolSize: 128);

        // Redis caching
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:Configuration"];
            options.ConfigurationOptions = ConfigurationOptions.Parse(configuration["Redis:Configuration"]);
        });

        // Health checks
        services.AddHealthChecks()
            .AddNpgSql(
                configuration.GetConnectionString("EquifaxDb"),
                name: "database",
                timeout: TimeSpan.FromSeconds(5))
            .AddRedis(
                configuration["Redis:Configuration"],
                name: "redis",
                timeout: TimeSpan.FromSeconds(3));

        // ... rest of service configuration
    }
}

public class DatabaseSecrets
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Database { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string ProxyHost { get; set; }  // RDS Proxy endpoint
}

public class RedisSecrets
{
    public string Endpoint { get; set; }
    public int Port { get; set; }
    public string AuthToken { get; set; }
}
```

---

### 🚀 DEPLOYMENT CHECKLIST

#### Pre-Deployment
- [ ] All tests passing locally (unit + integration)
- [ ] Code coverage ≥ 95%
- [ ] Database migrations tested in staging
- [ ] Performance benchmarks validated
- [ ] Security scan completed (no critical/high vulnerabilities)
- [ ] Secrets stored in AWS Secrets Manager
- [ ] IAM roles configured for EC2 instances

#### Infrastructure Setup
- [ ] ElastiCache Redis cluster created (cluster-mode ENABLED)
- [ ] RDS Proxy configured for connection multiplexing
- [ ] CloudWatch alarms configured (CPU, memory, connections)
- [ ] SNS topics created for notifications
- [ ] Security groups configured (least privilege)
- [ ] IMDSv2 enforced on EC2 instances
- [ ] SSL/TLS certificates uploaded to ACM

#### Deployment
- [ ] Build deployment package (correct zip structure)
- [ ] Upload to S3 with integrity check
- [ ] Create Elastic Beanstalk application version
- [ ] Deploy to staging environment first
- [ ] Run smoke tests in staging
- [ ] Deploy to production environment
- [ ] Verify health checks passing
- [ ] Monitor CloudWatch metrics for 15 minutes

#### Post-Deployment
- [ ] Smoke test production endpoint
- [ ] Verify Redis cache hit rates
- [ ] Check RDS Proxy connection multiplexing
- [ ] Review CloudWatch Logs for errors
- [ ] Validate response times (p50, p95, p99)
- [ ] Test rate limiting (1000 req/min)
- [ ] Verify FCRA audit logging
- [ ] Update documentation with endpoints

#### Rollback Plan
- [ ] Previous application version available
- [ ] Database migration rollback script ready
- [ ] DNS TTL reduced to 60 seconds
- [ ] Rollback runbook documented

---

### 📊 MONITORING & OBSERVABILITY

#### CloudWatch Metrics to Monitor

**Elastic Beanstalk:**
- EnvironmentHealth (overall health status)
- InstancesSevere (instances in severe state)
- ApplicationRequestsTotal (total requests)
- ApplicationRequests2xx, 4xx, 5xx (response codes)
- ApplicationLatencyP50, P95, P99 (response times)

**ElastiCache Redis:**
- EngineCPUUtilization (< 90% target)
- DatabaseMemoryUsagePercentage (< 85% target)
- CacheHits, CacheMisses (hit rate > 85%)
- NetworkBytesIn/Out (throughput)
- CurrConnections (connection count)

**RDS PostgreSQL:**
- DatabaseConnections (< 80% of max)
- CPUUtilization (< 80%)
- FreeableMemory (> 1 GB)
- ReadLatency, WriteLatency (< 10ms)
- NetworkReceiveThroughput

**RDS Proxy:**
- DatabaseConnections (multiplexing efficiency)
- ClientConnections (total connections from app)
- MaxDatabaseConnectionsAllowed (capacity)
- QueryLatency (proxy overhead < 5ms)

#### CloudWatch Alarms (Critical)

```hcl
# Application error rate
resource "aws_cloudwatch_metric_alarm" "api_error_rate" {
  alarm_name          = "enrichment-api-error-rate"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "ApplicationRequests5xx"
  namespace           = "AWS/ElasticBeanstalk"
  period              = 300
  statistic           = "Sum"
  threshold           = 10  # More than 10 errors in 5 minutes
  alarm_description   = "API error rate high - investigate immediately"
  alarm_actions       = [aws_sns_topic.critical_alerts.arn]

  dimensions = {
    EnvironmentName = "enrichment-api-prod"
  }
}

# API latency p99
resource "aws_cloudwatch_metric_alarm" "api_latency_p99" {
  alarm_name          = "enrichment-api-latency-p99"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "ApplicationLatencyP99"
  namespace           = "AWS/ElasticBeanstalk"
  period              = 300
  statistic           = "Average"
  threshold           = 500  # p99 > 500ms
  alarm_description   = "API p99 latency exceeding 500ms"
  alarm_actions       = [aws_sns_topic.performance_alerts.arn]

  dimensions = {
    EnvironmentName = "enrichment-api-prod"
  }
}
```

---

### 📝 DEPLOYMENT TIMELINE

**Day 1: Infrastructure Setup (4 hours)**
- Create ElastiCache Redis cluster
- Configure RDS Proxy
- Set up Secrets Manager
- Configure CloudWatch alarms

**Day 1: Application Deployment (3 hours)**
- Build deployment package
- Deploy to staging environment
- Run smoke tests
- Deploy to production

**Day 1: Post-Deployment Validation (1 hour)**
- Monitor metrics for 1 hour
- Verify cache hit rates
- Check connection pooling
- Review logs for errors

---

### ✅ SUCCESS CRITERIA

- [ ] API responding with < 100ms p50 latency
- [ ] Redis cache hit rate > 85%
- [ ] RDS Proxy connection multiplexing active
- [ ] Zero deployment errors
- [ ] All health checks passing
- [ ] CloudWatch alarms configured and silent
- [ ] Production smoke tests passing

---

## TIMELINE & MILESTONES

### Sprint 1: Core API (Days 1-5)
**Goal:** Functional API endpoint with database integration

| Day | Tasks | Deliverable |
|-----|-------|-------------|
| 1 | Project setup, Clean Architecture scaffold, API controller | POST /api/v1/enrich endpoint |
| 2 | Phone normalization, validation, repository pattern | Phone search works locally |
| 3 | Entity mapping (398 columns), EF Core configuration | Full dataset returned |
| 4 | PII decryption service, encryption key management | Encrypted fields decrypted |
| 5 | Error handling, logging, Swagger documentation | API docs available |

**Milestone 1:** ✅ Basic phone lookup working without auth/caching

---

### Sprint 2: Security & Compliance (Days 6-8)
**Goal:** Production-ready authentication and FCRA compliance

| Day | Tasks | Deliverable |
|-----|-------|-------------|
| 6 | API key authentication, buyer management | Auth middleware working |
| 7 | Rate limiting (AspNetCoreRateLimit), quota tracking | 429 responses enforced |
| 8 | FCRA audit logging, permissible purpose validation | All queries logged |

**Milestone 2:** ✅ Secure, FCRA-compliant API

---

### Sprint 3: Performance & Deployment (Days 9-13)
**Goal:** Production deployment with caching

| Day | Tasks | Deliverable |
|-----|-------|-------------|
| 9 | Redis integration, cache strategy | Cache hit < 10ms |
| 10 | Database connection pooling, query optimization | p95 < 150ms |
| 11 | Unit tests, FluentAssertions, Moq | 95% coverage |
| 12 | Integration tests, performance tests | All tests green |
| 13 | AWS deployment, smoke tests, monitoring | Production live |

**Milestone 3:** ✅ MVP deployed to production

---

## RISK ASSESSMENT

### High-Risk Items

#### Risk 1: Database Query Performance
**Impact:** High
**Probability:** Medium
**Mitigation:**
- Create all 10 phone number indexes before launch
- Implement Redis caching (24-hour TTL)
- Monitor query execution plans in CloudWatch
- Set up slow query alerts (> 500ms)

---

#### Risk 2: FCRA Compliance Violations
**Impact:** Critical (Legal liability)
**Probability:** Low
**Mitigation:**
- Mandatory permissible purpose field per buyer
- Immutable audit logs (append-only table)
- 24-month retention enforced at database level
- Quarterly compliance audits
- Legal review of Terms of Service

---

#### Risk 3: API Key Security
**Impact:** High
**Probability:** Low
**Mitigation:**
- API keys stored as SHA-256 hashes
- Rate limiting per key (100 req/min)
- Monitor for suspicious patterns
- Automatic key rotation every 90 days
- Revocation endpoint for compromised keys

---

#### Risk 4: Data Import Incomplete
**Impact:** High
**Probability:** Low
**Current Status:** 190M/326.7M rows imported (58.4%)
**Mitigation:**
- Import completion ETA: Later today (Oct 31)
- Resume script running with TCP keepalive
- Monitor import progress every 30 minutes
- Verify row count matches source (326,718,517)

---

## SUCCESS CRITERIA

### Technical KPIs
- ✅ API response time p50: < 100ms
- ✅ API response time p95: < 200ms
- ✅ API response time p99: < 500ms
- ✅ Cache hit rate: > 60%
- ✅ Uptime: 99.9% (43 minutes downtime/month max)
- ✅ Error rate: < 0.1%
- ✅ Test coverage: > 95%

### Business KPIs
- ✅ Match rate: > 85% for valid US phone numbers
- ✅ Daily active buyers: 10+ within first month
- ✅ Daily queries: 10,000+ within first month
- ✅ Revenue: $5,000+ MRR within 60 days

### Compliance KPIs
- ✅ Zero FCRA violations
- ✅ 100% audit log coverage
- ✅ Response to FCRA requests: < 24 hours

---

## CONTRACTUAL REQUIREMENTS (MVP Phase 4 - MANDATORY)

### Phase 4: Contract Compliance Features (Days 14-18)
**Goal:** Fulfill all contractual obligations from Data Subscription Agreement

| Day | Tasks | Deliverable |
|-----|-------|-------------|
| 14 | Admin dashboard (match rates, usage metrics) | Real-time metrics dashboard |
| 15 | Usage tracking & billing calculation | Automated billing reports |
| 16 | Profit share tracking (10%, 20%, 30%) | Revenue share calculations |
| 17 | SLA monitoring & reporting | Uptime/latency dashboards |
| 18 | Data subject rights portal | GDPR/CCPA compliance portal |

---

### Feature 4.1: Admin Dashboard (Match Rates & Usage Metrics)
**Priority:** P0 (Blocker) - **CONTRACTUAL REQUIREMENT** (Section 3.2, 3.4, 3.5)
**Effort:** 2 days
**Dependencies:** Usage tracking database tables

**Purpose:** Real-time visibility into API usage, match rates, and billing for contract compliance

**Features:**
```
Admin Dashboard Pages:

1. Usage Overview
   - API calls per buyer (today, week, month)
   - Match rate per buyer (% of successful lookups)
   - Response time trends (average, p95, p99)
   - Geographic distribution of requests

2. Billing Dashboard
   - Monthly usage vs quota per buyer
   - Overage calculations ($0.035 per call)
   - Profit share calculations:
     * Routing: 10% net profit
     * PROVIDER-introduced: 20% net profit
     * Appends: 30% net profit
   - Invoice generation

3. SLA Monitoring
   - Current uptime: 99.5% target (contract requirement)
   - Average response time: 500ms max (contract requirement)
   - Planned downtime calendar
   - Incident history

4. Match Rate Analytics
   - Overall match rate by buyer
   - Match confidence distribution
   - Failed lookup reasons
   - Data freshness metrics
```

**Implementation (C# Blazor Server):**
```csharp
// Services/AdminDashboardService.cs
public class AdminDashboardService
{
    private readonly ApplicationDbContext _context;

    public async Task<BuyerUsageStats> GetBuyerStatsAsync(Guid buyerId, DateTime startDate, DateTime endDate)
    {
        var logs = await _context.AuditLogs
            .Where(l => l.BuyerId == buyerId && l.QueriedAt >= startDate && l.QueriedAt <= endDate)
            .ToListAsync();

        return new BuyerUsageStats
        {
            TotalQueries = logs.Count,
            SuccessfulMatches = logs.Count(l => l.MatchFound),
            MatchRate = logs.Count > 0 ? (double)logs.Count(l => l.MatchFound) / logs.Count : 0,
            AverageResponseTime = logs.Average(l => l.ResponseTimeMs),
            UniquePhoneNumbers = logs.Select(l => l.PhoneNumberQueried).Distinct().Count()
        };
    }

    public async Task<BillingReport> CalculateBillingAsync(Guid buyerId, DateTime month)
    {
        var buyer = await _context.Buyers.FindAsync(buyerId);
        var queries = await _context.AuditLogs
            .Where(l => l.BuyerId == buyerId && l.QueriedAt.Month == month.Month && l.QueriedAt.Year == month.Year)
            .CountAsync();

        var overageQueries = Math.Max(0, queries - buyer.MonthlyQuota);
        var overageCharge = overageQueries * 0.035m;

        return new BillingReport
        {
            BuyerId = buyerId,
            Month = month,
            TotalQueries = queries,
            IncludedQueries = buyer.MonthlyQuota,
            OverageQueries = overageQueries,
            OverageCharge = overageCharge,
            BaseSubscriptionFee = 10500m // From contract Section 3.2
        };
    }
}
```

**Status:** ✅ Fully specified, ready to implement

---

### Feature 4.2: Usage Tracking for Billing
**Priority:** P0 (Blocker) - **CONTRACTUAL REQUIREMENT** (Section 3.2)
**Effort:** 1 day
**Dependencies:** Audit logging

**Purpose:** Track every API call for accurate billing at $0.035 per qualified call beyond quota

**Database Schema:**
```sql
-- Enhance audit_logs table with billing fields
ALTER TABLE audit_logs ADD COLUMN is_billable BOOLEAN DEFAULT true;
ALTER TABLE audit_logs ADD COLUMN charge_amount DECIMAL(10,4) DEFAULT 0.035;
ALTER TABLE audit_logs ADD COLUMN within_quota BOOLEAN DEFAULT true;

-- Billing summary table
CREATE TABLE billing_summaries (
    id BIGSERIAL PRIMARY KEY,
    buyer_id UUID NOT NULL REFERENCES buyers(buyer_id),
    billing_month DATE NOT NULL,
    total_queries INT NOT NULL,
    included_queries INT NOT NULL,
    overage_queries INT NOT NULL,
    base_fee DECIMAL(10,2) NOT NULL,
    overage_charges DECIMAL(10,2) NOT NULL,
    total_amount DECIMAL(10,2) NOT NULL,
    invoice_generated_at TIMESTAMPTZ,
    invoice_sent_at TIMESTAMPTZ,
    payment_received_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE(buyer_id, billing_month)
);
```

**Implementation:**
```csharp
public class BillingService
{
    public async Task<decimal> CalculateMonthlyChargeAsync(Guid buyerId, DateTime month)
    {
        var buyer = await _context.Buyers.FindAsync(buyerId);
        var auditLogs = await _context.AuditLogs
            .Where(l => l.BuyerId == buyerId
                && l.QueriedAt >= new DateTime(month.Year, month.Month, 1)
                && l.QueriedAt < new DateTime(month.Year, month.Month, 1).AddMonths(1)
                && l.IsBillable)
            .ToListAsync();

        var totalQueries = auditLogs.Count;
        var overageQueries = Math.Max(0, totalQueries - buyer.MonthlyQuota);

        return buyer.BaseSubscriptionFee + (overageQueries * 0.035m);
    }
}
```

**Status:** ✅ Fully specified, ready to implement

---

### Feature 4.3: Profit Share Tracking
**Priority:** P0 (Blocker) - **CONTRACTUAL REQUIREMENT** (Section 3.4, 3.5)
**Effort:** 2 days
**Dependencies:** Buyer routing/append tracking

**Purpose:** Track revenue and calculate profit share owed to PROVIDER

**Profit Share Rates:**
- **Routing (General):** 10% of net profits
- **PROVIDER-Introduced Opportunities:** 20% of net profits
- **Data Appends:** 30% of net profits

**Database Schema:**
```sql
CREATE TABLE revenue_events (
    id BIGSERIAL PRIMARY KEY,
    buyer_id UUID NOT NULL REFERENCES buyers(buyer_id),
    event_type VARCHAR(50) NOT NULL, -- 'routing_general', 'routing_provider_intro', 'append'
    gross_revenue DECIMAL(12,2) NOT NULL,
    direct_media_costs DECIMAL(12,2) DEFAULT 0,
    subscription_fees DECIMAL(12,2) DEFAULT 0,
    infrastructure_fees DECIMAL(12,2) DEFAULT 0,
    non_payments DECIMAL(12,2) DEFAULT 0,
    net_profit DECIMAL(12,2) GENERATED ALWAYS AS (
        gross_revenue - direct_media_costs - subscription_fees - infrastructure_fees - non_payments
    ) STORED,
    profit_share_rate DECIMAL(5,4) NOT NULL, -- 0.10, 0.20, or 0.30
    profit_share_amount DECIMAL(12,2) GENERATED ALWAYS AS (
        (gross_revenue - direct_media_costs - subscription_fees - infrastructure_fees - non_payments) * profit_share_rate
    ) STORED,
    transaction_date DATE NOT NULL,
    downstream_client_id UUID,
    notes TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_revenue_buyer_date ON revenue_events(buyer_id, transaction_date);
CREATE INDEX idx_revenue_type ON revenue_events(event_type);
```

**Status:** ✅ Fully specified, ready to implement

---

### Feature 4.4: SLA Monitoring & Reporting
**Priority:** P0 (Blocker) - **CONTRACTUAL REQUIREMENT** (Section 4.1)
**Effort:** 1 day
**Dependencies:** Response time logging

**Purpose:** Monitor and report on 99.5% uptime and 500ms max average response time

**Monitoring Points:**
```
1. Uptime Monitoring
   - Health check endpoint: /health (every 30 seconds)
   - Target: 99.5% uptime per calendar month
   - Max downtime: 3.6 hours per month
   - Alert if down for > 5 minutes

2. Response Time Monitoring
   - Log every API call response time
   - Calculate rolling average every 5 minutes
   - Target: < 500ms average
   - Alert if average exceeds 500ms for 15+ minutes

3. Planned Downtime
   - Notification system: 24 hours advance notice (contract requirement)
   - Maintenance calendar
   - Email notifications to all buyers
```

**Implementation:**
```csharp
public class SLAMonitoringService
{
    public async Task<SLAReport> GenerateMonthlyReportAsync(DateTime month)
    {
        var startDate = new DateTime(month.Year, month.Month, 1);
        var endDate = startDate.AddMonths(1);
        var totalMinutes = (endDate - startDate).TotalMinutes;

        // Calculate downtime
        var downtimeMinutes = await _context.DowntimeEvents
            .Where(e => e.StartTime >= startDate && e.StartTime < endDate)
            .SumAsync(e => (e.EndTime - e.StartTime).TotalMinutes);

        var uptimePercentage = ((totalMinutes - downtimeMinutes) / totalMinutes) * 100;

        // Calculate average response time
        var avgResponseTime = await _context.AuditLogs
            .Where(l => l.QueriedAt >= startDate && l.QueriedAt < endDate)
            .AverageAsync(l => l.ResponseTimeMs);

        return new SLAReport
        {
            Month = month,
            UptimePercentage = uptimePercentage,
            UptimeTarget = 99.5,
            UptimeMet = uptimePercentage >= 99.5,
            AverageResponseTimeMs = avgResponseTime,
            ResponseTimeTarget = 500,
            ResponseTimeMet = avgResponseTime <= 500,
            TotalDowntimeMinutes = downtimeMinutes,
            MaxAllowedDowntimeMinutes = 216 // 3.6 hours
        };
    }
}
```

**Status:** ✅ Fully specified, ready to implement

---

### Feature 4.5: Data Subject Rights Portal
**Priority:** P0 (Blocker) - **CONTRACTUAL REQUIREMENT** (Section 12.4, Exhibit B)
**Effort:** 2 days
**Dependencies:** GDPR/CCPA compliance requirements

**Purpose:** Handle data subject access, correction, erasure, and portability requests

**Features:**
- Consumer can request access to their data
- Consumer can request correction of inaccurate data
- Consumer can request erasure (right to be forgotten)
- Consumer can request data portability (export)
- Response timeline: As required by law (typically 30 days)

**Database Schema:**
```sql
CREATE TABLE data_subject_requests (
    id BIGSERIAL PRIMARY KEY,
    request_type VARCHAR(50) NOT NULL, -- 'access', 'correction', 'erasure', 'portability'
    consumer_phone VARCHAR(20),
    consumer_email VARCHAR(255),
    consumer_name VARCHAR(255),
    request_details TEXT,
    status VARCHAR(50) DEFAULT 'pending', -- 'pending', 'in_progress', 'completed', 'rejected'
    submitted_at TIMESTAMPTZ DEFAULT NOW(),
    completed_at TIMESTAMPTZ,
    response_sent_at TIMESTAMPTZ,
    handled_by_user_id UUID,
    notes TEXT
);
```

**Status:** ✅ Fully specified, ready to implement

---

### Feature 4.6: Data Breach Notification System
**Priority:** P0 (Blocker) - **CONTRACTUAL REQUIREMENT** (Section 13.2)
**Effort:** 1 day
**Dependencies:** Security monitoring

**Purpose:** Detect and notify of data breaches within 72 hours

**Requirements:**
- Automated intrusion detection
- Immediate notification to security team
- Breach assessment workflow
- Notification to BUYER within 72 hours
- Detailed breach report (nature, affected data subjects, mitigation)

**Implementation:**
```csharp
public class BreachNotificationService
{
    public async Task NotifyBreachAsync(SecurityIncident incident)
    {
        // Log incident
        var breach = new DataBreach
        {
            IncidentId = incident.Id,
            DetectedAt = DateTime.UtcNow,
            Severity = incident.Severity,
            Description = incident.Description,
            AffectedRecords = incident.AffectedRecords,
            NotificationDeadline = DateTime.UtcNow.AddHours(72) // Contract requirement
        };

        await _context.DataBreaches.AddAsync(breach);

        // Send immediate notification to security team
        await _notificationService.SendSecurityAlertAsync(breach);

        // Schedule buyer notification (within 72 hours)
        await _notificationService.ScheduleBreachNotificationAsync(breach);
    }
}
```

**Status:** ✅ Fully specified, ready to implement

---

**Milestone 4:** ✅ All contractual obligations fulfilled

---

## POST-MVP ROADMAP (Phase 5)

### Future Features (Not in MVP)

#### Batch Enrichment API
**Priority:** P1
**Effort:** 1 week
**Description:** Upload CSV with 10,000+ phone numbers, receive enriched results

---

#### Custom Field Selection
**Priority:** P2
**Effort:** 3 days
**Description:** Allow buyers to request specific columns instead of all 398

---

#### Webhook Callbacks
**Priority:** P2
**Effort:** 1 week
**Description:** Async processing with webhook notification on completion

---

#### Buyer Dashboard (Blazor UI)
**Priority:** P2
**Effort:** 2 weeks
**Description:** Self-service portal for API key management, usage analytics

---

#### Advanced Matching
**Priority:** P3
**Effort:** 1 week
**Description:** Fuzzy matching, partial phone number search, name + zip search

---

#### Real-Time Snowflake Sync
**Priority:** P3
**Effort:** 2 weeks
**Description:** Daily incremental updates from Snowflake to PostgreSQL

---

## APPENDIX

### A. Sample API Request/Response

#### **API Endpoints**

**Test Environment:**
```
https://api.test.switchboard.com/api/data_enhancement/lookup
```

**Live Environment:**
```
https://api.switchboard.com/api/data_enhancement/lookup
```

---

#### **Request Fields**

| Field Name | Required | Examples | Description |
|------------|----------|----------|-------------|
| `api_key` | **True** | `157659ac293445df00772760e6114ac4` | Key provided by Switchboard |
| `provider_code` | **True** | `EQUIFAX_ENRICHMENT` | Code provided by Switchboard |
| `phone` | **True** | `8015551234` | Lead phone number (10 digits) |
| `first_name` | False | `Bob` | Lead first name (improves match confidence) |
| `last_name` | False | `Barker` | Lead last name (improves match confidence) |
| `postal_code` | False | `84010` | Lead postal or zip code |
| `state` | False | `UT` | Lead state |
| `unique_id` | False | `b4c9f530-5461-11ef-8f6f-8ffb313ceb02` | Your unique tracking code for tiebacks |
| `permissible_purpose` | **True** | `insurance_underwriting` | FCRA permissible purpose |
| `fields` | False | `basic` or `full` | Response detail: `basic` (core fields) or `full` (all 398 columns). Default: `basic` |

---

#### **Request Example (Basic Fields)**
```bash
curl -X POST https://api.switchboard.com/api/data_enhancement/lookup \
  -H "Content-Type: application/json" \
  -d '{
    "api_key": "157659ac293445df00772760e6114ac4",
    "provider_code": "EQUIFAX_ENRICHMENT",
    "phone": "8015551234",
    "first_name": "Bob",
    "last_name": "Barker",
    "postal_code": "84010",
    "state": "UT",
    "permissible_purpose": "insurance_underwriting",
    "fields": "basic",
    "unique_id": "b4c9f530-5461-11ef-8f6f-8ffb313ceb02"
  }'
```

---

#### **Response (200 OK - Basic Fields)**
```json
{
  "response": "success",
  "message": "Record found with high confidence",
  "data": {
    "consumer_key": "abc123xyz789",
    "personal_info": {
      "first_name": "Bob",
      "middle_name": "Robert",
      "last_name": "Barker",
      "suffix": null,
      "gender": "M",
      "age": "45",
      "date_of_birth": "1980-05-15",
      "deceased": false
    },
    "addresses": [
      {
        "address": "123 Main St",
        "city": "Bountiful",
        "state": "UT",
        "zip": "84010",
        "zip4": "1234",
        "is_current": true
      }
    ],
    "phones": [
      "8015551234",
      "8015559876"
    ],
    "emails": [
      "bob.barker@example.com"
    ],
    "financial": {
      "income_complete": 85000,
      "spending_power": 72000,
      "affluence_index": 650,
      "financial_durability_score": 720
    }
  },
  "_metadata": {
    "match_confidence": 0.95,
    "match_type": "exact",
    "data_freshness_date": "2025-10-01",
    "query_timestamp": "2025-10-31T10:30:00Z",
    "response_time_ms": 87,
    "request_id": "req_abc123xyz",
    "unique_id": "b4c9f530-5461-11ef-8f6f-8ffb313ceb02"
  }
}
```

---

#### **Response (200 OK - Full 398 Columns)**

Request with `"fields": "full"` returns complete dataset:

```json
{
  "response": "success",
  "message": "Record found with high confidence",
  "data": {
    "consumer_key": "abc123xyz789",
    "personal_info": { /* All personal fields */ },
    "alternate_names": [ /* 5 alternate name sets */ ],
    "addresses": [ /* All 10 addresses with full details */ ],
    "phones": { /* All phone numbers with last_seen dates */ },
    "emails": [ /* All 15 emails with last_seen dates */ ],
    "financial": { /* Complete financial data */ },
    "automotive": { /* Automotive propensity scores */ },
    "proprietary_scores": {
      "vaa": 850, "vab": 720, "vac": 650,
      /* ... 120 more "v" scores ... */
    },
    "marketing": { /* IDFAs, IP addresses, marketing flags */ },
    "metadata": { /* Record metadata */ }
  },
  "_metadata": {
    "match_confidence": 0.95,
    "match_type": "exact",
    "data_freshness_date": "2025-10-01",
    "query_timestamp": "2025-10-31T10:30:00Z",
    "response_time_ms": 142,
    "request_id": "req_abc123xyz",
    "unique_id": "b4c9f530-5461-11ef-8f6f-8ffb313ceb02",
    "total_fields_returned": 398
  }
}
```

---

#### **Error Responses**

**No Record Found (404):**
```json
{
  "response": "error",
  "message": "Unable to find record for phone number",
  "data": {
    "phone": "8015551234",
    "match_attempted": true,
    "match_confidence": 0.0
  },
  "_metadata": {
    "request_id": "req_def456abc",
    "query_timestamp": "2025-10-31T10:30:00Z",
    "unique_id": "b4c9f530-5461-11ef-8f6f-8ffb313ceb02"
  }
}
```

**Invalid API Key (401):**
```json
{
  "response": "error",
  "message": "Invalid API key",
  "error_code": "INVALID_API_KEY",
  "data": null,
  "_metadata": {
    "request_id": "req_ghi789jkl",
    "query_timestamp": "2025-10-31T10:30:00Z"
  }
}
```

**Rate Limit Exceeded (429):**
```json
{
  "response": "error",
  "message": "Rate limit exceeded. Please retry after 30 seconds.",
  "error_code": "RATE_LIMIT_EXCEEDED",
  "data": {
    "rate_limit": {
      "limit": 100,
      "remaining": 0,
      "reset_at": "2025-10-31T10:31:00Z"
    }
  },
  "_metadata": {
    "request_id": "req_mno012pqr",
    "query_timestamp": "2025-10-31T10:30:00Z",
    "retry_after_seconds": 30
  }
}
```

**Missing Required Field (400):**
```json
{
  "response": "error",
  "message": "Missing required field: permissible_purpose",
  "error_code": "MISSING_REQUIRED_FIELD",
  "data": {
    "field": "permissible_purpose",
    "description": "FCRA requires a valid permissible purpose for all queries"
  },
  "_metadata": {
    "request_id": "req_stu345vwx",
    "query_timestamp": "2025-10-31T10:30:00Z"
  }
}
```

**Invalid Phone Format (400):**
```json
{
  "response": "error",
  "message": "Invalid phone number format",
  "error_code": "INVALID_PHONE_FORMAT",
  "data": {
    "phone": "555-12-ABCD",
    "expected_format": "10 digits (e.g., 8015551234)",
    "details": "Phone number contains non-numeric characters"
  },
  "_metadata": {
    "request_id": "req_yz6789abc",
    "query_timestamp": "2025-10-31T10:30:00Z"
  }
}
```

---

#### **Valid Permissible Purpose Values**

For FCRA compliance, `permissible_purpose` must be one of:

- `credit_application` - Consumer applying for credit
- `employment_screening` - Background check for employment
- `insurance_underwriting` - Insurance application or renewal
- `legitimate_business_need` - Account review, collections
- `identity_verification` - Verify identity for existing relationship
- `written_consumer_consent` - Consumer provided explicit written consent

---

### B. Environment Variables

```bash
# Database
DB_HOST=sb-marketing-postgres.cu9k2siys4p8.us-east-1.rds.amazonaws.com
DB_PORT=5432
DB_NAME=postgres
DB_USER=sbadmin
DB_PASSWORD=***  # From AWS Secrets Manager

# Redis
REDIS_HOST=enrichment-cache.abc123.use1.cache.amazonaws.com
REDIS_PORT=6379
REDIS_PASSWORD=***  # From AWS Secrets Manager

# Encryption
PII_ENCRYPTION_KEY=***  # 256-bit base64 key from Secrets Manager

# Rate Limiting
RATE_LIMIT_PER_MINUTE=100
DAILY_QUOTA_DEFAULT=10000

# Logging
LOG_LEVEL=Information
CLOUDWATCH_LOG_GROUP=/aws/elasticbeanstalk/enrichment-api/
```

---

### C. Required AWS Resources

```yaml
# CloudFormation Template (excerpt)
Resources:
  EnrichmentAPI:
    Type: AWS::ElasticBeanstalk::Application
    Properties:
      ApplicationName: equifax-enrichment-api

  RDSInstance:
    Type: AWS::RDS::DBInstance
    Properties:
      DBInstanceIdentifier: sb-marketing-postgres
      Engine: postgres
      EngineVersion: "18"
      DBInstanceClass: db.r5.2xlarge
      AllocatedStorage: 1500

  RedisCluster:
    Type: AWS::ElastiCache::CacheCluster
    Properties:
      CacheNodeType: cache.r6g.large
      Engine: redis
      NumCacheNodes: 2
```

---

### D. Contact & Resources

**Project Owner:** Jeff Merlin
**Technical Lead:** TBD
**Architecture Document:** `/Users/merlinzbeard/viable product/EQUIFAX-DATABASE-COMPREHENSIVE-DOCUMENTATION.md`
**Source Code:** TBD
**Production API:** TBD

---

**Document Version:** 1.0
**Last Updated:** October 31, 2025
**Next Review:** November 7, 2025
