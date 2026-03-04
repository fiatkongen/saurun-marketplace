# Plan: Neo Scaffold Skill

## Context

The existing `scaffold` skill creates a basic local project (.NET 9, no frontend build, no Docker, no deploy). The design at `_docs/neo-scaffold-skill-design.md` specifies a full pipeline: empty dir → live deployed URL in one call. The SKILL.md draft (491 lines) has already been written to `plugins/saurun/skills/scaffold/SKILL.md` but needs review/fixes, and the `references/` dir is empty.

## Tasks

### T1: Copy default design template to references/
- Copy `_docs/default-design-template.md` → `plugins/saurun/skills/scaffold/references/default-design-template.md`
- Verbatim copy — SKILL.md step 3 reads from this bundled reference
- **Verify:** `diff` shows no difference

### T2: Fix SKILL.md issues
Minor fixes to the already-written draft:

1. **Step 8c (tsconfig merge)** — Clarify that `baseUrl` and `paths` must be MERGED into the existing `compilerOptions`, not replace the whole file. Vite's template already has `compilerOptions` with other settings.

2. **Step 8g cleanup** — Also remove `src/assets/` dir if empty after deleting `react.svg`.

3. **Verify `Skill(use-railway)` stays in allowed-tools** — Confirmed by user.

4. **Verify gate count** — 12 local + 2 build + 4 remote = 18. Matches the design doc's decision table. The summary in the design doc that says "20" is wrong, but we use 18. Confirm SKILL.md report says "18/18".

### T3: Final review pass
- Confirm all 14 steps (0-13) present and numbered correctly
- Confirm file paths consistent throughout
- Confirm error handling table covers all failure modes
- Read through once for clarity

## Files to Create/Modify

| File | Action |
|------|--------|
| `plugins/saurun/skills/scaffold/references/default-design-template.md` | **Create** — copy from `_docs/default-design-template.md` |
| `plugins/saurun/skills/scaffold/SKILL.md` | **Edit** — minor fixes from T2 |

## Verification

1. `diff _docs/default-design-template.md plugins/saurun/skills/scaffold/references/default-design-template.md` → no diff
2. `grep -c "Step" plugins/saurun/skills/scaffold/SKILL.md` → 14 steps (0-13)
3. `grep "18/18" plugins/saurun/skills/scaffold/SKILL.md` → present in report template
4. `grep "use-railway" plugins/saurun/skills/scaffold/SKILL.md` → referenced in step 12 + allowed-tools
5. `wc -l plugins/saurun/skills/scaffold/SKILL.md` → ~490 lines
