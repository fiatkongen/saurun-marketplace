# Claude Code Skills & Sub-Agents Reference

Research compiled Feb 2026 from official Anthropic documentation.

## Skill Frontmatter Fields

Every skill has a `SKILL.md` with YAML frontmatter. Complete field reference:

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `name` | string | **required** | Skill identifier. Used in `Skill` tool invocations and qualified references (`plugin:skill-name`). |
| `description` | string | **required** | When/how to use the skill. Claude reads this to decide if a skill applies. Also drives `find-skills` search. |
| `context` | `"fork"` | inline | `fork`: runs in isolated subagent context (own conversation, no access to parent history). Omit or leave blank: skill content injected into current conversation. |
| `allowed-tools` | string | all | Comma-separated tool allowlist. Supports glob patterns: `Bash(npm:*)`, `Bash(node:*)`. When set, only these tools are available. |
| `user-invocable` | bool | `false` | If `true`, user can trigger via `/skill-name`. If `false`, only model-invocable (Claude decides when to use it). |
| `disable-model-invocation` | bool | `false` | If `true`, Claude never auto-invokes this skill. Must be triggered manually by user. |
| `model` | string | parent model | Override model for this skill (e.g., `"haiku"`, `"sonnet"`, `"opus"`). |
| `argument-hint` | string | none | Placeholder text shown to user when skill accepts arguments. Appears in autocomplete. |
| `hooks` | object | none | Lifecycle hooks (e.g., `PreToolCall`, `PostToolCall`). Run shell commands at specific points. |
| `agent` | string | none | Name of a sub-agent to run the skill in. The agent must be defined in `.claude/agents/`. |

## Context Modes

### Inline (default)

Skill content is injected directly into the current conversation. The skill has full access to conversation history, user messages, and all context. Good for skills that need to reference ongoing work.

### Fork (`context: fork`)

Skill runs in a completely isolated subagent. It gets its own conversation context with only the skill's SKILL.md content as its prompt. No access to parent conversation history. Good for self-contained workflows that shouldn't be influenced by conversation state.

**Known bug (#17283):** When a skill with `context: fork` is invoked via the `Skill` tool (model-invoked), the `fork` setting is currently ignored and the skill runs inline. Only user-invoked skills (`/skill-name`) properly fork. Track: https://github.com/anthropics/claude-code/issues/17283

## Sub-Agent Definitions

Sub-agents are defined as `.md` files in `.claude/agents/` (or `plugins/<name>/agents/`). They are purpose-built execution contexts.

### Sub-Agent Frontmatter

| Field | Type | Description |
|-------|------|-------------|
| `name` | string | Agent identifier |
| `description` | string | When to use this agent |
| `tools` | string[] | Allowed tools |
| `disallowedTools` | string[] | Explicitly blocked tools |
| `model` | string | Model override |
| `permissionMode` | string | Permission handling (e.g., `"plan"`, `"auto"`) |
| `skills` | string | Comma-separated skills to preload into agent context |
| `hooks` | object | Agent-specific lifecycle hooks |

### Skills vs Sub-Agents

| Aspect | Skills | Sub-Agents |
|--------|--------|------------|
| **Purpose** | Portable, reusable expertise | Purpose-built execution context |
| **Context** | Inline or forked | Always own context |
| **Tools** | Inherits or restricts via `allowed-tools` | Defines own tool set |
| **Permissions** | Inherits parent | Can have own permission mode |
| **Preloading** | N/A | Can preload skills via `skills` field |
| **Invocation** | `Skill` tool or `/name` | `Task(subagent_type="name")` |
| **Reuse** | Across conversations, plugins | Scoped to defining project/plugin |

## String Substitutions in Skills

Skills support these template variables in their content:

| Variable | Expands To |
|----------|------------|
| `$ARGUMENTS` | Full argument string passed to the skill |
| `$ARGUMENTS[0]`, `$ARGUMENTS[1]`, etc. | Individual space-separated arguments |
| `$1`, `$2`, etc. | Alias for `$ARGUMENTS[N-1]` |
| `${CLAUDE_SESSION_ID}` | Current session identifier |

## Dynamic Context Injection

Skills can include shell command output using the `` !`command` `` syntax:

```markdown
Current branch: !`git branch --show-current`
Recent changes: !`git log --oneline -5`
```

Shell commands are executed during skill loading, and their stdout replaces the `` !`...` `` block. Useful for injecting project state into skill prompts.

## Spawning Sub-Agents from Skills

Skills commonly spawn sub-agents for decomposed work:

```
Task(subagent_type="general-purpose", prompt="<agent prompt + input>")
```

For custom agents defined in the plugin:
```
Task(subagent_type="plugin-name:agent-name", prompt="<task description>")
```

## Tool Restriction Patterns

The `allowed-tools` field supports:

- Exact names: `Read, Write, Edit`
- Glob patterns for Bash: `Bash(npm:*)` allows any npm command, `Bash(node:*)` allows any node command
- Combination: `Task, Read, Write, Edit, Bash(node:*), AskUserQuestion, Glob`

When `allowed-tools` is set, only listed tools are available. Unlisted tools are blocked entirely.

## Practical Patterns

### Self-Contained Workflow Skill

```yaml
---
name: my-workflow
description: Use when user wants to run the full workflow
context: fork
allowed-tools: Task, Read, Write, Edit, AskUserQuestion
user-invocable: true
---
```

Fork + restricted tools = isolated, predictable execution.

### Agent-Delegated Skill

```yaml
---
name: my-delegated-skill
description: Delegates to a specialized agent
agent: my-special-agent
user-invocable: true
---
```

The skill's content becomes input to the named agent, which has its own tools/permissions/model.

### Model-Only Skill (No User Trigger)

```yaml
---
name: internal-helper
description: Use when encountering X pattern in code
user-invocable: false
---
```

Claude auto-invokes when its description matches the situation. User cannot trigger directly.

### User-Only Skill (No Auto-Invoke)

```yaml
---
name: dangerous-operation
description: Performs destructive cleanup
user-invocable: true
disable-model-invocation: true
---
```

Only runs when user explicitly types `/dangerous-operation`. Claude never triggers it autonomously.
