Feature: Unit Tests (Comprehensive Test Coverage with Moq + xUnit)
  As a development team building a high-quality API
  I want comprehensive unit tests with proper mocking and coverage
  So that business logic is validated in isolation with 95%+ code coverage

  Background:
    Given xUnit 2.9.0 test framework is configured
    And Moq 4.20+ mocking framework is configured
    And FluentAssertions 6.12+ is configured for readable assertions
    And Coverlet is configured for code coverage measurement
    And 95% code coverage threshold is enforced
    And tests follow AAA pattern (Arrange-Act-Assert)
    And tests follow FAST principle (Fast, Isolated, Repeatable, Self-validating, Timely)
    And test naming convention: TestMethod_WhatShouldHappen_WhenScenario

  # ============================================================================
  # SCENARIO 1: CRITICAL - MockBehavior.Strict Enforces Explicit Setup
  # ============================================================================
  Scenario: MockBehavior.Strict fails test if method not explicitly configured
    Given I create a mock repository with MockBehavior.Strict
    And I do NOT configure FindByPhoneNumberAsync method
    When the test calls repository.FindByPhoneNumberAsync()
    Then the test should throw MockException
    And the error should indicate "invocation failed with mock behavior Strict"
    And this prevents silent failures from unconfigured mocks

  Scenario: MockBehavior.Loose allows unconfigured methods (anti-pattern)
    Given I create a mock repository with MockBehavior.Loose
    And I do NOT configure FindByPhoneNumberAsync method
    When the test calls repository.FindByPhoneNumberAsync()
    Then the method returns null by default (silent failure)
    And the test may pass incorrectly
    And this is an ANTI-PATTERN that hides bugs

  # ============================================================================
  # SCENARIO 2: Setup WITHOUT Returns/ReturnsAsync Returns Null
  # ============================================================================
  Scenario: Mock setup without Returns causes NullReferenceException
    Given I configure mock with Setup() but forget ReturnsAsync()
    When the test calls the mocked async method
    Then the method returns null (default Task<T> behavior)
    And the consuming code throws NullReferenceException
    And the test fails with confusing error message

  Scenario: Correct mock setup with ReturnsAsync returns expected value
    Given I configure mock: Setup(...).ReturnsAsync(expectedRecord)
    When the test calls the mocked async method
    Then the method returns the expected EquifaxRecord
    And the test passes correctly
    And no null reference errors occur

  # ============================================================================
  # SCENARIO 3: Times.AtLeastOnce vs Times.Once (Precision Matters)
  # ============================================================================
  Scenario: Times.AtLeastOnce allows extra calls (weak assertion)
    Given I verify mock was called Times.AtLeastOnce
    And the method is called 5 times during the test
    When I run the verification
    Then the verification passes
    And this is too permissive (doesn't catch unexpected extra calls)

  Scenario: Times.Once catches unexpected extra calls (strict assertion)
    Given I verify mock was called Times.Once
    And the method is called 2 times during the test
    When I run the verification
    Then the verification fails
    And the error indicates "Expected invocation on the mock once, but was 2 times"
    And this catches logic bugs where methods are called too many times

  # ============================================================================
  # SCENARIO 4: One Test = One Assertion (Single Responsibility Principle)
  # ============================================================================
  Scenario: Testing multiple behaviors in one test (anti-pattern)
    Given a test named "EnrichByPhone_ValidPhone_WorksCorrectly"
    And the test asserts cache is checked
    And the test asserts repository is called
    And the test asserts result is cached
    And the test asserts correct record is returned
    When one assertion fails
    Then the test stops at first failure
    And remaining assertions are not validated
    And this violates Single Responsibility Principle

  Scenario: One test per behavior (correct pattern)
    Given separate tests exist:
      | test_name                                      | assertion                  |
      | EnrichByPhone_CacheMiss_QueriesRepository      | Repository called once     |
      | EnrichByPhone_CacheMiss_CachesResult           | Cache.Set called once      |
      | EnrichByPhone_ValidPhone_ReturnsRecord         | Correct record returned    |
      | EnrichByPhone_CacheHit_SkipsRepository         | Repository never called    |
    When any test fails
    Then only that specific behavior is flagged
    And other behaviors are validated independently
    And this follows Single Responsibility Principle

  # ============================================================================
  # SCENARIO 5: Mock Setup MUST Be in Arrange Section (Before Act)
  # ============================================================================
  Scenario: Configuring mock after Act causes test to fail incorrectly
    Given I have Arrange section with no mock setup
    And I have Act section that calls service
    And I have Assert section that sets up mock behavior
    When the test runs
    Then the Act section receives unconfigured mock (returns null)
    And the test fails with NullReferenceException
    And this is developer error (mock setup in wrong place)

  Scenario: Mock setup in Arrange section before Act (correct pattern)
    Given I configure all mock behaviors in Arrange section
    When Act section calls the service
    Then the service receives configured mock responses
    And the test runs correctly
    And this follows AAA pattern (Arrange-Act-Assert)

  # ============================================================================
  # SCENARIO 6: Mock Setup Must Match Actual Parameters
  # ============================================================================
  Scenario: Mock configured for wrong parameters (setup not matched)
    Given I configure mock: Setup(r => r.FindByPhoneNumberAsync("5551234567"))
    When the test calls: repository.FindByPhoneNumberAsync("5559999999")
    Then the parameters do not match the setup
    And the mock returns null (setup not matched)
    And the test fails with NullReferenceException
    And this is a common mistake with literal parameter values

  Scenario: Use It.IsAny for flexible parameter matching (correct pattern)
    Given I configure mock: Setup(r => r.FindByPhoneNumberAsync(It.IsAny<string>()))
    When the test calls repository with ANY phone number
    Then the setup matches all calls
    And the mock returns configured value
    And the test passes correctly
    And this is more flexible and maintainable

  # ============================================================================
  # SCENARIO 7: Clearing Mock Invocations Between Tests
  # ============================================================================
  Scenario: Shared mock accumulates invocations across tests (flaky tests)
    Given I have a shared mock: private readonly Mock<IRepo> _mock = new Mock<IRepo>()
    And Test1 calls repository.FindByPhoneNumberAsync() once
    And Test1 verifies Times.Once (passes)
    When Test2 runs and verifies repository was NEVER called
    Then the verification fails (mock still has Test1 invocation)
    And this causes flaky tests depending on execution order

  Scenario: Create new mock per test in constructor (correct pattern)
    Given I create new mocks in test class constructor
    And xUnit creates new test class instance per test
    When each test runs
    Then each test gets fresh mocks with no invocations
    And no shared state exists between tests
    And this prevents flaky tests

  # ============================================================================
  # SCENARIO 8: xUnit Parallel Execution + Shared State = Race Conditions
  # ============================================================================
  Scenario: Shared static state with parallel execution causes race conditions
    Given xUnit runs tests in parallel by default
    And multiple tests access shared static field: private static int _counter = 0
    When Test1 and Test2 both increment _counter simultaneously
    Then race condition occurs (lost updates)
    And tests produce non-deterministic results (flaky tests)
    And this is a CRITICAL anti-pattern

  Scenario: No shared state with parallel execution (correct pattern)
    Given each test uses local variables only
    And no static fields or singletons are modified
    When xUnit runs tests in parallel
    Then no race conditions occur
    And all tests pass reliably
    And this enables fast parallel test execution

  Scenario: Disable parallelization for tests requiring sequential execution
    Given some tests MUST run sequentially (e.g., database integration tests)
    When I add [Collection("Serial")] attribute to test class
    Then all tests in that collection run sequentially
    And no parallel execution race conditions occur
    And this is acceptable when sequential execution is required

  # ============================================================================
  # SCENARIO 9: Comprehensive Edge Case Testing
  # ============================================================================
  Scenario Outline: Test edge cases for phone number validation
    Given I have a phone number normalization service
    When I normalize phone number "<input>"
    Then the result should match "<expected_behavior>"
    Examples:
      | input                      | expected_behavior               |
      | null                       | ArgumentNullException           |
      | ""                         | ArgumentException               |
      | "   "                      | ArgumentException               |
      | "abc"                      | ValidationException             |
      | "123"                      | ValidationException (too short) |
      | "12345678901234567890"     | ValidationException (too long)  |
      | "(555) 123-4567"           | Success: "5551234567"           |
      | "+1 555 123 4567"          | Success: "5551234567"           |

  Scenario: Test boundary values for rate limiting
    Given rate limit is 1000 requests per minute
    When I test with request counts:
      | request_count | expected_result |
      | 0             | Allowed         |
      | 1             | Allowed         |
      | 999           | Allowed         |
      | 1000          | Allowed         |
      | 1001          | Rate limited    |
      | 10000         | Rate limited    |
    Then all boundary values should be tested
    And off-by-one errors should be caught

  # ============================================================================
  # SCENARIO 10: Code Coverage Measurement (95% Threshold)
  # ============================================================================
  Scenario: Measure code coverage with Coverlet
    Given Coverlet is configured in test project
    When I run: dotnet test --collect:"XPlat Code Coverage"
    Then coverage report should be generated: coverage.cobertura.xml
    And the report should show line coverage percentage
    And the report should show branch coverage percentage
    And the report should identify untested code paths

  Scenario: Enforce 95% code coverage threshold
    Given coverlet.runsettings configures Threshold=95
    And ThresholdType=line,branch
    When I run tests with coverage enforcement
    And line coverage is 94% (below threshold)
    Then the build should FAIL
    And the error should indicate "Line coverage (94%) is below threshold (95%)"
    And this prevents coverage regressions

  Scenario: Generate HTML coverage report for visualization
    Given code coverage data is collected
    When I run: reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport"
    Then HTML coverage report should be generated
    And the report should show per-file coverage breakdown
    And the report should highlight uncovered lines in red
    And the report should be viewable in browser: coveragereport/index.html

  # ============================================================================
  # SCENARIO 11: Test Naming Conventions
  # ============================================================================
  Scenario: Follow consistent test naming convention
    Given test naming pattern: TestMethod_WhatShouldHappen_WhenScenario
    Then test names should clearly communicate intent:
      | correct_name                                       | communicates                              |
      | EnrichByPhone_ValidPhone_ReturnsRecord             | What happens with valid phone             |
      | EnrichByPhone_CacheHit_SkipsRepository             | Cache hit behavior                        |
      | EnrichByPhone_NullPhone_ThrowsArgumentException    | Null input handling                       |
      | EnrichByPhone_RepositoryThrows_PropagatesException | Exception propagation behavior            |
    And test names should be readable without looking at test body

  # ============================================================================
  # SCENARIO 12: AAA Pattern (Arrange-Act-Assert)
  # ============================================================================
  Scenario: Follow AAA pattern for test structure
    Given a unit test is structured correctly
    Then the test should have three clear sections:
      | section | purpose                           | example                                |
      | Arrange | Setup test data and mocks         | var record = new EquifaxRecord {...}   |
      | Act     | Execute the method being tested   | var result = await service.EnrichAsync |
      | Assert  | Verify expected behavior          | result.Should().NotBeNull()            |
    And sections should be separated by blank lines
    And sections should be commented for clarity

  # ============================================================================
  # SCENARIO 13: Testing Async Methods Correctly
  # ============================================================================
  Scenario: Test async method with proper await
    Given I have an async service method: EnrichByPhoneAsync
    When I test the method
    Then the test method should be async: public async Task Test()
    And the service call should be awaited: await service.EnrichByPhoneAsync()
    And assertions should await async results: await act.Should().ThrowAsync()
    And this prevents test completing before async operation finishes

  Scenario: Verify async mock was called (ReturnsAsync)
    Given I configure mock with ReturnsAsync(expectedRecord)
    When the test calls the async method
    Then I should verify the mock was called: _mockRepo.Verify()
    And the verification should specify Times.Once
    And this confirms async behavior was executed

  # ============================================================================
  # SCENARIO 14: Testing Exception Handling
  # ============================================================================
  Scenario: Test expected exceptions with FluentAssertions
    Given a service method should throw ArgumentException for null input
    When I test the exception behavior
    Then I should use FluentAssertions pattern:
      """
      Func<Task> act = async () => await service.EnrichByPhoneAsync(null);
      await act.Should().ThrowAsync<ArgumentException>()
          .WithMessage("*phone number*");
      """
    And this provides clear assertion syntax
    And this validates exception message content

  Scenario: Test exception propagation from dependencies
    Given repository throws InvalidOperationException("Database error")
    When service calls repository
    Then the exception should propagate to caller
    And the test should verify exception type and message
    And the test should verify error handling behavior

  # ============================================================================
  # SCENARIO 15: Behavior Verification vs State Verification
  # ============================================================================
  Scenario: Verify behavior (method was called) with Moq.Verify
    Given I want to test that cache.CacheRecordAsync was called
    When the service completes successfully
    Then I should use behavior verification:
      """
      _mockCache.Verify(
          c => c.CacheRecordAsync("5551234567", record, It.IsAny<CancellationToken>()),
          Times.Once);
      """
    And this confirms side effects occurred
    And this tests interactions between components

  Scenario: Verify state (return value) with FluentAssertions
    Given I want to test that service returns correct record
    When the service completes successfully
    Then I should use state verification:
      """
      var result = await service.EnrichByPhoneAsync("5551234567");
      result.Should().NotBeNull();
      result.ConsumerKey.Should().Be("ABC123");
      """
    And this validates return values
    And this tests output correctness

  # ============================================================================
  # SCENARIO 16: Test Isolation (Each Test Independent)
  # ============================================================================
  Scenario: Tests should not depend on execution order
    Given I have 3 unit tests: Test1, Test2, Test3
    When tests run in different orders:
      | execution_order      | all_tests_pass |
      | Test1, Test2, Test3  | true           |
      | Test3, Test2, Test1  | true           |
      | Test2, Test1, Test3  | true           |
    Then all tests should pass in all orders
    And no test should depend on state from previous test
    And this ensures test isolation

  # ============================================================================
  # SCENARIO 17: Fast Test Execution Performance
  # ============================================================================
  Scenario: Unit tests should execute quickly (< 10ms per test)
    Given unit tests use mocks (no I/O operations)
    When I run a single unit test
    Then the test should complete in less than 10ms
    And no database connections should be opened
    And no file system operations should occur
    And this enables fast feedback during development

  Scenario: Full unit test suite should complete quickly (< 2 minutes)
    Given the project has 500+ unit tests
    When I run: dotnet test --filter "Category=Unit"
    Then the full test suite should complete in less than 2 minutes
    And xUnit parallel execution should be enabled
    And this enables rapid CI/CD pipelines

  # ============================================================================
  # SCENARIO 18: Test Coverage Targets by Component
  # ============================================================================
  Scenario: Enforce component-specific coverage targets
    Given coverage targets are defined per component
    Then each component should meet its coverage target:
      | component                 | coverage_target | rationale                    |
      | PhoneEnrichmentService    | 95%             | Business logic               |
      | EquifaxRepository         | 90%             | Data access                  |
      | PIIDecryptionService      | 100%            | Security-critical            |
      | PhoneNumberNormalizer     | 100%            | Validation logic             |
      | ApiKeyAuthMiddleware      | 100%            | Security-critical            |
      | RateLimitingMiddleware    | 95%             | Business rules               |
    And coverage should be measured with Coverlet
    And failing to meet target should block CI/CD pipeline

  # ============================================================================
  # SCENARIO 19: Testing with CancellationToken
  # ============================================================================
  Scenario: Test cancellation token propagation
    Given all async methods accept CancellationToken parameter
    When I test the service method
    Then I should verify CancellationToken is propagated to dependencies:
      """
      await service.EnrichByPhoneAsync("5551234567", cancellationToken);

      _mockRepo.Verify(
          r => r.FindByPhoneNumberAsync(It.IsAny<string>(), cancellationToken),
          Times.Once);
      """
    And this confirms cancellation support is implemented

  Scenario: Test cancellation handling
    Given a long-running operation is in progress
    When cancellation token is cancelled
    Then the operation should throw OperationCanceledException
    And the test should verify exception type
    And this confirms graceful cancellation behavior

  # ============================================================================
  # SCENARIO 20: Mock Callbacks for Complex Scenarios
  # ============================================================================
  Scenario: Use mock callback to simulate complex behavior
    Given I need to test retry logic with multiple responses
    When I configure mock with callback:
      """
      var callCount = 0;
      _mockRepo.Setup(r => r.FindByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(() =>
          {
              callCount++;
              if (callCount == 1) throw new TimeoutException();
              return expectedRecord;
          });
      """
    Then the mock returns different values on subsequent calls
    And this enables testing complex retry/fallback logic

  # ============================================================================
  # SCENARIO 21: Testing Logging Behavior
  # ============================================================================
  Scenario: Verify logging calls with mock ILogger
    Given I have a mock logger: Mock<ILogger<PhoneEnrichmentService>>
    When the service logs an error
    Then I should verify logging occurred:
      """
      _mockLogger.Verify(
          x => x.Log(
              LogLevel.Error,
              It.IsAny<EventId>(),
              It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Database error")),
              It.IsAny<Exception>(),
              It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
      """
    And this confirms error logging behavior

  # ============================================================================
  # SCENARIO 22: Test Data Builders for Readability
  # ============================================================================
  Scenario: Use test data builders for complex objects
    Given I need to create EquifaxRecord with 398 columns
    When I create a test data builder:
      """
      public class EquifaxRecordBuilder
      {
          public EquifaxRecordBuilder WithPhone(string phone) { ... }
          public EquifaxRecordBuilder WithName(string first, string last) { ... }
          public EquifaxRecord Build() { ... }
      }
      """
    Then tests become more readable:
      """
      var record = new EquifaxRecordBuilder()
          .WithPhone("5551234567")
          .WithName("John", "Doe")
          .Build();
      """
    And this reduces test setup boilerplate

  # ============================================================================
  # SCENARIO 23: Test Framework Setup (xUnit Configuration)
  # ============================================================================
  Scenario: Configure xUnit test project correctly
    Given xUnit test project is created
    Then the .csproj should include:
      | package                              | version | purpose                      |
      | xunit                                | 2.9.0   | Test framework               |
      | xunit.runner.visualstudio            | 2.8.2   | Visual Studio integration    |
      | Microsoft.NET.Test.Sdk               | 17.11.0 | Test SDK                     |
      | Moq                                  | 4.20.70 | Mocking framework            |
      | FluentAssertions                     | 6.12.1  | Readable assertions          |
      | coverlet.collector                   | 6.0.2   | Code coverage                |
    And TargetFramework should be net9.0
    And IsPackable should be false

  # ============================================================================
  # SCENARIO 24: Running Tests (Command-Line Operations)
  # ============================================================================
  Scenario: Run all unit tests with coverage
    Given unit tests are implemented
    When I run: dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
    Then all unit tests should execute
    And code coverage should be measured
    And coverage report should be generated
    And build should fail if coverage < 95%

  Scenario: Run tests in watch mode for rapid development
    Given I am actively developing code
    When I run: dotnet watch test
    Then tests should run automatically on file changes
    And this provides instant feedback
    And this enables test-driven development workflow

  Scenario: Run specific test class or method
    Given I want to run specific tests
    When I run: dotnet test --filter "FullyQualifiedName~PhoneEnrichmentServiceTests"
    Then only matching tests should execute
    And this enables focused test debugging
    And this speeds up test iteration during development

  # ============================================================================
  # SCENARIO 25: Test Documentation and Maintainability
  # ============================================================================
  Scenario: Document complex test scenarios with comments
    Given a test has complex setup or non-obvious behavior
    When writing the test
    Then I should add comments explaining:
      | comment_purpose           | example                                      |
      | Why this test exists      | // Test for CVE-2025-59425 timing attack    |
      | Complex mock setup reason | // Returns null on first call, record on 2nd|
      | Business rule being tested| // FCRA requires 72-hour notification        |
    And this helps future maintainers understand test intent

  Scenario: Keep tests maintainable with DRY principle
    Given multiple tests share common setup
    When I extract shared setup to helper methods or test fixtures
    Then tests become more maintainable
    And changes to setup only need to be made once
    And this follows Don't Repeat Yourself principle
