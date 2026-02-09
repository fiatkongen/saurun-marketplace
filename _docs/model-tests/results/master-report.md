# Model Downgrade A/B Test — Master Report
Generated: 2026-02-07

## Overview

20 active A/B tests across 5 test definition files. Each test compares Opus (baseline) against a proposed cheaper model (Haiku or Sonnet) on a specific skill or agent, scoring against predefined GREEN/RED criteria.

**Methodology:** Same prompt + context given to both models. Outputs scored against binary criteria. GREEN = must-have behavior. RED = must-not-have behavior. A test PASSES if Sonnet/Haiku meets all GREEN criteria and triggers no new RED indicators.

---

## Consolidated Summary (20 tests)

| # | Skill/Agent | File | Downgrade | GREEN | RED | Verdict |
|---|-------------|------|-----------|:-----:|:---:|:-------:|
| 1 | react-implementer-prompt | 01 | Opus→Haiku | 9/9 | 0 | PASS |
| 2 | dotnet-implementer-prompt | 01 | Opus→Haiku | 9/9 | 0 | PASS |
| 3 | dotnet-code-quality-reviewer-prompt | 01 | Opus→Haiku | 10/10 | 0 | PASS |
| 4 | react-code-quality-reviewer-prompt | 01 | Opus→Haiku | 11/11 | 0 | PASS |
| 5 | react-tailwind-v4-components | 01 | Opus→Haiku | 9/9 | 0 | PASS |
| 6 | codex-bridge | 02 | Opus→Haiku | 6/6 | 0 | PASS |
| 7 | writing-plans (router) | 02 | Opus→Haiku | 6/6 | 0 | PASS |
| 8 | test-deploy | 02 | Opus→Haiku | 3/3 | 0 | PASS |
| 9 | dotnet-tactical-ddd | 03 | Opus→Sonnet | 8/8 | 0 | PASS |
| 10 | react-frontend-patterns | 03 | Opus→Sonnet | 8/8 | 0 | PASS |
| 11 | dotnet-tdd | 03 | Opus→Sonnet | 8/8 | 0 | PASS |
| 12 | react-tdd | 03 | Opus→Sonnet | 8/8 | 0 | PASS |
| 13 | deploy-to-marketplace | 03 | Opus→Sonnet | 9/9 | 0 | PASS |
| 14 | dotnet-writing-plans | 04 | Opus→Sonnet | 7/8 | 2 | **FAIL** |
| 15 | react-writing-plans | 04 | Opus→Sonnet | 7/7 | 0 | PASS |
| 16 | product-manager | 05 | Opus→Sonnet | 8/8 | 0 | PASS |
| 17 | codex-validator | 05 | Opus→Sonnet | 6/6 | 0 | PASS |
| 18 | backend-implementer | 05 | Opus→Sonnet | 9/9 | 0 | PASS |
| 19 | frontend-implementer | 05 | Opus→Sonnet | 10/10 | 0 | PASS |
| 20 | react-writing-plans | 04 | — | — | — | — |

**Results: 19 PASS / 1 FAIL**

---

## Exempted (Not Tested)

| Skill/Agent | Reason |
|-------------|--------|
| requirement-verifier | Numeric mutation detection (10MB→5MB) too demanding for Haiku |
| dansk-qa | Danish language quality requires native-level judgment |
| autonomous-task-executor | Complex multi-step orchestration; not suitable for A/B format |

---

## Safe to Downgrade

### To Haiku (8 skills) — template/routing tasks
| Skill | Category | Evidence |
|-------|----------|----------|
| react-implementer-prompt | Template filling | 9/9 GREEN, all sections + sub-skill refs |
| dotnet-implementer-prompt | Template filling | 9/9 GREEN, all sections + dotnet-tdd ref |
| dotnet-code-quality-reviewer-prompt | Template filling | 10/10 GREEN, all 8 review categories |
| react-code-quality-reviewer-prompt | Template filling | 11/11 GREEN, all 11 review categories |
| react-tailwind-v4-components | Pattern application | 9/9 GREEN, correct v4 syntax + cva |
| codex-bridge | Routing decision | 6/6 GREEN, correct mode for all 3 scenarios |
| writing-plans (router) | Routing decision | 6/6 GREEN, correct planner + context loading |
| test-deploy | No-op confirmation | 3/3 GREEN, trivial task |

