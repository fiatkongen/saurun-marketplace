# Research Protocol

Covers Step 4 (Research Decision) and Step 5 (Execute Research).

## Step 4: Research Decision

Extract research topics from user input:

1. Read the input/spec
2. Identify: technologies mentioned, feature types requiring patterns, external integrations, architecture patterns implied
3. Generate 3-5 searchable topics with current year

### Topic Extraction Examples

| Input mentions | Extracted topic |
|---------------|-----------------|
| "voice commands" | "Web Speech API voice recording patterns {year}" |
| "real-time updates" | "SignalR .NET 9 real-time patterns {year}" |
| "image upload" | "ASP.NET Core image upload compression thumbnail {year}" |
| "drag and drop" | "React DnD sortable list patterns {year}" |
| "payments" | "Stripe integration ASP.NET Core {year}" |
| "maps" | "React map integration Leaflet vs Mapbox {year}" |

### Present Choices via AskUserQuestion

Two questions:

1. **Codebase research** (extension mode only): "Should I research the existing codebase for patterns and conventions?"
   - Options: Yes / No

2. **Web research topics**: multi-select from extracted list
   - Each option = one extracted topic
   - User can select any combination or "Other" for custom topics

### Decision Recording

Write `.god-agent/research-decision.md`:
```markdown
# Research Decision
- **Date:** {ISO date}
- **Codebase research:** {yes | no | n/a (greenfield)}
- **Web topics:** {list of selected topics, or "none"}
- **Skipped:** {true if user selected NO codebase AND no web topics}
```

If `skipped: true` -> skip Step 5 entirely (no `research.md` created), proceed to Step 6.

## Step 5: Execute Research

### Two Research Types

**Codebase research** (if user chose Yes):
- Dispatch `Task(subagent_type=Explore)` to analyze:
  - Existing patterns and conventions
  - CLAUDE.md contents
  - Endpoint structure
  - Component patterns
  - Database schema
- Subagent returns findings as markdown text

**Web research** (for each selected topic):
- Dispatch `Task(subagent_type=Explore)` with WebSearch + ref_search_documentation
- Search for each selected topic
- Subagent returns findings as markdown text

### Parallel Execution

If both codebase AND web research needed, launch both Tasks in a SINGLE message (two parallel tool calls).

### Critical: Subagent Data Flow

```
DO NOT have subagents write to files directly.
Subagents return their findings as markdown text.
Main context combines all results and writes a single .god-agent/research.md file.

Why: Prevents race conditions, keeps main context as single source of truth,
enables consistent formatting, and avoids partial file writes on subagent failure.
```

```
Task(subagent_type=Explore, "codebase") ---+
                                           +--> Main combines --> .god-agent/research.md
Task(subagent_type=Explore, "web")      ---+
```

### Output Format

`.god-agent/research.md`:
```markdown
# Research Findings

## Codebase Analysis
{combined codebase findings, or "Skipped" if not requested}

## Web Research
### {Topic 1}
{findings}

### {Topic 2}
{findings}

## Key Takeaways
- {3-5 bullet points summarizing most impactful findings}
```

### Failure Handling

| Scenario | Action |
|----------|--------|
| Both subagents succeed | Combine results, write `.god-agent/research.md` |
| One subagent fails | Write results from successful one, note failure in file |
| Both subagents fail | Log error, skip research, proceed to interview (Step 6) |
| User skipped all research | Skip Step 5 entirely, no `.god-agent/research.md` created |
