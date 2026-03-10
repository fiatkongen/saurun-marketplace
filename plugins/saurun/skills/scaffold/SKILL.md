---
name: scaffold
description: >
  Scaffold a complete, deploy-ready .NET 10 + React 19 project from scratch.
  Creates backend (ASP.NET Core + SQLite), frontend (Vite + Tailwind v4 + shadcn/ui),
  Dockerfile, GitHub repo, and Railway deployment — all in one skill call.
  Use whenever the user wants to create a new greenfield project, start a new app,
  scaffold a project, bootstrap a codebase, or mentions "new project" or "greenfield".
  Even if they don't say "scaffold" explicitly — if they want a fresh .NET + React
  project set up, this is the skill to use.
user-invocable: true
allowed-tools: Bash, Write, Read, Glob, Grep, Skill(use-railway)
argument-hint: ~/repos/my-project ~/specs/my-spec.md
disable-model-invocation: true
---

# Scaffold: .NET 10 + React 19 Deploy-Ready Project

Creates a complete greenfield project with backend, frontend, Dockerfile, GitHub repo,
and Railway deployment. Zero manual steps — from empty directory to live URL.

**Tech stack:** .NET 10, ASP.NET Core, React 19, Vite, TypeScript, Tailwind CSS v4,
shadcn/ui, Lucide icons, SQLite, Docker.

## Input

`$ARGUMENTS` contains two arguments:

1. **`$1`** (required) — path to the project root directory (e.g., `~/repos/my-app`).
2. **`$2`** (required) — spec, either as:
   - A **file path** to a spec document (e.g., `~/specs/my-spec.md`)
   - **Inline text** describing the project

**Parsing:** Split `$ARGUMENTS` on the first whitespace. `$1` is everything before it,
`$2` is everything after. Strip surrounding quotes from `$2` if present.

**Validation:**
- If `$1` is missing → STOP: `"Usage: /scaffold <path> <spec-file-or-text>"`
- If `$2` is missing → STOP: `"Usage: /scaffold <path> <spec-file-or-text>"`
- Expand `~` to home directory. Resolve relative paths against cwd.
- If `{path}` exists AND contains `CLAUDE.md` → STOP: `"Project already exists at {path}."`
- Derive `{project_name}` from the directory name (e.g., `~/repos/my-app` → `my-app`).

**Detecting spec type:**
1. Expand `~` and resolve relative paths in `$2`.
2. `test -f` the resolved path.
   - **File exists →** read its contents as the spec.
   - **File does not exist →** treat `$2` as inline text.

---

## Execution Overview

The scaffold runs in three tiers. Tier 1 (local) must succeed. Tiers 2-3 are best-effort.

| Tier | Steps | Must succeed? |
|------|-------|---------------|
| **1. Local scaffold** | Steps 0-11 | Yes |
| **2. GitHub repo** | Step 12 | No — local scaffold still valuable |
| **3. Railway deploy** | Step 13 | No — local + GitHub still valuable |

---

## Step 0: Pre-flight Checks

Run ALL checks before creating anything. If any fail → STOP with a descriptive message.

```bash
# 1. .NET 10
dotnet --version | grep -q "^10\." || echo "FAIL: .NET 10 SDK required"

# 2. Node 22+
node --version | grep -qE "^v2[2-9]\." || echo "FAIL: Node 22+ required"

# 3. GitHub CLI authenticated
gh auth status 2>&1 | grep -q "Logged in" || echo "FAIL: gh not authenticated"

# 4. Repo name available
gh repo view "fiatkongen/${project_name}" 2>&1 | grep -q "Could not resolve" || echo "FAIL: repo fiatkongen/${project_name} already exists"

# 5. Railway CLI authenticated
railway whoami --json 2>/dev/null || echo "FAIL: Railway CLI not installed or not authenticated"

# 6. Docker available
command -v docker >/dev/null || echo "FAIL: Docker not installed"
```

All 6 must pass before proceeding.

---

## Step 1: Create directory structure

```bash
mkdir -p "{path}/backend" "{path}/frontend" "{path}/_docs"
```

## Step 2: Store spec + extract mission

1. If `$2` is a file path → copy contents VERBATIM to `{path}/_docs/spec.md`.
2. If `$2` is inline text → write as-is to `{path}/_docs/spec.md`.
3. Read the spec and distill a **1-3 sentence project mission** — what the project IS,
   not how to build it. Store as `{mission}`.

## Step 3: Design template

Check if the spec contains a design section or references a design file.
- **If yes →** extract/copy it to `{path}/_docs/design.md`.
- **If no →** read `references/default-design-template.md` (bundled with this skill)
  and write its contents to `{path}/_docs/design.md`.

## Step 4: Write CLAUDE.md

