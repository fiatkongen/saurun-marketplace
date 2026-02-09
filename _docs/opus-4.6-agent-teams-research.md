# Opus 4.6 & Agent Teams Research

> Research date: 2026-02-06

## Opus 4.6 — Key New Capabilities

### Model Improvements
- **1M token context window** (beta) — first for Opus-class. Premium pricing ($10/$37.50/M tokens) for prompts >200k
- **128k output tokens** — larger tasks can complete in single requests
- **MRCR v2 score: 76%** (8-needle, 1M context) vs Sonnet 4.5's 18.5% — massive long-context reliability improvement
- Top score on Terminal-Bench 2.0 (agentic coding) and Humanity's Last Exam
- Outperforms GPT-5.2 by ~144 Elo on GDPval-AA (finance, legal knowledge work)

### API Enhancements
- **Adaptive Thinking**: Model picks up contextual clues about how much extended thinking to use (no longer binary on/off)
- **Effort Controls**: 4 levels — `low`, `medium`, `high`, `max` — granular intelligence/speed/cost tradeoffs
- **Context Compaction** (beta): Auto-summarizes older context when approaching limits, enabling longer agentic sessions
- **Pricing**: Same as before — $5/$25 per million tokens (standard context)

### Other
- Claude in Excel improvements (interprets messy spreadsheets without explicit explanations)
- Claude in PowerPoint (research preview) — auto-matches existing colors, fonts, layouts

---

## Agent Teams (Swarm Mode)

### What It Is
Multi-agent orchestration built into Claude Code. One session acts as **team lead** (coordinator), spawning **teammates** (independent Claude Code instances) that work in parallel with shared task tracking and direct inter-agent messaging.

### How to Enable

```json
// settings.json
{
  "env": {
    "CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS": "1"
  }
}
```

Or set `CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=1` in shell environment.

### Architecture

| Component | Role |
|-----------|------|
| **Team Lead** | Main session. Creates team, spawns teammates, coordinates, synthesizes |
| **Teammates** | Separate Claude Code instances, each with own context window |
| **Task List** | Shared work queue with dependency tracking. Auto-unblocks when blockers complete |
| **Mailbox** | Direct agent-to-agent messaging system |

Storage:
- Team config: `~/.claude/teams/{team-name}/config.json`
- Tasks: `~/.claude/tasks/{team-name}/`

### Agent Teams vs Subagents

| | Subagents | Agent Teams |
|---|---|---|
| **Context** | Own window; results return to caller | Own window; fully independent |
| **Communication** | Report back to main agent only | Teammates message each other directly |
| **Coordination** | Main agent manages all | Shared task list, self-coordination |
| **Best for** | Focused tasks where only result matters | Complex work requiring discussion |
| **Token cost** | Lower | Higher (each teammate = separate Claude instance) |

**Rule of thumb**: Use subagents for quick focused work. Use agent teams when teammates need to share findings, challenge each other, and coordinate independently.

### Display Modes

1. **In-process** (default): All teammates in main terminal
   - `Shift+Up/Down` — select teammate
   - `Enter` — view teammate's session
   - `Escape` — interrupt teammate's turn
   - `Ctrl+T` — toggle task list

2. **Split panes**: Each teammate gets own terminal pane (requires tmux or iTerm2)
   ```json
   { "teammateMode": "tmux" }
   ```
   Or: `claude --teammate-mode in-process`

### Key Controls

- **`Shift+Tab`**: Enable delegate mode — restricts lead to coordination only (no code touching)
- **Direct messaging**: Talk to any teammate without going through lead
- **Plan approval**: Can require teammates to plan before implementing; lead reviews/approves

### Creating Teams

Natural language — describe task and team structure:

```
Create an agent team to review PR #142. Spawn three reviewers:
- One focused on security implications
- One checking performance impact
- One validating test coverage
Have them each review and report findings.
```

Can specify model per teammate:
```
Create a team with 4 teammates to refactor these modules in parallel.
Use Sonnet for each teammate.
```

### Best Use Cases

