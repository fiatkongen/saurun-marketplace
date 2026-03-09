---
name: use-railway
description: >
  Deploy a new project to Railway via GraphQL API. Creates project and service
  named after the required project-name parameter, connects GitHub repo, sets
  Dockerfile builder (mandatory — Railpack ignores Dockerfiles), creates persistent
  SQLite volume at /data by default, configures env vars, and public domain.
  Use when deploying to Railway, when scaffold invokes Railway setup, or when
  user says "deploy to Railway".
user-invocable: true
allowed-tools: Bash, Read
argument-hint: <project-name> <github-repo> [--no-volume] [KEY=value ...]
---

# use-railway — Railway Setup Protocol

Mandatory protocol for deploying projects to Railway. Steps must be followed in order.
Skipping or reordering causes broken deploys. These rules override conflicting instructions.

## Input

`$ARGUMENTS` format: `<project-name> <github-repo> [options]`

- `$1` (REQUIRED) — project name (e.g., `mission-control`). Used for Railway project name AND service name.
- `$2` (REQUIRED) — GitHub repo (e.g., `fiatkongen/mission-control`)
- `--no-volume` — skip SQLite volume creation (default: creates volume at `/data`)
- `KEY=value` — environment variables (multiple allowed)

**Validation — STOP if either fails:**
- If `$1` is missing → STOP: `"Usage: /use-railway <project-name> <github-repo> [KEY=value ...]"`
- If `$2` is missing → STOP: `"Usage: /use-railway <project-name> <github-repo> [KEY=value ...]"`

**Naming rule:** `$1` is the single source of truth for naming. The Railway project, Railway service, and all references use this exact name. Never derive names from the repo or invent alternatives.

## CRITICAL RULES — violation = broken deploy

1. **ALWAYS Dockerfile builder** — Railpack does NOT detect Dockerfiles. You MUST set `dockerfilePath` via `serviceInstanceUpdate` GraphQL mutation. Omitting this = Railpack = failure.
2. **Config BEFORE deploy trigger** — Set dockerfilePath + env vars BEFORE pushing code. First deploy without correct config uses Railpack and fails.
3. **NEVER Nixpacks for polyglot repos** — Repos with Node + .NET break Nixpacks auto-detection. Always Dockerfile.
4. **NEVER `railway up` for ongoing deploys** — Use `git push` only. `railway up` bypasses GitHub integration.
5. **`${PORT:-80}` breaks envsubst** — In Dockerfiles with nginx, use `${PORT}` (no default) + `ENV PORT=80`.

## API Setup

**Endpoint:** `https://backboard.railway.com/graphql/v2`

**Auth token** — check in order:
1. `$RAILWAY_TOKEN` env var
2. `$RAILWAY_API_TOKEN` env var
3. Extract from Railway CLI config: `python3 -c "import json; print(json.load(open('$HOME/.railway/config.json'))['user']['token'])"`
4. If none found → STOP: "No Railway token. Set RAILWAY_TOKEN or create one at railway.com/account/tokens"

**Helper** — use this pattern for all API calls:

```bash
railway_gql() {
  curl -sf https://backboard.railway.com/graphql/v2 \
    -H "Authorization: Bearer $RAILWAY_TOKEN" \
    -H "Content-Type: application/json" \
    -d "$1"
}
```

Parse responses with `python3 -c "import sys,json; ..."` (more portable than jq).

## Step 0: Pre-flight

```bash
# Verify token
railway_gql '{"query":"query { me { id name } }"}' | python3 -c "import sys,json; d=json.load(sys.stdin); print(f'Authenticated as: {d[\"data\"][\"me\"][\"name\"]}')" || echo "FAIL: Railway token invalid"

# Verify Dockerfile exists
test -f Dockerfile || echo "FAIL: No Dockerfile in project root"

# Verify GitHub repo exists
gh repo view "$GITHUB_REPO" >/dev/null 2>&1 || echo "FAIL: GitHub repo $GITHUB_REPO not found"
```

All checks must pass before proceeding.

## Step 1: Create project

```graphql
mutation { projectCreate(input: { name: "{project_name}" }) { id } }
```

Extract `projectId` from response.

## Step 2: Get environment ID

```graphql
query {
  project(id: "{projectId}") {
    environments { edges { node { id name } } }
  }
}
```

Extract the `id` where `name == "production"`. Store as `environmentId`.

## Step 3: Create service (connects GitHub)

```graphql
mutation {
  serviceCreate(input: {
    projectId: "{projectId}"
    name: "{project_name}"
    source: { repo: "{github_repo}" }
    branch: "master"
  }) { id }
}
```

