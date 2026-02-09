# Model A/B Comparison Report
Generated: 2026-02-07 | Source: _docs/model-tests/04-sonnet-planning.md

## dotnet-writing-plans (Opus → Sonnet)

### GREEN Criteria
| # | Criterion | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Task 1 is test infrastructure (CustomWebApplicationFactory + TestHelpers) | pass | pass |
| 2 | All tasks have exact file paths (Create/Modify/Test) | pass | pass |
| 3 | Each task has `Implements:` contract reference | pass | pass |
| 4 | Behaviors are concise (1 line each), not verbose | pass | pass |
| 5 | Dependencies properly identified | pass | pass |
| 6 | Does NOT repeat validation rules inline (references architecture) | pass | FAIL |
| 7 | Does NOT include full test code or implementation code | pass | pass |
| 8 | Task count appropriate (3-6 tasks, not 10+) | pass | pass |

### RED Indicators
| # | Indicator | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Missing test infrastructure in Task 1 | clean | clean |
| 2 | Vague behaviors like "handles errors" | clean | clean |
| 3 | No `Implements:` contract references | clean | clean |
| 4 | Tasks list full test code or implementation code | clean | clean |
| 5 | Repeats validation rules instead of referencing architecture | clean | triggered |
| 6 | File paths missing or incorrect | clean | clean |
| 7 | Circular dependencies or missing dependency tracking | clean | clean |
| 8 | Tasks too large (>3 files per task) | clean | triggered |

### Verdict: FAIL
Sonnet repeated individual validation rules in Task 5 (Name null/empty → 400, Name > 100 chars → 400, Quantity < 1 → 400, etc.) instead of writing "Validates per Architecture §API Contract" as the skill instructs. Also used 403 instead of 404 for wrong-user (spec says 404). Task 5 has 4 files (borderline violation of >3 files rule). Opus correctly referenced architecture and kept it concise.

---

## react-writing-plans (Opus → Sonnet)

### GREEN Criteria
| # | Criterion | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Creates Task 1 for test infrastructure (renderWithProviders + MSW setup) | pass | pass |
| 2 | Form behaviors grouped by field (1 line per field with all constraints) | pass | pass |
| 3 | Does NOT list CSS/styling details | pass | pass |
| 4 | Does NOT list standard patterns ("accepts className", "disables when loading") | pass | pass |
| 5 | Max ~7-9 behaviors for form (field count + 2-3 for submit/populate) | pass | pass |
| 6 | All tasks have `Implements:` references to architecture | pass | pass |
| 7 | File paths use exact component structure (Create + Test) | pass | pass |

### RED Indicators
| # | Indicator | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Splits field validation into separate behaviors | clean | clean |
| 2 | Lists implementation details as behaviors | clean | clean |
| 3 | Includes full test code or component implementation | clean | clean |
| 4 | No grouping by field for validation | clean | clean |
| 5 | Missing test infrastructure in Task 1 | clean | clean |
| 6 | Vague behaviors instead of concrete constraints | clean | clean |
| 7 | Tasks too verbose (>10 behaviors for simple form) | clean | clean |
| 8 | Forgets Tailwind v4 syntax rules | clean | clean |
| 9 | Storing itemCount when items array exists | clean | clean |

### Verdict: PASS
Sonnet produced correct React implementation plan matching Opus quality. Field validation correctly grouped (1 line per field), sub-components split into separate tasks, test infrastructure in Task 1, no implementation details as behaviors.

---

## Summary
| Skill/Agent | Downgrade | GREEN | RED | Verdict |
|-------------|-----------|:-----:|:---:|:-------:|
| dotnet-writing-plans | Opus→Sonnet | 7/8 | 2 new | FAIL |
| react-writing-plans | Opus→Sonnet | 7/7 | 0 new | PASS |

## Safe to Downgrade
- react-writing-plans: Sonnet follows all React planning patterns correctly.

## Keep on Opus
- dotnet-writing-plans: Sonnet repeats validation rules inline instead of referencing architecture. The skill explicitly prohibits this ("Repeating validation rules from architecture → Just write Validates per Architecture §API Contract"). This is a core conciseness requirement.

## Needs More Testing
None — verdicts are clear.
