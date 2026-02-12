# Synthesis: The Optimal Spec Contract for AI Coding Agents

**Date:** 2026-02-12
**Input:** 3 research documents + analysis of existing saurun pipeline formats
**Purpose:** Recommend the preferred contract format for an AI agent to receive implementation specs

---

## Executive Summary

**Winner: Layered hybrid — YAML metadata + Markdown narrative + GWT acceptance criteria + concrete examples**

No single format (GWT, EARS, User Stories, etc.) is sufficient alone. The research converges on a **layered contract** that combines:

1. **Structured metadata** (YAML frontmatter) — machine-parseable identity, dependencies, constraints
2. **Intent narrative** (Markdown, ~100 words) — WHY we're building this, design rationale
3. **Testable acceptance criteria** (GWT format) — unambiguous pass/fail behavioral contracts
4. **Concrete examples** (I/O snippets) — primary disambiguation mechanism
5. **Explicit boundaries** (Always/Ask/Never) — prevents scope creep and hallucinated requirements

This isn't theoretical — it's what the empirical evidence and industry convergence both point to.

---

## Why Not Just One Format?

| Format | What it nails | What it misses |
|--------|--------------|----------------|
| **GWT/BDD** | Acceptance criteria (testable) | Intent, architecture, constraints |
| **EARS** | Precise requirements (parseable) | Intent, technical context |
| **User Stories** | User intent (why) | Technical detail, acceptance rigor |
| **Design Docs** | Architecture + rationale | Testable criteria, conciseness |
| **Natural Language** | Flexibility, nuance | Everything else (ambiguous) |
| **Pure YAML/JSON** | Machine parseability | Behavioral description |

Every format excels at 1-2 dimensions. An AI coding agent needs ALL dimensions:
- **Intent** — why are we building this?
- **Constraints** — what are the boundaries?
- **Acceptance criteria** — how do we know it's done?
- **Technical context** — what exists already?
- **Examples** — what does correct behavior look like concretely?

---

## The Evidence

### Key findings (confidence varies — see Source column)

| Finding | Impact | Source |
|---------|--------|--------|
| Adding I/O examples improves pass@1 | Significant (varies by model/task) | "More Than a Score" (2025, arxiv.org/abs/2508.03678) |
| Detailed prompts with algorithmic hints enable previously-failing tasks | Directional (unquantified) | Practitioner reports, directional only |
| Markdown outperforms structured formats for modern models | Directional | Practitioner consensus; no rigorous comparative study found |
| Instruction quality degrades in long contexts | "Lost in the middle" effect | Liu et al. (2023); threshold varies by model |
| Spec modules should be concise (~500 words) | Diminishing returns past this | Practitioner consensus |
| "One code snippet outperforms three paragraphs" | Qualitative consensus | Addy Osmani, practitioner surveys |
| Type constraints reduce errors | Directional (magnitude unquantified) | Internal pipeline observation |
| Spec/system design failures are a leading cause of agent failures | Directional | Practitioner reports; no rigorous study with exact % found |

### Medium-confidence findings (observational/directional)

- Spec-first workflows reduce rework significantly (Thoughtworks SDD, 2025; no specific multiplier published)
- EARS-style keywords are trivially machine-parseable and eliminate ambiguity
- Current-state / desired-state delta helps agents understand scope
- Separate requirements/design/tasks produces more maintainable output
- Tests as input > tests as process (agent-written tests cost +19.8% tokens for only +2.6% quality)

### What definitely hurts

- Vague prompts without concrete examples
- Mid-task requirement changes (Devin performance data)
- Massive context dumps without summarization
- Generic templates across different models (23-29% degradation)
- Abstract requirements without I/O examples

---

## What Our Pipeline Already Does Well

The existing saurun pipeline (god-agent / execute-spec) already has strong patterns:

**Strengths:**
- **Layered approach** — Product Spec → Architecture → Plans → Execute
- **Strict plan templates** — `Implements:`, `Files:`, `Behaviors:`, `Dependencies:` is already structured and parseable
- **Mechanical validation** — verify-plan-coverage does coverage checking with anti-hallucination rules
- **Conciseness enforcement** — plan skills explicitly prohibit redundancy ("NO full code", "NO obvious patterns")
- **Separation of concerns** — persistent context (CLAUDE.md) vs per-task specs