1. **Research & review**: Multiple teammates investigate different aspects simultaneously, share and challenge findings
2. **New modules/features**: Each teammate owns a separate piece without stepping on each other
3. **Debugging with competing hypotheses**: Test different theories in parallel, converge faster
4. **Cross-layer coordination**: Frontend/backend/tests each owned by different teammate
5. **Parallel code review**: Split security, performance, test coverage across reviewers

### Orchestration Patterns

| Pattern | Description |
|---------|-------------|
| **Leader (Parallel Specialists)** | Multiple experts review/work simultaneously, each reports back independently |
| **Pipeline (Sequential Dependencies)** | Tasks execute in sequence; completed blockers auto-unblock dependent tasks |
| **Swarm (Self-Organizing)** | Workers continuously poll/claim available tasks, naturally load-balancing |
| **Research-then-Implement** | Initial research phase informs subsequent implementation phase |

### Task Management

- Tasks have 3 states: `pending`, `in_progress`, `completed`
- Dependencies auto-resolve (blocked tasks unblock when blockers complete)
- File locking prevents race conditions on task claiming
- Lead can assign explicitly, or teammates self-claim next unblocked task
- Aim for **5-6 tasks per teammate** for good productivity

### Limitations

- **No session resumption**: `/resume` and `/rewind` don't restore in-process teammates
- **One team per session**: Clean up before starting a new one
- **No nested teams**: Teammates can't spawn their own teams
- **Lead is fixed**: Can't promote a teammate or transfer leadership
- **Permissions set at spawn**: All teammates inherit lead's permissions
- **Split panes**: Not supported in VS Code terminal, Windows Terminal, or Ghostty
- **Task status can lag**: May need manual updates sometimes
- **Token-heavy**: Each teammate is a separate Claude instance

### Best Practices

1. **Give enough context in spawn prompts** — teammates don't inherit lead's conversation history
2. **Size tasks right** — not too small (overhead), not too large (wasted effort). Self-contained units with clear deliverables
3. **Avoid file conflicts** — assign different files to different teammates
4. **Start with research/review** — lower risk than parallel implementation
5. **Monitor and steer** — check in on progress, redirect approaches that aren't working
6. **Pre-approve permissions** — reduce interruptions from permission prompts

---

## Custom Subagents (Enhanced in Opus 4.6 era)

### What's New
The subagent system has matured significantly. Key additions:
- **`/agents` command**: Interactive UI for creating, editing, deleting subagents
- **Persistent memory**: Subagents can maintain knowledge across sessions (`memory: user|project|local`)
- **Lifecycle hooks**: `PreToolUse`, `PostToolUse`, `SubagentStart`, `SubagentStop` events
- **Skills preloading**: Inject skill content at subagent startup via `skills` frontmatter
- **Background execution**: Subagents can run concurrently with `Ctrl+B` or explicit request
- **Resumable subagents**: Resume with full context history instead of starting fresh
- **Auto-compaction**: Subagents compact at ~95% capacity (configurable via `CLAUDE_AUTOCOMPACT_PCT_OVERRIDE`)

### Subagent Definition (Markdown with YAML Frontmatter)

```markdown
---
name: code-reviewer
description: Expert code review specialist. Use proactively after code changes.
tools: Read, Grep, Glob, Bash
disallowedTools: Write, Edit
model: sonnet
permissionMode: dontAsk
skills:
  - api-conventions
  - error-handling-patterns
memory: user
hooks:
  PreToolUse:
    - matcher: "Bash"
      hooks:
        - type: command
          command: "./scripts/validate-command.sh"
---

You are a senior code reviewer. [system prompt here]
```

### Frontmatter Fields

| Field | Required | Description |
|-------|----------|-------------|
| `name` | Yes | Unique identifier (lowercase + hyphens) |
| `description` | Yes | When Claude should delegate (drives auto-invocation) |
| `tools` | No | Allowlist. Inherits all if omitted |
| `disallowedTools` | No | Denylist, removed from inherited/specified |
| `model` | No | `sonnet`, `opus`, `haiku`, `inherit` (default: inherit) |
| `permissionMode` | No | `default`, `acceptEdits`, `dontAsk`, `bypassPermissions`, `plan` |
| `skills` | No | Skills injected at startup (full content, not just availability) |
| `memory` | No | `user`, `project`, `local` — enables persistent cross-session learning |
| `hooks` | No | Lifecycle hooks scoped to this subagent |

