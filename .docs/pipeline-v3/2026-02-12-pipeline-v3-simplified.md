# Pipeline v3 Simplified: Spec-First, Let It Code, Verify Honestly

**Date:** 2026-02-12
**Status:** Design draft. Simplified from the 7-step pipeline after research team critique.
**Predecessor:** `2026-02-12-pipeline-v3-design.md` (7-step version — over-engineered)
**Key insight:** 80% of cases are ties between single-agent and multi-agent on frontier models. Invest in specs, not orchestration.

---

## 1. Why Simplify

The 7-step pipeline (Decompose → Spec Writer → Implement → Review → Test Writer → Verify → Compound) was researched and principled but over-engineered:

- **35+ agent dispatches per 5-feature app.** 4-9x more expensive than a 3-step pipeline.
- **Marginal quality gain.** "Why Not Both?" paper: 80% of tasks show same outcome regardless of single vs. multi-agent. Frontier models don't need babysitting.
- **Complexity breeds failure.** 31.4% of multi-agent failures are inter-agent misalignment. More agents = more coordination risk.
- **Compound Engineering validates simplicity.** Their Work phase is 10% of effort. If the spec is good, execution is mechanical.

The research team critique identified the spec writer as the only step where investment clearly pays off. Everything else is either marginal or compensable with E2E testing.

---

## 2. The Pipeline

Five steps. Steps 1-2 are where effort lives. Steps 3-5 are lightweight verification and learning.

```
[1. Spec + Contract]    70% of effort — produce Task Contracts + compiled types
[2. Implement]          Let the AI code — one backend agent, one frontend agent
[3. Verify]             Mechanical — build, E2E, contract compliance
[4. Review]             Optional single pass — security + correctness
[5. Compound]           Document learnings for future sessions
```

Repeat steps 1-3 per feature slice. Step 4 is optional for simple features. Step 5 runs once after all slices.

---

## 3. Step 1: Spec + Contract (The Only Step That Matters)

| | |
|---|---|
| **Input** | Feature description + project context + existing Contracts/ + knowledge base |
| **Output** | (1) Compiled C# DTOs + routes in Contracts/, (2) Task Contracts (one per implementation unit) |
| **Who** | Single Opus agent. Or human with agent assist. |
| **Token budget** | 1,000-3,500 tokens output (task-dependent) |
| **Gate** | `dotnet build Contracts/` passes |

This is where 70% of effort goes. The spec writer produces two artifact types:

### Artifact 1: Contract Code

Real C# files in a shared Contracts/ project. DTOs as records with validation attributes. Route constants. Error response shape.

```
Contracts/
├── DTOs/
│   ├── RecipeDto.cs
│   ├── CreateRecipeRequest.cs    ← [Required], [StringLength] etc.
│   └── UpdateRecipeRequest.cs
├── Routes.cs                     ← static route constants
└── Contracts.csproj
```

The implementer imports these types. The compiler enforces the contract. Type drift is mechanically impossible. Research: type constraints reduce errors 50%+.

### Artifact 2: Task Contracts (one per implementation unit)

Each implementation unit gets a self-contained Task Contract — the single document the implementing agent receives. ~500 words max (quality degrades in long contexts per lost-in-middle effect). Critical info at top.

**Format — every Task Contract follows this structure:**

```yaml
---
# METADATA (machine-parseable)
task: create-recipe-endpoint
implements: POST /api/recipes
depends-on: [setup-database-schema]
layer: backend
spec: _docs/specs/2026-02-12-recipe-sharing.md
---
```

```markdown
## Intent

Enable authenticated users to create recipes. This is the core write
operation for the recipe-sharing feature — everything else builds on it.

## Contract

Implements `POST /api/recipes` from architecture spec.

Request: `CreateRecipeRequest { Title: string, Description: string?, Ingredients: IngredientDto[] }`
Response: `RecipeResponse { Id: Guid, Title: string, ... }`

## Acceptance Criteria

### GWT-1: Create recipe with valid input
GIVEN authenticated user (ID: any valid GUID)
WHEN POST /api/recipes with body:
  { "title": "Pasta Carbonara", "description": "Classic Roman pasta",
    "ingredients": [{"name": "Spaghetti", "amount": "400", "unit": "g"}],
    "steps": ["Boil water", "Cook pasta", "Mix with eggs"] }
THEN status = 201
AND body.id is non-empty GUID
AND body.title = "Pasta Carbonara"
AND body.authorId = authenticated user's ID
AND body.createdAt is ISO 8601 within 5s of now

### GWT-2: Empty title
GIVEN authenticated user
WHEN POST /api/recipes with title = ""
THEN status = 400
AND body.details has key "Title"

### GWT-3: No authentication
GIVEN no auth token
WHEN POST /api/recipes with valid body
THEN status = 401

## Examples

JSON request/response for the happy path. Disambiguates what GWT describes abstractly.
Research: I/O examples significantly improve pass@1 ("More Than a Score", 2025).

​```json
// Happy path request
POST /api/recipes
Authorization: Bearer <token>
{
  "title": "Pasta Carbonara",
  "description": "Classic Roman pasta",
  "ingredients": [
    { "name": "Spaghetti", "amount": "400", "unit": "g" },
    { "name": "Guanciale", "amount": "200", "unit": "g" }
  ]
}

