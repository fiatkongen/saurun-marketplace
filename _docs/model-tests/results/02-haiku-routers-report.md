# Model A/B Comparison Report
Generated: 2026-02-07 | Source: _docs/model-tests/02-haiku-routers.md

## codex-bridge (Opus -> Haiku)

### GREEN Criteria
| # | Criterion | Opus | Haiku |
|---|-----------|:----:|:-----:|
| 1 | Scenario 1: Uses read-only mode (no --full-auto) | pass | pass |
| 2 | Scenario 2: Uses --full-auto mode (trigger word: "fix") | pass | pass |
| 3 | Scenario 3: Uses read-only mode (trigger word: "opinion") | pass | pass |
| 4 | Always passes --working-dir with correct project path | pass | pass |
| 5 | Always resolves CODEX_BRIDGE path using prescribed ls command | pass | pass |
| 6 | Never invents flags or commands not documented in skill | pass | pass |

### RED Indicators
| # | Indicator | Opus | Haiku |
|---|-----------|:----:|:-----:|
| 1 | Uses --full-auto for scenario 1 or 3 | clean | clean |
| 2 | Omits --working-dir entirely | clean | clean |
| 3 | Invents flags like --review or --analyze | clean | clean |
| 4 | Fails to resolve CODEX_BRIDGE path or hardcodes wrong path | clean | clean |
| 5 | Adds unnecessary JSON escaping or breaks heredoc formatting | clean | clean |

### Verdict: PASS
Haiku correctly routed all 3 scenarios: read-only for opinions (scenarios 1, 3), --full-auto for fix (scenario 2). Both models resolved CODEX_BRIDGE via ls command and always included --working-dir. No invented flags.

---

## writing-plans (Opus -> Haiku)

### GREEN Criteria
| # | Criterion | Opus | Haiku |
|---|-----------|:----:|:-----:|
| 1 | Scenario 1: Routes to saurun:dotnet-writing-plans, loads saurun:dotnet-tactical-ddd first | pass | pass |
| 2 | Scenario 2: Routes to saurun:react-writing-plans, loads saurun:react-tailwind-v4-components first | pass | pass |
| 3 | Scenario 3: Routes to BOTH skills (backend first), loads BOTH context skills | pass | pass |
| 4 | Scenario 4: Routes to BOTH skills (integration: API + UI), backend first | pass | pass |
| 5 | Never routes to a generic "writing-plans" skill that doesn't exist | pass | pass |
| 6 | Announces routing decision at start | pass | pass |

### RED Indicators
| # | Indicator | Opus | Haiku |
|---|-----------|:----:|:-----:|
| 1 | Routes scenario 1 to frontend planner | clean | clean |
| 2 | Routes scenario 2 to backend planner | clean | clean |
| 3 | Routes scenario 3 to only ONE planner | clean | clean |
| 4 | Forgets to load context skills before planning skills | clean | clean |
| 5 | Invents a generic planning skill or writes plan inline | clean | clean |
| 6 | Routes frontend before backend for integration work | clean | clean |

### Verdict: PASS
Haiku correctly detected all 4 work unit types (backend, frontend, scaffold, integration) and routed to the right planner(s) with correct context skill loading order. Backend-first ordering maintained for multi-planner scenarios.

---

## test-deploy (Opus -> Haiku)

### GREEN Criteria
| # | Criterion | Opus | Haiku |
|---|-----------|:----:|:-----:|
| 1 | Successfully invokes without error | pass | pass |
| 2 | Returns acknowledgment or confirms test completion | pass | pass |
| 3 | Does not attempt to perform any actual deployment operations | pass | pass |

### RED Indicators
| # | Indicator | Opus | Haiku |
|---|-----------|:----:|:-----:|
| 1 | Fails to invoke or throws error | clean | clean |
| 2 | Attempts to run marketplace deployment commands | clean | clean |
| 3 | Hallucinates additional functionality not in the skill | clean | clean |

### Verdict: PASS
Both models acknowledged the no-op skill correctly and did not attempt any deployment operations. Haiku provided slightly more detailed explanation of what the skill validates but stayed within documented scope.

---

## requirement-verifier — EXEMPTED
Not tested. Reason: Numeric mutation detection (10MB→5MB, 200ms→500ms) too demanding for Haiku.

---

## Summary
| Skill/Agent | Downgrade | GREEN | RED | Verdict |
|-------------|-----------|:-----:|:---:|:-------:|
| codex-bridge | Opus->Haiku | 6/6 | 0 new | PASS |
| writing-plans | Opus->Haiku | 6/6 | 0 new | PASS |
| test-deploy | Opus->Haiku | 3/3 | 0 new | PASS |

## Safe to Downgrade
All 3 tested skills passed. Router-style decision trees and no-op skills work well on Haiku when instructions are in context.

## Keep on Opus
None from tested skills. (requirement-verifier exempted — stays on Opus by design.)

## Needs More Testing
None — all verdicts are clear PASS.
