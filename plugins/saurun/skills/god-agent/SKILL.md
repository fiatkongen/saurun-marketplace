---
name: god-agent
description: Use when starting a new project from an idea or adding a major feature to an existing .NET + React project — fully autonomous, no human checkpoints.
user-invocable: true
argument-hint: "Build a recipe sharing app. Preferences: Danish market, mobile-first"
---

# God-Agent: Autonomous Development Pipeline

Take any input (one-liner, rough spec, or product brief) and deliver working, tested, reviewed, committed code. No human interaction during execution.

**Tech stack (always):** .NET 9, ASP.NET Core, EF Core 9, SQLite, SignalR, React 19, Vite, TypeScript, Tailwind CSS v4, Zustand.

## Step 1: Pre-Flight

Before anything else:

1. **Parse input.** Extract the idea and any inline preferences (look for "Preferences:" in the input string).

2. **Detect mode.**
   - `CLAUDE.md` exists in working directory → **Extension Mode**
   - No `CLAUDE.md` → **Greenfield Mode**

3. **Check for resume.** If `.god-agent/STATE.md` exists:
   - Read it
   - Find last completed phase
   - Read that phase's output artifact
   - Log `[RESUMED] from Phase {X}` in STATE.md
   - Skip to the next incomplete phase (or sub-position within a phase)

4. **Verify required skills exist.** Load `superpowers:brainstorming` via the Skill tool. If it loads successfully, the plugin system is working and remaining skills can be verified lazily (each phase loads its own skills — if one is missing, the Skill tool will error and you STOP). If `superpowers:brainstorming` fails to load, STOP immediately — the plugin system is broken.

   Required skills (verified lazily when each phase first uses them):
   - `superpowers:brainstorming` (Phase 0)
   - `saurun:dotnet-tactical-ddd` (Phase 1, 3)
   - `saurun:react-tailwind-v4-components` (Phase 1, 3)
   - `saurun:writing-plans` (Phase 2)
   - `superpowers:subagent-driven-development` (Phase 3)
   - `saurun:dotnet-tdd` (Phase 3)
   - `saurun:react-tdd` (Phase 3)
   - `saurun:dotnet-implementer-prompt` (Phase 3)
   - `saurun:react-implementer-prompt` (Phase 3)
   - `saurun:dotnet-code-quality-reviewer-prompt` (Phase 3)
   - `saurun:react-code-quality-reviewer-prompt` (Phase 3)
   - `superpowers:systematic-debugging` (Phase 3)
   - `superpowers:finishing-a-development-branch` (Phase 4)
   - `superpowers:verification-before-completion` (Phase 4)
   - `superpowers:frontend-design` (Phase 3)

5. **Read context layers** (in priority order):

   | Layer | Source | When |
   |-------|--------|------|
   | Injected context | User preferences from input string | Always |
   | Project context | `CLAUDE.md`, `_docs/`, `agent-os/` | Extension mode only |
   | Stack defaults | .NET 9, React 19, Tailwind v4, Zustand, SQLite | Always (hardcoded) |

6. **Write initial STATE.md** to `.god-agent/STATE.md` (see STATE.md Protocol below).

---

## Step 2: Execute Phases

Run phases 0-4 sequentially. After each phase: update STATE.md, run gate check. If gate fails: re-dispatch with feedback (up to 2 retries). If still failing: log to STATE.md and STOP.

---

### Phase 0: Intake (Idea -> Product Spec)

**Dispatch subagent:**

