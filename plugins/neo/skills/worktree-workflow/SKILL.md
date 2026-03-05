---
name: worktree-workflow
description: >-
  Git worktree isolation for parallel Claude Code agents. Each task runs in its own
  worktree branch, main stays clean, merging happens sequentially after verification.
---

# Worktree Workflow

Every implementation task MUST run in an isolated git worktree. No task ever touches main directly.

## Spawning

```bash
claude -w t<N>-<slug> -p "<task prompt>" --dangerously-skip-permissions --model claude-opus-4-6
```

What `-w t1-backend` does:
- Creates `.claude/worktrees/t1-backend/` with branch `worktree-t1-backend`
- Agent works in complete isolation — main is untouched
- You merge manually afterward

## Parallel Execution

```bash
claude -w t1-backend -p "..." --dangerously-skip-permissions --model claude-opus-4-6 &
claude -w t2-frontend -p "..." --dangerously-skip-permissions --model claude-opus-4-6 &
wait
```

## Merge Procedure

One at a time. Never two simultaneously.

```bash
git checkout main
git merge worktree-t<N>-<slug> --no-ff -m "Merge T<N>: <title>"
# Run verify commands from plan
git worktree remove .claude/worktrees/t<N>-<slug> --force
git branch -D worktree-t<N>-<slug>
```

## State Tracking

Write `.neo/state.json` before first spawn, update after every state change:

```json
{
  "plan": "_docs/plans/<file>.md",
  "startedAt": "<ISO>",
  "tasks": {
    "T1": { "status": "pending|running|done|failed", "agent": "backend-implementer", "deps": [] }
  }
}
```

## Error Handling

| Scenario | Action |
|----------|--------|
| Task fails | Retry 1x with error context in prompt |
| Retry fails | Mark failed, continue independent tasks |
| Merge conflict | Spawn fix agent in the worktree |
| Timeout (>45 min) | Kill process, retry 1x |
| All tasks blocked | Report full blocker details |

## File Structure

```
<project>/
  .neo/
    state.json         ← Foreman state (not committed, add to .gitignore)
    reports/t<N>.json  ← Written by implementer agents
  .claude/
    worktrees/         ← Created by -w flag (git managed)
```
