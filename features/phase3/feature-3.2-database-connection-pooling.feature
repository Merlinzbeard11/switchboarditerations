Feature: Database Connection Pooling (Npgsql + EF Core Optimization)
  As a high-performance API system
  I want to manage database connections efficiently through pooling
  So that connection exhaustion is avoided and performance is optimized

  Background:
    Given Npgsql connection pooling is enabled
    And AWS RDS PostgreSQL db.r5.2xlarge with max_connections=500
    And EF Core DbContext pooling is configured with poolSize=128
    And connection string is consistent across application
    And prepared statements are enabled (Max Auto Prepare=10)
    And connection pool monitoring is active

  # ============================================================================
  # SCENARIO 1: CRITICAL - Connection Leaks Cause Pool Exhaustion (ALWAYS Use `using`)
  # ============================================================================
  Scenario: Connection leak from not disposing causes gradual pool exhaustion
    Given MaxPoolSize is 100 connections
    And connections are NOT properly disposed (anti-pattern)
    When 100 connections are leaked (exception prevents Dispose)
    Then new requests fail with connection timeout error
    And PostgreSQL shows 100 "idle" or "idle in transaction" connections
    And API returns 500 errors to users
    And this is the #1 cause of pool exhaustion in production

  Scenario: using statement ensures connection disposal even on exception
    Given a database query throws an exception
    When the connection is wrapped in using statement
    Then the connection should be disposed automatically
    And the connection should be returned to pool
    And no connection leak should occur
    And this prevents gradual pool exhaustion

  Scenario: EF Core handles connection disposal automatically
    Given I use ApplicationDbContext with using statement
    When database operations complete (success or exception)
    Then EF Core disposes connection automatically
    And connection is returned to pool
    And no manual Dispose() call is needed

  # ============================================================================
  # SCENARIO 2: Connection String Inconsistency Creates Separate Pools
  # ============================================================================
  Scenario: Different connection strings create separate pools (pool fragmentation)
    Given connection string A uses "Password=abc"
    And connection string B uses "password=abc" (lowercase 'password')
    When both connection strings are used
    Then TWO separate connection pools are created
    And each pool has its own MaxPoolSize limit
    And total connections = Pool A size + Pool B size
    And this can exhaust PostgreSQL max_connections with multiple pools
    And ANY difference in connection string = separate pool (case-sensitive keys, whitespace)

  Scenario: Use single consistent connection string from IConfiguration
    Given connection string is retrieved from IConfiguration
    When all components use Configuration.GetConnectionString("EquifaxDb")
    Then only ONE connection pool is created
    And all connections share the same pool
    And no pool fragmentation occurs

  # ============================================================================
  # SCENARIO 3: Two Pooling Layers - Npgsql vs. EF Core DbContext
  # ============================================================================
  Scenario: Understand two distinct pooling layers
    Given Npgsql manages physical database connections (Layer 1)
    And EF Core can pool DbContext instances (Layer 2)
    Then Layer 1 (Npgsql) pools connections: MinPoolSize=20, MaxPoolSize=200
    And Layer 2 (EF Core) pools DbContext instances: poolSize=128
    And AddDbContextPool does NOT affect connection pooling (only DbContext reuse)
    And both layers provide independent performance benefits
    And connection pooling is automatic, DbContext pooling is opt-in

  Scenario: Use AddDbContextPool for high-traffic scenarios
    Given the API handles 1,000+ requests/second
    When AddDbContextPool is used instead of AddDbContext
    Then up to 128 DbContext instances should be reused
    And DbContext setup cost should be reduced
    And garbage collection pressure should be reduced
    And this is recommended for high-traffic APIs

  # ============================================================================
  # SCENARIO 4: PgBouncer Compatibility (No Reset On Close)
  # ============================================================================
  Scenario: DISCARD ALL fails in PgBouncer transaction/statement mode
    Given PgBouncer is used in transaction mode
    And Npgsql executes DISCARD ALL when returning connection to pool
    When connection is returned to pool
    Then DISCARD ALL fails in PgBouncer transaction/statement mode
    And errors occur: "DISCARD ALL not allowed in transaction mode"
    And this breaks connection pooling

  Scenario: Set No Reset On Close=true when using PgBouncer
    Given PgBouncer is used in transaction or statement mode
    When connection string includes "No Reset On Close=true"
    Then Npgsql skips DISCARD ALL on connection return
    And connection pooling works correctly with PgBouncer
    And this is required for PgBouncer compatibility

  # ============================================================================
  # SCENARIO 5: Connection Lifetime vs. Idle Lifetime Confusion
  # ============================================================================
  Scenario: Short Connection Lifetime kills active long-running queries
    Given Connection Lifetime is set to 60 seconds
    And a query takes 90 seconds to execute (long-running report)
    When the connection reaches 60-second lifetime
    Then the connection is terminated EVEN IF query is active
    And the query fails with connection closed error
    And this is NOT desired behavior

  Scenario: Set high Connection Lifetime, low Idle Lifetime
    Given Connection Lifetime is set to 600 seconds (10 minutes)
    And Connection Idle Lifetime is set to 60 seconds
    When a long-running query executes for 5 minutes
    Then the connection remains active until query completes
    When a connection is idle for 60 seconds
    Then the idle connection is returned to pool
    And active queries are not interrupted
    And idle connections are released efficiently

  # ============================================================================
  # SCENARIO 6: Missing Max Auto Prepare = Missed Performance Gains
  # ============================================================================
  Scenario: No prepared statements = repeated query parsing overhead
    Given Max Auto Prepare is not configured (default 0)
    When the same SQL query executes 1,000 times
    Then PostgreSQL parses and plans the query 1,000 times
    And each execution incurs parsing overhead
    And this wastes CPU and adds latency

  Scenario: Automatic prepared statements improve performance 20-40%
    Given Max Auto Prepare is set to 10
    When frequently-used queries execute
    Then Npgsql automatically prepares up to 10 unique statements
    And prepared statements are reused (no re-parsing)
    And performance improves by 20-40% (industry benchmark)
    And this is a simple performance win

  # ============================================================================
  # SCENARIO 7: Pool Size Too Small for Concurrent Load
  # ============================================================================
  Scenario: MaxPoolSize too small causes request queuing and timeouts
    Given MaxPoolSize is 100 connections
    And 200 concurrent requests arrive
    When all 200 requests need database connections
    Then the first 100 requests get connections immediately
    And the remaining 100 requests wait in queue
    And requests timeout after 30 seconds (connection timeout)
    And API returns 500 errors for queued requests

  Scenario: Calculate MaxPoolSize based on expected load
    Given expected concurrent requests: 1,000/second
    And average query time: 50ms (0.05 seconds)
    When calculating MaxPoolSize
    Then concurrent queries = 1,000 × 0.05 = 50
    And buffer for spikes = 50 × 2 = 100
    And recommended MaxPoolSize = 100
    And this formula prevents pool exhaustion: (Concurrent Requests) / (Avg Query Time)

  Scenario: AWS RDS max_connections=500, use MaxPoolSize=200 (leave buffer)
    Given AWS RDS max_connections is 500
    When configuring connection pool
    Then MaxPoolSize should be 200 (40% of RDS limit)
    And remaining 300 connections reserved for:
      | purpose                    | connections |
      | other_services             | 100         |
      | admin_connections          | 50          |
      | replication                | 50          |
      | buffer_for_spikes          | 100         |
    And this prevents exhausting RDS max_connections

  # ============================================================================
  # SCENARIO 8: Monitoring for Connection Leaks (PostgreSQL Side)
  # ============================================================================
  Scenario: Detect connection leaks using pg_stat_activity
    Given connection leaks are occurring
    When querying pg_stat_activity for idle connections > 5 minutes
    Then leaked connections show state = 'idle' or 'idle in transaction'
    And state_change timestamp shows connection idle for 5+ minutes
    And these connections should be investigated and fixed
    And monitoring should alert on idle connections > 5 minutes

  # ============================================================================
  # SCENARIO 9: EF Core Connection Leak Detection
  # ============================================================================
  Scenario: Enable EF Core DbContext pooling with leak detection
    Given AddDbContextPool is used instead of AddDbContext
    When DbContext instances are not properly disposed
    Then EF Core automatically detects leaked DbContext instances
    And warnings are logged: "DbContext pooled instance leaked"
    And this helps identify code locations causing leaks
    And leak detection is automatically enabled with AddDbContextPool

  # ============================================================================
  # SCENARIO 10: Optimize Queries BEFORE Increasing Pool Size
  # ============================================================================
  Scenario: Increasing pool size does not fix slow queries
    Given a slow query takes 5 seconds to execute
    And MaxPoolSize is 100 connections
    When MaxPoolSize is increased to 200 connections
    Then the slow query still takes 5 seconds
    And increasing pool size does NOT improve query performance
    And this is optimizing the wrong thing

  Scenario: Optimization priority order - queries before pool size
    Given slow database performance is observed
    Then optimization should follow this order:
      | step | optimization                        | expected_gain |
      | 1    | Fix slow queries (add indexes)      | 10-100×       |
      | 2    | Fix N+1 query problems              | 5-20×         |
      | 3    | Optimize query plans                | 2-10×         |
      | 4    | Increase pool size (if needed)      | 1.2-2×        |
    And query optimization provides far greater gains than pool tuning

  # ============================================================================
  # SCENARIO 11: Connection String Parameters Explained
  # ============================================================================
  Scenario: Optimized connection string configuration for production
    Given the production connection string is configured
    Then it should include these parameters:
      | parameter                | value | description                            |
      | Pooling                  | true  | Enable connection pooling              |
      | MinPoolSize              | 20    | Keep 20 connections warm (cold start)  |
      | MaxPoolSize              | 200   | Allow up to 200 concurrent connections |
      | Connection Lifetime      | 600   | Close connections after 10 minutes     |
      | Connection Idle Lifetime | 60    | Return idle connections after 60s      |
      | Max Auto Prepare         | 10    | Prepare up to 10 frequent statements   |
      | Command Timeout          | 30    | Query timeout (prevent long-running)   |
      | Timeout                  | 30    | Connection attempt timeout (fail fast) |
      | Keepalive                | 30    | TCP keepalive every 30s (detect dead)  |
    And these settings optimize for AWS RDS environment

  # ============================================================================
  # SCENARIO 12: Connection Resiliency (Retry on Transient Failures)
  # ============================================================================
  Scenario: Enable retry on transient failures
    Given EF Core connection resiliency is enabled
    And maxRetryCount is 3
    And maxRetryDelay is 5 seconds
    When a transient network error occurs
    Then EF Core should retry the operation up to 3 times
    And each retry should wait up to 5 seconds (exponential backoff)
    And transient errors should not fail API requests
    And this handles temporary network glitches automatically

  # ============================================================================
  # SCENARIO 13: Query Splitting for Large Joins (Cartesian Explosion)
  # ============================================================================
  Scenario: Single query with multiple includes causes cartesian explosion
    Given an EF Core query includes multiple collections
    When UseQuerySplittingBehavior is NOT enabled
    Then a single SQL query with multiple JOINs is generated
    And cartesian product occurs (N × M × P rows)
    And large result sets are transferred over network
    And memory usage spikes

  Scenario: Query splitting prevents cartesian explosion
    Given UseQuerySplittingBehavior.SplitQuery is enabled
    When an EF Core query includes multiple collections
    Then multiple separate SQL queries are executed
    And each collection is fetched independently
    And no cartesian product occurs
    And network transfer and memory usage are optimized

  # ============================================================================
  # SCENARIO 14: Health Checks for Connection Pool
  # ============================================================================
  Scenario: Monitor connection pool health via health check endpoint
    Given AddHealthChecks with AddNpgSql is configured
    When /health endpoint is queried
    Then the health check should verify database connectivity
    And the health check should report:
      | status      | condition                          |
      | Healthy     | Connection successful < 5s         |
      | Degraded    | Connection slow 5-10s              |
      | Unhealthy   | Connection timeout > 10s           |
    And this enables automated monitoring and alerting

  # ============================================================================
  # SCENARIO 15: Development vs. Production Logging
  # ============================================================================
  Scenario: Enable detailed logging in development only
    Given Environment.IsDevelopment() is true
    When EF Core is configured
    Then EnableDetailedErrors should be enabled
    And EnableSensitiveDataLogging should be enabled
    And LogTo(Console.WriteLine) should log all SQL queries
    And this helps debug connection and query issues

  Scenario: Disable sensitive logging in production
    Given Environment.IsProduction() is true
    When EF Core is configured
    Then EnableSensitiveDataLogging should be disabled
    And query parameters should NOT be logged
    And this prevents exposing sensitive data in logs

  # ============================================================================
  # SCENARIO 16: MinPoolSize Benefits (Warm Connection Pool)
  # ============================================================================
  Scenario: MinPoolSize keeps connections warm (reduces cold start latency)
    Given MinPoolSize is set to 20
    When the application starts
    Then 20 connections should be opened immediately
    And these connections remain in pool even when idle
    And first 20 API requests have no connection establishment latency
    And this reduces cold start time from 50ms to 1ms

  # ============================================================================
  # SCENARIO 17: Command Timeout Prevents Long-Running Query Locks
  # ============================================================================
  Scenario: Command timeout prevents queries from holding connections indefinitely
    Given Command Timeout is set to 30 seconds
    When a query takes longer than 30 seconds
    Then Npgsql cancels the query
    And the connection is returned to pool
    And this prevents long-running queries from exhausting pool
    And poorly-optimized queries are detected and fail fast

  # ============================================================================
  # SCENARIO 18: Proper Repository Pattern Usage
  # ============================================================================
  Scenario: Repository pattern with EF Core manages connections automatically
    Given ApplicationDbContext is injected via DI
    When repository methods query the database
    Then EF Core manages connection lifecycle automatically
    And connection is opened on first query
    And connection is disposed when context is disposed
    And no manual connection management is needed

  # ============================================================================
  # SCENARIO 19: Detect Pool Exhaustion Before It Happens
  # ============================================================================
  Scenario: Monitor pool size metrics to detect approaching exhaustion
    Given MaxPoolSize is 200 connections
    And current active connections is 180
    When monitoring checks pool utilization
    Then pool utilization is 90% (180/200)
    And an alert should trigger: "Connection pool nearing capacity - 90% utilization"
    And this allows proactive scaling before exhaustion

  # ============================================================================
  # SCENARIO 20: AWS RDS Failover Compatibility
  # ============================================================================
  Scenario: Connection Lifetime helps with AWS RDS failover
    Given AWS RDS multi-AZ failover occurs
    And Connection Lifetime is set to 600 seconds (10 minutes)
    When failover completes and DNS updates
    Then connections older than 10 minutes are closed
    And new connections are established to new primary
    And this ensures connections don't stick to old instance
    And failover recovery is faster (< 2 minutes)
