# Neo Scaffold Skill — Design

## Oversigt

Når Neo modtager et greenfield build, scaffolder han et komplet, deploy-klart projekt med ét skill-kald. Zero manual steps.

**Input:** Sti + spec (fil eller inline tekst)
```
/scaffold ~/repos/my-project ~/specs/my-project-spec.md
```

**Output:** Et fungerende, deployet projekt med live URL.

## Beslutninger

| # | Beslutning |
|---|-----------|
| 1 | Neo scaffolder (step 1 af greenfield) |
| 2 | Fuld frontend (Vite + Tailwind + shadcn) |
| 3 | Minimal CLAUDE.md med fil-referencer |
| 4 | Code style lever i subagent-prompts, ikke i repo |
| 5 | GitHub private repo auto-oprettet (private) |
| 6 | Railway fuld auto (projekt + connect + deploy) |
| 7 | Same-origin monolith (frontend → wwwroot) |
| 8 | Standard multi-stage Dockerfile |
| 9 | .NET 10 |
| 10 | Default design.md (skandinavisk/Linear/dark+light) |
| 11 | Spec kan override design.md |
| 12 | 18-point verify gate |

## Pre-flight Checks (Step 0)

**Kør ALLE checks FØR noget oprettes. Hvis nogen fejler → STOP med descriptiv fejlbesked.**

```bash
# .NET 10
dotnet --version | grep -q "^10\." || echo "FAIL: .NET 10 SDK required"

# Node 22+
node --version | grep -qE "^v2[2-9]\." || echo "FAIL: Node 22+ required"

# GitHub CLI authenticated
gh auth status || echo "FAIL: gh not authenticated"

# Repo-navn ledigt
gh repo view fiatkongen/{project_name} 2>&1 | grep -q "Not Found" || echo "FAIL: repo fiatkongen/{project_name} already exists"

# Railway CLI + authenticated
railway whoami --json || echo "FAIL: Railway not authenticated"

# Docker available
command -v docker || echo "FAIL: Docker not installed"
```

**Alle 6 pre-flight checks skal passere.**

## Fejlstrategi

### Partial success er OK
Scaffold kan lykkes delvist. Prioritetsorden:

1. **Lokal scaffold** (steps 1-11) — SKAL lykkes
2. **GitHub repo** (step 12) — bør lykkes, men lokal scaffold er stadig værdifuld uden
3. **Railway deploy** (step 13) — nice to have, kan sættes op manuelt bagefter

### Fejlhåndtering per step
- **Pre-flight fejl →** STOP. Fix problemet først.
- **npm install fejler →** `rm -rf node_modules package-lock.json && npm install` (retry 1x)
- **shadcn init fejler →** Skip shadcn, rapportér "shadcn init failed — manual setup needed"
- **gh repo create fejler →** Rapportér fejl, fortsæt til verify gate (lokal success)
- **Railway fejler →** Rapportér fejl, lokal + GitHub er stadig OK
- **Docker build fejler →** Debug, fix Dockerfile, retry 1x
- **Health check timeout →** Tjek Railway build logs, rapportér specifik fejl

### Rapport ved partial success
```
Scaffold PARTIAL at {path}.
✅ Local: 11/11 steps complete
✅ GitHub: fiatkongen/{name} created
❌ Railway: [specifik fejl]
Action needed: [hvad Rasmus skal gøre manuelt]
```

## Steps

### Step 1: Validér input
- `$1` = projekt-sti (required)
- `$2` = spec fil eller inline tekst (required)
- Hvis mappen eksisterer OG har `CLAUDE.md` → STOP
- Expand `~`, resolve relative paths
- Quote alle paths (håndtér spaces)

### Step 2: Opret mappestruktur
```bash
mkdir -p "{path}/backend" "{path}/frontend" "{path}/_docs"
```

### Step 3: Gem spec + udtræk mission
- Hvis `$2` er fil → kopiér til `_docs/spec.md`
- Hvis `$2` er tekst → skriv til `_docs/spec.md`
- Udtræk 1-3 sætningers mission fra spec

### Step 4: Design template
- Tjek om spec indeholder en `design.md` sektion eller refererer til en design-fil
- **Hvis ja →** brug den, gem som `_docs/design.md`
- **Hvis nej →** kopiér default design template til `_docs/design.md`

### Step 5: Skriv CLAUDE.md
```markdown
# CLAUDE.md — {project_name}
{mission}

## Spec
Se `_docs/spec.md` for fuld spec.

## Design
Se `_docs/design.md` for UI design system.

## Deploy
Se `_docs/deploy.md` for Railway-regler og Dockerfile.

## Commands
cd backend/Api && dotnet build && dotnet run
cd frontend && npm install && npm run dev

## Implementation Status
*No features implemented yet.*
```

