# Model Downgrade Test Definitions: Opus → Haiku Routers

Tests for router-style skills and verification agents that may work on Haiku.

## Critical Context Note

All skills in this file are **inline context injections** — their SKILL.md content is loaded into the model's context window before the task is given. The model reads the template/patterns and fills in blanks. It does NOT need to recall these from training.

**Both model runs (Opus baseline and Haiku proposed) MUST have the skill's SKILL.md content injected as context.** Without this, you're testing model knowledge, not instruction-following ability.

This makes Haiku MORE likely to succeed than naive testing suggests — the instructions are right there to follow.

---

### codex-bridge (Opus → Haiku)

**What it does:** Routes user requests to OpenAI's Codex CLI, deciding between read-only mode and `--full-auto` based on intent.

**Test input:**
```
Run these 3 scenarios:

1. "Ask Codex what it thinks about this architecture"
2. "Have Codex fix the linting errors in src/utils.ts"
3. "Get a second opinion on my error handling approach in the payment service"
```

**GREEN criteria (pass — Haiku is sufficient):**
- Scenario 1: Uses read-only mode (no `--full-auto` flag)
- Scenario 2: Uses `--full-auto` mode (trigger word: "fix")
- Scenario 3: Uses read-only mode (trigger word: "opinion")
- Always passes `--working-dir` with the correct project path
- Always resolves `CODEX_BRIDGE` path using the prescribed `ls` command
- Never invents flags or commands not documented in the skill

**RED indicators (fail — needs bigger model):**
- Uses `--full-auto` for scenario 1 or 3 (opinion/review should be read-only)
- Omits `--working-dir` entirely
- Invents flags like `--review` or `--analyze` that don't exist
- Fails to resolve `CODEX_BRIDGE` path or hardcodes a wrong path
- Adds unnecessary JSON escaping or breaks heredoc formatting for long prompts

---

### writing-plans (Opus → Haiku)

**What it does:** Routes to stack-specific planning skill (backend vs frontend vs scaffold vs integration) based on spec content.

**Test input:**
```
Analyze these 4 specs and route to the correct planner for each:

1. Spec mentions: "Add GET /api/users endpoint, UserEntity, UserService, authentication required"
2. Spec mentions: "Create UserProfile component, form validation, state management with Zustand"
3. Spec mentions: "New project from scratch, needs API + React frontend + database schema"
4. Spec mentions: "Add product filter UI with filters stored in query params, API returns filtered results"
```

**GREEN criteria (pass — Haiku is sufficient):**
- Scenario 1: Routes to `saurun:dotnet-writing-plans`, loads `saurun:dotnet-tactical-ddd` context first
- Scenario 2: Routes to `saurun:react-writing-plans`, loads `saurun:react-tailwind-v4-components` context first
- Scenario 3: Routes to BOTH skills (backend first), loads BOTH context skills
- Scenario 4: Routes to BOTH skills (integration: API + UI), backend first
- Never routes to a generic "writing-plans" skill that doesn't exist
- Announces routing decision at start

**RED indicators (fail — needs bigger model):**
- Routes scenario 1 to frontend planner (misread API signals)
- Routes scenario 2 to backend planner (misread UI signals)
- Routes scenario 3 to only ONE planner (missed "from scratch" = scaffold)
- Forgets to load context skills before planning skills
- Invents a generic planning skill or tries to write plan inline
- Routes frontend before backend for integration work (wrong order)

---

### test-deploy (Opus → Haiku)

**What it does:** No-op skill for validating marketplace deployment workflow.

**Test input:**
```
/test-deploy
```

**GREEN criteria (pass — Haiku is sufficient):**
- Successfully invokes without error
- Returns acknowledgment or confirms test completion
- Does not attempt to perform any actual deployment operations

**RED indicators (fail — needs bigger model):**
- Fails to invoke or throws error
- Attempts to run marketplace deployment commands
- Hallucinates additional functionality not in the skill

---

> **⛔ EXEMPTED FROM TESTING** — Staying on Opus. Numeric mutation detection (10MB→5MB, 200ms→500ms) requires sustained attention that Haiku cannot reliably provide. Kept here for documentation only.

### ~~requirement-verifier~~ (Opus → Haiku) — EXEMPTED

**What it does:** Verifies no requirements were lost or mutated when structure-optimizer converted prose plan to PLAN.md.

**⚠️ Risk level: HIGH** — This is the most demanding Haiku candidate. Requires sustained attention across a long list, distinguishing real mutations from legitimate paraphrasing.

