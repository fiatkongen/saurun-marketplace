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

## Skill Frontmatter

The new `refine/SKILL.md` YAML frontmatter:

```yaml
---
name: refine
description: >
  Use when user wants to improve, polish, clean up, or fix inconsistencies in any text artifact.
  Use when user says "polish", "refine", "clean up", "make more concise", "fix inconsistencies",
  "improve this doc", "review this code", "refine config", "refine plan".
  Supports: markdown, code, plans, configs, and any text file.
context: fork
allowed-tools: Task, Read, Write, Edit, Bash(node:*), AskUserQuestion, Glob
user-invocable: true
---
```

**Changes from `refine-document`:**
- `name`: `refine-document` -> `refine`
- `description`: expanded trigger words to cover code/config/plan use cases
- `allowed-tools`: added `Glob` (needed for content-type file resolution and codex-bridge path lookup)

## Skill Structure

```text
skills/refine/
├── SKILL.md                    # Engine — 3-phase loop, content-agnostic
├── agents/
│   ├── issue-finder.md         # Criteria-driven: receives base + type as input
│   ├── issue-fixer.md          # Extended with strategies for all content types
│   └── codex-validator.md      # Minor update to ACCEPT/REJECT criteria
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
    ├── plan-fixture.md         # (future)
    └── config-fixture.yaml     # (future)
```

## Content-Type Detection

Resolution order:

1. **User explicit** — `/refine plan` or `/refine code` overrides all detection
2. **File extension + heuristics** — map extension first, then apply heuristics that can override:
   - `.md` -> markdown (default), BUT if file contains 3+ `<task>` tags at line start outside code blocks -> plan (overrides markdown)
   - `.ts/.js/.cs/.py/.go/.rs/.java` -> code
   - `.yaml/.yml/.json/.toml` -> config
3. **Fallback** — no match -> `_base.md` only (universal issues, no type-specific)

**Deprecated:** The current `prose` content type is merged into the fallback path (base-only). Plain prose files get universal issue detection without type-specific rules. If prose-specific rules are needed later, add `content-types/prose.md`.

## Base + Type Inheritance

No runtime merge logic. The engine passes both files as context to the issue-finder agent:

```text
issue-finder receives:
  1. _base.md content (always)
  2. {detected-type}.md content (if matched)
  3. Instructions: "Apply base criteria first, then type-specific. Type overrides base on conflicts."
```

### Override Rules

Type files follow these explicit rules:

- **CAN** add new issue types not in base (e.g., `heading_skip` for markdown)
- **CAN** override severity of base issues (e.g., `trailing_whitespace` -> Critical for config files)
- **CAN** override auto-fixable status of base issues (both true->false and false->true, with documented reason)
- **CANNOT** suppress or remove base issue types entirely — every base issue is always checked
- Severity escalation (Minor -> Critical) is allowed. Severity reduction (Critical -> Minor) requires a documented reason in the override section.
- Auto-fixable changes in either direction require a documented reason in the override section.

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
   - Load `_base.md` + `{type}.md` via Read tool (resolve paths via Glob)
   - Pass both to all subsequent agent spawns

2. **Agent spawn calls gain criteria parameters:**
   ```text
   Task(subagent_type="general-purpose", prompt=<issue-finder prompt
     + file_path + content_type + mode
     + base_criteria
     + type_criteria>)
   ```

