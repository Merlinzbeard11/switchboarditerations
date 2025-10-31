Feature: Integration Tests (End-to-End Testing with Real Database)
  As a development team building a production-ready API
  I want comprehensive integration tests with WebApplicationFactory and real database
  So that API endpoints, database interactions, and middleware are validated end-to-end

  Background:
    Given Microsoft.AspNetCore.Mvc.Testing 9.0+ is configured
    And xUnit 2.9.0 test framework is configured
    And FluentAssertions 6.12+ is configured
    And real PostgreSQL test database is configured (not in-memory)
    And WebApplicationFactory<Program> is configured
    And CustomWebApplicationFactory overrides test services
    And transaction rollback is used for test isolation (4x faster than database recreation)
    And performance target is < 100ms per test with transaction rollback

  # ============================================================================
  # SCENARIO 1: CRITICAL - Program Class Visibility (.NET 6+ Breaking Change)
  # ============================================================================
  Scenario: .NET 6+ generates Program class as internal (inaccessible to tests)
    Given I am using .NET 6+ with minimal hosting model
    And the test project references API project
    When I create WebApplicationFactory<Program>
    Then the compiler error should occur: "Program is inaccessible due to its protection level"
    And this is the #1 cause of integration test failures in .NET 6+
    And Program class is auto-generated as internal by default

  Scenario: Add public partial class Program to make it accessible to tests
    Given I add to bottom of Program.cs:
      """
      var app = builder.Build();
      app.Run();

      // ✅ REQUIRED for integration tests (.NET 6+)
      public partial class Program { }
      """
    When I create WebApplicationFactory<Program> in test project
    Then the Program class should be accessible
    And integration tests should compile successfully
    And this is REQUIRED for all .NET 6+ integration tests

  # ============================================================================
  # SCENARIO 2: Using EF InMemory Database Provider (Anti-Pattern)
  # ============================================================================
  Scenario: EF Core InMemory provider is NOT a real relational database
    Given I use EF Core InMemory provider for testing
    When I test database constraints (foreign keys, unique constraints)
    Then the constraints are NOT enforced (InMemory is not relational)
    And SQL dialect differences are NOT caught (PostgreSQL-specific queries pass in tests, fail in prod)
    And this is slower than SQLite in-memory mode
    And Microsoft recommends AGAINST using InMemory for integration tests

  Scenario: Use real PostgreSQL test database for accurate testing
    Given I configure real PostgreSQL test database
    When I test database constraints and SQL queries
    Then foreign key constraints should be enforced
    And unique constraints should be enforced
    And PostgreSQL-specific SQL syntax should be validated
    And this catches production database bugs during testing
    And this is the BEST approach for integration tests

  Scenario: SQLite in-memory is better than EF InMemory (if real DB not feasible)
    Given I cannot use real PostgreSQL test database
    When I configure SQLite in-memory mode:
      """
      services.AddDbContext<ApplicationDbContext>(options =>
          options.UseSqlite("DataSource=:memory:"));
      """
    Then SQLite should enforce relational constraints
    And SQLite should be faster than EF InMemory
    And this is BETTER than EF InMemory provider (but not as good as real PostgreSQL)

  # ============================================================================
  # SCENARIO 3: Database Recreation = Slow Tests (4x Slower)
  # ============================================================================
  Scenario: Dropping and recreating database between tests is catastrophically slow
    Given I recreate database for each test
    When I run 700 integration tests
    Then database recreation takes 400ms per test
    And total overhead is 4.7 minutes wasted on database setup
    And tests are 4x slower than transaction rollback approach
    And this makes CI/CD pipelines painfully slow

  Scenario: Transaction rollback is 4x faster than database recreation
    Given I wrap each test in database transaction
    When the test completes (success or failure)
    Then I rollback the transaction
    And all test data changes are instantly reverted
    And no database recreation is needed
    And test performance improves from 100ms to 25ms per test
    And this is the BEST practice for integration test isolation

  Scenario: Implement IAsyncLifetime for automatic transaction rollback
    Given I create IntegrationTestBase class implementing IAsyncLifetime
    When InitializeAsync() runs before each test
    Then database transaction should be started
    When DisposeAsync() runs after each test
    Then transaction should be rolled back automatically
    And this pattern ensures test isolation with minimal overhead

  # ============================================================================
  # SCENARIO 4: ConfigureServices vs ConfigureTestServices (Timing Issue)
  # ============================================================================
  Scenario: ConfigureServices runs BEFORE Startup.ConfigureServices (wrong order)
    Given I use factory.WithWebHostBuilder(builder => builder.ConfigureServices(...))
    When the test factory initializes
    Then ConfigureServices runs BEFORE Startup.ConfigureServices
    And production DbContext is already registered
    And test database configuration is ignored
    And tests use production database (CATASTROPHIC)

  Scenario: ConfigureTestServices runs AFTER Startup (correct order)
    Given I use factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(...))
    When the test factory initializes
    Then ConfigureTestServices runs AFTER Startup.ConfigureServices
    And I can remove production DbContext: services.RemoveAll<DbContextOptions<ApplicationDbContext>>()
    And I can add test database configuration
    And tests use test database correctly
    And this is the CORRECT approach for integration tests

  # ============================================================================
  # SCENARIO 5: Not Mocking Authentication (Can't Test Protected Endpoints)
  # ============================================================================
  Scenario: API endpoints require authentication but HttpClient has no token
    Given API endpoints are protected with [Authorize] attribute
    And integration test HttpClient has no authentication
    When I POST to protected endpoint
    Then the response should be 401 Unauthorized
    And I cannot test protected endpoints
    And this blocks testing of most API functionality

  Scenario: Create custom TestAuthHandler for mock authentication
    Given I create TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    And HandleAuthenticateAsync() returns success with test claims
    When I register TestAuthHandler in ConfigureTestServices:
      """
      services.AddAuthentication("Test")
          .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
      """
    Then HttpClient should bypass authentication
    And protected endpoints should be accessible in tests
    And test claims should be available in controllers (User.Claims)
    And this enables testing of protected API endpoints

  # ============================================================================
  # SCENARIO 6: Everything in Program.cs Runs During Tests
  # ============================================================================
  Scenario: WebApplicationFactory executes entire Program.cs (real dependencies initialized)
    Given Program.cs registers real Redis connection
    And Program.cs registers real email service
    And Program.cs registers real payment gateway
    When WebApplicationFactory initializes
    Then all real services are initialized
    And tests may fail if Redis is unavailable
    And tests may send real emails or charge real credit cards
    And this is CATASTROPHIC for test safety

  Scenario: Replace real services with test doubles in ConfigureTestServices
    Given I need to mock external dependencies
    When I configure test services:
      """
      builder.ConfigureTestServices(services =>
      {
          // Remove real services
          services.RemoveAll<IEmailService>();
          services.RemoveAll<IConnectionMultiplexer>(); // Redis

          // Add mock services
          services.AddSingleton<IEmailService, MockEmailService>();
          services.AddSingleton<IConnectionMultiplexer>(_ => Mock.Of<IConnectionMultiplexer>());
      });
      """
    Then tests should use mock services
    And no real external dependencies are called
    And tests are safe and isolated

  # ============================================================================
  # SCENARIO 7: Shared Database State Between Tests (Flaky Tests)
  # ============================================================================
  Scenario: Shared database state causes test failures and race conditions
    Given Test1 inserts data into database
    And Test2 expects empty database
    And xUnit runs tests in parallel by default
    When both tests run simultaneously
    Then Test2 sees data from Test1
    And Test2 fails with unexpected data
    And tests produce non-deterministic results (flaky tests)

  Scenario: Transaction rollback isolates each test (BEST solution)
    Given each test wraps database operations in transaction
    When each test completes
    Then transaction is rolled back
    And all test data changes are reverted
    And next test starts with clean database state
    And tests can run in parallel without conflicts
    And this is the BEST solution for test isolation

  Scenario: Disable parallelization for integration tests (ALTERNATIVE solution)
    Given I need tests to run sequentially
    When I add [Collection("Integration Tests")] attribute to test classes
    Then all tests in that collection run sequentially
    And no parallel execution race conditions occur
    And this is acceptable when transaction rollback is not feasible

  # ============================================================================
  # SCENARIO 8: HttpClient Cookie Handling (Automatic in ASP.NET Core)
  # ============================================================================
  Scenario: HttpClient automatically preserves cookies (no configuration needed)
    Given I have login endpoint that sets authentication cookie
    When I POST to /api/auth/login with credentials
    Then the response should set authentication cookie
    When I GET /api/data with same HttpClient
    Then the cookie should be sent automatically
    And subsequent requests should be authenticated
    And no manual cookie handling is needed
    And this is automatic in ASP.NET Core

  # ============================================================================
  # SCENARIO 9: Not Testing Raw SQL Queries
  # ============================================================================
  Scenario: In-memory database cannot test raw SQL queries
    Given application uses context.Database.ExecuteSqlRaw() for performance
    And tests use EF InMemory database provider
    When I test raw SQL queries
    Then SQL queries are NOT executed (InMemory ignores raw SQL)
    And SQL syntax errors are NOT caught in tests
    And queries fail in production but pass in tests
    And this is a CRITICAL testing gap

  Scenario: Real database catches raw SQL syntax errors
    Given application uses raw SQL queries
    And tests use real PostgreSQL test database
    When I test raw SQL query:
      """
      var results = await _context.Database
          .SqlQueryRaw<EquifaxRecord>(@"
              SELECT * FROM equifax_records
              WHERE phone1 = '5551234567'
              LIMIT 10
          ")
          .ToListAsync();
      """
    Then SQL syntax should be validated
    And PostgreSQL-specific syntax should be validated
    And SQL errors should be caught during testing
    And this prevents SQL failures in production

  # ============================================================================
  # SCENARIO 10: Missing Test Data Cleanup (Database Bloat)
  # ============================================================================
  Scenario: Integration tests insert thousands of rows without cleanup
    Given integration tests insert test data
    And no cleanup strategy is implemented
    When hundreds of tests run over time
    Then test database grows to gigabytes
    And queries become slow (table bloat)
    And disk space runs out
    And tests fail due to performance degradation

  Scenario: Transaction rollback prevents database bloat (BEST solution)
    Given each test wraps operations in transaction
    When each test completes
    Then transaction is rolled back
    And all test data is instantly deleted
    And no database bloat occurs
    And test database remains small and fast
    And this is the BEST solution for data cleanup

  Scenario: Selective table cleanup if transactions not feasible (ALTERNATIVE)
    Given I track test start time: _testStartTime = DateTime.UtcNow
    When test completes
    Then I delete only records created during test:
      """
      await _context.Database.ExecuteSqlRawAsync(
          "DELETE FROM audit_logs WHERE created_at > @p0", _testStartTime);
      """
    And this prevents database bloat
    And this is acceptable when transaction rollback is not feasible

  # ============================================================================
  # SCENARIO 11: End-to-End API Tests (Happy Path)
  # ============================================================================
  Scenario: Test complete API request flow with authentication
    Given I insert test data into database:
      """
      var testRecord = new EquifaxRecord
      {
          ConsumerKey = "TEST123",
          FirstName = "John",
          LastName = "Doe",
          Phone1 = "5551234567"
      };
      Context.EquifaxRecords.Add(testRecord);
      await Context.SaveChangesAsync();
      """
    When I POST to /api/v1/enrich with: { "phone_number": "5551234567" }
    Then the response status should be 200
    And the response should contain ConsumerKey "TEST123"
    And the response should contain FirstName "John"
    And the response time should be less than 150ms
    And FCRA audit log should be created

  # ============================================================================
  # SCENARIO 12: Error Response Testing (4xx, 5xx Status Codes)
  # ============================================================================
  Scenario: Test 404 Not Found when phone number doesn't exist
    Given no record exists for phone number "9999999999"
    When I POST to /api/v1/enrich with: { "phone_number": "9999999999" }
    Then the response status should be 404
    And the error message should indicate "Phone number not found"
    And FCRA audit log should still be created (all queries logged)

  Scenario: Test 400 Bad Request for invalid phone format
    Given I POST to /api/v1/enrich with: { "phone_number": "abc" }
    When the request is processed
    Then the response status should be 400
    And the error message should indicate "Invalid phone number format"
    And the error should reference validation rules

  Scenario: Test 401 Unauthorized without authentication
    Given I create HttpClient without authentication
    When I POST to /api/v1/enrich with valid request
    Then the response status should be 401
    And the error message should indicate "Unauthorized"

  # ============================================================================
  # SCENARIO 13: Rate Limiting Integration Tests
  # ============================================================================
  Scenario: Test rate limiting enforcement (1000 requests/minute)
    Given buyer has rate limit of 1000 requests/minute
    When I make 1000 requests
    Then all requests should succeed (200 or 404)
    When I make 1001st request
    Then the response status should be 429 (Too Many Requests)
    And the response should include X-RateLimit-Limit header
    And the response should include X-RateLimit-Remaining header (0)
    And the response should include Retry-After header

  # ============================================================================
  # SCENARIO 14: Cache Integration Tests
  # ============================================================================
  Scenario: Test cache miss then cache hit scenario
    Given I insert test record for phone "5559876543"
    When I make first request for phone "5559876543"
    Then the response time should be 50-150ms (database query)
    When I make second request for same phone
    Then the response time should be less than 50ms (cache hit)
    And the response data should be identical
    And Redis GET should be called (cache lookup)

  # ============================================================================
  # SCENARIO 15: FCRA Audit Logging Integration Tests
  # ============================================================================
  Scenario: Verify every API request creates audit log entry
    Given audit logs table is empty
    When I POST to /api/v1/enrich with phone "5551112222"
    Then I should wait 100ms for async logging to complete
    And audit logs count should increase by 1
    And latest audit log should contain:
      | field                | expected_value       |
      | buyer_id             | 1 (from test claims) |
      | phone_number_hash    | SHA-256(5551112222)  |
      | permissible_purpose  | marketing            |
      | match_found          | false                |

  # ============================================================================
  # SCENARIO 16: Database Performance Integration Tests
  # ============================================================================
  Scenario: Test database query performance under load
    Given I insert test record for phone "5551231234"
    When I measure response time for 100 consecutive requests
    Then p50 response time should be less than 100ms
    And p95 response time should be less than 150ms
    And p99 response time should be less than 200ms
    And this validates performance SLA

  # ============================================================================
  # SCENARIO 17: Multi-Phone Search Integration Test
  # ============================================================================
  Scenario: Test multi-phone search with confidence scoring
    Given I insert record with phone "5551234567" in phone_3 column
    When I search for phone "5551234567"
    Then the record should be found
    And the matched column should be 3
    And the match confidence should be 0.90 (90%)
    And the confidence formula should be: 100 - ((3 - 1) × 5) = 90%

  # ============================================================================
  # SCENARIO 18: PII Decryption Integration Test
  # ============================================================================
  Scenario: Test PII decryption with valid permissible purpose
    Given I insert encrypted PII record
    And the permissible purpose is "insurance_underwriting" (valid FCRA purpose)
    When I request full fields including PII
    Then PII fields should be decrypted
    And SSN should be returned (masked format: XXX-XX-1234)
    And decryption should complete in less than 5ms

  Scenario: Test PII decryption rejection with invalid permissible purpose
    Given I insert encrypted PII record
    And the permissible purpose is "marketing" (invalid for SSN)
    When I request SSN field
    Then the request should fail with 403 Forbidden
    And the error should reference FCRA § 604 requirements
    And SSN should NOT be returned

  # ============================================================================
  # SCENARIO 19: Health Check Endpoint Integration Test
  # ============================================================================
  Scenario: Test /health endpoint reports system status
    When I GET /health
    Then the response status should be 200
    And the response should contain:
      | field       | expected_value |
      | status      | healthy        |
      | version     | 20251031.1200  |
      | database    | connected      |
      | redis_cache | connected      |

  # ============================================================================
  # SCENARIO 20: Connection Pooling Integration Test
  # ============================================================================
  Scenario: Test database connection pooling under concurrent load
    Given I make 100 concurrent API requests
    When all requests complete
    Then no connection pool exhaustion should occur
    And no connection timeout errors should occur
    And all requests should succeed or return valid errors
    And connection pool should be properly managed

  # ============================================================================
  # SCENARIO 21: Test Factory Setup and Configuration
  # ============================================================================
  Scenario: CustomWebApplicationFactory should configure test environment
    Given I create CustomWebApplicationFactory
    Then the factory should:
      | configuration                        | purpose                              |
      | Remove production DbContext          | Prevent using production database    |
      | Add test PostgreSQL DbContext        | Use test database                    |
      | Add TestAuthHandler                  | Bypass authentication                |
      | Remove real Redis connection         | Use mock Redis                       |
      | Set environment to "Test"            | Load test configuration              |
    And this ensures safe, isolated testing

  # ============================================================================
  # SCENARIO 22: Test Database Setup and Teardown
  # ============================================================================
  Scenario: Create test database before running integration tests
    Given PostgreSQL is running
    When I run: createdb enrichment_test
    Then test database should be created
    When I run: dotnet ef database update --connection "Host=localhost;Database=enrichment_test"
    Then test database schema should be applied
    And test database is ready for integration tests

  # ============================================================================
  # SCENARIO 23: Running Integration Tests
  # ============================================================================
  Scenario: Run integration tests with category filter
    Given integration tests are tagged with [Category("Integration")]
    When I run: dotnet test --filter "Category=Integration"
    Then only integration tests should execute
    And unit tests should be skipped
    And this enables selective test execution

  Scenario: Run integration tests with detailed logging
    When I run: dotnet test --filter "Category=Integration" --logger "console;verbosity=detailed"
    Then detailed test output should be displayed
    And HTTP requests/responses should be logged
    And database queries should be logged
    And this helps with integration test debugging

  # ============================================================================
  # SCENARIO 24: Integration Test Performance Targets
  # ============================================================================
  Scenario: Enforce integration test performance targets
    Given integration tests use transaction rollback
    Then per-test performance should be:
      | test_type                | target_time |
      | Simple API call          | 25-50ms     |
      | API call with database   | 50-100ms    |
      | API call with cache      | 25-75ms     |
      | Full suite (50 tests)    | < 5 seconds |
    And transaction rollback should be 4x faster than database recreation
    And tests should run in parallel (when safe)

  # ============================================================================
  # SCENARIO 25: Test Data Management Best Practices
  # ============================================================================
  Scenario: Use realistic test data that mirrors production
    Given I need test data for integration tests
    Then I should create:
      | data_type           | example                                  |
      | Valid phone numbers | "5551234567"                             |
      | Valid names         | "John Doe"                               |
      | Valid addresses     | "123 Main St, Salt Lake City, UT 84101" |
      | Valid dates         | "1980-01-15"                             |
    And test data should match production data formats
    And test data should cover common and edge cases
    And test data should be maintained in test fixtures
