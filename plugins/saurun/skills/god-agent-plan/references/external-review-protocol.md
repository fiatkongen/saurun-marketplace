# External Review Protocol

Covers Step 10. Two external LLM reviewers (Gemini + Codex) review architecture and plans.

## Pre-Check (MANDATORY)

Run before launching reviews:
```bash
which gemini && which codex
```

| Scenario | Action |
|----------|--------|
| Both present | Proceed to parallel review |
| Either missing | STOP with install instructions |

### STOP Message (if CLI missing)

```
External review requires both Gemini and Codex CLIs.
Missing: {list missing CLIs}

Install:
- Gemini: https://github.com/google-gemini/gemini-cli
- Codex: https://github.com/openai/codex

Then re-invoke /god-agent-plan to resume from Step 10.
```

## Dispatch Reviews

Two reviewers -- dispatch in SINGLE message (two parallel Bash calls):

**Bash call 1 (Gemini):**
```bash
echo '{REVIEW_PROMPT}' | gemini -m gemini-3-pro-preview --approval-mode yolo
```

**Bash call 2 (Codex):**
```bash
echo '{REVIEW_PROMPT}' | codex exec -m gpt-5.3 --sandbox read-only --full-auto
```

## Reviewer Prompt

Substitute `{ARCHITECTURE_CONTENT}` and `{ALL_PLAN_CONTENTS}` before dispatch:

```
You are a senior software architect reviewing an implementation plan
for a .NET 9 + React 19 full-stack application.

Review the architecture document and all implementation plans below.
Identify:
- Missing edge cases in task behaviors
- Entity classification issues (should X be rich or anemic?)
- API contract gaps (missing endpoints, incomplete DTOs)
- Component tree completeness (missing pages, stores, hooks)
- Cross-plan dependency issues
- Security vulnerabilities
- Performance concerns
- Unclear or ambiguous requirements

Be specific and actionable. Reference specific sections/tasks.

ARCHITECTURE:
{ARCHITECTURE_CONTENT}

PLANS:
{ALL_PLAN_CONTENTS}
```

## Failure Handling

| Scenario | Action |
|----------|--------|
| Both succeed | Write both review files, proceed to integration |
| One times out (>5 min) | Kill timed-out process, use successful review only, note in output |
| One errors | Write successful review only, note failure |
| Both fail/timeout | Proceed to Step 11 without reviews, note in integration-notes.md |

## Output Files

`.god-agent/reviews/gemini-review.md`:
```markdown
# Gemini Review
- **Model:** gemini-3-pro-preview
- **Timestamp:** {ISO timestamp}

## Review
{full review content}
```

`.god-agent/reviews/codex-review.md`:
```markdown
# Codex Review
- **Model:** gpt-5.3
- **Timestamp:** {ISO timestamp}

## Review
{full review content}
```

## Integration Criteria

After reading both reviews, decide for each suggestion:

### ACCEPT if:
- Fixes a data model bug (missing FK, broken constraint, wrong aggregate boundary)
- Adds a missing contract (endpoint, DTO, entity behavior) that spec requires
- Identifies a security vulnerability
- Fixes an entity classification error (e.g., should be rich but marked anemic)
- Addresses a gap explicitly in spec requirements

### REJECT if:
- Contradicts an explicit user decision from the interview
- Over-engineers for MVP scope (e.g., suggests Postgres when SQLite was chosen)
- Adds infrastructure complexity beyond current stack choices
- Is advisory best-practice without a specific bug or gap (e.g., "consider adding caching")
- Would significantly expand scope beyond spec

## Integration Output

1. Write `.god-agent/integration-notes.md`:
   ```markdown
   # Integration Notes
   - **Date:** {ISO date}
   - **Reviewers:** Gemini ({model used}), Codex ({model used})

   ## Integrated
   1. {what was accepted} -- {one-line reasoning}
   2. ...

   ## Rejected
   1. {what was rejected} -- {one-line reasoning}
   2. ...
   ```

2. Update `_docs/specs/*-architecture.md` with accepted architectural changes
3. Update `_docs/plans/*.md` with accepted plan changes (only affected plans)
