I'll generate the dispatch prompt for the code-reviewer agent with .NET-specific quality criteria.

Here's the dispatch prompt for the code-reviewer agent:

```
Task (superpowers:code-reviewer):
  Use template at requesting-code-review/code-reviewer.md

  WHAT_WAS_IMPLEMENTED: CreateOrderCommandHandler with validation (customer exists, products exist, quantities > 0, max 100 items). Added 12 xUnit tests covering happy path, validation failures, 404 cases.

  PLAN_OR_REQUIREMENTS: Task 2 from _docs/plans/orders-feature.md

  BASE_SHA: abc123def
  HEAD_SHA: 456ghi789

  DESCRIPTION: CreateOrder feature implementation with command handler and integration tests

  ADDITIONAL_REVIEW_CRITERIA: |
    ## .NET Test Quality Review Checklist

    Review ALL test files in the diff. For each test, apply these checks:

    ### Behavioral Testing
    - [ ] No getter/setter tests (flag: `CanSetProperties`, `CanSetName`, `HasInitializedCollections`, `IsNullable`, `HasEmptyDefault`)
    - [ ] Every test catches a real bug — ask "what bug does this prevent?" If answer is "none", flag it
    - [ ] Tests verify outcomes (returned data, DB state, domain state), not mock interactions (`Received()`, `Verify()`)
    - [ ] No assertion-less tests (every test has at least one Assert)
    - [ ] Integration tests assert response body or DB state, not just HTTP status code

    ### Test Structure
    - [ ] Max 3 assertions per test (single logical assertion OK even if multiple Assert calls)
    - [ ] Naming follows `MethodName_Scenario_ExpectedBehavior`
    - [ ] `[Theory]`+`[InlineData]` used when testing same behavior with multiple inputs
    - [ ] No "and" in test names (split into separate tests)

    ### Test Infrastructure
    - [ ] Shared `CustomWebApplicationFactory` used for integration tests (not inline `WebApplicationFactory<Program>`)
    - [ ] `IClassFixture<CustomWebApplicationFactory>` pattern (not new factory per test)
    - [ ] Setup helpers in shared `TestHelpers` class (not duplicated across test files)
    - [ ] No test-only methods added to production classes
    - [ ] SQLite in-memory with kept-alive connection (NOT EF InMemory provider)

    ### Mock Boundaries
    - [ ] Infrastructure mocked: DbContext, IHttpClientFactory, ILogger, TimeProvider, external APIs
    - [ ] Domain NOT mocked: entities, value objects, pure functions use real instances
    - [ ] NSubstitute used (not Moq, not FakeItEasy) — project convention, adjust if your project differs
    - [ ] Mock setup is <50% of test code

    ### EF Core Patterns
    - [ ] No N+1 queries — eager load with `.Include()` or use projections (`.Select()`) instead of lazy-loading in loops
    - [ ] Read-only queries use `.AsNoTracking()` to avoid unnecessary change tracking overhead
    - [ ] DbContext lifetime is scoped (not singleton/transient) — verify DI registration
    - [ ] Migrations are clean — no empty migrations, no manual SQL unless justified, migration order matches commit history
    - [ ] No business logic in migrations — data transforms belong in domain services or one-time scripts
    - [ ] Queries use server-side evaluation — flag `.ToList()` before `.Where()` or LINQ that forces client evaluation

    ### Coverage
    - [ ] Edge cases tested (null, empty, boundary values)
    - [ ] Error paths tested (invalid input, not found, unauthorized)
    - [ ] All new public methods have at least one test
    - [ ] Happy path + at least one failure path per endpoint

    ### Anti-Patterns to Flag
    Flag these by name in review:
    - **CanSetProperties**: Test that only verifies property assignment
    - **IsNullable**: Test that only checks default null values
    - **HasEmptyDefault**: Test that only checks empty collection initialization
    - **MockVerifyOnly**: Test where only assertion is `Received()` or `Verify()`
    - **DuplicateFactory**: `new WebApplicationFactory<Program>()` instead of shared fixture
    - **AssertionOverload**: Test with >3 assertions testing unrelated things (Important); >5 unrelated assertions is Critical
    - **DomainMock**: `Substitute.For<DomainEntity>()` on domain objects
    - **Assertionless**: Test with zero assertions — passes silently, catches nothing
    - **StatusCodeOnly**: Integration test that only checks `EnsureSuccessStatusCode()` or status code without verifying body/DB

    ### Severity Guide
    - **Critical**: getter/setter tests, domain mocking, no tests for new methods, >5 unrelated assertions, assertion-less tests
    - **Important**: missing Theory, >3 unrelated assertions, duplicate factory, mock-verify-only, status-code-only integration tests
    - **Minor**: naming convention, missing edge case test
```

**Notes for dispatch:**

1. **SHAs**: You provided `abc123def` (base) and `456ghi789` (head). If these are placeholders, replace with actual commit SHAs from `git log`.
2. **Superpowers plugin required**: Ensure `superpowers` is available in your Claude Code environment. The code-reviewer agent loads the base template and applies the .NET test quality criteria above.
3. **12 tests**: The checklist will inspect all 12 tests in CreateOrderCommandHandlerTests for behavioral coverage, structure, mocks, and anti-patterns.
4. **Key focus areas for this task**:
   - Validation tests (customer exists, products exist, quantities > 0, max 100 items) should verify real outcomes, not just mock calls
   - 404 cases should assert response body/status, not mock interactions
   - Happy path should verify order was persisted to DB, not just handler return value
   - No domain mocking (Order, Customer, Product entities should be real, not Substitute.For)

Paste this dispatch into your Task tool invocation or send to the subagent context.