# Model A/B Comparison Report
Generated: 2026-02-07 | Source: _docs/model-tests/03-sonnet-patterns.md

## dotnet-tactical-ddd (Opus -> Sonnet)

### GREEN Criteria
| # | Criterion | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Product has private setter on Price property | pass | pass |
| 2 | UpdatePrice() method returns Result<T> with validation | pass | pass |
| 3 | Private constructor + static Create() factory method with validation | pass | pass |
| 4 | Price is a Money value object (not decimal), immutable with validation | pass | pass |
| 5 | Controller uses ProductDto (not Product entity in return type) | pass | pass |
| 6 | Extension methods for ToDto() mapping (no AutoMapper, no inline mapping) | pass | pass |
| 7 | EF Core configuration for owned Money value object with OwnsOne() | pass | pass |
| 8 | No dependency violations (Domain doesn't reference Infrastructure) | pass | pass |

### RED Indicators
| # | Indicator | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Public setters on Product entity | clean | clean |
| 2 | Raw decimal instead of Money value object | clean | clean |
| 3 | Product entity exposed directly in controller (no DTO) | clean | clean |
| 4 | Missing Result<T> pattern, throws exceptions instead | clean | clean |
| 5 | Inline mapping in controller instead of extension methods | clean | clean |
| 6 | No factory method (public constructor allows invalid state) | clean | clean |
| 7 | Missing EF Core configuration for owned types | clean | clean |
| 8 | AutoMapper used despite explicit prohibition | clean | clean |
| 9 | Domain layer importing EF Core or HTTP libraries | clean | clean |

### Verdict: PASS
Sonnet produced complete tactical DDD implementation matching Opus quality. All patterns correct: Money value object, Result<T>, private setters, extension method mapping, OwnsOne() EF config.

---

## react-frontend-patterns (Opus -> Sonnet)

### GREEN Criteria
| # | Criterion | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Zustand selectors everywhere: useCartStore((s) => s.items) — never bare useCartStore() | pass | pass |
| 2 | useShallow for multiple field selection | pass | pass |
| 3 | Export selector hooks (useCartItems, useCartActions) | pass | pass |
| 4 | Derived state (total) computed in selector, not stored | pass | pass |
| 5 | Product data fetched via TanStack Query useQuery hook | pass | pass |
| 6 | Query keys follow convention: ['products', 'list', ...] | pass | pass |
| 7 | Error boundaries at page level | pass | pass |
| 8 | All interactive elements have data-testid attributes | pass | pass |

### RED Indicators
| # | Indicator | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Bare store usage without selectors | clean | clean |
| 2 | Storing derived state instead of computing | clean | clean |
| 3 | API data stored in Zustand instead of TanStack Query | clean | clean |
| 4 | Missing useShallow for multiple field selection | clean | clean |
| 5 | useEffect + useState for data fetching | clean | clean |
| 6 | Missing or incorrectly formatted data-testid | clean | clean |
| 7 | No error boundaries | clean | clean |
| 8 | Query keys missing parameters or not following convention | clean | clean |
| 9 | Storing itemCount when items array exists | clean | clean |

### Verdict: PASS
Sonnet followed all React frontend patterns correctly. Selectors, useShallow, TanStack Query, derived state, error boundaries, and data-testid all present and correct.

---

## dotnet-tdd (Opus -> Sonnet)

### GREEN Criteria
| # | Criterion | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Test written FIRST before any implementation code | pass | pass |
| 2 | Test naming: MethodName_Scenario_ExpectedBehavior | pass | pass |
| 3 | Uses real domain objects — no mocks for domain | pass | pass |
| 4 | NSubstitute used for mocking (not Moq) — N/A domain-only | pass | pass |
| 5 | Max 3 assertions per test or uses [Theory] | pass | pass |
| 6 | Test verified to fail correctly before implementation | pass | pass |
| 7 | Minimal implementation to pass the test | pass | pass |
| 8 | No "CanSetProperty" or getter/setter tests | pass | pass |

### RED Indicators
| # | Indicator | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Implementation code written before test | clean | clean |
| 2 | Test passes immediately (never saw it fail) | clean | clean |
| 3 | Mocking domain objects | clean | clean |
| 4 | Generic naming like "CanSetName" or "TestAddItem" | clean | clean |
| 5 | More than 3 unrelated assertions without [Theory] | clean | clean |
| 6 | Testing language features (property setters work) | clean | clean |
| 7 | Using Moq instead of NSubstitute | clean | clean |
| 8 | Test with zero assertions | clean | clean |
| 9 | Rationalizing "write test after implementation" | clean | clean |

### Verdict: PASS
Both models demonstrated correct TDD discipline: test-first, proper naming convention, real domain objects, [Theory] for parameterized cases, and minimal implementation.

---

## react-tdd (Opus -> Sonnet)

### GREEN Criteria
| # | Criterion | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Test written FIRST before any implementation code | pass | pass |
| 2 | Naming: it('should [behavior] when [condition]') | pass | pass |
| 3 | Real Zustand stores (no vi.mock on stores) | pass | pass |
| 4 | MSW for API mocking (not applicable — no API in test) | pass | pass |
| 5 | Max 3 assertions per test | pass | pass |
| 6 | Test verified to fail correctly before implementation | pass | pass |
| 7 | Minimal implementation to pass | pass | pass |
| 8 | Tests user-visible behavior, not implementation details | pass | pass |

### RED Indicators
| # | Indicator | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Implementation code written before test | clean | clean |
| 2 | Test passes immediately (never saw it fail) | clean | clean |
| 3 | vi.mock used on Zustand stores or React components | clean | clean |
| 4 | Testing implementation details (className, prop structure) | clean | clean |
| 5 | More than 3 unrelated assertions | clean | clean |
| 6 | Test with zero assertions | clean | clean |
| 7 | Testing that a mock was called instead of behavior | clean | clean |
| 8 | Rationalizing "explore first, test later" | clean | clean |

### Verdict: PASS
Both models showed two complete RED-GREEN-COMMIT cycles. Tests target user-visible behavior (empty message, item names). No mocking violations.

---

## deploy-to-marketplace (Opus -> Sonnet)

### GREEN Criteria
| # | Criterion | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Validates SKILL.md exists before proceeding | pass | pass |
| 2 | Validates frontmatter (name, description) present | pass | pass |
| 3 | Checks git state for uncommitted changes, warns user | pass | pass |
| 4 | Compares modification times between local and marketplace | pass | pass |
| 5 | Shows diff and asks user confirmation if marketplace newer | pass | pass |
| 6 | Uses correct copy command for platform (rsync on Unix) | pass | pass |
| 7 | Calls /publish-plugin bump (doesn't manually bump version) | pass | pass |
| 8 | Copies entire skill directory including references/, scripts/ | pass | pass |
| 9 | All safety checks performed — never skipped | pass | pass |

### RED Indicators
| # | Indicator | Opus | Sonnet |
|---|-----------|:----:|:------:|
| 1 | Skips validation checks | clean | clean |
| 2 | Doesn't detect or warn about marketplace being newer | clean | clean |
| 3 | Manual version bumping instead of /publish-plugin bump | clean | clean |
| 4 | Overwrites marketplace without showing diff or asking | clean | clean |
| 5 | Only copies SKILL.md, ignores supporting files | clean | clean |
| 6 | Doesn't check git state | clean | clean |
| 7 | Wrong copy command for platform | clean | clean |
| 8 | Proceeds despite validation failures | clean | clean |
| 9 | Skips safety checks with rationalization | clean | clean |

### Verdict: PASS
Both models followed the complete deployment workflow with all safety checks. rsync on Darwin, stat for timestamps, diff for conflicts, /publish-plugin bump for versioning.

---

## Summary
| Skill/Agent | Downgrade | GREEN | RED | Verdict |
|-------------|-----------|:-----:|:---:|:-------:|
| dotnet-tactical-ddd | Opus->Sonnet | 8/8 | 0 new | PASS |
| react-frontend-patterns | Opus->Sonnet | 8/8 | 0 new | PASS |
| dotnet-tdd | Opus->Sonnet | 8/8 | 0 new | PASS |
| react-tdd | Opus->Sonnet | 8/8 | 0 new | PASS |
| deploy-to-marketplace | Opus->Sonnet | 9/9 | 0 new | PASS |

## Safe to Downgrade
All 5 skills passed. Pattern-following and TDD discipline skills work well on Sonnet when the skill content is injected as context.

## Keep on Opus
None.

## Needs More Testing
None — all verdicts are clear PASS.
