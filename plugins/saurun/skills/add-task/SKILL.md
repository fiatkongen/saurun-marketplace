---
name: add-task
description: Quickly add tasks to a markdown task list from natural language. Use when the user says "add task", "add to my todo", "remember to", "I need to", or wants to append a new task to their task file. Supports automatic dependency parsing and proper formatting.
---

# Add Task

Quickly add tasks to a markdown task list using natural language. Automatically parses dependencies and formats tasks correctly.

## Usage

User says naturally:
- "Add task to fix the authentication bug"
- "Add a task to deploy after tests pass"
- "Remember to update the documentation"
- "I need to write unit tests"

## Workflow

### 1. Parse User Intent

Extract from natural language:
- **Task description**: Main action to perform
- **Dependencies**: Look for phrases like "after X", "once Y is done", "depends on Z"
- **Target file**: If not provided, ask user for task file path

**Dependency patterns to detect:**
- "after [task]" → `(Depends on: [task])`
- "once [task] is done" → `(Depends on: [task])`
- "when [task] completes" → `(Depends on: [task])`
- "depends on [task]" → `(Depends on: [task])`

### 2. Read Task File

Read the existing task file to:
- Verify file exists and has correct structure
- Check for duplicate tasks (avoid adding same task twice)
- Find the `## Pending` section

Expected format:
```markdown
## Pending
- Existing task 1
- Existing task 2

## In Progress

## Completed

## Failed
```

### 3. Format Task

Format the new task properly:
- Start with `- ` (markdown list item)
- Clear, actionable description
- Add dependency annotation if detected: `(Depends on: X)`

**Examples:**
```markdown
- Fix authentication bug
- Deploy to staging (Depends on: Run integration tests)
- Update API documentation
```

### 4. Append to Pending

Use Edit tool to add the task to the `## Pending` section:

```
Old: ## Pending
- Existing task 1
- Existing task 2

New: ## Pending
- Existing task 1
- Existing task 2
- [new task]
```

### 5. Confirm Addition

Report back to user what was added:
```
✅ Added to Pending: "[task description]"
```

If dependency was detected:
```
✅ Added to Pending: "[task description]" (Depends on: [dependency])
```

## Examples

**Simple task:**
```
User: "Add task to fix login bug"
Claude: ✅ Added to Pending: "Fix login bug"
```

**Task with dependency:**
```
User: "Add task to deploy to production after tests pass"
Claude: ✅ Added to Pending: "Deploy to production (Depends on: tests pass)"
```

**Multiple tasks:**
```
User: "Add tasks: fix bug, write tests, deploy"
Claude: ✅ Added 3 tasks to Pending:
- Fix bug
- Write tests
- Deploy
```

## Duplicate Detection

Before adding, check if similar task already exists:
- Compare task descriptions (case-insensitive, fuzzy match)
- If very similar task found, ask user: "Similar task exists: '[existing]'. Add anyway?"

## Key Principles

**Natural language first**: User doesn't need to remember syntax

**Smart parsing**: Automatically detect dependencies from context

**Quick confirmation**: Simple success message, don't block user

**Context efficient**: Single read + single edit = ~300 tokens per add

## Complementary Skills

Works perfectly with `autonomous-task-executor`:
- Use `add-task` to build your task list
- Use `autonomous-task-executor` to work through it
