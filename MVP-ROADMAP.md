# EQUIFAX ENRICHMENT API - MVP ROADMAP

**Version:** 1.0 MVP
**Last Updated:** November 1, 2025
**Status:** In Production - Final MVP Features Pending

---

## EXECUTIVE SUMMARY

### MVP Definition
Minimum Viable Product includes only features **absolutely required** to:
1. Accept and process enrichment requests securely
2. Meet contractual SLA requirements (< 500ms response, 99.5% uptime)
3. Comply with FCRA legal requirements
4. Protect API resources from abuse
5. Enable revenue generation through usage tracking

### MVP Success Criteria
- ✅ API responds to enrichment requests
- ✅ Authentication protects endpoints
- 🔴 Rate limiting prevents abuse and enables billing
- 🔴 FCRA audit logging ensures legal compliance
- ✅ Swagger documentation available to buyers
- ✅ Production deployment on AWS ECS
- 🟡 Custom domain for professional branding

---

## PHASE 1: CORE API FUNCTIONALITY ✅ COMPLETED

### Feature 1.1: REST API Endpoint ✅ DEPLOYED
**Status:** Production
**BDD Feature:** `features/phase1/feature-1.1-rest-api-endpoint.feature`

**Capabilities:**
- `POST /api/data_enhancement/lookup`
- Accepts phone + optional fields (first_name, last_name, postal_code, state)
- Returns 398-field enrichment data or error response
- Response time: < 200ms (basic), < 300ms (full dataset)

**Completed Scenarios:**
- ✅ Basic phone lookup
- ✅ Enhanced matching with optional fields
- ✅ No-match handling
- ✅ Validation errors (400 Bad Request)
- ✅ Response time performance (<200ms p95)

---

### Feature 1.2: Phone Number Normalization ✅ DEPLOYED
**Status:** Production
**BDD Feature:** `features/phase1/feature-1.2-phone-number-normalization.feature`

**Capabilities:**
- Handles multiple formats: `(555) 123-4567`, `555-123-4567`, `+1-555-123-4567`
- Normalizes to: `5551234567` (10 digits, no formatting)
- Validates NANP format (US/Canada)
- Rejects invalid formats

**Completed Scenarios:**
- ✅ Normalize formatted numbers
- ✅ Handle international prefixes (+1, 1-)
- ✅ Reject invalid formats (too short, too long, non-numeric)
- ✅ Preserve leading zeros in area codes

---

### Feature 1.3: Database Query (Multi-Phone) ✅ DEPLOYED
**Status:** Production
**BDD Feature:** `features/phase1/feature-1.3-database-query-multi-phone.feature`

**Capabilities:**
- Searches across multiple phone columns (phone1, phone2, phone3, etc.)
- Uses PostgreSQL B-tree index for O(log n) lookups
- Returns first match with highest confidence
- Supports 326M+ records in production database

**Completed Scenarios:**
- ✅ Single phone column match
- ✅ Multi-phone column search
- ✅ Index performance (< 50ms query time)
- ✅ No-match handling

---

### Feature 1.4: PII Decryption ✅ DEPLOYED
**Status:** Production
**BDD Feature:** `features/phase1/feature-1.4-pii-decryption.feature`

**Capabilities:**
- AES-256-GCM decryption for encrypted PII fields
- Decrypts: SSN, DOB, full address, email
- Keys stored in AWS Secrets Manager
- Automatic key rotation support

**Completed Scenarios:**
- ✅ Decrypt sensitive fields
- ✅ Handle non-encrypted fields gracefully
- ✅ Key rotation support
- ✅ Error handling for decryption failures

---

## PHASE 2: SECURITY & COMPLIANCE 🔴 CRITICAL MVP GAPS

### Feature 2.1: API Key Authentication ✅ DEPLOYED
**Status:** Production
**BDD Feature:** `features/phase2/feature-2.1-api-key-authentication.feature`

**Capabilities:**
- X-API-Key header validation
- SHA-256 hashed keys (never store plaintext)
- Timing-attack prevention (constant-time comparison)
- Per-buyer authentication

**Completed Scenarios:**
- ✅ Valid API key authentication
- ✅ Missing API key rejection (401)
- ✅ Invalid API key rejection (401)
- ✅ Inactive buyer rejection (401)
- ✅ Timing-attack prevention
- ✅ Swagger UI bypasses auth (public documentation)

**Production Status:** Fully functional, protecting all API endpoints

---

