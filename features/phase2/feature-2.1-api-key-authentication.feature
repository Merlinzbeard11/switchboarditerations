Feature: API Key Authentication
  As an API security system
  I want to authenticate API clients using secure API keys with timing-attack resistance
  So that only authorized buyers can access Equifax enrichment data

  Background:
    Given the API key authentication middleware is configured
    And timing-attack-resistant comparison is enabled (CryptographicOperations.FixedTimeEquals)
    And API keys are stored as SHA-256 hashes in the database
    And authentication audit logging is enabled with 24-month retention
    And automated key rotation is scheduled for 90-day intervals
    And anti-brute-force protection is active (5 failed attempts = 15-minute block)

  # ============================================================================
  # SCENARIO 1: Successful Authentication with Valid API Key (Happy Path)
  # ============================================================================
  Scenario: Successfully authenticate request with valid API key
    Given I have a valid API key "157659ac293445df00772760e6114ac4"
    And the API key hash exists in the database
    And the associated buyer account is active (IsActive = true)
    And the API key is less than 90 days old
    When I send a request to "/api/data_enhancement/lookup" with X-API-Key header
    Then the authentication should succeed
    And the buyer information should be stored in request context
    And the authenticated timestamp should be recorded
    And an audit log entry should be created with:
      | field         | value                        |
      | buyer_id      | <buyer_id_for_key>           |
      | ip_address    | <source_ip>                  |
      | user_agent    | <client_user_agent>          |
      | result        | success                      |
      | timestamp     | <current_utc_timestamp>      |
    And the request should proceed to downstream middleware

  # ============================================================================
  # SCENARIO 2: Missing API Key (401 Unauthorized)
  # ============================================================================
  Scenario: Reject request with missing API key
    Given I send a request without X-API-Key header
    When the authentication middleware processes the request
    Then the response status should be 401
    And the error message should be "Missing API key"
    And the response should include guidance: "Include X-API-Key header with valid API key"
    And no database query should be executed
    And an audit log entry should be created with:
      | result         | missing_api_key             |
      | failure_reason | missing_api_key             |
    And the request should NOT proceed to downstream middleware

  # ============================================================================
  # SCENARIO 3: Invalid API Key (401 Unauthorized)
  # ============================================================================
  Scenario: Reject request with invalid API key
    Given I have an invalid API key "invalid_key_xyz123"
    And the API key hash does NOT exist in the database
    When I send a request with the invalid API key
    Then the response status should be 401
    And the error message should be "Invalid API key"
    And the response should NOT reveal whether the key exists (prevent enumeration)
    And an audit log entry should be created with:
      | result         | failure                     |
      | failure_reason | invalid_api_key             |
      | api_key_prefix | invalid_ (first 8 chars)    |
    And the validation should use constant-time comparison (timing-attack resistant)

  # ============================================================================
  # SCENARIO 4: CRITICAL - Timing Attack Prevention (CVE-2025-59425)
  # ============================================================================
  Scenario: Use constant-time comparison to prevent timing attacks
    Given I have two API keys with different lengths and values
    And I measure the authentication time for both keys
    When I compare authentication times for 10,000 requests
    Then the comparison time should be CONSTANT regardless of:
      | factor                          | should_not_affect_time |
      | key length                      | true                   |
      | matching prefix characters      | true                   |
      | position of first mismatch      | true                   |
    And the system should use CryptographicOperations.FixedTimeEquals()
    And the system should NEVER use ==, Equals(), or String.Compare()
    And timing analysis should NOT reveal information about correct key

  # ============================================================================
  # SCENARIO 5: Inactive Buyer Account (401 Unauthorized)
  # ============================================================================
  Scenario: Reject request for inactive buyer account
    Given I have a valid API key "157659ac293445df00772760e6114ac4"
    And the API key hash exists in the database
    But the associated buyer account is inactive (IsActive = false)
    When I send a request with the API key
    Then the response status should be 401
    And the error message should be "Invalid API key" (same as invalid key - prevent enumeration)
    And an audit log entry should be created with:
      | result         | failure                     |
      | failure_reason | inactive_buyer              |
      | buyer_id       | <buyer_id>                  |

  # ============================================================================
  # SCENARIO 6: API Key Rotation Warning (90-Day Policy)
  # ============================================================================
  Scenario: Log warning when API key is older than 90 days
    Given I have a valid API key that was created 95 days ago
    And the API key is still active
    When I send a request with the API key
    Then the authentication should succeed (key still valid)
    But a warning should be logged: "API key for buyer {BuyerId} is 95 days old - rotation recommended"
    And the security team should be notified of rotation requirement
    And the buyer should receive rotation notification via email

  # ============================================================================
  # SCENARIO 7: API Key Generation (Cryptographically Secure)
  # ============================================================================
  Scenario: Generate cryptographically secure API key with 256-bit entropy
    When I generate a new API key
    Then the key should be generated using RandomNumberGenerator.Create()
    And the key should have 32 bytes (256 bits) of entropy
    And the key should be Base64 URL-safe encoded (replace +/= characters)
    And the key should NOT use Random() or Guid.NewGuid() (insufficient entropy)
    And the key format should match: [A-Za-z0-9_-]{43}
    And the key should be unpredictable (pass randomness tests)

  # ============================================================================
  # SCENARIO 8: API Key Hashing for Storage (SHA-256)
  # ============================================================================
  Scenario: Store API key as SHA-256 hash in database
    Given I have generated a new API key
    When I store the API key in the database
    Then the plaintext key should be hashed using SHA-256
    And only the hash should be stored in the database
    And the hash should be Base64-encoded for storage
    And the plaintext key should NEVER be stored in the database
    And the plaintext key should be returned to buyer ONCE during generation
    And the buyer must store the key securely (cannot be retrieved later)

  # ============================================================================
  # SCENARIO 9: Automated API Key Rotation (90-Day Cycle)
  # ============================================================================
  Scenario: Automatically rotate API keys older than 90 days
    Given an API key was created 91 days ago
    And the automated rotation Lambda function runs daily
    When the Lambda function executes
    Then the old API key should be invalidated
    And a new API key should be generated (256-bit entropy)
    And the new API key hash should replace the old hash in database
    And the old key hash should be stored in rotation history table
    And the rotation record should include:
      | field             | value                              |
      | buyer_id          | <buyer_id>                         |
      | old_api_key_hash  | <old_hash>                         |
      | new_api_key_hash  | <new_hash>                         |
      | rotated_at        | <current_utc_timestamp>            |
      | rotation_reason   | scheduled_90_day_rotation          |
    And the new API key should be stored in AWS Secrets Manager (encrypted)
    And the buyer should be notified of the key rotation via email
    And the old key should be immediately invalid after rotation

  # ============================================================================
  # SCENARIO 10: Anti-Brute-Force Protection (5 Failed Attempts = Block)
  # ============================================================================
  Scenario: Block IP address after 5 failed authentication attempts
    Given I have made 4 failed authentication attempts from IP 192.168.1.100
    And the failed attempts occurred within 5 minutes
    When I make a 5th failed authentication attempt from the same IP
    Then the IP address should be temporarily blocked for 15 minutes
    And the response status should be 429 (Too Many Requests)
    And the error message should be "Too many failed authentication attempts. Try again in 15 minutes."
    And the Retry-After header should indicate 900 seconds (15 minutes)
    And an audit log entry should be created with:
      | result         | blocked_brute_force         |
      | ip_address     | 192.168.1.100               |
      | block_duration | 15_minutes                  |
    And security monitoring should alert for potential brute force attack

  # ============================================================================
  # SCENARIO 11: Exponential Backoff for Repeated Failures
  # ============================================================================
  Scenario Outline: Increase block duration for repeated brute force attempts
    Given I have been blocked "<previous_blocks>" times for failed authentication
    When I trigger another brute force block
    Then the block duration should be "<new_duration>"
    And the exponential backoff should increase with each offense

    Examples:
      | previous_blocks | new_duration  | description                       |
      | 0               | 15 minutes    | First offense                     |
      | 1               | 30 minutes    | Second offense (2x)               |
      | 2               | 60 minutes    | Third offense (4x)                |
      | 3               | 120 minutes   | Fourth offense (8x)               |
      | 4+              | 240 minutes   | Fifth+ offense (max: 4 hours)     |

  # ============================================================================
  # SCENARIO 12: Audit Logging for All Authentication Attempts
  # ============================================================================
  Scenario Outline: Log all authentication attempts with comprehensive audit trail
    Given I make an authentication attempt with result "<result>"
    When the authentication completes
    Then an audit log entry should be created with:
      | field          | requirement                          |
      | buyer_id       | NULL for failures, ID for success    |
      | ip_address     | Source IP address                    |
      | user_agent     | Client User-Agent header             |
      | api_key_prefix | First 8 characters (NOT full key)    |
      | result         | <result>                             |
      | failure_reason | <failure_reason> (if failure)        |
      | timestamp      | UTC timestamp                        |
    And the audit log should be retained for 24 months (FCRA compliance)
    And the full API key should NEVER be logged

    Examples:
      | result  | failure_reason       | description                    |
      | success | NULL                 | Valid authentication           |
      | failure | missing_api_key      | X-API-Key header missing       |
      | failure | invalid_api_key      | Key hash not found in database |
      | failure | inactive_buyer       | Buyer account is inactive      |

  # ============================================================================
  # SCENARIO 13: OWASP API2:2023 Compliance (Client Authentication Only)
  # ============================================================================
  Scenario: API keys used for client authentication, NOT user authentication
    Given the system implements OWASP API2:2023 best practices
    Then API keys should authenticate client applications (server-to-server)
    And API keys should NOT authenticate individual end users
    And user authentication should use OAuth 2.0 or JWT (separate mechanism)
    And documentation should clarify: "API keys = client auth, NOT user auth"
    And the system should NOT conflate client and user identity

  # ============================================================================
  # SCENARIO 14: Zero Trust Architecture - Verify Every Request
  # ============================================================================
  Scenario: Verify buyer status on every request (zero trust model)
    Given I have a valid API key
    And the buyer account was active when the request started
    But the buyer account becomes inactive during request processing
    When the authentication middleware validates the request
    Then the buyer IsActive status should be checked on EVERY request
    And cached authentication should NOT be used (always verify)
    And the request should be rejected if buyer becomes inactive
    And the system should NOT trust previous authentication results

  # ============================================================================
  # SCENARIO 15: AWS Secrets Manager Integration (Encrypted Key Storage)
  # ============================================================================
  Scenario: Store API keys in AWS Secrets Manager for secure retrieval
    Given I rotate an API key for buyer ID 12345
    When the new API key is generated
    Then the plaintext key should be stored in AWS Secrets Manager
    And the secret name should be "equifax-api-buyer-12345-key"
    And the secret should be encrypted at rest using AWS KMS
    And the secret should have version tracking enabled
    And the secret description should include buyer context
    And the API should NOT store plaintext keys in application database

  # ============================================================================
  # SCENARIO 16: Key Rotation History (Audit Trail)
  # ============================================================================
  Scenario: Maintain complete history of all API key rotations
    Given I rotate an API key
    When the rotation completes
    Then a rotation history record should be created with:
      | field             | requirement                        |
      | buyer_id          | Buyer identifier                   |
      | old_api_key_hash  | Hash of old key                    |
      | new_api_key_hash  | Hash of new key                    |
      | rotated_at        | UTC timestamp of rotation          |
      | rotation_reason   | scheduled_90_day_rotation OR manual|
      | rotated_by        | System OR admin user ID            |
    And the history should be immutable (append-only)
    And the history should be retained indefinitely for audit compliance

  # ============================================================================
  # SCENARIO 17: Index Performance for API Key Lookups
  # ============================================================================
  Scenario: Use B-tree index for fast API key hash lookups
    Given the database contains 10,000 active buyers
    And a B-tree index exists on buyers.api_key_hash column
    When I authenticate a request with an API key
    Then the database lookup should use the api_key_hash index
    And the query should be O(log n) complexity (< 5ms for 10K records)
    And the query should use AsNoTracking() for read-only operations
    And the authentication should complete in less than 10ms total

  # ============================================================================
  # SCENARIO 18: Incident Response - Compromised Key Detection
  # ============================================================================
  Scenario: Detect and respond to compromised API key usage
    Given an API key shows anomalous behavior patterns:
      | indicator                       | threshold             |
      | Geographic location changes     | 2+ continents in 1hr  |
      | Unusual request volume spike    | 10x normal rate       |
      | Requests from blacklisted IPs   | Any request           |
      | Failed permissible purpose      | 5+ in 5 minutes       |
    When the security monitoring system detects anomalies
    Then the system should automatically:
      | action                          | timing                |
      | Alert security team             | Immediate             |
      | Flag buyer account for review   | Immediate             |
      | Optional: Temporarily suspend   | If severity > HIGH    |
      | Force key rotation              | Within 24 hours       |
    And the incident should be logged for forensic analysis

  # ============================================================================
  # SCENARIO 19: IP Allowlisting for High-Security Clients (Optional)
  # ============================================================================
  Scenario: Support IP allowlisting for buyers requiring extra security
    Given buyer ID 12345 has IP allowlisting enabled
    And the allowlist contains IPs: [192.168.1.100, 192.168.1.101]
    And I have a valid API key for buyer 12345
    When I send a request from IP 192.168.1.100
    Then the authentication should succeed (IP in allowlist)
    When I send a request from IP 10.0.0.50
    Then the authentication should fail (IP not in allowlist)
    And the error should indicate "IP address not authorized for this account"
    And an audit log entry should record the blocked IP

  # ============================================================================
  # SCENARIO 20: Rate Limiting for Authentication Endpoints (Stricter Limits)
  # ============================================================================
  Scenario: Enforce stricter rate limits for authentication endpoints
    Given authentication endpoints have rate limit: 100 requests/minute
    And regular API endpoints have rate limit: 1000 requests/minute
    When I make 101 authentication requests in 1 minute
    Then the 101st request should be rate limited (429 status)
    And the authentication endpoint should have 10x stricter limits than regular API
    And this prevents brute force attacks on authentication

  # ============================================================================
  # SCENARIO 21: Key Revocation (Soft Delete with Audit Trail)
  # ============================================================================
  Scenario: Revoke API key while preserving audit trail
    Given I need to revoke API key for buyer 12345 due to security incident
    When I revoke the API key
    Then the buyer.IsActive flag should be set to false (soft delete)
    And the API key hash should remain in the database (audit trail)
    And a revocation record should be created with:
      | field              | value                            |
      | buyer_id           | 12345                            |
      | api_key_hash       | <hash>                           |
      | revoked_at         | <current_utc_timestamp>          |
      | revoked_by         | <admin_user_id>                  |
      | revocation_reason  | security_incident                |
    And all future authentication attempts should fail with 401
    And the audit trail should be preserved for compliance

  # ============================================================================
  # SCENARIO 22: Never Log Full API Keys (Security Best Practice)
  # ============================================================================
  Scenario: Ensure full API keys are never written to logs
    Given I make an authentication attempt with API key "157659ac293445df00772760e6114ac4"
    When the authentication is processed and logged
    Then application logs should NEVER contain the full API key
    And audit logs should only store the first 8 characters: "157659ac"
    And error messages should NEVER include API keys
    And stack traces should NEVER expose API keys
    And this prevents accidental key exposure via log aggregation systems

  # ============================================================================
  # SCENARIO 23: Constant-Time Hash Comparison (Security Critical)
  # ============================================================================
  Scenario: Compare API key hashes in constant time to prevent timing attacks
    Given I have two API key hashes to compare
    When the authentication middleware compares the hashes
    Then the comparison should use CryptographicOperations.FixedTimeEquals()
    And the comparison time should be CONSTANT regardless of:
      | factor                          | should_not_affect_time |
      | hash value differences          | true                   |
      | position of first differing byte| true                   |
      | number of matching bytes        | true                   |
    And byte-by-byte comparison should be avoided (vulnerable to timing attacks)

  # ============================================================================
  # SCENARIO 24: Geographic Restrictions (Optional Enhancement)
  # ============================================================================
  Scenario: Restrict API access by geographic location for compliance
    Given buyer 12345 has geographic restrictions enabled
    And allowed regions are: [US, CA, EU]
    And I have a valid API key for buyer 12345
    When I send a request from IP in Russia (RU)
    Then the authentication should fail
    And the error should indicate "Geographic restriction - API access not allowed from this region"
    And an audit log entry should record the blocked region

  # ============================================================================
  # SCENARIO 25: CloudWatch Monitoring for Authentication Metrics
  # ============================================================================
  Scenario: Track authentication metrics for monitoring and alerting
    Given CloudWatch metrics are configured for authentication
    When authentication events occur
    Then the following metrics should be published:
      | metric_name                     | dimension         |
      | AuthenticationAttempts          | Total count       |
      | AuthenticationSuccesses         | Success rate      |
      | AuthenticationFailures          | Failure rate      |
      | TimingAttackDetected            | Security alerts   |
      | BruteForceBlocks                | Security alerts   |
      | ApiKeyRotations                 | Lifecycle events  |
      | ApiKeyAgeWarnings               | 90-day threshold  |
    And CloudWatch alarms should trigger for:
      | condition                       | threshold         |
      | Failure rate > 10%              | Alert DevOps      |
      | Brute force blocks > 5/hour     | Alert Security    |
      | Key age > 90 days (no rotation) | Alert Compliance  |
