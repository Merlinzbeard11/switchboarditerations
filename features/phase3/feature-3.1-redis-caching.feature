Feature: Redis Caching (Distributed, High-Performance)
  As a high-performance API system
  I want to cache database query results in Redis
  So that response times are reduced from 50-150ms to sub-10ms

  Background:
    Given AWS ElastiCache Redis cluster is configured
    And IDistributedCache interface is used for provider-agnostic caching
    And ConnectionMultiplexer is registered as singleton
    And AbortOnConnectFail is set to false (graceful degradation)
    And 24-hour TTL with jitter is configured (prevents thundering herd)
    And cache monitoring with hit rate tracking is enabled
    And performance target is < 10ms cache hit response time

  # ============================================================================
  # SCENARIO 1: Cache Hit (Sub-10ms Response Time)
  # ============================================================================
  Scenario: Return cached record in less than 10ms (cache hit)
    Given phone number "8015551234" is cached in Redis
    When I query for phone number "8015551234"
    Then the record should be retrieved from Redis cache
    And the response time should be less than 10ms
    And a cache HIT should be recorded in metrics
    And the database should NOT be queried
    And this is 5-15× faster than database query (50-150ms)

  # ============================================================================
  # SCENARIO 2: Cache Miss - Fallback to Database
  # ============================================================================
  Scenario: Query database on cache miss and cache the result
    Given phone number "8015551234" is NOT cached in Redis
    When I query for phone number "8015551234"
    Then the system should query the database (50-150ms)
    And the result should be cached in Redis with 24-hour TTL
    And a cache MISS should be recorded in metrics
    And subsequent requests should hit cache (sub-10ms)

  # ============================================================================
  # SCENARIO 3: CRITICAL - NEVER Use KEYS Command (Catastrophic Performance)
  # ============================================================================
  Scenario: KEYS command blocks entire Redis instance (O(n) operation)
    Given Redis contains 1,000,000 keys
    When someone runs KEYS * command on database 0
    Then ALL Redis operations should be blocked for 1-2 seconds
    And database 9, database 15, and ALL numbered databases should be frozen
    And this is O(n) operation that blocks entire Redis instance
    And KEYS command should be FORBIDDEN in production code
    And SCAN cursor pattern should be used instead (O(1) per call, non-blocking)

  # ============================================================================
  # SCENARIO 4: Use SCAN Instead of KEYS (Non-Blocking Iteration)
  # ============================================================================
  Scenario: Use SCAN cursor pattern for non-blocking key iteration
    Given I need to iterate over keys matching "phone:*" pattern
    When I use SCAN cursor pattern instead of KEYS
    Then the iteration should be non-blocking (O(1) per call)
    And other Redis operations should NOT be affected
    And SCAN should return cursor + partial results on each call
    And this allows safe iteration in production

  # ============================================================================
  # SCENARIO 5: CRITICAL - Always Set TTL (Prevent Memory Leak)
  # ============================================================================
  Scenario: Keys without TTL accumulate forever (memory leak)
    Given I cache phone number "8015551234" WITHOUT TTL
    When the cache entry is created
    Then the key will NEVER expire automatically
    And keys without TTL accumulate indefinitely (memory leak)
    And Redis eventually hits memory limit and triggers LRU eviction
    And eviction is unpredictable (you don't control WHAT gets evicted)
    And ALL cache keys MUST have TTL (industry standard)

  Scenario: Set TTL on every cache entry to prevent memory leak
    Given I cache phone number "8015551234"
    When the cache entry is created
    Then a 24-hour TTL should be set
    And the key should automatically expire after 24 hours
    And this prevents indefinite memory accumulation

  # ============================================================================
  # SCENARIO 6: TTL Jitter Prevents Thundering Herd
  # ============================================================================
  Scenario: Thundering herd when 10,000 keys expire simultaneously
    Given 10,000 cache entries were created at the same time
    And all entries have EXACT 24-hour TTL (no jitter)
    When all 10,000 entries expire at exactly the same time
    Then all 10,000 requests simultaneously hit the database (thundering herd)
    And database overload occurs
    And API latency spikes to 5-10 seconds

  Scenario: TTL jitter prevents synchronized cache expiration
    Given I cache phone number "8015551234"
    When the cache entry is created with TTL jitter
    Then the base TTL should be 24 hours
    And random jitter should be added: 0-60 minutes
    And the final TTL should be between 24:00 and 25:00 hours
    And this spreads cache expiration over 1-hour window
    And prevents thundering herd (synchronized database hits)

  # ============================================================================
  # SCENARIO 7: CRITICAL - ConnectionMultiplexer MUST Be Singleton
  # ============================================================================
  Scenario: Creating new ConnectionMultiplexer per request exhausts connections
    Given Redis allows 65,535 max connections per instance
    And the system creates new ConnectionMultiplexer on every request (anti-pattern)
    When high-traffic API creates 100+ connections/second
    Then connection exhaustion occurs in 10 minutes (65,535 / 600 seconds = 109/sec max)
    And new requests fail with "connection timeout" errors

  Scenario: Register ConnectionMultiplexer as singleton in DI container
    Given ConnectionMultiplexer is registered as singleton via DI
    When multiple requests are processed
    Then the same ConnectionMultiplexer instance should be reused
    And connection pooling should be managed automatically
    And no connection exhaustion should occur

  # ============================================================================
  # SCENARIO 8: Avoid Numbered Databases 0-15 (Architectural Flaw)
  # ============================================================================
  Scenario: Numbered databases share same connection pool and event loop
    Given Redis has numbered databases 0 through 15
    And I use database 0 for users and database 1 for sessions
    When someone runs KEYS * on database 0
    Then database 1 is also blocked (same event loop)
    And Redis creator called numbered databases "worst design mistake"
    And databases are NOT truly isolated (just namespace prefixes)
    And single database (0) with key prefixes should be used instead

  Scenario: Use single database with key prefixes for logical separation
    Given I use single database (0) with key prefixes
    When I store user data with prefix "user:" and session data with prefix "session:"
    Then logical separation is achieved via prefixes
    And no shared blocking risk between logical namespaces
    And this is the recommended Redis best practice

  # ============================================================================
  # SCENARIO 9: Network Latency is #1 Performance Bottleneck
  # ============================================================================
  Scenario: Sequential GET operations accumulate network round-trip latency
    Given I need to fetch 100 phone records from Redis
    And network round-trip time is 1-2ms per operation
    When I use sequential GET operations (100 round trips)
    Then the total latency is 100-200ms (network dominates)
    And Redis processes 100K+ ops/second (not the bottleneck)
    And network latency is the actual bottleneck

  Scenario: Use MGET batch operation to reduce network round trips
    Given I need to fetch 100 phone records from Redis
    When I use MGET batch operation (single round trip)
    Then all 100 records are retrieved in 2-5ms total latency
    And this is 20-40× faster than sequential GETs
    And pipelining or batching (MGET/MSET) reduces round trips

  # ============================================================================
  # SCENARIO 10: CRITICAL - AbortOnConnectFail=false for Graceful Degradation
  # ============================================================================
  Scenario: AbortOnConnectFail=true causes cascading failures
    Given AbortOnConnectFail is set to true (default, anti-pattern)
    And Redis becomes unavailable (network error, service outage)
    When API request attempts to access Redis
    Then exception is thrown and kills the API request
    And the API returns 500 Internal Server Error to user
    And Redis outage = complete API outage (tight coupling)

  Scenario: AbortOnConnectFail=false enables graceful degradation
    Given AbortOnConnectFail is set to false (best practice)
    And Redis becomes unavailable (network error, service outage)
    When API request attempts to access Redis
    Then cache GET returns null (no exception)
    And the system falls back to database query
    And the API continues to function (slower, but available)
    And Redis outage does NOT kill the entire API

  # ============================================================================
  # SCENARIO 11: Use IDistributedCache Interface (Provider-Agnostic)
  # ============================================================================
  Scenario: Direct StackExchange.Redis usage creates tight coupling
    Given the code directly uses IConnectionMultiplexer
    When requirements change to use Memcached or NCache
    Then complete code rewrite is required
    And testing requires real Redis instance (slow integration tests)
    And switching cache providers is difficult

  Scenario: IDistributedCache interface enables provider flexibility
    Given the code uses IDistributedCache interface
    When requirements change to use Memcached or NCache
    Then only DI registration needs to change (no code rewrite)
    And testing can use in-memory cache (fast unit tests)
    And switching cache providers is trivial

  # ============================================================================
  # SCENARIO 12: Large Value Sizes Hurt Performance (< 100KB Sweet Spot)
  # ============================================================================
  Scenario: Caching large values (> 1MB) causes performance degradation
    Given Equifax record has 398 columns (~5MB serialized)
    When I cache the entire record in Redis
    Then serialization/deserialization becomes slow
    And network transfer becomes bottleneck (5MB over network)
    And memory fragmentation increases
    And Redis performs best with values < 100KB (industry benchmark)

  Scenario: Cache only essential fields to reduce value size
    Given Equifax record has 398 columns (~5MB serialized)
    When I cache only top 15 most-accessed fields (~10KB)
    Then cache size reduces from 5MB to 10KB per record (500× reduction)
    And serialization/deserialization is fast
    And network transfer is efficient
    And this is within Redis sweet spot (< 100KB)

  # ============================================================================
  # SCENARIO 13: Missing Monitoring = Silent Cache Failures
  # ============================================================================
  Scenario: Cache hit rate unknown without monitoring
    Given cache monitoring is not implemented
    When cache hit rate drops from 90% to 20%
    Then database overload occurs (unnoticed)
    And no visibility into eviction rate, memory pressure, connection issues
    And silent degradation goes undetected

  Scenario: Emit cache metrics for monitoring and alerting
    Given cache monitoring is enabled
    When cache operations occur
    Then the following metrics should be emitted:
      | metric_name        | description                        |
      | cache_hit_count    | Number of cache hits               |
      | cache_miss_count   | Number of cache misses             |
      | cache_hit_rate     | Percentage of cache hits           |
      | cache_latency      | Cache operation latency (ms)       |
      | eviction_count     | Number of keys evicted             |
    And CloudWatch alarms should trigger for:
      | condition                | threshold  | action         |
      | cache_hit_rate < 70%     | 70%        | Alert DevOps   |
      | cache_latency > 50ms     | 50ms       | Alert DevOps   |
      | eviction_rate high       | 1000/min   | Alert DevOps   |

  # ============================================================================
  # SCENARIO 14: Cache Statistics Tracking
  # ============================================================================
  Scenario: Track cache hit/miss statistics for monitoring
    Given the cache service tracks hit/miss statistics
    And 900 requests result in cache hits
    And 100 requests result in cache misses
    When cache statistics are retrieved
    Then cache_hits should be 900
    And cache_misses should be 100
    And hit_rate should be 0.90 (90%)
    And total_requests should be 1,000

  # ============================================================================
  # SCENARIO 15: Graceful Degradation on Redis Failure
  # ============================================================================
  Scenario: Cache GET failure falls back to database
    Given Redis is unavailable
    When I query for phone number "8015551234"
    Then cache GET should catch exception and return null
    And the system should fall back to database query
    And the API request should succeed (slower, but functional)
    And a warning should be logged: "Redis cache GET failed - Falling back to database"

  Scenario: Cache SET failure does not fail API request
    Given Redis is unavailable
    And I successfully query the database for phone number "8015551234"
    When I attempt to cache the result
    Then cache SET should catch exception
    And the API request should complete successfully (result returned to user)
    And a warning should be logged: "Redis cache SET failed - Continuing without cache"
    And cache write failures are non-critical (data still served from DB)

  # ============================================================================
  # SCENARIO 16: Cache Key Structure (Prefix-Based Organization)
  # ============================================================================
  Scenario: Use consistent key naming convention
    Given I cache phone number "8015551234"
    When the cache key is generated
    Then the key should follow pattern: "phone:<phone_number>"
    And the key should be: "phone:8015551234"
    And prefix-based organization enables:
      | benefit                  | description                        |
      | logical_separation       | Different data types have prefixes |
      | pattern_matching         | SCAN phone:* finds all phone keys  |
      | bulk_operations          | Delete all phone keys via pattern  |

  # ============================================================================
  # SCENARIO 17: Batch Cache Operations (MGET Performance)
  # ============================================================================
  Scenario: Batch cache lookup for multiple phone numbers
    Given I need to look up 50 phone numbers
    When I use batch cache lookup (Task.WhenAll)
    Then all 50 lookups should execute concurrently
    And the total time should be < 50ms (parallelized)
    And cache hits should be returned immediately
    And cache misses should fallback to database query
    And batch statistics should be logged

  # ============================================================================
  # SCENARIO 18: Cache Only Essential Fields (Data Minimization)
  # ============================================================================
  Scenario: Cache top 15 most-accessed fields instead of all 398 columns
    Given Equifax record has 398 columns
    And API usage analytics show top 15 fields accessed 95% of the time
    When I cache the record
    Then only these fields should be cached:
      | field             | reason                             |
      | consumer_key      | Primary identifier                 |
      | first_name        | Most accessed PII field            |
      | last_name         | Most accessed PII field            |
      | middle_name       | Common PII field                   |
      | phone_1           | Queried phone number               |
      | phone_1_type      | Phone type metadata                |
      | address_1         | Most accessed address              |
      | city_1            | Geographic data                    |
      | state_1           | Geographic data                    |
      | zip_1             | Geographic data                    |
      | email_1           | Contact information                |
      | date_of_birth     | Demographic data                   |
      | age               | Demographic data                   |
      | gender            | Demographic data                   |
    And this reduces cache size from 5MB to 10KB per record

  # ============================================================================
  # SCENARIO 19: Connection Pooling with Singleton ConnectionMultiplexer
  # ============================================================================
  Scenario: ConnectionMultiplexer manages connection pooling automatically
    Given ConnectionMultiplexer is singleton
    When multiple concurrent requests access Redis
    Then connection pooling should be managed automatically
    And connections should be reused efficiently
    And no connection leaks should occur
    And StackExchange.Redis handles connection multiplexing internally

  # ============================================================================
  # SCENARIO 20: AWS ElastiCache Redis Cluster Configuration
  # ============================================================================
  Scenario: Configure Redis connection for AWS ElastiCache
    Given AWS ElastiCache Redis cluster is deployed
    When Redis connection is configured
    Then the configuration should include:
      | setting              | value                              |
      | endpoint             | sb-redis.cache.amazonaws.com:6379  |
      | ssl                  | true (encrypted in transit)        |
      | abort_on_connect_fail| false (graceful degradation)       |
      | connect_retry        | 3 attempts                         |
      | connect_timeout      | 5000ms                             |
      | sync_timeout         | 5000ms                             |
      | allow_admin          | false (security - no admin in prod)|
    And AWS ElastiCache provides managed Redis with automatic failover
