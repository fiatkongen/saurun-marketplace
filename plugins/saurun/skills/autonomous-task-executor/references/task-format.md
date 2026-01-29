# Task File Format Reference

## Structure

Task files use markdown with four required sections:

```markdown
## Pending
- Task 1
- Task 2 (Depends on: Task 1)
- Task 3

## In Progress
- Task being worked on

## Completed
- Finished task 1
- Finished task 2

## Failed
- Task that encountered errors (Error: description)
```

## Dependency Syntax

**Explicit dependencies:**
```markdown
- Implement feature X (Depends on: Setup database)
```

**Blocker annotations:**
```markdown
- Deploy to production (Blocked by: Fix authentication bug)
```

## Task State Transitions

```
Pending → In Progress → Completed
                      ↘ Failed
```

Tasks in Failed section can be retried (moved back to Pending with retry annotations).

## Retry Annotations

When a task fails and is retried:
```markdown
- Run integration tests (Retry 1/3, last failed: timeout)
```

Format: `(Retry X/Y, last failed: reason)`

## Examples

### Simple task list:
```markdown
## Pending
- Write unit tests
- Update documentation
- Deploy to staging

## In Progress

## Completed
- Setup project
- Configure CI/CD

## Failed
```

### With dependencies:
```markdown
## Pending
- Run integration tests (Depends on: Deploy to staging)
- Deploy to production (Depends on: Run integration tests)

## In Progress
- Deploy to staging

## Completed
- Write unit tests
- Update documentation

## Failed
```

### With blockers:
```markdown
## Pending
- Deploy to production (Blocked by: Database migration failed)
- Update API docs

## In Progress

## Completed
- Write unit tests

## Failed
- Database migration (Error: permission denied)
```
