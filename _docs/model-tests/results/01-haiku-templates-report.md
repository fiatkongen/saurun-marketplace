# Model A/B Comparison Report
Generated: 2026-02-07 | Source: _docs/model-tests/01-haiku-templates.md

## react-implementer-prompt (Opus -> Haiku)

### GREEN Criteria
| # | Criterion | Opus | Haiku |
|---|-----------|:----:|:-----:|
| 1 | All 7 required sections present | pass | pass |
| 2 | Task Description contains full task text | pass | pass |
| 3 | Context mentions Task 1 dependency | pass | pass |
| 4 | "Your Job" references both sub-skills (react-tdd, react-tailwind-v4-components) | pass | pass |
| 5 | Working directory is absolute path | pass | pass |
| 6 | Self-review has all 5 categories | pass | pass |
| 7 | Stack specification correct (React 19 + Vite + TS + Tailwind v4 + Zustand + Vitest + RTL + MSW) | pass | pass |
| 8 | TDD section says "REQUIRED SUB-SKILL: Follow saurun:react-tdd" | pass | pass |
| 9 | Report format lists all 5 outputs | pass | pass |

### RED Indicators
| # | Indicator | Opus | Haiku |
|---|-----------|:----:|:-----:|
| 1 | Missing any of 7 required sections | clean | clean |
| 2 | "see task 3 in plan" instead of full text | clean | clean |
| 3 | Omits either sub-skill reference | clean | clean |
| 4 | Working directory relative or missing | clean | clean |
| 5 | Self-review abbreviated or missing categories | clean | clean |
| 6 | Hallucinated extra instructions | clean | clean |
| 7 | Fails to mention Task 1 dependency | clean | clean |
| 8 | Stack specification incomplete or wrong | clean | clean |

### Verdict: PASS
Haiku produced all 7 required sections with full task text, both sub-skill references, correct absolute path, all 5 self-review categories, and complete stack specification. Opus provided richer context (store shape, expected RED steps) but Haiku met all template requirements.

---

## dotnet-implementer-prompt (Opus -> Haiku)

### GREEN Criteria
| # | Criterion | Opus | Haiku |
|---|-----------|:----:|:-----:|
| 1 | All 6 required sections present | pass | pass |
| 2 | Task Description has full text (all 7 bullets) | pass | pass |
| 3 | Context mentions bounded context, layer, patterns, dependencies | pass | pass |
| 4 | "Your Job" has "REQUIRED SUB-SKILL: Follow saurun:dotnet-tdd" | pass | pass |
| 5 | Working directory is absolute path | pass | pass |
| 6 | Self-review has all 4 categories | pass | pass |
| 7 | Testing references saurun:dotnet-tdd verification checklist | pass | pass |
| 8 | Verification command is `dotnet test` | pass | pass |
| 9 | Report format lists all 5 items | pass | pass |

### RED Indicators
| # | Indicator | Opus | Haiku |
|---|-----------|:----:|:-----:|
| 1 | Missing any required section | clean | clean |
| 2 | "see task 2" instead of full spec | clean | clean |
| 3 | Omits saurun:dotnet-tdd reference | clean | clean |
| 4 | Self-review abbreviated | clean | clean |
| 5 | Working directory relative or missing | clean | clean |
| 6 | Wrong verification command | clean | clean |
| 7 | Hallucinated extra instructions | clean | clean |
| 8 | Fails to mention CreateCustomerCommandHandler pattern | clean | clean |
| 9 | Testing doesn't reference dotnet-tdd checklist | clean | clean |

### Verdict: PASS
Haiku correctly produced all 6 sections, pasted full task specification with all validation rules, referenced CreateCustomerCommandHandler, specified correct absolute path and `dotnet test` command.

---

## dotnet-code-quality-reviewer-prompt (Opus -> Haiku)

### GREEN Criteria
| # | Criterion | Opus | Haiku |
|---|-----------|:----:|:-----:|
| 1 | Structured as Task call targeting superpowers:code-reviewer | pass | pass |
| 2 | All 5 placeholder variables present | pass | pass |
| 3 | WHAT_WAS_IMPLEMENTED contains implementation summary | pass | pass |
| 4 | PLAN_OR_REQUIREMENTS references correct task/file | pass | pass |
| 5 | BASE_SHA and HEAD_SHA use actual values from input | pass | pass |
| 6 | ADDITIONAL_REVIEW_CRITERIA has all 8 categories | pass | pass |
| 7 | Behavioral Testing flags getter/setter with examples | pass | pass |
| 8 | Mock Boundaries mentions NSubstitute | pass | pass |
| 9 | Anti-Patterns lists all 9 named patterns | pass | pass |
| 10 | Severity Guide has 3 levels (Critical, Important, Minor) | pass | pass |

### RED Indicators
| # | Indicator | Opus | Haiku |
|---|-----------|:----:|:-----:|
| 1 | Missing placeholder variables | clean | clean |
| 2 | ADDITIONAL_REVIEW_CRITERIA abbreviated (<8 categories) | clean | clean |
| 3 | Behavioral Testing doesn't flag getter/setter | clean | clean |
| 4 | Anti-Patterns lists fewer than 10 patterns | clean | clean |
| 5 | Severity Guide missing or incomplete | clean | clean |
| 6 | Doesn't target superpowers:code-reviewer | clean | clean |
| 7 | SHAs left as placeholders | clean | clean |
| 8 | EF Core Patterns section missing | clean | clean |
| 9 | Test Infrastructure missing CustomWebApplicationFactory/SQLite | clean | clean |
| 10 | Hallucinated unrelated criteria | clean | clean |

