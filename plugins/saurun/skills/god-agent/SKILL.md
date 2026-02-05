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
   - `saurun:dotnet-tactical-ddd` (Phase 1)
   - `saurun:react-tailwind-v4-components` (Phase 1)
   - `saurun:dotnet-writing-plans` (Phase 2 — backend)
   - `saurun:react-writing-plans` (Phase 2 — frontend)
   - `superpowers:subagent-driven-development` (Phase 3)
   - `saurun:dotnet-code-quality-reviewer-prompt` (Phase 3)
   - `saurun:react-code-quality-reviewer-prompt` (Phase 3)
   - `superpowers:systematic-debugging` (Phase 3)
   - `superpowers:finishing-a-development-branch` (Phase 4)
   - `superpowers:verification-before-completion` (Phase 4)

   Required agents (Phase 3 implementers — have skills pre-loaded):
   - `saurun:backend-implementer` (has `dotnet-tactical-ddd` + `dotnet-tdd`)
   - `saurun:frontend-implementer` (has `react-tailwind-v4-components` + `frontend-design` + `react-tdd`)

5. **Read context layers** (in priority order):

   | Layer | Source | When |
   |-------|--------|------|
   | Injected context | User preferences from input string | Always |
   | Project context | `CLAUDE.md`, `_docs/`, `agent-os/` | Extension mode only |
   | Stack defaults | .NET 9, React 19, Tailwind v4, Zustand, SQLite | Always (hardcoded) |

6. **Write initial STATE.md** to `.god-agent/STATE.md` (see STATE.md Protocol below).

---

## How Gates Work

After each phase completes, the **controller** (not the subagent) validates the output by reading the artifact and running through the gate checklist:

1. **Read artifact** — controller reads the phase output (spec file, architecture doc, plan files, etc.)
2. **Run checklist** — controller checks each item in the gate checklist
3. **Pass** — all items checked → proceed to next phase
4. **Fail** — one or more items unchecked → re-dispatch subagent with: `"Gate N failed: {unchecked items}. Fix these."`
5. **Retry limit** — up to 2 re-dispatches per gate. Still failing → log to STATE.md and STOP.

---

## Step 2: Execute Phases

Run phases -1 through 4 sequentially. After each phase: update STATE.md, run gate check. If gate fails: re-dispatch with feedback (up to 2 retries). If still failing: log to STATE.md and STOP.

---

### Phase -1: Scaffold (Greenfield Only)

**Skip if:** Extension mode (CLAUDE.md exists).

**Runs as:** Controller executes directly (no subagent).

**Steps:**

1. Create directory structure:
   ```
   {project}/
   ├── backend/
   ├── frontend/
   └── _docs/
   ```

2. Create `.gitignore`:
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

3. Create placeholder `CLAUDE.md` (populated after Phase 0):
   ```markdown
   # CLAUDE.md — {project_name}

   ## Project
   {to be populated after Phase 0}

   ## Tech Stack
   - Backend: .NET 9, ASP.NET Core, EF Core 9, SQLite
   - Frontend: React 19, Vite, TypeScript, Tailwind CSS v4, Zustand

   ## Commands

   ### Backend
   ```bash
   cd backend && dotnet restore && dotnet build && dotnet run
   ```

   ### Frontend
   ```bash
   cd frontend && npm install && npm run dev
   ```

   ### Tests
   ```bash
   cd backend && dotnet test
   cd frontend && npm test
   ```
   ```

4. Git init + initial commit:
   ```bash
   git init
   git add .
   git commit -m "chore: initial scaffold"
   ```

**Gate -1 Checklist:**
- [ ] Directory `backend/` exists
- [ ] Directory `frontend/` exists
- [ ] Directory `_docs/` exists
- [ ] File `CLAUDE.md` exists
- [ ] File `.gitignore` exists
- [ ] Git repo initialized (`.git/` directory exists)
- [ ] At least one commit exists (`git log` returns a commit)

**On failure:** Re-dispatch with: `"Gate -1 failed: {unchecked items}. Fix these."`

**Update STATE.md:** Phase -1 complete.

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
1. Write product spec to _docs/specs/{DATE}-{feature}.md using the template below
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

