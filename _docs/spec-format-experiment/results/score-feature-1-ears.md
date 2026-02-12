# Scoring: Feature 1 EARS Specification

**Spec file:** `R:\Repos\saurun-marketplace\_docs\spec-format-experiment\specs\feature-1-ears.md`

---

## Completeness

| # | Checklist Item | Covered (1/0) | Which requirement covers it |
|---|---------------|--------------|--------------------------|
| 1 | POST happy path → 201 + BookmarkDto | 1 | REQ-4 |
| 2 | POST empty url → 400 | 1 | REQ-5 |
| 3 | POST invalid URL format → 400 | 1 | REQ-6 |
| 4 | POST empty title → 400 | 1 | REQ-7 |
| 5 | POST title > 200 chars → 400 | 1 | REQ-8 |
| 6 | POST description > 1000 chars → 400 | 1 | REQ-9 |
| 7 | POST no auth → 401 | 1 | REQ-1 |
| 8 | GET list happy path → 200 + BookmarkDto[] | 1 | REQ-10 |
| 9 | GET list no auth → 401 | 1 | REQ-1 |
| 10 | GET single happy path → 200 + BookmarkDto | 1 | REQ-13 |
| 11 | GET single not found → 404 | 1 | REQ-14 |
| 12 | GET single other user's bookmark → 403 or 404 | 1 | REQ-2 (returns 404) |
| 13 | GET single no auth → 401 | 1 | REQ-1 |
| 14 | DELETE happy path → 204 | 1 | REQ-15 |
| 15 | DELETE other user's bookmark → 403 or 404 | 1 | REQ-3 (returns 404) |
| 16 | DELETE no auth → 401 | 1 | REQ-1 |

**Completeness Score: 16/16**

---

## Precision

| # | Requirement | Status Code | DTO Name | Field Names | Error Shape | Numeric Values | Vague? | Score |
|---|---------|------------|----------|-------------|-------------|---------------|--------|-------|
| 1 | REQ-1 | 1 | 0 | N/A | 0 | N/A | N | 1/2 |
| 2 | REQ-2 | 1 | 1 | 0 | 0 | N/A | N | 2/3 |
| 3 | REQ-3 | 1 | 1 | 0 | 0 | N/A | N | 2/3 |
| 4 | REQ-4 | 1 | 1 | 0 | 0 | N/A | N | 2/3 |
| 5 | REQ-5 | 1 | 0 | N/A | 0 | N/A | N | 1/2 |
| 6 | REQ-6 | 1 | 0 | N/A | 0 | N/A | N | 1/2 |
| 7 | REQ-7 | 1 | 0 | N/A | 0 | N/A | N | 1/2 |
| 8 | REQ-8 | 1 | 0 | N/A | 0 | 1 | N | 2/3 |
| 9 | REQ-9 | 1 | 0 | N/A | 0 | 1 | N | 2/3 |
| 10 | REQ-10 | 1 | 1 | 1 | 0 | N/A | N | 3/3 |
| 11 | REQ-11 | 0 | 1 | 1 | 0 | N/A | N | 2/3 |
| 12 | REQ-12 | 1 | 1 | 0 | 0 | N/A | N | 2/3 |
| 13 | REQ-13 | 1 | 1 | 0 | 0 | N/A | N | 2/3 |
| 14 | REQ-14 | 1 | 0 | N/A | 0 | N/A | N | 1/2 |
| 15 | REQ-15 | 1 | 0 | N/A | 0 | N/A | N | 1/2 |
| 16 | REQ-16 | 1 | 0 | N/A | 0 | N/A | N | 1/2 |
| 17 | PROP-1 | N/A | 1 | 1 | N/A | 0 | N | 2/3 |
| 18 | PROP-2 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |
| 19 | PROP-3 | N/A | 1 | 1 | N/A | 1 | N | 3/3 |
| 20 | PROP-4 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |
| 21 | PROP-5 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |
| 22 | PROP-6 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |
| 23 | PROP-7 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |
| 24 | PROP-8 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |
| 25 | PROP-9 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |
| 26 | PROP-10 | 1 | 0 | N/A | 0 | N/A | N | 1/2 |
| 27 | PROP-11 | 1 | 1 | 0 | 0 | 1 | N | 3/4 |
| 28 | PROP-12 | 1 | 1 | 0 | 0 | 1 | N | 3/4 |
| 29 | PROP-13 | 1 | 0 | N/A | 0 | 1 | N | 2/3 |
| 30 | PROP-14 | 1 | 1 | 0 | 0 | 1 | N | 3/4 |
| 31 | PROP-15 | 1 | 0 | N/A | 0 | 1 | N | 2/3 |
| 32 | PROP-16 | N/A | 1 | 1 | N/A | N/A | N | 2/2 |
| 33 | PROP-17 | 1 | 1 | 0 | 0 | N/A | N | 2/3 |

