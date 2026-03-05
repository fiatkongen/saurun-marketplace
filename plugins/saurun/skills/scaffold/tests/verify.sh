#!/usr/bin/env bash
set -euo pipefail

# verify.sh — Deterministic assertions for scaffolded projects
# Usage: ./verify.sh <project-path>
# Exit 0 = all pass, Exit 1 = any FAIL

if [[ $# -lt 1 ]]; then
  echo "Usage: $0 <project-path>"
  exit 1
fi

PROJECT="$1"
if [[ ! -d "$PROJECT" ]]; then
  echo "FAIL: Directory does not exist: $PROJECT"
  exit 1
fi

PASS=0
FAIL=0
WARN=0

pass() { echo "PASS: $1"; ((PASS++)); }
fail() { echo "FAIL: $1"; ((FAIL++)); }
warn() { echo "WARN: $1"; ((WARN++)); }

# --- File existence (14 checks) ---

check_file() {
  if [[ -f "$PROJECT/$1" ]]; then
    pass "$1 exists"
  else
    fail "$1 missing"
  fi
}

check_file "backend/Api/Api.csproj"
check_file "backend/Api/Program.cs"
check_file "frontend/package.json"
check_file "frontend/vite.config.ts"
check_file "frontend/tsconfig.app.json"
check_file "frontend/src/index.css"
check_file "frontend/src/App.tsx"
check_file "Dockerfile"
check_file ".dockerignore"
check_file ".gitignore"
check_file "CLAUDE.md"
check_file "_docs/spec.md"
check_file "_docs/design.md"
check_file "_docs/deploy.md"

# --- Content checks (8 checks) ---

check_contains() {
  local file="$1" pattern="$2" label="$3"
  if [[ -f "$PROJECT/$file" ]] && grep -q "$pattern" "$PROJECT/$file"; then
    pass "$label"
  else
    fail "$label"
  fi
}

check_contains "backend/Api/Api.csproj" "net10.0" "Api.csproj targets net10.0"
check_contains "backend/Api/Program.cs" "/health" "Program.cs has /health endpoint"
check_contains "frontend/package.json" '"react"' "package.json has react"
check_contains "frontend/package.json" "tailwindcss" "package.json has tailwindcss"
check_contains "frontend/vite.config.ts" "proxy" "vite.config.ts has proxy"
check_contains "frontend/tsconfig.app.json" "@/" "tsconfig.app.json has @/ alias"
check_contains "frontend/src/index.css" "tailwindcss" "index.css imports tailwindcss"
check_contains "Dockerfile" "FROM.*AS" "Dockerfile has multi-stage build"

# --- Design system check (1 check) ---

check_contains "_docs/design.md" "\-\-background" "design.md has CSS custom properties"

# --- Boilerplate removal (2 checks) ---

if [[ ! -f "$PROJECT/frontend/src/App.css" ]]; then
  pass "App.css removed (boilerplate cleanup)"
else
  fail "App.css still exists (boilerplate not cleaned)"
fi

if [[ ! -f "$PROJECT/frontend/public/vite.svg" ]]; then
  pass "vite.svg removed (boilerplate cleanup)"
else
  fail "vite.svg still exists (boilerplate not cleaned)"
fi

# --- Git checks (2 checks) ---

if [[ -d "$PROJECT/.git" ]]; then
  pass "git initialized"
else
  fail "git not initialized"
fi

if git -C "$PROJECT" log --oneline -1 &>/dev/null; then
  pass "at least 1 commit"
else
  fail "no git commits"
fi

# --- shadcn (1 warn-only check) ---

if [[ -f "$PROJECT/frontend/components.json" ]]; then
  pass "shadcn initialized"
else
  warn "shadcn not initialized (components.json missing)"
fi

# --- Build checks (2 checks) ---

echo ""
echo "--- Build checks ---"

if (cd "$PROJECT/backend/Api" && dotnet build --verbosity quiet 2>&1); then
  pass "backend builds"
else
  fail "backend build failed"
fi

if (cd "$PROJECT/frontend" && npm run build 2>&1); then
  pass "frontend builds"
else
  fail "frontend build failed"
fi

# --- Summary ---

echo ""
echo "=============================="
echo "Results: $PASS passed, $FAIL failed, $WARN warnings"
echo "=============================="

if [[ $FAIL -gt 0 ]]; then
  exit 1
fi
exit 0
