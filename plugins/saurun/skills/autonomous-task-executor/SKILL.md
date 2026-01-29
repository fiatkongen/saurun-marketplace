---
name: autonomous-task-executor
description: Autonomously execute tasks from a markdown task list with dependency awareness, blocker detection, and retry logic. Use when the user asks to "work through tasks", "execute pending tasks", "run the task list", "continue with tasks", or wants automated task execution from a markdown file with sections (Pending, In Progress, Completed, Failed).
---

# Autonomous Task Executor

Execute tasks from a markdown task list autonomously with intelligent dependency handling, blocker detection, and exponential backoff retry logic.

## Task File Location

**Default location:** `~/tasks.md` (user's home directory)

**Before first execution:**
- Check if `~/tasks.md` exists using: `test -f ~/tasks.md`
- If not, create it with the template structure:
  ```bash
  cat > ~/tasks.md << 'EOF'
  ## Pending

  ## In Progress

  ## Completed

  ## Failed

  EOF
  ```
- User can specify alternative path, but default is always `~/tasks.md`

**Path expansion:**
- Always expand `~` to full home directory path using `$HOME` or `os.path.expanduser()` in scripts
- Example: `~/tasks.md` → `/Users/username/tasks.md`

## Core Workflow

Execute this sequence continuously until all tasks complete or all remaining tasks are blocked:

### 1. Read and Parse Task File

Read the markdown task file from `~/tasks.md` (or user-specified path) once per cycle. Expected format:
```markdown
## Pending
- Task 1
- Task 2 (Depends on: Task 1)

## In Progress
- Currently executing task

## Completed
- Finished task

## Failed
- Failed task (Error: reason)
```

See [task-format.md](references/task-format.md) for complete format reference.

### 2. Find Next Executable Task

Parse the file content to identify the first unblocked pending task:

**Parsing logic:**
- Iterate through lines under `## Pending` section
- For each task line starting with `-`:
  - Check for `(Depends on: X)` - if found, verify X exists in `## Completed` section
  - Check for `(Blocked by: Y)` - if found, task is blocked
  - If no blockers, this is the next executable task

**Iteration behavior:**
- Check each pending task in order
- SKIP blocked tasks, move to next
- Execute FIRST non-blocked task found
- LEAVE blocked tasks in Pending section

**If ALL pending tasks are blocked:**
- Do NOT attempt execution
- Wait 5 minutes
- Restart from step 1 (blockers may be manually resolved)

### 3. Move Task to In Progress

Use Edit tool to move the selected task line from Pending section to In Progress section:

```
Old: (task line under ## Pending)
New: (empty line)

Old: ## In Progress

New: ## In Progress
- [task text]
```

### 4. Execute the Task

Execute the task according to its description. Apply full problem-solving capabilities:
- Read relevant files
- Make necessary code changes
- Run commands
- Verify results

**Error handling:**
- If task fails, capture error details
- Determine if retryable (transient errors) or permanent (logic errors)
- Make retry decision based on error type

### 5. Update Task Status

**On Success:**
Move task from In Progress to Completed using Edit tool.

**On Failure (Non-Retryable):**
Move task from In Progress to Failed, appending `(Error: <description>)`.

**On Failure (Retryable):**
Extract retry count from task text:
- `(Retry 1/3, last failed: reason)` → retry_count = 1
- No annotation → retry_count = 0

Calculate backoff delay:
- Attempt 1: 1 minute (60 seconds)
- Attempt 2: 2 minutes (120 seconds)
- Attempt 3: 4 minutes (240 seconds)

If retry_count < 3:
- Move task back to Pending with annotation: `(Retry X/3, last failed: <reason>)`
- Wait exponential backoff time with `sleep <seconds>`
- Continue to next cycle

If retry_count >= 3:
- Move task to Failed with: `(Error: Max retries exceeded. Last: <reason>)`

### 6. Continue to Next Task

After completing or failing a task, immediately return to step 1 (Read and Parse) and repeat the cycle.

## Blocker Detection

Tasks are blocked when:

**Explicit dependencies:**
```markdown
- Deploy to production (Depends on: Run integration tests)
```
Blocked until "Run integration tests" appears in Completed section.

**Implicit blockers:**
Review Failed section for tasks that may block pending tasks:
- Shared files/resources
- Sequential workflow requirements
- Prerequisites

If a pending task likely depends on a failed task, add blocker annotation:
```markdown
- Update API docs (Blocked by: API endpoint implementation failed)
```

## Key Principles

**Context efficiency:** Read file once per cycle, parse and edit directly. Avoid script calls during execution.

**Continuous execution:** Loop through steps 1-6 without user intervention until completion or all tasks blocked.

**Skip blocked, continue:** When encountering blocked tasks, skip to next available task. Never wait on a single blocked task.

**Exponential backoff:** Transient failures get 3 retries with increasing delays (1m, 2m, 4m).

**Clean edits:** Use Edit tool to move task lines between sections, maintaining markdown structure.

## Helper Scripts (Optional)

Scripts are provided for manual use or debugging. Both default to `~/tasks.md` if no file specified.

**parse_tasks.py:** Analyzes task file, returns next unblocked task
```bash
# Use default ~/tasks.md
python3 scripts/parse_tasks.py

# Or specify custom file
python3 scripts/parse_tasks.py <task-file.md>
```

**move_task.py:** Moves tasks between sections with annotations
```bash
# Use default ~/tasks.md
python3 scripts/move_task.py <task-text> <from> <to> [note]

# Or specify custom file
python3 scripts/move_task.py <file> <task-text> <from> <to> [note]
```

Both located in `scripts/` directory. These are utilities for users, not required for autonomous execution.
