---
name: dotnet-writing-plans
description: Use when you have a spec or requirements for a multi-step .NET backend task, before touching code. Covers task decomposition, test design, and TDD-ready implementation plans.
---

# .NET Writing Plans

## Overview

Every implementation plan is a self-contained instruction set: an engineer with zero codebase context should be able to execute it task-by-task using only the plan and TDD.

Write bite-sized tasks with exact file paths, complete code, and explicit test specs. DRY. YAGNI. TDD. Frequent commits. Assume the implementer is skilled but knows nothing about our toolset, problem domain, or good test design — be explicit about what to test and what NOT to test.

**Announce at start:** "I'm using the dotnet-writing-plans skill to create the implementation plan."

**REQUIRED CONTEXT:** Run in a dedicated worktree (created by `superpowers:brainstorming`).

**Save plans to:** `docs/plans/YYYY-MM-DD-<feature-name>.md`

## When NOT to Use
- Single-file bug fix with obvious cause and fix
- Config/environment changes (appsettings, launchSettings, .csproj tweaks)
- Renaming or moving files without logic changes
- Adding a NuGet package with no code changes beyond the import

## Test Quality Rules for Plan Writers

**These rules apply to EVERY test you spec in a plan. Violations mean the plan is broken.**

### NEVER spec these tests:
- Getter/setter tests (`CanSetName`, `CanSetProperties`, `HasInitializedCollections`)
- Tests that verify a property is nullable/not-null by default
- Tests that assert a constructor assigns values to properties
- Tests that check framework behavior (EF navigation properties, DI resolution)

### Every test MUST:
- **Verify behavior that could have a bug.** Ask: "What bug does this catch?" If the answer is "none" — delete the test from the plan.
- **Have max 3 assertions.** More than 3? Split into separate tests.
- **Use `[Theory]`+`[InlineData]` for multiple inputs** of the same behavior (e.g., empty string, null, whitespace all invalid).
- **Follow naming: `MethodName_Scenario_ExpectedBehavior`** — not `Test1`, `ItWorks`, `CanSetName`.

### Phase 1 of every plan MUST include:
- Shared test infrastructure setup (`CustomWebApplicationFactory`, test helpers)
- SQLite in-memory with kept-alive connection (NOT EF InMemory provider)
- `IClassFixture<CustomWebApplicationFactory>` pattern for integration tests

### Mock Boundary Rule in plans:
- Spec mocking for: DbContext, IHttpClientFactory, ILogger, TimeProvider, external APIs
- NEVER spec mocking for: domain entities, value objects, pure functions
- Always specify NSubstitute

## Bite-Sized Task Granularity

**Each step is one action:**
- "Write the failing test" — step
- "Run it to make sure it fails" — step
- "Implement the minimal code to make the test pass" — step
- "Run the tests and make sure they pass" — step
- "Commit" — step

## Plan Document Header

**Every plan MUST start with this header:**

```markdown
# [Feature Name] Implementation Plan

> **For Claude:** **REQUIRED SUB-SKILL:** Use `saurun:dotnet-tdd` to implement this plan task-by-task with TDD.

**Goal:** [One sentence describing what this builds]

**Architecture:** [2-3 sentences about approach]

**Tech Stack:** .NET 9, ASP.NET Core, EF Core 9, SQLite, xUnit, NSubstitute

---
```

## Task Structure

```markdown
### Task N: [Component Name]

**Files:**
- Create: `exact/path/to/File.cs`
- Modify: `exact/path/to/Existing.cs:123-145`
- Test: `tests/exact/path/to/FileTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var list = new ShoppingList("Groceries", householdId);

    // Act
    list.AddItem("Milk", Category.Dairy);

    // Assert
    Assert.Single(list.Items);
    Assert.Equal("Milk", list.Items[0].Name);
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test --filter "FullyQualifiedName~MethodName_Scenario_ExpectedBehavior"`
Expected: FAIL with "method not found" or "Assert.Single() Failure"

**Step 3: Write minimal implementation**

```csharp
public void AddItem(string name, Category category)
{
    _items.Add(new ShoppingItem(name, category));
}
```

**Step 4: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~MethodName_Scenario_ExpectedBehavior"`
Expected: PASS

**Step 5: Commit**

```bash
git add path/to/files
git commit -m "feat: add item to shopping list"
```
```

## Integration Test Pattern

Integration tests use `IClassFixture<CustomWebApplicationFactory>` with `TestHelpers.CreateAuthenticatedClient(factory, Guid.NewGuid())`. Arrange via API calls (not direct DB manipulation). Assert HTTP response + deserialized body. Same naming: `Endpoint_Scenario_ExpectedBehavior`.

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Tasks too large (multiple files + tests in one task) | One behavior per task. If task has >2 new files, split it. |
| Missing file paths | Every task MUST list exact `Create:` / `Modify:` / `Test:` paths. |
| Vague steps like "add validation" | Write the actual code in the plan. No placeholders. |
| Missing expected failure messages | Step 2 must say exactly what the test runner prints on failure. |
| Speccing getter/setter tests | Ask "what bug does this catch?" If "none" -- delete. |
| No test infrastructure in Phase 1 | `CustomWebApplicationFactory` + helpers MUST be Task 1. |

## Remember
- Exact file paths always
- Complete code in plan (not "add validation")
- Exact commands with expected output
- **REQUIRED SUB-SKILL:** `saurun:dotnet-tdd` for TDD workflow
- DRY, YAGNI, TDD, frequent commits
- NO getter/setter tests — ever

## Execution Handoff

After saving the plan, offer execution choice:

**"Plan complete and saved to `docs/plans/<filename>.md`. Two execution options:**

**1. Subagent-Driven (this session)** — I dispatch fresh subagent per task using `saurun:dotnet-implementer-prompt`, review between tasks with `saurun:dotnet-code-quality-reviewer-prompt`, fast iteration

**2. Parallel Session (separate)** — Open new session in worktree, batch execution with checkpoints

**Which approach?"**

**If Subagent-Driven chosen:**
- **REQUIRED SUB-SKILL:** `saurun:dotnet-implementer-prompt` for implementer dispatch
- **REQUIRED SUB-SKILL:** `saurun:dotnet-code-quality-reviewer-prompt` for code review
- Stay in this session, fresh subagent per task + code review

**If Parallel Session chosen:**
- Guide them to open new session in worktree
- **REQUIRED SUB-SKILL:** New session uses `superpowers:executing-plans`