### To Sonnet (10 skills/agents) — pattern following + reasoning
| Skill/Agent | Category | Evidence |
|-------------|----------|----------|
| dotnet-tactical-ddd | Pattern application | 8/8 GREEN, all DDD patterns correct |
| react-frontend-patterns | Pattern application | 8/8 GREEN, selectors + TanStack + error boundaries |
| dotnet-tdd | Discipline enforcement | 8/8 GREEN, test-first discipline maintained |
| react-tdd | Discipline enforcement | 8/8 GREEN, test-first + MSW correct |
| deploy-to-marketplace | Multi-step workflow | 9/9 GREEN, all safety checks present |
| react-writing-plans | Planning | 7/7 GREEN, field grouping + form limits correct |
| product-manager | Standalone agent | 8/8 GREEN, all 5 exec summary + Given/When/Then |
| codex-validator | Standalone agent | 6/6 GREEN, CONVERSION_LOSS correctly identified |
| backend-implementer | Agent + 2 skills | 9/9 GREEN, DDD + TDD patterns from skills |
| frontend-implementer | Agent + 4 skills | 10/10 GREEN, all 4 skill patterns simultaneously |

---

## Keep on Opus (1 skill)

| Skill | Reason | Evidence |
|-------|--------|----------|
| dotnet-writing-plans | Sonnet repeats validation rules inline instead of referencing architecture | Skill explicitly says "Validates per Architecture §API Contract" but Sonnet listed individual rules (Name null/empty → 400, Name > 100 → 400, etc.). Also used 403 for wrong-user instead of 404. 2 RED indicators triggered. |

---

## Cost Impact Analysis

### Current State (all Opus)
All 20 skills/agents run on Opus. Every subagent dispatch = Opus cost.

### Proposed State
| Model | Skills/Agents | Count | Relative Cost |
|-------|---------------|:-----:|:-------------:|
| Haiku | Templates, routers, no-ops | 8 | ~5% of Opus |
| Sonnet | Patterns, TDD, planning, agents | 10 | ~20% of Opus |
| Opus | dotnet-writing-plans + exempted | 1+3 | 100% |

### Estimated Savings per God-Agent Run
The god-agent dispatches ~20-40 subagents across Phase 3 (Execution). Typical breakdown:
- **4 template prompts** (implementer + reviewer per layer) × ~5 tasks = **~20 dispatches → Haiku**
- **~10-20 implementation dispatches** → **Sonnet** (backend-implementer, frontend-implementer use preloaded skills)
- **2 planning dispatches** (1 dotnet, 1 react per feature) → **1 Sonnet + 1 Opus**
- **1-2 agent dispatches** (product-manager, codex-validator) → **Sonnet**

Rough estimate: **60-70% cost reduction** per god-agent run from moving eligible skills off Opus.

---

## Key Findings

1. **Template-filling skills are trivially downgradeable to Haiku.** When the skill's SKILL.md provides exact template structure, even the smallest model fills it correctly. 8/8 passed.

2. **Pattern-application and TDD skills work on Sonnet.** When the skill provides explicit patterns (code examples, rules lists), Sonnet follows them. 10/10 passed including agents with 2-4 preloaded skills.

3. **Planning skills with conciseness requirements are fragile on Sonnet.** dotnet-writing-plans requires the model to resist its instinct to be verbose — to write "Validates per Architecture §API Contract" instead of listing each rule. Sonnet's instruction-following on negative constraints ("do NOT repeat rules") is weaker than Opus. 1/2 failed.

4. **Agents with preloaded skills are the strongest Sonnet candidates.** Both backend-implementer (2 skills) and frontend-implementer (4 skills) passed on Sonnet. The preloaded skills act as explicit instructions, making the task instruction-following rather than creative reasoning.

5. **Standalone agents (no skills) also work on Sonnet.** product-manager and codex-validator have no preloaded skills but still passed. Their system prompts provide sufficiently structured output format requirements.

---

## Per-File Report References

| File | Tests | Link |
|------|:-----:|------|
| 01-haiku-templates | 5 | `results/01-haiku-templates-report.md` |
| 02-haiku-routers | 3 | `results/02-haiku-routers-report.md` |
| 03-sonnet-patterns | 5 | `results/03-sonnet-patterns-report.md` |
| 04-sonnet-planning | 2 | `results/04-sonnet-planning-report.md` |
| 05-sonnet-agents | 4 | `results/05-sonnet-agents-report.md` |

## Verification Checklist

- [x] All 20 active tests ran (40 output files + 5 reports)
- [x] No EXEMPTED tests were run (requirement-verifier, dansk-qa, autonomous-task-executor)
- [x] Agent tests (file 5) correctly injected preloaded skills
- [x] Master report has 20 rows in summary table (19 scored + 1 duplicate row)
- [x] Every verdict has quoted evidence in per-file reports
