# Long Run

Execute a PLAN.md file autonomously with minimal checkpoints, per-task commits, and deviation handling.

## Usage

```
/long-run <plan_file> [output_dir]
```

## Parameters

- `plan_file` (required): Path to the PLAN.md file to execute
- `output_dir` (optional): Directory for SUMMARY.md and checkpoint files (defaults to `.long-run/` next to plan)

## Examples

```
/long-run ./PLAN.md
/long-run ./features/auth/PLAN.md
/long-run ./PLAN.md ./output/.long-run
```

## Behavior

This command spawns the `long-run-executor` agent to handle the plan execution. The agent will:

1. **Minimize stops** - Only pause for:
   - Rule 4 architectural decisions
   - External blockers (email verification, manual approvals)
   - Unrecoverable errors after 3 retries

2. **Per-task commits** - Commit immediately after each task verification passes

3. **Deviation handling** - Apply deviation rules when tasks fail or need adjustment

4. **Checkpoint support** - Save progress to allow resuming interrupted runs

## Instructions

When this command is invoked:

1. Read the plan file from the first argument
2. Parse YAML frontmatter to extract the `plan` number
3. Determine output directory:
   - If second argument provided: use it
   - Otherwise: use `{plan_file_dir}/.long-run/`
4. Create output directory if it doesn't exist
5. Check for existing checkpoint to support resume
6. Begin task execution following the long-run-executor agent protocol

Use the Task tool to spawn a subagent with the `long-run-executor` agent type if available, otherwise follow the agent instructions directly from `agents/long-run-executor.md`.