### Feature 2.2: Rate Limiting 🔴 BLOCKING MVP
**Status:** NOT STARTED - CRITICAL
**BDD Feature:** `features/phase2/feature-2.2-rate-limiting.feature`
**Priority:** HIGH - Required before public launch

**Why Critical:**
1. **Prevents API abuse** - Without rate limiting, single buyer can overload system
2. **Enables billing** - Overage tracking required for revenue ($0.035/call over quota)
3. **Contractual requirement** - Section 3.2 specifies overage pricing
4. **Resource protection** - Prevents database exhaustion

**Required Capabilities:**
- ✅ Redis-based distributed rate limiting
- ✅ Per-buyer limits: 1,000/min, 60,000/hour, 1M/day
- ✅ Overage billing tracking ($0.035 per qualified call)
- ✅ HTTP 429 responses with rate limit headers
- ✅ Lua scripts for atomic operations (no race conditions)
- ✅ Sliding window algorithm (prevents thundering herd)

**Implementation Estimate:** 2-3 days
**Blocking:** YES - Cannot launch without this

**Scenarios to Implement:**
- Allow requests within rate limit (200 OK with X-RateLimit-* headers)
- Reject requests exceeding limit (429 Too Many Requests)
- Atomic operations (Lua scripts prevent race conditions)
- Sliding window counter (precise enforcement)
- Per-API-key limiting (not IP-based)
- Overage billing tracking in Redis
- Graceful degradation (fail-open if Redis unavailable)

---

### Feature 2.3: FCRA Audit Logging 🔴 BLOCKING MVP
**Status:** NOT STARTED - CRITICAL
**BDD Feature:** `features/phase2/feature-2.3-fcra-audit-logging.feature`
**Priority:** HIGH - Legal compliance requirement

**Why Critical:**
1. **Federal law requirement** - FCRA § 607(b) mandates 24-month retention
2. **Regulatory defense** - Audit trail protects against FTC enforcement
3. **Permissible purpose tracking** - Required by FCRA § 604
4. **Contractual obligation** - Section 6.1 compliance requirements

**Required Capabilities:**
- ✅ Log every API request with full context
- ✅ Track: buyer_id, phone, permissible_purpose, IP, timestamp, response
- ✅ 24-month retention (FCRA § 607(b) requirement)
- ✅ Immutable audit trail (append-only)
- ✅ Permissible purpose validation
- ✅ Consumer rights portal (access requests)

**Implementation Estimate:** 2-3 days
**Blocking:** YES - Legal liability without this

**Scenarios to Implement:**
- Log all enrichment requests (success and failure)
- Store permissible purpose with every query
- Prevent modification of audit logs (append-only table)
- Automatic 24-month retention enforcement
- Consumer access request handling
- Buyer compliance monitoring

---

## PHASE 4: DEPLOYMENT ✅ COMPLETED

### Feature 4.3: AWS Deployment ✅ DEPLOYED
**Status:** Production
**BDD Feature:** `features/phase4/feature-4.3-aws-deployment.feature`

**Capabilities:**
- AWS ECS Fargate container orchestration
- Application Load Balancer with health checks
- PostgreSQL RDS integration (326M+ records)
- Docker containerization
- Zero-downtime deployments

**Completed Scenarios:**
- ✅ ECS task definition with init container for migrations
- ✅ ALB health check monitoring
- ✅ Database connection from container
- ✅ Environment variable configuration
- ✅ Image versioning and deployment

**Production URLs:**
- ALB: `http://equifax-enrichment-api-lb-1485595057.us-east-1.elb.amazonaws.com`
- Swagger: `http://equifax-enrichment-api-lb-1485595057.us-east-1.elb.amazonaws.com/swagger`
- Health: `http://equifax-enrichment-api-lb-1485595057.us-east-1.elb.amazonaws.com/health`

---

## ADDITIONAL MVP TASKS

### Custom Domain Setup 🟡 IN PROGRESS
**Status:** Awaiting DNS validation
**Priority:** MEDIUM - Professional branding

**Capabilities:**
- Custom domain: `api.theswitchboardmarketing.com`
- SSL/TLS certificate from AWS ACM
- HTTPS listener on ALB
- DNS configuration in Squarespace

**Remaining Steps:**
1. ⏳ Add DNS validation CNAME record in Squarespace
2. ⏳ Wait for certificate validation (5-30 minutes)
3. ⏳ Add HTTPS listener to ALB
4. ⏳ Create CNAME record pointing to ALB
5. ⏳ Test HTTPS access

**Blocking:** NO - Can launch with ALB DNS, add custom domain later

