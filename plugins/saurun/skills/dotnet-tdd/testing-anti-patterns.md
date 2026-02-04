# .NET Testing Anti-Patterns

## Overview

Tests must verify real behavior, not mock behavior. Mocks isolate infrastructure, not the thing being tested.

**Core principle:** Test what the code does, not what the mocks do.

**REFERENCE:** See SKILL.md for the Mock Boundary Rule (what to mock vs. never mock) and the full TDD cycle.

## Anti-Pattern Rules

```
1. NEVER test mock behavior — assert on real outcomes
2. NEVER add test-only methods to production classes
3. Mock COMPLETE data structures, not partial fields
4. ONE shared CustomWebApplicationFactory, never duplicate
```

## Anti-Pattern 1: Testing Mock Behavior

**Violation:**
```csharp
// BAD: Testing that the mock was called, not that behavior happened
await handler.Handle(new CreateListCommand("Groceries", householdId));
await repo.Received(1).AddAsync(Arg.Any<ShoppingList>()); // Testing mock!
```

**Why wrong:** Verifies mock was called, not that a list was created with correct name/household.

**Fix:** Test real behavior through integration test or domain object assertion.
```csharp
// GOOD: Test real outcome
var response = await _client.PostAsJsonAsync("/api/lists", new { Name = "Groceries" });
response.EnsureSuccessStatusCode();
var list = await response.Content.ReadFromJsonAsync<ListDto>();
Assert.Equal("Groceries", list!.Name);
```

### Gate Function

```
BEFORE asserting on mock calls (Received, Verify, Times):
  Ask: "Am I testing real behavior or just that a mock was called?"
  IF testing mock calls → STOP. Assert on returned data, DB state, or domain state instead.
```

## Anti-Pattern 2: Test-Only Methods in Production

**Violation:**
```csharp
// BAD: ResetState() only used in tests — pollutes production API
public void ResetState() { _items.Clear(); _version = 0; }
```

**Why wrong:** Production class polluted with test-only code. Dangerous if accidentally called.

**Fix:** Create fresh instances in tests or use test helpers.
```csharp
var list = new ShoppingList("Groceries", householdId); // Fresh instance
var list = TestHelpers.CreateListWithItems(householdId, "Milk", "Bread"); // Helper
```

### Gate Function

```
BEFORE adding any method to production class:
  Ask: "Is this only used by tests?"
  IF yes → STOP. Create fresh instances or put helpers in test utilities.
```

## Anti-Pattern 3: Mocking Without Understanding

**Violation:**
```csharp
// BAD: Mocked GetByJoinCodeAsync returns null by default — join logic never exercised
var repo = Substitute.For<IHouseholdRepository>();
await handler.Handle(new JoinHouseholdCommand("ABC123", deviceId));
await repo.Received(1).GetByJoinCodeAsync("ABC123"); // Passes but tests nothing
```

**Why wrong:** Mock returns null, so join logic is never exercised. Test passes for wrong reason.

**Fix:** Use integration test with real DB, or configure mock with realistic return values.

### Gate Function

```
BEFORE mocking any method:
  1. Ask: "What does the real method return/do?"
  2. Ask: "Does this test depend on that return value?"

  IF depends on return value → Configure mock with realistic data OR use integration test.
  IF unsure → Write integration test FIRST, extract unit test later if needed.

  Red flags: "I'll mock this to be safe", mocking without configuring return values.
```

## Anti-Pattern 4: Incomplete Mocks

**Violation:**
```csharp
// BAD: Only fields you think you need — downstream code may use missing fields
var mockStore = new StoreDto { Id = Guid.NewGuid(), Name = "Test Store" };
// Missing: Latitude, Longitude, GeofenceRadius
```

**Why wrong:** Partial mocks hide structural assumptions. Tests pass but integration fails.

**Fix:** Mock the COMPLETE data structure as it exists in reality.
```csharp
var store = new StoreDto
{
    Id = Guid.NewGuid(), Name = "Test Store",
    Latitude = 55.6761, Longitude = 12.5683,
    GeofenceRadius = 200, HouseholdId = householdId
};
```

### Gate Function

```
BEFORE creating mock/test data objects:
  1. Examine actual class/DTO definition
  2. Include ALL fields that code might consume downstream
  3. Verify mock matches real schema completely
  If uncertain → Include all properties with realistic values.
```

## Anti-Pattern 5: Getter/Setter Tests

**Violation:**
```csharp
// BAD: Tests C# language works, not your code's behavior
[Fact]
public void CanSetName()
{
    var item = new ShoppingItem();
    item.Name = "Milk";
    Assert.Equal("Milk", item.Name); // Will never catch a real bug
}
```

**Why wrong:** Tests language/framework features. Inflates test count with zero value.

**REFERENCE:** See SKILL.md "What to Test" section for the full DO/DON'T list.

**Fix:** Test behavior that uses the property instead.
```csharp
// GOOD: Tests real behavior
[Fact]
public void AddItem_WithValidName_AppendsToItemsList()
{
    var list = new ShoppingList("Groceries", householdId);
    list.AddItem("Milk", Category.Dairy);
    Assert.Equal("Milk", list.Items[0].Name);
}
```

### Gate Function

```
BEFORE writing any test:
  Ask: "What bug would this test catch?"
  IF "none" or "C# property assignment stops working" → STOP. Delete the test.
  IF "validation logic fails" or "state transition is wrong" → PROCEED.
```

## Anti-Pattern 6: Duplicate WebApplicationFactory

**Violation:**
```csharp
// BAD: Every test class creates its own factory with copy-pasted SQLite setup
public class ListTests
{
    private readonly WebApplicationFactory<Program> _factory;
    public ListTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => { /* same setup copy-pasted */ });
    }
}
```

**Why wrong:** Configuration duplicated across classes, inconsistent environments, slow bootstrapping.

**Fix:** Single shared factory as `IClassFixture`:

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open(); // Keep alive for test lifetime

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connection));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }
}

// All integration test classes share it:
public class ListTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    public ListTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
}
```

**Use SQLite in-memory with kept-alive connection.** NOT the EF InMemory provider (doesn't enforce constraints, foreign keys, or SQL behavior).

### Gate Function

```
BEFORE creating WebApplicationFactory in a test class:
  Ask: "Does a shared CustomWebApplicationFactory already exist?"
  IF yes → Use IClassFixture<CustomWebApplicationFactory>.
  IF no → Create ONE in test infrastructure, use everywhere via IClassFixture.
```

## Quick Reference

| Anti-Pattern | Gate Question | Fix |
|--------------|---------------|-----|
| Assert on mock calls | "Am I testing real behavior?" | Assert on outcome (DB, response, domain) |
| Test-only production methods | "Is this only used by tests?" | Fresh instances + test helpers |
| Mock without understanding | "What does the real method return?" | Configure realistic data or integration test |
| Incomplete mocks | "Does mock match real schema?" | Include ALL fields with realistic values |
| Getter/setter tests | "What bug would this catch?" | Test behavior that uses the property |
| Duplicate factory | "Does shared factory exist?" | Single `CustomWebApplicationFactory` + `IClassFixture` |

## Red Flags

- Assertions check `Received()` instead of outcomes
- Methods only called in test files
- Mock setup is >50% of test code
- Can't explain why mock is needed
- Mocking "just to be safe"
- Test named `CanSet*`, `HasInitialized*`, `IsNullable*`
- `new WebApplicationFactory<Program>()` copy-pasted across files
- Mocking domain entities or value objects
