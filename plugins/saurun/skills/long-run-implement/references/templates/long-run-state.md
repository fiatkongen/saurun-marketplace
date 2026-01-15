# Long-Run Implementation State

## Spec Reference
Spec: agent-os/specs/[spec-name]/
Tasks: agent-os/specs/[spec-name]/tasks.md
Started: [YYYY-MM-DD]

## Current Position
Plan: [X] of [Y]
Task Group: [current-group-name]
Status: [Initializing / Transforming / Executing / Complete]
Last activity: [YYYY-MM-DD] - [description]
Progress: [░░░░░░░░░░] 0%

**Progress Calculation:** `(completed_plans / total_plans) * 100`
- Use 10-character bar: each █ = 10%
- Example: 3/5 plans = 60% = `[██████░░░░] 60%`

## Execution Metrics
Plans completed: [N]
Tasks completed: [M]
Total commits: [K]
Duration: [X hours Y minutes]

## Rollback Reference
start_commit: [hash recorded at Phase 1 initialization]

## Accumulated Context
### Decisions Made
| Plan | Decision | Rationale |
|------|----------|-----------|

### Deferred Issues
- ISS-XXX: [description] (Plan N)

### Blockers
None.

## Session Continuity
Last session: [YYYY-MM-DD HH:MM]
Stopped at: [description]
Resume agent: [agent_id or "None"]

## Configuration
clarification_timeout_minutes: 10
timeout_per_plan_minutes: 30
timeout_action: prompt
