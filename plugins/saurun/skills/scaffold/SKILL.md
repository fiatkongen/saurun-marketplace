---
name: scaffold
description: >
  Use when creating a new greenfield .NET 9 + React 19 project structure.
  Creates backend/, frontend/, _docs/ directories, .gitignore, CLAUDE.md,
  minimal backend with /health endpoint, and git init.
user-invocable: true
allowed-tools: Bash, Write, Read, Glob
argument-hint: ~/repos/playground/my-new-project
---

# Scaffold: .NET 9 + React 19 Project

Creates a complete greenfield project structure ready for development.

**Tech stack:** .NET 9, ASP.NET Core, React 19, Vite, TypeScript, Tailwind CSS v4, Zustand, SQLite.

## Input

`$ARGUMENTS` = absolute or relative path to the project root directory (e.g., `~/repos/playground/my-app`).

**Validation:** If `$ARGUMENTS` is empty or missing, STOP with error: "Usage: /scaffold <project-path>".

Expand `~` to the user's home directory. Resolve relative paths against the current working directory.

If the target directory already exists AND contains a `CLAUDE.md` file, STOP: "Project already exists at {path}. Use god-agent extension mode instead."

## Steps

### 1. Create directory structure

```bash
mkdir -p {path}/backend {path}/frontend {path}/_docs
```

Result:
```
{path}/
├── backend/
├── frontend/
└── _docs/
```

### 2. Create `.gitignore`

Write this EXACT content to `{path}/.gitignore`:

```
# .NET
bin/
obj/
*.user
.vs/

# Node
node_modules/
dist/
.vite/

# IDE
.idea/

# OS
.DS_Store

# Environment
.env
.env.local

# SQLite
*.db
*.db-journal
```

### 3. Create placeholder `CLAUDE.md`

Derive `{project_name}` from the directory name (e.g., `/tmp/my-app` → `my-app`).

Write this EXACT content to `{path}/CLAUDE.md` (substitute `{project_name}`):

````markdown
# CLAUDE.md — {project_name}

## Project
{to be populated after Phase 0}

## Tech Stack
- Backend: .NET 9, ASP.NET Core, EF Core 9, SQLite
- Frontend: React 19, Vite, TypeScript, Tailwind CSS v4, Zustand

## Implementation Status
<!-- Updated by god-agent after each feature completion -->

*No features implemented yet.*

## Commands

### Backend
```bash
cd backend && dotnet restore && dotnet build && dotnet run
```

### Frontend
```bash
cd frontend && npm install && npm run dev
```

### Tests
```bash
cd backend && dotnet test
cd frontend && npm test
```
````

### 4. Create minimal backend with health endpoint

Run from within the project directory:

```bash
cd {path}/backend && dotnet new web -n Api
```

Then **overwrite** `{path}/backend/Api/Program.cs` with this EXACT content:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();
app.UseCors();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
```

This provides:
- `/health` endpoint for E2E test startup detection
- Permissive CORS for dev (Phase 3 will configure properly)
- Minimal foundation for Phase 3 backend implementation

### 5. Git init + initial commit

```bash
cd {path} && git init && git add . && git commit -m "chore: initial scaffold"
```

## Self-Verify Gate Checklist

After completing all steps, verify EVERY item. If ANY item fails, fix it before reporting success.

- [ ] Directory `backend/` exists
- [ ] Directory `frontend/` exists
- [ ] Directory `_docs/` exists
- [ ] File `CLAUDE.md` exists at project root
- [ ] File `.gitignore` exists at project root
- [ ] File `backend/Api/Program.cs` exists AND contains `/health` endpoint
- [ ] `.git/` exists (git repo initialized)
- [ ] At least one commit exists (`git log --oneline -1` succeeds)

**Verification commands:**
```bash
cd {path}
test -d backend && echo "PASS: backend/" || echo "FAIL: backend/"
test -d frontend && echo "PASS: frontend/" || echo "FAIL: frontend/"
test -d _docs && echo "PASS: _docs/" || echo "FAIL: _docs/"
test -f CLAUDE.md && echo "PASS: CLAUDE.md" || echo "FAIL: CLAUDE.md"
test -f .gitignore && echo "PASS: .gitignore" || echo "FAIL: .gitignore"
grep -q "/health" backend/Api/Program.cs && echo "PASS: /health endpoint" || echo "FAIL: /health endpoint"
test -d .git && echo "PASS: .git/" || echo "FAIL: .git/"
git log --oneline -1 && echo "PASS: commit exists" || echo "FAIL: no commits"
```

Report: "Scaffold complete at {path}. All 8 gate items verified."
