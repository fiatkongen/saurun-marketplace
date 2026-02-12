# Score: Feature 3 EARS Spec

## Completeness
| # | Checklist Item | Covered (1/0) | Which requirement covers it |
|---|---------------|--------------|--------------------------|
| 1 | POST invite happy path → 201 + InvitationDto (status = "Pending") | 1 | REQ-1, PROP-21 |
| 2 | POST invite as non-owner → 403 | 1 | REQ-3 |
| 3 | POST invite no auth → 401 | 1 | REQ-2 |
| 4 | POST invite invalid email → 400 | 1 | REQ-5 |
| 5 | POST invite duplicate pending → 409 | 1 | REQ-6, REQ-26 |
| 6 | POST invite to nonexistent team → 404 | 1 | REQ-4 |
| 7 | GET list as team member → 200 + InvitationDto[] | 1 | REQ-7 |
| 8 | GET list as non-member → 403 | 1 | REQ-9 |
| 9 | PUT accept as invitee → 200 + InvitationDto (status = "Accepted") | 1 | REQ-11, PROP-23 |
| 10 | PUT accept as non-invitee → 403 | 1 | REQ-13 |
| 11 | PUT accept already accepted → 409 | 1 | REQ-15 |
| 12 | PUT decline as invitee → 200 + InvitationDto (status = "Declined") | 1 | REQ-16, PROP-24 |
| 13 | PUT decline as non-invitee → 403 | 1 | REQ-18 |
| 14 | PUT decline already declined → 409 | 1 | REQ-20 |
| 15 | DELETE cancel as inviter → 204 | 0 | REQ-21 returns 200, not 204 |
| 16 | DELETE cancel as team owner → 204 | 0 | REQ-21 returns 200, not 204 |
| 17 | DELETE cancel as other user → 403 | 1 | REQ-23 |
| 18 | DELETE cancel already accepted → 409 | 1 | REQ-25 |

**Completeness Score: 16/18**

**Notes:**
- Items 15 & 16 not covered: The spec says DELETE returns 200 with InvitationDto (REQ-21), but checklist expects 204 (no content). This is a mismatch between spec and checklist requirements.

## Precision

| # | Requirement | Status Code | DTO Name | Field Names | Error Shape | Numeric Values | Vague? | Score |
|---|---------|------------|----------|-------------|-------------|---------------|--------|-------|
| 1 | REQ-1 | 1 | 1 | 0 | 0 | N/A | N | 2/4 |
| 2 | REQ-2 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 3 | REQ-3 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 4 | REQ-4 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 5 | REQ-5 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 6 | REQ-6 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 7 | REQ-7 | 1 | 1 | 0 | 0 | N/A | N | 2/3 |
| 8 | REQ-8 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 9 | REQ-9 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 10 | REQ-10 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 11 | REQ-11 | 1 | 1 | 1 | 0 | N/A | N | 3/4 |
| 12 | REQ-12 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 13 | REQ-13 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 14 | REQ-14 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 15 | REQ-15 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 16 | REQ-16 | 1 | 1 | 1 | 0 | N/A | N | 3/4 |
| 17 | REQ-17 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 18 | REQ-18 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 19 | REQ-19 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 20 | REQ-20 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 21 | REQ-21 | 1 | 1 | 1 | 0 | N/A | N | 3/4 |
| 22 | REQ-22 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 23 | REQ-23 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 24 | REQ-24 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 25 | REQ-25 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 26 | REQ-26 | 1 | 0 | 0 | 0 | N/A | N | 1/3 |
| 27 | PROP-1 | N/A | 1 | 1 | N/A | 0 | N | 2/3 |
| 28 | PROP-2 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |
| 29 | PROP-3 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |
| 30 | PROP-4 | N/A | 1 | 1 | N/A | 0 | N | 2/3 |
| 31 | PROP-5 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |
| 32 | PROP-6 | N/A | 0 | 1 | N/A | N/A | N | 1/2 |
| 33 | PROP-7 | N/A | 0 | 1 | N/A | N/A | N | 1/2 |
| 34 | PROP-8 | N/A | 0 | 1 | N/A | N/A | N | 1/2 |
| 35 | PROP-9 | N/A | 0 | 1 | N/A | N/A | N | 1/2 |
| 36 | PROP-10 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |
| 37 | PROP-11 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |
| 38 | PROP-12 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |
| 39 | PROP-13 | N/A | 0 | 1 | N/A | N/A | N | 1/2 |
| 40 | PROP-14 | N/A | 0 | 1 | N/A | 0 | N | 1/3 |
| 41 | PROP-15 | 1 | 0 | 0 | N/A | N/A | N | 1/2 |
| 42 | PROP-16 | 1 | 0 | 1 | N/A | N/A | N | 2/3 |
| 43 | PROP-17 | 1 | 0 | 0 | N/A | N/A | N | 1/2 |
| 44 | PROP-18 | 1 | 0 | 1 | N/A | N/A | N | 2/3 |
| 45 | PROP-19 | 1 | 0 | 1 | N/A | N/A | N | 2/3 |
| 46 | PROP-20 | 1 | 0 | 1 | N/A | N/A | N | 2/3 |
| 47 | PROP-21 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |
| 48 | PROP-22 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |
| 49 | PROP-23 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |
| 50 | PROP-24 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |
| 51 | PROP-25 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |

