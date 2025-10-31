Feature: Phone Number Normalization
  As a phone enrichment service
  I want to normalize phone numbers to a consistent format
  So that lookups work regardless of input format

  Background:
    Given the phone normalizer service is configured
    And the system uses NANP (North American Numbering Plan) rules
    And the target format is 10-digit US format (no country code)

  # ============================================================================
  # SCENARIO 1: Basic US Phone Number Formats (Common Cases)
  # ============================================================================
  Scenario Outline: Normalize common US phone number formats to 10-digit standard
    Given I have a phone number "<input_phone>"
    When I normalize the phone number
    Then the normalized result should be "<expected_output>"
    And the normalization should complete in less than 1ms
    And the result should be exactly 10 digits
    And the result should contain only numeric characters

    Examples:
      | input_phone           | expected_output | description                  |
      | 5551234567            | 5551234567      | Already normalized           |
      | (555) 123-4567        | 5551234567      | Parentheses and hyphens      |
      | 555-123-4567          | 5551234567      | Hyphens only                 |
      | 555.123.4567          | 5551234567      | Dots as separators           |
      | 555 123 4567          | 5551234567      | Spaces as separators         |
      | 1-555-123-4567        | 5551234567      | US country code (1) prefix   |
      | +1 555 123 4567       | 5551234567      | E.164 with country code      |
      | +1 (555) 123-4567     | 5551234567      | E.164 with formatting        |
      | +15551234567          | 5551234567      | E.164 compact format         |
      | 1 (555) 123-4567      | 5551234567      | Country code with parens     |
      | 1.555.123.4567        | 5551234567      | Country code with dots       |

  # ============================================================================
  # SCENARIO 2: NANP N11 Rule Validation (Reserved Codes)
  # ============================================================================
  Scenario Outline: Reject phone numbers with N11 area codes or exchanges (reserved)
    Given I have a phone number "<invalid_phone>"
    When I attempt to normalize the phone number
    Then the normalization should fail
    And the error message should indicate "N11 format reserved for special services"
    And the error should mention "<n11_type>"

    Examples:
      | invalid_phone    | n11_type         | description                          |
      | 211-555-1234     | area code        | N11 area code (211)                  |
      | 311-555-1234     | area code        | N11 area code (311)                  |
      | 411-555-1234     | area code        | Directory assistance area code       |
      | 511-555-1234     | area code        | Traffic/weather area code            |
      | 611-555-1234     | area code        | Repair service area code             |
      | 711-555-1234     | area code        | TDD relay service area code          |
      | 811-555-1234     | area code        | Call before you dig area code        |
      | 911-555-1234     | area code        | Emergency services area code         |
      | 555-211-1234     | exchange         | N11 exchange (211)                   |
      | 555-411-1234     | exchange         | Directory assistance exchange        |
      | 555-911-1234     | exchange         | Emergency services exchange          |
      | 411-411-1234     | both             | N11 in both area code AND exchange   |

  # ============================================================================
  # SCENARIO 3: NANP First Digit Rule (Must Be 2-9)
  # ============================================================================
  Scenario Outline: Reject phone numbers with invalid first digits in area code or exchange
    Given I have a phone number "<invalid_phone>"
    When I attempt to normalize the phone number
    Then the normalization should fail
    And the error message should indicate "First digit must be 2-9"
    And the error should mention "<invalid_part>"

    Examples:
      | invalid_phone    | invalid_part     | description                          |
      | 055-123-4567     | area code        | Area code starts with 0              |
      | 155-123-4567     | area code        | Area code starts with 1              |
      | 555-023-4567     | exchange         | Exchange starts with 0               |
      | 555-123-4567     | none             | Valid (baseline for comparison)      |
      | 255-123-4567     | none             | Valid (area code starts with 2)      |
      | 955-123-4567     | none             | Valid (area code starts with 9)      |
      | 555-223-4567     | none             | Valid (exchange starts with 2)       |
      | 555-923-4567     | none             | Valid (exchange starts with 9)       |

  # ============================================================================
  # SCENARIO 4: Invalid Phone Number Lengths
  # ============================================================================
  Scenario Outline: Reject phone numbers with incorrect lengths
    Given I have a phone number "<invalid_phone>"
    When I attempt to normalize the phone number
    Then the normalization should fail
    And the error message should indicate "Invalid phone number length"
    And the error should state expected length is 10 digits
    And the error should state actual length is "<actual_digits>" digits

    Examples:
      | invalid_phone    | actual_digits    | description                          |
      | 123              | 3                | Only 3 digits                        |
      | 555-1234         | 7                | Only 7 digits (missing area code)    |
      | 555-123-456      | 9                | Only 9 digits (missing last digit)   |
      | 555-123-45678    | 11               | 11 digits (extra digit)              |
      | 12345678901234   | 14               | 14 digits (way too long)             |

  # ============================================================================
  # SCENARIO 5: Non-Numeric Characters Handling
  # ============================================================================
  Scenario Outline: Successfully strip non-numeric characters before validation
    Given I have a phone number "<input_phone>"
    When I normalize the phone number
    Then all non-numeric characters should be removed
    And the result should be "<expected_output>"

    Examples:
      | input_phone                | expected_output | description                        |
      | (555)-123-4567             | 5551234567      | Parentheses and hyphens            |
      | 555.123.4567               | 5551234567      | Dots                               |
      | 555 123 4567               | 5551234567      | Spaces                             |
      | +1 (555) 123-4567          | 5551234567      | Plus, parens, spaces, hyphens      |
      | Phone: 555-123-4567        | Phone:5551234567| Letters NOT stripped (should fail) |

  # ============================================================================
  # SCENARIO 6: Empty and Null Input Handling
  # ============================================================================
  Scenario Outline: Reject empty, null, or whitespace-only phone numbers
    Given I have a phone number "<input_phone>"
    When I attempt to normalize the phone number
    Then the normalization should fail
    And the error message should indicate "Phone number cannot be empty"

    Examples:
      | input_phone    | description                           |
      | ""             | Empty string                          |
      | "   "          | Whitespace only                       |
      | "\t\n"         | Tabs and newlines only                |

  # ============================================================================
  # SCENARIO 7: E.164 Format Handling (International Standard)
  # ============================================================================
  Scenario Outline: Normalize E.164 formatted numbers to 10-digit US format
    Given I have an E.164 formatted phone number "<e164_phone>"
    When I normalize the phone number
    Then the US country code (+1) should be stripped
    And the result should be "<expected_output>"
    And the result should be exactly 10 digits

    Examples:
      | e164_phone       | expected_output | description                        |
      | +15551234567     | 5551234567      | Compact E.164 format               |
      | +1 555 123 4567  | 5551234567      | E.164 with spaces                  |
      | +1-555-123-4567  | 5551234567      | E.164 with hyphens                 |

  # ============================================================================
  # SCENARIO 8: Performance Requirements (Sub-Millisecond)
  # ============================================================================
  Scenario: Regex normalization meets sub-millisecond performance requirement
    Given I have 1000 valid US phone numbers in various formats
    When I normalize all 1000 phone numbers using regex approach
    Then the average normalization time should be less than 1ms per number
    And the total processing time should be less than 1 second
    And the p95 normalization time should be less than 2ms
    And the p99 normalization time should be less than 5ms

  # ============================================================================
  # SCENARIO 9: Hybrid Approach - Fast Path (Regex)
  # ============================================================================
  Scenario: Hybrid normalizer uses fast regex path for standard requests
    Given I have a phone number "555-123-4567"
    And strict validation is set to FALSE
    When I normalize using the hybrid normalizer
    Then the regex normalizer should be used
    And the normalization should complete in less than 1ms
    And the result should be "5551234567"
    And libphonenumber should NOT be invoked

  # ============================================================================
  # SCENARIO 10: Hybrid Approach - Strict Path (libphonenumber)
  # ============================================================================
  Scenario: Hybrid normalizer uses libphonenumber for strict validation
    Given I have a phone number "555-123-4567"
    And strict validation is set to TRUE
    When I normalize using the hybrid normalizer
    Then libphonenumber validation should be invoked
    And the phone number should be validated as valid
    And the number type should be determined (MOBILE, FIXED_LINE, etc.)
    And the result should be "5551234567"
    And validation details should be logged

  # ============================================================================
  # SCENARIO 11: libphonenumber Validation Details
  # ============================================================================
  Scenario Outline: libphonenumber provides detailed validation and number type
    Given I have a phone number "<input_phone>"
    And strict validation is enabled (libphonenumber)
    When I validate and normalize the phone number
    Then the validation result should be "<is_valid>"
    And the number type should be "<number_type>"
    And the normalized number should be "<normalized>" (if valid)

    Examples:
      | input_phone       | is_valid | number_type  | normalized  | description              |
      | 555-123-4567      | true     | FIXED_LINE   | 5551234567  | Valid landline           |
      | 800-555-1234      | true     | TOLL_FREE    | 8005551234  | Toll-free number         |
      | 900-555-1234      | true     | PREMIUM_RATE | 9005551234  | Premium rate number      |
      | 123-456-7890      | false    | UNKNOWN      | null        | Invalid NANP format      |

  # ============================================================================
  # SCENARIO 12: Special Cases and Edge Cases
  # ============================================================================
  Scenario Outline: Handle special cases and edge cases correctly
    Given I have a phone number "<input_phone>"
    When I attempt to normalize the phone number
    Then the result should be "<expected_outcome>"
    And the normalized value should be "<normalized>" (if applicable)

    Examples:
      | input_phone              | expected_outcome | normalized | description                   |
      | 5551234567               | success          | 5551234567 | Already normalized            |
      | 15551234567              | success          | 5551234567 | With US country code          |
      | 115551234567             | error            | null       | Two leading 1s (invalid)      |
      | 0055512345678            | error            | null       | International format (not US) |
      | 555-1234                 | error            | null       | Too short (7 digits)          |
      | 5551234567890            | error            | null       | Too long (13 digits)          |

  # ============================================================================
  # SCENARIO 13: Normalization Idempotency
  # ============================================================================
  Scenario: Normalizing an already normalized number returns same result
    Given I have a normalized phone number "5551234567"
    When I normalize the phone number
    Then the result should be "5551234567"
    And the normalization should be idempotent
    And normalizing the result again should produce the same output

  # ============================================================================
  # SCENARIO 14: Case Sensitivity (Should Not Exist)
  # ============================================================================
  Scenario: Phone numbers should not contain letters (case insensitive rejection)
    Given I have a phone number "555-CALL-NOW"
    When I attempt to normalize the phone number
    Then the normalization should fail
    And the error should indicate invalid characters
    And letters should not be converted to numbers (old phone pad style)

  # ============================================================================
  # SCENARIO 15: Caching Normalized Results (Performance Optimization)
  # ============================================================================
  Scenario: Normalized phone numbers are cached in Redis for 24 hours
    Given I have a phone number "(555) 123-4567"
    When I normalize the phone number for the first time
    Then the normalized result "5551234567" should be cached in Redis
    And the cache TTL should be 24 hours
    And the cache key should include the input format for consistency
    When I normalize the same phone number again within 24 hours
    Then the result should be retrieved from cache
    And the database should NOT be queried
    And the response time should be less than 5ms

  # ============================================================================
  # SCENARIO 16: Database Schema Compatibility
  # ============================================================================
  Scenario: Normalized numbers match existing database schema format
    Given the database contains phone numbers in 10-digit format
    And the database has 10 phone columns (phone_1 through phone_10)
    When I normalize a phone number "555-123-4567"
    Then the result should be "5551234567" (10 digits, no formatting)
    And the format should match the existing 326M records
    And the format should be compatible with all 10 phone columns
    And the query should use the normalized format for all phone column lookups

  # ============================================================================
  # SCENARIO 17: Error Messages Clarity
  # ============================================================================
  Scenario Outline: Error messages provide clear guidance for invalid inputs
    Given I have an invalid phone number "<invalid_phone>"
    When I attempt to normalize the phone number
    Then the error message should be clear and actionable
    And the error should include the specific validation rule violated
    And the error should NOT expose internal implementation details

    Examples:
      | invalid_phone    | expected_error_fragment                           |
      | 411-555-1234     | N11 format reserved for special services          |
      | 155-123-4567     | First digit must be 2-9                           |
      | 555-123-456      | Invalid phone number length: 9 digits (expected 10)|
      | ""               | Phone number cannot be empty                      |
      | abcdefghij       | Invalid phone number length                       |

  # ============================================================================
  # SCENARIO 18: Logging and Monitoring
  # ============================================================================
  Scenario: Normalization failures are logged for monitoring and debugging
    Given I have an invalid phone number "411-555-1234"
    When I attempt to normalize the phone number
    Then the normalization failure should be logged
    And the log should include:
      | log_field        | description                                    |
      | input_phone      | Original input value                           |
      | error_type       | Type of validation error (N11, length, etc.)   |
      | timestamp        | UTC timestamp of the failure                   |
      | severity         | Warning (not error - expected validation)      |
    And the log should NOT include sensitive data

  # ============================================================================
  # SCENARIO 19: Future International Expansion Readiness
  # ============================================================================
  Scenario: System is ready for international expansion via feature flag
    Given the system has a feature flag "EnableInternationalNumbers"
    And the feature flag is set to FALSE (US-only for MVP)
    When I normalize a phone number "555-123-4567"
    Then the regex normalizer should be used
    And the libphonenumber validator should NOT be used
    When the feature flag is set to TRUE (international enabled)
    And I normalize a UK phone number "+44 20 7946 0958"
    Then libphonenumber validation should be used
    And the number should be normalized to E.164 format
    And the database schema should support VARCHAR(32) for international numbers

  # ============================================================================
  # SCENARIO 20: Bulk Normalization Performance
  # ============================================================================
  Scenario: Bulk normalization of phone numbers maintains performance
    Given I have 10,000 phone numbers in various formats
    When I normalize all 10,000 numbers in a batch operation
    Then the total processing time should be less than 10 seconds
    And the average time per number should be less than 1ms
    And all valid numbers should be normalized correctly
    And all invalid numbers should be identified with clear error messages
    And a summary report should include:
      | metric                 | requirement           |
      | total_processed        | 10,000                |
      | successful_normalizations | > 9,500 (95%+)     |
      | failed_normalizations  | Count with error types|
      | average_time_per_number| < 1ms                 |