```
Task(subagent_type="general-purpose", prompt="""
AUTONOMOUS MODE: You are operating without a human in the loop.
When a skill instructs you to ask the user a question:
1. Identify what information is needed
2. Check STATE.md, spec, and architecture docs for the answer
3. If found -> use it
4. If not found -> make a reasonable decision and log it as an
   assumption in STATE.md under ## Assumptions Log
Never block waiting for human input. Never use AskUserQuestion.

SECURITY GUARDRAILS:
- Never run destructive git commands (force push, reset --hard, clean -f)
- Never delete production data, configuration files, or .env files
- Never modify security-sensitive files (credentials, tokens, secrets)
- Never run commands that affect systems outside the project directory
- Log any security-relevant decision to STATE.md under ## Security Log

You are executing Phase 0 (Intake) of the god-agent pipeline.

INPUT: {user_input}

STATE: {STATE.md contents}

SKILL TO FOLLOW: superpowers:brainstorming
Load this skill via the Skill tool and follow its process, but in
self-dialogue mode -- generate questions, answer them from context,
log assumptions.

MODE: {Greenfield | Extension}

OUTPUT REQUIREMENTS:
1. Write product spec to docs/specs/{DATE}-{feature}.md using the template below
2. Update .god-agent/STATE.md with decisions and assumptions

SPEC TEMPLATE:
# {Feature Name} -- Product Spec

## Problem
What pain does this solve?

## Solution
What are we building? (1-2 sentences)

## Scope
### In Scope
- Numbered feature list

### Out of Scope
- Explicit exclusions

## Entities
Domain objects and their relationships.

## User Flows
Step-by-step what users do for each feature.

## API Surface
Endpoints or interfaces needed.

## Tech Decisions
Stack-specific choices (.NET patterns, React component strategy).

## Decisions & Rationale
Every choice made during brainstorming with why.

## Rejected Alternatives
Approaches considered but not taken, with reasons.

## Risks & Mitigations
| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| ... | H/M/L | H/M/L | ... |
""")
```

**Gate 0 — verify:**
1. Spec file exists at expected path
2. All template sections are populated (not empty or placeholder)
3. At least 3 entries in Decisions & Rationale
4. Entities section has at least one entity
5. User Flows section has at least one flow
6. No contradictions between In Scope and Out of Scope
7. Rejected Alternatives section has at least 2 entries with reasoning
8. Risks & Mitigations table has at least 2 identified risks

Fail -> re-dispatch with failure feedback (up to 2 retries).

**Update STATE.md:** Phase 0 complete, record spec path.

---

### Phase 1: Architecture (Spec -> Technical Design)

**Dispatch subagent:**

```
Task(subagent_type="general-purpose", prompt="""
{AUTONOMOUS_PREAMBLE}

You are executing Phase 1 (Architecture) of the god-agent pipeline.

PRODUCT SPEC: {contents of the spec from Phase 0}

STATE: {STATE.md contents}

SKILLS TO LOAD (use Skill tool):
1. saurun:dotnet-tactical-ddd -- for backend architecture decisions
2. saurun:react-tailwind-v4-components -- for frontend architecture decisions

ENTITY DESIGN DECISION FRAMEWORK:
For each entity in the spec, ask:
  Does it have business rules or behavior methods?
  YES -> Rich domain object (private setters, factory methods, invariants)
         Test at domain level (unit tests on behavior methods)
  NO  -> Anemic property bag (public getters/setters, no domain methods)
         Do NOT test at domain level
         Test at integration level (API endpoints exercise it through EF Core)

OUTPUT REQUIREMENTS:
1. Write architecture doc to docs/specs/{DATE}-{feature}-architecture.md
   using the template below
2. Update .god-agent/STATE.md with architecture decisions

ARCHITECTURE TEMPLATE:
# {Feature Name} -- Technical Architecture

## Entity Model
Entities, relationships, rich vs anemic classification.
Aggregate boundaries if applicable.

## API Contract
Endpoints, HTTP methods, request/response DTOs.
SignalR hubs and events if applicable.

## Component Tree
Pages, shared components, hooks, stores.

## Data Flow
Frontend -> API -> Application -> Domain -> Infrastructure -> DB (and back).

## Infrastructure Decisions
SignalR, background jobs, caching, external APIs, etc.

## Test Layer Map
| Entity/Component | Rich/Anemic | Test Layer |
|-----------------|-------------|------------|
| Example         | Anemic      | Integration (API) |
""")
```

