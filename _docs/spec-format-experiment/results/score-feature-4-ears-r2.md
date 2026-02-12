# Score: Feature 4 EARS R2

## Completeness

| # | Checklist Item | Covered (1/0) | Which requirement covers it |
|---|----------------|---------------|----------------------------|
| 1 | POST create happy path → 201 + CommentDto (status = "Active") | 1 | REQ-10 |
| 2 | POST create with parentId (reply) → 201 | 1 | REQ-10 (handles ParentId), PROP-60 |
| 3 | POST create depth > 3 → 400 | 1 | REQ-16 |
| 4 | POST create empty content → 400 | 1 | REQ-12 |
| 5 | POST create content > 5000 chars → 400 | 1 | REQ-14 |
| 6 | POST create no auth → 401 | 1 | REQ-1 |
| 7 | POST create on nonexistent post → 404 | 1 | REQ-11 |
| 8 | GET list happy path → 200 + CommentDto[] (flagged filtered for non-admins) | 1 | REQ-17, REQ-18 |
| 9 | GET list as admin → 200 + includes flagged comments | 1 | REQ-19 |
| 10 | PUT edit happy path → 200 + CommentDto (status = "Edited", editCount incremented) | 1 | REQ-22 |
| 11 | PUT edit no auth → 401 | 1 | REQ-2 |
| 12 | PUT edit by non-author → 403 | 1 | REQ-7 |
| 13 | PUT edit after 3 edits → 400 or 409 | 1 | REQ-27 (returns 409) |
| 14 | PUT edit after 24 hours → 400 or 409 | 1 | REQ-28 (returns 409) |
| 15 | PUT edit deleted comment → 400 or 409 | 1 | REQ-30 (returns 409) |
| 16 | PUT edit flagged comment → 400 or 409 | 1 | REQ-29 (returns 409) |
| 17 | DELETE happy path → 204 (status → "Deleted") | 0 | REQ-33 returns 200, not 204 |
| 18 | DELETE no auth → 401 | 1 | REQ-3 |
| 19 | DELETE by non-author → 403 | 1 | REQ-8 |
| 20 | DELETE already deleted → 404 or 409 | 1 | REQ-36 (returns 409) |
| 21 | PUT flag happy path → 200 (status → "Flagged") | 1 | REQ-39 |
| 22 | PUT flag no auth → 401 | 1 | REQ-4 |
| 23 | PUT flag own comment → 400 or 403 | 1 | REQ-9 (returns 403) |
| 24 | PUT flag deleted comment → 400 or 409 | 1 | REQ-42 (returns 409) |
| 25 | PUT moderate approve → 200 (status → "Approved") | 1 | REQ-45 |
| 26 | PUT moderate remove → 200 (status → "Removed") | 1 | REQ-46 |
| 27 | PUT moderate no auth → 401 | 1 | REQ-5 |
| 28 | PUT moderate by non-admin → 403 | 1 | REQ-6 |

**Completeness Score: 27/28**

Note: Item #17 expects 204 but spec defines 200 with CommentDto. This is a design choice difference, not missing coverage.

## Precision