**Weaknesses:**
- **Product specs are narrative prose** — ambiguous, agent must interpret
- **Architecture docs are semi-structured markdown** — no compiler enforcement, drift possible
- **Acceptance criteria are implicit** — buried in "Behaviors" bullet points, not explicit GWT
- **No concrete examples** — plans describe behaviors but don't show expected I/O
- **No explicit boundaries at spec level** — out-of-scope exists in specs but not in plan tasks

---

## The Recommended Contract Format

### Structure: Task Contract

This is what an implementing agent receives for a single unit of work:

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
GIVEN authenticated user
WHEN POST /api/recipes with { "title": "Pasta Carbonara", "description": "Classic Roman pasta" }
THEN status = 201
AND body.id is non-empty GUID
AND body.title = "Pasta Carbonara"
AND body.authorId = authenticated user's ID

### GWT-2: Reject missing title
GIVEN authenticated user
WHEN POST /api/recipes with { "title": "" }
THEN status = 400
AND body.errors contains "Title is required"

### GWT-3: Reject unauthenticated request
GIVEN no authentication token
WHEN POST /api/recipes with valid body
THEN status = 401

## Examples

```json
// Happy path request
POST /api/recipes
Authorization: Bearer <token>
{
  "title": "Pasta Carbonara",
  "description": "Classic Roman pasta",
  "ingredients": [
    { "name": "Spaghetti", "amount": "400g" },
    { "name": "Guanciale", "amount": "200g" }
  ]
}

// Happy path response (201)
{
  "id": "a1b2c3d4-...",
  "title": "Pasta Carbonara",
  "authorId": "user-guid-...",
  "createdAt": "2026-02-12T10:30:00Z"
}
```

## Boundaries

- **Always:** Validate all input fields before persisting
- **Never:** Return 500 for validation errors (always 400)
- **Out of scope:** Image upload, ingredient search, recipe sharing
- **Assume standard:** EF Core, repository pattern, FluentValidation

## Files

- Create: `backend/src/Recipes/CreateRecipeEndpoint.cs`
- Create: `backend/src/Recipes/CreateRecipeValidator.cs`
- Test: `backend/tests/Recipes/CreateRecipeEndpointTests.cs`
```

### Why This Structure Works

| Section | Purpose | Format | Evidence |
|---------|---------|--------|----------|
| **YAML frontmatter** | Machine-parseable metadata, dependencies | YAML | Dominant pattern across Claude Code, Cursor, Codex CLI |
| **Intent** | Design rationale — WHY | Markdown prose (~50 words) | User Stories best practice; prevents hallucinated intent |
| **Contract** | Type signatures — WHAT shape | Inline DTOs | Type constraints reduce errors (directional) |
| **Acceptance Criteria** | Testable behavior — WHEN/THEN | GWT format | Highest AI-agent friendliness for verification |
| **Examples** | Concrete I/O — WHAT it looks like | JSON snippets | Significant pass@1 improvement ("More Than a Score", 2025) |
| **Boundaries** | Explicit constraints — what NOT to do | Always/Ask/Never | High-impact addition (Osmani) |
| **Files** | Exact paths — WHERE | File list | Already proven in saurun plan templates |

### Design Principles

1. **~500 words per task contract** — stays concise; quality degrades in long contexts (lost-in-middle effect)
2. **Critical info at top** — YAML metadata + intent first, details below (lost-in-middle effect)
3. **GWT over prose for acceptance** — unambiguous, maps directly to test assertions
4. **Examples are NOT redundant** — they disambiguate what GWT describes abstractly
5. **Boundaries prevent scope creep** — "Never" rules are more valuable than "Do" rules
6. **One task = one contract file** — modular, load on-demand, no monolithic specs
7. **Contract references, not repetition** — `implements: POST /api/recipes` points to architecture, doesn't restate it

---

## Format Comparison: Before vs After

### BEFORE (current plan task format)

```markdown
### Task 2: Create Recipe Endpoint

