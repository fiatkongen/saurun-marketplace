# Score: Feature 3 GWT R2 Spec

## Completeness

| # | Checklist Item | Covered (1/0) | Which scenario covers it |
|---|----------------|---------------|--------------------------|
| 1 | POST invite happy path → 201 + InvitationDto (status = "Pending") | 1 | GWT-1 |
| 2 | POST invite as non-owner → 403 | 1 | GWT-2 |
| 3 | POST invite no auth → 401 | 1 | GWT-3 |
| 4 | POST invite invalid email → 400 | 1 | GWT-5 |
| 5 | POST invite duplicate pending → 409 | 1 | GWT-7 |
| 6 | POST invite to nonexistent team → 404 | 1 | GWT-4 |
| 7 | GET list as team member → 200 + InvitationDto[] | 1 | GWT-11 |
| 8 | GET list as non-member → 403 | 1 | GWT-13 |
| 9 | PUT accept as invitee → 200 + InvitationDto (status = "Accepted") | 1 | GWT-17 |
| 10 | PUT accept as non-invitee → 403 | 1 | GWT-18 |
| 11 | PUT accept already accepted → 409 | 1 | GWT-21 |
| 12 | PUT decline as invitee → 200 + InvitationDto (status = "Declined") | 1 | GWT-24 |
| 13 | PUT decline as non-invitee → 403 | 1 | GWT-25 |
| 14 | PUT decline already declined → 409 | 1 | GWT-28 |
| 15 | DELETE cancel as inviter → 204 | 0 | GWT-32 returns 200, not 204 |
| 16 | DELETE cancel as team owner → 204 | 0 | GWT-31 returns 200, not 204 |
| 17 | DELETE cancel as other user → 403 | 1 | GWT-33 |
| 18 | DELETE cancel already accepted → 409 | 1 | GWT-38 |

**Completeness Score: 16/18**

**Notes:**
- Items 15 & 16: Spec defines DELETE returning 200 with InvitationDto body, but checklist expects 204 (no content). This is a mismatch between checklist expectations and spec implementation. Strict binary scoring = 0.

## Precision

| # | Scenario | Status Code | DTO Name | Field Names | Error Shape | Numeric Values | Vague? | Score |
|---|----------|-------------|----------|-------------|-------------|----------------|--------|-------|
| 1 | GWT-1 | 1 | 1 | 1 | N/A | 1 | No | 4/4 |
| 2 | GWT-2 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 3 | GWT-3 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 4 | GWT-4 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 5 | GWT-5 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 6 | GWT-6 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 7 | GWT-7 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 8 | GWT-8 | 1 | 1 | 1 | N/A | N/A | No | 3/3 |
| 9 | GWT-9 | 1 | 1 | 0 | N/A | N/A | No | 2/3 |
| 10 | GWT-10 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 11 | GWT-11 | 1 | 1 | 1 | N/A | 1 | No | 4/4 |
| 12 | GWT-12 | 1 | 1 | 0 | N/A | 1 | No | 3/4 |
| 13 | GWT-13 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 14 | GWT-14 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 15 | GWT-15 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 16 | GWT-16 | 1 | N/A | N/A | N/A | N/A | No | 1/1 |
| 17 | GWT-17 | 1 | 1 | 1 | N/A | 1 | No | 4/4 |
| 18 | GWT-18 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 19 | GWT-19 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 20 | GWT-20 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 21 | GWT-21 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 22 | GWT-22 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 23 | GWT-23 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 24 | GWT-24 | 1 | 1 | 1 | N/A | 1 | No | 4/4 |
| 25 | GWT-25 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 26 | GWT-26 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 27 | GWT-27 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 28 | GWT-28 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 29 | GWT-29 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 30 | GWT-30 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 31 | GWT-31 | 1 | 1 | 1 | N/A | 1 | No | 4/4 |
| 32 | GWT-32 | 1 | 1 | 0 | N/A | N/A | No | 2/3 |
| 33 | GWT-33 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 34 | GWT-34 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 35 | GWT-35 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 36 | GWT-36 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 37 | GWT-37 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 38 | GWT-38 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 39 | GWT-39 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |
| 40 | GWT-40 | 1 | N/A | N/A | 0 | N/A | No | 1/2 |

**Precision Score: 60/100**

**Precision Breakdown by Sub-Criteria:**
- Status Code: 40/40 (100%) — All explicit numeric codes
- DTO Name: 10/10 scenarios with DTOs (100%) — All use exact `InvitationDto`
- Field Names: 6/10 (60%) — GWT-1, GWT-8, GWT-11, GWT-17, GWT-24, GWT-31 list all fields explicitly; GWT-9, GWT-12, GWT-32 partial
- Error Shape: 0/30 (0%) — No error scenarios specify structure beyond "contains a validation error referencing field X"
- Numeric Values: 5/5 scenarios with numeric constraints (100%) — All use "within 5 seconds", "3 invitations", "2 invitations"

**Notes:**
- Error shape: Validation errors say "contains a validation error referencing field" but don't specify structure (e.g., `{ "errors": { "InviteeEmail": [...] } }`). All 4xx/5xx errors beyond validation lack body specification.
- Field names: Some success scenarios only list changed fields (e.g., GWT-9 only mentions `Status`, not full DTO).

## Token Count

- Characters: 13,909
- Estimated tokens: 13,909 / 4 = 3,477

## Ambiguity

**Ambiguous requirements: None identified**

All GWT scenarios follow unambiguous structure:
- Explicit numeric status codes
- Explicit field value assertions with equality checks
- Boolean conditions (is/is not owner, member, inviter)
- Deterministic state transitions

**Ambiguity count: 0**

---

## Summary

| Metric | Score | Percentage |
|--------|-------|------------|
| Completeness | 16/18 | 89% |
| Precision | 60/100 | 60% |
| Token Count | 3,477 | — |
| Ambiguity | 0 | — |

**Key Strengths:**
- Zero vague language
- All status codes explicit
- All DTO names match contract
- All numeric constraints have exact values
- Zero ambiguous pass/fail criteria

**Key Weaknesses:**
- DELETE endpoint returns 200 + body instead of 204 (checklist mismatch)
- Error response structures not specified (30 error scenarios have no body shape)
- Some success scenarios only document changed fields, not full DTO structure
