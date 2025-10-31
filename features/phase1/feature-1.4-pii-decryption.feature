Feature: PII Decryption Service
  As a phone enrichment service
  I want to securely decrypt PII fields with FCRA compliance
  So that consumer data is protected and only accessed with valid permissible purpose

  Background:
    Given the PII decryption service is configured with AWS Secrets Manager
    And AES-256-GCM authenticated encryption is enabled
    And encryption keys are stored in AWS Secrets Manager with automatic rotation
    And FCRA permissible purpose validation is enabled
    And the system uses 96-bit (12 byte) IVs for optimal security
    And audit logging is enabled for all PII decryption operations

  # ============================================================================
  # SCENARIO 1: Successful PII Field Decryption (Happy Path)
  # ============================================================================
  Scenario Outline: Successfully decrypt PII fields with valid permissible purpose
    Given I have an encrypted PII field "<field_name>" with format "[IV]:[AuthTag]:[Ciphertext]"
    And the encryption key is available in AWS Secrets Manager secret "prod_equifax_encryption_key"
    And the permissible purpose is "<permissible_purpose>"
    And the permissible purpose is valid per FCRA § 604
    When I decrypt the PII field
    Then the decryption should succeed
    And the decrypted value should match the original plaintext
    And the decryption should complete in less than 5ms
    And an audit log entry should be created with:
      | field               | value                       |
      | permissible_purpose | <permissible_purpose>       |
      | field_name          | <field_name>                |
      | timestamp           | <current_utc_timestamp>     |
      | result              | success                     |
    And sensitive decrypted data should be cleared from memory after use

    Examples:
      | field_name       | permissible_purpose       | description                      |
      | first_name       | insurance_underwriting    | Basic PII field                  |
      | last_name        | credit_transaction        | Basic PII field                  |
      | middle_name      | employment_purposes       | Optional PII field               |
      | date_of_birth    | legitimate_business_need  | Sensitive PII field              |
      | email            | account_review            | Contact PII field                |
      | address_1        | collection_of_debt        | Location PII field               |
      | address_2        | insurance_underwriting    | Secondary address field          |
      | ssn_hash         | credit_transaction        | Highest sensitivity PII          |

  # ============================================================================
  # SCENARIO 2: FCRA Permissible Purpose Validation (CRITICAL)
  # ============================================================================
  Scenario Outline: Reject PII decryption with invalid permissible purpose
    Given I have an encrypted PII field "first_name"
    And the permissible purpose is "<invalid_purpose>"
    When I attempt to decrypt the PII field
    Then the decryption should fail
    And the error should indicate "Invalid permissible purpose for PII decryption"
    And the error should reference FCRA § 604 requirements
    And no decryption should occur
    And an audit log entry should be created with result "rejected_invalid_purpose"
    And the log should include the attempted invalid purpose

    Examples:
      | invalid_purpose         | description                               |
      | marketing               | Not a valid FCRA purpose                  |
      | research                | Not a valid FCRA purpose                  |
      | internal_analytics      | Not a valid FCRA purpose                  |
      | ""                      | Empty string (missing purpose)            |
      | null                    | Null value (missing purpose)              |
      | unauthorized_access     | Explicitly unauthorized                   |

  # ============================================================================
  # SCENARIO 3: Valid FCRA Permissible Purposes (Comprehensive List)
  # ============================================================================
  Scenario Outline: Accept valid FCRA § 604 permissible purposes
    Given I have an encrypted PII field "first_name"
    And the permissible purpose is "<valid_purpose>"
    When I validate the permissible purpose
    Then the validation should succeed
    And the purpose should be recognized as valid per FCRA § 604

    Examples:
      | valid_purpose                  | fcra_reference                        |
      | credit_transaction             | § 604(a)(3)(A) - Credit transaction   |
      | insurance_underwriting         | § 604(a)(3)(C) - Insurance            |
      | employment_purposes            | § 604(a)(3)(B) - Employment           |
      | legitimate_business_need       | § 604(a)(3)(F)(i) - Business need     |
      | account_review                 | § 604(a)(3)(A) - Account management   |
      | collection_of_debt             | § 604(a)(3)(A) - Debt collection      |

  # ============================================================================
  # SCENARIO 4: Authentication Tag Validation (Tampering Detection)
  # ============================================================================
  Scenario: Reject decryption when authentication tag validation fails
    Given I have encrypted data with format "[IV]:[AuthTag]:[Ciphertext]"
    And the authentication tag has been modified or corrupted
    When I attempt to decrypt the PII field
    Then the decryption should fail with CryptographicException
    And the error should indicate "authentication tag mismatch"
    And the error should suggest "Possible data tampering detected"
    And an audit log entry should be created with:
      | result        | authentication_failure      |
      | severity      | high                        |
      | alert_type    | potential_tampering         |
    And security monitoring should be alerted of tampering attempt
    And the decryption should complete in less than 5ms (fail-fast)

  # ============================================================================
  # SCENARIO 5: Invalid Encrypted Data Format Handling
  # ============================================================================
  Scenario Outline: Reject decryption with invalid encrypted data format
    Given I have encrypted data in format "<invalid_format>"
    When I attempt to decrypt the PII field
    Then the decryption should fail
    And the error should indicate "Invalid encrypted data format"
    And the error should state "Expected [IV]:[AuthTag]:[Ciphertext]"
    And no decryption attempt should be made
    And the validation should complete in less than 1ms

    Examples:
      | invalid_format                  | description                           |
      | OnlyOnePartNoColons             | Missing separator colons              |
      | Part1:Part2                     | Only 2 parts (missing AuthTag)        |
      | Part1:Part2:Part3:Part4         | Too many parts (4 instead of 3)       |
      | :AuthTag:Ciphertext             | Missing IV (empty first part)         |
      | IV::Ciphertext                  | Missing AuthTag (empty middle part)   |
      | IV:AuthTag:                     | Missing Ciphertext (empty last part)  |
      | ""                              | Empty string                          |

  # ============================================================================
  # SCENARIO 6: Empty and Null Input Handling
  # ============================================================================
  Scenario Outline: Handle empty or null encrypted data gracefully
    Given I have encrypted PII field data that is "<input_value>"
    When I attempt to decrypt the PII field
    Then the decryption should return null (no error)
    And no decryption operation should be performed
    And no audit log should be created (no PII access occurred)

    Examples:
      | input_value    | description                |
      | null           | Null reference             |
      | ""             | Empty string               |
      | "   "          | Whitespace only            |

  # ============================================================================
  # SCENARIO 7: AWS Secrets Manager Key Retrieval
  # ============================================================================
  Scenario: Successfully retrieve encryption key from AWS Secrets Manager
    Given AWS Secrets Manager contains secret "prod_equifax_encryption_key"
    And the secret contains a valid Base64-encoded 256-bit encryption key
    And the secret version stage is "AWSCURRENT"
    When I retrieve the encryption key for the first time
    Then the key should be retrieved from AWS Secrets Manager
    And the key should be cached in memory for 30 minutes
    And the key loaded timestamp should be recorded
    And an audit log entry should record key retrieval with SecretId and VersionId
    And the key retrieval should complete in less than 100ms

  # ============================================================================
  # SCENARIO 8: Encryption Key Caching (Performance Optimization)
  # ============================================================================
  Scenario: Use cached encryption key for subsequent decryptions
    Given the encryption key was retrieved 10 minutes ago
    And the key is cached in memory with 30-minute TTL
    When I decrypt a PII field
    Then the cached key should be used
    And AWS Secrets Manager should NOT be queried
    And the decryption should complete in less than 5ms
    And no additional network latency should occur

  Scenario: Refresh encryption key after cache expiration
    Given the encryption key was cached 31 minutes ago
    And the cache TTL is 30 minutes
    When I decrypt a PII field
    Then the cached key should be considered expired
    And a new key should be retrieved from AWS Secrets Manager
    And the new key should be cached for 30 minutes
    And the decryption should complete in less than 100ms (includes network time)

  # ============================================================================
  # SCENARIO 9: Key Rotation Support (Zero-Downtime)
  # ============================================================================
  Scenario: Support automatic key rotation without downtime
    Given AWS Secrets Manager has rotated the encryption key
    And the new key version stage is "AWSCURRENT"
    And the old key version stage is "AWSPREVIOUS"
    And some encrypted data uses the old key
    And some encrypted data uses the new key
    When I decrypt data encrypted with the old key
    Then the decryption should succeed using "AWSPREVIOUS" key
    When I decrypt data encrypted with the new key
    Then the decryption should succeed using "AWSCURRENT" key
    And no decryption errors should occur during rotation period
    And old keys should remain available for 7 days after rotation

  # ============================================================================
  # SCENARIO 10: IV (Nonce) Size Validation
  # ============================================================================
  Scenario: Validate recommended IV size for AES-GCM
    Given I have encrypted data with a 96-bit (12 byte) IV
    When I decrypt the PII field
    Then the decryption should succeed
    And no warnings should be logged (recommended size)

  Scenario: Log warning for non-standard IV sizes
    Given I have encrypted data with a "<iv_size>-bit" IV
    When I decrypt the PII field
    Then the decryption should still be attempted
    And a warning should be logged: "Non-standard IV size detected: <iv_bytes> bytes (recommended: 12 bytes)"
    And security monitoring should track non-standard IV usage

    Examples:
      | iv_size | iv_bytes | description                        |
      | 64      | 8        | Below recommended size             |
      | 128     | 16       | Above recommended size             |
      | 256     | 32       | Significantly above recommended    |

  # ============================================================================
  # SCENARIO 11: Performance Optimization with Span<T> and ArrayPool
  # ============================================================================
  Scenario: Use stackalloc for small buffers (< 1024 bytes) - zero heap allocation
    Given I have encrypted data with total size less than 1024 bytes
    When I decrypt the PII field
    Then the system should use stackalloc for buffer allocation
    And no heap allocations should occur for buffers
    And the decryption should complete in less than 2ms
    And garbage collection pressure should be zero

  Scenario: Use ArrayPool for large buffers (>= 1024 bytes) - memory pooling
    Given I have encrypted data with ciphertext size of 5000 bytes
    When I decrypt the PII field
    Then the system should rent buffers from ArrayPool<byte>.Shared
    And buffers should be returned to the pool after use
    And buffers containing sensitive data should be returned with clearArray: true
    And the decryption should complete in less than 10ms
    And memory pooling should reduce GC pressure

  # ============================================================================
  # SCENARIO 12: Batch Decryption Performance (Parallel Processing)
  # ============================================================================
  Scenario: Batch decrypt multiple PII fields in parallel
    Given I have 10 encrypted PII fields to decrypt
    And all fields belong to the same record
    And the permissible purpose is "insurance_underwriting"
    When I decrypt all fields using batch decryption
    Then all 10 fields should be decrypted using parallel processing (Task.WhenAll)
    And the total time should be less than 20ms
    And the average time per field should be less than 5ms
    And all decryptions should complete successfully
    And a single audit log entry should be created for the batch operation

  # ============================================================================
  # SCENARIO 13: Selective Decryption (Data Minimization - GDPR Article 25)
  # ============================================================================
  Scenario: Decrypt only requested PII fields (FCRA/GDPR data minimization)
    Given a record contains encrypted fields: [first_name, last_name, ssn_hash, dob, email, address_1, address_2]
    And the API request specifies fields: ["first_name", "last_name", "address_1"]
    And the permissible purpose is "insurance_underwriting"
    When I perform selective decryption
    Then ONLY first_name, last_name, and address_1 should be decrypted
    And ssn_hash, dob, email, and address_2 should remain encrypted (not accessed)
    And the audit log should list only the 3 decrypted fields
    And unnecessary decryption operations should be avoided (performance + compliance)

  # ============================================================================
  # SCENARIO 14: NEVER Cache Decrypted PII (Security Requirement)
  # ============================================================================
  Scenario: Ensure decrypted PII is never cached in Redis
    Given I have decrypted a PII field "ssn_hash"
    When the decryption completes
    Then the decrypted value should NEVER be stored in Redis cache
    And only encrypted data should be cached
    And the decrypted value should exist only in memory for request duration
    And memory buffers should be cleared after use (clearArray: true)

  # ============================================================================
  # SCENARIO 15: Memory Security - Clear Sensitive Data After Use
  # ============================================================================
  Scenario: Securely dispose of decrypted PII from memory
    Given I have decrypted a PII field "ssn_hash"
    And the decrypted value was stored in an ArrayPool buffer
    When the decryption operation completes
    Then the buffer should be returned to ArrayPool with clearArray: true
    And all bytes containing sensitive data should be zeroed out
    And the decrypted string should not persist in memory after request

  # ============================================================================
  # SCENARIO 16: AWS Secrets Manager Failure Handling
  # ============================================================================
  Scenario: Handle AWS Secrets Manager unavailability gracefully
    Given AWS Secrets Manager is unavailable (network error, timeout, or service outage)
    When I attempt to retrieve the encryption key
    Then the key retrieval should fail with descriptive error
    And the error should be logged with severity: critical
    And the decryption request should fail with 503 Service Unavailable
    And the error should NOT expose internal AWS details to API consumers
    And retry logic should be attempted with exponential backoff

  Scenario: Handle missing encryption key in AWS Secrets Manager
    Given AWS Secrets Manager secret "prod_equifax_encryption_key" does not exist
    When I attempt to retrieve the encryption key
    Then the retrieval should fail with ResourceNotFoundException
    And the error should be logged with severity: critical
    And the system should NOT fall back to hardcoded keys (security violation)
    And deployment validation should catch this before production

  # ============================================================================
  # SCENARIO 17: CRITICAL SECURITY: Nonce (IV) Reuse Detection
  # ============================================================================
  Scenario: Prevent catastrophic nonce reuse with same key
    Given the system has encrypted 2^32 records with the current key
    And the encryption operation counter reaches the AES-GCM safety limit
    When I attempt to encrypt another record
    Then the system should BLOCK the encryption operation
    And the error should indicate "Key rotation required - usage limit reached"
    And the system should enforce automatic key rotation
    And security monitoring should alert: "AES-GCM nonce exhaustion imminent"
    And the usage counter should be reset after key rotation

  # ============================================================================
  # SCENARIO 18: Encryption Format Specification Compliance
  # ============================================================================
  Scenario: Validate encrypted data format specification
    Given I have encrypted PII data in the system
    Then the format should be: [IV]:[AuthTag]:[Ciphertext]
    And all components should be Base64-encoded
    And IV size should be 96-bit (12 bytes)
    And AuthTag size should be 128-bit (16 bytes)
    And Key size should be 256-bit (32 bytes) for AES-256-GCM
    And the encrypted data should be stored as VARCHAR/TEXT in database
    And encryption keys should NEVER be hardcoded (AWS Secrets Manager only)

  # ============================================================================
  # SCENARIO 19: Platform Performance - Native Crypto Acceleration
  # ============================================================================
  Scenario: Leverage platform-specific crypto implementations
    Given the system is running on Windows, Linux, or macOS
    When I decrypt PII using AesGcm class
    Then the system should use native platform implementations:
      | platform      | crypto_implementation              |
      | Windows       | Windows CNG (Cryptography Next Gen)|
      | Linux         | OpenSSL                            |
      | macOS         | OpenSSL                            |
    And platform intrinsics should achieve ~8 Gbps/core throughput
    And the implementation should be available since .NET Core 3+

  # ============================================================================
  # SCENARIO 20: FCRA Audit Logging Requirements
  # ============================================================================
  Scenario: Log all PII decryption events with FCRA-compliant audit trail
    Given I decrypt a PII field "ssn_hash"
    And the permissible purpose is "credit_transaction"
    When the decryption completes
    Then an audit log entry should be created with:
      | field                 | requirement                          |
      | permissible_purpose   | FCRA § 604 purpose code              |
      | field_name            | Which PII field was accessed         |
      | buyer_id              | Who accessed the data                |
      | timestamp             | UTC timestamp of access              |
      | ip_address            | Source IP address                    |
      | user_agent            | API client identifier                |
      | result                | success or failure                   |
      | field_length          | Length of decrypted value (not value)|
    And the audit log should be retained for 24 months (FCRA § 607)
    And the audit log should NOT contain the actual decrypted PII value

  # ============================================================================
  # SCENARIO 21: SSN Hash - Highest Sensitivity PII
  # ============================================================================
  Scenario: Require highest permissible purpose for SSN hash decryption
    Given I have an encrypted "ssn_hash" field
    And the permissible purpose is "<purpose>"
    When I attempt to decrypt the SSN hash
    Then the decryption should "<result>"

    Examples:
      | purpose                      | result  | reason                                    |
      | credit_transaction           | succeed | Valid high-security purpose               |
      | employment_purposes          | fail    | Insufficient purpose for SSN access       |
      | insurance_underwriting       | succeed | Valid high-security purpose               |
      | legitimate_business_need     | fail    | Too broad for SSN access                  |
      | account_review               | fail    | Insufficient purpose for SSN access       |
      | collection_of_debt           | succeed | Valid high-security purpose               |

  # ============================================================================
  # SCENARIO 22: Error Handling - Decryption Failures
  # ============================================================================
  Scenario Outline: Handle various decryption failure scenarios
    Given I have encrypted PII data
    And the failure scenario is "<failure_type>"
    When I attempt to decrypt the PII field
    Then the decryption should fail
    And the error type should be "<error_type>"
    And the error should be logged with severity "<severity>"
    And the API should return appropriate HTTP status code

    Examples:
      | failure_type                  | error_type                | severity  |
      | invalid_base64_encoding       | FormatException           | warning   |
      | corrupted_ciphertext          | CryptographicException    | warning   |
      | authentication_tag_mismatch   | SecurityException         | high      |
      | missing_permissible_purpose   | UnauthorizedException     | warning   |
      | aws_secrets_unavailable       | ServiceUnavailableError   | critical  |
      | key_rotation_in_progress      | TransientException        | info      |

  # ============================================================================
  # SCENARIO 23: Key Rotation Lambda Function Integration
  # ============================================================================
  Scenario: AWS Lambda automatically rotates encryption key every 30 days
    Given AWS Secrets Manager rotation is configured for 30-day intervals
    And the Lambda function RotateEncryptionKey is configured
    When the 30-day rotation period expires
    Then AWS Secrets Manager should invoke the Lambda function
    And the Lambda should generate a new 256-bit encryption key
    And the new key should be stored with version stage "AWSPENDING"
    And the Lambda should test the new key with sample decryption
    And the Lambda should promote the new key to "AWSCURRENT"
    And the old key should be demoted to "AWSPREVIOUS"
    And the old key should remain available for 7 days
    And all new encryptions should use the new key
    And existing encrypted data should remain decryptable with old key

  # ============================================================================
  # SCENARIO 24: CloudTrail Auditing for Compliance
  # ============================================================================
  Scenario: Log all AWS Secrets Manager key retrievals in CloudTrail
    Given CloudTrail logging is enabled for AWS Secrets Manager
    When I retrieve the encryption key from AWS Secrets Manager
    Then a CloudTrail event should be created with:
      | field           | value                                     |
      | eventName       | GetSecretValue                            |
      | eventSource     | secretsmanager.amazonaws.com              |
      | userIdentity    | Application IAM role ARN                  |
      | requestParameters | SecretId, VersionStage                  |
      | responseElements  | VersionId (not the actual key)          |
      | eventTime       | UTC timestamp                             |
    And the CloudTrail log should be immutable (append-only)
    And the log should be retained for compliance audits

  # ============================================================================
  # SCENARIO 25: Performance Regression Detection
  # ============================================================================
  Scenario: Detect performance degradation in decryption operations
    Given baseline decryption performance is less than 5ms per field
    And I decrypt 1000 PII fields in a batch
    When I measure the performance metrics
    Then the p50 decryption time should be less than 2ms
    And the p95 decryption time should be less than 5ms
    And the p99 decryption time should be less than 10ms
    And the average decryption time should be less than 3ms
    And any performance degradation over 20% should trigger an alert
    And performance metrics should be tracked in monitoring dashboard