**Gate 0 Checklist:**
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
1. Write architecture doc to _docs/specs/{DATE}-{feature}-architecture.md
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

**Gate 1 Checklist:**
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
| Scaffold | `saurun:dotnet-writing-plans` then `saurun:react-writing-plans` |
| Backend | `saurun:dotnet-writing-plans` |
| Frontend | `saurun:react-writing-plans` |
| Integration | `saurun:dotnet-writing-plans` then `saurun:react-writing-plans` |

**Subagent prompt (per work unit):**

```
Task(subagent_type="general-purpose", prompt="""
{AUTONOMOUS_PREAMBLE}

You are executing Phase 2 (Planning) of the god-agent pipeline.
Generate an implementation plan for work unit: {unit_name}

ARCHITECTURE DOC: {contents of architecture doc}
PRODUCT SPEC: {contents of product spec}
STATE: {STATE.md contents}

SKILL TO LOAD: {saurun:dotnet-writing-plans | saurun:react-writing-plans}
Load via Skill tool. Use dotnet-writing-plans for backend work units, react-writing-plans for frontend.

CONTEXT FOR PLAN WRITER:
{relevant sections of architecture doc for this work unit}

OUTPUT: Write plan to _docs/plans/{DATE}-{unit_name}.md
""")
```

**After all Phase 2 subagents complete**, the controller writes `_docs/plans/MANIFEST.json`:

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

Array order is execution order. The .NET + React stack has predictable, near-linear dependencies (backend before frontend), so DAG complexity is unnecessary.

**Gate 2 Checklist (per plan):**
- [ ] Plan file exists at `_docs/plans/{DATE}-{unit_name}.md`
- [ ] Every task has explicit file paths (no vague "update the controller")
- [ ] Every task specifies TDD workflow (test first, then implement)
- [ ] No getter/setter tests for anemic entities
- [ ] Test names follow `MethodName_Scenario_Expected` convention
- [ ] "What bugs do these tests catch?" table exists and has at least 1 row

**Gate 2 Checklist (MANIFEST.json):**
- [ ] File `_docs/plans/MANIFEST.json` exists
- [ ] `plans` array contains all applicable work units
- [ ] Plans are in correct execution order (backend plans before frontend plans)
- [ ] Each plan entry has `id` and `path` fields
- [ ] Each `path` references an existing file

**On failure:** Re-dispatch the failing plan's subagent with: `"Gate 2 failed for {plan_name}: {unchecked items}. Fix these."`

**Update STATE.md:** Phase 2 complete, list all plan paths.

---

### Phase 3: Execution (Plans -> Working Code)

**You (the controller) run this directly.** Do NOT delegate Phase 3 to a single subagent. You orchestrate the task loop yourself, dispatching individual implementer/reviewer subagents per task. This preserves cross-phase context from Phases 0-2.

**Load skill:** `superpowers:subagent-driven-development` — follow its process for the implement/review loop.

**Orchestration loop:**

```
Read MANIFEST.json -> plans array defines execution order
For each plan (in array order):
  Read the plan file, extract all tasks with full text
  For each task:
    1. Update STATE.md: current position
    2. Dispatch IMPLEMENTER subagent (see agent routing below)
       - Use specialized agent: saurun:backend-implementer or saurun:frontend-implementer
       - Provide: full task text, autonomous preamble, product spec summary,
         architecture context, previous task outcomes
       - Skills are pre-loaded by the agent (no skill-loading instructions needed)
       - If implementer asks questions -> answer from Phase 0/1 context
    3. Dispatch REVIEWER subagent (single-pass, combined spec + quality review)
       - Uses superpowers:code-reviewer with stack-specific quality criteria
       - Pass -> continue
       - Fail (Critical/Important) -> resume implementer with fix instructions -> re-review (up to 2 retries)
       - Fail (Minor) -> note and continue
    4. Update STATE.md: task complete
  After all tasks in plan:
    Run full test suite: dotnet test backend/ && npm test (if frontend exists)
    Pass -> next plan
    Fail -> dispatch superpowers:systematic-debugging subagent (up to 2 attempts)
    Still fails -> STOP and report
```