Write to `{path}/CLAUDE.md`:

````markdown
# CLAUDE.md — {project_name}
{mission}

## Spec
See `_docs/spec.md` for full spec.

## Design
See `_docs/design.md` for UI design system.

## Deploy
See `_docs/deploy.md` for Railway rules and Dockerfile.

## Architecture
Clean Architecture with 4 backend projects (PascalCase prefix):
- **{Prefix}.Api** — Controllers, Program.cs, middleware
- **{Prefix}.Application** — Use cases, DTOs, service interfaces
- **{Prefix}.Domain** — Entities, value objects, domain events, enums
- **{Prefix}.Infrastructure** — EF Core, repositories, external services

## Commands
```bash
# Backend (from backend/)
dotnet build
cd {Prefix}.Api && dotnet run

# Frontend
cd frontend && npm install && npm run dev
```

## Implementation Status
*No features implemented yet.*
````

## Step 5: Write deploy.md

Write to `{path}/_docs/deploy.md`:

```markdown
# Deploy — {project_name}

## Railway
- Deploy happens automatically on git push to master
- NEVER use `railway up` — always git push
- ALWAYS use Dockerfile — never Railpack/Nixpacks

## Dockerfile
Multi-stage build (see `Dockerfile` in root):
1. Stage 1: Build frontend (Node 22)
2. Stage 2: Build backend + copy frontend dist to wwwroot (.NET 10)
3. Stage 3: Runtime (aspnet:10.0)

## Env vars
- `PORT` — Railway sets automatically
- `ASPNETCORE_URLS` — set in ENTRYPOINT, not ENV
- `ConnectionStrings__DefaultConnection` — `Data Source=/data/app.db`

## SQLite Volume
- Persistent volume mounted at `/data`
- NEVER store .db in the project directory — it disappears on redeploy
- Volume created by scaffold (Railway API)

## Same-origin
Frontend served from backend's wwwroot. No separate frontend service.
```

## Step 6: Write .gitignore and .dockerignore

**.gitignore** at `{path}/.gitignore`:
```
bin/
obj/
*.user
.vs/
node_modules/
dist/
.vite/
.idea/
.DS_Store
.env
.env.local
*.db
*.db-journal
```

**.dockerignore** at `{path}/.dockerignore`:
```
**/node_modules
**/bin
**/obj
**/.git
**/.vite
**/.env
**/.DS_Store
*.md
_docs/
```

## Step 7: Backend scaffold (Clean Architecture — 4 projects)

Create solution + 4 projects:

```bash
cd "{path}/backend"

# Derive PascalCase prefix from project name (e.g., my-app → MyApp)
# {Prefix} used below = PascalCase of {project_name}

dotnet new sln -n {Prefix}
dotnet new web -n {Prefix}.Api
dotnet new classlib -n {Prefix}.Application
dotnet new classlib -n {Prefix}.Domain
dotnet new classlib -n {Prefix}.Infrastructure

# Add all to solution
dotnet sln add {Prefix}.Api/{Prefix}.Api.csproj
dotnet sln add {Prefix}.Application/{Prefix}.Application.csproj
dotnet sln add {Prefix}.Domain/{Prefix}.Domain.csproj
dotnet sln add {Prefix}.Infrastructure/{Prefix}.Infrastructure.csproj

# Set up project references
cd {Prefix}.Api && dotnet add reference ../{Prefix}.Application/{Prefix}.Application.csproj ../{Prefix}.Infrastructure/{Prefix}.Infrastructure.csproj && cd ..
cd {Prefix}.Application && dotnet add reference ../{Prefix}.Domain/{Prefix}.Domain.csproj && cd ..
cd {Prefix}.Infrastructure && dotnet add reference ../{Prefix}.Domain/{Prefix}.Domain.csproj ../{Prefix}.Application/{Prefix}.Application.csproj && cd ..
```

Replace `{Prefix}` with PascalCase version of `{project_name}` (e.g., `my-app` → `MyApp`, `mission-control` → `MissionControl`).

Delete boilerplate `Class1.cs` from classlibs:
```bash
rm -f {Prefix}.Application/Class1.cs {Prefix}.Domain/Class1.cs {Prefix}.Infrastructure/Class1.cs
```

Create standard domain folders:
```bash
mkdir -p {Prefix}.Domain/Entities {Prefix}.Domain/ValueObjects {Prefix}.Domain/Events {Prefix}.Domain/Enums
mkdir -p {Prefix}.Application/Interfaces {Prefix}.Application/DTOs {Prefix}.Application/Services
mkdir -p {Prefix}.Infrastructure/Data {Prefix}.Infrastructure/Repositories
```