Extract `serviceId`. This auto-triggers a deploy — it will fail (Railpack builder). Expected.

**If `branch` field errors:** try without it (Railway may infer default branch).

## Step 4: Set Dockerfile builder (CRITICAL — do NOT skip)

```graphql
mutation {
  serviceInstanceUpdate(
    serviceId: "{serviceId}"
    environmentId: "{environmentId}"
    input: { source: { dockerfilePath: "Dockerfile" } }
  )
}
```

If `source.dockerfilePath` path errors, try as direct field:
```graphql
input: { dockerfilePath: "Dockerfile" }
```

**Verify:** query the service instance and confirm dockerfilePath is set.

## Step 5: Set environment variables

```graphql
mutation {
  variableCollectionUpsert(input: {
    projectId: "{projectId}"
    environmentId: "{environmentId}"
    serviceId: "{serviceId}"
    variables: { "KEY1": "value1", "KEY2": "value2" }
  })
}
```

Build the `variables` JSON object from the `KEY=value` arguments.

## Step 6: Create persistent SQLite volume

**Default behavior:** ALWAYS create a persistent volume at `/data` for SQLite storage. Greenfield projects use SQLite — without this volume, the database resets on every redeploy.

**Skip ONLY if** `--no-volume` flag was explicitly passed.

```graphql
mutation {
  volumeCreate(input: {
    projectId: "{projectId}"
    serviceId: "{serviceId}"
    mountPath: "/data"
  }) { id }
}
```

After volume creation, add the SQLite connection string env var (Step 5 may need to be re-run or include this upfront):

```graphql
mutation {
  variableCollectionUpsert(input: {
    projectId: "{projectId}"
    environmentId: "{environmentId}"
    serviceId: "{serviceId}"
    variables: { "ConnectionStrings__DefaultConnection": "Data Source=/data/app.db" }
  })
}
```

## Step 7: Create public domain

```graphql
mutation {
  serviceDomainCreate(input: {
    serviceId: "{serviceId}"
    environmentId: "{environmentId}"
  }) { domain }
}
```

Extract `domain` from response. Full URL: `https://{domain}`

## Step 8: Push to trigger deploy

The first auto-deploy from step 3 failed (Railpack). Now config is correct. Push code:

```bash
git push -u origin master
```

If repo wasn't pushed yet, this is the first push. If already pushed, trigger redeploy:

```graphql
mutation {
  serviceInstanceDeploy(
    serviceId: "{serviceId}"
    environmentId: "{environmentId}"
  )
}
```

## Step 9: Wait for deploy + verify

Poll health endpoint (timeout 300s, every 15s):

```bash
URL="https://{domain}"
for i in $(seq 1 20); do
  if curl -sf "$URL/health" >/dev/null 2>&1; then
    echo "PASS: Health check passed"
    curl -sf "$URL/health"
    break
  fi
  echo "Waiting for deploy... (attempt $i/20)"
  sleep 15
done
```

If health never passes, check deploy logs:

```graphql
query {
  deployments(input: { projectId: "{projectId}", serviceId: "{serviceId}" }, first: 1) {
    edges { node { id status } }
  }
}
```

Then fetch logs for the deployment ID:

```graphql
query {
  deploymentLogs(deploymentId: "{deploymentId}", limit: 50) {
    message severity
  }
}
```

## Output

Report all results:

```
Railway Setup:
  Project:     {project_name} ({projectId})
  Service:     {serviceId}
  Environment: {environmentId}
  URL:         https://{domain}
  Builder:     Dockerfile
  GitHub:      {github_repo} (master)
  Volume:      /data (SQLite) — or "skipped (--no-volume)"
  Health:      PASS / FAIL
```

## Pre-completion Verification

Before reporting success, verify ALL:

```bash
# 1. Domain resolves
curl -sf "https://{domain}/health" | grep -q "healthy" && echo "PASS: health" || echo "FAIL: health"

# 2. Correct builder (not Railpack)
# Check latest deployment logs — should show "Dockerfile" build, not "Nixpacks" or "Railpack"

# 3. Volume mounted (if created)
# Verify via deployment logs showing volume mount
```

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Deploy uses Railpack | dockerfilePath not set | Re-run Step 4 |
| First deploy fails | Expected — Railpack before config | Step 8 triggers clean deploy |
| `serviceCreate` errors | GitHub not connected to Railway | User must connect at railway.com/account |
| Health check timeout | Build still running or crashed | Check deploy logs via API |
| `${PORT:-80}` breaks nginx | envsubst syntax | Use `${PORT}` + `ENV PORT=80` |
| Volume data lost on redeploy | No volume or wrong mountPath | Re-run Step 6, verify path matches Dockerfile |