| # | Requirement | Status Code | DTO Name | Field Names | Error Shape | Numeric Values | Vague? | Score |
|---|-------------|-------------|----------|-------------|-------------|----------------|--------|-------|
| REQ-1 | POST no auth | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-2 | PUT edit no auth | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-3 | DELETE no auth | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-4 | PUT flag no auth | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-5 | PUT moderate no auth | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-6 | Moderate non-admin | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-7 | Edit non-author | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-8 | Delete non-author | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-9 | Flag own comment | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-10 | POST create happy | 1 | 1 | 1 | 1 | 1 | No | 5/5 |
| REQ-11 | POST nonexistent post | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-12 | Content null/empty | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-13 | Content whitespace | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-14 | Content > 5000 | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-15 | Invalid ParentId | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-16 | Depth > 3 | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-17 | GET unauth | 1 | 1 | 1 | 1 | 1 | No | 5/5 |
| REQ-18 | GET non-admin | 1 | 1 | 1 | 1 | 1 | No | 5/5 |
| REQ-19 | GET admin | 1 | 1 | 1 | 1 | 1 | No | 5/5 |
| REQ-20 | GET nonexistent post | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-21 | GET empty list | 1 | 0 | 0 | 1 | 1 | No | 3/5 |
| REQ-22 | PUT edit happy | 1 | 1 | 1 | 1 | 1 | No | 5/5 |
| REQ-23 | PUT edit nonexistent | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-24 | Update null/empty | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-25 | Update whitespace | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-26 | Update > 5000 | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-27 | EditCount = 3 | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-28 | > 24 hours | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-29 | Edit flagged | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-30 | Edit deleted | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-31 | Edit approved | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-32 | Edit removed | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-33 | DELETE happy | 1 | 1 | 1 | 1 | 1 | No | 5/5 |
| REQ-34 | DELETE nonexistent | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-35 | Delete flagged | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-36 | Delete deleted | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-37 | Delete approved | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-38 | Delete removed | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-39 | Flag happy | 1 | 1 | 1 | 1 | 1 | No | 5/5 |
| REQ-40 | Flag nonexistent | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-41 | Flag flagged | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-42 | Flag deleted | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-43 | Flag approved | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-44 | Flag removed | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-45 | Moderate approve | 1 | 1 | 1 | 1 | 1 | No | 5/5 |
| REQ-46 | Moderate remove | 1 | 1 | 1 | 1 | 1 | No | 5/5 |
| REQ-47 | Moderate nonexistent | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-48 | Invalid Decision | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-49 | Decision null/empty | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-50 | Moderate Active | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-51 | Moderate Edited | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-52 | Moderate Deleted | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-53 | Moderate Approved | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| REQ-54 | Moderate Removed | 1 | 0 | 1 | 0 | 1 | No | 3/5 |

**REQ Precision Score: 158/270 (58.5%)**

### PROP Requirements

