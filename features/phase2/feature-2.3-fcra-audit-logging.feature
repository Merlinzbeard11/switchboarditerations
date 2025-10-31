Feature: FCRA Audit Logging (Immutable, High-Performance)
  As a compliance-focused API system
  I want to log all API queries with immutable audit trails
  So that FCRA and SOC2 compliance requirements are met

  Background:
    Given FCRA audit logging is enabled
    And PostgreSQL table partitioning is configured (monthly partitions)
    And immutability triggers are enabled (reject UPDATE/DELETE)
    And fire-and-forget async logging is configured (Channel<T>)
    And batch processing is enabled (100 logs per batch or 1-second timeout)
    And phone numbers are hashed using SHA-256 for privacy
    And 24-month retention policy is configured
    And encryption at rest is enabled (AWS RDS encryption)

  # ============================================================================
  # SCENARIO 1: Successful API Query Logged (Happy Path)
  # ============================================================================
  Scenario: Log every API query with comprehensive audit trail
    Given buyer 12345 queries phone number "8015551234"
    And the permissible purpose is "insurance_underwriting"
    And the query results in a match (consumer record found)
    When the API query completes successfully
    Then an audit log entry should be created with:
      | field                      | value                              |
      | buyer_id                   | 12345                              |
      | phone_number_queried_hash  | <SHA-256 hash of phone>            |
      | consumer_key_returned      | <UUID>                             |
      | match_found                | true                               |
      | permissible_purpose        | insurance_underwriting             |
      | ip_address                 | <source_ip>                        |
      | user_agent                 | <client_user_agent>                |
      | request_id                 | <UUID>                             |
      | response_time_ms           | <integer>                          |
      | queried_at                 | <PostgreSQL NOW() timestamp>       |
      | log_version                | 1                                  |
    And the audit log should be persisted to PostgreSQL
    And the plaintext phone number should NOT be stored in audit logs

  # ============================================================================
  # SCENARIO 2: CRITICAL - Fire-and-Forget Async Logging (Performance)
  # ============================================================================
  Scenario: Audit logging does NOT block API response
    Given buyer 12345 makes an API query
    When the query processing completes
    Then the audit log should be enqueued to Channel<T> (non-blocking)
    And the API response should be returned immediately (does NOT wait for DB write)
    And the audit logging overhead should be less than 1ms
    And the background service should process the queue asynchronously
    And this prevents synchronous `await _auditRepository.LogAsync()` blocking

  # ============================================================================
  # SCENARIO 3: Batch Processing for High Throughput
  # ============================================================================
  Scenario: Process audit logs in batches for optimal performance
    Given the audit log queue contains 150 pending entries
    When the background service processes the queue
    Then the first batch should contain 100 entries (batch size limit)
    And the batch should be persisted using EF Core AddRangeAsync (bulk insert)
    And the second batch should contain the remaining 50 entries
    And the system should achieve 10,000+ audit logs/second sustained throughput
    And the batch timeout should be 1 second (if queue < 100 entries)

  # ============================================================================
  # SCENARIO 4: Bounded Channel Prevents Memory Exhaustion
  # ============================================================================
  Scenario: Bounded channel queue protects against memory exhaustion
    Given the audit log queue has capacity: 10,000 entries
    And the queue is currently full (10,000 entries)
    When a new audit log entry is enqueued
    Then the oldest entry should be dropped (FullMode.DropOldest)
    And the new entry should be added to the queue
    And an alert should be logged: "Audit queue at capacity - dropped oldest entry"
    And this prevents unbounded memory growth during traffic spikes

  # ============================================================================
  # SCENARIO 5: PostgreSQL Monthly Partitioning (Performance at Scale)
  # ============================================================================
  Scenario: Use monthly table partitions for query performance
    Given the audit logs table is partitioned by month
    And partitions exist for: audit_logs_2025_10, audit_logs_2025_11, audit_logs_2025_12
    When I query audit logs for buyer 12345 for November 2025
    Then PostgreSQL should use partition pruning (query only audit_logs_2025_11)
    And the query should NOT scan other month partitions (O(1) instead of O(N))
    And the query should complete in less than 50ms even with billions of rows
    And this prevents table bloat performance degradation

  # ============================================================================
  # SCENARIO 6: CRITICAL - Immutability Enforcement (FCRA Compliance)
  # ============================================================================
  Scenario: PostgreSQL trigger rejects UPDATE operations on audit logs
    Given an audit log entry exists with audit_log_id 12345
    When I attempt to UPDATE the audit log entry
    Then PostgreSQL should reject the operation with exception
    And the error message should be "Audit logs are immutable - modifications not allowed (FCRA compliance)"
    And the trigger should fire BEFORE UPDATE
    And the audit log should remain unchanged

  Scenario: PostgreSQL trigger rejects DELETE operations on audit logs
    Given an audit log entry exists with audit_log_id 12345
    When I attempt to DELETE the audit log entry
    Then PostgreSQL should reject the operation with exception
    And the error message should be "Audit logs are immutable - modifications not allowed (FCRA compliance)"
    And the trigger should fire BEFORE DELETE
    And the audit log should remain in the database

  # ============================================================================
  # SCENARIO 7: Database-Level Permissions (Role-Based Access)
  # ============================================================================
  Scenario: Enforce database-level permissions for immutability
    Given the database has role "api_service_role" for API operations
    And the database has role "auditor_role" for compliance auditors
    Then api_service_role should have permissions: SELECT, INSERT only
    And api_service_role should NOT have permissions: UPDATE, DELETE
    And auditor_role should have permissions: SELECT only
    And auditor_role should NOT have permissions: INSERT, UPDATE, DELETE
    And this enforces immutability at database security level

  # ============================================================================
  # SCENARIO 8: Privacy-Preserving SHA-256 Phone Number Hashing
  # ============================================================================
  Scenario: Hash phone numbers for privacy-preserving audit
    Given I need to log phone number "8015551234" in audit log
    When the phone number is hashed using SHA-256
    Then the hash should be computed: SHA-256("8015551234")
    And the hash should be stored as lowercase hexadecimal string
    And the hash should be 64 characters long (256 bits = 64 hex chars)
    And the hash should be irreversible (cannot recover plaintext from hash)
    And the same phone number should always produce the same hash (deterministic)
    And this allows querying audit logs without exposing plaintext PII

  # ============================================================================
  # SCENARIO 9: PostgreSQL Server Time for Clock Synchronization
  # ============================================================================
  Scenario: Use PostgreSQL server time to avoid clock skew
    Given application server A has system clock at 10:00:00
    And application server B has system clock at 10:00:05 (5-second skew)
    When both servers log audit entries
    Then both entries should use PostgreSQL NOW() for queried_at timestamp
    And the timestamps should be consistent (single source of truth)
    And this prevents clock skew issues in distributed environments

  # ============================================================================
  # SCENARIO 10: What to Log vs What NOT to Log (Security Best Practice)
  # ============================================================================
  Scenario: Log required fields without exposing excessive PII
    Given buyer 12345 queries phone number "8015551234" and receives SSN data
    When the audit log is created
    Then the audit log MUST include:
      | field                      | description                         |
      | buyer_id                   | WHO (who made the query)            |
      | phone_number_queried_hash  | WHAT (SHA-256 hash, not plaintext)  |
      | match_found                | RESULT (was record found)           |
      | permissible_purpose        | WHY (FCRA § 604 purpose)            |
      | queried_at                 | WHEN (timestamp)                    |
    But the audit log should NEVER include:
      | forbidden_field            | reason                              |
      | ssn_plaintext              | Excessive PII exposure risk         |
      | full_address               | Excessive PII exposure risk         |
      | phone_number_plaintext     | Use hash instead                    |
      | password                   | Never log credentials               |
      | api_key                    | Never log credentials               |

  # ============================================================================
  # SCENARIO 11: 24-Month Retention Policy (FCRA Requirement)
  # ============================================================================
  Scenario: Retain audit logs for 24 months per FCRA requirements
    Given FCRA requires 24-month minimum retention for audit logs
    And audit logs exist from October 2023 (25 months ago)
    When the automated retention policy runs
    Then the October 2023 partition should be archived to S3 cold storage
    And the partition should be detached: ALTER TABLE audit_logs DETACH PARTITION audit_logs_2023_10
    And the archived data should be exported to S3: s3://audit-archive/2023-10.parquet
    And the partition should be dropped to free up PostgreSQL storage
    And recent audit logs (< 24 months) should remain in hot PostgreSQL storage

  # ============================================================================
  # SCENARIO 12: No Match Found - Still Logged
  # ============================================================================
  Scenario: Log queries even when no match is found
    Given buyer 12345 queries phone number "5559999999"
    And the phone number does NOT exist in the database (no match)
    When the API returns "no match found"
    Then an audit log entry should still be created with:
      | field                      | value                              |
      | match_found                | false                              |
      | consumer_key_returned      | NULL                               |
      | permissible_purpose        | insurance_underwriting             |
    And the query should be logged for FCRA compliance (all queries logged)

  # ============================================================================
  # SCENARIO 13: Query Performance with Indexes
  # ============================================================================
  Scenario: Fast audit log queries using optimized indexes
    Given the audit logs table contains 100 million rows
    And indexes exist on: buyer_id, phone_number_queried_hash, request_id
    When I query audit logs for buyer 12345 for the last 30 days
    Then the query should use index: idx_audit_logs_buyer_id
    And the query should complete in less than 50ms
    And PostgreSQL should use partition pruning to limit scan to 1-month partition

  # ============================================================================
  # SCENARIO 14: Dead Letter Queue for Failed Audit Writes
  # ============================================================================
  Scenario: Handle failed audit log writes without losing data
    Given the background audit service attempts to persist a batch of 100 logs
    And the database connection fails (network error, timeout)
    When the batch write fails
    Then the error should be logged with severity: CRITICAL
    And the failed batch should be written to dead-letter queue
    And an alert should be sent to operations team
    And the batch should be retried with exponential backoff
    And audit logs should NEVER be lost (FCRA requirement)

  # ============================================================================
  # SCENARIO 15: Encryption at Rest (Security Requirement)
  # ============================================================================
  Scenario: Audit logs encrypted at rest using AWS RDS encryption
    Given AWS RDS PostgreSQL encryption is enabled
    When audit logs are persisted to PostgreSQL
    Then all data should be encrypted at rest using AES-256
    And the encryption key should be managed by AWS KMS
    And backups should also be encrypted
    And this protects audit logs from physical disk access

  # ============================================================================
  # SCENARIO 16: Encryption in Transit (TLS 1.3)
  # ============================================================================
  Scenario: Audit logs transmitted over encrypted connections
    Given the application connects to PostgreSQL over TLS 1.3
    When audit log data is transmitted from application to database
    Then the connection should use TLS 1.3 encryption
    And certificate validation should be enforced
    And this protects audit logs from network eavesdropping

  # ============================================================================
  # SCENARIO 17: Audit Log Access is Audited (Meta-Logging)
  # ============================================================================
  Scenario: Log who accesses audit logs (meta-auditing)
    Given compliance auditor "jane@company.com" queries audit logs
    When Jane runs a SELECT query on audit_logs table
    Then a meta-audit log entry should be created with:
      | field              | value                              |
      | auditor_email      | jane@company.com                   |
      | query_type         | SELECT                             |
      | filters_used       | buyer_id = 12345                   |
      | timestamp          | <current_utc_timestamp>            |
      | ip_address         | <auditor_ip>                       |
    And this creates audit trail of who accessed audit logs

  # ============================================================================
  # SCENARIO 18: Tamper Detection with Cryptographic Checksums
  # ============================================================================
  Scenario: Detect tampering using cryptographic checksums
    Given audit logs are persisted to PostgreSQL
    When each partition is closed (end of month)
    Then a cryptographic checksum should be calculated for the partition
    And the checksum should be stored separately (tamper-evident)
    And periodic verification should compare checksums
    And checksum mismatch should trigger security alert

  # ============================================================================
  # SCENARIO 19: Request ID Tracing Across Systems
  # ============================================================================
  Scenario: Trace requests across distributed systems using request_id
    Given buyer 12345 makes an API request
    And a request_id UUID is generated: "550e8400-e29b-41d4-a716-446655440000"
    When the request is processed through multiple services
    Then the request_id should be propagated to all downstream services
    And the audit log should store the request_id
    And distributed traces should be correlated using request_id
    And this enables end-to-end request tracing

  # ============================================================================
  # SCENARIO 20: Bulk Insert Performance Optimization
  # ============================================================================
  Scenario: Achieve high-throughput bulk inserts using PostgreSQL COPY
    Given the audit log queue contains 10,000 entries
    When the background service processes the batch
    Then EF Core AddRangeAsync should be used for bulk insert
    And PostgreSQL should use COPY command internally (high throughput)
    And the batch should be inserted in less than 100ms
    And this achieves 10,000+ audit logs/second sustained throughput

  # ============================================================================
  # SCENARIO 21: Monitoring - Alert on Audit Queue Backpressure
  # ============================================================================
  Scenario: Alert when audit queue exceeds capacity threshold
    Given the audit log queue capacity is 10,000 entries
    And the queue currently contains 6,000 entries (60% full)
    When the queue size exceeds 5,000 entries (50% threshold)
    Then a CloudWatch metric should be published: "AuditQueueSize"
    And an alert should trigger: "Audit queue backpressure detected - 6000/10000 entries"
    And the operations team should be notified
    And this prevents queue overflow and audit log loss

  # ============================================================================
  # SCENARIO 22: Monitoring - Verify Audit Log Count Matches API Requests
  # ============================================================================
  Scenario: Daily verification that audit logs match API request count
    Given the system processed 100,000 API requests today
    And the audit log count for today is 99,950 (50 missing)
    When the daily verification job runs
    Then a discrepancy alert should be triggered
    And the alert should state: "Audit log count mismatch - 100,000 requests, 99,950 logs (50 missing)"
    And the missing requests should be investigated
    And this ensures no API queries are missed in audit logs

  # ============================================================================
  # SCENARIO 23: SOC2 Type II Compliance Requirements
  # ============================================================================
  Scenario: Meet SOC2 Type II audit trail requirements
    Given SOC2 Type II requires 3-6 months of evidence collection
    When the SOC2 auditor requests audit trail evidence
    Then audit logs should provide immutable, timestamped trail
    And the audit logs should include: WHO, WHAT, WHEN, WHY, RESULT
    And 6 months of audit logs should be readily available (hot storage)
    And audit log access controls should be documented
    And automated retention policies should be demonstrated

  # ============================================================================
  # SCENARIO 24: Partitioned Query Performance Test
  # ============================================================================
  Scenario: Verify partition pruning performance at scale
    Given the audit_logs table has 12 partitions (1 per month)
    And each partition contains 100 million rows (1.2 billion total)
    When I query audit logs for buyer 12345 for November 2025
    Then PostgreSQL EXPLAIN should show partition pruning
    And only audit_logs_2025_11 partition should be scanned
    And the other 11 partitions should be pruned (not scanned)
    And the query should complete in less than 50ms

  # ============================================================================
  # SCENARIO 25: Archive to S3 for Cost Optimization
  # ============================================================================
  Scenario: Archive old audit logs to S3 for cost-effective long-term storage
    Given audit logs older than 24 months should be archived
    And the October 2023 partition is ready for archiving
    When the automated archive job runs
    Then the partition should be exported to S3: s3://audit-archive/2023-10.parquet
    And the export format should be Parquet (columnar, compressed)
    And the S3 storage class should be Glacier (lowest cost for archival)
    And the partition should be detached and dropped from PostgreSQL
    And this reduces PostgreSQL storage costs by 90% for archived data
    And archived data remains accessible for compliance audits
