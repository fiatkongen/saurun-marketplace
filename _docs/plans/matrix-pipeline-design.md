# Matrix — Contracts-First Implementation Pipeline

**Status:** Skills og agenter færdigbygget. Klar til deploy på Neo.
**Repo:** `fiatkongen/matrix` — Neo's source of truth for alle skills og agenter.
**Opdateret:** 2026-03-11

---

## Kerne-idé

Contracts-first + worktree-isolation. Alle agenter bygger fra contracts (interfaces, DTOs, entities). Test-agent og impl-agent arbejder parallelt i separate git worktrees og kan aldrig se hinandens kode.

---

## Pipeline-oversigt

```
Phase 0: SETUP         → Read plan, assess complexity, run baseline tests
Phase 1: CONTRACTS     → Writer + reviewer loop (Opus, max 3 rounds)
Phase 2: PARALLEL      → Domain tests ‖ backend impl ‖ frontend impl
Phase 3: MERGE         → Merge worktrees, build, run tests
Phase 4: VERIFY        → implementation-verifier checks plan compliance
Phase 5: INTEGRATION   → Integration tests against running app
Phase 6: E2E           → Playwright happy-path tests (large only)
Phase 7: CLEANUP       → Remove worktrees, update LEARNINGS, report
```

---

## Matrix Repo-struktur

```
matrix/
├── agents/                          ← Roller (hvem er du, hvad gør du)
│   ├── orchestrator/                ← Neo's hovedagent (router + 10 refs)
│   │   ├── SKILL.md
│   │   └── references/
│   │       ├── phase-0-setup.md
│   │       ├── phase-1-contracts.md
│   │       ├── phase-2-parallel.md
│   │       ├── phase-3-merge.md
│   │       ├── phase-4-verify.md
│   │       ├── phase-5-integration.md
│   │       ├── phase-6-e2e.md
│   │       ├── phase-7-cleanup.md
│   │       ├── error-handling.md
│   │       └── state-file.md
│   ├── backend-implementer/         ← .NET impl, contracts-aware, 0% drift
│   ├── frontend-implementer/        ← React 19 impl + frontend-design
│   ├── e2e-test-writer/             ← Playwright happy-path tests
│   ├── domain-test-writer/          ← (deprecated — prompt er nok)
│   └── integration-test-writer/     ← (deprecated — prompt er nok)
│
├── skills/                          ← Viden (hvad ved du)
│   ├── contracts/                   ← Writer + reviewer prompts
│   │   ├── SKILL.md
│   │   ├── writer-prompt.md
│   │   └── reviewer-prompt.md
│   ├── dotnet-tactical-ddd/         ← DDD patterns + base classes
│   ├── frontend-design/             ← Æstetik, anti-AI-slop (Anthropic)
│   ├── react-enforcement/           ← Code quality rules
│   ├── react-frontend-patterns/     ← Zustand, TanStack, providers
│   └── implementation-verifier/     ← Plan compliance checker
│
└── _docs/
    ├── LEARNINGS.md                 ← Cross-run erfaringer
    └── plans/                       ← Test-planer
```

---

## Agent-roller og skill-mapping

| Fase | Rolle | Skills/agents | Model |
|------|-------|---------------|-------|
| 1 | Contracts Writer | `skills/contracts/` + `skills/dotnet-tactical-ddd/` | **Opus** |
| 1 | Contracts Reviewer | `skills/contracts/` + `skills/dotnet-tactical-ddd/` | **Opus** |
| 2 | Domain Test Writer | Prompt only (ingen skill nødvendig) | **Sonnet** |
| 2 | Backend Impl | `agents/backend-implementer/` + `skills/dotnet-tactical-ddd/` | **Opus** |
| 2 | Frontend Impl | `agents/frontend-implementer/` + `skills/frontend-design/` + `skills/react-enforcement/` + `skills/react-frontend-patterns/` | **Opus** |
| 4 | Code Review | `skills/implementation-verifier/` | **Opus** |
| 5 | Integration Tests | Prompt only | **Sonnet** |
| 6 | E2E Tests | `agents/e2e-test-writer/` | **Sonnet** |
| * | Orchestrator | `agents/orchestrator/` | **Opus** |

**Model-valg:** Opus til faser der kræver arkitektur-vurdering (contracts, impl, verifikation). Sonnet til mekaniske faser (tests). A/B evals bekræftede at Sonnet skriver lige gode tests med og uden skill.

---

## Validerede Resultater

| Hvad | Resultat |
|------|----------|
| Contracts skill (A/B) | 12/12 assertions med skill vs 7/12 baseline |
| Contracts reviewer | Fangede 3 reelle bugs (int ID=0, manglende using) |
| Backend impl | 0% contract drift, 0 errors, 11 min |
| Domain tests (worktree-isolated) | 129/129 grønne, 3 min |
| Integration tests | 66/66 grønne, 7 min |
| Impl-verifier | Fangede net10/net9, port, .slnx korrekt |
| E2E tests (A/B) | 26/26 begge, ingen skill-forskel for tests |
| **Total pipeline** | **~34 min** |

---

## Kompleksitetsniveauer

| Niveau | Kriterier | Pipeline |
|--------|-----------|----------|
| **small** | Single entity, <3 endpoints, bug fix | Contracts → Backend (ingen tests, ingen worktrees) |
| **medium** | Multi-entity, 3-10 endpoints | Fuld pipeline minus E2E |
| **large** | Ny module, 10+ endpoints, frontend | Fuld pipeline inkl. Playwright E2E |

---

## Kommunikation og bot-roller

```
Rasmus ──→ Claire ──→ Plan ──→ Neo (orchestrator)
                                    │
                       ┌────────────┼────────────┐
                       ▼            ▼            ▼
                 Contracts    Test-agent    Impl-agent
                 (writer↔     (worktree)   (worktree)
                  reviewer)
```

- **Rasmus/Claire:** Opretter plan
- **Neo:** Orchestrator — modtager plan, kører pipeline, rapporterer resultat
- **Workers:** `claude -p` subagenter med klart scope, spawnet af Neo

---

## Fejlhåndtering

| Fejl | Handling | Max retries |
|------|----------|-------------|
| Contracts reviewer afviser | Writer fikser med feedback | 3 runder |
| Build fejler efter merge | Fix-agent med compiler output | 2 |
| Domain tests fejler | Impl-agent med test output | 2 |
| Integration tests fejler | Fix-agent med test output | 2 |
| E2E tests fejler | Rapportér kun — aldrig auto-fix | 0 |
| Agent timeout (>45 min) | Kill + respawn med kontekst | 2 |
| Alt fejler efter max retries | Eskalér til human, bevar state-fil | 0 |

---

## Næste Skridt

1. ✅ ~~Byg contracts skill~~ (done, tested)
2. ✅ ~~Byg backend-implementer~~ (done, tested)
3. ✅ ~~Byg orchestrator~~ (done, reviewed)
4. ✅ ~~Byg e2e-test-writer~~ (done, tested)
5. ✅ ~~Kopiér saurun-skills ind i matrix~~ (done)
6. ✅ ~~Adskil agents/ fra skills/~~ (done)
7. 🔜 Deploy matrix repo til Neo (git clone)
8. 🔜 Første rigtige pipeline-kørsel
9. 🔜 Tank-integration (kompleksitetsvurdering → Neo)