### Scope/Priority (highest to lowest)
1. `--agents` CLI flag (session-only, JSON)
2. `.claude/agents/` (project-level, version-controllable)
3. `~/.claude/agents/` (user-level, all projects)
4. Plugin `agents/` directory

### Persistent Memory

When `memory` is set, subagent gets a persistent directory:
- `user` → `~/.claude/agent-memory/<name>/`
- `project` → `.claude/agent-memory/<name>/`
- `local` → `.claude/agent-memory-local/<name>/`

First 200 lines of `MEMORY.md` auto-loaded into system prompt. Subagent can read/write to build knowledge over time.

### Agent SDK — Programmatic Subagents

For building standalone agent apps (not Claude Code plugins):

```python
from claude_agent_sdk import query, ClaudeAgentOptions, AgentDefinition

async for message in query(
    prompt="Review the auth module",
    options=ClaudeAgentOptions(
        allowed_tools=["Read", "Grep", "Glob", "Task"],
        agents={
            "code-reviewer": AgentDefinition(
                description="Expert code reviewer",
                prompt="You are a code review specialist...",
                tools=["Read", "Grep", "Glob"],
                model="sonnet"
            )
        }
    )
):
    if hasattr(message, "result"):
        print(message.result)
```

Key SDK features:
- Dynamic agent configuration (factory functions returning AgentDefinition)
- Detect subagent invocation via `tool_use` blocks with `name: "Task"`
- Resume subagents with `resume: sessionId` parameter
- Tool restrictions per agent
- `parent_tool_use_id` field identifies messages from within subagent context

### Built-in Subagent Types

| Agent | Model | Purpose |
|-------|-------|---------|
| **Explore** | Haiku | Fast, read-only codebase search/analysis |
| **Plan** | Inherit | Research during plan mode |
| **General-purpose** | Inherit | Complex multi-step tasks (all tools) |
| **Bash** | Inherit | Terminal commands in separate context |
| **Claude Code Guide** | Haiku | Questions about Claude Code features |

### When to Use What

| Scenario | Use |
|----------|-----|
| Quick focused task, only result matters | **Subagent** |
| Verbose output you don't want in main context | **Subagent** |
| Enforce specific tool restrictions | **Subagent** |
| Multiple workers need to communicate | **Agent Team** |
| Workers need to challenge each other | **Agent Team** |
| Complex work requiring real-time coordination | **Agent Team** |
| Reusable prompt in main conversation | **Skill** |

---

## Implications for Saurun God-Agent

The god-agent currently uses subagents (Task tool) for Phase 3 execution, which is the biggest context bottleneck (~55% of controller growth). Agent teams could potentially:

1. **Replace subagent dispatch in Phase 3** — teammates work independently with own context, reducing lead's context growth
2. **Enable true parallel task execution** — multiple implementation tasks running simultaneously instead of sequentially
3. **Improve cross-layer work** — frontend and backend teammates working in parallel on different tasks

**Caveats**:
- Higher token cost (each teammate = full Claude instance)
- Experimental status — no session resumption, task status can lag
- Need to avoid file conflicts across teammates
- One team per session limit could conflict with god-agent's multi-phase structure

**Recommendation**: Wait for agent teams to stabilize before integrating into god-agent. The subagent approach is simpler and more predictable. Consider agent teams for specific use cases like parallel code review or competing-hypothesis debugging.

---

## Sources

- [Anthropic Official Announcement](https://www.anthropic.com/news/claude-opus-4-6)
- [Claude Code Agent Teams Docs](https://code.claude.com/docs/en/agent-teams)
- [TechCrunch Coverage](https://techcrunch.com/2026/02/05/anthropic-releases-opus-4-6-with-new-agent-teams/)
- [Addy Osmani's Guide](https://addyosmani.com/blog/claude-code-agent-teams/)
- [CNBC — Vibe Working Era](https://www.cnbc.com/2026/02/05/anthropic-claude-opus-4-6-vibe-working.html)
- [Swarm Orchestration Skill Gist](https://gist.github.com/kieranklaassen/4f2aba89594a4aea4ad64d753984b2ea)
