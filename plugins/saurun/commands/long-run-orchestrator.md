# Long-Run Orchestrator Command

## Overview

Execute-only entry point for running pre-prepared plans. Use this when `.long-run/plans/` already contains plan files (created by plan-fixer or `/long-run-implement --prepare-only`).

## Usage

```
/long-run-orchestrator [spec-path]
```

**Arguments:**
- `spec-path`: Path to spec folder containing `.long-run/plans/` directory

## When to Use

| Scenario | Command |
|----------|---------|
| Full pipeline (prep + execute) | `/long-run-implement spec-name` |
| Prepare only, review before execute | `/long-run-implement spec-name --prepare-only` |
| Execute pre-prepared plans | `/long-run-orchestrator spec-path` |
| Resume interrupted execution | `/long-run-orchestrator spec-path` |

## What This Command Does

1. **Validate** - Ensure `.long-run/plans/` exists with plan files
2. **Execute** - Spawn fresh subagent per plan via orchestrator
3. **Finalize** - Present summary and verification options

**Does NOT:**
- Run plan-fixer
- Create `.long-run/` directory
- Transform tasks.md

## Prerequisites

Before running this command, ensure:
- `.long-run/plans/` contains `NN-PLAN.md` files
- `.long-run/STATE.md` exists
- Plans are in correct format (`<task type="auto"><name>...</name>`)

These are created by either:
- `/long-run-implement spec-name --prepare-only`
- `/plan-fixer tasks.md` (with Phase 4 splitting)

## Workflow

### Step 1: Validate State

```
1. Check spec_path exists
2. Check {spec_path}/.long-run/ exists
3. Check {spec_path}/.long-run/plans/ contains NN-PLAN.md files
4. Check {spec_path}/.long-run/STATE.md exists

IF any check fails:
  Report error with suggestion:
    "No prepared plans found at {spec_path}/.long-run/plans/

     Run one of:
     - /long-run-implement {spec-name} --prepare-only  (to prepare plans)
     - /plan-fixer {spec_path}/tasks.md               (to create plans)

     Then run this command again."
  STOP
```

### Step 2: Load State

```
1. Read STATE.md for:
   - Current plan position
   - Configuration (timeout, etc.)
   - Metrics

2. Read agent-history.json (if exists)

3. Check for interrupted execution:
   - current-agent-id.txt exists → offer resume
   - CHECKPOINT.json exists → partial plan resume
```

### Step 3: Execute Plans

Delegate to orchestrator agent:

```
Spawn {{agents/long-run-orchestrator}} with:
  spec_path: {spec_path}

The orchestrator will:
  - For each plan in .long-run/plans/:
    - Skip if SUMMARY.md exists (completed)
    - Resume from CHECKPOINT.json if exists
    - Spawn fresh executor subagent
    - Handle response (COMPLETED, NEEDS_DECISION, etc.)
    - Update state files
```

### Step 4: Finalize

```
1. Aggregate all summaries
2. Update STATE.md with final status
3. Present completion report:

   "Execution Complete

   Plans: {completed}/{total} completed
   Commits: {total_commits}
   Files modified: {file_count}

   Summaries: {list of summary paths}"

4. Offer options:
   - View detailed summary
   - Run verification commands
   - Clean up state files
```

## Error Handling

| Error | Action |
|-------|--------|
| No .long-run/ directory | Suggest running /long-run-implement --prepare-only |
| Empty plans/ directory | Suggest running plan-fixer |
| Invalid plan format | Report specific issues, suggest re-running plan-fixer |
| Executor failure | Offer retry/skip/abort via orchestrator |

## Resume Capability

If execution is interrupted, simply run the command again:

```
/long-run-orchestrator spec-path
```

The command will:
1. Detect existing state
2. Find last checkpoint
3. Offer resume options
4. Continue from where it left off

## Relation to /long-run-implement

This command is equivalent to:
```
/long-run-implement spec-name --execute-only
```

The difference:
- `/long-run-implement` takes a spec NAME (looks up in `agent-os/specs/`)
- `/long-run-orchestrator` takes a spec PATH (direct path)

## Agents Used

- **{{agents/long-run-orchestrator}}** - Spawns subagents per plan, handles responses
- **{{agents/long-run-executor}}** - Executed as subagent per plan

## Workflows Used

- {{workflows/long-run/spawn-plan-executor}}
- {{workflows/long-run/track-agent}}
- {{workflows/long-run/atomic-commit}}
- {{workflows/long-run/apply-deviation-rules}}