---

### Public API Documentation ✅ DEPLOYED
**Status:** Production

**Capabilities:**
- Swagger UI at `/swagger`
- Quick Start Guide embedded
- Full 398-field enrichment example
- Interactive endpoint testing
- Authentication instructions
- No authentication required (public access)

---

## MVP COMPLETION CHECKLIST

### ✅ COMPLETED (6/9)
- [x] REST API Endpoint (1.1)
- [x] Phone Number Normalization (1.2)
- [x] Database Query (1.3)
- [x] PII Decryption (1.4)
- [x] API Key Authentication (2.1)
- [x] AWS Deployment (4.3)

### 🔴 BLOCKING MVP LAUNCH (2/9)
- [ ] **Rate Limiting (2.2)** - CRITICAL
- [ ] **FCRA Audit Logging (2.3)** - CRITICAL

### 🟡 NICE TO HAVE (1/9)
- [ ] Custom Domain Setup - IN PROGRESS

---

## IMMEDIATE NEXT STEPS

### 1. Implement Rate Limiting (Feature 2.2)
**Estimate:** 2-3 days
**Blocking:** YES

**Tasks:**
1. Deploy AWS ElastiCache Redis cluster (cache.r6g.large)
2. Create RateLimitingMiddleware with Lua scripts
3. Implement sliding window counter algorithm
4. Add overage billing tracking
5. Add rate limit headers to responses
6. Test distributed behavior (multiple ECS tasks)
7. Deploy to production

---

### 2. Implement FCRA Audit Logging (Feature 2.3)
**Estimate:** 2-3 days
**Blocking:** YES

**Tasks:**
1. Create AuditLog database table (append-only)
2. Create AuditLoggingMiddleware
3. Log every API request with full context
4. Implement 24-month retention policy
5. Add permissible purpose validation
6. Create consumer rights portal endpoint
7. Test audit trail immutability
8. Deploy to production

---

### 3. Complete Custom Domain Setup
**Estimate:** 1 hour (after DNS validation)
**Blocking:** NO

**Tasks:**
1. Add DNS validation record in Squarespace
2. Wait for ACM certificate validation
3. Add HTTPS listener to ALB with certificate
4. Create CNAME: `api.theswitchboardmarketing.com` → ALB DNS
5. Test HTTPS access
6. Update Swagger documentation with new URL

---

## LAUNCH READINESS

### Pre-Launch Checklist
- [x] API functional and deployed
- [x] Authentication protecting endpoints
- [x] Public documentation available
- [ ] **Rate limiting protecting resources** 🔴 REQUIRED
- [ ] **FCRA audit logging compliant** 🔴 REQUIRED
- [x] Health checks monitoring uptime
- [x] Database optimized and indexed
- [ ] Custom domain configured 🟡 OPTIONAL

### Post-Launch Monitoring
- Monitor rate limit hit rates (CloudWatch)
- Track API response times (< 500ms SLA)
- Monitor uptime (99.5% SLA requirement)
- Review FCRA audit logs weekly
- Track overage billing accuracy

---

## ESTIMATED TIMELINE TO MVP LAUNCH

**Current Status:** 67% Complete (6/9 features)

**Remaining Work:**
- Feature 2.2 (Rate Limiting): 2-3 days
- Feature 2.3 (FCRA Audit Logging): 2-3 days
- Testing & Validation: 1 day

**Total:** 5-7 days to MVP launch

**LAUNCH BLOCKER:** Cannot launch until Features 2.2 and 2.3 are complete.

---

## SUCCESS METRICS (POST-LAUNCH)

**Contractual SLA Requirements:**
- API response time: < 500ms average (Section 4.1)
- Uptime: 99.5% per calendar month (Section 4.1)
- Zero FCRA compliance violations (Section 6.1)

**Business Metrics:**
- Daily queries: 10K+ within first month
- Match rate: > 85% for valid phone numbers
- Overage revenue: Track $0.035/call billing accuracy

**Technical Metrics:**
- Rate limit effectiveness: < 1% abuse attempts succeed
- Audit log completeness: 100% of requests logged
- Database performance: < 50ms query time (p95)

---

## REFERENCE DOCUMENTS

- Full Roadmap: `EQUIFAX-ENRICHMENT-API-ROADMAP.md` (12,275 lines - comprehensive)
- 2nd Iteration Roadmap: `ITERATION-2-ROADMAP.md` (deferred features)
- BDD Features: `features/` directory
- Architecture: See main roadmap for Clean Architecture layers