| # | Requirement | Status Code | DTO Name | Field Names | Error Shape | Numeric Values | Vague? | Score |
|---|-------------|-------------|----------|-------------|-------------|----------------|--------|-------|
| PROP-1 | Id non-empty GUID | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-2 | Id unique | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-3 | PostId exists | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-4 | AuthorId match | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-5 | ParentId ref integrity | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-6 | Id immutable | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-7 | Status enum | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-8-13 | State transitions | 0 | 0 | 1 | 0 | 0 | No | 1/5 each (6 total) |
| PROP-14 | CreatedAt UTC | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-15 | CreatedAt within 5s | 0 | 1 | 1 | 0 | 1 | No | 3/5 |
| PROP-16 | CreatedAt immutable | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-17 | Active EditedAt null | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-18 | EditedAt > CreatedAt | 0 | 1 | 1 | 0 | 1 | No | 3/5 |
| PROP-19 | EditedAt monotonic | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-20 | EditedAt never null | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-21 | EditCount 0-3 | 0 | 1 | 1 | 0 | 1 | No | 3/5 |
| PROP-22 | Active EditCount 0 | 0 | 1 | 1 | 0 | 1 | No | 3/5 |
| PROP-23 | EditCount increment | 0 | 1 | 1 | 0 | 1 | No | 3/5 |
| PROP-24 | EditCount monotonic | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-25 | EditCount edit-only | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-26 | Content 1-5000 | 0 | 1 | 1 | 0 | 1 | No | 3/5 |
| PROP-27 | Create Content match | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-28 | Update Content match | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-29 | Content edit-only | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-30 | Depth max 3 | 0 | 1 | 1 | 0 | 1 | No | 3/5 |
| PROP-31 | Root null parent | 0 | 1 | 1 | 0 | 1 | No | 3/5 |
| PROP-32 | ParentId immutable | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-33 | Unauth no flag/remove | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-34 | Non-admin no flag/remove | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-35 | Admin see all | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-36 | PostId filter | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-37 | Edit authz | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-38 | Delete authz | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-39 | Flag authz | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-40 | Moderate authz | 0 | 0 | 0 | 0 | 0 | No | 0/5 |
| PROP-41 | Edit 24h block | 0 | 0 | 1 | 0 | 1 | No | 2/5 |
| PROP-42 | Edit 24h allow | 0 | 0 | 1 | 0 | 1 | No | 2/5 |
| PROP-43 | Create len=1 | 0 | 1 | 1 | 0 | 1 | No | 3/5 |
| PROP-44 | Create len=5000 | 0 | 1 | 1 | 0 | 1 | No | 3/5 |
| PROP-45 | Create len=5001 | 0 | 0 | 1 | 0 | 1 | No | 2/5 |
| PROP-46 | Update len=1 | 0 | 0 | 1 | 0 | 1 | No | 2/5 |
| PROP-47 | Update len=5000 | 0 | 0 | 1 | 0 | 1 | No | 2/5 |
| PROP-48 | Update len=5001 | 0 | 0 | 1 | 0 | 1 | No | 2/5 |
| PROP-49 | EditCount=2 allow | 0 | 1 | 1 | 0 | 1 | No | 3/5 |
| PROP-50 | EditCount=3 block | 0 | 0 | 1 | 0 | 1 | No | 2/5 |
| PROP-51 | Depth=2 allow | 0 | 1 | 1 | 0 | 1 | No | 3/5 |
| PROP-52 | Depth=3 block | 0 | 0 | 1 | 0 | 1 | No | 2/5 |
| PROP-53 | Delete idempotency | 0 | 0 | 1 | 0 | 0 | No | 1/5 |
| PROP-54 | Flag idempotency | 0 | 0 | 1 | 0 | 0 | No | 1/5 |
| PROP-55 | Create Status Active | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-56 | Create EditCount 0 | 0 | 1 | 1 | 0 | 1 | No | 3/5 |
| PROP-57 | Create EditedAt null | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-58 | Create PostId match | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-59 | Create AuthorId match | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-60 | Create ParentId non-null | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-61 | Create ParentId null | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-62 | Edit Status Edited | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-63 | Edit EditedAt set | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-64 | Delete Status Deleted | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-65 | Flag Status Flagged | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-66 | Moderate approve | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-67 | Moderate remove | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-68 | Decision values | 0 | 0 | 1 | 0 | 0 | No | 1/5 |
| PROP-69 | PostId immutable | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-70 | AuthorId immutable | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-71 | ParentId immutable | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-72 | Create consistency | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-73 | Delete visibility | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-74 | Flag visibility | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-75 | Moderate remove visibility | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-76 | Moderate approve visibility | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-77 | Flag side-effects | 0 | 0 | 1 | 0 | 0 | No | 1/5 |
| PROP-78 | Delete side-effects | 0 | 0 | 1 | 0 | 0 | No | 1/5 |
| PROP-79 | Moderate side-effects | 0 | 0 | 1 | 0 | 0 | No | 1/5 |

**PROP Precision Score: 156/395 (39.5%)**

**Total Precision Score: 314/665 (47.2%)**

## Token Count

- Characters: 22290
- Estimated tokens: 22290/4 = 5572.5 ≈ 5573

## Ambiguity

**Ambiguous requirements: None**

The spec is completely unambiguous. Every requirement has explicit pass/fail criteria:
- All status codes are exact numbers (200, 201, 400, 401, 403, 404, 409)
- All constraints have exact numeric values (5000 chars, 3 edits, 24 hours, depth 3, EditCount 0-3)
- All status transitions are explicitly enumerated with exact string values
- All field names match contract types exactly
- All conditional logic uses precise boolean conditions

**Ambiguity count: 0**

## Summary

**Completeness**: 27/28 (96.4%) - Only difference: DELETE returns 200 with CommentDto instead of 204 empty body.

**Precision**: 314/665 (47.2%)
- REQ precision: 158/270 (58.5%)
- PROP precision: 156/395 (39.5%)

**Token Count**: 5573 tokens

**Ambiguity**: 0 - No ambiguous requirements

### Precision Analysis

The spec loses precision points primarily on error responses:
- **Status codes**: All present (54/54 = 100%)
- **DTO names**: Only present in success cases (8 REQs + limited PROPs = low coverage)
- **Field names**: High coverage (all requirements reference specific fields)
- **Error shape**: Never specified (returns "400" not "400 with ValidationError")
- **Numeric values**: High coverage (all constraints have exact values)

Error responses simply say "return 400" or "return 409" without specifying response body structure. This is the primary precision loss area.
