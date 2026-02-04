# Generalize Refine Skill — Design

**Date:** 2026-02-04
**Status:** Draft
**Goal:** Extract the iterative review loop from `refine-document` into a content-agnostic `refine` skill with pluggable content-type criteria.

---

## Motivation

The 3-phase review loop (self-review -> Codex cross-validation -> user resolution) is valuable beyond documents. The pattern should work for any text artifact: code, configs, plans, specs, changelogs, skill definitions.

## Approach

**Hybrid: Content-Type Registry with Base File**

- Single `refine` skill replaces `refine-document`
- Universal issues defined once in `_base.md`
- Per-type issues in separate content-type files
- Engine loads base + type criteria and passes to agents
- Type files can override base severity and auto-fixable rules

## Skill Structure

```
skills/refine/
├── SKILL.md                    # Engine — 3-phase loop, content-agnostic
├── agents/
│   ├── issue-finder.md         # Generic: receives criteria as input
│   ├── issue-fixer.md          # Unchanged from refine-document
│   └── codex-validator.md      # Unchanged from refine-document
├── content-types/
│   ├── _base.md                # Universal issues
│   ├── markdown.md             # Markdown-specific issues
│   ├── plan.md                 # Implementation plan issues
│   ├── code.md                 # Source code issues
│   └── config.md               # YAML/JSON/TOML issues
├── references/
│   └── change-log-format.md    # Unchanged — universal
└── test-fixtures/
    ├── markdown-fixture.md     # Current test-fixture.md
    ├── code-fixture.ts         # (future)
    └── plan-fixture.md         # (future)
```

## Content-Type Detection

Resolution order:

1. **User explicit** — `/refine plan` or `/refine code` overrides detection
2. **File extension** — `.md` -> markdown, `.ts/.js/.cs/.py` -> code, `.yaml/.json/.toml` -> config
3. **Content heuristics** — `.md` file containing `<task>`, `<verify>`, `<done>` tags -> plan
4. **Fallback** — no match -> `_base.md` only (universal issues, no type-specific)

## Base + Type Inheritance

No runtime merge logic. The engine passes both files as context to the issue-finder agent:

```
issue-finder receives:
  1. _base.md content (always)
  2. {detected-type}.md content (if matched)
  3. Instructions: "Apply base criteria first, then type-specific. Type overrides base on conflicts."
```

Type files declare overrides explicitly. Anything not mentioned inherits from base.

## Engine Changes (SKILL.md)

### What stays identical

- Phase 1: Self-Review Loop (iterate, spawn issue-finder, fix, re-scan)
- Phase 2: Codex Cross-Validation (codex-bridge, codex-validator filtering)
- Phase 3: User Resolution (AskUserQuestion per ambiguous issue)
- Final Validation + Report
- Change log tracking
- Parameter detection (codex-fix, aggressive, conservative)
- Max iterations (5)
- Red flags & common rationalizations
- "Never fabricate" rules

### What changes

1. **Content-type resolution step** — new first step before Phase 1:
   - Detect or accept content type
   - Load `_base.md` + `{type}.md` via Read tool
   - Pass both to all subsequent agent spawns

2. **Agent spawn calls gain criteria parameters:**
   ```
   Task(subagent_type="general-purpose", prompt=<issue-finder prompt
     + file_path + file_type + mode
     + base_criteria_content
     + type_criteria_content>)
   ```

3. **Codex prompt becomes templated** — base Codex "LOOK FOR" list comes from `_base.md`. Type files extend it with type-specific items. The engine assembles the full prompt.

4. **Report gains content-type line:**
   ```
   File: [name]  Type: [detected type]  Content-Type: [markdown]  Mode: [default]
   ```

## Agent Changes

| Agent | Change |
|-------|--------|
| issue-finder | Receives base + type criteria as input. Applies them instead of hardcoded rules. |
| issue-fixer | Unchanged. Already generic (receives one issue, fixes it). |
| codex-validator | Unchanged. Already content-agnostic. |

### issue-finder input (expanded)

```
- file_path: Path to the file
- file_type: Detected content type (markdown, code, plan, config, base-only)
- mode: aggressive, conservative, default
- base_criteria: Content of _base.md
- type_criteria: Content of {type}.md (may be empty)
```

### issue-finder analysis protocol

1. Read the entire file
2. Read base_criteria — understand universal issue types
3. Read type_criteria — understand type-specific additions/overrides
4. For each issue type in base + type criteria, apply detection rules
5. Respect severity and auto_fixable as defined (type overrides base)
6. Return structured JSON (same format as current)

## Content-Type File Format

Each file follows this standard structure:

```markdown
# {Type Name} Content Type

## Detection
- Extensions: [list]
- Heuristics: [description or "None"]
- Override keyword: [user-facing name]

## Issue Types

### Type-Specific Issues
| Type | Description | Severity | Auto-fixable |
|------|-------------|----------|--------------|
| ... | ... | ... | ... |

### Severity Overrides (from base)
- [issue_type]: [new_severity] (reason)

### Auto-fixable Overrides (from base)
- [issue_type]: [true/false] (reason)

## Detection Patterns
[Per issue type: how to detect it. This is the "expertise" the issue-finder reads.]

## Mode Adjustments

### Aggressive
[What changes in aggressive mode for this type]

### Conservative
[What changes in conservative mode for this type]

## Codex Prompt Extension
[Additional items for Codex to look for, beyond the base prompt]
```

### `_base.md` contents

Universal issues extracted from current `references/issue-criteria.md`:

- contradiction (Critical)
- broken_reference (Critical)
- missing_definition (Critical)
- vague_instruction (Major)
- inconsistent_terminology (Major, auto-fixable if >70% dominant)
- unresolved_todo (Major)
- redundant_content (Major, auto-fixable)
- ambiguous_scope (Major)
- missing_context (Major)
- formatting_inconsistency (Minor, auto-fixable)
- trailing_whitespace (Minor, auto-fixable)
- incomplete_list (Minor, auto-fixable)

Plus: issue type enum, universal detection patterns, base mode adjustments, base Codex prompt.

## Migration Plan

1. Extract universal issues from `references/issue-criteria.md` -> `content-types/_base.md`
2. Extract markdown-specific criteria -> `content-types/markdown.md`
3. Extract plan-specific criteria -> `content-types/plan.md`
4. Extract code/config criteria -> `content-types/code.md` and `content-types/config.md`
5. Refactor `SKILL.md` — add content-type resolution, parameterize agent spawns, template Codex prompt
6. Refactor `agents/issue-finder.md` — accept criteria as input
7. Move `test-fixture.md` -> `test-fixtures/markdown-fixture.md`
8. Delete `refine-document/` directory, create `refine/`
9. Update plugin manifest references

## Extensibility

Adding a new content type requires one file:

1. Create `content-types/{type}.md`
2. Define type-specific issue types, severity, auto-fixable rules
3. Add detection patterns
4. Add Codex prompt extension
5. Done. No engine or agent changes.

## Future Consideration: plan-fixer Integration

`plan-fixer` has its own multi-phase review loop. After this generalization, plan-fixer could delegate its review phases to `refine` with `content-types/plan.md`. However, plan-fixer also does things refine doesn't (splitting plans into task files, structure optimization). The relationship would be: plan-fixer uses refine as a sub-tool for review, keeping its own orchestration for plan-specific operations.

This is not part of the initial generalization.