### Step 6: Skriv deploy.md
```markdown
# Deploy — {project_name}

## Railway
- Deploy sker automatisk ved git push til master
- ALDRIG brug `railway up` — altid git push
- ALTID Dockerfile — aldrig Railpack/Nixpacks

## Dockerfile
Multi-stage build (se `Dockerfile` i root):
1. Stage 1: Build frontend (Node 22)
2. Stage 2: Build backend + copy frontend dist til wwwroot (.NET 10)
3. Stage 3: Runtime (aspnet:10.0)

## Env vars
- `PORT` — Railway sætter automatisk
- `ASPNETCORE_URLS` — sættes i ENTRYPOINT, ikke ENV
- `ConnectionStrings__DefaultConnection` — `Data Source=/data/app.db`

## SQLite Volume
- Persistent volume mounted på `/data`
- ALDRIG gem .db i projekt-mappen — den forsvinder ved redeploy
- Volume oprettes af scaffold (Railway API)

## Same-origin
Frontend serves fra backend's wwwroot. Ingen separat frontend-service.
```

### Step 7: Skriv .gitignore + .dockerignore
**.gitignore:**
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

**.dockerignore:**
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

### Step 8: Backend scaffold
```bash
cd "{path}/backend" && dotnet new web -n Api
```
Overskriv `Program.cs`:
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
app.UseStaticFiles();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// SPA fallback — serve index.html for client-side routing
app.MapFallbackToFile("index.html");

app.Run();
```

### Step 9: Frontend scaffold
```bash
cd "{path}/frontend"
npm create vite@latest . -- --template react-ts --yes
npm install
npm install -D tailwindcss @tailwindcss/vite
npm install lucide-react
```

**Konfigurér Tailwind v4** i `vite.config.ts`:
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
})
```

**Tilføj path alias** i `tsconfig.app.json`:
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

**Skriv `src/index.css`** (erstatter Vite default):
```css
@import "tailwindcss";
```

**Skriv minimal `src/App.tsx`** (erstatter Vite default):
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

**Installér shadcn/ui:**
```bash
npx shadcn@latest init --yes --defaults
npx shadcn@latest add button card input label
```

Hvis shadcn init fejler → skip, rapportér i output.

### Step 10: Skriv Dockerfile
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
COPY backend/Api/ .
COPY --from=frontend /app/dist ./wwwroot
RUN dotnet publish -c Release -o /app

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=backend /app .
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "dotnet Api.dll --urls http://+:${PORT:-8080}"]
```

**NB:** Port håndteres via ENTRYPOINT med shell form, så `$PORT` env var fra Railway evalueres korrekt. Ingen hardcoded `ENV ASPNETCORE_URLS`.

### Step 11: Git init + commit
```bash
cd "{path}"
git init
git add .
git commit -m "chore: initial scaffold"
```

### Step 12: GitHub repo (UDEN push)
```bash
cd "{path}"
gh repo create fiatkongen/{project_name} --private --source .
```

**Push sker IKKE endnu** — Railway skal konfigureres først (step 13) for at undgå at første auto-deploy kører uden env vars og volume.

### Step 13: Railway setup + push
Brug `use-railway` skill:
1. Opret nyt Railway projekt
2. Opret service connected til `fiatkongen/{project_name}` repo
3. Sæt `dockerfilePath` til `Dockerfile`
4. Opret persistent volume mounted på `/data` (til SQLite)
5. Sæt env vars:
   - `ConnectionStrings__DefaultConnection=Data Source=/data/app.db`
   - Andre projekt-specifikke env vars fra spec
6. **Nu push:** `git push -u origin master`
7. Vent på første deploy (poll build status, timeout 300s)
8. Hent public URL

Hvis Railway fejler → rapportér specifik fejl, push alligevel (GitHub repo er stadig værdifuldt).

### Step 14: Self-Verify Gate

**Pre-flight (allerede passed):** 6 checks

**Lokal verifikation (10 checks):**
- [ ] `backend/Api/Api.csproj` eksisterer + target `net10.0`
- [ ] `backend/Api/Program.cs` indeholder `/health`
- [ ] `frontend/package.json` eksisterer + har `react`, `tailwindcss`
- [ ] `frontend/components.json` eksisterer (shadcn initialiseret)
- [ ] `Dockerfile` eksisterer i root
- [ ] `.dockerignore` eksisterer i root
- [ ] `CLAUDE.md` eksisterer + indeholder mission (ikke placeholder)
- [ ] `_docs/spec.md` eksisterer + indeholder fuld spec
- [ ] `_docs/design.md` eksisterer
- [ ] `_docs/deploy.md` eksisterer

**Build verifikation (2 checks):**
- [ ] `cd backend/Api && dotnet build` → exit 0
- [ ] `cd frontend && npm run build` → exit 0

*Docker build skippes — stol på Railway's build i stedet.*

**Remote verifikation (4 checks):**
- [ ] `gh repo view fiatkongen/{name}` → exit 0
- [ ] Railway projekt connected til repo
- [ ] Railway persistent volume mounted på `/data`
- [ ] `curl https://{railway-url}/health` → HTTP 200 + `{"status":"healthy"}` (timeout 300s)

**16 checks total (lokal) + 4 remote = 20 total.**

Ved partial success rapportér hvilke der passede/fejlede.

Rapport: "Scaffold complete at {path}. 20/20 gates passed. Live at https://{url}"