**Implements:** POST /api/recipes (Architecture §API Contract)

**Files:**
- Create: `backend/src/Recipes/CreateRecipeEndpoint.cs`
- Test: `backend/tests/Recipes/CreateRecipeEndpointTests.cs`

**Behaviors:**
- Creates recipe with valid input, returns 201 with recipe data
- Returns 400 when title is missing
- Returns 401 when unauthenticated
```

**Problems:** No examples, no explicit boundaries, behaviors are one-liners without expected values, no type signatures inline, acceptance criteria ambiguous ("recipe data" = what fields exactly?).

### AFTER (proposed contract format)

See full example above. Key additions:
- **GWT with exact values** — `body.title = "Pasta Carbonara"` not just "returns recipe data"
- **I/O examples** — concrete JSON showing exact request/response shape
- **Boundaries** — explicit Always/Never/Out-of-scope
- **Type signatures** — inline DTO shape (not full code, just contract)

---

## Integration with Pipeline v3

The proposed contract format fits the evolving pipeline:

```
User Input
    ↓
Product Spec (narrative — human writes or god-agent-plan generates)
    ↓
Architecture (typed contracts in code OR semi-structured markdown)
    ↓
Task Contracts (THIS FORMAT — one per implementation unit)
    ↓ [Validation Gate]
GWT ↔ Contract semantic check (automated)
    ↓
Implement (agent receives single task contract, ~500 words)
    ↓
Test (separate agent writes tests from GWT criteria)
    ↓
Verify (run tests, check contract compliance)
```

**Key change:** The task contract IS the spec for the implementing agent. Not a reference to a spec — the contract itself. This keeps context focused and prevents the agent from loading 10-page documents.

---

## Recommendations by Priority

### P0: Adopt immediately
1. **Add GWT acceptance criteria to plan tasks** — replace one-liner behaviors with GIVEN/WHEN/THEN + exact expected values
2. **Add I/O examples to plan tasks** — one happy-path JSON example per task
3. **Add explicit boundaries** — Always/Never/Out-of-scope per task

### P1: Adopt in pipeline v3
4. **YAML frontmatter on task contracts** — machine-parseable metadata (task ID, implements, depends-on, layer)
5. **Inline type signatures** — DTO shapes in the contract (not full code)
6. **Automated GWT ↔ Contract validation gate** — catch semantic mismatches before implementation
7. **One file per task** — modular contracts loaded on-demand

### P2: Future evolution
8. **Contract-as-Code** — C# records / TypeScript interfaces as the source of truth (compiler-enforced)
9. **GWT → test generation** — automated test scaffolding from acceptance criteria
10. **Compound learning** — agent documents implementation patterns back to `docs/solutions/` for future spec writers

---

## What We're NOT Recommending

- **Pure EARS** — too constraint-focused, misses intent and examples. Good for individual requirements within a contract, not as the overall format.
- **Pure Gherkin/Cucumber** — too verbose at scale, requires step definition maintenance overhead. GWT inline in markdown is lighter.
- **User Stories as contracts** — too abstract for implementation. Good for product specs, not for agent task contracts.
- **JSON/YAML-only schemas** — diminishing returns for behavioral description. Modern models prefer markdown.
- **Monolithic specs** — quality degrades in long contexts (lost-in-middle effect). Modular task contracts win.

---

## Sources

### Research Documents (this team)
- `research-spec-formats-landscape.md` — 11 format analyses with comparative matrix
- `research-ai-agent-contracts.md` — 12+ agent survey with empirical evidence
- `research-parseability-vs-readability.md` — structure/prose tradeoffs with quantified findings

### Key External Sources
- Addy Osmani: "How to write a good spec for AI agents" (addyosmani.com/blog/good-spec/)
- Liu et al. (2023): "Lost in the Middle" — positional bias in long contexts
- "More Than a Score" (2025, arxiv.org/abs/2508.03678): Prompt specificity → code quality correlation
- Thoughtworks: SDD tools analysis incl. Kiro, Spec-Kit, Tessl (Technology Radar Vol 33, 2025)
- Thoughtworks: Spec-Driven Development (2025)
