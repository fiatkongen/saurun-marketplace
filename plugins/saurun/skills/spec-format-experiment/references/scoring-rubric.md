# Scoring Rubric: GWT vs EARS+Properties

All scoring is **binary per item** (met/not met) to prevent subjective assessments. This follows the model-ab-test discipline.

---

## Dimension 1: Completeness (Weight: 30%)

Score against the feature's completeness checklist in `feature-descriptions.md`.

| Rule | Score |
|------|-------|
| Checklist item is covered by at least one GWT/REQ/PROP | 1 |
| Checklist item is not covered | 0 |

**Score:** items covered / total items

---

## Dimension 2: Precision (Weight: 20%)

Does every assertion/requirement specify exact verifiable values?

| Check | Per Scenario/Requirement | Score |
|-------|------------------------|-------|
| Status code is explicit number (201, 400, etc.) | 1/0 |
| Response DTO name matches contract exactly | 1/0 |
| Response field names match contract exactly | 1/0 |
| Validation error shape specified (not just "returns error") | 1/0 |
| Numeric constraints have exact values | 1/0 |

**Score:** precise assertions / total assertions

**Vague language disqualifiers** (if found, that requirement scores 0 on precision):
- "appropriate response"
- "some kind of error"
- "proper validation"
- "correct status"
- "relevant error message"
- "various fields"

---

## Dimension 3: Token Efficiency (Weight: 15%)

Measured mechanically:

| Metric | How |
|--------|-----|
| Total output tokens | Character count / 4 (approximation) |
| Tokens per completeness item covered | Total tokens / completeness score numerator |
| Tokens per endpoint covered | Total tokens / endpoints with happy path coverage |

**Score:** Lower is better. Report absolute numbers. For weighted scoring, normalize: best format gets 1.0, other gets (best / its value).

---

## Dimension 4: Implementer Success Rate (Weight: 25%)

After dispatching backend-implementer with each spec:

| Check | Per Endpoint | Score |
|-------|-------------|-------|
| `dotnet build` passes | 1/0 (one-time, all-or-nothing) |
| Endpoint returns correct status code for happy path | 1/0 |
| Response body matches DTO shape (all fields present) | 1/0 |
| Validation error returns correct status (400/413/415) | 1/0 |
| Auth boundary returns 401 when no token | 1/0 |
| Auth boundary returns 403 when wrong user | 1/0 |

Run 2 trials per spec. Report average.

**Score:** checks passing / total checks, averaged across trials.

---

## Dimension 5: Spec Writer Consistency (Weight: 5%)

Run spec writer 3 times per feature per format:

| Metric | How |
|--------|-----|
| Structural consistency | Same number of scenarios/requirements across 3 runs? |
| Coverage consistency | Same completeness checklist items covered across 3 runs? |
| Value consistency | Same exact values (status codes, field names) across 3 runs? |

**Score:** Jaccard similarity of covered completeness items across 3 runs.

Formula: |intersection of items covered in all 3 runs| / |union of items covered in any run|

---

## Dimension 6: Readability / Debuggability (Weight: 5%)

All scoring is binary per item where possible. Given a failing implementation (specific curl output showing wrong status/shape):

| Metric | How |
|--------|-----|
| Traceability (scan count) | Count the number of requirements/scenarios that must be scanned to find the violated one. Lower is better. |
| Ambiguity count | Count requirements where two people could reasonably disagree on pass/fail |

**Score:**
- Traceability: normalize as (best format scan count) / (this format scan count). Best gets 1.0.
- Ambiguity: 0 = 5, 1 = 4, 2-3 = 3, 4-5 = 2, 6+ = 1 (then normalize to 0.0-1.0)
- Average the two sub-scores

---

## Weighted Score Calculation

```
WEIGHTED = (Completeness × 0.30) + (Precision × 0.20) + (TokenEfficiency × 0.15) + (ImplSuccess × 0.25) + (Consistency × 0.05) + (Readability × 0.05)
```

All dimension scores normalized to 0.0-1.0 before weighting. TokenEfficiency and Readability are inverted scales (see dimension descriptions).

---

## Verdict Rules

| Outcome | Decision |
|---------|----------|
| EARS weighted score > GWT on >= 4/5 features | Adopt EARS+properties |
| GWT weighted score > EARS on >= 4/5 features | Keep GWT |
| Mixed (each wins some features) | Investigate hybrid |
| Weighted scores within 5% on >= 3 features | Statistical tie → choose by token efficiency |

---

## Scoring Template

```markdown
## Feature {N}: {Name}

### Completeness
| Item | GWT | EARS |
|------|-----|------|
| ... | 1/0 | 1/0 |
| **Total** | X/Y | X/Y |

### Precision
| GWT scenarios | Precise? | EARS requirements | Precise? |
|--------------|----------|-------------------|----------|
| ... | 1/0 | ... | 1/0 |
| **Total** | X/Y | | X/Y |

### Tokens
| Metric | GWT | EARS |
|--------|-----|------|
| Total tokens | | |
| Per completeness item | | |

### Implementer Success (Trial 1 / Trial 2)
| Check | GWT T1 | GWT T2 | EARS T1 | EARS T2 |
|-------|--------|--------|---------|---------|
| Build pass | | | | |
| ... | | | | |

### Consistency (3 runs)
| Metric | GWT | EARS |
|--------|-----|------|
| Jaccard similarity | | |

### Readability
| Metric | GWT | EARS |
|--------|-----|------|
| Scan count (requirements checked) | | |
| Ambiguity count | | |

### Weighted Score
| Dimension | Weight | GWT | EARS |
|-----------|--------|-----|------|
| Completeness | 0.30 | | |
| Precision | 0.20 | | |
| Token efficiency | 0.15 | | |
| Impl success | 0.25 | | |
| Consistency | 0.05 | | |
| Readability | 0.05 | | |
| **WEIGHTED** | | **?** | **?** |
```
