Feature: REST API Endpoint for Phone Number Enrichment
  As an API consumer (buyer)
  I want to query phone numbers to retrieve enrichment data
  So that I can enhance my lead data with demographic and financial information

  Background:
    Given the API is deployed and running
    And the database contains 326M+ Equifax records
    And valid API keys exist for authorized buyers
    And FCRA audit logging is enabled

  # ============================================================================
  # SCENARIO 1: Successful Phone Lookup with Basic Fields (Happy Path)
  # ============================================================================
  Scenario: Successfully enrich phone number with basic fields
    Given I have a valid API key "157659ac293445df00772760e6114ac4"
    And the provider code is "EQUIFAX_ENRICHMENT"
    And a valid phone number "8015551234" exists in the database
    And the permissible purpose is "insurance_underwriting"
    When I POST to "/api/data_enhancement/lookup" with:
      """
      {
        "api_key": "157659ac293445df00772760e6114ac4",
        "provider_code": "EQUIFAX_ENRICHMENT",
        "phone": "8015551234",
        "permissible_purpose": "insurance_underwriting",
        "fields": "basic"
      }
      """
    Then the response status should be 200
    And the response body should contain:
      """
      {
        "response": "success",
        "message": "Record found with high confidence",
        "data": {
          "consumer_key": "<any_string>",
          "personal_info": { ... },
          "addresses": [ ... ],
          "phones": [ ... ],
          "financial": { ... }
        },
        "_metadata": {
          "match_confidence": <float_0_to_1>,
          "match_type": "<string>",
          "data_freshness_date": "<date>",
          "query_timestamp": "<timestamp>",
          "response_time_ms": "<integer>",
          "request_id": "<uuid>",
          "total_fields_returned": null
        }
      }
      """
    And the response time should be less than 200ms
    And the data should contain approximately 50 fields
    And an FCRA audit log entry should be created
    And the audit log should include:
      | buyer_id              | <buyer_id_for_api_key>      |
      | phone_number_queried  | 8015551234                  |
      | permissible_purpose   | insurance_underwriting      |
      | timestamp             | <current_utc_timestamp>     |
      | response_status       | success                     |

  # ============================================================================
  # SCENARIO 2: Successful Phone Lookup with Full Fields (398 Columns)
  # ============================================================================
  Scenario: Successfully enrich phone number with full dataset
    Given I have a valid API key "157659ac293445df00772760e6114ac4"
    And a valid phone number "8015551234" exists in the database
    When I POST to "/api/data_enhancement/lookup" with:
      """
      {
        "api_key": "157659ac293445df00772760e6114ac4",
        "provider_code": "EQUIFAX_ENRICHMENT",
        "phone": "8015551234",
        "permissible_purpose": "insurance_underwriting",
        "fields": "full"
      }
      """
    Then the response status should be 200
    And the response should be "success"
    And the data should contain all 398 columns
    And the metadata should show "total_fields_returned": 398
    And the response time should be less than 300ms
    And the response payload size should be larger than basic fields

  # ============================================================================
  # SCENARIO 3: Phone Lookup with Optional Fields (Improved Match Confidence)
  # ============================================================================
  Scenario: Enrich phone number with optional fields for higher match confidence
    Given I have a valid API key "157659ac293445df00772760e6114ac4"
    And a valid phone number "8015551234" exists in the database
    When I POST to "/api/data_enhancement/lookup" with:
      """
      {
        "api_key": "157659ac293445df00772760e6114ac4",
        "provider_code": "EQUIFAX_ENRICHMENT",
        "phone": "8015551234",
        "first_name": "Bob",
        "last_name": "Barker",
        "postal_code": "84010",
        "state": "UT",
        "ip_address": "192.168.1.100",
        "permissible_purpose": "insurance_underwriting",
        "unique_id": "b4c9f530-5461-11ef-8f6f-8ffb313ceb02"
      }
      """
    Then the response status should be 200
    And the match_confidence should be greater than 0.90
    And the metadata should include "unique_id": "b4c9f530-5461-11ef-8f6f-8ffb313ceb02"
    And the match_type should indicate enhanced matching
    And the response time should be less than 200ms

  # ============================================================================
  # SCENARIO 4: No Match Found (Valid Request, Phone Not in Database)
  # ============================================================================
  Scenario: Phone number not found in database returns success response with error message
    Given I have a valid API key "157659ac293445df00772760e6114ac4"
    And the phone number "5559999999" does NOT exist in the database
    When I POST to "/api/data_enhancement/lookup" with:
      """
      {
        "api_key": "157659ac293445df00772760e6114ac4",
        "provider_code": "EQUIFAX_ENRICHMENT",
        "phone": "5559999999",
        "permissible_purpose": "insurance_underwriting"
      }
      """
    Then the response status should be 200
    And the response should be "error"
    And the message should be "Unable to find record for phone number"
    And the data should include:
      """
      {
        "phone": "5559999999",
        "match_attempted": true,
        "match_confidence": 0.0
      }
      """
    And an FCRA audit log entry should still be created
    And the audit log response_status should be "no_match"

  # ============================================================================
  # SCENARIO 5: Invalid API Key (401 Unauthorized)
  # ============================================================================
  Scenario: Request with invalid API key is rejected
    Given I have an invalid API key "invalid_key_12345"
    When I POST to "/api/data_enhancement/lookup" with:
      """
      {
        "api_key": "invalid_key_12345",
        "provider_code": "EQUIFAX_ENRICHMENT",
        "phone": "8015551234",
        "permissible_purpose": "insurance_underwriting"
      }
      """
    Then the response status should be 401
    And the error message should indicate "Invalid API key"
    And no FCRA audit log should be created
    And no database query should be executed

  # ============================================================================
  # SCENARIO 6: Missing Required Fields (400 Bad Request)
  # ============================================================================
  Scenario Outline: Request with missing required fields is rejected
    Given I have a valid API key "157659ac293445df00772760e6114ac4"
    When I POST to "/api/data_enhancement/lookup" with incomplete data missing "<missing_field>"
    Then the response status should be 400
    And the error message should indicate "<missing_field>" is required
    And no database query should be executed

    Examples:
      | missing_field        |
      | api_key              |
      | provider_code        |
      | phone                |
      | permissible_purpose  |

  # ============================================================================
  # SCENARIO 7: Invalid Phone Number Format (400 Bad Request)
  # ============================================================================
  Scenario Outline: Request with invalid phone number format is rejected
    Given I have a valid API key "157659ac293445df00772760e6114ac4"
    When I POST to "/api/data_enhancement/lookup" with:
      """
      {
        "api_key": "157659ac293445df00772760e6114ac4",
        "provider_code": "EQUIFAX_ENRICHMENT",
        "phone": "<invalid_phone>",
        "permissible_purpose": "insurance_underwriting"
      }
      """
    Then the response status should be 400
    And the error message should indicate invalid phone number format

    Examples:
      | invalid_phone       |
      | 123                 |
      | abcdefghij          |
      | +1-800-555-1234     |
      | (555) 123-456       |
      | 555-1234            |

  # ============================================================================
  # SCENARIO 8: Invalid Permissible Purpose (400 Bad Request)
  # ============================================================================
  Scenario: Request with invalid permissible purpose is rejected
    Given I have a valid API key "157659ac293445df00772760e6114ac4"
    When I POST to "/api/data_enhancement/lookup" with:
      """
      {
        "api_key": "157659ac293445df00772760e6114ac4",
        "provider_code": "EQUIFAX_ENRICHMENT",
        "phone": "8015551234",
        "permissible_purpose": "invalid_purpose"
      }
      """
    Then the response status should be 400
    And the error message should indicate invalid permissible purpose
    And the error should list valid FCRA permissible purposes

  # ============================================================================
  # SCENARIO 9: Rate Limit Exceeded (429 Too Many Requests)
  # ============================================================================
  Scenario: Request exceeding rate limit is rejected
    Given I have a valid API key "157659ac293445df00772760e6114ac4"
    And my rate limit is 1000 requests per hour
    And I have already made 1000 requests in the current hour
    When I POST to "/api/data_enhancement/lookup" with:
      """
      {
        "api_key": "157659ac293445df00772760e6114ac4",
        "provider_code": "EQUIFAX_ENRICHMENT",
        "phone": "8015551234",
        "permissible_purpose": "insurance_underwriting"
      }
      """
    Then the response status should be 429
    And the response should include "Retry-After" header
    And the error message should indicate rate limit exceeded
    And no database query should be executed

  # ============================================================================
  # SCENARIO 10: Phone Number Normalization Handling
  # ============================================================================
  Scenario Outline: Different phone number formats are normalized before lookup
    Given I have a valid API key "157659ac293445df00772760e6114ac4"
    And the normalized phone number "8015551234" exists in the database
    When I POST to "/api/data_enhancement/lookup" with:
      """
      {
        "api_key": "157659ac293445df00772760e6114ac4",
        "provider_code": "EQUIFAX_ENRICHMENT",
        "phone": "<input_phone>",
        "permissible_purpose": "insurance_underwriting"
      }
      """
    Then the response status should be 200
    And the response should be "success"
    And the phone number should be normalized to "8015551234" before querying

    Examples:
      | input_phone          |
      | 8015551234           |
      | (801) 555-1234       |
      | 801-555-1234         |
      | 1-801-555-1234       |
      | +1 (801) 555-1234    |

  # ============================================================================
  # SCENARIO 11: Default Fields Selection
  # ============================================================================
  Scenario: Request without fields parameter defaults to basic fields
    Given I have a valid API key "157659ac293445df00772760e6114ac4"
    And a valid phone number "8015551234" exists in the database
    When I POST to "/api/data_enhancement/lookup" without the "fields" parameter
    Then the response status should be 200
    And the response should contain basic fields (~50 columns)
    And the metadata should show "total_fields_returned": null
    And the response time should be less than 200ms

  # ============================================================================
  # SCENARIO 12: Response Metadata Completeness
  # ============================================================================
  Scenario: Response includes complete metadata for tracking and monitoring
    Given I have a valid API key "157659ac293445df00772760e6114ac4"
    And a valid phone number "8015551234" exists in the database
    When I POST to "/api/data_enhancement/lookup" with:
      """
      {
        "api_key": "157659ac293445df00772760e6114ac4",
        "provider_code": "EQUIFAX_ENRICHMENT",
        "phone": "8015551234",
        "permissible_purpose": "insurance_underwriting",
        "unique_id": "test-tracking-id-123"
      }
      """
    Then the response metadata should include:
      | field                 | requirement                    |
      | match_confidence      | float between 0.0 and 1.0      |
      | match_type            | non-empty string               |
      | data_freshness_date   | valid ISO 8601 date            |
      | query_timestamp       | current UTC timestamp          |
      | response_time_ms      | positive integer < 200         |
      | request_id            | non-empty string (UUID format) |
      | unique_id             | equals "test-tracking-id-123"  |

  # ============================================================================
  # SCENARIO 13: Performance SLA Compliance
  # ============================================================================
  Scenario: API response times meet SLA requirements
    Given I have a valid API key "157659ac293445df00772760e6114ac4"
    And a valid phone number exists in the database
    When I make 100 requests to "/api/data_enhancement/lookup" with "basic" fields
    Then the p95 response time should be less than 200ms
    And the p99 response time should be less than 250ms
    And the average response time should be less than 150ms

  Scenario: Full dataset response times meet SLA requirements
    Given I have a valid API key "157659ac293445df00772760e6114ac4"
    And a valid phone number exists in the database
    When I make 100 requests to "/api/data_enhancement/lookup" with "full" fields
    Then the p95 response time should be less than 300ms
    And the p99 response time should be less than 350ms
    And the average response time should be less than 250ms
