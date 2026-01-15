# Phase 1: Initialize

## Purpose
Set up or restore execution state for the long-run implementation session.

## Inputs
- `spec_name` (optional): Name of spec to implement
- Resume from previous session if state exists

## Workflow

### Step 1: Locate Spec

```
IF spec_name provided:
  spec_path = agent-os/specs/{spec_name}/
ELSE:
  List available specs in agent-os/specs/
  Ask user to select one
  spec_path = selected spec path
```

### Step 2: Validate Required Files

```
REQUIRED:
  - {spec_path}/tasks.md

OPTIONAL (at least one):
  - {spec_path}/spec.md
  - {spec_path}/planning/requirements.md

IF tasks.md missing:
  Output: "tasks.md not found. Run /create-tasks first."
  STOP
```

### Step 3: Check for Existing State

```
IF {spec_path}/.long-run/ exists:
  Use {{workflows/long-run/detect-resume}} to check state

  IF interrupted:
    Present resume options to user
    Handle user choice
  ELSE:
    Present: "Previous run completed. Start fresh? [y/n]"
```

### Step 4: Initialize State (if new)

```
IF no existing state OR user chose fresh start:
  1. Create directory structure:
     mkdir -p {spec_path}/.long-run/plans
     mkdir -p {spec_path}/.long-run/summaries

  2. Copy templates:
     - STATE.md from {{templates/long-run-state}}
     - agent-history.json from {{templates/agent-history.json}}

  3. Update STATE.md:
     - Spec: {spec_path}
     - Tasks: {spec_path}/tasks.md
     - Started: {today's date}
     - Status: Initializing
```

### Step 5: Pre-Flight Checks

```
1. Record start commit:
   start_commit = git rev-parse HEAD
   Write to STATE.md

2. Check working tree:
   IF git status --porcelain has output:
     Present options:
       1. Stash changes
       2. Commit changes first
       3. Abort
     Handle user choice
```

### Step 6: Output

```
Output to user:
  "✅ Initialized long-run session for: {spec_name}

   Spec: {spec_path}
   Tasks: {task_count} task groups found
   State: {spec_path}/.long-run/

   Proceeding to transform phase..."

Pass to Phase 2:
  - spec_path
  - state_path: {spec_path}/.long-run/
  - is_resume: boolean
  - resume_from: plan number if resuming
```

## Error Handling

```
ON any error:
  Log to STATE.md
  Present error to user
  Offer: Retry / Abort
```
