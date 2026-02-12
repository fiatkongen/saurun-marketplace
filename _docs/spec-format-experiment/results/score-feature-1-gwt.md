# Score Report: feature-1-gwt.md

## Completeness
| # | Checklist Item | Covered (1/0) | Which scenario covers it |
|---|---------------|--------------|--------------------------|
| 1 | POST happy path → 201 + BookmarkDto | 1 | GWT-1 |
| 2 | POST empty url → 400 | 1 | GWT-3 |
| 3 | POST invalid URL format → 400 | 1 | GWT-4 |
| 4 | POST empty title → 400 | 1 | GWT-5 |
| 5 | POST title > 200 chars → 400 | 1 | GWT-6 |
| 6 | POST description > 1000 chars → 400 | 1 | GWT-7 |
| 7 | POST no auth → 401 | 1 | GWT-8 |
| 8 | GET list happy path → 200 + BookmarkDto[] | 1 | GWT-9 |
| 9 | GET list no auth → 401 | 1 | GWT-12 |
| 10 | GET single happy path → 200 + BookmarkDto | 1 | GWT-13 |
| 11 | GET single not found → 404 | 1 | GWT-15 |
| 12 | GET single other user's bookmark → 403 or 404 | 1 | GWT-14 |
| 13 | GET single no auth → 401 | 1 | GWT-16 |
| 14 | DELETE happy path → 204 | 1 | GWT-17 |
| 15 | DELETE other user's bookmark → 403 or 404 | 1 | GWT-18 |
| 16 | DELETE no auth → 401 | 1 | GWT-20 |

**Completeness Score: 16/16**

## Precision
| # | Scenario | Status Code | DTO Name | Field Names | Error Shape | Numeric Values | Vague? | Score |
|---|---------|------------|----------|-------------|-------------|---------------|--------|-------|
| 1 | GWT-1: Create bookmark with all fields | 1 | 1 | 1 | N/A | 1 | N | 4/4 |
| 2 | GWT-2: Create bookmark with optional Description omitted | 1 | 1 | 1 | N/A | N/A | N | 3/3 |
| 3 | GWT-3: Validation error — missing Url | 1 | N/A | 1 | 1 | N/A | N | 3/3 |
| 4 | GWT-4: Validation error — invalid Url format | 1 | N/A | 1 | 1 | N/A | N | 3/3 |
| 5 | GWT-5: Validation error — missing Title | 1 | N/A | 1 | 1 | N/A | N | 3/3 |
| 6 | GWT-6: Validation error — Title exceeds 200 characters | 1 | N/A | 1 | 1 | 1 | N | 4/4 |
| 7 | GWT-7: Validation error — Description exceeds 1000 characters | 1 | N/A | 1 | 1 | 1 | N | 4/4 |
| 8 | GWT-8: Unauthorized — no auth token | 1 | 1 | N/A | N/A | N/A | N | 2/2 |
| 9 | GWT-9: List bookmarks returns user's bookmarks ordered by CreatedAt descending | 1 | 1 | 1 | N/A | 1 | N | 4/4 |
| 10 | GWT-10: List bookmarks returns empty array when user has no bookmarks | 1 | 1 | N/A | N/A | 1 | N | 3/3 |
| 11 | GWT-11: List bookmarks does not include other users' bookmarks | 1 | 1 | 1 | N/A | 1 | N | 4/4 |
| 12 | GWT-12: Unauthorized — no auth token | 1 | N/A | N/A | N/A | N/A | N | 1/1 |
| 13 | GWT-13: Get bookmark by Id returns the bookmark | 1 | 1 | 1 | N/A | N/A | N | 3/3 |
| 14 | GWT-14: Get bookmark owned by another user returns 403 | 1 | 1 | N/A | N/A | N/A | N | 2/2 |
| 15 | GWT-15: Get nonexistent bookmark returns 404 | 1 | N/A | N/A | N/A | N/A | N | 1/1 |
| 16 | GWT-16: Unauthorized — no auth token | 1 | N/A | N/A | N/A | N/A | N | 1/1 |
| 17 | GWT-17: Delete own bookmark succeeds | 1 | N/A | N/A | N/A | N/A | N | 1/1 |
| 18 | GWT-18: Delete bookmark owned by another user returns 403 | 1 | N/A | N/A | N/A | N/A | N | 1/1 |
| 19 | GWT-19: Delete nonexistent bookmark returns 404 | 1 | N/A | N/A | N/A | N/A | N | 1/1 |
| 20 | GWT-20: Unauthorized — no auth token | 1 | N/A | N/A | N/A | N/A | N | 1/1 |

**Precision Score: 50/50 (100%)**

### Precision Breakdown
- **Status Code explicit:** 20/20 (every scenario has explicit status code like `201 Created`, `400 Bad Request`)
- **DTO Name matches contract:** 11/11 applicable (uses `BookmarkDto` consistently)
- **Field Names match contract:** 9/9 applicable (uses `Id`, `Url`, `Title`, `Description`, `UserId`, `CreatedAt`)
- **Validation error shape specified:** 5/5 applicable (states "contains a validation error referencing field X")
- **Numeric constraints exact:** 5/5 applicable (201 chars, 1001 chars, 3 bookmarks, 2 bookmarks, 5 seconds)
- **Vague language:** 0 instances (no "appropriate", "some", "proper", "correct", "relevant", "various")

## Token Count
- Characters: 8,536
- Estimated tokens: 8,536 / 4 = 2,134

## Ambiguity
- Ambiguous requirements: None
- Ambiguity count: 0

### Analysis
Every scenario is unambiguous:
- Explicit numeric values for all constraints (200 chars, 1000 chars, 201 chars, 1001 chars)
- Explicit status codes (201, 400, 401, 403, 404, 204, 200)
- Explicit field names matching contract DTOs
- Explicit validation error shape ("contains a validation error referencing field X")
- Explicit ordering behavior ("ordered by CreatedAt descending")
- Explicit isolation requirements ("does not include other users' bookmarks")
- Explicit post-conditions ("subsequent GET returns 404", "still exists in database")

No requirement leaves room for interpretation about pass/fail criteria.

## Summary
- **Completeness:** 16/16 (100%) — All checklist items covered
- **Precision:** 50/50 (100%) — All applicable sub-criteria met
- **Token Cost:** 2,134 estimated tokens
- **Ambiguity:** 0 ambiguous requirements

This spec achieves perfect scores on completeness and precision with zero ambiguity, at a token cost of approximately 2,134 tokens.