### Precision Sub-Criteria Breakdown

**Status Code:** 27/29 applicable (93.1%)
- Missing: REQ-11 (no status code for ordering requirement)
- All others explicit

**DTO Name:** 21/33 (63.6%)
- REQ requirements for errors (REQ-1, 5, 6, 7, 14, 15, 16): No DTO in response
- PROP requirements for errors (PROP-26): No DTO in response
- All success paths specify BookmarkDto

**Field Names:** 14/33 (42.4%)
- Most requirements specify DTO name but not individual fields
- Only REQ-10, REQ-11, PROP-2 through PROP-9, PROP-16 reference specific fields

**Error Shape:** 0/33 (0.0%)
- No requirement specifies validation error structure
- All errors just say "return 400" or "return 404" without body shape

**Numeric Values:** 10/13 applicable (76.9%)
- REQ-8: 200 chars ✓
- REQ-9: 1000 chars ✓
- PROP-3: 5 seconds ✓
- PROP-11: 1 char ✓
- PROP-12: 200 chars ✓
- PROP-13: 201 chars ✓
- PROP-14: 1000 chars ✓
- PROP-15: 1001 chars ✓
- PROP-1: "non-empty GUID" not explicit size ✗
- PROP-9: No explicit numeric value ✗

**Precision Score: 64/86 sub-criteria (74.4%)**

---

## Token Count

- Characters: 6435
- Estimated tokens: 6435 / 4 = **1609**

---

## Ambiguity

**Ambiguous requirements:**

1. **REQ-6: "valid URL format"** — What constitutes validity? Protocol required? Domain format? Localhost allowed? IPv6? This is testable but implementation-dependent.

2. **PROP-1: "non-empty GUID"** — Is `00000000-0000-0000-0000-000000000000` valid? Technically non-empty string but semantically empty GUID.

3. **PROP-3: "within 5 seconds of server's current UTC time"** — Pass/fail depends on server clock. If server clock is wrong, tests could fail incorrectly. Also doesn't specify inclusive/exclusive.

**Ambiguity count: 3**

---

## Summary

| Metric | Score | Notes |
|--------|-------|-------|
| Completeness | 16/16 (100%) | All checklist items covered |
| Precision | 64/86 (74.4%) | Strong on status codes, weak on error shapes |
| Token Count | ~1609 | 6435 characters |
| Ambiguity | 3 | URL validation, empty GUID, time boundary |

### Key Observations

**Strengths:**
- Zero vague language detected
- All numeric constraints explicit
- Full checklist coverage
- Contract types match exactly where referenced
- Status codes always explicit

**Weaknesses:**
- No error response body specification (0% error shape coverage)
- Field-level detail missing for most requirements (42% coverage)
- Some semantic ambiguity in validation rules
- Properties don't always specify which fields they assert on

**Overall Quality:** High precision format with excellent completeness but missing error contract details.