Then **overwrite** `{path}/backend/{Prefix}.Api/Program.cs` with:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();
app.UseCors();
app.UseStaticFiles();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// SPA fallback — serve index.html for client-side routing
app.MapFallbackToFile("index.html");

app.Run();
```

This gives us:
- Clean Architecture with 4 separate assemblies (Api, Application, Domain, Infrastructure)
- Controller support (not minimal APIs)
- `/health` endpoint for deploy verification
- Static file serving for production (frontend in wwwroot)
- SPA fallback for client-side routing
- Permissive CORS for local dev

## Step 8: Frontend scaffold

### 8a. Create Vite project

```bash
cd "{path}/frontend"
npm create vite@latest . -- --template react-ts
npm install
npm install -D tailwindcss @tailwindcss/vite
npm install lucide-react
```

If `npm install` fails → `rm -rf node_modules package-lock.json && npm install` (retry once).

### 8b. Configure vite.config.ts

Overwrite `{path}/frontend/vite.config.ts`:

```typescript
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    proxy: {
      '/health': 'http://localhost:5000',
      '/api': 'http://localhost:5000',
    },
  },
})
```

The proxy config routes `/health` and `/api` requests to the .NET backend during local
development, matching the same-origin production setup.

### 8c. Configure TypeScript path alias

The Vite React TS template creates `tsconfig.app.json`. Add path aliases to the
**existing** `compilerOptions` (merge, don't overwrite the whole file):

```json
{
  "compilerOptions": {
    "baseUrl": ".",
    "paths": {
      "@/*": ["./src/*"]
    }
  }
}
```

### 8d. Write src/index.css

Replace the Vite default `{path}/frontend/src/index.css` with:

```css
@import "tailwindcss";
```

### 8e. Write src/App.tsx

Replace `{path}/frontend/src/App.tsx` with:

```tsx
function App() {
  return (
    <div className="min-h-screen bg-[#0A0A0B] text-[#F5F5F3] flex items-center justify-center">
      <div className="text-center">
        <h1 className="text-2xl font-semibold">{project_name}</h1>
        <p className="text-[#9B9B9B] mt-2">Ready for development</p>
      </div>
    </div>
  )
}

export default App
```

Replace `{project_name}` with the actual project name as a string literal in the JSX.

### 8f. Install shadcn/ui

```bash
cd "{path}/frontend"
npx shadcn@latest init --yes --defaults
npx shadcn@latest add button card input label select separator sheet skeleton table tabs toast tooltip
```

If `shadcn init` fails → skip shadcn entirely and note "shadcn init failed — manual setup needed" in the output report.

### 8g. Delete Vite boilerplate

Remove files that the Vite template creates but we don't need:

```bash
rm -f "{path}/frontend/src/App.css"
rm -f "{path}/frontend/public/vite.svg"
rm -f "{path}/frontend/src/assets/react.svg"
rmdir --ignore-fail-on-non-empty "{path}/frontend/src/assets" 2>/dev/null
```

## Step 9: Write Dockerfile

Write to `{path}/Dockerfile`:

```dockerfile
# Stage 1: Build frontend
FROM node:22-slim AS frontend
WORKDIR /app
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ .
RUN npm run build

