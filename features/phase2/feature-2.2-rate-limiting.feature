Feature: Distributed Rate Limiting with Redis
  As an API system
  I want to enforce rate limits using distributed Redis-based rate limiting
  So that API resources are protected and usage is tracked for billing

  Background:
    Given Redis-based distributed rate limiting is enabled
    And AWS ElastiCache Redis cluster is configured
    And rate limits are enforced using Lua scripts for atomic operations
    And sliding window counter algorithm is implemented
    And token bucket algorithm is available for burst handling
    And overage billing is enabled at $0.035 per qualified call
    And HTTP 429 responses include standard rate limit headers

  # ============================================================================
  # SCENARIO 1: Rate Limit Configuration (Logical Consistency Check)
  # ============================================================================
  Scenario: Verify rate limit configuration is mathematically consistent
    Given the rate limit configuration is:
      | time_period | limit     | description                          |
      | per_minute  | 1,000     | Burst protection                     |
      | per_hour    | 60,000    | Sustained load (1000/min × 60)       |
      | per_day     | 1,000,000 | Daily quota (allows variance)        |
      | burst_10s   | 1,500     | Burst allowance (50% over per-second)|
    Then the hourly limit should equal per-minute × 60 (1,000 × 60 = 60,000) ✓
    And the daily limit should accommodate hourly variance (60,000 × 24 = 1,440,000 theoretical, cap at 1M)
    And the burst limit should allow 1,500 requests in 10 seconds (150/sec vs 16.67/sec sustained = 9× burst)
    And the configuration should be logically consistent (no mathematical impossibilities)

  # ============================================================================
  # SCENARIO 2: Requests Within Rate Limit (Happy Path)
  # ============================================================================
  Scenario: Allow requests within per-minute rate limit
    Given I have a valid API key for buyer 12345
    And the per-minute rate limit is 1,000 requests
    And I have made 950 requests in the current minute
    When I send request #951 within the same minute
    Then the request should be allowed (200 OK)
    And the response should include rate limit headers:
      | header                  | value                              |
      | X-RateLimit-Limit       | 1000                               |
      | X-RateLimit-Remaining   | 49                                 |
      | X-RateLimit-Reset       | <seconds_until_minute_resets>      |
    And the request should proceed to downstream middleware
    And no overage billing should be tracked

  # ============================================================================
  # SCENARIO 3: Rate Limit Exceeded (429 Too Many Requests)
  # ============================================================================
  Scenario: Reject requests exceeding per-minute rate limit
    Given I have a valid API key for buyer 12345
    And the per-minute rate limit is 1,000 requests
    And I have already made 1,000 requests in the current minute
    When I send request #1,001 within the same minute
    Then the response status should be 429 (Too Many Requests)
    And the response should include rate limit headers:
      | header                  | value                              |
      | X-RateLimit-Limit       | 1000                               |
      | X-RateLimit-Remaining   | 0                                  |
      | X-RateLimit-Reset       | <seconds_until_minute_resets>      |
      | Retry-After             | <seconds_until_minute_resets>      |
    And the error message should be "Rate limit exceeded"
    And the error should state "You have exceeded the rate limit of 1000 requests per minute"
    And the error should include overage billing message: "Overage requests are billed at $0.035 per call"
    And an overage tracking entry should be created for billing
    And the request should NOT proceed to downstream middleware

  # ============================================================================
  # SCENARIO 4: CRITICAL - Atomic Operations with Lua Scripts (No Race Conditions)
  # ============================================================================
  Scenario: Use Lua scripts to prevent race conditions in distributed environment
    Given I have 3 application servers running simultaneously
    And buyer 12345 has 999 requests already consumed in the current minute
    When server A and server B both receive a request at the exact same millisecond
    And both servers read "999 requests" from Redis simultaneously
    Then the Lua script should execute atomically (single Redis operation)
    And only ONE of the two servers should be allowed to increment to 1,000
    And the other server should receive 1,001 and be denied (429 status)
    And there should be NO race condition (get-then-set would allow both through)
    And the final count should be exactly 1,000 (not 1,001 or 1,002)

  # ============================================================================
  # SCENARIO 5: Sliding Window Counter Algorithm (Precise Rate Limiting)
  # ============================================================================
  Scenario: Use sliding window for precise rate limit enforcement
    Given the per-minute rate limit is 1,000 requests
    And I made 1,000 requests between 10:00:00 and 10:00:59
    When I send a request at 10:01:15 (75 seconds after first request)
    Then the sliding window should remove requests older than 60 seconds
    And requests from before 10:00:15 should be expired from the window
    And the current count should be less than 1,000 (old requests expired)
    And the new request should be allowed
    And this prevents the "thundering herd" problem at minute boundaries

  # ============================================================================
  # SCENARIO 6: CRITICAL - Native .NET Rate Limiter Limitations (In-Memory Only)
  # ============================================================================
  Scenario: Native .NET rate limiter fails in multi-node deployments
    Given I have 3 application servers (multi-node deployment)
    And each server uses native Microsoft.AspNetCore.RateLimiting (in-memory)
    And the per-minute rate limit is 1,000 requests per API key
    When buyer 12345 sends 1,000 requests to server A
    And buyer 12345 sends 1,000 requests to server B
    And buyer 12345 sends 1,000 requests to server C
    Then the total requests should be 3,000 (3× the intended limit)
    And each server enforces limits independently (NOT distributed)
    And counters reset on application restart (lose all tracking)
    And this demonstrates why Redis-based rate limiting is required for multi-node deployments

  # ============================================================================
  # SCENARIO 7: Per-API-Key Rate Limiting (More Accurate than IP-Based)
  # ============================================================================
  Scenario: Rate limit by API key for accurate per-buyer enforcement
    Given buyer A has API key "key_buyer_a"
    And buyer B has API key "key_buyer_b"
    And both buyers are behind the same NAT gateway (same source IP: 192.168.1.100)
    And the rate limit is 1,000 requests per minute per API key
    When buyer A sends 1,000 requests from IP 192.168.1.100
    And buyer B sends 1,000 requests from the same IP 192.168.1.100
    Then buyer A should be rate limited (1,000 requests consumed)
    But buyer B should NOT be rate limited (separate API key = separate limit)
    And both buyers should be able to send 1,000 requests each (2,000 total from same IP)
    And this demonstrates why per-API-key rate limiting is superior to IP-based

  # ============================================================================
  # SCENARIO 8: IP-Based Rate Limiting Insufficient (NAT/Proxy Environments)
  # ============================================================================
  Scenario: IP-based rate limiting unfairly blocks users behind NAT
    Given the system uses IP-based rate limiting (anti-pattern)
    And 100 users share the same NAT gateway IP (192.168.1.100)
    And the rate limit is 1,000 requests per minute per IP
    When all 100 users send 10 requests each (1,000 total)
    Then the IP 192.168.1.100 should hit the rate limit
    And all 100 users should be blocked (unfair - they only sent 10 requests each)
    And this demonstrates the problem with IP-only rate limiting
    And the system should use per-API-key rate limiting instead

  # ============================================================================
  # SCENARIO 9: Clock Skew Between Distributed Nodes (Single Source of Truth)
  # ============================================================================
  Scenario: Use Redis server time to avoid clock skew issues
    Given server A has system clock at 10:00:00
    And server B has system clock at 10:00:05 (5 second skew)
    And both servers use Redis server time (single source of truth)
    When server A records a request at Redis timestamp 1234567890
    And server B records a request at Redis timestamp 1234567892
    Then both servers should use consistent Redis timestamps (no skew)
    And rate limit windows should be consistent across all servers
    And Unix timestamps should be used (no timezone issues)

  # ============================================================================
  # SCENARIO 10: Token Bucket Algorithm for Burst Handling
  # ============================================================================
  Scenario: Allow burst traffic using token bucket algorithm
    Given the token bucket has capacity: 1,000 tokens
    And the refill rate is 16.67 tokens/second (1,000/60)
    And the bucket is currently full (1,000 tokens available)
    When buyer 12345 sends 1,500 requests in 10 seconds (burst)
    Then the first 1,000 requests should consume all tokens immediately
    And the remaining 500 requests should be denied (429 status)
    But after 30 seconds (30 × 16.67 = 500 tokens refilled)
    Then 500 more requests should be allowed
    And this allows legitimate burst traffic while preventing sustained overload

  # ============================================================================
  # SCENARIO 11: Overage Billing Tracking (Real-Time Redis Counters)
  # ============================================================================
  Scenario: Track overage requests for monthly billing
    Given buyer 12345 has monthly quota: 100,000 included calls
    And the overage rate is $0.035 per qualified call
    And buyer 12345 has consumed 100,000 calls this month
    When buyer 12345 makes request #100,001 (first overage)
    Then the request should still be allowed (not rate limited, just billed)
    And an overage counter should be incremented in Redis: "overage:buyer:12345:202501"
    And the overage counter should have 2-month TTL (for billing reconciliation)
    And the 429 response should include overage billing message

  # ============================================================================
  # SCENARIO 12: Overage Billing Calculation (Monthly Reconciliation)
  # ============================================================================
  Scenario: Calculate monthly overage charges from audit logs
    Given buyer 12345 has monthly quota: 100,000 included calls
    And the overage rate is $0.035 per qualified call
    And buyer 12345 made 150,000 total requests in January 2025
    And 140,000 of those requests resulted in matches (qualified calls)
    When the automated billing job runs on February 1st, 2025
    Then the overage should be calculated as: 140,000 - 100,000 = 40,000 overage calls
    And the overage charge should be: 40,000 × $0.035 = $1,400
    And the base subscription fee should be: $1,000
    And the total monthly charge should be: $1,000 + $1,400 = $2,400
    And a billing record should be created with status "pending"
    And an invoice should be sent to the buyer

  # ============================================================================
  # SCENARIO 13: Only Qualified Calls Billed (Matches Found, Not 404s)
  # ============================================================================
  Scenario: Only bill for qualified calls (successful matches)
    Given buyer 12345 has monthly quota: 100,000 included calls
    And the overage rate is $0.035 per qualified call
    And buyer 12345 made 200,000 total API requests
    But only 120,000 requests resulted in matches (qualified calls)
    And 80,000 requests returned "no match found" (404 equivalent)
    When the monthly billing is calculated
    Then the billable calls should be 120,000 (only qualified calls)
    And the overage should be: 120,000 - 100,000 = 20,000 calls
    And the overage charge should be: 20,000 × $0.035 = $700
    And the "no match found" requests should NOT be billed

  # ============================================================================
  # SCENARIO 14: HTTP Standard Rate Limit Headers (Industry Best Practice)
  # ============================================================================
  Scenario: Include standard rate limit headers in all responses
    Given I make a request that is rate limited
    When the API returns 429 (Too Many Requests)
    Then the response must include these standard headers:
      | header                  | description                              | example      |
      | X-RateLimit-Limit       | Total requests allowed in window         | 1000         |
      | X-RateLimit-Remaining   | Requests remaining in current window     | 0            |
      | X-RateLimit-Reset       | Seconds until rate limit window resets   | 45           |
      | Retry-After             | Seconds to wait before retrying (RFC)    | 45           |
    And these headers should comply with IETF draft-ietf-httpapi-ratelimit-headers
    And API consumers can use these headers to implement client-side rate limiting

  # ============================================================================
  # SCENARIO 15: Per-Buyer Custom Rate Limits (Enterprise Plans)
  # ============================================================================
  Scenario: Support custom rate limits for enterprise buyers
    Given buyer A has standard plan with 1,000 requests/minute
    And buyer B has enterprise plan with 10,000 requests/minute
    And buyer C has basic plan with 100 requests/minute
    When buyer A sends 1,001 requests in one minute
    Then buyer A should be rate limited (429 status)
    When buyer B sends 5,000 requests in one minute
    Then buyer B should NOT be rate limited (within custom limit)
    When buyer C sends 101 requests in one minute
    Then buyer C should be rate limited (429 status)
    And custom limits should be stored in RateLimitConfig table per buyer

  # ============================================================================
  # SCENARIO 16: Rate Limit Reset at Window Boundary
  # ============================================================================
  Scenario: Rate limit counters reset at window boundary (sliding window)
    Given buyer 12345 has rate limit: 1,000 requests per minute
    And buyer 12345 made 1,000 requests between 10:00:00 and 10:00:59
    And buyer 12345 is currently rate limited (429 responses)
    When the time reaches 10:01:00 (new minute begins)
    And 60 seconds have passed since the first request at 10:00:00
    Then the sliding window should expire all requests from before 10:00:00
    And the rate limit counter should reset to 0
    And the next request should be allowed (200 OK)
    And X-RateLimit-Remaining should be 999

  # ============================================================================
  # SCENARIO 17: Redis Connection Failure - Graceful Degradation
  # ============================================================================
  Scenario: Handle Redis unavailability with fail-open strategy
    Given Redis-based rate limiting is configured
    And Redis connection becomes unavailable (network error, service outage)
    When I send a request to the API
    Then the system should fail open (allow request to proceed)
    And a critical alert should be logged: "Redis unavailable - rate limiting disabled"
    And the request should proceed to downstream middleware
    And circuit breaker pattern should prevent excessive Redis connection attempts
    And fallback to in-memory rate limiting should be activated (per-node)

  # ============================================================================
  # SCENARIO 18: Rate Limit Performance (Sub-5ms Overhead)
  # ============================================================================
  Scenario: Rate limit check completes in less than 5ms
    Given I send a request to the API
    When the rate limit middleware processes the request
    Then the rate limit check should complete in less than 5ms
    And the Redis Lua script should execute in less than 2ms
    And network round-trip to AWS ElastiCache should be less than 3ms
    And the total API response time should not increase by more than 5ms

  # ============================================================================
  # SCENARIO 19: Rate Limit Monitoring and Alerts
  # ============================================================================
  Scenario: Monitor rate limit hit rates and alert on anomalies
    Given rate limit monitoring is enabled in CloudWatch
    When buyer 12345 hits rate limits 100 times in one hour
    Then a CloudWatch metric should be published: "RateLimitExceeded"
    And the metric should include dimensions: [BuyerId, Endpoint]
    And an alert should trigger: "Buyer 12345 exceeding rate limits frequently - possible attack or misconfiguration"
    And the security team should be notified
    And a dashboard should display real-time rate limit usage per buyer

  # ============================================================================
  # SCENARIO 20: Lua Script Atomicity (Single Redis Operation)
  # ============================================================================
  Scenario: Lua script executes as single atomic Redis operation
    Given the rate limit Lua script performs multiple operations:
      | operation                          | description                        |
      | ZREMRANGEBYSCORE                   | Remove expired entries             |
      | ZCARD                              | Count requests in current window   |
      | ZADD                               | Add current request with timestamp |
      | EXPIRE                             | Set TTL on rate limit key          |
    When the Lua script executes
    Then all 4 operations should execute atomically (single Redis transaction)
    And no other client can interleave operations during script execution
    And this prevents race conditions in distributed environment
    And the script should return: [allowed (0/1), remaining, reset_time]

  # ============================================================================
  # SCENARIO 21: Rate Limit Key Structure (Per API Key)
  # ============================================================================
  Scenario: Use hierarchical Redis key structure for rate limits
    Given buyer 12345 has API key "157659ac293445df00772760e6114ac4"
    When the rate limit middleware generates Redis keys
    Then the per-minute key should be: "rate_limit:apikey:157659ac:minute"
    And the per-hour key should be: "rate_limit:apikey:157659ac:hour"
    And the per-day key should be: "rate_limit:apikey:157659ac:day"
    And the overage key should be: "overage:buyer:12345:202501" (month-based)
    And keys should use API key prefix (first 8 chars) for readability
    And keys should have appropriate TTLs: minute=120s, hour=7200s, day=172800s

  # ============================================================================
  # SCENARIO 22: Burst Allowance Calculation
  # ============================================================================
  Scenario: Calculate burst allowance as percentage over sustained rate
    Given the sustained rate is 1,000 requests per minute (16.67/second)
    And the burst allowance is 50% over sustained rate
    When burst traffic is allowed for 10 seconds
    Then the burst limit should be: 16.67/sec × 10 sec × 1.5 = 1,500 requests
    And this allows 250 extra requests during burst (1,500 - 1,250 sustained)
    And burst capacity should be 9× sustained rate (150/sec burst vs 16.67/sec sustained)

  # ============================================================================
  # SCENARIO 23: Overage Billing Report Generation
  # ============================================================================
  Scenario: Generate detailed monthly overage billing report
    Given buyer 12345 used 150,000 qualified calls in January 2025
    And the monthly quota is 100,000 included calls
    When the automated billing job runs on February 1st
    Then a billing report should be generated with:
      | field                   | value                              |
      | buyer_id                | 12345                              |
      | billing_month           | 2025-01-01                         |
      | total_requests          | 150,000                            |
      | included_quota          | 100,000                            |
      | overage_requests        | 50,000                             |
      | price_per_call          | $0.035                             |
      | overage_charge          | $1,750.00                          |
      | base_subscription_fee   | $1,000.00                          |
      | total_charge            | $2,750.00                          |
      | generated_at            | <current_utc_timestamp>            |
      | status                  | pending                            |

  # ============================================================================
  # SCENARIO 24: Distributed Rate Limiting Consistency Test
  # ============================================================================
  Scenario: Verify rate limit consistency across multiple application servers
    Given I have 5 application servers in production
    And all servers use the same Redis cluster for rate limiting
    And buyer 12345 has rate limit: 1,000 requests per minute
    When 5 servers each receive 200 requests simultaneously for buyer 12345
    Then the total allowed requests should be exactly 1,000 (not 1,005 or 995)
    And the 1,001st request should receive 429 status regardless of which server
    And all servers should have consistent view of rate limit counters
    And Redis should be the single source of truth for all counters

  # ============================================================================
  # SCENARIO 25: Contractual Compliance - Section 3.2 Pricing
  # ============================================================================
  Scenario: Enforce contractual overage pricing per Section 3.2
    Given the contract specifies $0.035 per qualified call in Section 3.2
    When overage billing is calculated
    Then the price per call must be exactly $0.035 (contractual requirement)
    And the price should NOT be configurable without contract amendment
    And the price should be displayed in 429 responses for transparency
    And billing reports should reference Section 3.2 for compliance
    And any pricing changes require contract renegotiation
