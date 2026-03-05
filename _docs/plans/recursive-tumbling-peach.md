# Plan: Scaffold Smoke Test

## Context

The scaffold skill (`plugins/saurun/skills/scaffold/SKILL.md`) creates a full .NET 10 + React 19 project in 14 steps (0-13). Steps 0-10 are local, steps 11-13 involve GitHub/Railway. We need an external smoke test to verify the skill actually creates all expected files and builds succeed — not an LLM eval, just "run it and check outputs."

## Files to Create

| File | Purpose |
|------|---------|
| `plugins/saurun/skills/scaffold/tests/verify.sh` | Standalone assertion script — checks file existence, content, builds |
| `plugins/saurun/skills/scaffold/tests/smoke-test.sh` | End-to-end orchestrator — temp dir, `claude -p`, then `verify.sh` |

## T1: `verify.sh <path>` — Deterministic Assertions

Pure bash script. Takes a project path, runs 30 checks, exits non-zero on any failure.

**Checks (grouped):**

File existence (14):
- `backend/Api/Api.csproj`, `backend/Api/Program.cs`
- `frontend/package.json`, `frontend/vite.config.ts`, `frontend/tsconfig.app.json`
- `frontend/src/index.css`, `frontend/src/App.tsx`
- `Dockerfile`, `.dockerignore`, `.gitignore`
- `CLAUDE.md`, `_docs/spec.md`, `_docs/design.md`, `_docs/deploy.md`

Content (9):
- `Api.csproj` contains `net10.0`
- `Program.cs` contains `/health`
- `package.json` contains `react` + `tailwindcss`
- `vite.config.ts` contains `proxy`
- `tsconfig.app.json` contains `@/`
- `index.css` contains `tailwindcss`
- `design.md` contains `--background` (CSS custom properties)
- `Dockerfile` contains multi-stage (`FROM.*AS`)

Boilerplate removal (2):
- `src/App.css` does NOT exist
- `public/vite.svg` does NOT exist

Git (2):
- `.git/` directory exists
- At least 1 commit

shadcn (1, warn-only):
- `frontend/components.json` exists

Build (2):
- `cd backend/Api && dotnet build --verbosity quiet`
- `cd frontend && npm run build`

**Output format:** `PASS:` / `FAIL:` / `WARN:` per check, summary line, exit 1 on any FAIL.

## T2: `smoke-test.sh` — End-to-End Orchestrator

1. Create temp dir + dummy spec file
2. Pre-flight: check `dotnet --version` (10.x) and `node --version` (22+). Exit 2 if missing.
3. Invoke `claude -p` with prompt telling it to run scaffold for the temp dir + spec, skipping steps 11-13 (GitHub/Railway) and their pre-flight checks (3-5).
   - Flags: `--permission-mode dontAsk --allowedTools "Bash Write Read Glob Grep Skill"`
   - `Skill(use-railway)` excluded from allowedTools → Railway calls blocked
4. Run `verify.sh` on the result
5. Cleanup via `trap 'rm -rf "$TMPDIR"' EXIT`

**Exit codes:** 0 = pass, 1 = fail, 2 = skip (missing prereqs)

## Verification

```bash
# Syntax check both scripts
bash -n plugins/saurun/skills/scaffold/tests/verify.sh
bash -n plugins/saurun/skills/scaffold/tests/smoke-test.sh

# Run verify.sh on any existing scaffolded project (manual sanity check)
./plugins/saurun/skills/scaffold/tests/verify.sh ~/repos/some-existing-project

# Full end-to-end (creates temp project, takes ~3-5 min)
./plugins/saurun/skills/scaffold/tests/smoke-test.sh
```
