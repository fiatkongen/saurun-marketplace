# Pipeline v3: Evidence-Based Feature Development Pipeline

**Date:** 2026-02-12
**Status:** Design draft. Peer-reviewed by 4 research agents (2026-02-12). Corrections applied.
**Predecessor:** God-Agent v1.x (monolithic, TDD-based, horizontal decomposition)
**Research basis:** `2026-02-11-god-agent-v3-research.md`, `compound-engineering-research.md`
**Peer review:** Citation validation, counter-evidence search, architecture critique, competitive analysis (4 parallel agents)

---

## 1. Problem Statement

The god-agent (v1.x) has three fundamental problems:

- **Token waste.** 7-phase monolithic pipeline with horizontal decomposition. Each phase dumps full context (autonomous preamble, state, architecture doc, contracts, behaviors, previous outcomes) into every subagent. Execution accounts for ~80% of token spend despite research showing execution should be ~30%.
- **Mediocre output.** Horizontal decomposition (backend domain → backend API → frontend state → frontend pages) means the entire backend is built before any frontend starts. No integration feedback until Phase 4. Specs are verbose, exceeding the 1,000-3,500 token sweet spot. Agent prompts routinely exceed 5,000 tokens.
- **Worthless tests.** The agent writes implementation code, then writes tests for that code. Research conclusively shows agent-written tests are observational — they verify what the code *does*, not what it *should do*. When the agent implements a bug, the test asserts the buggy behavior. The test passes. The bug ships. False positive tests are worse than no tests because they create an illusion of safety.

---

## 2. Research-Backed Principles

Every design decision traces to empirical evidence. These are the non-negotiable constraints.

