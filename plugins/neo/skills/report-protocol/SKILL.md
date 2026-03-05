---
name: report-protocol
description: >-
  Standardized reporting protocol for implementer agents spawned by the foreman.
  Defines the JSON report format, commit requirements, and status reporting.
---

# Report Protocol

Every implementer agent MUST write a structured report when finished. The foreman reads these to determine success/failure and decide next steps.

## Report File

Write to `.neo/reports/t<N>.json` where `<N>` is the task number from the plan.

```json
{
  "taskId": "T<N>",
  "status": "done",
  "summary": "Implemented user authentication with JWT tokens",
  "filesChanged": ["src/Auth/AuthService.cs", "src/Controllers/AuthController.cs"],
  "blockers": []
}
```

### Fields

| Field | Required | Values |
|-------|----------|--------|
| `taskId` | Yes | `T<N>` matching the plan |
| `status` | Yes | `done` or `failed` |
| `summary` | Yes | One-line description of what was done |
| `filesChanged` | Yes | Array of modified file paths |
| `blockers` | Yes | Array of blocking issues (empty if none) |

### On Failure

```json
{
  "taskId": "T3",
  "status": "failed",
  "summary": "Could not implement — dependency X missing from project",
  "filesChanged": [],
  "blockers": ["Package X not available for net9.0", "No fallback documented in plan"]
}
```

## Commit Requirements

Before writing the report:
1. Ensure all implementation files are committed
2. Ensure tests pass (if verify commands are specified)
3. Commit the report file itself

```bash
git add -A
git commit -m "T<N>: <summary>"
```

## Foreman Status Report

The foreman aggregates reports and outputs:

```
✅ 3/4 tasks done
❌ T3 failed (2 attempts): <reason from report>
⏸ T4 blocked (depends on T3)
Ready for review + push?
```
