# Scoring: Feature 1 EARS R2 Spec

## Completeness
| # | Checklist Item | Covered (1/0) | Which requirement covers it |
|---|---------------|--------------|--------------------------|
| 1 | POST happy path → 201 + BookmarkDto | 1 | REQ-7 |
| 2 | POST empty url → 400 | 1 | REQ-8 |
| 3 | POST invalid URL format → 400 | 1 | REQ-9 |
| 4 | POST empty title → 400 | 1 | REQ-10 |
| 5 | POST title > 200 chars → 400 | 1 | REQ-11 |
| 6 | POST description > 1000 chars → 400 | 1 | REQ-12 |
| 7 | POST no auth → 401 | 1 | REQ-1 |
| 8 | GET list happy path → 200 + BookmarkDto[] | 1 | REQ-15 |
| 9 | GET list no auth → 401 | 1 | REQ-2 |
| 10 | GET single happy path → 200 + BookmarkDto | 1 | REQ-18 |
| 11 | GET single not found → 404 | 1 | REQ-19 |
| 12 | GET single other user's bookmark → 403 or 404 | 1 | REQ-5 (returns 404) |
| 13 | GET single no auth → 401 | 1 | REQ-3 |
| 14 | DELETE happy path → 204 | 1 | REQ-21 |
| 15 | DELETE other user's bookmark → 403 or 404 | 1 | REQ-6 (returns 404) |
| 16 | DELETE no auth → 401 | 1 | REQ-4 |

**Completeness Score: 16/16**

## Precision
| # | Requirement | Status Code | DTO Name | Field Names | Error Shape | Numeric Values | Vague? | Score |
|---|---------|------------|----------|-------------|-------------|---------------|--------|-------|
| REQ-1 | POST auth | 1 | 0 | 0 | 0 | 0 | No | 1/5 |
| REQ-2 | GET list auth | 1 | 0 | 0 | 0 | 0 | No | 1/5 |
| REQ-3 | GET single auth | 1 | 0 | 0 | 0 | 0 | No | 1/5 |
| REQ-4 | DELETE auth | 1 | 0 | 0 | 0 | 0 | No | 1/5 |
| REQ-5 | GET other user | 1 | 0 | 1 | 0 | 0 | No | 2/5 |
| REQ-6 | DELETE other user | 1 | 0 | 1 | 0 | 0 | No | 2/5 |
| REQ-7 | POST happy | 1 | 1 | 0 | 0 | 0 | No | 2/5 |
| REQ-8 | POST empty url | 1 | 0 | 0 | 0 | 0 | No | 1/5 |
| REQ-9 | POST invalid url | 1 | 0 | 0 | 0 | 0 | No | 1/5 |
| REQ-10 | POST empty title | 1 | 0 | 0 | 0 | 0 | No | 1/5 |
| REQ-11 | POST title length | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-12 | POST desc length | 1 | 0 | 0 | 0 | 1 | No | 2/5 |
| REQ-13 | POST whitespace url | 1 | 0 | 0 | 0 | 0 | No | 1/5 |
| REQ-14 | POST whitespace title | 1 | 0 | 0 | 0 | 0 | No | 1/5 |
| REQ-15 | GET list happy | 1 | 1 | 1 | 0 | 0 | No | 3/5 |
| REQ-16 | GET list ordering | 1 | 0 | 1 | 0 | 0 | No | 2/5 |
| REQ-17 | GET list empty | 1 | 0 | 0 | 0 | 0 | No | 1/5 |
| REQ-18 | GET single happy | 1 | 1 | 0 | 0 | 0 | No | 2/5 |
| REQ-19 | GET single not found | 1 | 0 | 0 | 0 | 0 | No | 1/5 |
| REQ-20 | GET invalid guid | 1 | 0 | 0 | 0 | 0 | No | 1/5 |
| REQ-21 | DELETE happy | 1 | 0 | 0 | 0 | 0 | No | 1/5 |
| REQ-22 | DELETE not found | 1 | 0 | 0 | 0 | 0 | No | 1/5 |
| REQ-23 | DELETE invalid guid | 1 | 0 | 0 | 0 | 0 | No | 1/5 |
| PROP-1 | Id non-empty GUID | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-2 | UserId matches user | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-3 | Id uniqueness | 1 | 1 | 1 | 0 | 0 | No | 3/5 |
| PROP-4 | CreatedAt window | 0 | 1 | 1 | 0 | 1 | No | 3/5 |
| PROP-5 | CreatedAt UTC valid | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-6 | CreatedAt immutable | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-7 | Url integrity | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-8 | Title integrity | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-9 | Description integrity | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-10 | Description null | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-11 | Array ordering | 1 | 1 | 1 | 0 | 0 | No | 3/5 |
| PROP-12 | DELETE idempotency | 1 | 0 | 0 | 0 | 0 | No | 1/5 |
| PROP-13 | Title min boundary | 1 | 1 | 1 | 0 | 1 | No | 4/5 |
| PROP-14 | Title max boundary | 1 | 1 | 1 | 0 | 1 | No | 4/5 |
| PROP-15 | Title over boundary | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| PROP-16 | Desc max boundary | 1 | 1 | 1 | 0 | 1 | No | 4/5 |
| PROP-17 | Desc over boundary | 1 | 0 | 1 | 0 | 1 | No | 3/5 |
| PROP-18 | List isolation | 0 | 1 | 1 | 0 | 0 | No | 2/5 |
| PROP-19 | GET isolation | 1 | 0 | 1 | 0 | 0 | No | 2/5 |
| PROP-20 | DELETE isolation | 1 | 0 | 1 | 0 | 0 | No | 2/5 |
| PROP-21 | POST→GET consistency | 1 | 1 | 1 | 0 | 0 | No | 3/5 |
| PROP-22 | POST→list consistency | 1 | 1 | 0 | 0 | 0 | No | 2/5 |
| PROP-23 | DELETE→list consistency | 1 | 1 | 0 | 0 | 0 | No | 2/5 |
| PROP-24 | DELETE→GET consistency | 1 | 0 | 0 | 0 | 0 | No | 1/5 |