**Precision Score: 82/152**

### Scoring Notes:

**Status Code**: All REQs have explicit numeric status codes (200, 201, 400, 401, 403, 404, 409). All relevant PROPs reference numeric codes. Score: 32/32 applicable.

**DTO Name**:
- 5 REQs mention InvitationDto explicitly (REQ-1, REQ-7, REQ-11, REQ-16, REQ-21)
- 21 REQs do not mention response shape for errors (just "return 401", etc.)
- 17 PROPs mention InvitationDto or Invitation entities explicitly
- 8 PROPs do not mention entity type directly
- Score: 22/51 applicable

**Field Names**:
- REQ-1: mentions "status", InvitationDto (Status field not explicitly named in return clause) = 0
- REQ-7: mentions "list of InvitationDto" but no field enumeration = 0
- REQ-11: explicitly mentions Status, RespondedAt fields = 1
- REQ-16: explicitly mentions Status, RespondedAt fields = 1
- REQ-21: explicitly mentions Status, RespondedAt fields = 1
- REQ-26: mentions InviteeEmail, TeamId fields = 1
- All 21 error REQs: no fields mentioned = 0
- All 25 PROPs: mention specific field names (Id, TeamId, InviterUserId, InviteeEmail, Status, CreatedAt, RespondedAt, OwnerId) = 25
- Score: 29/51 applicable

**Error Shape**: No REQ or PROP specifies validation error structure (e.g., "returns 400 with ValidationError containing field-level errors"). All just say "return 400" or "return 409". Score: 0/26 applicable (26 error-returning requirements).

**Numeric Values**:
- PROP-1: "non-empty GUID" - no numeric constraint applicable
- PROP-4: "non-empty string" - no length specified = 0
- PROP-14: "at most one" - this is numeric but refers to cardinality, not a value = 0
- All other PROPs: no numeric constraints to specify
- Score: 0/3 applicable

**Vague Language**: None detected. No use of "appropriate", "some", "proper", "correct", "relevant", "various".

## Token Count
- Characters: 7950
- Estimated tokens: 7950/4 = 1988 tokens

## Ambiguity
- **REQ-8**: Missing from GET /api/teams/{teamId}/invitations section in spec (jumps from REQ-7 to REQ-9). Auth requirement inferred but not explicitly stated in the spec as written. Actually, re-checking: REQ-8 IS present at line 32. No ambiguity.
- **REQ-21**: Says DELETE should "return 200 with an InvitationDto" but checklist expects 204 (no content). Spec is clear, but conflicts with typical REST convention for DELETE (which returns 204 No Content). This is unambiguous within the spec itself.
- **PROP-1**: "non-empty GUID" - what constitutes "non-empty"? All GUIDs have bytes, so this likely means "not Guid.Empty (00000000-0000-0000-0000-000000000000)". Minor ambiguity on whether empty means null vs. zero GUID.
- **PROP-6**: "the only valid transitions are" - does this mean the system MUST prevent other transitions (enforcement requirement) or that the spec only defines these transitions (documentation)? Ambiguous on enforcement vs. description.
- **PROP-7, PROP-8, PROP-9**: "no status transitions are valid" - same ambiguity as PROP-6. Is this enforcement or description?
- **PROP-13**: "must never change once set" - enforcement requirement or invariant assertion? Ambiguous on whether this implies API rejection or data integrity constraint.
- **PROP-14**: "at most one" - does this mean the system prevents creation of duplicates (enforced by REQ-6) or that cleanup is required? Likely enforced, but phrasing is slightly ambiguous.
- **PROP-15**: Says "repeated calls must return 409" but REQ-21 + REQ-25 together imply DELETE on already-cancelled returns 409 (status other than Pending). However, this conflicts with idempotency semantics (typically DELETE is idempotent and returns 404 on repeated calls). Ambiguous on expected behavior for DELETE on already-cancelled invitation.

**Ambiguous requirements:** PROP-1, PROP-6, PROP-7, PROP-8, PROP-9, PROP-13, PROP-14, PROP-15

**Ambiguity count: 8**
