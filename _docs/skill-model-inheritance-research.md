# Skill `model` Frontmatter and Subagent Model Inheritance

Researched: 2026-02-07

## The Core Question

If a skill with `model: haiku` dispatches a subagent via the Task tool (without specifying a model), does the subagent inherit `haiku` or the original session model (e.g., `opus`)?

## What IS Documented

### Skill `model` field — minimal docs

Official docs at `code.claude.com/docs/en/skills`:

| Field | Description |
|-------|-------------|
| `model` | Model to use when this skill is active. |

No elaboration on what "when this skill is active" means for downstream subagent dispatch. No mention of inheritance or propagation.

### Subagent `model` field — well documented

Official subagent docs at `code.claude.com/docs/en/sub-agents`:
- `model` options: `sonnet`, `opus`, `haiku`, or `inherit`
- Omitted = defaults to `inherit` (uses same model as main conversation)

### Task tool `model` parameter

The Task tool in Claude Code accepts an optional `model` parameter: `"sonnet"`, `"opus"`, `"haiku"`. If not specified, inherits from parent.

### Built-in subagent defaults

- `Explore` agent: hardcoded to Haiku
- `Plan` agent: inherits from main conversation
- `general-purpose` agent: inherits from main conversation
- `Bash` agent: inherits from main conversation

### `context: fork` behavior

`context: fork` creates a subagent. Sub-sub-agents are NOT supported (GitHub issue #19077) — forked skills can't dispatch Task. Bug #17283 (fork ignored when model-invoked via Skill tool) is reported as fixed (exact date unverified from primary source).

## What is NOT Documented (The Gap)

**The critical undocumented question: What is "parent" / "main conversation" when running inside a skill with a model override?**

Two interpretations:

**A — "parent" = the skill's model override:**
skill(haiku) → Task(inherit) → runs on haiku

**B — "parent" = the original session model:**
skill(haiku) → Task(inherit) → runs on opus (skill model is scoped)

No official documentation addresses this distinction.

## Evidence From GitHub Issues

| Issue | Status | Finding |
|-------|--------|---------|
| #5456 | Closed (dup) | Sub-agents defaulting to Sonnet instead of inheriting. Session model not propagating. |
| #8932 | Closed (not planned) | general-purpose agent hardcoded to Sonnet rather than inherit. Not fixed — closed without resolution. |
| #10993 | Closed (inactivity) | `CLAUDE_CODE_SUBAGENT_MODEL` env var overrides ALL agent model settings. Note: only accepts full model names (e.g., `claude-sonnet-4-5-20250929`), not aliases like `sonnet`. |
| #19077 | Open | Sub-sub-agents not supported. Subagents can't spawn other subagents. Confirmed as intentional design. |
| #19174 | Open | Contradiction between docs ("defaults to sonnet") vs changelog v2.1.3 ("Fixed subagents not inheriting parent model"). Note: subagent docs page has since been updated to say "inherit", but CLI Reference still has ambiguity. |

**Changelog v2.1.3**: "Fixed sub-agents using the wrong model during conversation compaction" and "Fixed web search in sub-agents using incorrect model."

None of these issues address the skill-model → subagent-model propagation chain specifically.

## Inference

**Interpretation A is most likely correct** for inline skills:

1. **Inline skills** (no `context: fork`) inject content into the current conversation. `model: haiku` switches which model processes the conversation. Since there's no subagent boundary, the "parent model" for any Task dispatched IS haiku.

2. **Forked skills** (`context: fork`) create a subagent. Sub-sub-agents aren't supported, so Task is unavailable — the propagation question is moot.

3. **Practical answer**: An inline skill with `model: haiku` causes Haiku to process the conversation and call the Task tool. A subagent with `model: inherit` would inherit haiku.

## Summary Table

| What | Documented? | Source |
|------|:-----------:|--------|
| Skill `model` field exists | YES | `code.claude.com/docs/en/skills` |
| Skill `model` changes which model runs the skill | YES (briefly) | "Model to use when this skill is active" |
| Subagent `model: inherit` = parent model | YES | `code.claude.com/docs/en/sub-agents` |
| Subagent `model` omitted = defaults to inherit | YES | Same |
| Task tool accepts `model` parameter | YES | Claude Code tool schema |
| What "parent model" means inside a model-overridden skill | **NO** | Gap in docs |
| Whether skill model propagates to Task-dispatched subagents | **NO** | Gap in docs |
| Whether inline vs forked changes propagation | **NO** | Gap in docs |
| Sub-sub-agents not supported | YES | Issue #19077 |
| `CLAUDE_CODE_SUBAGENT_MODEL` overrides everything | Partially | Issue #10993 |

## Practical Recommendations

1. **Always specify model explicitly** on Task calls when the parent context may have a model override (as `model-ab-test` skill does)
2. **Pin model on agent definitions** instead of using `model: inherit` if you need certainty (e.g., `model: opus` on `backend-implementer.md`)
3. **Don't set `model` on skills that dispatch subagents** unless you want the override to propagate
4. **Safe to set `model` on**: routing skills, template-filling skills that return content without dispatching, no-op skills