// Happy path response (201)
{
  "id": "a1b2c3d4-...",
  "title": "Pasta Carbonara",
  "authorId": "user-guid-...",
  "createdAt": "2026-02-12T10:30:00Z"
}
​```

## Boundaries

Prevents scope creep and hallucinated requirements. High-impact addition (Osmani).

- **Always:** Validate all input fields before persisting
- **Never:** Return 500 for validation errors (always 400)
- **Out of scope:** Image upload, ingredient search, recipe sharing
- **Assume standard:** EF Core, repository pattern, FluentValidation

## Files

- Create: `backend/src/Recipes/CreateRecipeEndpoint.cs`
- Create: `backend/src/Recipes/CreateRecipeValidator.cs`
- Test: `backend/tests/Recipes/CreateRecipeEndpointTests.cs`
```

### Task Contract Design Principles

| Section | Purpose | Format | Evidence |
|---------|---------|--------|----------|
| **YAML frontmatter** | Machine-parseable metadata, dependencies | YAML | Dominant pattern across Claude Code, Cursor, Codex CLI |
| **Intent** | Design rationale — WHY (~50 words) | Markdown prose | Prevents hallucinated intent; user story best practice |
| **Contract** | Inline DTO shapes — WHAT shape | Type signatures | Type constraints reduce errors (directional) |
| **Acceptance Criteria** | Testable behavior — WHEN/THEN | GWT format | Highest AI-agent friendliness for verification |
| **Examples** | Concrete I/O — WHAT it looks like | JSON snippets | Significant pass@1 improvement ("More Than a Score", 2025) |
| **Boundaries** | Explicit constraints — what NOT to do | Always/Never/OOS | "Never" rules more valuable than "Do" rules (Osmani) |
| **Files** | Exact paths — WHERE | File list | Already proven in saurun plan templates |

**Key rules:**
1. **~500 words per task contract** — stays concise; quality degrades in long contexts
2. **Critical info at top** — YAML metadata + intent first (lost-in-middle effect)
3. **GWT over prose for acceptance** — unambiguous, maps directly to test assertions
4. **Examples are NOT redundant with GWT** — they disambiguate what GWT describes abstractly
5. **Boundaries prevent scope creep** — "Never" rules are more valuable than "Do" rules
6. **One task = one contract file** — modular, load on-demand, no monolithic specs
7. **Contract references, not repetition** — `implements: POST /api/recipes` points to architecture, doesn't restate it

**Quality heuristic:** 2-4 GWT scenarios per endpoint (happy path + error cases + edge cases). One I/O example per endpoint (happy path). Every endpoint must have at least one error scenario and one auth boundary test.

**Knowledge base integration:** Spec writer reads `docs/solutions/` for past learnings before writing.

---

## 4. Step 2: Implement (Let It Code)

| | |
|---|---|
| **Input** | Task Contract (single document per implementation unit) |
| **Output** | Working code |
| **Who** | Backend agent + Frontend agent (separate dispatches, can be parallel) |
| **Token budget** | ~1,500-2,500 tokens per agent |
| **Gate** | `dotnet build` + `npm run build` pass |

Two dispatches. No mandated process. No TDD requirement. No reviewer watching over their shoulder.

Each agent receives its Task Contract — a ~500 word self-contained document with intent, types, GWT criteria, I/O examples, and boundaries. The contract IS the spec. The agent doesn't need to load 10-page documents or cross-reference multiple files.

