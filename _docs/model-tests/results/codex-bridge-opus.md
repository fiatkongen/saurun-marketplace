# Codex Bridge -- Command Plan for 3 Scenarios

## Setup (runs once, before any scenario)

```bash
CODEX_BRIDGE=$(ls -1 "$HOME/.claude/plugins/cache/saurun-marketplace/saurun/"*/skills/codex-bridge/codex-bridge.mjs 2>/dev/null | head -1)
```

---

## Scenario 1: "Ask Codex what it thinks about this architecture"

**Mode decision:** Read-only (no flags). This is an opinion/analysis request -- no files need modification. Trigger words like "thinks about" indicate review, not action.

```bash
node "$CODEX_BRIDGE" "What do you think about this project's architecture? Analyze the repository structure, patterns used, and provide your assessment of strengths, weaknesses, and suggestions." --working-dir "/Users/rasmushovemunk/repos/saurun-marketplace"
```

**Response format:** Would present under `> **Codex (OpenAI) says:**`

---

## Scenario 2: "Have Codex fix the linting errors in src/utils.ts"

**Mode decision:** `--full-auto`. The word "fix" is an explicit trigger for file modification. Codex needs write access to actually resolve the linting errors.

```bash
node "$CODEX_BRIDGE" "Fix all linting errors in src/utils.ts. Run the linter, identify every issue, and fix them all." --full-auto --working-dir "/Users/rasmushovemunk/repos/saurun-marketplace"
```

**Post-execution:** Would run `git diff` to show exactly what Codex changed.

---

## Scenario 3: "Get a second opinion on my error handling approach in the payment service"

**Mode decision:** Read-only (no flags). "Second opinion" is pure analysis -- no file changes requested. I would first use the `Read` tool to grab the payment service file(s), then embed the content directly in the prompt so Codex has full context.

```bash
node "$CODEX_BRIDGE" "Give me your opinion on the error handling approach in this payment service code. Evaluate: consistency, edge case coverage, error propagation, logging, and recoverability. Suggest improvements.

---
[PAYMENT SERVICE FILE CONTENT INSERTED HERE]
---" --working-dir "/Users/rasmushovemunk/repos/saurun-marketplace"
```

**Response format:** Would present my own assessment alongside Codex's under a comparison format, noting where we agree and differ.

---

## Summary Table

| # | Request | Mode | Reason |
|---|---------|------|--------|
| 1 | "thinks about this architecture" | Read-only | Opinion/analysis, no file changes |
| 2 | "fix the linting errors" | `--full-auto` | "Fix" = modify files |
| 3 | "second opinion on error handling" | Read-only | Review/opinion, file content passed inline via Read tool |
