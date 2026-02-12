## Response Style

**MANDATORY:** When reporting information, be extremely concise. Sacrifice grammar for comprehension. Full answers, no filler.

**Feedback:** Direct and specific. No hedging or gentle suggestions. Use bullet points. Concrete examples > vague advice.

## Planning Protocol

**For non-trivial tasks** (multi-file, architectural, new features):
1. Discuss strategy before writing code
2. Plan high-level (goals, flow) → then task-level (specific files)
3. Get approval before implementing

Skip planning for: typos, single-file fixes, obvious changes.

## Preferences

- **Frontend:** shadcn/ui + Tailwind v4 (utility-first, customizable)
- **Backend:** .NET Core + PostgreSQL
- When working with libraries, check the docs with Ref.

## Search Preferences

- Specific library/API docs (function signatures, config, usage): use Ref
- Technical research, best practices, "how should I": use `perplexity_ask`
- Deep multi-source investigation: use `perplexity_research`
- Architecture/tradeoff reasoning: use `perplexity_reason`
- Simple factual lookups (dates, URLs, versions): use WebSearch (free)
- Never use `perplexity_research` for simple questions

## Question Format

**MANDATORY:** When asking clarifying questions, use the AskUserQuestion tool with interactive multiple choice prompts. Only use plain text questions when genuinely requiring free-text input.

**Spec Shaping:** When running `/shape-spec`, ALWAYS present the spec-shaper's clarifying questions as multiple-choice prompts via AskUserQuestion.

## Git Push Behavior

**MANDATORY:** When asked to push, just push. Do NOT create a PR unless explicitly asked. This applies especially to pushes to master/main.

## CRITICAL: Windows Path Separator Bug

**Root Cause:** Claude's file tracking treats `/` and `\` as different paths. Reading a file with one format (e.g., `src\app.ts`) then writing with another (e.g., `src/app.ts`) causes false "File has been unexpectedly modified" errors.

**Primary Fix - Use Consistent Backslashes:**
```
✅ src\services\file.ts
❌ src/services/file.ts
```

**Rule:** On Windows, ALWAYS use backslashes (`\`) in file paths for Read, Edit, and Write tools.

**Fallback:** If errors persist after ensuring consistent path separators, use Bash:
```bash
# Small edits - use sed (forward slashes OK in Bash)
sed -i 's/old_text/new_text/g' path/to/file

# Multi-line edits - use heredoc
cat > path/to/file << 'MYEOF'
file contents here
MYEOF
```

See: https://github.com/anthropics/claude-code/issues/13824
