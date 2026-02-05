# God-Agent Design: Autonomous .NET + React Development Pipeline

> Fully autonomous agent that takes an idea (any format) and delivers working, tested, reviewed code.

## Architecture Overview

The god-agent is a **skill-as-orchestrator** invoked via `/god-agent`. A single invocation runs all 5 phases (0–4) sequentially within the current session. Each phase dispatches subagents via the Task tool. Phases 0-2 and 4 use `general-purpose` subagents with skill-loading prompts. Phase 3 uses specialized agents (`saurun:backend-implementer`, `saurun:frontend-implementer`) for implementation and `superpowers:code-reviewer` for review. `STATE.md` serves as both cross-phase handoff and crash-resume checkpoint.

```
/god-agent "Build a recipe sharing app"
         │
         ▼
┌──────────────────────────────────────────────────┐
│  God-Agent Skill (Controller Session)            │
│                                                  │
│  1. Parse input + read context layers            │
│  2. Pre-flight: verify all required skills exist │
│  3. Write initial STATE.md                       │
│  4. Internal loop:                               │
│                                                  │
│     Phase 0 → dispatch subagent → gate check     │
│     Phase 1 → dispatch subagent → gate check     │
│     Phase 2 → dispatch subagent → gate check     │
│     Phase 3 → run subagent-driven-dev → gate     │
│     Phase 4 → dispatch subagent → gate check     │
│                                                  │
│  5. Each phase: update STATE.md after completion │
│  6. On crash: re-invoke reads STATE.md, resumes  │
└──────────────────────────────────────────────────┘
```

**Hard constraints:**
- Tech stack: .NET 9 + React 19 + Tailwind v4 + Zustand (always)
- Fully autonomous: no human checkpoints during execution
- Failure handling: retry 2x → escalate to `superpowers:systematic-debugging` subagent → STOP if debugging fails
- Runtime: works both interactively (`claude` CLI) and headlessly (`claude -p`)

---

## Autonomous Wrapper Preamble

All subagent dispatches include this preamble to handle interactive skills without human input:

```
AUTONOMOUS MODE: You are operating without a human in the loop.
When a skill instructs you to ask the user a question:
1. Identify what information is needed
2. Check STATE.md, spec, and architecture docs for the answer
3. If found → use it
4. If not found → make a reasonable decision and log it as an
   assumption in STATE.md under ## Assumptions Log
Never block waiting for human input. Never use AskUserQuestion.

SECURITY GUARDRAILS:
- Never run destructive git commands (force push, reset --hard, clean -f)
- Never delete production data, configuration files, or .env files
- Never modify security-sensitive files (credentials, tokens, secrets)
- Never run commands that affect systems outside the project directory
- Log any security-relevant decision to STATE.md under ## Security Log
```

This single pattern enables all interactive skills (`brainstorming`, `finishing-a-development-branch`, `writing-plans`) to run autonomously without forking them into separate variants.

---

## STATE.md Protocol

Written to `.god-agent/STATE.md` in the project root. Updated after every phase completion and before every subagent dispatch.

### Structure

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
| 0. Intake | Complete | _docs/specs/YYYY-MM-DD-<feature>.md | <timestamp> |
| 1. Architecture | Complete | _docs/specs/YYYY-MM-DD-<feature>-architecture.md | <timestamp> |
| 2. Planning | In Progress | — | — |
| 3. Execution | Pending | — | — |
| 4. Integration | Pending | — | — |

## Current Position
- **Phase:** 2
- **Sub-position:** Generating plan for work unit 3 of 7 (backend-api)

## Decisions Log
Ordered list of every design/architecture decision with rationale:
1. [Phase 0] Chose offline-first approach because <rationale>
2. [Phase 1] ShoppingList classified as anemic — no domain methods, test at integration level
3. ...

## Assumptions Log
Decisions made without human input (self-dialogue results):
1. [Phase 0] Assumed Danish-language UI based on product context
2. [Phase 0] Assumed mobile-first responsive design
3. ...

## Context Layers
### Injected Context
<user-provided constraints, verbatim>

### Project Context
<CLAUDE.md summary, existing structure, conventions>

### Stack Defaults
.NET 9, ASP.NET Core, EF Core 9, SQLite, SignalR, React 19, Vite, TypeScript, Tailwind CSS v4, Zustand

## Failures
| Phase | Task | Attempts | Resolution |
|-------|------|----------|------------|
| 3 | backend-api/task-3 | 2 | Fixed: missing FK constraint |