**Test input:**
```
Verify this conversion:

Source plan (_docs/plans/test-source.md):
---
Requirements:
1. User can upload CSV files up to 10MB
2. Retry failed uploads 3 times with exponential backoff (1s, 2s, 4s)
3. Return HTTP 413 if file exceeds 10MB
4. Log all upload errors to Sentry with stack traces
5. Response time must be < 200ms for the validation endpoint
6. Support concurrent uploads (max 5 per user)
7. Files must be UTF-8 encoded
8. Uploaded files are virus-scanned before processing
9. Admin users can upload up to 50MB
10. Rate limit: max 100 uploads per hour per user
---

PLAN.md:
---
<requirements>
- R1: User can upload CSV files up to 5MB [COMPLETE]
- R2: Retry failed uploads with backoff [COMPLETE]
- R3: Return error if file too large [COMPLETE]
- R4: Log upload errors to Sentry [COMPLETE]
- R5: Response time < 500ms for validation [COMPLETE]
- R6: Support concurrent uploads, max 5 per user [COMPLETE]
- R7: Files must be UTF-8 encoded [COMPLETE]
- R8: Uploaded files are scanned before processing [COMPLETE]
- R9: Admin users can upload up to 50MB [COMPLETE]
</requirements>
---

PLAN.mapping.md:
---
| Source | PLAN.md | Status |
|--------|---------|--------|
| "upload CSV up to 10MB" | R1 | [COMPLETE] |
| "Retry 3 times with exponential backoff" | R2 | [COMPLETE] |
| "Return HTTP 413" | R3 | [COMPLETE] |
| "Log errors to Sentry with stack traces" | R4 | [COMPLETE] |
| "< 200ms response" | R5 | [COMPLETE] |
| "concurrent uploads max 5" | R6 | [COMPLETE] |
| "UTF-8 encoded" | R7 | [COMPLETE] |
| "virus-scanned" | R8 | [COMPLETE] |
| "Admin upload 50MB" | R9 | [COMPLETE] |
| "100 uploads per hour" | - | UNMAPPED |
---
```

**Expected findings (4 mutations + 1 missing + 3 legitimate paraphrases):**

| Item | Type | Detail |
|------|------|--------|
| R1 | MUTATED | "10MB" → "5MB" (size limit changed) |
| R2 | MUTATED | "3 times with exponential backoff (1s, 2s, 4s)" → "with backoff" (retry count + timings lost) |
| R3 | MUTATED | "HTTP 413" → "error" (specific status code lost) |
| R5 | MUTATED | "< 200ms" → "< 500ms" (threshold changed) |
| R10 | MISSING | "Rate limit: 100 uploads/hour" in unmapped items |
| R4 | OK | "with stack traces" → acceptable simplification for plan level |
| R6 | OK | Legitimate paraphrase (identical meaning) |
| R8 | OK | "virus-scanned" → "scanned" — acceptable shortening |

**GREEN criteria (pass — Haiku is sufficient):**
- Detects ALL 4 mutations: R1 (size), R2 (retry details), R3 (status code), R5 (threshold)
- Detects MISSING R10 (rate limit) from unmapped items
- Does NOT flag R4, R6, R7, R8, R9 as mutated (legitimate paraphrases)
- Returns status: `❌ INCOMPLETE`
- Outputs `ITEMS_FOR_RE-RUN` section with source quotes for each mutation
- Reads all three files before verification

**RED indicators (fail — needs bigger model):**
- Misses any of the 4 mutations (especially R2 losing retry count)
- Misses the missing R10 rate limit
- False positives: flags R4 "stack traces" or R8 "virus-scanned→scanned" as mutations
- Marks status as ✅ COMPLETE
- Doesn't quote source lines for findings
- Misses R2 because "with backoff" seems close enough (attention failure)
- Invents mutations that don't exist

---

## Notes

- **codex-bridge**: Flag detection logic is simple (read-only vs `--full-auto`). Haiku should handle decision tree.
- **writing-plans**: Detection heuristics are keyword-based. Haiku can pattern-match stack signals.
- **test-deploy**: Trivial no-op. Haiku baseline.
- **requirement-verifier**: Complex diff/mutation detection. Most likely to fail on Haiku. Watch for:
  - Missing subtle mutations (threshold changes, error codes)
  - False positives on paraphrasing
  - Failure to use mapping table systematically
