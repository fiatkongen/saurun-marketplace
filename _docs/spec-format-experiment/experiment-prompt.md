# GWT vs EARS+Properties Spec Format Experiment — 3-Feature Minimum

You are running an A/B experiment comparing two behavioral spec formats for AI coding agents. Your job is to execute the full experiment protocol: generate specs, score them, and produce a verdict.

## Background

We're building a spec-first AI development pipeline where 70% of effort goes into spec quality. The spec writer is the single point of failure. We need to decide: should specs use **GWT (Given-When-Then)** or **EARS+Properties (Easy Approach to Requirements Syntax + universal invariants)**?

Both formats sit on top of the same **contract-as-code** (compiled C# DTOs). Only the behavioral spec format varies.

## Hypothesis

- **H0:** GWT and EARS+properties produce equivalent results
- **H1:** EARS+properties is more token-efficient with equal or better completeness and implementer success

---

## STEP 1: Generate Specs

Generate specs for 3 features × 2 formats = **6 specs total**. Generate each spec sequentially (one at a time) to ensure full attention per spec. Use the Task tool with `subagent_type: "general-purpose"` for each dispatch.

**Dispatch order (alternating to avoid order bias):**
1. Feature 1: GWT first, then EARS
2. Feature 3: EARS first, then GWT
3. Feature 4: GWT first, then EARS

**Substitution instructions:** For each dispatch, take the relevant prompt template below and replace:
- `{FEATURE_DESCRIPTION}` → the ENTIRE feature section (endpoints table, entity, rules — everything from the feature heading through the end of the rules)
- `{CONTRACT_TYPES}` → the C# code block from that feature's "Contract Types" section

Save each spec to: `_docs/spec-format-experiment/specs/feature-{N}-{format}.md`

### GWT Spec Writer Prompt Template

```
You are a spec writer for a .NET Core + React application.

Your job: produce GWT (Given-When-Then) acceptance criteria for the feature below.

## Input

- Feature description
- Contract types (compiled C# DTOs from Contracts/ assembly)

## Output Format

For each endpoint or behavior, write GWT scenarios following this template:

### GWT-{N}: {Descriptive name}
GIVEN {precondition — auth state, existing data}
WHEN {action — HTTP method + path + body/params}
THEN {assertion — status code, response body fields, exact values}
AND {additional assertions}

## GWT Patterns

Use these patterns to ensure comprehensive coverage:

| Pattern | Template | Use When |
|---------|----------|----------|
| Happy path | GIVEN auth user, WHEN valid request, THEN success response | Every endpoint |
| Validation error | GIVEN auth user, WHEN invalid field, THEN 400 | Every POST/PUT |
| Auth boundary | GIVEN no auth, WHEN request, THEN 401 | Every protected endpoint |
| Not found | GIVEN auth user, WHEN request with nonexistent ID, THEN 404 | Every path param |
| State transition (valid) | GIVEN entity in state X, WHEN action, THEN new state Y | State machines |
| State transition (invalid) | GIVEN entity in state X, WHEN invalid action, THEN 409 | State machines |
| Ownership boundary | GIVEN user A, WHEN accessing user B's resource, THEN 403 | Multi-user resources |

## Rules

1. Every assertion specifies EXACT values: status codes, field names, error shapes.
2. Every endpoint needs: happy path, at least one validation error (400), auth boundary (401/403).
3. Every path parameter needs a "not found" scenario (404).
4. State transitions: one GWT per valid transition AND one per invalid transition.
5. No vague language: "appropriate", "some", "various", "proper", "correct" are BANNED. Use exact values.
6. Target: 2-4 GWTs per endpoint. More for complex endpoints with state transitions.
7. Be comprehensive. Cover all checklist items. Token usage will be measured as an outcome, not constrained as an input.
8. Reference DTO names and field names EXACTLY as they appear in the contract types.
9. Group GWTs by endpoint, ordered by: happy path first, then validation errors, then auth boundaries, then edge cases.
10. Every GWT must have all three clauses: GIVEN, WHEN, THEN.

## Quality Checklist (Self-Check Before Submitting)

- [ ] Every endpoint has a happy path GWT
- [ ] Every POST/PUT has at least one validation error GWT (400)
- [ ] Every auth-protected endpoint has an unauthorized GWT (401)
- [ ] Every path parameter has a not-found GWT (404)
- [ ] Every state transition (valid AND invalid) has a GWT
- [ ] Every business rule has a GWT that tests the boundary
- [ ] All field names match the contract DTOs exactly
- [ ] All status codes are explicit numbers (201, 400, 401, 403, 404, 409, 413, 415)
- [ ] No vague language anywhere

## Feature

{FEATURE_DESCRIPTION}

## Contract Types

{CONTRACT_TYPES}
```

### EARS+Properties Spec Writer Prompt Template

```
You are a spec writer for a .NET Core + React application.

Your job: produce EARS (Easy Approach to Requirements Syntax) requirements + property descriptions for the feature below.

## Input

- Feature description
- Contract types (compiled C# DTOs from Contracts/ assembly)

## Output Format

Two sections: EARS Requirements and Property Descriptions.

### Section 1: EARS Requirements

Use these EARS templates:

| Pattern | Template | Use When |
|---------|----------|----------|
| **Ubiquitous** | The [system] shall [action]. | Always-on requirements |
| **Event-driven** | When [trigger], the [system] shall [action]. | Reactive behavior (endpoint calls) |
| **State-driven** | While [state], the [system] shall [action]. | Condition-dependent behavior |
| **Unwanted** | If [condition], then the [system] shall [action]. | Error handling, validation, edge cases |
| **Optional** | Where [feature], the [system] shall [action]. | Product variants |
| **Complex** | While [state], when [trigger], the [system] shall [action]. | State + event combination |

Format:
REQ-{N}: {EARS requirement sentence}

### Section 2: Property Descriptions

Universal invariants that hold across ALL requests. Use these patterns:

| Pattern | Template |
|---------|----------|
| **Output property** | For all valid {Request} inputs, the returned {Dto}.{field} must {constraint}. |
| **Idempotency** | For all DELETE /path/{id} requests, repeated calls must return {status}. |
| **Ordering** | For all GET /path responses with sort={x}, items must be ordered by {x} {direction}. |
| **Boundary** | For all {Request} inputs where {field} {violates constraint}, the API must return {status}. |
| **Invariant** | For all {Dto} responses, {field} must {constraint}. |
| **State** | For all {Entity} in state {S}, the only valid transitions are {S1, S2, ...}. |

Format:
PROP-{N}: {Property description sentence}

## Rules

1. One requirement/property per line. Prefix with REQ-{N} or PROP-{N}.
2. Reference DTO names from contracts EXACTLY (e.g., "return a BookmarkDto", not "return a bookmark response").
3. Status codes are part of the requirement (e.g., "return 201 with a BookmarkDto").
4. Group EARS requirements by endpoint or concern.
5. Every endpoint needs at least one EARS requirement and relevant properties.
6. Auth boundaries: explicit EARS requirement for each auth rule using the Unwanted pattern.
7. No vague language: "appropriate", "some", "various", "proper", "correct" are BANNED. Use exact values.
8. Properties complement EARS requirements — they express what is ALWAYS true regardless of specific input.
9. Each property must be independently verifiable (testable in isolation).
10. Be comprehensive. Cover all checklist items. Token usage will be measured as an outcome, not constrained as an input.

## Quality Checklist (Self-Check Before Submitting)

- [ ] Every endpoint has at least one Event-driven EARS requirement (happy path)
- [ ] Every POST/PUT has at least one Unwanted EARS requirement (validation error, 400)
- [ ] Every auth-protected endpoint has an Unwanted requirement (401/403)
- [ ] Every path parameter has an Unwanted requirement (not found, 404)
- [ ] Every state transition is covered (valid transitions as Event-driven, invalid as Unwanted)
- [ ] Every business rule has a Property or Unwanted requirement
- [ ] All field names match contract DTOs exactly
- [ ] All status codes are explicit numbers
- [ ] Properties cover cross-cutting invariants (timestamps, IDs, ownership)
- [ ] No vague language anywhere

## Feature

{FEATURE_DESCRIPTION}

## Contract Types

{CONTRACT_TYPES}
```

---

## FEATURE DESCRIPTIONS

### Feature 1: Bookmark CRUD (Simple Baseline)

**Complexity:** Low — single entity, 4 endpoints, no state transitions.

#### Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | /api/bookmarks | Required | Create bookmark |
| GET | /api/bookmarks | Required | List user's bookmarks |
| GET | /api/bookmarks/{id} | Required | Get single bookmark |
| DELETE | /api/bookmarks/{id} | Required | Delete bookmark |

#### Entity

```
Bookmark { id: GUID, url: string, title: string, description: string?, userId: GUID, createdAt: DateTime }
```

#### Rules

- url: required, must be valid URL format
- title: required, 1-200 characters
- description: optional, max 1000 characters
- Users can only see/delete their own bookmarks
- Deleting a nonexistent bookmark returns 404
- Listing returns bookmarks ordered by createdAt descending

#### Contract Types (C#)

```csharp
public record BookmarkDto(Guid Id, string Url, string Title, string? Description, Guid UserId, DateTime CreatedAt);
public record CreateBookmarkRequest(string Url, string Title, string? Description);

public static class BookmarkRoutes
{
    public const string Create = "/api/bookmarks";
    public const string List = "/api/bookmarks";
    public const string Get = "/api/bookmarks/{id}";
    public const string Delete = "/api/bookmarks/{id}";
}
```

#### Completeness Checklist (16 items)

- [ ] POST happy path → 201 + BookmarkDto
- [ ] POST empty url → 400
- [ ] POST invalid URL format → 400
- [ ] POST empty title → 400
- [ ] POST title > 200 chars → 400
- [ ] POST description > 1000 chars → 400
- [ ] POST no auth → 401
- [ ] GET list happy path → 200 + BookmarkDto[]
- [ ] GET list no auth → 401
- [ ] GET single happy path → 200 + BookmarkDto
- [ ] GET single not found → 404
- [ ] GET single other user's bookmark → 403 or 404
- [ ] GET single no auth → 401
- [ ] DELETE happy path → 204
- [ ] DELETE other user's bookmark → 403 or 404
- [ ] DELETE no auth → 401

---

### Feature 3: Invitation System (Authorization Boundaries)

**Complexity:** Medium-high — multiple roles, authorization rules that vary by context.

#### Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | /api/teams/{teamId}/invitations | Required | Invite user to team |
| GET | /api/teams/{teamId}/invitations | Required | List team invitations |
| PUT | /api/invitations/{id}/accept | Required | Accept invitation |
| PUT | /api/invitations/{id}/decline | Required | Decline invitation |
| DELETE | /api/invitations/{id} | Required | Cancel invitation |

#### Entities

```
Team { id: GUID, name: string, ownerId: GUID }
Invitation { id: GUID, teamId: GUID, inviterUserId: GUID, inviteeEmail: string, status: InvitationStatus, createdAt: DateTime, respondedAt: DateTime? }
InvitationStatus: Pending | Accepted | Declined | Cancelled
```

#### Rules

- Only team owner can invite
- Only the invitee (matched by email from auth token) can accept/decline
- Only the inviter OR team owner can cancel
- Non-members cannot view team invitations
- Cannot invite same email twice to same team while an invitation is pending
- Status transitions: Pending → Accepted, Pending → Declined, Pending → Cancelled. No other transitions.
- Accepted/Declined/Cancelled invitations cannot be modified
- Team must exist (404 if not)
- inviteeEmail must be valid email format

#### Contract Types (C#)

```csharp
public record InvitationDto(Guid Id, Guid TeamId, Guid InviterUserId, string InviteeEmail, string Status, DateTime CreatedAt, DateTime? RespondedAt);
public record CreateInvitationRequest(string InviteeEmail);

public static class InvitationRoutes
{
    public const string Create = "/api/teams/{teamId}/invitations";
    public const string List = "/api/teams/{teamId}/invitations";
    public const string Accept = "/api/invitations/{id}/accept";
    public const string Decline = "/api/invitations/{id}/decline";
    public const string Cancel = "/api/invitations/{id}";
}
```

#### Completeness Checklist (18 items)

- [ ] POST invite happy path → 201 + InvitationDto (status = "Pending")
- [ ] POST invite as non-owner → 403
- [ ] POST invite no auth → 401
- [ ] POST invite invalid email → 400
- [ ] POST invite duplicate pending → 409
- [ ] POST invite to nonexistent team → 404
- [ ] GET list as team member → 200 + InvitationDto[]
- [ ] GET list as non-member → 403
- [ ] PUT accept as invitee → 200 + InvitationDto (status = "Accepted")
- [ ] PUT accept as non-invitee → 403
- [ ] PUT accept already accepted → 409 (Conflict — invitation already actioned)
- [ ] PUT decline as invitee → 200 + InvitationDto (status = "Declined")
- [ ] PUT decline as non-invitee → 403
- [ ] PUT decline already declined → 409 (Conflict — invitation already actioned)
- [ ] DELETE cancel as inviter → 204
- [ ] DELETE cancel as team owner → 204
- [ ] DELETE cancel as other user → 403
- [ ] DELETE cancel already accepted → 409 (Conflict — invitation already actioned)

---

### Feature 4: Comment Thread with Moderation (State Machine)

**Complexity:** High — nested entity with state transitions, soft delete, moderation rules.

#### Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | /api/posts/{postId}/comments | Required | Create comment |
| GET | /api/posts/{postId}/comments | Public | List comments on post |
| PUT | /api/comments/{id} | Required | Edit comment |
| DELETE | /api/comments/{id} | Required | Delete comment (soft) |
| PUT | /api/comments/{id}/flag | Required | Flag comment |
| PUT | /api/comments/{id}/moderate | Required (Admin) | Moderate flagged comment |

#### Entity

```
Comment { id: GUID, postId: GUID, authorId: GUID, parentId: GUID?, content: string, status: CommentStatus, editCount: int, createdAt: DateTime, editedAt: DateTime? }
CommentStatus: Active | Edited | Flagged | Deleted | Approved | Removed
```

#### Rules

**State transitions:**
- Active → Edited (on PUT), Active → Flagged (on flag), Active → Deleted (on DELETE)
- Edited → Flagged (on flag), Edited → Deleted (on DELETE)
- Flagged → Approved (admin moderate), Flagged → Removed (admin moderate)
- No other transitions allowed (e.g., Deleted → Active is invalid)

**Business rules:**
- Max 3 edits per comment (editCount tracks)
- Comments older than 24 hours cannot be edited
- Flagged comments are hidden from non-admins in listings (still exist, just filtered)
- Nested comments max depth 3 (parentId chain)
- Content: required, 1-5000 characters
- Only the author can edit/delete their own comments
- Any authenticated user can flag a comment (not their own)
- Only admins can moderate

#### Contract Types (C#)

```csharp
public record CommentDto(Guid Id, Guid PostId, Guid AuthorId, Guid? ParentId, string Content, string Status, int EditCount, DateTime CreatedAt, DateTime? EditedAt);
public record CreateCommentRequest(string Content, Guid? ParentId);
public record UpdateCommentRequest(string Content);
public record ModerateCommentRequest(string Decision); // "approve" or "remove"

public static class CommentRoutes
{
    public const string Create = "/api/posts/{postId}/comments";
    public const string List = "/api/posts/{postId}/comments";
    public const string Update = "/api/comments/{id}";
    public const string Delete = "/api/comments/{id}";
    public const string Flag = "/api/comments/{id}/flag";
    public const string Moderate = "/api/comments/{id}/moderate";
}
```

#### Completeness Checklist (28 items)

- [ ] POST create happy path → 201 + CommentDto (status = "Active")
- [ ] POST create with parentId (reply) → 201
- [ ] POST create depth > 3 → 400
- [ ] POST create empty content → 400
- [ ] POST create content > 5000 chars → 400
- [ ] POST create no auth → 401
- [ ] POST create on nonexistent post → 404
- [ ] GET list happy path → 200 + CommentDto[] (flagged filtered for non-admins)
- [ ] GET list as admin → 200 + includes flagged comments
- [ ] PUT edit happy path → 200 + CommentDto (status = "Edited", editCount incremented)
- [ ] PUT edit no auth → 401
- [ ] PUT edit by non-author → 403
- [ ] PUT edit after 3 edits → 400 or 409
- [ ] PUT edit after 24 hours → 400 or 409
- [ ] PUT edit deleted comment → 400 or 409
- [ ] PUT edit flagged comment → 400 or 409
- [ ] DELETE happy path → 204 (status → "Deleted")
- [ ] DELETE no auth → 401
- [ ] DELETE by non-author → 403
- [ ] DELETE already deleted → 404 or 409
- [ ] PUT flag happy path → 200 (status → "Flagged")
- [ ] PUT flag no auth → 401
- [ ] PUT flag own comment → 400 or 403
- [ ] PUT flag deleted comment → 400 or 409
- [ ] PUT moderate approve → 200 (status → "Approved")
- [ ] PUT moderate remove → 200 (status → "Removed")
- [ ] PUT moderate no auth → 401
- [ ] PUT moderate by non-admin → 403

---

## STEP 2: Score Specs

After all 6 specs are generated and saved to files, score them. **Before scoring each spec, read it back from the saved file** — do not score from memory.

**Scorer model:** Use the Task tool with `model: "sonnet"` for scoring agents to avoid self-evaluation bias (the spec writer uses Opus, the scorer uses Sonnet). If model selection is unavailable, self-score but note "self-eval bias risk" in the results.

### Scoring Dimensions

**Dimension 1: Completeness (Weight: 30%)**

For each checklist item: does the spec cover it? Binary 1/0.

Score = items covered / total items

**Dimension 2: Precision (Weight: 20%)**

For each scenario/requirement in the spec, check 5 sub-criteria:
- Status code is explicit number? (1/0)
- Response DTO name matches contract exactly? (1/0)
- Response field names match contract exactly? (1/0)
- Validation error shape specified, not just "returns error"? (1/0)
- Numeric constraints have exact values, not "within limits"? (1/0)

Any requirement containing vague language scores 0 on ALL sub-criteria. Vague language disqualifiers: "appropriate response", "some kind of error", "proper validation", "correct status", "relevant error message", "various fields"

Score = precise sub-criteria passed / total sub-criteria checked

**Dimension 3: Token Efficiency (Weight: 15%)**

- Total output tokens = character count / 4
- Tokens per completeness item covered
- Lower is better. Normalize: best format gets 1.0, other gets (best / its value)

**Dimension 4: Readability (Weight: 5%)**

- Count ambiguous requirements (where two people could reasonably disagree on pass/fail)
- Score: 0 ambiguities = 1.0, 1 = 0.8, 2-3 = 0.6, 4-5 = 0.4, 6+ = 0.2

*Note: Implementer success (25%) and consistency (5%) are deferred — they require running implementations and multiple spec generation runs respectively. This experiment produces the spec-quality dimensions (completeness, precision, tokens, readability = 70% of weight).*

### Scoring Prompt for Each Spec

```
You are a scorer for a spec format experiment. Score the following spec against its completeness checklist and precision criteria.

SPEC TO SCORE:
{spec contents}

COMPLETENESS CHECKLIST:
{checklist from feature description}

CONTRACT TYPES:
{contract types}

SCORING INSTRUCTIONS:

1. COMPLETENESS: For each checklist item, search the spec. Mark 1 if covered, 0 if not.
   - "Covered" means a specific scenario/requirement addresses this exact behavior
   - Partial coverage (e.g., mentions the endpoint but not the specific error case) = 0

2. PRECISION: For each scenario/requirement in the spec, check 5 sub-criteria:
   - Status code explicit number? (1/0)
   - Response DTO name matches contract exactly? (1/0)
   - Field names match contract exactly? (1/0)
   - Validation error shape specified, not just "returns error"? (1/0)
   - Numeric constraints have exact values, not "within limits"? (1/0)
   If vague language found (appropriate, some, proper, correct, relevant, various), score ALL sub-criteria 0 for that requirement.

3. TOKEN COUNT: Count total characters in the spec, divide by 4. (Use `wc -c < filepath` via Bash if available.)

4. AMBIGUITY: Count requirements where pass/fail is genuinely ambiguous.

OUTPUT FORMAT (use exactly this structure):

## Completeness
| # | Checklist Item | Covered (1/0) | Which scenario/requirement covers it |
|---|---------------|--------------|--------------------------------------|
| 1 | ... | 1 | GWT-3 / REQ-5 / PROP-2 |

**Completeness Score: X/Y**

## Precision
| # | Scenario/Requirement | Status Code | DTO Name | Field Names | Error Shape | Numeric Values | Vague? | Score |
|---|---------------------|------------|----------|-------------|-------------|---------------|--------|-------|
| 1 | ... | 1/0 | 1/0 | 1/0 | 1/0 | 1/0 | Y/N | X/5 |

**Precision Score: X/Y (out of total sub-criteria)**

## Token Count
- Characters: X
- Estimated tokens: X/4 = Y

## Ambiguity
- Ambiguous requirements: [list them]
- Ambiguity count: N

Be strict. Do not give benefit of the doubt. Binary scoring only.
```

---

## STEP 3: Compile Results

After scoring all 6 specs, compile into this table:

```markdown
## Results Summary

### Per-Feature Scores

| Feature | Format | Completeness | Precision | Tokens | Tokens/Item | Ambiguity | Readability |
|---------|--------|-------------|-----------|--------|-------------|-----------|-------------|
| 1 | GWT | X/16 | X% | N | N | N | X |
| 1 | EARS | X/16 | X% | N | N | N | X |
| 3 | GWT | X/18 | X% | N | N | N | X |
| 3 | EARS | X/18 | X% | N | N | N | X |
| 4 | GWT | X/28 | X% | N | N | N | X |
| 4 | EARS | X/28 | X% | N | N | N | X |

### Weighted Scores (available dimensions: 70% of total weight)

| Feature | Dimension | Weight | GWT | EARS |
|---------|-----------|--------|-----|------|
| 1 | Completeness | 0.30 | X | X |
| 1 | Precision | 0.20 | X | X |
| 1 | Token Efficiency | 0.15 | X | X |
| 1 | Readability | 0.05 | X | X |
| **1** | **Subtotal (70%)** | | **X** | **X** |
| 3 | ... | | | |
| 4 | ... | | | |

### Verdict

| Feature | Winner | Margin | Key Differentiator |
|---------|--------|--------|-------------------|
| 1 (CRUD) | ? | X% | ? |
| 3 (Auth) | ? | X% | ? |
| 4 (State Machine) | ? | X% | ? |

### Overall: [GWT / EARS / Hybrid / Tie]

Reasoning: ...

### Limitations
- Single-run spec generation (no consistency measurement). If margin < 10%, consider result inconclusive pending multi-run validation.
- Self-evaluation bias risk if scorer model = spec writer model.
- Implementer success (25% weight) and consistency (5% weight) deferred — results represent 70% of full scoring weight.

### Recommendation for pipeline-v3 spec writer: ...
```

Save the full results to `_docs/spec-format-experiment/results/scoring-matrix.md`

---

## Execution Order

1. **Create output directory**: `_docs/spec-format-experiment/specs/` and `_docs/spec-format-experiment/results/`
2. **Generate 6 specs** (3 features × 2 formats) — use parallel agents where possible
3. **Score 6 specs** — use Sonnet model, parallel scoring agents
4. **Compile results** into the scoring matrix
5. **Write verdict** with recommendation

You have full autonomy to execute. Do not ask for confirmation between steps. Run the full experiment and report results.