3. **Codex prompt becomes templated** — see [Codex Prompt Assembly](#codex-prompt-assembly) below.

4. **Report format:**
   ```text
   File: [name]  Content-Type: [markdown]  Mode: [default]
   ```
   The `Type` field is renamed to `Content-Type` for consistency with the new terminology.

## Codex Prompt Assembly

The engine assembles the Codex prompt from two sources:

**`_base.md` contains a `## Codex Prompt` section** with the universal "LOOK FOR" list:

```text
LOOK FOR:
1. Vague instructions (reader must guess intent)
2. Inconsistent terminology (same concept, different names)
3. Missing information (referenced but not defined)
4. Redundant content (same thing said multiple ways)
5. Unclear scope (ambiguous boundaries)
6. Broken references (links to non-existent things)
7. Unresolved placeholders (TODOs, TBDs)
8. Inconsistent casing (same term in different cases: API vs Api vs api)
```

**Each content-type file has a `## Codex Prompt Extension` section** with type-specific items.

**Assembly rule: concatenation.** The engine appends the type extension after the base list:

```text
LOOK FOR:
{items from _base.md Codex Prompt section}
{items from {type}.md Codex Prompt Extension section, numbered continuing from base}

DO NOT FLAG:
{standard exclusions from _base.md}
```

Type files **extend** the base prompt. They cannot remove base LOOK FOR items from the prompt text. If a type needs Codex to skip a base check, it adds a "DO NOT FLAG" item in its extension — this suppresses the check in the Codex prompt only. The issue-finder agent still applies all base criteria regardless of Codex prompt suppression.

The JSON return format schema (type enum, field definitions, severity values) is appended after DO NOT FLAG. This schema is hardcoded in the engine, not part of the content-type files — it applies universally.

**Assembled example for markdown:**

```text
LOOK FOR:
1. Vague instructions (reader must guess intent)
2. Inconsistent terminology (same concept, different names)
3. Missing information (referenced but not defined)
4. Redundant content (same thing said multiple ways)
5. Unclear scope (ambiguous boundaries)
6. Broken references (links to non-existent things)
7. Unresolved placeholders (TODOs, TBDs)
8. Heading hierarchy violations (h1 -> h3 skipping h2)
9. Internal links pointing to non-existent anchors
10. Unclosed markdown formatting
11. Empty table cells or malformed tables

DO NOT FLAG:
- Stylistic preferences (unless inconsistent)
- Subjective 'could be better' suggestions
- Things already fixed
```

## Agent Changes

| Agent | Change |
|-------|--------|
| issue-finder | Rewritten: receives base + type criteria as input, applies them instead of hardcoded rules |
| issue-fixer | Extended: add fix strategies for code/config/plan issue types |
| codex-validator | Minor: broaden ACCEPT/REJECT criteria for non-document content |

### issue-finder — Prompt Rewrite

The current issue-finder has hardcoded sections: "Issue Types and Severity", "Mode Adjustments", "auto_fixable Rules", "Detection Patterns". All of these are **removed** and replaced with criteria-driven instructions.

**Current prompt structure (removed sections marked with ~~):**

```text
## Input
- file_path, file_type, mode

~~## Issue Types and Severity~~        <- REMOVED (comes from criteria)
~~### Critical / Major / Minor~~       <- REMOVED
~~## Mode Adjustments~~                <- REMOVED (comes from criteria)
~~## auto_fixable Rules~~              <- REMOVED (comes from criteria)
~~## Detection Patterns~~              <- REMOVED (comes from criteria)

## Output Format                       <- KEPT
## Analysis Protocol                   <- REWRITTEN
## What NOT to Flag                    <- KEPT
```

**New prompt structure:**

```text
## Input (expanded)
- file_path: Path to the file
- content_type: Detected content type (markdown, code, plan, config, base-only)
- mode: aggressive, conservative, default
- base_criteria: Content of _base.md
- type_criteria: Content of {type}.md (may be empty for base-only)

## Instructions
You are given two criteria documents that define what to look for.
- base_criteria defines universal issue types, severities, auto-fixable rules, and detection patterns
- type_criteria (if present) adds type-specific issues and may override base severities/auto-fixable rules
- When type_criteria overrides a base rule, the type-specific version wins
- Apply mode adjustments from both criteria documents. If base and type specify conflicting adjustments for the same issue type, type wins.

## Analysis Protocol
1. Read the entire file
2. Read base_criteria — understand universal issue types and detection patterns
3. Read type_criteria — understand type-specific additions and any overrides
4. For each issue type in base criteria, apply its detection patterns against the file
5. For each issue type in type criteria, apply its detection patterns against the file
6. Respect severity and auto_fixable as defined (type overrides base on conflicts)
7. Apply mode adjustments (aggressive/conservative/default) from both criteria
8. Return structured JSON (same format as before)

## Output Format                       <- unchanged from current
## What NOT to Flag                    <- unchanged from current
```

### issue-fixer — Strategy Extensions

The current fix strategies table covers document issues. New strategies for code/config/plan/markdown types:

| Type | Strategy |
|------|----------|
| naming_inconsistency | Identify dominant convention (camelCase vs snake_case). Rename all occurrences of the minority style. Preserve imports/exports. |
| stale_comment | Remove the comment or update to match current code. If unclear, set auto_fixable=false. |
| dead_code | Remove unreachable/unused code block. Verify no side effects. |
| magic_number | Extract to named constant. Name based on usage context. Auto-fixable only when usage is unambiguous. |
| heading_skip | Insert missing heading level or adjust to sequential. |
| orphan_heading | Set auto_fixable=false — user must decide: add content or remove heading. |
| duplicate_key | Keep the last occurrence (standard parser behavior). Warn in change log. |
| type_mismatch | Set auto_fixable=false — user must confirm intended type. |
| missing_verification | Set auto_fixable=false — user must provide verification steps. |
| vague_action | Set auto_fixable=false — user must specify concrete action. |
| inconsistent_casing | Identify correct casing from authoritative source (official name, first usage). Normalize all occurrences. |

Existing document strategies (inconsistent_terminology, redundant_content, etc.) remain unchanged.

### codex-validator — Criteria Broadening

Current ACCEPT criteria are document-oriented. Add these to cover non-document content:

**Additional ACCEPT conditions:**
- Code: naming convention violation is objectively verifiable (not stylistic preference)
- Code: comment contradicts the code it describes
- Config: key is duplicated (parser will silently drop one)
- Plan: task has no way to verify completion

**Additional REJECT conditions:**
- Code: "could use a better name" without concrete inconsistency
- Code: stylistic preference for one pattern over another equally valid one
- Config: "could be organized better" without concrete issue

## Content-Type File Format

Each file follows this standard structure:

```markdown
# {Type Name} Content Type

## Detection
- Extensions: [list]
- Heuristics: [description or "None"]
- Override keyword: [name the user types after `/refine` to force this content type, e.g., "plan" for `/refine plan`]

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
[Additional items for Codex to look for, appended to the base LOOK FOR list]
```

### `_base.md` contents

Universal issues extracted from current `references/issue-criteria.md`:

- contradiction (Critical)
- broken_reference (Critical)
- missing_definition (Critical)
- vague_instruction (Major)
- inconsistent_terminology (Major, auto-fixable if >70% dominant)
- inconsistent_casing (Minor, auto-fixable — e.g., "API" vs "Api" vs "api")
- unresolved_todo (Major)
- redundant_content (Major, auto-fixable)
- ambiguous_scope (Major)
- missing_context (Major)
- formatting_inconsistency (Minor, auto-fixable)
- trailing_whitespace (Minor, auto-fixable)
- incomplete_list (Minor, auto-fixable)

Plus: issue type enum (including `inconsistent_casing`, which must be added to the formal enum during migration), universal detection patterns, base mode adjustments, base Codex prompt section.

**Note:** `missing_context` appears in both `issue-criteria.md` and `issue-finder.md`. During migration, `_base.md` becomes the single source of truth for all issue type definitions.

## Terminology

This design uses **`content_type`** consistently to refer to the classification of the file being refined. The terms "file type" and "type" are avoided in isolation to prevent ambiguity.

Where the term appears:
- SKILL.md parameter: `content_type`
- Agent input field: `content_type`
- Report field: `Content-Type: [value]`
- Directory name: `content-types/`
- User-facing: "content type" (lowercase, two words)

## Migration Plan

Migration happens in a single atomic commit. If issues are found post-migration, revert the commit.

1. Create `skills/refine/` directory structure
2. Extract universal issues from `references/issue-criteria.md` -> `content-types/_base.md`
3. Extract markdown-specific criteria -> `content-types/markdown.md`
4. Extract plan-specific criteria -> `content-types/plan.md`
5. Extract code/config criteria -> `content-types/code.md` and `content-types/config.md`
6. Write new `SKILL.md` — content-type resolution, parameterized agent spawns, templated Codex prompt
7. Rewrite `agents/issue-finder.md` — remove hardcoded criteria, accept criteria as input (see prompt structure above)
8. Extend `agents/issue-fixer.md` — add fix strategies for code/config/plan issue types
9. Update `agents/codex-validator.md` — broaden ACCEPT/REJECT criteria
10. Move `references/change-log-format.md` to new location, update any internal references
11. Move `test-fixture.md` -> `test-fixtures/markdown-fixture.md`
12. Delete `refine-document/` directory
13. Delete `references/issue-criteria.md` (superseded by `_base.md` + content-type files)
14. Update plugin manifest references (plugin.json skill paths)

**Rollback:** Steps 1-14 happen in a single commit. If `refine` has issues, `git revert <commit>` restores `refine-document` exactly.

## Testing

Each content type should be validated before the migration is considered complete:

1. **markdown**: Run `refine` against `test-fixtures/markdown-fixture.md` (existing fixture with known issues). Verify all expected issues are found.
2. **code**: Create `test-fixtures/code-fixture.ts` with intentional naming inconsistencies, stale comments, and TODOs. Verify detection.
3. **plan**: Create `test-fixtures/plan-fixture.md` with missing verification steps and vague actions. Verify detection.
4. **config**: Create `test-fixtures/config-fixture.yaml` with duplicate keys and type mismatches. Verify detection.
5. **base-only fallback**: Run `refine` against a `.txt` file. Verify only universal issues are checked.
6. **content-type override**: Run `/refine plan` against a `.md` file. Verify plan criteria are used instead of markdown.
7. **Codex integration**: Verify Phase 2 Codex prompt includes both base and type-specific items.

## Extensibility

Adding a new content type requires creating one file: `content-types/{type}.md` following the standard format (which includes issue types, detection patterns, and Codex prompt extension). No engine or agent changes needed.

## Future Consideration: plan-fixer Integration

`plan-fixer` has its own multi-phase review loop. After this generalization, plan-fixer could delegate its review phases to `refine` with `content-types/plan.md`. However, plan-fixer also does things refine doesn't (splitting plans into task files, structure optimization). The relationship would be: plan-fixer uses refine as a sub-tool for review, keeping its own orchestration for plan-specific operations.

This is not part of the initial generalization.
