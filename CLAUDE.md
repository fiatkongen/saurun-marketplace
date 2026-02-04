# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

A Claude Code plugin marketplace repo (`fiatkongen/saurun-marketplace`). Contains the `saurun` plugin — a collection of skills, agents, and commands that extend Claude Code.

## Repository Structure

```
.claude-plugin/marketplace.json   # Marketplace manifest (version + plugin list)
plugins/saurun/
  .claude-plugin/plugin.json      # Plugin manifest (version must match marketplace.json)
  skills/                         # Skills (SKILL.md + optional references/, phases/, etc.)
  agents/                         # Agent definitions (single .md files with frontmatter)
  commands/                       # Slash commands (single .md files)
.claude/commands/                 # Repo-level commands (e.g., publish-plugin)
```

## Publishing

Use `/publish-plugin` or `/publish-plugin bump` to publish changes. The command:
- Optionally bumps patch version in both `plugin.json` and `marketplace.json`
- Stages `.claude-plugin/` and `plugins/`, commits, and pushes to `origin main`

Version must stay in sync between `plugins/saurun/.claude-plugin/plugin.json` and `.claude-plugin/marketplace.json`.

## Skill Anatomy

Each skill lives in `plugins/saurun/skills/<name>/` with at minimum a `SKILL.md` containing YAML frontmatter (`name`, `description`, optionally `allowed-tools`, `user-invocable`) followed by markdown instructions. Complex skills may include:
- `references/` — supporting docs, templates, workflow definitions
- `phases/` — ordered execution phases (e.g., `1-initialize.md`, `2-transform.md`)
- Additional prompt files (e.g., `concept-template.md`, `verification-prompt.md`)

## Agent Anatomy

Each agent is a single `.md` file in `plugins/saurun/agents/` with YAML frontmatter (`name`, `description`, `skills` to preload, `model`). The body is the agent's system prompt.

## Key Conventions

- Commit messages: `chore(plugin): publish plugin v{version}`
- Skills reference other skills by qualified name: `saurun:<skill-name>` or `<plugin>:<skill-name>`
- Agents can preload skills via the `skills` frontmatter field (comma-separated)