**Precision Score: 94/230 (40.9%)**

### Precision Analysis

**Status Codes (41/46 = 89.1%)**: Nearly all requirements specify exact numeric status codes (201, 200, 204, 400, 401, 404). Properties don't always specify status codes when they describe response characteristics rather than endpoint behaviors (e.g., PROP-1 through PROP-10, PROP-18).

**DTO Names (27/46 = 58.7%)**: Requirements that return data specify "BookmarkDto" explicitly (REQ-7, REQ-15, REQ-18, all PROP entries dealing with response structure). Auth/error requirements don't specify DTOs.

**Field Names (25/46 = 54.3%)**: Requirements mentioning specific fields use exact contract names (Id, Url, Title, Description, UserId, CreatedAt). Generic requirements don't reference fields.

**Error Shape (0/46 = 0%)**: No requirement specifies validation error response structure beyond status codes. REQ-8 through REQ-14 say "return 400" but don't specify error payload format.

**Numeric Values (10/46 = 21.7%)**: Length constraints (200 chars, 1000 chars, 201 chars, 1001 chars) and time window (5 seconds) are explicit. Most requirements don't involve numeric constraints.

**Vague Language (0/46)**: No instances of "appropriate", "some", "proper", "correct", "relevant", or "various" found.

## Token Count
- Characters: 8705
- Estimated tokens: 8705/4 = 2176

## Ambiguity
- Ambiguous requirements: None
- Ambiguity count: 0

### Analysis

All requirements are testable with binary pass/fail outcomes. Examples:
- REQ-7: Submit valid request, verify 201 + BookmarkDto structure
- REQ-11: Submit title with 201 chars, verify 400
- PROP-4: Create bookmark, verify CreatedAt within 5 seconds of UTC now
- PROP-12: DELETE twice, verify 204 then 404

No requirement requires subjective interpretation to determine pass/fail.
