---
name: dotnet-tdd
description: Use when implementing ASP.NET Core + EF Core features or bugfixes with xUnit + NSubstitute tests, before writing implementation code.
---

# .NET TDD — Enforcement Protocol

Mandatory TDD constraints for all .NET code. Violations must be fixed before committing.

## The Iron Laws

```
1. NO PRODUCTION CODE WITHOUT A FAILING TEST FIRST
2. NEVER add test-only methods to production classes
3. MOCK INFRASTRUCTURE ONLY — never domain
4. COMMIT after each green cycle
```

Write code before test? Delete it. `git checkout .` — not stash, not "reference."

## Red-Green-Refactor

| Step | Action | Verify |
|------|--------|--------|
| RED | Write one failing test | Fails for expected reason (missing feature, not typo) |
| GREEN | Write minimal code to pass | This test + all others pass |
| REFACTOR | Clean up, no new behavior | All tests still green |
| COMMIT | After each green cycle | `git commit` with passing tests |

## Mock Boundary Rule

**Mock infrastructure with NSubstitute. NEVER mock domain.**

| OK to mock (NSubstitute) | NEVER mock |
|--------------------------|------------|
| `IRepository`, `DbContext` | Domain entities |
| `IHttpClientFactory` | Value objects |
| `ILogger<T>`, `TimeProvider` | Pure functions |
| External API clients, message queues | Domain services with no I/O |

```csharp
// OK: mocking infrastructure
var repo = Substitute.For<IOrderRepository>();
repo.GetByIdAsync(orderId).Returns(existingOrder);

// VIOLATION
var order = Substitute.For<Order>();  // Never mock domain entities
var order = Order.Create(customerId); // Use real entity
```

`Substitute.For<>()` on a domain entity/value object is always a violation. Use NSubstitute only — not Moq, not FakeItEasy.

## What to Test

Test behavior, not structure. Ask: "If this test didn't exist, what bug could ship?"

**DO test:** Public method behavior, edge cases, error paths, state transitions, integration (HTTP → response → DB state).

**NEVER test:** Getters/setters (`CanSetName`), constructor property assignment, framework behavior, private methods.

**Integration test rule:** Assert response body AND/OR DB state — not just HTTP status. `response.EnsureSuccessStatusCode()` alone is a violation.

## Test Structure Rules

- **Naming:** `MethodName_Scenario_ExpectedBehavior` (e.g., `AddItem_WithEmptyName_ReturnsFailure`)
- **Max 3 assertions** per test. Use `[Theory]` + `[InlineData]` for parameterized cases.
- **Zero-assertion test** is always a violation.
- **One `CustomWebApplicationFactory`** shared via `IClassFixture<>` — never duplicate per test class.

## CustomWebApplicationFactory (copy once)

**Check what the project uses, then pick the matching pattern.**

### SQLite projects (greenfield default)

Swap file-based → in-memory SQLite. Same SQL dialect, zero mismatch, no Docker needed. In-memory SQLite dies when the connection closes, so the connection MUST be kept alive.

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(o => o.UseSqlite(_connection));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing) { _connection.Dispose(); base.Dispose(disposing); }
}
```

### PostgreSQL projects (Testcontainers)

Test against real PostgreSQL in Docker. Zero SQL dialect mismatch. Requires `Testcontainers.PostgreSql` NuGet.

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(o => o.UseNpgsql(_postgres.GetConnectionString()));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        });
    }

    public async Task InitializeAsync() => await _postgres.StartAsync();
    public async Task DisposeAsync() => await _postgres.DisposeAsync();
}
```

### Usage (same for both)

```csharp
public class OrderTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    public OrderTests(CustomWebApplicationFactory factory) => _client = factory.CreateClient();
}
```

**EF InMemory provider is a violation** (doesn't enforce constraints, FK, or SQL behavior). Always use real database engine matching production.

## Test Project Structure

```
tests/{Project}.Tests/
  Unit/              # Domain + application service tests
  Integration/       # HTTP endpoint tests via CustomWebApplicationFactory
  Infrastructure/    # Shared fixtures (CustomWebApplicationFactory)
```

## Critical Anti-Patterns (auto-fail)

| Anti-Pattern | What it looks like | Fix |
|-------------|-------------------|-----|
| **MockVerifyOnly** | `repo.Received(1).AddAsync(Arg.Any<Order>())` | Assert on response body, DB state, or domain state |
| **GetterSetterTest** | `item.Name = "X"; Assert.Equal("X", item.Name)` | Test behavior that uses the property |
| **DomainMock** | `Substitute.For<Order>()` | Use real domain entity |
| **TestOnlyMethod** | `public void ResetState()` only used in tests | Fresh instances + test helpers |
| **IncompleteMock** | `new StoreDto { Id = id, Name = "Test" }` missing fields | Include ALL fields with realistic values |
| **DuplicateFactory** | Copy-pasted `WebApplicationFactory` setup per class | Single `CustomWebApplicationFactory` + `IClassFixture` |
| **StatusOnlyAssert** | `response.EnsureSuccessStatusCode()` without body check | Assert response body AND/OR DB state |

## Pre-Commit Verification

```bash
# 1. Tests exist
ls tests/ 2>/dev/null | grep -q "." || echo "FAIL: no test project"

# 2. No Substitute.For on domain entities
grep -rn "Substitute.For<" tests/ --include="*.cs" | grep -iv "Repository\|IHttp\|ILogger\|IEmail\|IMessage\|INotif\|ICache\|IFile\|TimeProvider\|IConfiguration\|IService\|IMediator\|IDispatcher" && echo "WARN: check if mocking domain"

# 3. No getter/setter tests
grep -rn "CanSet\|CanGet\|HasDefault\|IsNullable" tests/ --include="*.cs" && echo "FAIL: getter/setter tests"

# 4. No test-only methods in production
grep -rn "ResetState\|ResetForTest\|SetForTest\|TestOnly" Domain/ Application/ --include="*.cs" 2>/dev/null && echo "FAIL: test-only methods in production"

# 5. No assertion-less tests
grep -B10 "^\s*}" tests/ -r --include="*.cs" | grep -B10 "\[Fact\]\|\[Theory\]" | grep -L "Assert\.\|\.Should\|Expect\|Verify" 2>/dev/null | head -3

# 6. Integration tests assert body/DB, not just status
grep -A5 "EnsureSuccessStatusCode\|StatusCode" tests/ -r --include="*.cs" | grep -v "Assert\.\|ReadFromJson\|Content\.\|Should" | head -3 && echo "WARN: check status-only assertions"

# 7. One shared factory
find tests/ -name "*.cs" -exec grep -l "WebApplicationFactory" {} \; 2>/dev/null | wc -l | xargs -I{} test {} -gt 1 && echo "FAIL: multiple WebApplicationFactory definitions"

# 8. All tests pass
dotnet test --no-build --verbosity quiet 2>&1 | tail -5
```

## Red Flags — Delete and Start Over

- Code before test / test passes immediately
- `Substitute.For<>()` on domain entities or value objects
- Getter/setter tests (`CanSetName`, `HasInitializedCollections`)
- Assert only on mock calls (`Received(1)`) without outcome assertion
- Integration test with only `EnsureSuccessStatusCode()` — no body/DB check
- Test with zero assertions
- More than 3 unrelated assertions without `[Theory]`
- "Already spent X hours, deleting is wasteful" — sunk cost. Delete.