> **Design note:** Single-pass review is sufficient because: (1) the implementer self-reviews before completing, (2) one combined review is more efficient (single dispatch, all feedback at once), and (3) less orchestration complexity.

**Example implementer dispatch (backend):**

```
Task(subagent_type="saurun:backend-implementer", prompt="""
{AUTONOMOUS_PREAMBLE}

TASK: {full task text from plan}

PRODUCT SPEC SUMMARY: {key points from Phase 0 spec}

ARCHITECTURE CONTEXT: {relevant sections from Phase 1 architecture doc}

PREVIOUS TASK OUTCOMES: {summary of completed tasks in this plan}
""")
```

**Example implementer dispatch (frontend):**

```
Task(subagent_type="saurun:frontend-implementer", prompt="""
{AUTONOMOUS_PREAMBLE}

TASK: {full task text from plan}

PRODUCT SPEC SUMMARY: {key points from Phase 0 spec}

ARCHITECTURE CONTEXT: {relevant sections from Phase 1 architecture doc}

PREVIOUS TASK OUTCOMES: {summary of completed tasks in this plan}
""")
```

**Example reviewer dispatch (single-pass):**

```
Task(subagent_type="superpowers:code-reviewer", prompt="""
Review this implementation for both spec compliance and code quality.

TASK SPEC:
{task spec from plan}

WHAT WAS IMPLEMENTED:
{implementer's report}

DIFF: Compare {BASE_SHA}..{HEAD_SHA}

{ADDITIONAL_REVIEW_CRITERIA}
""")
```

**Quality criteria loading:** The `ADDITIONAL_REVIEW_CRITERIA` section is populated by loading the appropriate criteria template skill and extracting its criteria:
- Backend tasks: load `saurun:dotnet-code-quality-reviewer-prompt`, extract criteria section
- Frontend tasks: load `saurun:react-code-quality-reviewer-prompt`, extract criteria section

These skills are "criteria templates" — not directly invocable reviewers, but sources of stack-specific review criteria to paste into the unified review prompt.

**Agent routing:**

| Plan Type | Implementer Agent | Pre-loaded Skills | Review Criteria Source |
|-----------|-------------------|-------------------|------------------------|
| Backend domain | `saurun:backend-implementer` | `dotnet-tactical-ddd`, `dotnet-tdd` | `saurun:dotnet-code-quality-reviewer-prompt` |
| Backend API | `saurun:backend-implementer` | `dotnet-tactical-ddd`, `dotnet-tdd` | `saurun:dotnet-code-quality-reviewer-prompt` |
| Backend SignalR | `saurun:backend-implementer` | `dotnet-tactical-ddd`, `dotnet-tdd` | `saurun:dotnet-code-quality-reviewer-prompt` |
| Frontend state | `saurun:frontend-implementer` | `react-tailwind-v4-components`, `frontend-design`, `react-tdd` | `saurun:react-code-quality-reviewer-prompt` |
| Frontend pages | `saurun:frontend-implementer` | `react-tailwind-v4-components`, `frontend-design`, `react-tdd` | `saurun:react-code-quality-reviewer-prompt` |
| Integration | Both agents | All skills (via agents) | Both criteria templates |

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

**Gate 3 Checklist:**
- [ ] Every plan in MANIFEST.json has status "Complete" in STATE.md Phase Tracker
- [ ] Backend tests pass: `dotnet test backend/ --verbosity minimal` returns 0 failures
- [ ] Frontend tests pass (if frontend exists): `npm test` returns 0 failures
- [ ] STATE.md Failures table has no unresolved entries (all have Resolution)
- [ ] Git log contains at least 1 commit per plan executed
- [ ] Git working tree is clean: `git status --porcelain` returns empty

**On failure:** Identify which plan/task failed, dispatch `superpowers:systematic-debugging` with failure details, then re-run gate.

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
4. Write completion report to _docs/reports/{DATE}-{feature}-report.md

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

**Gate 4 Checklist:**
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
| -1. Scaffold | Pending/Skipped | — | — |
| 0. Intake | Pending | — | — |
| 1. Architecture | Pending | — | — |
| 2. Planning | Pending | — | — |
| 3. Execution | Pending | — | — |
| 4. Integration | Pending | — | — |

## Current Position
- **Phase:** -1
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
