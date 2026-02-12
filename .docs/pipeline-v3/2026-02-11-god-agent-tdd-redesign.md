# God-Agent TDD Redesign: Context-Isolated Red-Green-Refactor

**Date:** 2026-02-11
**Status:** Design — reviewed, all gaps resolved
**Version:** 1.2 (addresses all v1.1 review findings: 3 critical, 4 high gaps, 3 contradictions)
**Motivation:** God-Agent v1.0.34 produces test-after code despite pre-loading TDD skills. Root cause: context pollution — the implementer subagent conceptualizes the full solution before writing tests, producing implementation-confirming tests rather than behavior-specifying tests.
**Inspiration:** [alexop.dev — Forcing Claude Code to TDD](https://alexop.dev/posts/custom-tdd-workflow-claude-code-vue/)

### v1.2 Changes (from v1.1 review)
- **Gate 2.5 made mechanical:** Automated `dotnet build` + `tsc --noEmit` verification with 2 retries
- **Missing types fast-path:** Phase 3 recovery if test writer discovers missing scaffolding types
- **Gate A escalation:** 3-attempt escalation path when tests pass against stubs (too easy)
- **Refactor always dispatched:** No longer optional — part of TDD cycle. Mechanical test verification after refactor.
- **Reviewer TDD compliance first:** Git log verification is now check #1 (BLOCKING), runs before test quality
- **Reviewer gets git history:** Controller injects `git log --oneline -n 10` into reviewer context
- **Anti-pattern detection mechanical:** Reviewer uses grep-based checks for Task.Delay, className, vi.mock
- **Bug table standardized:** Required for BOTH backend AND frontend test files (was frontend-only)
- **SignalR hub coverage gated:** Reviewer verifies all declared hubs have 3+ test scenarios
- **Review escalation path:** After 2x rejection for same category, escalate to systematic-debugging
- **Contract scope clarified:** Full behavioral contract (50-200 lines), not a brief excerpt
- **No repository mocking:** Explicit callout — repositories use real EF Core + SQLite, never NSubstitute
- **SignalR hook stubs typed:** Full interface shape with connected/messages/error/send
- **Frontend test writer gets design system:** For semantic HTML understanding (heading levels, ARIA roles)
- **Refactor verification explicit:** Controller runs test suite after refactor commit, re-dispatches on failure

---

## Problem Statement

The God-Agent dispatches implementer subagents with TDD skills pre-loaded. But within a single subagent context, the LLM:
1. Conceptualizes the full solution before writing anything
2. Writes tests that confirm the implementation it already planned
3. Commits everything in bulk

Evidence from ai-prototype-tool:
- Backend TDD: 5/10 (code review), arguably 3-4/10
- Frontend TDD: 3/10
- Git history: bulk commits (`165809b`: "all 91 tests" in one commit)
- Test quality: mega-tests (15-40 assertions), Task.Delay, structural className assertions

**Core insight:** Context pollution makes same-context TDD structurally impossible for LLMs. The test writer cannot "unsee" the implementation it's already planning.

---

## Solution: Three-Agent Loop with Context Isolation

Replace the current two-agent loop (implementer -> reviewer) with a three-agent loop where each agent sees only what it needs:

| Agent | Sees | Does NOT see |
|-------|------|-------------|
| Test Writer | Behavioral contract, test infrastructure, available types | Implementation plans, production file structure, how to organize code |
| Implementer | Failing test file paths, design system (frontend only) | Original spec, behavioral contract, architecture doc |
| Reviewer | Test files + implementation files + behavioral contract | Full spec (only the relevant contract) |

---

## Changes to Pipeline

### New: Phase 2.5 — Contract Scaffolding

Inserted between Phase 2 (Planning) and Phase 3 (Execution). Generates type shells, route stubs, and infrastructure scaffolding from the architecture doc so test writers can import real types and tests fail at assertion time (not compile time).

#### Backend Scaffolding

| Artifact | What gets generated | Example |
|----------|-------------------|---------|
| DTO records | Properties only, no methods | `public record SessionDto(Guid Id, string Status, DateTime CreatedAt);` |
| Enums | All values from spec | `public enum SessionStatus { TargetSelected, Conversation, ... }` |
| Interfaces | Method signatures, throw NotImplementedException | `Task<Result<string>> SendAsync(...) => throw new NotImplementedException();` |
| Rich entity shells | Properties + behavior method stubs | `public Result<Message> AddMessage(...) => throw new NotImplementedException();` |
| Anemic entity shells | Properties only, public get/set | `public class ExpertVerdict { public string Expert { get; set; } ... }` |
| Route registrations | All endpoints return 501 Not Implemented | `app.MapPost("/api/sessions", () => Results.StatusCode(501)).Produces<SessionDto>(201);` |
| EF Core DbContext | Entity sets declared, OnModelCreating empty | `public DbSet<PrototypeSession> Sessions { get; set; }` |
| DI registrations | Interface -> stub implementation | `services.AddSingleton<ILlmService, StubLlmService>();` |

**Not scaffolded:** Handler implementations, mapping logic, EF Core configurations (owned entities, converters), middleware, SignalR hub logic.

#### Frontend Scaffolding

| Artifact | What gets generated | Example |
|----------|-------------------|---------|
| TypeScript types | Interfaces matching backend DTOs | `export interface SessionDto { id: string; status: string; ... }` |
| Component stubs | Valid React component, empty render | `export function ConversationPage() { return <div /> }` |
| Query hooks | Real useQuery with empty/default data | `useQuery({ queryKey: [...], queryFn: () => Promise.resolve([]) })` |
| Mutation hooks | Real useMutation, no-op mutationFn | `useMutation({ mutationFn: async () => ({} as SessionDto) })` |
| SignalR hooks | Stub returning typed no-op connection | `export function useConversationHub(): UseHubReturn { return { connected: false, messages: [], error: undefined, send: async () => {} } }` |
| Zustand store | Correct shape, initial values | `create(() => ({ sessionId: null, setSession: () => {} }))` |
| MSW handlers | All endpoints, return empty/default | `http.get('/api/sessions/:id', () => HttpResponse.json({} as SessionDto))` |
| Router | All routes from component tree | `<Route path="/session/:id/chat" element={<ConversationPage />} />` |

**Not scaffolded:** Component internals, styling, event handlers, conditional rendering, error boundaries, loading states.

#### Why This Makes Tests Fail Correctly

- **Backend:** Tests hit real routes (registered in Program.cs) but get 501 Not Implemented. Assertions expecting 201/200 fail at assertion time, not compile time.
- **Frontend:** Tests render real components (stub exports) but see empty `<div />`. Assertions expecting headings/inputs/text fail because nothing renders.
- **Hooks:** Tests call real hooks that return empty data. Assertions expecting messages/sessions fail because data is empty.

#### Gate 2.5 — Mechanical Verification (AUTOMATED)

Gate 2.5 is **not a checklist** — the controller runs these commands automatically. All must pass before Phase 3 begins.

**Step 1: Compilation check (BLOCKING)**
```bash
dotnet build backend/   # Must exit 0
npx tsc --noEmit        # Must exit 0
```
If either fails: re-dispatch scaffolding agent with build output, up to 2 retries. If still failing after 2 retries: escalate to systematic-debugging.

**Step 2: Route stub verification**
```bash
dotnet test backend/ --filter "Category=Scaffolding" 2>&1 | grep -c "501"
# OR: curl each endpoint from API Contract, verify 501 response
```

**Step 3: Artifact existence check**
- [ ] All DTOs from architecture doc exist as files
- [ ] All enums from architecture doc exist as files
- [ ] All interfaces exist with method signatures (throw NotImplementedException)
- [ ] Rich entity shells exist with behavior method stubs (throw NotImplementedException)
- [ ] All endpoints from API Contract registered in Program.cs returning 501
- [ ] DbContext has DbSet declarations for all entities
- [ ] DI registrations for all interfaces -> stub implementations
- [ ] Frontend: all component stubs export valid React components
- [ ] Frontend: all hook stubs return correct TypeScript shape with empty data
- [ ] Frontend: Zustand store exists with correct shape
- [ ] Frontend: MSW handlers configured for all endpoints
- [ ] Frontend: Router wires all routes to stub components

**Step 4: Commit**
```bash
git add . && git commit -m "chore: contract scaffolding for {feature}"
```

#### Missing Types Fast-Path (Phase 3 recovery)

If a test writer reports "cannot import type X" during Phase 3:
1. Controller dispatches scaffolding agent with narrow scope: "Add type X to contract scaffolding"
2. Controller verifies `dotnet build` / `tsc --noEmit` still passes
3. Controller re-dispatches test writer with updated type list
4. This does NOT reset the per-task loop — it's a pre-step recovery

#### Scaffolding Agent Dispatch

```
Task(subagent_type="general-purpose", prompt="""
{AUTONOMOUS_PREAMBLE}

You are executing Phase 2.5 (Contract Scaffolding) of the god-agent pipeline.

ARCHITECTURE DOC: {contents}
PRODUCT SPEC: {contents}

Generate type shells, route stubs, and infrastructure scaffolding so that:
1. Backend compiles (`dotnet build backend/`)
2. Frontend type-checks (`npx tsc --noEmit`)
3. All endpoints return 501 Not Implemented
4. All hooks return correct TypeScript shape with empty/default data
5. All components render empty <div />

BACKEND rules:
- DTO records: properties only
- Enums: all values
- Rich entities: properties + method stubs (throw NotImplementedException)
- Anemic entities: properties only (public get/set)
- All routes from API Contract registered in Program.cs, returning 501
  Example: app.MapPost("/api/sessions", () => Results.StatusCode(501)).Produces<SessionDto>(201);
- DbContext with DbSet declarations for all entities
- DI: interfaces -> stub implementations (throw NotImplementedException)

FRONTEND rules:
- TypeScript types matching backend DTOs
- Components: valid React export, return <div />
- Query hooks: real useQuery, return empty arrays/objects matching type
- Mutation hooks: real useMutation, no-op mutationFn
- SignalR hooks: return typed shape `{ connected: false, messages: [], error: undefined, send: async () => {} }`
- Zustand store: correct shape, initial/empty values
- MSW handlers: all endpoints, return 200 with empty/default responses
- Router: all routes wired to stub components

Commit: "chore: contract scaffolding for {feature}"
Verify: dotnet build + tsc --noEmit both pass.
""")
```

---

### Modified: Phase 3 — Execution (Red-Green-Refactor-Review Loop)

#### Per-Task Orchestration

**Contract scope:** The "behavioral contract" is NOT a brief excerpt. It includes ALL relevant information from the architecture doc for this task: endpoint signatures, DTO definitions, validation rules, state transitions, expected behaviors, and error cases. The test writer receives the full contract scaffolding types (from Phase 2.5) plus the behavioral description from the architecture doc. Typical size: 50-200 lines depending on task complexity.

```
For each task in plan:
  1. Controller: extract behavioral contract from architecture doc
     (include: endpoint, DTOs, validation rules, state transitions, error cases)
  2. Controller: dispatch TEST WRITER
     -> Input: behavioral contract + test infrastructure + available types
     -> Output: failing test files + commit
  3. Controller: GATE A — verify tests FAIL (mechanical)
     -> Run `dotnet test backend/` or `npx vitest run {file}`
     -> MUST return non-zero exit code (test failures, not compile errors)
     -> If compile error: re-dispatch test writer — "tests must compile against scaffolding"
     -> If exit 0 (tests pass against stubs):
        Attempt 1: re-dispatch test writer — "Tests pass against 501/NotImplemented
        stubs. Your tests are too easy — they don't assert on real behavior. Rewrite
        tests that assert on response bodies, DB state, or rendered content that
        the stubs cannot provide."
        Attempt 2: re-dispatch test writer with same feedback + "Focus: what would
        break if the feature were deleted?"
        Attempt 3 (escalation): dispatch systematic-debugging — "Test writer cannot
        produce failing tests for this feature. Diagnose: is the contract too vague?
        Are stubs accidentally returning valid data? Recommend fix."
  4. Controller: dispatch IMPLEMENTER
     -> Input: failing test file paths ONLY (+ design system for frontend)
     -> Output: passing implementation + commit
  5. Controller: GATE B — verify tests PASS
     -> Run `dotnet test backend/` or `npm test`
     -> MUST return zero exit code
     -> If non-zero: dispatch systematic-debugging (up to 2 retries)
  6. Controller: REFACTOR step (always dispatched — part of TDD cycle)
     -> Implementer gets: "All tests pass. Refactor for clarity: remove
        duplication, improve names, extract helpers. Do NOT add new behavior.
        All tests must remain green."
     -> If implementer reports "no refactoring needed": skip commit, proceed
     -> If implementer refactors: Controller runs GATE B2 (mechanical):
        `dotnet test backend/` or `npm test` — MUST exit 0
        If tests break: re-dispatch implementer with "Refactor broke tests.
        Revert your changes and refactor more carefully. Run tests before committing."
     -> Commit: "refactor: {feature} - clean up"
  7. Controller: dispatch REVIEWER
     -> Input: test files + implementation files + behavioral contract + git log
     -> Output: PASS or FAIL with specific issues
     -> Test quality fail -> re-dispatch test writer with fixes
     -> Implementation fail -> re-dispatch implementer with fixes
     -> Both pass -> task complete
     -> ESCALATION: If reviewer rejects same task 2x for the same category
        (test quality or implementation quality), dispatch systematic-debugging
        with: test files + impl files + reviewer feedback from both rejections.
        Debugging agent diagnoses root cause and recommends targeted fix.
  8. Controller: log token usage to STATE.md
  9. Controller: update STATE.md — task complete
```

#### Commit History Pattern

Each task produces 2-3 commits:
```
test(red): {feature} - {N} failing tests
feat(green): {feature} - all tests passing
refactor: {feature} - clean up                  # optional, if refactoring needed
```

If reviewer requires fixes, additional commits appear:
```
fix(test): {feature} - reviewer feedback on test quality
fix(impl): {feature} - reviewer feedback on implementation
```

---

### Test Strategy: Integration-First

The God-Agent always uses SQLite (in-memory for tests). This eliminates the need for most mocking.

**Default:** Integration tests through HTTP with real SQLite in CustomWebApplicationFactory.

**NSubstitute ONLY for external service boundaries:**
- ILlmService (calls OpenAI — can't hit real API in tests)
- IHttpClientFactory (if calling external APIs)
- TimeProvider (if non-deterministic timestamps needed)

**Explicitly NOT mocked (use real implementations):**
- Repositories — all data access goes through real EF Core + SQLite in-memory
- DbContext — real database, real migrations, real queries
- Domain services — test behavior through HTTP, not by mocking dependencies
- MediatR handlers — called through real pipeline via HTTP requests

**Domain unit tests ONLY as exception when:**
- Entity has 5+ state transitions with documented invariants
- Reaching some paths through HTTP requires 4+ sequential API calls
- Test writer explicitly documents why unit test is necessary

**Frontend:** Render components, interact as user would, assert visible outcomes. MSW for API mocking. Real Zustand stores, real hooks, real components.

---

### New Agents

#### `backend-test-writer`

```yaml
---
name: backend-test-writer
description: >-
  Writes failing integration tests for .NET backend features.
  Gets behavioral contract only — no implementation context.
  Tests must compile against contract scaffolding but FAIL.
skills: saurun:dotnet-tdd
model: opus
---
```

System prompt:

```
You are a .NET test engineer. You write FAILING integration tests
from behavioral contracts. You follow the pre-loaded `dotnet-tdd`
skill strictly.

You NEVER write production code. You ONLY write test files.
Test files must compile against the existing contract scaffolding
(DTO records, enum definitions, interface stubs, 501 route stubs)
but MUST FAIL when run — endpoints return 501 and entity methods
throw NotImplementedException.

Use the existing CustomWebApplicationFactory. Do NOT modify it.
If the factory needs changes (e.g., registering a new mock service),
report to controller — do not modify factory during test writing.

Implement all test tasks assigned to you and ONLY those tasks.
```

#### `frontend-test-writer`

```yaml
---
name: frontend-test-writer
description: >-
  Writes failing tests for React frontend features using Vitest
  and React Testing Library. Gets behavioral contract only — no
  component implementation context. Tests must FAIL.
skills: saurun:react-tdd
model: opus
---
```

System prompt:

```
You are a React test engineer. You write FAILING tests from
behavioral contracts using Vitest and React Testing Library.
You follow the pre-loaded `react-tdd` skill strictly.

You NEVER write component code, hooks, or stores. You ONLY
write test files. Components are stubbed (render empty <div />),
hooks return empty data. Your tests assert on behavior that
doesn't exist yet — they MUST fail.

Query priority: getByRole > getByLabelText > getByText > findBy*.
getByTestId is LAST RESORT only.

NEVER assert on className, style, or CSS. Test user-visible behavior.

Implement all test tasks assigned to you and ONLY those tasks.
```

---

### Modified Agents

#### `backend-implementer` (modified)

```yaml
---
name: backend-implementer
description: >-
  Implements .NET backend code to make failing tests pass.
  Sees ONLY failing test files — no spec or contract.
  Does NOT handle frontend code, infrastructure, or deployment.
skills: saurun:dotnet-tactical-ddd
model: opus
---
```

**Change:** Removed `saurun:dotnet-tdd` from skills. Test writing is now the test-writer's job. Implementer focuses on making tests pass using DDD patterns.

System prompt:

```
You are a .NET backend developer. You receive FAILING tests and
write the minimal production code to make them pass. You follow
DDD patterns from the pre-loaded `dotnet-tactical-ddd` skill.

You NEVER modify test files. You ONLY write production code.
Read the test files to understand what to build. The tests ARE
your specification.

Replace 501 route stubs with real handler implementations.
Replace NotImplementedException stubs with real behavior.

Implement all tasks assigned to you and ONLY those tasks.
```

#### `frontend-implementer` (modified)

```yaml
---
name: frontend-implementer
description: >-
  Implements React frontend code to make failing tests pass.
  Sees ONLY failing test files + design system. Does NOT handle
  backend code, React Native, or deployment.
skills: saurun:react-frontend-patterns, saurun:react-tailwind-v4-components, frontend-design:frontend-design
model: opus
---
```

**Change:** Removed `saurun:react-tdd` from skills. Added note that implementer receives design system context since tests don't specify visual design.

System prompt:

```
You are a frontend React developer. You receive FAILING tests and
write the minimal component code to make them pass. You apply
visual design from the design system provided.

You NEVER modify test files. You ONLY write components, hooks,
and stores. Read the test files to understand what to build. The
tests ARE your specification.

Replace stub components (empty <div />) with real implementations.
Replace stub hooks (empty data) with real TanStack Query hooks.

Tests don't check visual design — that's your responsibility using
the pre-loaded design skills and the design system summary provided.

Implement all tasks assigned to you and ONLY those tasks.
```

---

## Dispatch Prompt Templates

### Backend Test Writer Dispatch

```
Task(subagent_type="saurun:backend-test-writer", prompt="""
{AUTONOMOUS_PREAMBLE}

You are writing FAILING integration tests for a feature. You do NOT
write production code. You ONLY write test files.

## Behavioral Contract

{extracted from architecture doc — endpoint, DTOs, validation, behaviors}

## Available Types (from contract scaffolding)

DTOs: {list with namespaces}
Enums: {list}
Interfaces: {list — methods throw NotImplementedException}
Routes: {list — all return 501 Not Implemented}

## Test Strategy: Integration-First

DEFAULT: Test through HTTP with real SQLite.

Pattern for EVERY test:
  1. Arrange: set up data via API calls or direct DB seeding
  2. Act: HTTP call to the endpoint under test
  3. Assert: response body AND/OR DB state

NSubstitute is ONLY used for external service boundaries, pre-registered
in CustomWebApplicationFactory:
- ILlmService -> returns predictable responses by default

For tests needing specific LLM responses, override per-test:
```csharp
factory.LlmService.SendAsync(Arg.Any<...>())
    .Returns(Task.FromResult("mock response"));
```

EXCEPTION: Domain unit tests ONLY when entity has 5+ state transitions
AND reaching test paths through HTTP requires 4+ sequential calls.
Document why unit test is necessary in a comment.

## Test Infrastructure

- Test project: backend/Api.Tests/
- Factory: backend/Api.Tests/Infrastructure/CustomWebApplicationFactory.cs
- Pattern: IClassFixture<CustomWebApplicationFactory>
- Assertions: FluentAssertions
- Namespace: Api.Tests.{FeatureArea}
{path to existing test file for import/style reference}

Do NOT modify CustomWebApplicationFactory. If changes are needed,
report to controller.

## Test Design Rules

1. ONE test per behavior. Name: `MethodName_Scenario_ExpectedBehavior`
2. Max 3 assertions per test. Use [Theory]+[InlineData] for variations.
3. Assert response body AND DB state. Status code alone proves nothing.
4. No `Task.Delay`. Use polling helper: retry with 50ms intervals, 5s timeout.
5. No test-only public methods on production classes.
6. No assertion-less tests.
7. No fire-and-forget Task.Run patterns in any code you write.
8. For each test: "What bug does this catch?" If "none" -> don't write it.
9. Seed test data through API when possible, not raw SQL.

## Bug Table (REQUIRED — same as frontend)

At the top of every test file, add a comment:
```csharp
// Bug Table:
// | Test | Bug It Catches |
// |------|---------------|
// | CreateSession_ValidInput_Returns201 | Session creation fails silently |
// | CreateSession_DuplicateId_Returns409 | Duplicate sessions corrupt data |
```

If you can't fill the "Bug It Catches" column — don't write the test.

## SignalR Hub Tests

For each SignalR hub in the behavioral contract, write integration tests
using Microsoft.AspNetCore.SignalR.Client:
- Normal message delivery
- Concurrent sends
- Disconnect/reconnect handling

## Commit & Report

1. Run `dotnet test backend/` — tests MUST fail (routes return 501,
   methods throw NotImplementedException)
2. git add {test files only}
3. git commit -m "test(red): {feature} - {N} failing tests"
4. Report:
   - File paths
   - Test count
   - Failure summary (which tests fail and why)
   - Confirm bug table exists at top of each test file

DO NOT modify production code files.
""")
```

### Frontend Test Writer Dispatch

```
Task(subagent_type="saurun:frontend-test-writer", prompt="""
{AUTONOMOUS_PREAMBLE}

You are writing FAILING tests for a frontend feature. You do NOT
write component code. You ONLY write test files.

## Behavioral Contract

{extracted from architecture doc — component, props, behaviors}

## Available Types (from contract scaffolding)

TypeScript types: {list}
Hook stubs: {list — return empty/default data}
Component stubs: {list — render empty <div />}
MSW handlers: {path to handlers file — return empty/default responses}

## Design System Context (for semantic HTML understanding)

{extract from design-system/MASTER.md if exists, otherwise omit}

NOTE: You do NOT test visual design (no className/style assertions).
Use this context ONLY to understand expected semantic structure:
correct heading levels (h1 vs h2), ARIA roles, form field labels,
and navigation patterns. This helps you write accurate getByRole
and getByLabelText queries.

## Test Infrastructure

- Test runner: Vitest
- Libraries: @testing-library/react, @testing-library/user-event
- API mocking: MSW (configured in src/mocks/)
- Custom render: src/test/test-utils.tsx (provides Router + QueryClient)
- SignalR mock: {path if exists}

## Query Priority (MANDATORY)

1. getByRole — buttons, headings, inputs, links
2. getByLabelText — form fields
3. getByText — visible text content
4. getByPlaceholderText — inputs without labels
5. findByRole / findByText — async content
LAST RESORT ONLY: getByTestId

## Mock Boundaries

- MSW for API responses (override per-test for error/empty states)
- vi.mock ONLY for: window.location, IntersectionObserver, ResizeObserver
- NEVER vi.mock: Zustand stores, components, custom hooks

## Test Design Rules

1. ONE test per behavior. Name: `should [behavior] when [condition]`
2. Max 3 assertions per test. Use `it.each` for parameterized cases.
3. Test USER-VISIBLE BEHAVIOR only.
   - YES: "should show error message when API returns 500"
   - NO: "should have className bg-red-500"
   - NO: "should call onSubmit handler"
4. No className/style assertions. EVER.
5. No assertion-less tests.
6. No `setTimeout` in tests. Use `findBy*` and `waitFor`.
7. For each test: "What bug does this catch?" If "none" -> don't write it.
8. Test error states, loading states, and empty states — not just happy path.

## Bug Table (REQUIRED)

At the top of every test file, add a comment:
```tsx
/*
 * Bug Table:
 * | Test | Bug It Catches |
 * |------|---------------|
 * | should show empty state when no messages | Empty chat shows broken UI |
 * | should show error when send fails | User gets no feedback |
 */
```

If you can't fill the "Bug It Catches" column — don't write the test.

## Commit & Report

1. Run `npx vitest run {test file}` — tests MUST fail (components render
   empty <div />, hooks return empty data)
2. git add {test files only}
3. git commit -m "test(red): {feature} - {N} failing tests"
4. Report: file paths, test count, failure summary, bug table

DO NOT create or modify component files, hooks, or stores.
""")
```

### Backend Implementer Dispatch

```
Task(subagent_type="saurun:backend-implementer", prompt="""
{AUTONOMOUS_PREAMBLE}

You have FAILING tests. Write minimal production code to make them pass.

## Failing Tests

{list of test file paths}

Run `dotnet test backend/` to see current failures.

## What Exists Already (contract scaffolding)

DTO records, enums, and entity shells exist but:
- Entity methods throw NotImplementedException
- All routes return 501 Not Implemented
- Interfaces have stub implementations

You need to:
- Replace 501 route stubs with real handler implementations
- Replace NotImplementedException stubs with real behavior
- Configure EF Core (owned entities, converters) if needed
- Wire up DI for real implementations

## Rules

1. Read the test files first. Understand what they expect.
2. Write MINIMAL code to pass. No gold-plating.
3. Follow DDD patterns from your pre-loaded skill.
4. Do NOT modify test files.
5. Do NOT add behaviors beyond what tests require.
6. If a test expects 404, implement that path. If no test checks
   rate limiting, don't add rate limiting.
7. No fire-and-forget Task.Run without error handling. If background
   work is needed, use proper error observability.
8. Commit: "feat(green): {feature} - all tests passing"

## Verification

Run `dotnet test backend/` — ALL tests must pass (including pre-existing).
If pre-existing tests break, fix without modifying new test files.

Report: files created/modified, test results, concerns.
""")
```

### Frontend Implementer Dispatch

```
Task(subagent_type="saurun:frontend-implementer", prompt="""
{AUTONOMOUS_PREAMBLE}

You have FAILING tests. Write minimal component code to make them pass.

## Failing Tests

{list of test file paths}

Run `npx vitest run` to see current failures.

## Design System

- Style: {style_name}
- Colors: Primary {hex}, Accent {hex}, Background {hex}
- Typography: {heading_font} / {body_font}
- Mood: {mood_keywords}

## What Exists Already (contract scaffolding)

- Component stubs: render empty <div /> — replace with real implementations
- Hook stubs: return empty data — replace with real TanStack Query hooks
- Zustand store: initial values — implement real actions
- MSW handlers: return defaults — keep for tests, implement real API client
- TypeScript types: complete — use as-is

## Rules

1. Read the test files first. Understand what they assert.
2. Write MINIMAL code to pass all tests.
3. Apply design system for visual styling (tests don't check appearance).
4. Do NOT modify test files.
5. Do NOT add behaviors beyond what tests require.
6. Use semantic HTML so tests using getByRole/getByText work naturally.
   Only add data-testid if tests reference them.
7. Placeholder convention for images:
   <div data-asset="{type}-{name}" className="..." />

## Verification

Run `npx vitest run` — ALL tests must pass.
Commit: "feat(green): {feature} - all tests passing"
Report: files created/modified, test results.
""")
```

### Refactor Dispatch (same implementer agent, resumed or new)

```
Task(subagent_type="{same implementer type}", prompt="""
{AUTONOMOUS_PREAMBLE}

All tests pass. Refactor the implementation for clarity.

## Files to Review

{list of implementation files from green commit}

## Refactor Checklist

- Remove code duplication
- Improve variable/method names for clarity
- Extract reusable helpers if pattern appears 3+ times
- Simplify complex conditionals
- Move business logic out of controllers/endpoints into domain

## Rules

1. Do NOT add new behavior or features.
2. Do NOT modify test files.
3. ALL tests must remain green after every change.
4. Run full test suite after refactoring to verify.
5. If nothing needs refactoring, report "no refactoring needed" and skip.
6. Commit: "refactor: {feature} - clean up"
""")
```

### Quality Reviewer Dispatch

```
Task(subagent_type="superpowers:code-reviewer", prompt="""
Review this TDD implementation for test quality, spec compliance,
and code quality.

## Context

TASK BEHAVIORAL CONTRACT:
{original contract from architecture doc}

TEST FILES (red commit):
{test file paths}

IMPLEMENTATION FILES (green commit):
{implementation file paths}

GIT HISTORY (controller injects this — do NOT skip):
{output of: git log --oneline -n 10}

DIFF: Compare {RED_COMMIT_SHA}..{GREEN_COMMIT_SHA}

SIGNALR HUBS (from architecture doc):
{list of declared SignalR hub names, or "none"}

## Review Checklist

### 1. TDD Compliance (BLOCKING — verify FIRST, before any other check)

Run these verifications against the git log provided above:

- [ ] `test(red):` commit exists BEFORE `feat(green):` commit
- [ ] Red commit modifies ONLY test files
      Verify: `git diff --name-only {RED_SHA}^..{RED_SHA}` — all paths contain `/Tests/` or `.test.` or `.spec.`
- [ ] Green commit modifies ONLY production files
      Verify: `git diff --name-only {GREEN_SHA}^..{GREEN_SHA}` — NO paths contain `/Tests/` or `.test.` or `.spec.`
- [ ] If `refactor:` commit exists: it comes AFTER `feat(green):`

If red/green order is wrong or files are mixed -> FAIL immediately with routing: controller.
This is non-negotiable — it proves TDD was practiced.

### 2. Test Quality Gate (BLOCKING)

Run through EVERY test in the test files:

**Anti-Pattern Check (use these mechanical searches):**
- [ ] `grep -c "Task\.Delay\|Thread\.Sleep" {test_files}` returns 0
- [ ] `grep -c "setTimeout\b" {test_files}` returns 0 (frontend)
- [ ] No test has >3 assertions (unless single FluentAssertions chain)
- [ ] `grep -c "className\|\.style\." {test_files}` returns 0 (frontend)
- [ ] No assertion-less tests (every test has `.Should` or `expect()`)
- [ ] No test-only public methods on production classes
- [ ] `grep -c "vi\.mock.*store\|vi\.mock.*Hook\|vi\.mock.*Component" {test_files}` returns 0 (frontend)
- [ ] No `getByTestId` where getByRole/getByLabelText/getByText works (frontend)
- [ ] No tests where `.Received()` or `toHaveBeenCalled` is the SOLE assertion

**Bug Table Check (BLOCKING — required for BOTH backend AND frontend):**
- [ ] Every test file has a bug table comment at the top:
      Backend format: `// Bug Table: | Test | Bug It Catches |`
      Frontend format: `/* Bug Table: | Test | Bug It Catches | */`
- [ ] Every test method has a corresponding entry in the bug table
- [ ] Every "Bug It Catches" entry describes a real user-facing or system bug

**Value Check:**
- [ ] Tests cover error paths, not just happy path
- [ ] Tests cover edge cases from contract
- [ ] No duplicate tests (same behavior tested twice with different names)

**SignalR Hub Coverage (if SIGNALR HUBS is not "none"):**
- [ ] Every hub listed has at least 3 test scenarios
- [ ] Hub tests cover: normal message delivery, concurrent sends, disconnect/reconnect

If ANY anti-pattern or bug table check fails -> FAIL with specific test names, line numbers, and routing: test-writer.

### 3. Spec Compliance

- [ ] Every behavior in contract has a corresponding test
- [ ] Implementation handles all tested behaviors
- [ ] No extra behaviors added beyond tests + contract
- [ ] Mock boundaries correct: NSubstitute ONLY for ILlmService, IHttpClientFactory, TimeProvider
      No repository mocking. All tests use real SQLite via CustomWebApplicationFactory.

### 4. Code Quality

**Load criteria skill:**
- Backend: `saurun:dotnet-code-quality-reviewer-prompt`
- Frontend: `saurun:react-code-quality-reviewer-prompt`

**Additional checks:**
- [ ] Implementation is minimal (no gold-plating beyond test requirements)
- [ ] No fire-and-forget `Task.Run` without error handling
      Verify: `grep -rn "Task\.Run" {impl_files}` — each occurrence MUST have try/catch with ILogger
- [ ] External services properly mocked in test factory
- [ ] No `Task.Delay` in production code for synchronization

## Severity & Routing

- **Critical:** FAIL
  - TDD compliance violations -> routing: controller (must re-run entire loop)
  - Test quality anti-patterns -> routing: test-writer (re-dispatch with fix list)
  - Implementation quality -> routing: implementer (re-dispatch with fix list)
- **Minor:** Note and continue

## Output

VERDICT: PASS | FAIL
If FAIL: issue list with file:line, severity, fix instruction, routing target (test-writer | implementer | controller).
""")
```

---

## Agent Routing Table (Updated)

| Plan Type | Test Writer | Implementer | Review Criteria |
|-----------|-------------|-------------|-----------------|
| Backend domain | `saurun:backend-test-writer` | `saurun:backend-implementer` | `saurun:dotnet-code-quality-reviewer-prompt` |
| Backend API | `saurun:backend-test-writer` | `saurun:backend-implementer` | `saurun:dotnet-code-quality-reviewer-prompt` |
| Backend SignalR | `saurun:backend-test-writer` | `saurun:backend-implementer` | `saurun:dotnet-code-quality-reviewer-prompt` |
| Frontend state | `saurun:frontend-test-writer` | `saurun:frontend-implementer` | `saurun:react-code-quality-reviewer-prompt` |
| Frontend pages | `saurun:frontend-test-writer` | `saurun:frontend-implementer` | `saurun:react-code-quality-reviewer-prompt` |
| Integration | Both test writers | Both implementers | Both criteria skills |

---

## Token Cost Analysis

**Current (per task):** 2 subagent dispatches
- Implementer (writes code + tests)
- Reviewer

**Proposed (per task):** 3-4 subagent dispatches + 2 mechanical verifications
- Test Writer
- Controller: `dotnet test` / `npm test` (verify fail) — negligible cost
- Implementer
- Controller: `dotnet test` / `npm test` (verify pass) — negligible cost
- Refactor (optional — same implementer, skip if nothing needed)
- Reviewer

**Estimated overhead:** ~50% more subagent dispatches per task. For a typical 15-task project, this means ~15 additional subagent calls. Each test-writer call should be shorter than current implementer calls (writing tests is less code than writing tests + implementation).

**Net token estimate:** ~30-40% increase over current approach. Offset by:
- Fewer reviewer rejections (tests catch issues earlier)
- Less systematic-debugging dispatch (tests are better designed)
- No rework from test quality issues

---

## Migration Path

### Phase 1: Create new agents (no god-agent changes)
1. Create `backend-test-writer` agent definition
2. Create `frontend-test-writer` agent definition
3. Update `backend-implementer` — remove `dotnet-tdd` skill
4. Update `frontend-implementer` — remove `react-tdd` skill

### Phase 2: Add Phase 2.5 to god-agent
5. Add Contract Scaffolding phase between Phase 2 and Phase 3
6. Add Gate 2.5 checklist
7. Add scaffolding agent dispatch prompt

### Phase 3: Restructure Phase 3 execution loop
8. Replace single implementer dispatch with: test-writer -> verify-fail -> implementer -> verify-pass -> refactor -> reviewer
9. Update dispatch prompt templates
10. Update STATE.md tracking for red/green/refactor commits
11. Update token usage logging (3-4 agents per task)

### Phase 4: Update TDD skills + add hooks
12. Update `dotnet-tdd` SKILL.md — add section on integration-first strategy
13. Update `react-tdd` SKILL.md — reinforce accessible query hierarchy, ban className assertions
14. Update testing-anti-patterns.md files — add Task.Delay and mega-test detection
15. Add hook enforcement for TDD skill activation (~84% improvement per alexop.dev)

---

## Expected Outcomes

| Metric | Current (v1.0.34) | Expected (v2.0) |
|--------|-------------------|-----------------|
| TDD compliance | F (0/10) | A- (verifiable red-green-refactor commits) |
| Test quality | C (5.5/10) | B+ (behavioral, single-assertion, no anti-patterns) |
| Backend TDD score | 5/10 -> 3-4/10 actual | 8-9/10 |
| Frontend TDD score | 3/10 | 7-8/10 |
| Completeness | B+ (92%) | B+ (unchanged — same spec pipeline) |
| Architecture | A- | A- (unchanged — same architecture phase) |
| Token cost | Baseline | +30-40% |
| Execution time | ~30 min | ~40-45 min |

---

## Resolved Design Questions

### 1. SignalR Hub Tests
**Decision:** Architecture doc must include a `## SignalR Hubs` section listing all hubs by name. Test writer dispatch receives this list. For each hub: write minimum 3 integration tests (normal delivery, concurrent sends, disconnect/reconnect) using `Microsoft.AspNetCore.SignalR.Client`. Reviewer gate verifies: every declared hub has test coverage. If hub tests are missing -> reviewer FAIL with routing: test-writer.

### 2. Complex State Machines
**Decision:** Integration-first as primary. Entity unit tests only as documented exception (5+ transitions, 4+ API calls to reach path). Test writer must document why unit test is necessary.

### 3. E2E Test Interaction
**Decision:** Keep E2E separate in Phase 5. Red-green-refactor covers unit/integration tests. E2E remains a validation phase, not a development phase.

### 4. Hook Enforcement
**Decision:** Implement hooks in Migration Phase 4. Pre-prompt hooks inject TDD skill evaluation, improving activation from ~20% to ~84% (per alexop.dev). This supplements agent-level skill pre-loading.