## Security Log
| Timestamp | Phase | Action | Justification |
|-----------|-------|--------|---------------|
| <ISO> | 3 | Deleted test fixture file | Outdated fixture from previous implementation |
```

### Resume Protocol

On re-invocation after crash:
1. Read `.god-agent/STATE.md`
2. Find last completed phase
3. Read that phase's output artifact
4. Resume from next incomplete phase (or sub-position within a phase)
5. Log `[RESUMED] from Phase X` in STATE.md

---

## How Gates Work

After each phase completes, the **controller** (not the subagent) validates the output by reading the artifact and running through the gate checklist:

1. **Read artifact** — controller reads the phase output (spec file, architecture doc, plan files, etc.)
2. **Run checklist** — controller checks each item in the gate checklist
3. **Pass** — all items checked → proceed to next phase
4. **Fail** — one or more items unchecked → re-dispatch subagent with: `"Gate N failed: {unchecked items}. Fix these."`
5. **Retry limit** — up to 2 re-dispatches per gate. Still failing → log to STATE.md and STOP.

---

## Phase -1: Scaffold (Greenfield Only)

**Purpose:** Create project structure for new projects.

**Runs as:** Controller executes directly (no subagent).

**Skip if:** Extension mode (CLAUDE.md exists).

### Steps

1. Create directory structure:
   ```
   {project}/
   ├── backend/
   ├── frontend/
   └── _docs/
   ```

2. Generate CLAUDE.md from template (see template below)

3. Create .gitignore:
   ```
   # .NET
   bin/
   obj/
   *.user
   .vs/

   # Node
   node_modules/
   dist/
   .vite/

   # IDE
   .idea/

   # OS
   .DS_Store

   # Environment
   .env
   .env.local

   # SQLite
   *.db
   *.db-journal
   ```

4. Git init + initial commit:
   ```bash
   git init
   git add .
   git commit -m "Initial commit: {project_name} scaffold"
   ```

### CLAUDE.md Template

```markdown
# CLAUDE.md — {project_name}

## Project
{description from Phase 0 spec - populated after Phase 0}

## Tech Stack
- Backend: .NET 9, ASP.NET Core, EF Core 9, SQLite
- Frontend: React 19, Vite, TypeScript, Tailwind CSS v4, Zustand

## Commands

### Backend
\`\`\`bash
cd backend && dotnet restore && dotnet build && dotnet run
\`\`\`

### Frontend
\`\`\`bash
cd frontend && npm install && npm run dev
\`\`\`

### Tests
\`\`\`bash
cd backend && dotnet test
cd frontend && npm test
\`\`\`
```

### Gate -1 Checklist
- [ ] Directory `backend/` exists
- [ ] Directory `frontend/` exists
- [ ] Directory `_docs/` exists
- [ ] File `CLAUDE.md` exists (placeholder OK, populated after Phase 0)
- [ ] File `.gitignore` exists
- [ ] Git repo initialized (`.git/` directory exists)
- [ ] At least one commit exists (`git log` returns a commit)

**On failure:** Re-dispatch with: `"Gate -1 failed: {unchecked items}. Fix these."`

---

## Phase 0: Intake (Idea → Product Spec)

**Purpose:** Normalize any input into a structured product spec.

**Runs as:** Subagent via `Task(subagent_type="general-purpose")` with `superpowers:brainstorming` skill + autonomous preamble.

### Input Assessment

```
Raw input received
  │
  ├── One-liner ("Add auth")        → Full self-dialogue brainstorming pass
  ├── Rough spec (paragraph/bullets) → Targeted gap-filling via self-dialogue
  └── Product brief (structured)     → Validation + contradiction check
```

### Self-Dialogue Process

The subagent plays both roles in the brainstorming skill's Q&A process:
1. Reads input + all context layers from STATE.md
2. Generates the questions brainstorming would ask (scope, constraints, UX, entities)
3. Answers each from context — injected context first, then project context, then stack defaults
4. If no answer available → makes reasonable decision → logs in STATE.md Assumptions Log
5. Presents design in sections (per brainstorming skill), self-validates each section
6. Writes final spec to disk

### Context Layers

Read in priority order, each constraining the next:

| Layer | Source | When |
|-------|--------|------|
| **Injected context** | User-provided text, file path, or nothing | Always checked first |
| **Project context** | `CLAUDE.md`, `_docs/`, `agent-os/` | Extension mode only |
| **Stack defaults** | .NET 9, React 19, Tailwind v4, Zustand, SQLite | Always (hardcoded) |

### Two Modes

**Greenfield Mode** (no project exists):
1. Injected context is the only constraint (besides stack defaults)
2. Self-dialogue brainstorming from idea + context
3. Output includes project scaffold requirements for Phase 2
4. Output includes initial `CLAUDE.md` content for the new project
5. Write spec to `_docs/specs/YYYY-MM-DD-<feature>.md`

**Extension Mode** (project exists):
1. Read `CLAUDE.md`, `_docs/`, `agent-os/` for project context
2. Read injected context if provided
3. Self-dialogue brainstorming within existing constraints
4. Write spec to `_docs/specs/YYYY-MM-DD-<feature>.md`