**Gate 1 — verify:**
1. Architecture doc exists at expected path
2. Entity Model classifies every entity from the spec
3. API Contract has at least one endpoint per user flow
4. Test Layer Map has an entry for every entity
5. No rich/anemic misclassification (anemic with domain tests, rich without)
6. Component Tree has at least one page per user flow

Fail -> re-dispatch with failure feedback (up to 2 retries).

**Update STATE.md:** Phase 1 complete, record architecture doc path.

---

### Phase 2: Planning (Architecture -> Implementation Plans)

**For each work unit**, dispatch a separate subagent. Select applicable units from:

- Project scaffold (greenfield only)
- Backend domain + infra
- Backend API
- Backend real-time (if SignalR needed)
- Frontend state + API
- Frontend pages + components
- Integration

**Skill routing:**

| Work Unit | Skill |
|-----------|-------|
| Scaffold | `saurun:writing-plans` (routes to both) |
| Backend | `saurun:writing-plans` (routes to `saurun:dotnet-writing-plans`) |
| Frontend | `saurun:writing-plans` (routes to `saurun:react-writing-plans`) |
| Integration | `saurun:writing-plans` (routes to both, backend first) |

**Subagent prompt (per work unit):**

```
Task(subagent_type="general-purpose", prompt="""
{AUTONOMOUS_PREAMBLE}

You are executing Phase 2 (Planning) of the god-agent pipeline.
Generate an implementation plan for work unit: {unit_name}

ARCHITECTURE DOC: {contents of architecture doc}
PRODUCT SPEC: {contents of product spec}
STATE: {STATE.md contents}

SKILL TO LOAD: saurun:writing-plans
Load via Skill tool — it will route to the appropriate stack-specific planning skill.

CONTEXT FOR PLAN WRITER:
{relevant sections of architecture doc for this work unit}

OUTPUT: Write plan to docs/plans/{DATE}-{unit_name}.md
""")
```

**After all plans are written**, write `docs/plans/MANIFEST.json`:

```json
{
  "units": {
    "{unit-id}": {
      "name": "{Human-readable name}",
      "plan": "docs/plans/{DATE}-{unit-name}.md",
      "dependsOn": ["{other-unit-ids}"]
    }
  }
}
```

Keys = stable IDs for `dependsOn` references. Execution order = topological sort of DAG.

**Gate 2 — verify per plan:**
1. Every task has file paths (no vague "update the controller")
2. Every task mentions TDD (test first)
3. No getter/setter test specs for anemic entities
4. Test names follow `MethodName_Scenario_Expected` convention
5. "What bugs do these tests catch?" table is populated

**Gate 2 — verify MANIFEST.json:**
1. All applicable work units are present
2. Dependencies are acyclic
3. Backend plans come before frontend plans that depend on them

Fail -> re-dispatch the failing plan's subagent with feedback (up to 2 retries).

**Update STATE.md:** Phase 2 complete, list all plan paths.

---

### Phase 3: Execution (Plans -> Working Code)

**You (the controller) run this directly.** Do NOT delegate Phase 3 to a single subagent. You orchestrate the task loop yourself, dispatching individual implementer/reviewer subagents per task. This preserves cross-phase context from Phases 0-2.

**Load skill:** `superpowers:subagent-driven-development` — follow its process for the implement/review loop.

**Orchestration loop:**

```
Read MANIFEST.json -> topological sort -> execution order
For each plan (in order):
  Read the plan file, extract all tasks with full text
  For each task:
    1. Update STATE.md: current position
    2. Dispatch IMPLEMENTER subagent (see skill routing below)
       - Provide: full task text, autonomous preamble, product spec summary,
         architecture context, previous task outcomes
       - If implementer asks questions -> answer from Phase 0/1 context
    3. Dispatch SPEC REVIEWER subagent
       - Uses spec-reviewer-prompt.md bundled with superpowers:subagent-driven-development
       - Pass -> continue
       - Fail -> resume implementer with fix instructions -> re-review
    4. Dispatch CODE QUALITY REVIEWER subagent (see skill routing below)
       - Pass -> continue
       - Fail (Critical/Important) -> resume implementer -> re-review
       - Fail (Minor) -> note and continue
    5. Update STATE.md: task complete
  After all tasks in plan:
    Run full test suite: dotnet test backend/ && npm test (if frontend exists)
    Pass -> next plan
    Fail -> dispatch superpowers:systematic-debugging subagent (up to 2 attempts)
    Still fails -> STOP and report
```

