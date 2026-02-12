# Experiment Protocol: Step-by-Step Execution Guide

## Prerequisites

- This skill installed in saurun plugin
- `saurun:backend-implementer` available
- Opus model available for spec generation
- A working .NET 9 SDK for compilation checks

---

## Step 0: Create Output Directory

```
_docs/spec-format-experiment/
  specs/                    # Generated specs (30 files)
  results/                  # Scoring data
    raw-scores.md           # Per-feature scoring
    scoring-matrix.md       # Final comparison table
```

---

## Step 1: Generate Specs

For each feature (1-5), for each format (gwt, ears):

### 1a. Prepare Input

1. Load feature description from `references/feature-descriptions.md` (the relevant section)
2. Load spec writer prompt from `references/gwt-spec-writer-prompt.md` or `references/ears-spec-writer-prompt.md`
3. Substitute `{FEATURE_DESCRIPTION}` and `{CONTRACT_TYPES}` in the prompt

### 1b. Dispatch Spec Writer

Dispatch a single Opus subagent with the assembled prompt. Capture output.

### 1c. Save Output

Save to: `_docs/spec-format-experiment/specs/feature-{N}-{format}-run{R}.md`

Example: `feature-1-gwt-run1.md`, `feature-3-ears-run2.md`

### 1d. Repeat

3 runs per feature per format = 30 spec files total.

**Alternate dispatch order to avoid order bias:**
- Feature 1: GWT first, then EARS
- Feature 2: EARS first, then GWT
- Feature 3: GWT first, then EARS
- Feature 4: EARS first, then GWT
- Feature 5: GWT first, then EARS

### 1e. Minimum Viable

For quick results, run Features 1, 3, 4 only (1 run each = 6 specs). Expand if results are inconclusive.

---

## Step 2: Score Specs

**Scorer selection:** Scoring should be done by a DIFFERENT model than the spec writer (e.g., if spec writer is Opus, scorer is Sonnet) to avoid self-evaluation bias. Alternatively, use human scoring.

For each of the 10 primary specs (run 1 of each feature x format):

### 2a. Completeness

Open the feature's completeness checklist from `references/feature-descriptions.md`. For each checklist item, search the spec for coverage. Mark 1 (covered) or 0 (not covered).

### 2b. Precision

For each scenario/requirement in the spec, check:
- Status code explicit? (1/0)
- DTO names match contracts? (1/0)
- Field names match contracts? (1/0)
- No vague language? (1/0)

### 2c. Token Count

Count characters in the spec output, divide by 4. Record total and per-item ratios.

### 2d. Record

Fill in the scoring template from `references/scoring-rubric.md` for each feature.

---

## Step 3: Implement From Specs

For each of the 10 primary specs:

### 3a. Scaffold

Create a fresh .NET project with:
- **.NET 9 minimal API** project (`Api/`)
- **EF Core with SQLite** (in-memory or file-based)
- **Auth middleware stub** that extracts `userId` from Bearer token (hardcoded test tokens, e.g., `Bearer user-1-token` → userId = `{guid-1}`, `Bearer admin-token` → userId = `{guid-admin}` + admin role)
- **Contracts/ project** containing the feature's DTOs and route constants (compiled, referenced by Api/)
- Standard `CLAUDE.md` conventions

> **Important:** The scaffold should be identical for all trials of the same feature. Create once, copy for each trial.

### 3b. Dispatch Implementer

Dispatch `saurun:backend-implementer` with:
```
Implement the following feature. Use the provided contract types (import from Contracts project, do not create new DTOs).

SPEC:
{contents of the primary spec file}

CONTRACTS:
{contents of contract types from feature-descriptions.md}
```

### 3c. Measure

After implementation completes:

```bash
# Build check
dotnet build

# Start the app
dotnet run &

# For each endpoint, test happy path
curl -s -w "%{http_code}" -X POST http://localhost:5000{route} -H "Authorization: Bearer test-token" -H "Content-Type: application/json" -d '{valid body}'

# Test validation errors
curl -s -w "%{http_code}" -X POST http://localhost:5000{route} -H "Authorization: Bearer test-token" -H "Content-Type: application/json" -d '{invalid body}'

# Test auth boundary
curl -s -w "%{http_code}" -X POST http://localhost:5000{route} -H "Content-Type: application/json" -d '{valid body}'
```

Record pass/fail per check.

### 3d. Root-Cause Analysis

When an implementation trial fails any check, record:

1. **Which check failed** — reference the specific checklist item
2. **Failure category:**
   - **Spec ambiguity** — the spec covered the behavior but wording was unclear
   - **Spec omission** — the spec did not mention this behavior at all
   - **Implementer error** — the spec was clear but the implementer got it wrong
   - **Scaffold issue** — the failure was caused by the project scaffold, not the spec or implementer
3. **Format attribution** — would a different spec format have prevented this failure? (yes/no/uncertain + brief rationale)

Save root-cause data alongside pass/fail results. This is critical for interpreting whether format differences cause implementation differences.

### 3e. Trial 2

Repeat scaffold + dispatch + measure + root-cause for a second trial.

### 3f. Minimum Viable

For quick results, run 1 trial per spec (10 dispatches instead of 20).

---

## Step 4: Consistency Measurement

For each feature x format, compare the 3 spec runs:

1. List completeness items covered in each run
2. Compute Jaccard similarity: |intersection| / |union|
3. Record in scoring template

---

## Step 5: Readability Test

For Features 3, 4, 5 (most complex):

1. Create a "broken" scenario: change one status code or response shape in the implementation
2. Run the curl test — it fails with a specific wrong status/shape
3. Open the spec (GWT or EARS version)
4. Count how many requirements/scenarios you must scan before finding the one that was violated
5. Record the scan count for each format
6. Also count ambiguous requirements: ones where two people could reasonably disagree on pass/fail

Record per feature:
- Scan count (GWT) vs scan count (EARS)
- Ambiguity count (GWT) vs ambiguity count (EARS)

---

## Step 6: Compile Results

1. Fill in the full scoring matrix from `references/scoring-rubric.md`
2. Calculate weighted scores
3. Apply verdict rules
4. Write findings to `_docs/spec-format-experiment/results/scoring-matrix.md`

---

## Step 7: Decision

Based on verdict rules:

| Outcome | Next Step |
|---------|-----------|
| Clear winner | Update pipeline-v3 spec writer to use winning format |
| Hybrid recommended | Design hybrid prompt (EARS for requirements, GWT for state transitions) and re-test on Features 3-4 |
| Inconclusive | Add Features 2, 5 (if running minimum viable) or refine prompts and re-run |

---

## Time Estimates

| Activity | Full (5 features) | Minimum (3 features) |
|----------|--------------------|----------------------|
| Generate specs | 1-2h | 30-60min |
| Score specs | 2-3h | 1-1.5h |
| Implement + measure | 4-5h | 2-3h |
| Readability test | 1h | 30min |
| Compile results | 1-2h | 30-60min |
| **Total** | **~10-13h** | **~5-7h** |

## Token Cost Estimates

| Activity | Full | Minimum |
|----------|------|---------|
| Spec generation | ~90K tokens | ~18K tokens |
| Implementation | ~300K tokens | ~60K tokens |
| **Total** | **~390K** | **~78K** |
