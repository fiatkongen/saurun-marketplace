---
name: e2e-test
description: >
  Run Playwright E2E tests against User Flows in spec. Starts app, runs tests
  with auto-fix, generates demo videos and categorized failure report.

  REQUIRES: _docs/specs/ with User Flows section, frontend with data-testid attributes.

  OUTPUT: _docs/e2e-results/ with videos, report.md, fix attempts.

user-invocable: true
argument-hint: "[spec-file-path] (optional, defaults to latest spec)"
---

# E2E Testing Skill

Run Playwright E2E tests against User Flows from a product spec. Self-contained lifecycle: starts app → runs tests → tears down. Includes intelligent auto-fix with up to 3 attempts per failing test.

## Prerequisites

- Product spec with `## User Flows` section in `_docs/specs/`
- Frontend with `data-testid` attributes on interactive elements
- Backend with `/health` endpoint
- Playwright installed in frontend

## Execution Flow

### Step 1: Find Spec File

```
If argument provided:
  spec_path = argument
Else:
  Find most recent file matching _docs/specs/*-spec.md or _docs/specs/*.md
  (Exclude *-architecture.md files)
```

### Step 2: Extract User Flows

Parse the `## User Flows` section from the spec file. Each flow has:
- Name (heading)
- Steps (numbered list)
- Expected outcomes

### Step 3: Ensure Playwright Setup

Check if Playwright is configured:

```bash
# If playwright.config.ts doesn't exist in frontend/
cd frontend && npm install -D @playwright/test && npx playwright install chromium
```

Create or update `frontend/playwright.config.ts` using the template in [references/playwright-config.md](references/playwright-config.md).

### Step 4: Generate Test Files

For each User Flow, generate a Playwright test file in `frontend/e2e/`.

See [references/test-generation.md](references/test-generation.md) for mapping rules.

Example output:
```typescript
// frontend/e2e/create-recipe.spec.ts
import { test, expect } from '@playwright/test';

test('Create Recipe flow', async ({ page }) => {
  // Step 1: User clicks "New Recipe" button
  await page.goto('/');
  await page.getByTestId('navbar-new-recipe').click();

  // Step 2: User fills in title, description, cook time
  await page.getByTestId('recipe-form-title').fill('Test Recipe');
  await page.getByTestId('recipe-form-description').fill('A test recipe');
  await page.getByTestId('recipe-form-cooktime').fill('30');

  // Step 5: User clicks "Save"
  await page.getByTestId('recipe-form-submit').click();

  // Step 6: System shows success
  await expect(page.getByTestId('toast-success')).toBeVisible();
});
```

### Step 5: Find Available Ports

Find two available ports to avoid conflicts with other running services:

```bash
# Find available port (Node.js one-liner)
find_port() {
  node -e "const s=require('net').createServer();s.listen(0,()=>{console.log(s.address().port);s.close()})"
}

BACKEND_PORT=$(find_port)
FRONTEND_PORT=$(find_port)

echo "Using ports: Backend=$BACKEND_PORT, Frontend=$FRONTEND_PORT"
```

Export for Playwright config:
```bash
export E2E_BACKEND_PORT=$BACKEND_PORT
export E2E_FRONTEND_PORT=$FRONTEND_PORT
```

### Step 6: Start Backend

```bash
cd backend/Api && dotnet run --urls "http://localhost:$BACKEND_PORT" &
BACKEND_PID=$!
```

Wait for ready:
```bash
# Poll until 200 response (max 60 seconds)
for i in {1..60}; do
  curl -s "http://localhost:$BACKEND_PORT/health" && break
  sleep 1
done
```

### Step 7: Start Frontend

```bash
cd frontend && npm run dev -- --port $FRONTEND_PORT &
FRONTEND_PID=$!
```

Wait for ready:
```bash
# Poll until response (max 30 seconds)
for i in {1..30}; do
  curl -s "http://localhost:$FRONTEND_PORT" && break
  sleep 1
done
```

### Step 8: Run Playwright

```bash
cd frontend && npx playwright test --reporter=html,json
```

Playwright reads `E2E_BACKEND_PORT` and `E2E_FRONTEND_PORT` from environment (see playwright.config.ts).

### Step 9: Process Results & Fix Loop

