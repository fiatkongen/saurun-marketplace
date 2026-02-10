> ⚠️ **Spelling:** It's `saurun:` (with U), not "sauron"

# Subagent Dispatch Prompt Template

Assembled by the harness for each A/B subagent. Both models receive the **identical** prompt — only the `model` parameter differs.

## Placeholder Reference

| Placeholder | Source | Required |
|-------------|--------|:--------:|
| `{{content_type}}` | `"skill"` or `"agent"` (from Phase 2) | Yes |
| `{{primary_content}}` | Full SKILL.md or agent .md content | Yes |
| `{{preloaded_skills_block}}` | XML block of preloaded skills (see below). Empty string if none. | No |
| `{{test_input}}` | `test_input` field from parsed test definition | Yes |

## Template

```
You are a Claude Code assistant. The following {{content_type}} instructions are loaded in your context. Follow them precisely.

<skill-context>
{{primary_content}}
</skill-context>

{{preloaded_skills_block}}

---

Handle this task:

{{test_input}}
```

## Preloaded Skills Block

When `preloaded_skills` is non-empty, build `{{preloaded_skills_block}}` as:

```xml
<preloaded-skills>
<skill name="skill-name-1">
...full SKILL.md content...
</skill>
<skill name="skill-name-2">
...full SKILL.md content...
</skill>
</preloaded-skills>
```

When empty, `{{preloaded_skills_block}}` = empty string (no tag at all).

## Cross-Plugin Skill Resolution

Skill names in agent frontmatter may be qualified: `plugin:skill-name`.
- `saurun:react-tdd` → `plugins/saurun/skills/react-tdd/SKILL.md`
- `frontend-design:frontend-design` → `plugins/frontend-design/skills/frontend-design/SKILL.md`
- Bare name `react-tdd` → `plugins/saurun/skills/react-tdd/SKILL.md` (default to saurun)
