# Playwright Configuration Template

Use this configuration for E2E tests in god-agent projects.

## playwright.config.ts

```typescript
import { defineConfig, devices } from '@playwright/test';

// Dynamic ports from e2e-test skill, with fallback defaults for local dev
const BACKEND_PORT = process.env.E2E_BACKEND_PORT || '5000';
const FRONTEND_PORT = process.env.E2E_FRONTEND_PORT || '5173';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ['html', { outputFolder: '../_docs/e2e-results/playwright-report' }],
    ['json', { outputFile: '../_docs/e2e-results/results.json' }],
  ],
  use: {
    baseURL: `http://localhost:${FRONTEND_PORT}`,
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'on',  // Always record for demo artifacts
  },
  outputDir: '../_docs/e2e-results/raw',
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  // webServer is optional when e2e-test skill manages server lifecycle
  // Uncomment for local development without the skill:
  // webServer: [
  //   {
  //     command: `cd ../backend/Api && dotnet run --urls "http://localhost:${BACKEND_PORT}"`,
  //     url: `http://localhost:${BACKEND_PORT}/health`,
  //     reuseExistingServer: !process.env.CI,
  //     timeout: 60000,
  //   },
  //   {
  //     command: `npm run dev -- --port ${FRONTEND_PORT}`,
  //     url: `http://localhost:${FRONTEND_PORT}`,
  //     reuseExistingServer: !process.env.CI,
  //     timeout: 30000,
  //   },
  // ],
});
```

## Port Configuration

The e2e-test skill finds available ports dynamically to avoid conflicts:

| Environment Variable | Purpose | Default |
|---------------------|---------|---------|
| `E2E_BACKEND_PORT` | Backend server port | 5000 |
| `E2E_FRONTEND_PORT` | Frontend dev server port | 5173 |

**How it works:**
1. Skill finds 2 available ports before starting servers
2. Exports them as environment variables
3. Starts backend with `--urls http://localhost:$BACKEND_PORT`
4. Starts frontend with `--port $FRONTEND_PORT`
5. Playwright reads ports from environment

**Local development:** If running Playwright manually without the skill, the defaults (5000/5173) are used.

## Key Settings Explained

| Setting | Value | Reason |
|---------|-------|--------|
| `video: 'on'` | Always record | Demo artifacts for stakeholders |
| `trace: 'retain-on-failure'` | Keep traces on fail | Debugging failed tests |
| `screenshot: 'only-on-failure'` | Capture on fail | Debugging context |
| `outputDir` | `_docs/e2e-results/raw` | Centralized artifact location |

## CI Environment

In CI (detected via `process.env.CI`):
- Retries set to 2 (handle flaky tests)
- Workers set to 1 (avoid race conditions)
- `forbidOnly: true` (prevent `.only` from passing)

## Custom Video Output

Videos are saved to `_docs/e2e-results/raw/` by Playwright. After test run, copy videos to `_docs/e2e-results/videos/` with friendly names:

```bash
# Post-test video organization
for f in _docs/e2e-results/raw/**/*.webm; do
  test_name=$(basename $(dirname $f))
  cp "$f" "_docs/e2e-results/videos/${test_name}.webm"
done
```
