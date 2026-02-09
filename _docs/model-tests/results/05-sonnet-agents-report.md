# Model A/B Comparison Report
Generated: 2026-02-07 | Source: _docs/model-tests/05-sonnet-agents.md

## product-manager (Opus → Sonnet)

### GREEN Criteria
| # | Criterion | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Executive Summary with all 5 required sections | pass | pass |
| 2 | At least 3 user stories in correct format | pass | pass |
| 3 | Acceptance criteria in Given/When/Then format | pass | pass |
| 4 | Functional requirements (user flows, state management, validation, integration) | pass | pass |
| 5 | Non-functional requirements (performance, security, scalability, accessibility) | pass | pass |
| 6 | Asks clarifying questions about gaps | pass | pass |
| 7 | Output saved to project-documentation/product-manager-output.md | pass | pass |
| 8 | Priorities justified (P0/P1/P2) | pass | pass |

### RED Indicators
| # | Indicator | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Missing any of the 5 Executive Summary components | clean | clean |
| 2 | User stories lack persona/action/benefit structure | clean | clean |
| 3 | Acceptance criteria missing or not in Given/When/Then | clean | clean |
| 4 | Skips non-functional requirements section entirely | clean | clean |
| 5 | No clarifying questions asked | clean | clean |
| 6 | Output not saved to specified location | clean | clean |
| 7 | Generic/vague criteria that aren't testable | clean | clean |
| 8 | Fails to address security in acceptance criteria | clean | clean |

### Verdict: PASS
Both models produced comprehensive product plans with all required sections. Sonnet's output had 3 features (vs Opus's 4) but meets the "at least 3" criterion. Both had 6+ clarifying questions, Given/When/Then acceptance criteria, security coverage, and structured non-functional requirements. Quality difference is negligible.

---

## codex-validator (Opus → Sonnet)

### GREEN Criteria
| # | Criterion | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Correctly identifies as CONVERSION_LOSS | pass | pass |
| 2 | Output includes exact format (VALIDATION + GAP_TYPE) | pass | pass |
| 3 | Source Check quotes both files accurately | pass | pass |
| 4 | Provides 2-3 sentence reasoning referencing both files | pass | pass |
| 5 | Recommends a fix in structured format | pass | pass |
| 6 | Concludes with END_OF_VALIDATION | pass | pass |

### RED Indicators
| # | Indicator | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Misclassifies the gap type | clean | clean |
| 2 | Missing required output format sections | clean | clean |
| 3 | Doesn't quote from both files | clean | clean |
| 4 | Reasoning is generic or doesn't reference actual content | clean | clean |
| 5 | Accepts a gap that's actually present (false positive) | clean | clean |
| 6 | Rejects a gap that's clearly missing (false negative) | clean | clean |
| 7 | No END_OF_VALIDATION marker | clean | clean |

### Verdict: PASS
Both models produced virtually identical validation outputs. Both correctly identified CONVERSION_LOSS, quoted the source file, noted the PLAN.md omission, and provided structured fix recommendations. Sonnet matches Opus quality exactly on this structured validation task.

---

## backend-implementer (Opus → Sonnet)

### GREEN Criteria
| # | Criterion | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Creates test file BEFORE implementation (TDD) | pass | pass |
| 2 | TeamInvitation entity has domain logic methods (not anemic) | pass | pass |
| 3 | Uses EmailAddress value object (not raw string) | pass | pass |
| 4 | Enums properly defined (TeamRole, InvitationStatus) | pass | pass |
| 5 | Command handler returns Result<TeamInvitationId> | pass | pass |
| 6 | Validation failures return error Results | pass | pass |
| 7 | Entity guards against invalid state (private setter, factory) | pass | pass |
| 8 | Test covers happy path + validation failures | pass | pass |
| 9 | No DTOs in domain layer | pass | pass |

