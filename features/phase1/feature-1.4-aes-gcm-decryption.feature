Feature: AES-GCM Decryption Service for Equifax Encrypted Fields
  As a data enrichment system
  I want to decrypt AES-256-GCM encrypted fields from Equifax CSV data
  So that I can store and query consumer data in plain text

  Background:
    Given the Equifax encryption key is configured
    And the key is "4142505A44514850594D493346463758424F59505A45424D4E345931364E5A50"

  @critical @encryption @equifax-import
  Scenario: Successfully decrypt valid encrypted consumer_key
    Given I have encrypted JSON with format {"ciphertext":"hex","iv":"hex","tag":"hex"}
    And the data was encrypted with the configured Equifax key
    When I decrypt the field
    Then I should receive the original plain text value
    And the decryption should complete without errors

  @critical @encryption @equifax-import
  Scenario: Successfully decrypt encrypted name field
    Given I have an encrypted first_name field from Equifax CSV
    And the encrypted value is {"ciphertext":"a1b2c3","iv":"d4e5f6","tag":"789012"}
    When I decrypt the field
    Then I should receive the decrypted name
    And the result should be valid UTF-8 text

  @critical @encryption @equifax-import
  Scenario: Successfully decrypt encrypted address field
    Given I have an encrypted street_name field from Equifax CSV
    And the field contains special characters in the original text
    When I decrypt the field
    Then I should receive the exact original street name
    And all special characters should be preserved

  @error-handling @encryption
  Scenario: Handle null or empty encrypted value gracefully
    Given I have a null encrypted value
    When I attempt to decrypt the field
    Then I should receive null as the result
    And no exception should be thrown

  @error-handling @encryption
  Scenario: Handle invalid JSON format
    Given I have encrypted data with invalid JSON format
    And the data is not in {"ciphertext","iv","tag"} structure
    When I attempt to decrypt the field
    Then I should receive a decryption error
    And the error should indicate invalid format

  @error-handling @encryption
  Scenario: Handle corrupt ciphertext gracefully
    Given I have encrypted JSON with corrupted ciphertext hex
    And the IV and tag are valid
    When I attempt to decrypt the field
    Then I should receive a decryption error
    And the error should indicate authentication failure

  @error-handling @encryption
  Scenario: Handle wrong encryption key
    Given I have encrypted data that was encrypted with a different key
    When I attempt to decrypt the field with my configured key
    Then I should receive a decryption error
    And the error should indicate authentication tag mismatch

  @performance @encryption
  Scenario: Decrypt large batch of fields efficiently
    Given I have 1000 encrypted fields to decrypt
    And each field is a valid encrypted JSON structure
    When I decrypt all fields
    Then all 1000 fields should decrypt successfully
    And the operation should complete in under 1 second

  @integration @csv-import
  Scenario: Decrypt all encrypted fields in a CSV row
    Given I have a CSV row with 103 encrypted fields
    And the fields include consumer_key, names, addresses, and device IDs
    When I decrypt all encrypted fields in the row
    Then all 103 fields should decrypt to plain text
    And I should be able to store them in the database
    And the decrypted consumer_key should match expected format

  @integration @csv-import
  Scenario: Handle mixed encrypted and plain text fields
    Given I have a CSV row with 398 total fields
    And 103 fields are encrypted (26%)
    And 295 fields are plain text (74%)
    When I process the row
    Then only the 103 encrypted fields should be decrypted
    And the 295 plain text fields should pass through unchanged
    And all 398 fields should be ready for database insertion