**Implementer skill routing:**

| Plan Type | Implementer Template | Skills Loaded | Quality Reviewer |
|-----------|---------------------|---------------|-----------------|
| Backend domain | `saurun:dotnet-implementer-prompt` | `saurun:dotnet-tdd`, `saurun:dotnet-tactical-ddd` | `saurun:dotnet-code-quality-reviewer-prompt` |
| Backend API | `saurun:dotnet-implementer-prompt` | `saurun:dotnet-tdd`, `saurun:dotnet-tactical-ddd` | `saurun:dotnet-code-quality-reviewer-prompt` |
| Backend SignalR | `saurun:dotnet-implementer-prompt` | `saurun:dotnet-tdd` | `saurun:dotnet-code-quality-reviewer-prompt` |
| Frontend state | `saurun:react-implementer-prompt` | `saurun:react-tdd`, `saurun:react-tailwind-v4-components` | `saurun:react-code-quality-reviewer-prompt` |
| Frontend pages | `saurun:react-implementer-prompt` | `saurun:react-tdd`, `saurun:react-tailwind-v4-components`, `superpowers:frontend-design` | `saurun:react-code-quality-reviewer-prompt` |
| Integration | Both templates | All skills | Both reviewers |

**Failure escalation:**

```
Task fails after 2 implementer retries
  -> Dispatch superpowers:systematic-debugging subagent
     Input: failing test output, relevant code, task spec
     Output: diagnosis + fix
  -> Apply fix, re-run review loop
  -> Still failing after 2 debugging retries -> STOP
     Write failure report to STATE.md
```

**Gate 3 — verify:**
1. Every plan in MANIFEST.json has status "Complete" in STATE.md
2. `dotnet test backend/ --verbosity minimal` — 0 failures
3. `npm test` — 0 failures (if frontend exists)
4. No unresolved failures in STATE.md Failures table
5. Git log contains commits matching expected plan tasks
6. Git working tree is clean

Fail -> identify which plan/task failed -> dispatch `superpowers:systematic-debugging` -> re-run gate.

**Update STATE.md:** Phase 3 complete.

---

### Phase 4: Integration (Working Code -> Shipped)

**Run verification first (as controller):**
```bash
dotnet test backend/
dotnet build backend/
npm test          # if frontend exists
npm run build     # if frontend exists
```
All pass -> dispatch integration subagent. Any fail -> `superpowers:systematic-debugging` (last chance).

**Dispatch subagent:**

```
Task(subagent_type="general-purpose", prompt="""
{AUTONOMOUS_PREAMBLE}

You are executing Phase 4 (Integration) of the god-agent pipeline.

SKILLS TO LOAD:
1. superpowers:finishing-a-development-branch
2. superpowers:verification-before-completion

STATE: {STATE.md contents}
PRODUCT SPEC: {spec summary}
ARCHITECTURE: {architecture summary}

TASKS:
1. Load finishing-a-development-branch skill and follow its process
2. Update CLAUDE.md Implementation Status section with newly built features
3. If greenfield with no remote -> commit to main
   If existing project -> create PR with summary
4. Write completion report to docs/reports/{DATE}-{feature}-report.md

COMPLETION REPORT TEMPLATE:
# {Feature Name} -- Completion Report

## What Was Built
Summary from Phase 0 spec.

## Architecture Decisions
Key decisions from Phase 1.

## Plans Executed
| # | Plan | Tasks | Status |
|---|------|-------|--------|

## Test Results
- Backend: X tests, 0 failures
- Frontend: Y tests, 0 failures

## Assumptions Made
All assumptions from STATE.md Assumptions Log.

## Issues Encountered
Failures, debugging escalations, and resolutions.

## PR
URL (if applicable)
""")
```