### RED Indicators
| # | Indicator | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Writes implementation before tests | clean | clean |
| 2 | Anemic entity (public setters, no domain logic) | clean | clean |
| 3 | Uses raw strings instead of value objects | clean | clean |
| 4 | Throws exceptions instead of returning Result<T> | clean | clean |
| 5 | Missing validation in domain logic | clean | clean |
| 6 | DTOs mixed with domain entities | clean | clean |
| 7 | Test is integration test instead of unit test | clean | clean |
| 8 | No factory method or domain invariant enforcement | clean | clean |
| 9 | Skips ExpiresAt validation | clean | clean |

### Verdict: PASS
Both models produced correct DDD implementations with TDD workflow. Key difference: Opus injected ITimeProvider for testable time (exact 7-day assertion), while Sonnet used DateTimeOffset.UtcNow directly (time-window assertion). Both pass all GREEN criteria. Sonnet's direct time usage is a design quality difference, not a criteria failure — the entity still sets 7-day expiration and tests still verify it.

---

## frontend-implementer (Opus → Sonnet)

### GREEN Criteria
| # | Criterion | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Creates test file BEFORE component (TDD) | pass | pass |
| 2 | Test uses Vitest + RTL + MSW | pass | pass |
| 3 | All interactive elements have data-testid | pass | pass |
| 4 | Uses Tailwind v4 syntax | pass | pass |
| 5 | Form uses controlled inputs | pass | pass |
| 6 | Mutation hook from TanStack Query | pass | pass |
| 7 | Loading/error/success states handled | pass | pass |
| 8 | Validation before submit | pass | pass |
| 9 | Placeholders use data-asset attribute | pass | pass |
| 10 | Uses shadcn/ui components | pass | pass |

### RED Indicators
| # | Indicator | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Writes component before test | clean | clean |
| 2 | Missing data-testid on any interactive element | clean | clean |
| 3 | Wrong testid format | clean | clean |
| 4 | Uses Tailwind v3 syntax | clean | clean |
| 5 | No validation before submit | clean | clean |
| 6 | Doesn't use TanStack Query | clean | clean |
| 7 | Missing loading/error states | clean | clean |
| 8 | Uses external placeholder service | clean | clean |
| 9 | Test uses Jest instead of Vitest | clean | clean |
| 10 | Test doesn't mock API with MSW | clean | clean |
| 11 | Uncontrolled form inputs | clean | clean |

### Verdict: PASS
Both models produced correct React implementations with TDD workflow. Opus used native `<select>` for test reliability (pragmatic choice); Sonnet used shadcn Select with Radix (more polished but potentially flaky in tests). Both correctly used Tailwind v4 parentheses syntax, cn(), data-testid attributes, data-asset placeholders, and react-hook-form + zod. Sonnet additionally used shadcn Form wrapper (FormField, FormItem, etc.) which is valid shadcn usage.

---

## Summary
| Skill/Agent | Downgrade | GREEN | RED | Verdict |
|-------------|-----------|:-----:|:---:|:-------:|
| product-manager | Opus→Sonnet | 8/8 | 0 new | PASS |
| codex-validator | Opus→Sonnet | 6/6 | 0 new | PASS |
| backend-implementer | Opus→Sonnet | 9/9 | 0 new | PASS |
| frontend-implementer | Opus→Sonnet | 10/10 | 0 new | PASS |

## Safe to Downgrade
- **product-manager**: Sonnet produces equivalent product plans with all required sections.
- **codex-validator**: Sonnet matches Opus exactly on structured validation tasks.
- **backend-implementer**: Sonnet follows all DDD and TDD patterns from preloaded skills correctly. Minor design quality difference (no time abstraction) does not affect criteria compliance.
- **frontend-implementer**: Sonnet follows all React patterns from 4 preloaded skills simultaneously. Uses shadcn Form components more thoroughly than Opus.

## Keep on Opus
None.

## Needs More Testing
None — all 4 agents showed clear Sonnet sufficiency. The test file's hypothesis was correct: agents with preloaded skills are strong Sonnet candidates because the model's job is instruction-following, not creative reasoning.
