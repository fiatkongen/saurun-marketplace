---
name: design-phase
description: >
  Use when needing to plan and design a product before writing implementation
  code. Sets up a Design OS workspace as a _design/ subdirectory.
user-invocable: true
allowed-tools: Bash, Write, Read, Glob
argument-hint: ~/repos/my-app
---

# Design Phase: Design OS Workspace

Sets up a [Design OS](https://github.com/buildermethods/design-os) workspace inside your project at `{path}/_design/`. Design first, then implement.

**When to use:** Any project (greenfield or existing) where you want to plan vision, data model, and UI before implementation.

## Input

`$ARGUMENTS` = absolute or relative path to your project root (e.g., `~/repos/my-app`).

**Validation:** If `$ARGUMENTS` is empty or missing, STOP with error: "Usage: /design-phase <project-path>"

Expand `~` to the user's home directory. Resolve relative paths against the current working directory.

If `{path}/_design/package.json` already exists, STOP: "Design workspace already exists at {path}/_design/. Run `cd {path}/_design && npm run dev` to resume."

## Steps

### 1. Clone Design OS

```bash
git clone https://github.com/buildermethods/design-os.git {path}/_design
```

If clone fails, verify network connectivity and that the repo exists at https://github.com/buildermethods/design-os.

### 2. Remove nested git

```bash
rm -rf {path}/_design/.git
```

Makes `_design/` regular files — part of your project's git repo, not a nested repo.

### 3. Install dependencies

```bash
cd {path}/_design && npm install
```

### 4. Start dev server

```bash
cd {path}/_design && npm run dev &
DEV_PID=$!
```

Run in background. The dev server listens on **port 3000**.

Wait for the server to be ready before verifying:
```bash
for i in $(seq 1 10); do curl -s -o /dev/null http://localhost:3000 && break || sleep 1; done
```

**If port 3000 is already in use:** kill the conflicting process or change the port in `{path}/_design/vite.config.ts`.

**To stop the server later:** `kill $DEV_PID` or `lsof -ti:3000 | xargs kill`.

### 5. Report ready

After the dev server starts, report:

> Design workspace ready at {path}/_design. Open http://localhost:3000. Start with `/product-vision`.

Then print the workflow quick-reference below.

## Self-Verify Gate

After completing all steps, verify EVERY item. If ANY fails, fix before reporting success.

- [ ] Directory `{path}/_design` exists
- [ ] `{path}/_design/.git` does NOT exist (nested git removed)
- [ ] `{path}/_design/node_modules` exists (npm install succeeded)
- [ ] Dev server responding on port 3000

**Verification commands:**
```bash
test -d {path}/_design && echo "PASS: _design dir" || echo "FAIL: _design dir"
test ! -d {path}/_design/.git && echo "PASS: no nested .git" || echo "FAIL: nested .git still exists"
test -d {path}/_design/node_modules && echo "PASS: node_modules" || echo "FAIL: node_modules"
curl -s -o /dev/null -w "%{http_code}" http://localhost:3000 | grep -q "200" && echo "PASS: dev server" || echo "FAIL: dev server"
```

All 4 gates must pass before reporting success.

## Cleanup

When you're done designing, remove the workspace:
```bash
rm -rf {path}/_design
```

## Design OS Workflow Quick-Reference

Print this after successful setup so the user knows the full design flow.

**These commands are provided by Design OS** (in `_design/.claude/commands/`), not saurun skills. Run them from within the design workspace.

### Phase 1 — Product Planning

| Command | Purpose |
|---------|---------|
| `/product-vision` | Define what you're building, split into sections, shape your data |
| `/design-tokens` | Choose colors and typography |
| `/design-shell` | Design navigation and layout |

### Phase 2 — Section Design (repeat per section)

| Command | Purpose |
|---------|---------|
| `/shape-section` | Define scope and requirements for one section |
| `/sample-data` | Generate realistic sample data + TypeScript types |
| `/design-screen` | Build the actual React components |

### Phase 3 — Export

| Command | Purpose |
|---------|---------|
| `/export-product` | Generate complete handoff package |

**After export:** `/export-product` generates a handoff directory with React components, TypeScript types, and design specs. These live inside `_design/` and can be referenced or copied into your implementation code. When done, `rm -rf _design/`.