**Backend agent receives:**
- Task Contract (Intent + Contract types + GWT + Examples + Boundaries + Files)
- Contracts/ types (imports, doesn't create DTOs)
- Project conventions (from CLAUDE.md)
- Instruction: "Implement endpoints that satisfy this contract. Write tests if you want. Commit when done."

**Frontend agent receives:**
- Task Contract (Intent + Contract types + GWT + Examples + Boundaries + Files)
- TypeScript types (generated from Contracts/ via OpenAPI)
- Design system (if exists)
- Instruction: "Build the UI for these interactions. Write tests if you want. Commit when done."

Neither agent sees the other's code. Both implement against the same contract.

**Why no mandated testing:** Research shows agent-written tests don't meaningfully improve the agent's own output (+2.6%, costs +19.8% more tokens). If the agent writes tests naturally, fine. If it doesn't, E2E catches the real bugs.

**Why no mid-implementation review:** The spec is detailed enough that execution is mechanical. Review happens after, not during. Compound Engineering validates this — their Work phase is 10% of time.

---

## 5. Step 3: Verify (Mechanical, Honest)

| | |
|---|---|
| **Input** | Implemented code + running app |
| **Output** | Pass/fail with specific failures |
| **Who** | Mechanical (shell commands + E2E) |
| **Token budget** | Minimal (orchestrator only) |
| **Gate** | All checks pass |

Three verification layers, all mechanical:

### Layer 1: Compilation
```bash
dotnet build                    # backend compiles
npm run build                   # frontend compiles
```

### Layer 2: Contract Compliance
Hit every endpoint defined in the contract. Validate response shapes.

```bash
# For each endpoint in Routes.cs:
# POST /api/recipes with valid input → expect 201, body matches RecipeDto shape
# POST /api/recipes with empty title → expect 400
# GET /api/recipes/{id} → expect 200, body matches RecipeDto shape
# GET /api/recipes/{nonexistent} → expect 404
```

This can be a simple script or a lightweight agent that reads the GWT criteria and curls each endpoint. Not a full test suite — just contract compliance spot-checks.

### Layer 3: E2E Smoke Test
Start the app. Run through the primary user flow:
1. Create user (if auth feature exists)
2. Create recipe
3. View recipe
4. Edit recipe
5. Delete recipe

The app either works end-to-end or it doesn't. E2E tests are honest — you can't fake a passing E2E test the way you can write a tautological unit test.

### On Failure

Specific failure → back to the relevant implementing agent with the exact error:
- Build failure: "Compilation error in RecipeEndpoints.cs line 45: missing reference to CreateRecipeRequest"
- Contract failure: "POST /api/recipes returns 200, expected 201 per GWT-1"
- E2E failure: "Create recipe flow fails at step 3: form submission returns 500"

One retry. If still failing, stop and report to human.

---

## 6. Step 4: Review (Optional, Single Pass)

| | |
|---|---|
| **Input** | Completed, verified code |
| **Output** | Findings (P1 Critical / P2 Important / P3 Minor) |
| **Who** | Single review agent OR human |
| **Token budget** | ~2,000-3,000 tokens |
| **Gate** | Zero P1 findings |

**When to run:** Non-trivial features — new auth boundaries, payment processing, data mutations, public API surfaces. Skip for simple CRUD, UI tweaks, read-only endpoints.

**Single agent, two concerns:**
1. **Security** — injection, auth bypass, secrets exposure, OWASP top 10
2. **Correctness** — does the implementation match the GWT criteria? Any edge cases the E2E didn't cover?

Not 5 parallel specialists. One agent, one pass, focused on what E2E tests can't catch.

**P1 findings** go back to the implementing agent. P2/P3 are logged. One retry on P1. If still failing, stop and report.

---

## 7. Step 5: Compound (Knowledge Capture)

| | |
|---|---|
| **Input** | Completed feature: spec, code, verification results, review findings |
| **Output** | Solution document in `docs/solutions/` |
| **Who** | Compound agent (Sonnet or Haiku — straightforward task) |
| **Token budget** | ~1,000-1,500 tokens |
| **Gate** | Document exists |

After verification passes, document what was learned:

```yaml
---
title: "Recipe CRUD: SQLite GUID handling gotcha"
category: database-issues
symptom: "EF Core generates empty GUIDs with SQLite"
root_cause: "SQLite lacks native GUID type"
---

## Problem
[What went wrong or was surprising]

## Solution
[How it was resolved]

## Prevention
[What to do differently next time]
```

Future spec writer sessions read `docs/solutions/` and incorporate learnings. This is the compound effect.

---

## 8. Feature Decomposition

Before the pipeline runs, decompose the PRD into independent feature slices:

```
PRD: "Recipe sharing app with auth, CRUD, search, favorites"
  → Feature 1: User Auth (foundation — runs first)
  → Feature 2: Recipe CRUD (depends on Auth)
  → Feature 3: Recipe Search (depends on CRUD)
  → Feature 4: Favorites (depends on Auth + CRUD)
```

Sequence by dependency. Each feature gets its own pipeline run (Steps 1-3, optionally 4). Step 5 runs once at the end.

For greenfield apps, a scaffold step runs first (directory structure, git init, base configuration).

---

## 9. Token Cost Estimate

For a 4-feature app:

| Step | Dispatches | Tokens per dispatch | Total |
|------|-----------|-------------------|-------|
| Spec + Contract (×4) | 4 | ~3,000 | ~12,000 |
| Backend Implement (×4) | 4 | ~2,500 | ~10,000 |
| Frontend Implement (×4) | 4 | ~2,500 | ~10,000 |
| Verify (×4) | ~4 | ~1,000 | ~4,000 |
| Review (×2, optional) | 2 | ~3,000 | ~6,000 |
| Compound (×1) | 1 | ~1,500 | ~1,500 |
| **Total** | **~19** | | **~43,500** |

Compare to the 7-step pipeline: ~42-52 dispatches, ~84,000-234,000 tokens. This is **2-5x cheaper** with equivalent output quality for 80% of features.

---

## 10. What This Keeps vs. Drops

### Keeps (high-value, research-backed)

| Component | Why |
|-----------|-----|
| **Contract-as-code** | Prevents frontend/backend type drift. Compiler enforces contract. 50%+ error reduction. |
| **GWT acceptance criteria** | Forces edge case thinking during spec. Provides concrete verification targets. +7-12% from tests-as-input. |
| **I/O examples per endpoint** | Disambiguates what GWT describes abstractly. Significant pass@1 improvement ("More Than a Score", 2025). |
| **Explicit boundaries** | Always/Never/Out-of-scope prevents scope creep and hallucinated requirements. High-impact (Osmani). |
| **Task Contracts as delivery format** | ~500 word self-contained docs. Prevents lost-in-middle degradation. One file per implementation unit. |
| **E2E as primary verification** | Honest — app works or doesn't. Can't be faked like unit tests. |
| **Compound step** | Knowledge accumulates. Each project makes the next easier. |
| **Vertical slices** | Feature-first decomposition. Verify per feature, not end-of-pipeline. |

### Drops (marginal value, high cost)

| Component | Why dropped |
|-----------|------------|
| **Separate test writer agent** | Under-validated. No research proves separate-context tests are non-observational. Adds a dispatch per feature for uncertain benefit. |
| **Parallel specialized reviewers** | 80% of cases are ties. One focused review pass catches what E2E misses. 5 agents is overkill. |
| **Spec validation gate** | Another agent checking another agent. If the spec writer can't produce good GWT, a validator won't reliably catch it either. Better: invest in a better spec writer prompt. |
| **Mandated TDD** | +2.6% quality, +19.8% tokens. Not worth it. Let the agent decide. |
| **Debugging agent escalation** | Over-engineered fix loops. Simple approach: tell the agent what failed, retry once, stop if still broken. |

---

## 11. Open Questions

### Frontend type generation
Contracts/ is C#. Frontend needs TypeScript. Proposed: `dotnet build` → OpenAPI → `openapi-typescript` → TS types. Needs validation that this works reliably without manual intervention. Fallback: spec writer produces both C# and TS types.

### Complexity routing
Should trivial changes (add a field, rename endpoint) get even this simplified pipeline? Could skip Steps 1 and 4 entirely for changes that don't affect contracts.

### Shared infrastructure
Auth middleware, database setup, CORS — these span features. Handle via a "foundation" feature slice that runs first, or via the scaffold step.

### Model routing
Spec writer: Opus (needs reasoning). Implementers: Opus or Sonnet (depends on complexity). Review: Sonnet. Compound: Haiku.

### Naming
What do we call this? Not god-agent. Something that reflects the philosophy: spec-first, let it code, verify honestly.

### Bidirectional spec sync
When verification reveals edge cases not in the GWT, should the spec be updated? Kiro does this. Prevents spec drift over iterations.

---

## 12. Influences

- **Research:** `2026-02-11-god-agent-v3-research.md` — empirical foundations
- **Spec Contract Synthesis:** `research-synthesis-spec-contract.md` — Task Contract format, I/O examples evidence, boundaries pattern
- **Compound Engineering (Every Inc):** Plan/Work/Review/Compound loop, plans as primary artifacts, knowledge accumulation
- **Research team critique:** Validated contract-as-code and GWT. Challenged over-engineering. Found 80% ties on frontier models.
- **Kiro/Spec Kit/Tessl:** Spec-driven development patterns, GWT as UI primitive
- **SWE-bench Pro top performers:** Context retrieval > complex orchestration
- **"Why Not Both?" paper:** Hybrid routing, 80% ties, diminishing returns of multi-agent

---

## 13. Next Steps

1. **Name it.** Pick a name, create the skill structure.
2. **Build the spec writer.** This is 70% of the value. Test on 3-5 feature types. Evaluate GWT quality.
3. **Build the orchestrator.** Simple: spec → implement (parallel backend/frontend) → verify → optional review → compound.
4. **Build contract compliance checker.** Script or lightweight agent that curls endpoints and validates against GWT.
5. **Build compound step.** Knowledge capture template + retrieval for future spec sessions.
6. **Empirical test.** Run on a real feature. Measure: token cost, output quality, E2E pass rate. Compare against god-agent v1.x on same feature.