### Subagent Prompt Template

```
Task(subagent_type="general-purpose", prompt="""
{AUTONOMOUS_PREAMBLE}

You are executing Phase 0 (Intake) of the god-agent pipeline.

INPUT: {user_input}

STATE: {STATE.md contents}

SKILL TO FOLLOW: superpowers:brainstorming
Load this skill via the Skill tool and follow its process, but in
self-dialogue mode — generate questions, answer them from context,
log assumptions.

MODE: {Greenfield | Extension}

OUTPUT REQUIREMENTS:
1. Write product spec to _docs/specs/YYYY-MM-DD-{feature}.md
   using the template below
2. Update .god-agent/STATE.md with decisions and assumptions

SPEC TEMPLATE:
# {Feature Name} — Product Spec

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

### Gate 0 Checklist

After subagent completes, controller verifies:
- [ ] Spec file exists at `_docs/specs/{DATE}-{feature}.md`
- [ ] Section "## Problem" exists and is not empty
- [ ] Section "## Solution" exists and is not empty
- [ ] Section "## Scope" has both "### In Scope" and "### Out of Scope" subsections
- [ ] Section "## Entities" lists at least 1 entity
- [ ] Section "## User Flows" has at least 1 flow
- [ ] Section "## API Surface" exists and is not empty
- [ ] Section "## Tech Decisions" exists and is not empty
- [ ] Section "## Decisions & Rationale" has at least 3 entries
- [ ] Section "## Rejected Alternatives" has at least 2 entries with reasoning
- [ ] Section "## Risks & Mitigations" table has at least 2 rows
- [ ] No contradictions between In Scope and Out of Scope items

**On failure:** Re-dispatch with: `"Gate 0 failed: {unchecked items}. Fix these."`

---

## Phase 1: Architecture (Spec → Technical Design)

**Purpose:** Turn product spec into concrete technical decisions. Answers **how**, not **what**.

**Runs as:** Subagent via `Task(subagent_type="general-purpose")` with `dotnet-tactical-ddd` + `react-tailwind-v4-components` loaded + autonomous preamble.

### Backend Decisions (via dotnet-tactical-ddd)

- Entity classification: rich domain objects vs anemic property bags
- Layer structure: Api / Application / Domain / Infrastructure
- Database: EF Core 9 + SQLite, migration strategy
- Auth strategy, real-time needs (SignalR), API shape
- Value objects, aggregate boundaries, domain events

### Frontend Decisions (via react-tailwind-v4-components)

- Component tree derived from user flows
- State management: Zustand stores mapped from entities
- API client shape from backend endpoints
- Page routing from user flows
- Design system tokens (if greenfield)

### Entity Design Decision Framework

For each entity in the spec:

```
Does the entity have business rules or behavior methods?
  │
  ├── YES → Rich domain object
  │   - Private setters, factory methods, invariant enforcement
  │   - Test at domain level (unit tests on behavior methods)
  │
  └── NO → Anemic property bag
      - Public getters/setters, no domain methods
      - Do NOT test at domain level (nothing behavioral to test)
      - Test at integration level (API endpoints exercise the entity through EF Core)
```

### Subagent Prompt Template

```
Task(subagent_type="general-purpose", prompt="""
{AUTONOMOUS_PREAMBLE}

You are executing Phase 1 (Architecture) of the god-agent pipeline.

PRODUCT SPEC: {contents of _docs/specs/YYYY-MM-DD-<feature>.md}

STATE: {STATE.md contents}

SKILLS TO LOAD (use Skill tool):
1. dotnet-tactical-ddd — for backend architecture decisions
2. react-tailwind-v4-components — for frontend architecture decisions

OUTPUT REQUIREMENTS:
1. Write architecture doc to _docs/specs/YYYY-MM-DD-{feature}-architecture.md
   using the template below
2. Update .god-agent/STATE.md with architecture decisions

ARCHITECTURE TEMPLATE:
# {Feature Name} — Technical Architecture

## Entity Model
Entities, relationships, rich vs anemic classification.
Aggregate boundaries if applicable.

## API Contract
Endpoints, HTTP methods, request/response DTOs.
SignalR hubs and events if applicable.

## Component Tree
Pages, shared components, hooks, stores.

## Data Flow
Frontend → API → Application → Domain → Infrastructure → DB (and back).

## Infrastructure Decisions
SignalR, background jobs, caching, external APIs, etc.

## Test Layer Map
| Entity/Component | Rich/Anemic | Test Layer |
|-----------------|-------------|------------|
| Example         | Anemic      | Integration (API) |
""")
```

### Gate 1 Checklist

- [ ] Architecture doc exists at `_docs/specs/{DATE}-{feature}-architecture.md`
- [ ] Section "## Entity Model" exists and is not empty
- [ ] Every entity from the spec is listed in Entity Model
- [ ] Each entity has a rich/anemic classification
- [ ] Section "## API Contract" exists with at least 1 endpoint per user flow
- [ ] Section "## Component Tree" exists with at least 1 page per user flow
- [ ] Section "## Data Flow" exists and is not empty
- [ ] Section "## Infrastructure Decisions" exists
- [ ] Section "## Test Layer Map" exists as a table
- [ ] Test Layer Map has an entry for every entity
- [ ] No misclassification: anemic entities do not have "Domain" test layer
- [ ] No misclassification: rich entities have "Domain" or "Unit" test layer

**On failure:** Re-dispatch with: `"Gate 1 failed: {unchecked items}. Fix these."`

---

## Phase 2: Planning (Architecture → Implementation Plans)

**Purpose:** Break the architecture into ordered, bite-sized implementation tasks.

**Runs as:** One subagent per work unit. Backend plans use `saurun:dotnet-writing-plans`. Frontend plans use `saurun:react-writing-plans`.

### Work Unit Catalog

The Phase 2 subagents generate plans for each applicable unit. After all subagents complete, the controller writes MANIFEST.json with plans in execution order. Not all units are needed for every project.

```
Possible work units:

- Project scaffold          (greenfield only)
- Backend domain + infra    (entities, DbContext, migrations)
- Backend API               (controllers, DTOs, endpoints)
- Backend real-time          (SignalR hubs — if applicable)
- Frontend state + API       (Zustand stores, API hooks)
- Frontend pages + components (UI)
- Integration                (wire everything, E2E smoke test)
```

### Plan Generation

| Work Unit | Skill Used | Subagent Prompt Includes |
|-----------|-----------|--------------------------|
| Scaffold | `saurun:dotnet-writing-plans` then `saurun:react-writing-plans` | Architecture doc, stack defaults |
| Backend domain | `saurun:dotnet-writing-plans` | Architecture doc entity model section |
| Backend API | `saurun:dotnet-writing-plans` | Architecture doc API contract + entity model |
| Backend SignalR | `saurun:dotnet-writing-plans` | Architecture doc infrastructure section |
| Frontend state | `saurun:react-writing-plans` | Architecture doc component tree + API contract |
| Frontend pages | `saurun:react-writing-plans` | Architecture doc component tree + design decisions |
| Integration | Both | Full architecture doc |

### Subagent Prompt Template (Backend Example)

```
Task(subagent_type="general-purpose", prompt="""
{AUTONOMOUS_PREAMBLE}

You are executing Phase 2 (Planning) of the god-agent pipeline.
Generate an implementation plan for work unit: {unit_name}

ARCHITECTURE DOC: {contents of architecture doc}
PRODUCT SPEC: {contents of product spec}
STATE: {STATE.md contents}

SKILL TO LOAD: saurun:dotnet-writing-plans (or saurun:react-writing-plans for frontend)
Load via Skill tool and follow its process.

CONTEXT FOR PLAN WRITER:
{relevant sections of architecture doc for this work unit}

OUTPUT: Write plan to _docs/plans/YYYY-MM-DD-{unit_name}.md
""")
```

### Plan Format

Plans follow the format defined by the writing-plans skills:
- Backend work units: `saurun:dotnet-writing-plans`
- Frontend work units: `saurun:react-writing-plans`

Key elements (see skills for complete specification):
- Header with Goal, Architecture, Tech Stack
- Tasks with exact file paths (`Create:`, `Modify:`, `Test:`)
- 5-step TDD workflow per task (write test → run fail → implement → run pass → commit)
- "What bugs do these tests catch?" table mapping tests to real bugs prevented

The skills ensure plans are self-contained instruction sets that an engineer with zero codebase context can execute task-by-task.

### Output Artifacts

- `_docs/plans/YYYY-MM-DD-<unit>.md` — one per work unit
- `_docs/plans/MANIFEST.json` — execution order as ordered list

### MANIFEST.json Format

**Controller writes MANIFEST.json after all Phase 2 subagents complete.** The controller:
1. Dispatches Phase 2 subagents for each work unit
2. Collects the plan paths from each
3. Writes MANIFEST.json with plans in execution order

```json
{
  "feature": "recipe-sharing",
  "created": "2026-02-04T10:30:00Z",
  "plans": [
    {"id": "backend-domain", "path": "_docs/plans/2026-02-04-backend-domain.md"},
    {"id": "backend-api", "path": "_docs/plans/2026-02-04-backend-api.md"},
    {"id": "frontend-state", "path": "_docs/plans/2026-02-04-frontend-state.md"},
    {"id": "frontend-pages", "path": "_docs/plans/2026-02-04-frontend-pages.md"}
  ]
}
```

Array order is execution order. The .NET + React stack has predictable, near-linear dependencies (backend before frontend), so DAG complexity is unnecessary. Status tracking lives in STATE.md, not the manifest.

### Gate 2 Checklist (per plan)

- [ ] Plan file exists at `_docs/plans/{DATE}-{unit_name}.md`
- [ ] Every task has explicit file paths (no vague "update the controller")
- [ ] Every task specifies TDD workflow (test first, then implement)
- [ ] No getter/setter tests for anemic entities
- [ ] Test names follow `MethodName_Scenario_Expected` convention
- [ ] "What bugs do these tests catch?" table exists and has at least 1 row

### Gate 2 Checklist (MANIFEST.json)

- [ ] File `_docs/plans/MANIFEST.json` exists
- [ ] `plans` array contains all applicable work units
- [ ] Plans are in correct execution order (backend plans before frontend plans)
- [ ] Each plan entry has `id` and `path` fields
- [ ] Each `path` references an existing file

**On failure:** Re-dispatch the failing plan's subagent with: `"Gate 2 failed for {plan_name}: {unchecked items}. Fix these."`

---

## Phase 3: Execution (Plans → Working Code)

**Purpose:** Execute all plans sequentially, producing committed, tested, reviewed code.

**Runs as:** Controller runs `superpowers:subagent-driven-development` directly — the controller itself orchestrates the task loop, dispatching individual implementer/reviewer subagents per task, rather than wrapping the entire phase in a single subagent. This preserves cross-phase context from Phases 0-2, enabling the controller to answer implementer questions from earlier context.

### Why Controller Runs This Directly

Phase 3 is the longest and most context-intensive phase. Running it from the controller means:
- Implementer questions can be answered using Phase 0/1 context (spec rationale, architecture decisions)
- The controller "remembers" failures from earlier tasks and can provide relevant context to later tasks
- STATE.md is updated after every task (not just every phase), enabling fine-grained resume

**Tradeoff:** This consumes the most context in the controller session. Mitigated by writing comprehensive STATE.md after each task completion.

### Orchestration Loop

```
Read MANIFEST.json
  ↓