### Verdict: PASS
Haiku produced complete dispatch prompt with all 5 placeholder variables filled, all 8 ADDITIONAL_REVIEW_CRITERIA categories, all 9 anti-patterns, and 3-level severity guide. Identical structural fidelity to Opus.

---

## react-code-quality-reviewer-prompt (Opus -> Haiku)

### GREEN Criteria
| # | Criterion | Opus | Haiku |
|---|-----------|:----:|:-----:|
| 1 | Targets superpowers:code-reviewer | pass | pass |
| 2 | All 5 placeholder variables present | pass | pass |
| 3 | WHAT_WAS_IMPLEMENTED contains implementation summary | pass | pass |
| 4 | PLAN_OR_REQUIREMENTS = "Task 3 from _docs/plans/products-feature.md" | pass | pass |
| 5 | SHAs use actual values (aaa111bbb, ccc222ddd) | pass | pass |
| 6 | All 11 checklist categories present | pass | pass |
| 7 | Behavioral Testing flags className tests with examples | pass | pass |
| 8 | Mock Boundaries mentions MSW + real Zustand stores | pass | pass |
| 9 | Anti-Patterns lists 11 named patterns | pass | pass |
| 10 | Tailwind v4 checks [--var] → (--var) syntax | pass | pass |
| 11 | Severity Guide has 3 levels | pass | pass |

### RED Indicators
| # | Indicator | Opus | Haiku |
|---|-----------|:----:|:-----:|
| 1 | Missing placeholder variables | clean | clean |
| 2 | ADDITIONAL_REVIEW_CRITERIA abbreviated (<9 categories) | clean | clean |
| 3 | Behavioral Testing doesn't flag className tests | clean | clean |
| 4 | Mock Boundaries doesn't mention MSW or mocks Zustand | clean | clean |
| 5 | Anti-Patterns lists fewer than 10 patterns | clean | clean |
| 6 | Tailwind v4 Compliance missing | clean | clean |
| 7 | React 19 Specifics missing | clean | clean |
| 8 | Accessibility section missing | clean | clean |
| 9 | Doesn't target superpowers:code-reviewer | clean | clean |
| 10 | Hallucinated unrelated criteria | clean | clean |
| 11 | Zustand Store Review missing | clean | clean |

### Verdict: PASS
Haiku produced complete dispatch with all 11 review categories, all 11 anti-patterns, correct placeholder values, and full Tailwind v4 + React 19 + Zustand + Accessibility sections.

---

## react-tailwind-v4-components (Opus -> Haiku)

### GREEN Criteria
| # | Criterion | Opus | Haiku |
|---|-----------|:----:|:-----:|
| 1 | Uses cva with variants object | pass | pass |
| 2 | CSS variable syntax uses parentheses bg-(--var) | pass | pass |
| 3 | data-variant={variant} attribute present | pass | pass |
| 4 | CSS file with [data-variant="..."] selectors | pass | pass |
| 5 | Shadow uses shadow-xs (smallest) | pass | pass |
| 6 | Focus uses outline-offset-* not ring-offset-* | pass | pass |
| 7 | Uses cn() for class merging | pass | pass |
| 8 | Imports cn from @/lib/utils | pass | pass |
| 9 | No template literals for className | pass | pass |

### RED Indicators
| # | Indicator | Opus | Haiku |
|---|-----------|:----:|:-----:|
| 1 | CSS variable syntax uses brackets [--var] | clean | clean |
| 2 | Uses inline style when data-attribute requested | clean | clean |
| 3 | Shadow uses shadow-sm or shadow instead of shadow-xs | clean | clean |
| 4 | Uses deprecated ring-offset-* | clean | clean |
| 5 | Template literals for class merging | clean | clean |
| 6 | Missing data-variant attribute | clean | clean |
| 7 | Missing CSS file with selectors | clean | clean |
| 8 | Uses theme(colors.blue.500) instead of var(--color-blue-500) | clean | clean |
| 9 | Doesn't use cva | clean | clean |
| 10 | v3 important syntax (!prefix) | clean | clean |
| 11 | Hallucinated CSS variable names | clean | clean |

### Verdict: PASS
Haiku produced correct Tailwind v4 implementation: parentheses CSS variable syntax, data-attribute pattern with CSS selectors using var(--color-*), shadow-xs, outline-offset focus states, cva + cn(). Haiku over-delivered with subcomponents (AlertTitle, AlertDescription, InteractiveAlert) but core requirements all met.

---

## Summary
| Skill/Agent | Downgrade | GREEN | RED | Verdict |
|-------------|-----------|:-----:|:---:|:-------:|
| react-implementer-prompt | Opus->Haiku | 9/9 | 0 new | PASS |
| dotnet-implementer-prompt | Opus->Haiku | 9/9 | 0 new | PASS |
| dotnet-code-quality-reviewer-prompt | Opus->Haiku | 10/10 | 0 new | PASS |
| react-code-quality-reviewer-prompt | Opus->Haiku | 11/11 | 0 new | PASS |
| react-tailwind-v4-components | Opus->Haiku | 9/9 | 0 new | PASS |

## Safe to Downgrade
All 5 skills passed. These are template/pattern-following tasks where the SKILL.md content is injected as context — Haiku follows the template structure faithfully when the instructions are right there in its context window.

## Keep on Opus
None.

## Needs More Testing
None — all verdicts are clear PASS.