For each failing test, run the fix loop (see below). After fixes, re-run the full suite to verify.

### Step 10: Generate Report

Write report to `_docs/e2e-results/report.md` using the report template below.

### Step 11: Teardown

```bash
kill $BACKEND_PID $FRONTEND_PID 2>/dev/null || true
```

---

## Fix Loop (Per Failing Test)

```
attempt = 0
max_attempts = 3

while attempt < max_attempts and test still failing:
  attempt++

  Dispatch Task(subagent_type="general-purpose", prompt="""
    {AUTONOMOUS_PREAMBLE}

    SKILL TO LOAD: superpowers:systematic-debugging

    E2E TEST FAILURE:
    - Test file: {path}
    - Test name: {name}
    - Error: {error message}
    - Screenshot: {failure screenshot path}
    - Trace: {trace file path}

    USER FLOW FROM SPEC:
    {original user flow steps}

    RELEVANT CODE:
    - Frontend component: {component path}
    - Backend endpoint: {endpoint path if applicable}
    - Test code: {test file path}

    DIAGNOSE: Is this a test code bug or app code bug?
    FIX: Apply the fix to the correct location.
    VERIFY: The fix compiles/builds.
  """)

  Re-run single test:
  cd frontend && npx playwright test {test file} --grep "{test name}"

Log attempt history to _docs/e2e-results/fix-attempts/{test-name}.md
```

---

## Failure Categorization

See [references/failure-categories.md](references/failure-categories.md) for detailed patterns.

| Pattern | Category |
|---------|----------|
| Connection refused / timeout on startup | `infra` |
| Element not found + selector issue | `test-code` |
| Element not found + missing in DOM | `app-code` |
| Assertion failed on content | `app-code` |
| Timeout waiting for element (intermittent) | `flaky` |

---

## Artifact Structure

```
_docs/e2e-results/
├── report.md                    # Main report
├── videos/
│   ├── create-recipe.webm
│   └── ...
├── failures/                    # Only if unresolved
│   ├── {test}-screenshot.png
│   └── {test}-trace.zip
└── fix-attempts/
    └── {test}.md                # Diagnosis + fix log
```

---

## Report Template

```markdown
# E2E Test Results

## Summary
- **Total flows tested:** {n}
- **Passed:** {n}
- **Fixed (auto):** {n}
- **Unresolved:** {n}
- **Run time:** {duration}

## Videos (Demo Artifacts)
| User Flow | Status | Video |
|-----------|--------|-------|
| {name} | ✅ Passed | [video](videos/{name}.webm) |
| {name} | 🔧 Fixed | [video](videos/{name}.webm) |
| {name} | ❌ Failed | [video](videos/{name}.webm) |

## Fix Attempts Log

### {Test Name} (Fixed on attempt {n})
- **Attempt 1:** Diagnosed as `{category}` — {description}. Fixed `{file}:{line}`.

### {Test Name} (Unresolved after 3 attempts)
- **Attempt 1:** {diagnosis and action}
- **Attempt 2:** {diagnosis and action}
- **Attempt 3:** {diagnosis and action}
- **Root cause:** {best hypothesis}

## Unresolved Failures
| Test | Category | Error | Attempts |
|------|----------|-------|----------|
| {name} | {category} | {error summary} | 3 |

## Failure Categories
| Category | Count | Notes |
|----------|-------|-------|
| `infra` | {n} | Environment/startup issues |
| `app-code` | {n} | Application bugs |
| `test-code` | {n} | Test selector/logic issues |
| `flaky` | {n} | Intermittent failures |
```

---

## Autonomous Preamble (Reference)

```
AUTONOMOUS MODE: You are operating without a human in the loop.
When a skill instructs you to ask the user a question:
1. Identify what information is needed
2. Check STATE.md, spec, and architecture docs for the answer
3. If found -> use it
4. If not found -> make a reasonable decision and log it as an
   assumption in STATE.md under ## Assumptions Log
Never block waiting for human input. Never use AskUserQuestion.

SECURITY GUARDRAILS:
- Never run destructive git commands (force push, reset --hard, clean -f)
- Never delete production data, configuration files, or .env files
- Never modify security-sensitive files (credentials, tokens, secrets)
- Never run commands that affect systems outside the project directory
- Log any security-relevant decision to STATE.md under ## Security Log
```