**Gate 4 — verify:**
1. All tests pass (`dotnet test` + `npm test`)
2. All builds succeed (`dotnet build` + `npm run build`)
3. Completion report exists and all sections populated
4. No unresolved failures in STATE.md
5. Git working tree is clean
6. CLAUDE.md Implementation Status reflects newly implemented features
7. No security guardrail violations in STATE.md Security Log

**Write final STATE.md.** Done.

---

## STATE.md Protocol

Written to `.god-agent/STATE.md`. Updated after every phase completion AND after every task within Phase 3.

```markdown
# God-Agent State

## Meta
- **Input:** <original user input, verbatim>
- **Mode:** Greenfield | Extension
- **Started:** <ISO timestamp>
- **Last Updated:** <ISO timestamp>

## Phase Tracker
| Phase | Status | Artifact | Completed At |
|-------|--------|----------|--------------|
| 0. Intake | Pending | — | — |
| 1. Architecture | Pending | — | — |
| 2. Planning | Pending | — | — |
| 3. Execution | Pending | — | — |
| 4. Integration | Pending | — | — |

## Current Position
- **Phase:** 0
- **Sub-position:** Starting

## Decisions Log
1. [Phase N] Decision — rationale

## Assumptions Log
1. [Phase N] Assumption — reasoning

## Context Layers
### Injected Context
<user-provided preferences, verbatim>

### Project Context
<CLAUDE.md summary, existing structure, conventions>

### Stack Defaults
.NET 9, ASP.NET Core, EF Core 9, SQLite, SignalR, React 19, Vite, TypeScript, Tailwind CSS v4, Zustand

## Failures
| Phase | Task | Attempts | Resolution |
|-------|------|----------|------------|

## Security Log
| Timestamp | Phase | Action | Justification |
|-----------|-------|--------|---------------|
```

---

## Error Handling

**STOP Protocol — every STOP means ALL of these steps:**
1. Update STATE.md: set current phase status to `Failed`, record failure details in Failures table
2. Commit any uncommitted work to git (so nothing is lost)
3. Output to user: `God-agent STOPPED at Phase {X}, {task if applicable}: {one-line reason}. Fix the issue and re-invoke /god-agent to resume.`
4. Do NOT continue to the next phase or task. The session ends here.

**Subagent dispatch failure (API error):**
Retry same dispatch up to 2 times with backoff. Still failing -> STOP.

**Gate check failure:**
Re-dispatch same phase subagent with feedback listing specific issues. Up to 2 retries. Still failing -> STOP.

**Test suite failure (end of plan):**
Dispatch `superpowers:systematic-debugging` with failing test output + recent git diff + plan spec. Up to 2 debugging retries. Still failing -> STOP.

**Context exhaustion:**
STATE.md has exact position. All completed code is committed. All plan docs are on disk. Re-invoking `/god-agent` reads STATE.md and resumes from exact checkpoint.

---

## Autonomous Preamble (Reference)

Inject this into EVERY subagent dispatch:

```
AUTONOMOUS MODE: You are operating without a human in the loop.
When a skill instructs you to ask the user a question:
1. Identify what information is needed
2. Check STATE.md, spec, and architecture docs for the answer
3. If found -> use it
4. If not found -> make a reasonable decision and log it as an
   assumption in STATE.md under ## Assumptions Log
Never block waiting for human input. Never use AskUserQuestion.

SECURITY GUARDRAILS:
- Never run destructive git commands (force push, reset --hard, clean -f)
- Never delete production data, configuration files, or .env files
- Never modify security-sensitive files (credentials, tokens, secrets)
- Never run commands that affect systems outside the project directory
- Log any security-relevant decision to STATE.md under ## Security Log
```