| # | Principle | Evidence | Implication |
|---|-----------|----------|-------------|
| 1 | **Specs are the product, not code** | Up to 50% error reduction from human-refined specs (ceiling, not typical; studies are nascent). The 80% problem: agents sprint through 80% then stall from poor specs. | Invest 70% of pipeline effort in spec quality. Spec/system design failures cause 44.3% of MAS failures — the largest category. |
| 2 | **Feature-first decomposition** | Doubling features in one prompt: o1-preview drops 95%→65%, Claude 88%→75%. VSA is most AI-friendly architecture for token efficiency and isolation. | Decompose by feature (vertical), not by layer (horizontal). Each feature is an independent pipeline run. Note: VSA trades code duplication for isolation; respect domain layer boundaries within each slice. |
| 3 | **Small context, high signal** | Optimal range is task-dependent: 500-2,000 for simple dispatches, 1,000-3,500 for complex constraint modeling (Particula). Quality drops measurably at 4,000+. Lost-in-middle effect degrades middle content. | Spec writer: 1,000-3,500 tokens. Simple dispatches (test writer, compound): 500-2,000. Critical info at top and bottom of every prompt. |
| 4 | **Type constraints reduce errors 50%+** | Type-constrained generation cuts compilation errors 50%+. TypeChat (Microsoft) shows schema engineering outperforms prompt engineering. Strongest-evidenced claim in this document. | Generate contract types as compiled code, not markdown. Implementers import types — the compiler enforces the contract. Note: compilation prevents type drift but not semantic errors (wrong field names, wrong validation ranges). Requires additional semantic validation. |
| 5 | **Tests as input help; tests as process don't** | Tests alongside problem statements: +7-12% pass rate (varies by dataset: +12% MBPP, +8.5% HumanEval, +7.7% CodeChef). TDD by agent: +2.6%, costs +19.8% more tokens. Suppressing tests: saves 32-49% tokens, -1.8-2.6% quality. 83.2% of tasks: same outcome regardless of test-writing intervention. | Include acceptance criteria (GWT) in specs as "tests-as-input." Don't mandate TDD as a process. Note: blanket rejection of TDD may be too strong — light TDD guardrails alongside specs-as-input could be a viable hybrid. |
| 6 | **Agent-written tests are observational** | "Agent-written tests often behave more like a reproduced software development lifecycle routine than a dependable source of help." Only ~2-4% of agent assertions are relational/complex checks. Paper recommends better test instruction, not abandonment. | Same agent writing code + tests = tautological. Separate the contexts. Test assertions must derive from spec, not from implementation. Note: the separate-context test writer approach is novel and not yet validated by literature — treat as experimental, measure empirically. |
| 7 | **Role separation improves quality (with caveats)** | Self-Collaboration (analyst + coder + tester): +29.9-47.1% Pass@1 — but on ChatGPT (2023). On frontier models (GPT-5, Gemini 2.0), ~80% of cases are ties (<3% improvement). Cost penalty: 4-220x more input tokens, 2-12x more output tokens. Hybrid routing: 2% better at 50% cost. | Use specialized agents (backend, frontend, reviewer) but don't over-fragment. Consider routing/cascading for cost efficiency. Start with fewer agents, add incrementally based on measured value. |
| 8 | **Spec quality and integration are the top MAS failure modes** | Spec/system design issues: 44.3% of MAS failures (#1). Inter-agent misalignment: 31.4% (#2). Task verification: 23.5% (#3). Source: Why Do MAS Fail?, 1,642 traces, 7 frameworks. | Spec quality (Steps 1-2) prevents the #1 failure mode. Shared contracts (Step 2) prevent the #2 failure mode. Per-slice verification (Step 6) catches the #3 failure mode. |
| 9 | **LLMs are better evaluators than generators** | LLM-as-judge research shows evaluation is a constrained, higher-accuracy task. Agents with rubrics produce reliable ratings. Anthropic confirms: "Give Claude a way to verify its work" is highest-leverage. Note: no specific paper cited; concept well-established but lacks single definitive study. | Use agents for review/rating (with GWT as rubric) rather than expecting them to generate perfect code. |
| 10 | **Knowledge must accumulate** | Compound Engineering: each problem solved feeds back into future planning. Without this, you solve the same problems repeatedly. Validated by Factory.ai (org memory), Kiro (bidirectional spec sync), Anthropic (memory systems). | Add a compound step that documents learnings for future sessions. |

---

## 3. The Pipeline

Seven steps. Each step has a single responsibility, clear input/output, and narrow token budget.

```
[1. Decompose]        PRD → feature slices (vertical)
[2. Spec Writer]      Feature slice → Contract code + GWT criteria
[2.5 Spec Validation] Mechanical check: GWT completeness + contract semantic correctness
[3. Implement]        Backend + Frontend agents (contract-as-code)
[4. Review]           Parallel specialized reviewers + conflict arbiter
[5. Test Writer]      GWT criteria → integration/acceptance tests
[6. Verify]           Run tests + build + cross-feature smoke
[7. Compound]         Document learnings → knowledge base
```

Steps 2-6 repeat per feature slice. Step 6 includes cross-feature integration smoke after all slices complete. Step 7 runs once after all slices.

### Step 1: Decompose

| | |
|---|---|
| **Input** | PRD, feature spec, or rough idea (post-planning/interview) |
| **Output** | N independent feature slices, ordered by dependency |
| **Who** | Orchestrator or human |
| **Token budget** | N/A (orchestrator-level decision) |

Decompose by feature, not by layer. "Recipe sharing app" becomes: User Auth, Recipe CRUD, Recipe Search, Favorites. Each slice is an independent pipeline run. Dependent slices are sequenced (Auth before CRUD).

### Step 2: Spec Writer (The Linchpin)

| | |
|---|---|
| **Input** | Feature slice description + project context + existing Contracts/ types + knowledge base (docs/solutions/) |
| **Output** | (1) Compiled C# DTOs + interfaces in Contracts/ assembly, (2) GWT acceptance criteria markdown |
| **Who** | Single agent (Opus) |
| **Token budget** | Input ~800-1,200 tokens. Output ~1,200-1,800 tokens total. |
| **Gate** | `dotnet build Contracts/` passes |

This is where 70% of quality is determined. The agent designs the API surface and produces two artifacts:

**Artifact 1: Contract Code** — Real C# files in a shared Contracts/ project:
- DTOs as records with DataAnnotation validation attributes
- Route constants (static strings for endpoint paths)
- Error response shape
- No domain entities, no service interfaces, no implementation details

The implementer imports these types. The compiler enforces the contract. Type drift is mechanically impossible.

**Artifact 2: GWT Acceptance Criteria** — Precise, falsifiable scenarios:
```
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
```

Quality heuristic: 2-4 GWT scenarios per endpoint (happy path + error cases + edge cases). Every assertion specifies exact values — status codes, field names, error shapes. No room for agent interpretation.

**Knowledge base integration:** The spec writer reads `docs/solutions/` for past learnings relevant to this feature type. If a previous project documented "SQLite doesn't support ALTER COLUMN," the spec writer accounts for it in constraints.

### Step 2.5: Spec Validation Gate

| | |
|---|---|
| **Input** | GWT criteria + Contracts/ code from Step 2 |
| **Output** | Pass/fail with specific issues |
| **Who** | Validation agent (Sonnet — mechanical, not creative) |
| **Token budget** | ~800-1,200 tokens |
| **Gate** | All checks pass (blocks Step 3) |

Mechanical rubric-based validation. The spec writer is the single point of failure — this gate prevents garbage from propagating through the entire pipeline.

**GWT Completeness Checks:**
- Every endpoint has a happy path GWT scenario
- Every POST/PUT endpoint has at least one validation error scenario (400)
- Every auth-protected endpoint has an unauthorized scenario (401/403)
- Every endpoint with path params has a not-found scenario (404)
- Every GWT assertion specifies exact values (no "some", "various", "appropriate")
- Every GWT scenario has a GIVEN, WHEN, and THEN clause

**Contract Semantic Checks (cross-reference GWT ↔ Contracts/):**
- Every field name referenced in GWT assertions exists in the corresponding DTO
- Every status code in GWT matches the endpoint's documented responses
- Every validation rule in GWT (e.g., "title 1-200 chars") has a matching DataAnnotation on the DTO
- Every route in GWT matches a constant in Routes.cs
- No DTO has fields that are never referenced in any GWT scenario (dead fields)

**On failure:** Route specific issues back to spec writer for targeted fixes (not full re-generation). Up to 2 retries, then escalate to human.

This step costs ~1,000 tokens per feature but prevents the #1 failure mode (spec/system design issues = 44.3% of MAS failures).

### Step 3: Implement

| | |
|---|---|
| **Input** | Contract types (compiled) + GWT criteria (markdown) |
| **Output** | Working implementation code |
| **Who** | Backend agent + Frontend agent (separate dispatches) |
| **Token budget** | ~1,500-2,500 tokens per agent (contract + GWT + constraints) |
| **Gate** | `dotnet build` + `npm run build` pass |

Two separate, parallel dispatches:

**Backend Implementer:**
- Receives: GWT criteria + Contracts/ types (imports, doesn't create DTOs)
- Writes: Endpoint implementations, domain entities, database context, migrations
- Does NOT receive: frontend code, component trees, UI requirements
- Tests: may write some tests naturally — we don't force or suppress this

**Frontend Implementer:**
- Receives: GWT criteria + TypeScript types (generated from Contracts/ via OpenAPI) + endpoint routes
- Writes: Components, stores, pages, API client
- Does NOT receive: backend implementation, EF Core details, domain model
- Tests: same — may write some, we don't force or suppress

Neither agent sees the other's implementation. Both implement against the same contract.

### Step 4: Review (Parallel Specialized Agents)

| | |
|---|---|
| **Input** | Implementation code + GWT criteria + contract |
| **Output** | Structured findings (P1 Critical / P2 Important / P3 Minor) |
| **Who** | 3-5 parallel specialized review agents |
| **Token budget** | Each reviewer: ~2,000-3,000 tokens (diff + contract + criteria) |
| **Gate** | Zero P1 findings (P1 = blocks progress) |

Instead of a single rating agent, parallel specialists each check one concern:

| Reviewer | Focus |
|----------|-------|
| **Contract Compliance** | Does the implementation match every GWT criterion? Status codes, response shapes, error formats, validation rules. This is the core verification. |
| **Security** | OWASP top 10, auth bypass, injection, secrets exposure. |
| **Performance** | N+1 queries, missing indexes, unnecessary allocations, frontend bundle size. |
| **Code Quality** | SOLID, naming, unnecessary complexity, pattern consistency with codebase. |

Each reviewer produces pass/fail per finding with severity (P1/P2/P3) and specific code references.

**Conflict Resolution:** When reviewers produce contradictory findings (e.g., Security P1 "add input sanitization" vs. Code Quality P2 "unnecessary — ORM handles this"), an arbiter resolves using priority rules:

Priority order: **Security > Contract Compliance > Correctness > Performance > Code Quality**

The arbiter (orchestrator or dedicated Sonnet agent) reads all findings, resolves conflicts with the priority order, and produces a unified fix list. Cost: ~500-1,000 tokens per feature.

**P1 findings** go back to the implementing agent with fix instructions. P2/P3 are logged but don't block.

**Incremental rollout:** Start with Contract Compliance + Security (highest ROI). Add Performance and Code Quality incrementally, measuring whether each adds signal vs. noise.

This replaces the single rating agent because:
- GWT compliance alone misses security, performance, and architectural issues
- Specialized agents with narrow focus produce higher-quality findings than one agent checking everything
- Parallel execution means no additional latency

### Step 5: Test Writer (Post-Implementation, From GWT)

| | |
|---|---|
| **Input** | GWT criteria + Contracts/ types + review report (pass/fail per GWT) |
| **Output** | Integration/acceptance test files |
| **Who** | Dedicated test-writing agent |
| **Token budget** | ~1,500-2,500 tokens (GWT + types + review summary) |
| **Gate** | Tests compile |

The test writer is a separate context from the implementer. It receives:
- **GWT criteria** — source of assertion values (what to test)
- **Compiled types from Contracts/** — source of imports (how to reference types)
- **Review report** — which GWT criteria passed (confidence signal)

Critical constraint: **The test writer does NOT read implementation files.** It imports types from Contracts/ and writes tests against the HTTP/component boundary. Assertion values come from the GWT spec, not from observing the code.

**Enforcement (not just instruction):** The test writer skill uses `allowed-tools` in its frontmatter to deny `Glob`, `Grep`, and `Read` access to implementation directories. Contract types are provided in the prompt (copy-pasted from Contracts/ files). The orchestrator verifies post-dispatch that no forbidden tool calls occurred. If violated: reject output, re-run with stricter sandboxing.

Tests are contract-boundary tests only:
- Backend: HTTP request → HTTP response (using WebApplicationFactory)
- Frontend: render component → interact → check DOM output (using RTL + MSW)

No unit tests of internal methods. Those are the implementer's concern (if the implementer writes them naturally, fine).

**Why post-implementation?**
- Types exist → tests compile without guessing at imports
- Test infrastructure exists (fixtures, helpers, factories)
- The test writer translates GWT to code mechanically — it doesn't need to "understand" the implementation
- Two independent verification layers: review agents checked the code, tests check the runtime behavior

**The false-positive mitigation:**
For a false positive to survive, BOTH the review agents AND the test writer must independently get the same GWT criterion wrong. The review agents read the code. The test writer runs the code. Different verification methods against the same spec.

### Step 6: Verify

| | |
|---|---|
| **Input** | All code + all tests |
| **Output** | Pass/fail with specific failures |
| **Who** | Mechanical (no agent) |
| **Token budget** | Zero (shell commands) |
| **Gate** | All checks pass |

Purely mechanical verification:
```bash
dotnet build                    # compilation
dotnet test                     # backend tests
npm run build                   # frontend compilation
npm test                        # frontend tests
# Optional: E2E smoke test against running app
```

**Fix loop protocol:**
1. If a GWT-derived test fails → the implementation has a bug (not the test). Route back to the implementing agent with: (a) failure description, (b) exact code location, (c) GWT criterion it violates, (d) suggested fix from review report if available.
2. If an implementer-written test fails → could be a bad test or a bug. Route to implementer to diagnose.
3. Max 2 retries per agent. If still failing → dispatch a debugging agent (Sonnet) with error logs, test output, and relevant code for independent diagnosis.
4. If debugging agent also fails (2 attempts) → escalate to human. Log to a "stuck features" report with full context.

**Cross-feature integration smoke (after all slices complete):**
After all individual feature slices pass verification, run a combined smoke test exercising features in combination. Example: (1) Create user (Auth) → (2) Create recipe (CRUD) → (3) Search recipes (Search) → (4) Favorite a recipe (Favorites). If any step fails, route back to the affected feature's implementer with the specific failure.

**Feedback loop:** If verification reveals missing edge cases not covered by GWT criteria, route the gap back to the spec writer as a refinement for future slices. This prevents the same spec gap from recurring.

### Step 7: Compound (Knowledge Capture)

| | |
|---|---|
| **Input** | Completed feature: spec, implementation, review findings, test results |
| **Output** | Solution document in `docs/solutions/` |
| **Who** | Compound agent |
| **Token budget** | ~1,000-2,000 tokens |
| **Gate** | Document exists with required sections |

After verification passes, document what was learned:

```yaml
---
title: "Recipe CRUD: SQLite GUID handling"
category: database-issues
module: recipes
symptom: "EF Core generates empty GUIDs with SQLite"
root_cause: "SQLite lacks native GUID type; EF Core ValueGenerator needed"
---

## Problem
[What went wrong]

## Solution
[How it was fixed]

## Prevention
[What to do differently next time]
```

Future spec writer sessions read `docs/solutions/` and incorporate past learnings. This is the compound effect — each project makes the next one easier.

---

## 4. Key Design Decisions

### Contract-as-Code (Not Markdown)

**Decision:** The API contract is compiled C# code in a shared Contracts/ assembly, not a markdown document.

**Why:** Markdown contracts can drift — the implementer interprets "returns a recipe" differently than the spec intended. Compiled types prevent structural drift — if the implementer tries to return a different shape, the code doesn't compile. Type constraints reduce errors 50%+ (strongest-evidenced claim in this document).

**Limitation (from peer review):** Compilation prevents type drift but NOT semantic errors. A DTO with `List<Ingredient> Items` compiles even though the spec says `ingredients`. Wrong validation ranges (`[Range(0, 100)]` instead of `[Range(1, 10)]`) compile fine. Step 2.5 (Spec Validation Gate) addresses this with semantic cross-checking of GWT criteria against Contracts/ code. Additionally, production contract testing (e.g., Pact) may be needed to catch runtime divergences that compile-time checks miss.

**Structure:**
```
Solution/
├── Contracts/           ← Spec writer produces this
│   ├── DTOs/            ← Request/response records with validation
│   ├── Routes.cs        ← Endpoint path constants
│   └── Contracts.csproj
├── Api/                 ← Backend implementer works here
│   └── Api.csproj → references Contracts
├── Tests/               ← Test writer works here
│   └── Tests.csproj → references Contracts + Api
└── frontend/            ← Frontend implementer works here
    └── (types generated from OpenAPI)
```

### No Mandatory TDD (Tests as Output, Not Process)

**Decision:** Agents are not mandated to write tests first or follow red-green-refactor. TDD is not banned — if an implementer naturally writes tests alongside code, that's fine. But the pipeline doesn't depend on it.

**Why:** Research shows TDD by agents costs +19.8% more tokens with only +2.6% quality improvement across 83.2% of tasks that show no difference either way. Suppressing tests entirely saves 32-49% tokens with only -1.8-2.6% quality drop. Agent-written tests trend observational.

**What instead:** GWT acceptance criteria in the spec serve as the "tests-as-input" that research shows helps (+7-12%). Implementers write code to satisfy the criteria. A separate test writer (Step 5) produces the actual test code post-implementation.

**Nuance (from peer review):** Blanket rejection of TDD may discard a useful feedback loop. Light TDD guardrails (implementer writes a quick smoke test while coding for fast feedback) alongside the full GWT-based test writing in Step 5 could be a viable hybrid. This is worth empirical comparison.

### Post-Implementation Test Writing From Separate Context

**Decision:** Tests are written by a dedicated agent after implementation, not by the implementing agent.

**Why:** Same agent writing code + tests = tautological verification. The agent implements a bug, then writes a test that asserts the buggy behavior. Separating the contexts means:
- The test writer derives assertion values from the GWT spec (intent)
- The implementer's bugs can't influence the test writer's assertions
- Two independent verification layers: review agents check code, tests check runtime

**Risk:** The test writer is still an agent and could produce observational tests if it reads implementation files. Mitigated by `allowed-tools` restrictions (see Step 5) and providing Contracts/ types in the prompt rather than via file access.

**Experimental status (from peer review):** The separate-context test writer approach is novel and not yet validated by literature. The cited research validates tests-as-input (specs + GWT improve generation by 8-18%) but doesn't specifically study "separate agent writes tests from spec post-implementation." Treat as experimental. Measure: false-positive rate, coverage adequacy, time cost on first 3-5 features.

### Parallel Specialized Review (Not Single Rating Agent)

**Decision:** 3-5 specialized review agents running in parallel, each focused on one concern.

**Why:** A single agent checking everything suffers from attention dilution. Specialized agents with narrow focus produce higher-quality findings. Compound Engineering validates this with 13+ parallel reviewers in production.

**Our reviewers:** Contract compliance, security, performance, code quality. Extensible — add more as patterns emerge.

### Compound Step (Knowledge Accumulation)

**Decision:** After each completed feature, document learnings to `docs/solutions/` for future sessions.

**Why:** Without knowledge accumulation, every project starts from zero. The compound step means the system gets better over time. Adapted from Compound Engineering (Every Inc), which attributes their 5x velocity improvement partly to this pattern.

### Vertical Slices (Not Horizontal Layers)

**Decision:** Decompose by feature (vertical), not by layer (horizontal). Each feature contains both backend and frontend, sharing a contract.

**Why:** Current god-agent builds all backend, then all frontend. Integration bugs surface in Phase 4. Vertical slices verify per feature — if Recipe CRUD works end-to-end, move to Search. Integration is tested at each step, not at the end.

Research: VSA is most AI-friendly architecture. All SDD tools assume feature-scoped specs. 23.5% of MAS failures are task verification caught too late.

---

## 5. Open Questions

### Can an agent reliably produce GWT criteria at the required precision?
The entire pipeline depends on the spec writer producing falsifiable, mechanically-translatable acceptance criteria. This is a design task requiring edge case thinking, validation rule completeness, and authorization boundary awareness. Needs empirical testing — try the spec writer on 3-5 feature types and evaluate GWT quality.

### Frontend type generation workflow
The Contracts/ assembly is C#. Frontend needs TypeScript. Proposed: `dotnet build` → Swagger/OpenAPI generation → `openapi-typescript` or NSwag → TS types. Needs to be validated that this pipeline produces usable types without manual intervention.

### Complexity routing
Should simple features (add a field, rename an endpoint) get the full pipeline? The "Why Not Both?" paper found up to 88.1% cost reduction from confidence-guided cascading (typical: 20-60% depending on routing strategy). A lightweight path (spec → implement → verify, skip review/test-writer) could handle simple changes. Decision heuristic: if spec is <500 tokens and no new auth boundaries or schema changes, use lightweight path.

### Shared infrastructure (the "foundation slice")
Auth middleware, global error handling, database setup, CORS config span features. Should there be a "foundation" slice that runs first with a lighter pipeline? Or is this handled by the scaffold step?

### Model routing
Which model for which step? The spec writer needs high reasoning (Opus). The test writer is more mechanical (Sonnet). The review agents need pattern recognition (Sonnet or Opus). The compound step is straightforward (Sonnet or Haiku).

### Naming
What do we call this pipeline? It replaces god-agent. Options discussed but not decided: forge, shipwright, architect. Or something entirely different that reflects the spec-first, contract-driven philosophy.

### How does this become skills/agents?
The pipeline is 7 conceptual steps. How do these map to actual Claude Code skills, agents, and commands? Options:
- One orchestrator skill that runs the full pipeline (like /lfg)
- Composable individual skills (spec-writer, contract-verifier, etc.) that can be run independently
- Both: individual skills + an orchestrator that chains them

### Fix loop mechanics (partially resolved)
Protocol defined in Step 6: max 2 retries → debugging agent → human escalation. Open: does the implementer get its full previous context back (resume), or start fresh with just the fix instruction + relevant code? Resume preserves context but costs tokens. Fresh start is cheaper but may re-introduce solved problems.

### E2E testing
The pipeline has integration tests from the test writer + cross-feature smoke in Step 6. Where do full E2E tests (Playwright) fit? After all feature slices? Per slice? Only for features with UI flows? The Compound Engineering research suggests E2E tests are central for validating holistic AI comprehension — consider per-feature E2E for features with UI flows.

### Contract evolution across feature slices
When Feature 2's spec writer runs, what happens if it needs to extend a type from Feature 1? Rules needed: (a) Contracts/ is additive (fields added, not removed), (b) new features can reference but not modify existing types, (c) if extension is needed, create a feature-specific DTO that wraps the base type. Document in CLAUDE.md.

### Frontend type generation robustness
The Swagger → OpenAPI → TypeScript type chain has 3 failure points (misconfigured routes, enum format mismatches, unsupported features). Alternatives: (a) spec writer also generates TS types alongside C# DTOs, (b) test the chain on day 1 with a minimal contract, (c) build a custom type generator. Needs evaluation.

### Bidirectional spec-code sync (from Kiro)
Our pipeline is one-directional: spec → code. Kiro demonstrates bidirectional sync where code changes can update specs. Should our compound step also update GWT criteria when edge cases are discovered during implementation? This would prevent the same spec gap from recurring.

### Formal verification layer (future consideration)
Theorem (YC, $6M) and MIT vericoding research show LLMs can generate formal proofs (40-60% success in Dafny). GWT criteria could generate Dafny preconditions as a machine-checked alternative to agent-based review. Not for v1, but worth tracking as the tooling matures.

### Anthropic Agent Teams integration (future consideration)
Opus 4.6 ships TeammateTool for native multi-agent orchestration. Our custom orchestrator may be partially redundant. Evaluate whether Steps 3-4 (implement + review) can use Agent Teams instead of manual Task dispatches.

---

## 6. Influences and Attribution

### From Research (2026-02-11 findings)
- Tests-as-input vs tests-as-process distinction (TDD for Code Gen, ASE 2024)
- Agent-written tests are observational (Rethinking Agent-Generated Tests, Feb 2026)
- Optimal prompt length is task-dependent: 500-2,000 simple, 1,000-3,500 complex (Particula, WebApp1K)
- Type constraints reduce errors 50%+ (Type-Constrained Code Gen) — strongest claim
- Feature-first decomposition (VSA research, SWE-Bench Pro)
- MAS failures: 44.3% spec/design (#1), 31.4% inter-agent misalignment (#2), 23.5% verification (#3) (Why Do MAS Fail?)
- Role separation +29.9-47.1% on ChatGPT (2023); ~80% ties on frontier models (Self-Collaboration + Why Not Both?)

### From Compound Engineering (Every Inc)
- The compound step (knowledge accumulation via docs/solutions/)
- Plans as primary artifacts (code is disposable, plans represent thinking)
- Parallel specialized review agents (encode taste into systems)
- Spec-flow analysis (validate completeness before execution)
- Protected knowledge artifacts (never delete solution docs)
- Progressive context loading (grep-first, read-second)

### Divergences from Compound Engineering
- Contract-as-code (they use markdown plans; we use compiled C# types)
- Post-implementation test writing from GWT (they use same-agent testing)
- Frontend/backend contract separation (they're Rails monolith; we have API boundary)
- Fewer agents per step (they use 13-40+; we target 3-5 for cost efficiency)

### From Competitive Analysis (2026-02-12 peer review)
- Bidirectional spec-code sync (Kiro) — specs should evolve, not be write-once
- Semantic code indexing (Auggie/SWE-bench Pro) — context retrieval > model capability
- Formal verification potential (Theorem, MIT vericoding) — machine-checked proofs as future alternative to agent review
- Constrained generation for boilerplate (DOMINO) — prevent errors during generation, not just catch them after
- Anthropic Agent Teams (TeammateTool) — native multi-agent may replace custom orchestration

---

## 7. Peer Review Summary (2026-02-12)

Four parallel research agents critiqued this design. Key corrections applied:

### Citation Corrections
| Original Claim | Correction |
|----------------|------------|
| "50% error reduction from specs" | "Up to 50%" — ceiling, not typical. Studies are nascent. |
| "1,000-3,500 token sweet spot" | Task-dependent: 500-2,000 for simple, 1,000-3,500 for complex. |
| "Integration is #1 MAS failure mode" | #2 (31.4%). Spec/system design is #1 (44.3%). |
| "Role separation +29.9-47.1%" | On ChatGPT (2023). Frontier models: ~80% ties, <3% improvement, 4-220x cost. |
| "88.1% cost reduction from cascading" | Up to 88.1% in optimal scenarios. Typical: 20-60%. |

### Architecture Issues Identified and Addressed
| Issue | Severity | Resolution |
|-------|----------|------------|
| Spec writer SPOF — no validation gate | CRITICAL | Added Step 2.5: Spec Validation Gate |
| Compilation gate catches syntax, not semantics | HIGH | Added semantic cross-check in Step 2.5 |
| Test writer file access is soft constraint | HIGH | Added `allowed-tools` enforcement |
| Reviewer conflicts have no tiebreaker | HIGH | Added conflict arbiter with priority rules |
| Fix loop undefined | MEDIUM | Added protocol: 2 retries → debugger → human |
| No cross-feature integration test | MEDIUM | Added to Step 6 |
| Frontend type generation fragile | HIGH | Added to open questions with alternatives |
| Contract evolution across slices undefined | MEDIUM | Added to open questions with rules |

### Counter-Evidence Summary
| Our Position | Counter-Evidence Strength | Adjustment |
|-------------|--------------------------|------------|
| No TDD for agents | Strong | Softened to "no mandatory TDD." Light TDD alongside specs may be viable hybrid. |
| Vertical slices > horizontal | Moderate | Added caveat: respect domain layer boundaries within slices. |
| Agent tests worthless | Strong agreement on problem, moderate on our solution | Marked test writer as experimental. Measure empirically. |
| Contract-as-code | Strong (contracts necessary but insufficient) | Added runtime contract validation note. |
| Parallel specialized reviewers | Moderate | Changed to incremental rollout (start with 2, add based on measured value). |

### Ideas We're Missing (Not Yet Incorporated)
- Formal verification layer (Dafny proofs from GWT — future consideration)
- Semantic code indexing for decomposition (Auggie pattern — engineering effort)
- Constrained generation for boilerplate code (DOMINO — research stage)
- Spec-as-source where code is regenerated from specs (Tessl pattern — radical, worth watching)

---

## 8. Next Steps

1. **Name the pipeline.** Pick a name, create the skill/agent structure.
2. **Build the spec writer + validation gate.** This is the linchpin AND the #1 failure mode. Start here, test on 3-5 feature types. Measure GWT quality, semantic accuracy, and token cost.
3. **Build the orchestrator.** Chains the steps, manages state, handles fix loops and feedback.
4. **Build Contract Compliance + Security reviewers.** Start with 2 (highest ROI). Add performance/code quality incrementally based on measured signal-to-noise.
5. **Build the test writer.** Post-implementation GWT-to-test translator. Mark as experimental. Measure false-positive rate vs. implementer-written tests.
6. **Build the compound step.** Knowledge capture and retrieval. Include implementer-annotated learnings (LEARNING: comments in code).
7. **Empirical testing.** Run the pipeline on a real feature. Measure:
   - Token cost per feature (vs. god-agent v1.x on same feature)
   - Output quality (working code, passing tests, no regressions)
   - Test false-positive rate (Step 5 tests vs. manually-written tests)
   - Per-reviewer value (which reviewers catch real issues vs. noise?)
   - Spec writer GWT precision (how often does Step 2.5 catch gaps?)
   - Time-to-working-code (wall clock)