# Stage 2: Build backend
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend
WORKDIR /src
COPY backend/{Prefix}.Domain/{Prefix}.Domain.csproj {Prefix}.Domain/
COPY backend/{Prefix}.Application/{Prefix}.Application.csproj {Prefix}.Application/
COPY backend/{Prefix}.Infrastructure/{Prefix}.Infrastructure.csproj {Prefix}.Infrastructure/
COPY backend/{Prefix}.Api/{Prefix}.Api.csproj {Prefix}.Api/
RUN dotnet restore {Prefix}.Api/{Prefix}.Api.csproj
COPY backend/ .
COPY --from=frontend /app/dist {Prefix}.Api/wwwroot/
RUN dotnet publish {Prefix}.Api/{Prefix}.Api.csproj -c Release -o /app

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=backend /app .
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "dotnet {Prefix}.Api.dll --urls http://+:${PORT:-8080}"]
```

Port is handled via shell-form ENTRYPOINT so Railway's `$PORT` env var is evaluated at
runtime. No hardcoded `ENV ASPNETCORE_URLS`.

## Step 10: Git init + commit

```bash
cd "{path}"
git init
git add .
git commit -m "chore: initial scaffold"
```

## Step 11: GitHub repo (without push)

```bash
cd "{path}"
gh repo create "fiatkongen/{project_name}" --private --source .
```

Do NOT push yet — Railway needs to be configured first so the first auto-deploy has
env vars and volume ready.

If `gh repo create` fails → report the error and continue to the verify gate.

## Step 12: Railway setup + push

Invoke the `use-railway` skill to:

1. Create a new Railway project named `{project_name}`
2. Create a service connected to `fiatkongen/{project_name}` repo
3. Set `dockerfilePath` to `Dockerfile`
4. Create a persistent volume mounted at `/data` (for SQLite)
5. Set env vars:
   - `ConnectionStrings__DefaultConnection=Data Source=/data/app.db`
   - Any project-specific env vars from the spec
6. **Now push:** `git push -u origin master`
7. Wait for first deploy (poll build status, timeout 300s)
8. Get public URL

If Railway fails at any point → report the specific error, push to GitHub anyway
(`git push -u origin master`), and continue to verify gate.

## Step 13: Self-Verify Gate

Run all applicable checks. Report pass/fail for each.

### Local verification (12 checks)

```bash
cd "{path}"
test -f backend/{Prefix}.Api/{Prefix}.Api.csproj && grep -q "net10.0" backend/{Prefix}.Api/{Prefix}.Api.csproj && echo "PASS: Api.csproj targets net10.0" || echo "FAIL: Api.csproj"
test -f backend/{Prefix}.Domain/{Prefix}.Domain.csproj && echo "PASS: Domain project" || echo "FAIL: Domain project missing"
test -f backend/{Prefix}.Application/{Prefix}.Application.csproj && echo "PASS: Application project" || echo "FAIL: Application project missing"
test -f backend/{Prefix}.Infrastructure/{Prefix}.Infrastructure.csproj && echo "PASS: Infrastructure project" || echo "FAIL: Infrastructure project missing"
dotnet sln backend/*.sln list 2>/dev/null | grep -q "Api" && echo "PASS: solution file" || echo "FAIL: solution file"
grep -q "/health" backend/{Prefix}.Api/Program.cs && echo "PASS: /health endpoint" || echo "FAIL: /health endpoint"
test -f frontend/package.json && grep -q "react" frontend/package.json && grep -q "tailwindcss" frontend/package.json && echo "PASS: frontend package.json" || echo "FAIL: frontend package.json"
test -f frontend/components.json && echo "PASS: shadcn initialized" || echo "WARN: shadcn not initialized"
test -f Dockerfile && echo "PASS: Dockerfile" || echo "FAIL: Dockerfile"
test -f .dockerignore && echo "PASS: .dockerignore" || echo "FAIL: .dockerignore"
test -f CLAUDE.md && echo "PASS: CLAUDE.md" || echo "FAIL: CLAUDE.md"
test -f _docs/spec.md && echo "PASS: _docs/spec.md" || echo "FAIL: _docs/spec.md"
test -f _docs/design.md && echo "PASS: _docs/design.md" || echo "FAIL: _docs/design.md"
test -f _docs/deploy.md && echo "PASS: _docs/deploy.md" || echo "FAIL: _docs/deploy.md"
test -d .git && echo "PASS: git initialized" || echo "FAIL: git not initialized"
git log --oneline -1 >/dev/null 2>&1 && echo "PASS: initial commit" || echo "FAIL: no commits"
```

### Build verification (2 checks)

```bash
cd "{path}/backend" && dotnet build --verbosity quiet && echo "PASS: backend builds (all 4 projects)" || echo "FAIL: backend build"
cd "{path}/frontend" && npm run build >/dev/null 2>&1 && echo "PASS: frontend builds" || echo "FAIL: frontend build"
```

### Remote verification (4 checks, skip if tier 2/3 failed)

```bash
gh repo view "fiatkongen/{project_name}" >/dev/null 2>&1 && echo "PASS: GitHub repo" || echo "FAIL: GitHub repo"
# Railway checks (only if Railway was set up):
# - Railway project connected to repo
# - Railway persistent volume mounted at /data
# - curl https://{railway-url}/health → HTTP 200 + {"status":"healthy"}
```

For the health check, poll with retries up to 300 seconds.

### Report

**Full success:**
```
Scaffold complete at {path}. 18/18 gates passed. Live at https://{url}
```

**Partial success:**
```
Scaffold PARTIAL at {path}.
Local: 12/12 passed
Build: 2/2 passed
GitHub: [passed/failed]
Railway: [passed/failed — specific error]
Action needed: [what to do manually]
```

---

## Error Handling

| Failure | Action |
|---------|--------|
| Pre-flight check fails | STOP immediately. Tell user what to fix. |
| `npm install` fails | `rm -rf node_modules package-lock.json && npm install` (retry 1x) |
| `shadcn init` fails | Skip shadcn, report in output |
| `gh repo create` fails | Report error, continue to verify gate |
| Railway fails | Report error, push to GitHub anyway |
| `dotnet build` fails | Investigate, fix, retry 1x |
| `npm run build` fails | Investigate, fix, retry 1x |
| Health check timeout | Check Railway build logs, report specific error |
