## Setup Step (Required First)

```bash
CODEX_BRIDGE=$(ls -1 "$HOME/.claude/plugins/cache/saurun-marketplace/saurun/"*/skills/codex-bridge/codex-bridge.mjs 2>/dev/null | head -1)
```

---

## Scenario 1: "Ask Codex what it thinks about this architecture"

**Mode Decision:** Read-only (no flags) — This is an opinion/review request, not a file modification.

**Command:**
```bash
node "$CODEX_BRIDGE" "What do you think about this architecture? Please provide your opinion on the design patterns, scalability, and potential improvements." --working-dir "/Users/rasmushovemunk/repos/saurun-marketplace"
```

**Why read-only:** User is asking for Codex's perspective/opinion, not asking Codex to change anything. No trigger words like "fix" or "refactor."

---

## Scenario 2: "Have Codex fix the linting errors in src/utils.ts"

**Mode Decision:** `--full-auto` — This is a file modification request with trigger word "fix."

**Command (two-step approach):**

Step 1 - Read the file to include in prompt:
```bash
cat "/Users/rasmushovemunk/repos/saurun-marketplace/src/utils.ts"
```

Step 2 - Send to Codex with `--full-auto`:
```bash
node "$CODEX_BRIDGE" "Fix the linting errors in this file:

---
[FILE CONTENT HERE]
---" --full-auto --working-dir "/Users/rasmushovemunk/repos/saurun-marketplace"
```

**Why `--full-auto`:** Explicit "fix" trigger word + user wants Codex to modify the actual file. After completion, verify with `git diff`.

---

## Scenario 3: "Get a second opinion on my error handling approach in the payment service"

**Mode Decision:** Read-only (no flags) — This is a comparative opinion/review, not a modification.

**Command (two-step approach):**

Step 1 - Locate and read the payment service file(s):
```bash
find "/Users/rasmushovemunk/repos/saurun-marketplace" -name "*payment*" -type f | head -5
```

Step 2 - Send to Codex for review:
```bash
node "$CODEX_BRIDGE" "Please review my error handling approach in this payment service and give me your second opinion on robustness, clarity, and best practices:

---
[FILE CONTENT HERE]
---" --working-dir "/Users/rasmushovemunk/repos/saurun-marketplace"
```

**Why read-only:** User is asking for Codex's opinion/perspective, not asking it to fix or modify the code. This is comparative analysis.

---

## Summary Table

| Scenario | Mode | Trigger Word | Reasoning |
|----------|------|--------------|-----------|
| 1. Architecture opinion | Read-only | None | Opinion request |
| 2. Fix linting errors | `--full-auto` | "fix" | File modification needed |
| 3. Second opinion on error handling | Read-only | None | Review/analysis request |
