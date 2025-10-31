Feature: Database Query with Multi-Phone Search
  As a phone enrichment repository
  I want to search across 10 phone number columns efficiently
  So that I can find consumer records regardless of which phone column contains the number

  Background:
    Given the database contains 326M+ Equifax records
    And each record has 10 phone number columns (phone_1 through phone_10)
    And phone columns are ordered by reliability (phone_1 = most reliable, phone_10 = least reliable)
    And single-column B-tree indexes exist on all 10 phone columns
    And PostgreSQL query planner uses bitmap heap scan for OR queries
    And Entity Framework Core 9 compiled queries are enabled

  # ============================================================================
  # SCENARIO 1: Find Record by Phone in First Column (Highest Confidence)
  # ============================================================================
  Scenario: Phone number found in phone_1 column returns highest confidence match
    Given a record exists with phone number "5551234567" in the "phone_1" column
    When I search for phone number "5551234567"
    Then the record should be found
    And the matched column should be 1
    And the match confidence should be 1.00 (100%)
    And the query should complete in less than 150ms (uncached)
    And the query should use bitmap index scan on idx_phone_1
    And the query should use AsNoTracking for read-only operation
    And a log entry should indicate "Phone match found: phone_1, Confidence: 100%"

  # ============================================================================
  # SCENARIO 2: Find Record by Phone in Middle Column (Medium Confidence)
  # ============================================================================
  Scenario Outline: Phone number found in middle columns returns appropriate confidence
    Given a record exists with phone number "5551234567" in the "<column>" column
    And the same phone does NOT exist in columns phone_1 through phone_<previous_column>
    When I search for phone number "5551234567"
    Then the record should be found
    And the matched column should be <column_index>
    And the match confidence should be <confidence>
    And the query should complete in less than 150ms (uncached)

    Examples:
      | column   | column_index | previous_column | confidence |
      | phone_2  | 2            | 1               | 0.95       |
      | phone_3  | 3            | 2               | 0.90       |
      | phone_4  | 4            | 3               | 0.85       |
      | phone_5  | 5            | 4               | 0.80       |
      | phone_6  | 6            | 5               | 0.75       |
      | phone_7  | 7            | 6               | 0.70       |
      | phone_8  | 8            | 7               | 0.65       |
      | phone_9  | 9            | 8               | 0.60       |
      | phone_10 | 10           | 9               | 0.55       |

  # ============================================================================
  # SCENARIO 3: Phone Number Not Found Returns Null
  # ============================================================================
  Scenario: Phone number that does not exist in any column returns null
    Given no record exists with phone number "9999999999" in any phone column
    When I search for phone number "9999999999"
    Then the result should be null
    And the matched column should be null
    And the match confidence should be 0.0
    And the query should scan all 10 phone column indexes using bitmap OR
    And the query should complete in less than 150ms
    And a log entry should indicate "Phone match not found: 9999999999"

  # ============================================================================
  # SCENARIO 4: Duplicate Phone Across Multiple Columns Returns First Match
  # ============================================================================
  Scenario: When phone exists in multiple columns, return first match (highest confidence)
    Given a record exists with phone number "5551234567" in columns:
      | column    |
      | phone_3   |
      | phone_7   |
      | phone_10  |
    When I search for phone number "5551234567"
    Then the record should be found
    And the matched column should be 3 (first occurrence)
    And the match confidence should be 0.90 (90%)
    And the query should stop after finding first match
    And columns phone_4 through phone_10 should NOT be checked

  # ============================================================================
  # SCENARIO 5: Compiled Query Performance Optimization
  # ============================================================================
  Scenario: Compiled queries cache execution plans for repeated searches
    Given a compiled query exists for multi-phone search
    When I search for phone number "5551234567" for the first time
    Then the EF Core execution plan should be compiled and cached
    And the query should complete in less than 150ms
    When I search for a different phone number "5559876543" immediately after
    Then the cached execution plan should be reused
    And the query compilation overhead should be eliminated
    And the query should complete in less than 100ms

  # ============================================================================
  # SCENARIO 6: AsNoTracking Eliminates Change Tracking Overhead
  # ============================================================================
  Scenario: Read-only queries use AsNoTracking for performance
    Given a record exists with phone number "5551234567"
    When I search for phone number "5551234567"
    Then the query should include AsNoTracking()
    And the Entity Framework change tracker should NOT track the entity
    And memory overhead should be reduced by eliminating tracking
    And the query should be faster than tracked queries

  # ============================================================================
  # SCENARIO 7: PostgreSQL Bitmap Heap Scan Optimization
  # ============================================================================
  Scenario: Query uses PostgreSQL bitmap heap scan to combine multiple indexes
    Given indexes exist on all 10 phone columns (idx_phone_1 through idx_phone_10)
    When I search for phone number "5551234567"
    Then PostgreSQL should generate a query plan using:
      """
      Bitmap Heap Scan on equifax_staging_all_text
        Recheck Cond: ((phone_1 = '5551234567') OR (phone_2 = '5551234567') ... OR (phone_10 = '5551234567'))
        -> BitmapOr
           -> Bitmap Index Scan on idx_phone_1
           -> Bitmap Index Scan on idx_phone_2
           ...
           -> Bitmap Index Scan on idx_phone_10
      """
    And all 10 indexes should be utilized via bitmap OR
    And the query should complete in 50-150ms range

  # ============================================================================
  # SCENARIO 8: Match Column Determination Logic
  # ============================================================================
  Scenario Outline: System accurately determines which column matched the phone number
    Given a record has the following phone numbers:
      | column   | phone_number |
      | phone_1  | 2015551111   |
      | phone_2  | 2015552222   |
      | phone_3  | 2015553333   |
      | phone_4  | 2015554444   |
      | phone_5  | 2015555555   |
      | phone_6  | 2015556666   |
      | phone_7  | 2015557777   |
      | phone_8  | 2015558888   |
      | phone_9  | 2015559999   |
      | phone_10 | 2015550000   |
    When I search for phone number "<search_phone>"
    Then the matched column should be <expected_column>
    And the match confidence should be <confidence>

    Examples:
      | search_phone | expected_column | confidence |
      | 2015551111   | 1               | 1.00       |
      | 2015552222   | 2               | 0.95       |
      | 2015553333   | 3               | 0.90       |
      | 2015554444   | 4               | 0.85       |
      | 2015555555   | 5               | 0.80       |
      | 2015556666   | 6               | 0.75       |
      | 2015557777   | 7               | 0.70       |
      | 2015558888   | 8               | 0.65       |
      | 2015559999   | 9               | 0.60       |
      | 2015550000   | 10              | 0.55       |

  # ============================================================================
  # SCENARIO 9: Performance Target - Uncached Query
  # ============================================================================
  Scenario: Uncached database query meets 150ms performance target
    Given Redis cache is empty (no cached result exists)
    And a record exists with phone number "5551234567"
    When I search for phone number "5551234567"
    Then the query should hit the PostgreSQL database
    And the response time should be between 50ms and 150ms
    And the p95 response time over 100 queries should be less than 150ms
    And the p99 response time over 100 queries should be less than 200ms

  # ============================================================================
  # SCENARIO 10: Performance Target - Cached Query
  # ============================================================================
  Scenario: Cached query returns in 1-10ms via Redis
    Given a record exists with phone number "5551234567"
    And the record has been previously queried and cached in Redis
    When I search for phone number "5551234567"
    Then the result should be retrieved from Redis cache
    And the database should NOT be queried
    And the response time should be between 1ms and 10ms
    And the match confidence should be preserved from original query

  # ============================================================================
  # SCENARIO 11: Index Usage Verification
  # ============================================================================
  Scenario: Verify all 10 phone column indexes are created and being used
    Given the database contains 326M records
    When I query the pg_stat_user_indexes system catalog
    Then indexes should exist for:
      | index_name    | column   |
      | idx_phone_1   | phone_1  |
      | idx_phone_2   | phone_2  |
      | idx_phone_3   | phone_3  |
      | idx_phone_4   | phone_4  |
      | idx_phone_5   | phone_5  |
      | idx_phone_6   | phone_6  |
      | idx_phone_7   | phone_7  |
      | idx_phone_8   | phone_8  |
      | idx_phone_9   | phone_9  |
      | idx_phone_10  | phone_10 |
    And each index should have idx_scan > 0 (indicating usage)
    And each index should be a B-tree index
    And each index should be created with CONCURRENTLY to avoid table locks

  # ============================================================================
  # SCENARIO 12: VACUUM Maintenance for Index-Only Scans
  # ============================================================================
  Scenario: Regular VACUUM enables index-only scans for optimal performance
    Given indexes exist on all 10 phone columns
    When VACUUM ANALYZE is run on equifax_staging_all_text table
    Then the visibility map should mark pages as "all-visible"
    And index-only scans should become possible
    And query performance should improve by avoiding heap fetches
    And index bloat should be minimized

  # ============================================================================
  # SCENARIO 13: UNION ALL Alternative Approach (If Needed)
  # ============================================================================
  Scenario: UNION ALL approach searches columns sequentially for selective queries
    Given the OR approach shows bitmap scan overhead for selective queries
    And EXPLAIN ANALYZE recommends sequential column search
    When I search for phone number "5551234567" using UNION ALL approach
    Then the query should search phone_1 first
    And if not found, search phone_2
    And continue sequentially until match is found or all columns exhausted
    And the query should return immediately upon first match
    And the confidence should be calculated based on matched column

  # ============================================================================
  # SCENARIO 14: Confidence Score Calculation Formula
  # ============================================================================
  Scenario Outline: Confidence score follows formula: 100 - ((column_index - 1) * 5)
    Given a phone number matches in column <column_index>
    When the confidence score is calculated
    Then the formula should be: 100 - ((<column_index> - 1) * 5)
    And the confidence should be <confidence_percentage>% or <confidence_decimal> as decimal

    Examples:
      | column_index | confidence_percentage | confidence_decimal | calculation          |
      | 1            | 100                   | 1.00               | 100 - ((1-1)*5) = 100|
      | 2            | 95                    | 0.95               | 100 - ((2-1)*5) = 95 |
      | 3            | 90                    | 0.90               | 100 - ((3-1)*5) = 90 |
      | 4            | 85                    | 0.85               | 100 - ((4-1)*5) = 85 |
      | 5            | 80                    | 0.80               | 100 - ((5-1)*5) = 80 |
      | 6            | 75                    | 0.75               | 100 - ((6-1)*5) = 75 |
      | 7            | 70                    | 0.70               | 100 - ((7-1)*5) = 70 |
      | 8            | 65                    | 0.65               | 100 - ((8-1)*5) = 65 |
      | 9            | 60                    | 0.60               | 100 - ((9-1)*5) = 60 |
      | 10           | 55                    | 0.55               | 100 - ((10-1)*5) = 55|

  # ============================================================================
  # SCENARIO 15: Null/Empty Phone Column Handling
  # ============================================================================
  Scenario: Records with null or empty phone columns are handled correctly
    Given a record exists where:
      | column   | value      |
      | phone_1  | 5551234567 |
      | phone_2  | NULL       |
      | phone_3  | ""         |
      | phone_4  | 5559876543 |
      | phone_5  | NULL       |
    When I search for phone number "5551234567"
    Then the record should be found in phone_1
    And null/empty columns should NOT cause query errors
    And the match confidence should be 1.00 (phone_1 match)

  # ============================================================================
  # SCENARIO 16: Concurrent Query Performance (High Load)
  # ============================================================================
  Scenario: System handles concurrent phone lookups efficiently
    Given 100 concurrent users are querying different phone numbers
    When all 100 queries execute simultaneously
    Then each query should complete in less than 200ms
    And the database connection pool should handle all connections
    And no query should be blocked waiting for connections
    And the average query time should remain under 150ms
    And the p99 query time should be less than 300ms

  # ============================================================================
  # SCENARIO 17: Large Result Set Handling (Memory Efficiency)
  # ============================================================================
  Scenario: FirstOrDefault limits result set to single record
    Given multiple records might match the search criteria
    When I search for phone number "5551234567"
    Then the query should include LIMIT 1
    And only the first matching record should be returned
    And memory usage should be minimal (single record, not full result set)
    And AsNoTracking should prevent unnecessary entity tracking

  # ============================================================================
  # SCENARIO 18: Logging and Monitoring
  # ============================================================================
  Scenario: Database queries are logged for monitoring and debugging
    Given a record exists with phone number "5551234567" in phone_3
    When I search for phone number "5551234567"
    Then a log entry should be created with:
      | log_field       | value                                           |
      | phone_number    | 5551234567                                      |
      | matched_column  | phone_3                                         |
      | confidence      | 90% or 0.90                                     |
      | log_level       | Information                                     |
      | message         | "Phone match found: 5551234567, Column: phone_3, Confidence: 90%"|
      | query_time_ms   | Actual query execution time                     |

  # ============================================================================
  # SCENARIO 19: Query Cancellation Support
  # ============================================================================
  Scenario: Long-running queries respect cancellation tokens
    Given a phone search is initiated with a cancellation token
    And the query is taking longer than expected
    When the cancellation token is triggered after 100ms
    Then the query should be cancelled gracefully
    And the database connection should be returned to the pool
    And an OperationCanceledException should be thrown
    And no partial results should be returned

  # ============================================================================
  # SCENARIO 20: Entity Framework Query Translation
  # ============================================================================
  Scenario: LINQ query translates correctly to PostgreSQL SQL
    Given the LINQ query uses OR conditions across 10 phone columns
    When Entity Framework Core translates the query to SQL
    Then the SQL should include:
      """
      SELECT * FROM equifax_staging_all_text
      WHERE phone_1 = @p0
         OR phone_2 = @p0
         OR phone_3 = @p0
         OR phone_4 = @p0
         OR phone_5 = @p0
         OR phone_6 = @p0
         OR phone_7 = @p0
         OR phone_8 = @p0
         OR phone_9 = @p0
         OR phone_10 = @p0
      LIMIT 1
      """
    And a single parameter @p0 should be used for all comparisons
    And the LIMIT 1 clause should be included

  # ============================================================================
  # SCENARIO 21: Confidence Scoring for Data Enrichment Standards
  # ============================================================================
  Scenario: Confidence scoring follows 2025 data enrichment industry standards
    Given the system implements multi-source waterfall enrichment
    And phone columns are ordered by reliability (Equifax standard)
    When a phone number is matched
    Then the confidence score should reflect:
      | factor                  | description                                   |
      | Column reliability      | phone_1 = most reliable, phone_10 = least    |
      | Data freshness          | Timestamp from Equifax data source            |
      | Multi-provider validation| Cross-validation opportunity (future)         |
      | ML-powered scoring      | Continuous learning (future enhancement)      |
    And the confidence should be returned to the caller
    And the API response metadata should include the confidence score

  # ============================================================================
  # SCENARIO 22: Error Handling - Database Connection Failure
  # ============================================================================
  Scenario: Graceful error handling when database is unavailable
    Given the PostgreSQL database is temporarily unavailable
    When I attempt to search for phone number "5551234567"
    Then a database connection exception should be caught
    And the error should be logged with severity "Error"
    And a ServiceResult with failure status should be returned
    And the error message should NOT expose database internals
    And the user should see "Service temporarily unavailable"

  # ============================================================================
  # SCENARIO 23: Index Selectivity and Query Planning
  # ============================================================================
  Scenario: PostgreSQL query planner chooses optimal index strategy
    Given 326M records exist in the database
    And phone number "5551234567" exists in only 1 record
    When I search for phone number "5551234567"
    Then the query planner should estimate low selectivity (1/326M)
    And the bitmap heap scan should be the chosen strategy
    And the query plan should show index scans on all 10 indexes
    And the execution time should be under 150ms

  # ============================================================================
  # SCENARIO 24: Consistency with Redis Cache
  # ============================================================================
  Scenario: Database results are consistent with cached results
    Given a record exists with phone number "5551234567" in phone_2
    When I search for phone number "5551234567" (first time, uncached)
    Then the database returns: record, confidence 0.95, matched_column 2
    And the result is cached in Redis with TTL 24 hours
    When I search for the same phone number again (cached)
    Then the cached result should exactly match the database result
    And the confidence should still be 0.95
    And the matched column should still be 2

  # ============================================================================
  # SCENARIO 25: Performance Regression Detection
  # ============================================================================
  Scenario: Automated performance testing detects query regressions
    Given baseline query performance is 50-150ms for uncached queries
    When I run 1000 test queries against the database
    Then the average query time should be within 20% of baseline
    And the p95 query time should not exceed 150ms
    And the p99 query time should not exceed 200ms
    And any queries exceeding 300ms should trigger performance alerts
    And slow query logs should be analyzed for optimization opportunities