For each plan (in order):
  ↓
  For each task in plan:
    │
    ├── 1. Update STATE.md: current position
    │
    ├── 2. Dispatch IMPLEMENTER subagent
    │   Prompt includes: task text + autonomous preamble
    │   Skills loaded depend on plan type (see table)
    │   Implementer: codes → tests → commits → self-reviews
    │   If implementer asks questions → controller answers from
    │   spec/architecture context
    │
    ├── 3. Dispatch REVIEWER subagent (single-pass, combined review)
    │   Uses superpowers:code-reviewer with stack-specific quality criteria
    │   Reviews both spec compliance AND code quality in one pass
    │   Pass → continue
    │   Fail (Critical/Important) → resume implementer with fix instructions → re-review (up to 2 retries)
    │   Fail (Minor) → note and continue
    │
    └── 4. Update STATE.md: task complete → next task
  ↓
  All tasks in plan complete
  ↓
  Run full test suite (dotnet test + npm test)
    Pass → next plan
    Fail → escalate to systematic-debugging subagent (up to 2 attempts)
    Still fails → STOP and report
```

> **Design rationale:** Single-pass review replaces the previous two-pass (spec + quality) approach because: (1) implementer self-review already covers spec compliance, (2) one combined review is more efficient (single dispatch, all feedback at once), and (3) less orchestration complexity.

### Subagent Skill Loading

| Plan Type | Implementer Skills | Review Criteria Source |
|-----------|-------------------|------------------------|
| Backend domain | `dotnet-tdd`, `dotnet-tactical-ddd` | `saurun:dotnet-code-quality-reviewer-prompt` |
| Backend API | `dotnet-tdd`, `dotnet-tactical-ddd` | `saurun:dotnet-code-quality-reviewer-prompt` |
| Backend SignalR | `dotnet-tdd` | `saurun:dotnet-code-quality-reviewer-prompt` |
| Frontend state | `react-tdd`, `react-tailwind-v4-components` | `saurun:react-code-quality-reviewer-prompt` |
| Frontend pages | `react-tdd`, `react-tailwind-v4-components`, `frontend-design` | `saurun:react-code-quality-reviewer-prompt` |
| Integration | Both backend + frontend skills | Both criteria templates |

**How Quality Criteria Are Used:**
The skills `saurun:dotnet-code-quality-reviewer-prompt` and `saurun:react-code-quality-reviewer-prompt` are **criteria templates** — not directly invocable reviewers. The god-agent:
1. Loads the appropriate criteria template skill for the task type
2. Extracts the `ADDITIONAL_REVIEW_CRITERIA` section
3. Pastes it into the unified review prompt sent to `superpowers:code-reviewer`

This enables a single reviewer dispatch that covers both spec compliance and stack-specific code quality in one pass.

### Implementer Dispatch

Backend tasks use `saurun:dotnet-implementer-prompt` as the dispatch template (loads `dotnet-tdd` + `dotnet-tactical-ddd`). Frontend tasks use `saurun:react-implementer-prompt` as the dispatch template (loads `react-tdd` + `react-tailwind-v4-components`).

**Backend tasks:**
```
Task(subagent_type="saurun:backend-implementer", prompt="""
{AUTONOMOUS_PREAMBLE}

You are implementing a task from the god-agent pipeline.

TASK SPEC:
{full task text from plan}

CONTEXT:
- Product spec: {summary of key decisions from Phase 0}
- Architecture: {relevant sections from Phase 1}
- Previous tasks in this plan: {completed task names + what they produced}

INSTRUCTIONS:
1. Follow TDD: write test first, run (expect fail), implement, run (expect pass)
2. Commit after each green test cycle
3. Self-review your work before finishing
4. Report what you built, what tests you wrote, and any assumptions
""")
```

**Frontend tasks:**
```
Task(subagent_type="saurun:frontend-implementer", prompt="""
{AUTONOMOUS_PREAMBLE}

You are implementing a task from the god-agent pipeline.

TASK SPEC:
{full task text from plan}

CONTEXT:
- Product spec: {summary of key decisions from Phase 0}
- Architecture: {relevant sections from Phase 1}
- Previous tasks in this plan: {completed task names + what they produced}

INSTRUCTIONS:
1. Follow TDD: write test first, run (expect fail), implement, run (expect pass)
2. Commit after each green test cycle
3. Self-review your work before finishing
4. Report what you built, what tests you wrote, and any assumptions
""")
```

### Reviewer Dispatch (Single-Pass)

After the implementer completes, dispatch a single unified review:

```
Task(subagent_type="superpowers:code-reviewer", prompt="""
Review this implementation for both spec compliance and code quality.

TASK SPEC:
{task spec from plan}

WHAT WAS IMPLEMENTED:
{implementer's report}

DIFF: Compare {BASE_SHA}..{HEAD_SHA}

{ADDITIONAL_REVIEW_CRITERIA from the appropriate criteria template skill}
""")
```

The `ADDITIONAL_REVIEW_CRITERIA` is loaded from:
- Backend tasks: `saurun:dotnet-code-quality-reviewer-prompt`
- Frontend tasks: `saurun:react-code-quality-reviewer-prompt`

### Failure Escalation

```
Task fails (tests don't pass after 2 implementer retries)
  ↓
Dispatch systematic-debugging subagent
  Input: failing test output, relevant code, task spec
  Output: diagnosis + fix
  ↓
Apply fix → re-run review loop
  ↓
Still failing after 2 debugging retries → STOP
  Write failure report to STATE.md
  Report: what failed, what was tried, diagnosis
```

### Gate 3 Checklist

After all plans in MANIFEST.json have been executed:
- [ ] Every plan in MANIFEST.json has status "Complete" in STATE.md Phase Tracker
- [ ] Backend tests pass: `dotnet test backend/ --verbosity minimal` returns 0 failures
- [ ] Frontend tests pass (if frontend exists): `npm test` returns 0 failures
- [ ] STATE.md Failures table has no unresolved entries (all have Resolution)
- [ ] Git log contains at least 1 commit per plan executed
- [ ] Git working tree is clean: `git status --porcelain` returns empty

**On failure:** Identify which plan/task failed, dispatch `superpowers:systematic-debugging` with failure details, then re-run gate.

---

## Phase 4: Integration (Working Code → Shipped)

**Purpose:** Verify everything works together and finish the branch.

**Runs as:** Subagent via `Task(subagent_type="general-purpose")` with `superpowers:finishing-a-development-branch` + `superpowers:verification-before-completion` + autonomous preamble.

### Steps

```
All plans executed, all tests green
  ↓
Final verification (run by controller before dispatching subagent):
  - dotnet test backend/ (full backend suite)
  - dotnet build backend/ (compilation check)
  - npm test (frontend suite, if exists)
  - npm run build (TypeScript + bundle check, if exists)
  ↓
  All pass → dispatch integration subagent
  Any fail → systematic-debugging subagent (last chance)
  ↓
Integration subagent:
  - Loads finishing-a-development-branch skill
  - Updates CLAUDE.md Implementation Status section with newly built features
  - Greenfield, no remote → commit to main
  - Existing project → create PR with summary
  - Writes completion report
  ↓
Controller writes final STATE.md
  ↓
Done.
```

### Completion Report

Written by the integration subagent to `_docs/reports/YYYY-MM-DD-<feature>-report.md`:

```markdown
# {Feature Name} — Completion Report

## What Was Built
Summary from Phase 0 spec.

## Architecture Decisions
Key decisions from Phase 1 (entity model, API shape, component tree).

## Plans Executed
| # | Plan | Tasks | Status |
|---|------|-------|--------|
| 1 | Backend domain | 8 | Complete |
| 2 | Backend API | 12 | Complete |
| ... | ... | ... | ... |

## Test Results
- Backend: X tests, 0 failures
- Frontend: Y tests, 0 failures

## Assumptions Made
All assumptions from STATE.md Assumptions Log.

## Issues Encountered
Failures, debugging escalations, and resolutions.

## PR
URL (if applicable)
```

### Gate 4 Checklist

- [ ] Backend tests pass: `dotnet test backend/` returns 0 failures
- [ ] Backend builds: `dotnet build backend/` succeeds
- [ ] Frontend tests pass (if exists): `npm test` returns 0 failures
- [ ] Frontend builds (if exists): `npm run build` succeeds
- [ ] Completion report exists at `_docs/reports/{DATE}-{feature}-report.md`
- [ ] Completion report has all sections populated (What Was Built, Architecture Decisions, Plans Executed, Test Results, Assumptions Made, Issues Encountered)
- [ ] STATE.md Failures table has no unresolved entries
- [ ] Git working tree is clean: `git status --porcelain` returns empty
- [ ] CLAUDE.md "## Implementation Status" section lists newly implemented features
- [ ] STATE.md Security Log has no violations (or all violations have Justification)

**On failure:** Re-dispatch integration subagent with: `"Gate 4 failed: {unchecked items}. Fix these."`

---

## Skill Dependency Map

### Existing Skills (ready to use)

| Skill | Owner | Phase |
|-------|-------|-------|
| `superpowers:brainstorming` | superpowers plugin | 0 |
| `saurun:dotnet-tactical-ddd` | saurun | 1 |
| `saurun:react-tailwind-v4-components` | saurun | 1 |
| `saurun:dotnet-writing-plans` | saurun | 2 |

> **Note:** Phase 3 skills (`dotnet-tdd`, `react-tdd`, `dotnet-tactical-ddd`, `react-tailwind-v4-components`, `frontend-design`) are pre-loaded by the specialized agents (`saurun:backend-implementer`, `saurun:frontend-implementer`), so they don't need explicit loading in Phase 3 prompts.
| `saurun:react-writing-plans` | saurun | 2 |
| `superpowers:subagent-driven-development` | superpowers plugin | 3 |
| `saurun:dotnet-tdd` | saurun | 3 |
| `saurun:react-tdd` | saurun | 3 |
| `saurun:dotnet-implementer-prompt` | saurun | 3 |
| `saurun:react-implementer-prompt` | saurun | 3 |
| `saurun:dotnet-code-quality-reviewer-prompt` | saurun | 3 |
| `saurun:react-code-quality-reviewer-prompt` | saurun | 3 |
| `superpowers:systematic-debugging` | superpowers plugin | 3 |
| `superpowers:verification-before-completion` | superpowers plugin | 4 |
| `superpowers:finishing-a-development-branch` | superpowers plugin | 4 |
| `frontend-design:frontend-design` | frontend-design plugin (Anthropic) | 3 |

### God-Agent Skill Location

The god-agent itself is published as a **saurun marketplace plugin** (`saurun:god-agent`). Invoked via `/god-agent` after plugin installation.

---

## Invocation

### Interactive (CLI)

```bash
# Greenfield — new project
claude "/god-agent Build a recipe sharing app for Danish families"

# With builder preferences inline
claude "/god-agent Build a meal planning app. Preferences: Danish market, \
  families 2-5 people, pirate vintage aesthetic, mobile-first, offline-first"

# Extension — existing project
cd my-project/
claude "/god-agent Add measurement units to shopping items"
# Agent reads CLAUDE.md automatically for project context
```

### Headless (API/CI)

```bash
claude -p "/god-agent Build a meal planning app. Preferences: Danish market, \
  families 2-5 people, offline-first, SignalR sync, minimalist UI"
```

### Resume After Crash

```bash
# Re-invoke in same project — reads STATE.md automatically
claude "/god-agent"
# Detects .god-agent/STATE.md, resumes from last checkpoint
```

---

## Context Budget Analysis

The controller session runs all 5 phases (0–4). Here's the expected context consumption:

| Phase | Context Cost | Mitigation |
|-------|-------------|------------|
| 0. Intake | Low — one subagent dispatch + result | — |
| 1. Architecture | Low — one subagent dispatch + result | — |
| 2. Planning | Medium — ~7 subagent dispatches + results | Summarize each plan result to key points |
| 3. Execution | **High** — many task dispatches + review loops | Write STATE.md after every task; if context exhausted, resume from STATE.md |
| 4. Integration | Low — one subagent dispatch | — |

**If context exhausts mid-Phase 3:**
1. STATE.md has exact position (which plan, which task)
2. All completed code is committed to git
3. All plan docs are on disk
4. Re-invoking `/god-agent` reads STATE.md and resumes from exact task
5. Loss: implicit context from earlier phases — mitigated by rich STATE.md Decisions Log

---

## Error Handling

### Subagent Dispatch Failure (API Error)

```
Subagent returns API error (500, timeout, etc.)
  ↓
Retry same dispatch up to 2 times with backoff
  ↓
Still failing → log to STATE.md → STOP
  Report: "Phase X subagent dispatch failed after 2 retries"
```

### Gate Check Failure

```
Gate check fails on phase output
  ↓
Re-dispatch same phase subagent with feedback:
  "Previous attempt failed gate check. Issues: {list}.
   Fix these specific issues."
  ↓
Re-check gate (up to 2 retries)
  ↓
Still failing → log to STATE.md → STOP
```

### Test Suite Failure (End of Plan)

```
Full test suite fails after all tasks in a plan
  ↓
Dispatch systematic-debugging subagent with:
  - Failing test output
  - Recent git diff
  - Plan spec for context
  ↓
Apply fix → re-run test suite
  ↓
Still failing (up to 2 debugging retries) → STOP
```

### Context Exhaustion

```
Controller hits context limit
  ↓
STATE.md already has current position + all decisions
  ↓
Session ends naturally
  ↓
User re-invokes: /god-agent
  ↓
Reads STATE.md → resumes from checkpoint
```

---

## Complete Flow Diagram

```
USER INPUT: /god-agent "idea" [--context "constraints"]
         │
         ▼
┌──────────────────────────────────────────────────┐
│  GOD-AGENT SKILL (Controller)                    │
│                                                  │
│  Read input → detect mode (greenfield/extension) │
│  Read context layers                             │
│  Pre-flight: verify all required skills exist    │
│    Missing skill → STOP with clear error         │
│  Check for existing STATE.md (resume?)           │
│  Write initial STATE.md                          │
└─────────────┬────────────────────────────────────┘
              │
              ▼
┌──────────────────────────────────────────────────┐
│  PHASE 0: INTAKE                                 │
│  Subagent: brainstorming + autonomous preamble   │
│                                                  │
│  Self-dialogue brainstorming                     │
│  Answer own questions from context layers        │
│  Log assumptions to STATE.md                     │
│  → Output: product spec                          │
│  → Gate 0 check                                  │
│  → Update STATE.md                               │
└─────────────┬────────────────────────────────────┘
              │
              ▼
┌──────────────────────────────────────────────────┐
│  PHASE 1: ARCHITECTURE                           │
│  Subagent: dotnet-tactical-ddd                   │
│          + react-tailwind-v4-components          │
│          + autonomous preamble                   │
│                                                  │
│  Entity model (rich vs anemic)                   │
│  API contract, component tree                    │
│  Test layer map                                  │
│  → Output: technical architecture                │
│  → Gate 1 check                                  │
│  → Update STATE.md                               │
└─────────────┬────────────────────────────────────┘
              │
              ▼
┌──────────────────────────────────────────────────┐
│  PHASE 2: PLANNING                               │
│  One subagent per work unit:                     │
│    Backend: saurun:dotnet-writing-plans          │
│    Frontend: saurun:react-writing-plans          │
│  + autonomous preamble                           │
│                                                  │
│  Split architecture into work units              │
│  Generate plan per unit                          │
│  → Output: MANIFEST.json + plan files              │
│  → Gate 2 check (per plan)                       │
│  → Update STATE.md                               │
└─────────────┬────────────────────────────────────┘
              │
              ▼
┌──────────────────────────────────────────────────┐
│  PHASE 3: EXECUTION                              │
│  Controller runs superpowers:subagent-driven-dev │
│  directly (not delegated)                        │
│                                                  │
│  For each plan (sequential):                     │
│    For each task:                                │
│      Implementer subagent (+ autonomous preamble)│
│        → Single-pass reviewer (spec + quality)   │
│        → Review loop until pass                  │
│      Update STATE.md per task                    │
│    End-of-plan: full test suite                  │
│    Fail? → systematic-debugging subagent         │
│  → Gate 3 check (all plans complete, tests pass) │
│  → Update STATE.md                               │
└─────────────┬────────────────────────────────────┘
              │
              ▼
┌──────────────────────────────────────────────────┐
│  PHASE 4: INTEGRATION                            │
│  Subagent: finishing-a-development-branch         │
│          + verification-before-completion         │
│          + autonomous preamble                   │
│                                                  │
│  Final test suite + build verification           │
│  Create PR or merge to main                      │
│  Write completion report                         │
│  → Gate 4 check                                  │
│  → Final STATE.md                                │
│  → Done                                          │
└──────────────────────────────────────────────────┘
```
