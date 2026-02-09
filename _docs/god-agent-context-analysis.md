# God-Agent Context Analysis

**Date:** 2026-02-05
**Status:** Preliminary analysis, awaiting usage logging data

## Controller Context Baseline

| Component | Est. Tokens |
|-----------|------------:|
| System prompt | 2,000 |
| god-agent SKILL.md | 4,900 |
| CLAUDE.md files | 800 |
| **Base total** | **7,700** |

## Phase-by-Phase Controller Growth

| Phase | Context Added | Running Total |
|-------|-------------:|---------------:|
| -1 Scaffold | 300 | 8,000 |
| 0 Intake | 3,050 | 11,050 |
| 1 Architecture | 2,900 | 13,950 |
| 2 Planning (×5) | 5,400 | 19,350 |
| 3 Execution (×20 tasks) | 33,000 | 52,350 |
| 4 Integration | 1,900 | 54,250 |
| 5 E2E Testing | 3,400 | 57,650 |
| 6 Design Polish | 800 | 58,450 |

**Estimated final controller context: 58,000-70,000 tokens**

## Key Unknowns (Need Logging)

1. **Skill loading location** — Do skills loaded via Skill tool go into controller context or only subagent context?
2. **Document re-reading** — How many times are spec/architecture docs actually read?
3. **STATE.md churn** — How much read/write overhead per update?
4. **Phase 3 task count** — Actual tasks per run (20? 40? 60?)

## Phase 3 is the Problem

Phase 3 accounts for ~55% of controller context growth:
- Per task: ~1,500 tokens (read task, dispatch, read results, update state)
- Linear growth: 20 tasks = 30,000 tokens, 40 tasks = 60,000 tokens

## Suspected Waste Points

1. **Full artifact reading for gate checks** — Reading entire files to check if sections exist
2. **Per-task STATE.md updates** — Full read/edit cycle each time
3. **Skill loading into controller** — If skills load into controller (not just subagent), adds 10,000+ tokens
4. **Subagent result verbosity** — Reading full reports when summary would suffice

## Next Steps

1. Add context usage logging to god-agent
2. Capture actual token counts per phase
3. Identify which reads are necessary vs redundant
4. Optimize based on real data

---

## Reference: Skill/Agent File Sizes

| File | Lines | Est. Tokens |
|------|------:|------------:|
| god-agent SKILL.md | 1,235 | 4,900 |
| dotnet-tactical-ddd/SKILL.md | 500 | 2,000 |
| dotnet-tactical-ddd/base-classes.cs | 134 | 500 |
| react-frontend-patterns/SKILL.md | 209 | 850 |
| react-frontend-patterns/references/* | ~500 | 2,000 |
| dotnet-writing-plans/SKILL.md | 185 | 750 |
| react-writing-plans/SKILL.md | 319 | 1,300 |
| e2e-test/SKILL.md + references | ~400 | 1,600 |
| backend-implementer.md | 15 | 60 |
| frontend-implementer.md | 52 | 200 |

## Reference: Example Artifact Sizes

From recipe-app `_docs/specs/`:
| File | Lines | Est. Tokens |
|------|------:|------------:|
| Product spec | 211 | 850 |
| Architecture doc | 483 | 1,900 |

---

## Logging Options (To Implement)

### Option A: STATE.md Section
```markdown
## Context Log
| Time | Action | Est. Tokens |
|------|--------|-------------|
| 10:30:01 | Read spec.md | 850 |
| 10:30:05 | Dispatch Phase 1 subagent | 100 |
```

### Option B: Append-only File
```bash
echo "[$(date +%H:%M:%S)] READ spec.md lines=211" >> .god-agent/context.log
```

### Option C: Structured JSON
```json
{"phase": 1, "action": "read", "file": "spec.md", "lines": 211}
```
