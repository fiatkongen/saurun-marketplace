#!/usr/bin/env bash
set -euo pipefail

# smoke-test.sh — End-to-end scaffold smoke test
# Creates a temp project via `claude -p`, then runs verify.sh
# Exit 0 = pass, Exit 1 = fail, Exit 2 = skip (missing prereqs)

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
VERIFY_SCRIPT="$SCRIPT_DIR/verify.sh"

# --- Cleanup trap ---

TMPDIR=""
cleanup() {
  if [[ -n "$TMPDIR" && -d "$TMPDIR" ]]; then
    echo "Cleaning up $TMPDIR ..."
    rm -rf "$TMPDIR"
  fi
}
trap cleanup EXIT

# --- Pre-flight checks ---

echo "=== Pre-flight checks ==="

if ! dotnet --version 2>/dev/null | grep -q "^10\."; then
  echo "SKIP: .NET 10 SDK required (got: $(dotnet --version 2>/dev/null || echo 'not installed'))"
  exit 2
fi
echo "OK: dotnet $(dotnet --version)"

NODE_MAJOR=$(node --version 2>/dev/null | sed 's/v\([0-9]*\).*/\1/' || echo "0")
if [[ "$NODE_MAJOR" -lt 22 ]]; then
  echo "SKIP: Node 22+ required (got: $(node --version 2>/dev/null || echo 'not installed'))"
  exit 2
fi
echo "OK: node $(node --version)"

if ! command -v claude &>/dev/null; then
  echo "SKIP: claude CLI not found"
  exit 2
fi
echo "OK: claude CLI found"

if [[ ! -x "$VERIFY_SCRIPT" ]]; then
  echo "SKIP: verify.sh not found or not executable at $VERIFY_SCRIPT"
  exit 2
fi
echo "OK: verify.sh found"

# --- Setup temp dir + dummy spec ---

TMPDIR="$(mktemp -d)"
PROJECT_DIR="$TMPDIR/smoke-test-project"
SPEC_FILE="$TMPDIR/spec.md"

cat > "$SPEC_FILE" << 'EOF'
# Smoke Test App

A minimal test application for verifying the scaffold skill.

## Features
- Display a welcome page
- Health check endpoint
EOF

echo ""
echo "=== Running scaffold ==="
echo "Project dir: $PROJECT_DIR"
echo "Spec file:   $SPEC_FILE"
echo ""

# --- Invoke scaffold via claude -p ---

PROMPT="Run /scaffold $PROJECT_DIR $SPEC_FILE

IMPORTANT: Skip steps 11-13 (GitHub repo creation, Railway deployment, and remote verification). Also skip pre-flight checks 3-5 (GitHub CLI, repo name availability, Railway CLI) since we are only testing local scaffold. Only run steps 0-10 and the local verification checks from step 13."

claude -p "$PROMPT" \
  --permission-mode dontAsk \
  --allowedTools "Bash Write Read Glob Grep Skill"

CLAUDE_EXIT=$?
if [[ $CLAUDE_EXIT -ne 0 ]]; then
  echo ""
  echo "FAIL: claude exited with code $CLAUDE_EXIT"
  exit 1
fi

# --- Run verify.sh ---

echo ""
echo "=== Running verification ==="
echo ""

"$VERIFY_SCRIPT" "$PROJECT_DIR"
