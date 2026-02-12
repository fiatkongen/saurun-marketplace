# Feature 1: Bookmark CRUD — GWT Acceptance Criteria (R2)

## Contract Reference

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

---

## POST /api/bookmarks — Create Bookmark

### GWT-1: Create bookmark with all fields populated

GIVEN an authenticated user with UserId `aaaa0000-0000-0000-0000-000000000001`
WHEN POST `/api/bookmarks` with body:
```json
{
  "Url": "https://example.com/article",
  "Title": "Example Article",
  "Description": "An interesting article about testing"
}
```
THEN response status is `201 Created`
AND response body is a `BookmarkDto` with:
  - `Id` is a non-empty GUID (not `00000000-0000-0000-0000-000000000000`)
  - `Url` equals `"https://example.com/article"`
  - `Title` equals `"Example Article"`
  - `Description` equals `"An interesting article about testing"`
  - `UserId` equals `aaaa0000-0000-0000-0000-000000000001`
  - `CreatedAt` is a UTC DateTime within 5 seconds of the current time
AND the `Location` response header equals `/api/bookmarks/{Id}` where `{Id}` matches the returned `BookmarkDto.Id`

### GWT-2: Create bookmark with Description omitted

GIVEN an authenticated user
WHEN POST `/api/bookmarks` with body:
```json
{
  "Url": "https://example.com",
  "Title": "Minimal Bookmark",
  "Description": null
}
```
THEN response status is `201 Created`
AND response body is a `BookmarkDto` with:
  - `Description` is `null`
  - `Url` equals `"https://example.com"`
  - `Title` equals `"Minimal Bookmark"`

### GWT-3: Validation error — Url is empty string

GIVEN an authenticated user
WHEN POST `/api/bookmarks` with body:
```json
{
  "Url": "",
  "Title": "No URL"
}
```
THEN response status is `400 Bad Request`
AND response body is a ProblemDetails object with a `errors` dictionary containing key `"Url"`

### GWT-4: Validation error — Url is not a valid URL format

GIVEN an authenticated user
WHEN POST `/api/bookmarks` with body:
```json
{
  "Url": "not-a-valid-url",
  "Title": "Bad URL"
}
```
THEN response status is `400 Bad Request`
AND response body is a ProblemDetails object with a `errors` dictionary containing key `"Url"`

### GWT-5: Validation error — Title is empty string

GIVEN an authenticated user
WHEN POST `/api/bookmarks` with body:
```json
{
  "Url": "https://example.com",
  "Title": ""
}
```
THEN response status is `400 Bad Request`
AND response body is a ProblemDetails object with a `errors` dictionary containing key `"Title"`

### GWT-6: Validation error — Title exceeds 200 characters

GIVEN an authenticated user
WHEN POST `/api/bookmarks` with body:
```json
{
  "Url": "https://example.com",
  "Title": "A<repeated 201 total characters>"
}
```
THEN response status is `400 Bad Request`
AND response body is a ProblemDetails object with a `errors` dictionary containing key `"Title"`

### GWT-7: Validation error — Title at exactly 200 characters is accepted

GIVEN an authenticated user
WHEN POST `/api/bookmarks` with body:
```json
{
  "Url": "https://example.com",
  "Title": "A<repeated 200 total characters>"
}
```
THEN response status is `201 Created`
AND response body is a `BookmarkDto` with `Title` of length 200

### GWT-8: Validation error — Description exceeds 1000 characters

GIVEN an authenticated user
WHEN POST `/api/bookmarks` with body:
```json
{
  "Url": "https://example.com",
  "Title": "Valid Title",
  "Description": "A<repeated 1001 total characters>"
}
```
THEN response status is `400 Bad Request`
AND response body is a ProblemDetails object with a `errors` dictionary containing key `"Description"`

### GWT-9: Validation error — Description at exactly 1000 characters is accepted

GIVEN an authenticated user
WHEN POST `/api/bookmarks` with body:
```json
{
  "Url": "https://example.com",
  "Title": "Valid Title",
  "Description": "A<repeated 1000 total characters>"
}
```
THEN response status is `201 Created`
AND response body is a `BookmarkDto` with `Description` of length 1000

### GWT-10: Unauthorized — no auth token on create

GIVEN no authentication credentials (no `Authorization` header)
WHEN POST `/api/bookmarks` with body:
```json
{
  "Url": "https://example.com",
  "Title": "Unauthorized Bookmark"
}
```
THEN response status is `401 Unauthorized`
AND response body does not contain a `BookmarkDto`

---

## GET /api/bookmarks — List User's Bookmarks

### GWT-11: List bookmarks returns user's bookmarks ordered by CreatedAt descending

GIVEN an authenticated user with UserId `aaaa0000-0000-0000-0000-000000000001`
AND the user owns 3 bookmarks with `CreatedAt` values:
  - Bookmark A: `2025-01-01T10:00:00Z`
  - Bookmark B: `2025-01-02T10:00:00Z`
  - Bookmark C: `2025-01-03T10:00:00Z`
WHEN GET `/api/bookmarks`
THEN response status is `200 OK`
AND response body is a JSON array of 3 `BookmarkDto` objects
AND element `[0].CreatedAt` equals `2025-01-03T10:00:00Z` (Bookmark C)
AND element `[1].CreatedAt` equals `2025-01-02T10:00:00Z` (Bookmark B)
AND element `[2].CreatedAt` equals `2025-01-01T10:00:00Z` (Bookmark A)
AND every element has `UserId` equal to `aaaa0000-0000-0000-0000-000000000001`

### GWT-12: List bookmarks returns empty array when user has none

GIVEN an authenticated user with UserId `aaaa0000-0000-0000-0000-000000000002`
AND user `aaaa0000-0000-0000-0000-000000000002` owns 0 bookmarks
WHEN GET `/api/bookmarks`
THEN response status is `200 OK`
AND response body is an empty JSON array `[]`

### GWT-13: List bookmarks excludes other users' bookmarks (ownership isolation)

GIVEN an authenticated user with UserId `aaaa0000-0000-0000-0000-000000000001`
AND user `aaaa0000-0000-0000-0000-000000000001` owns 2 bookmarks
AND user `aaaa0000-0000-0000-0000-000000000002` owns 3 bookmarks
WHEN GET `/api/bookmarks`
THEN response status is `200 OK`
AND response body is a JSON array of 2 `BookmarkDto` objects
AND every element has `UserId` equal to `aaaa0000-0000-0000-0000-000000000001`
AND no element has `UserId` equal to `aaaa0000-0000-0000-0000-000000000002`

### GWT-14: Unauthorized — no auth token on list

GIVEN no authentication credentials (no `Authorization` header)
WHEN GET `/api/bookmarks`
THEN response status is `401 Unauthorized`

---

## GET /api/bookmarks/{id} — Get Single Bookmark

### GWT-15: Get own bookmark by Id

GIVEN an authenticated user with UserId `aaaa0000-0000-0000-0000-000000000001`
AND a bookmark exists with:
  - `Id` = `bbbb0000-0000-0000-0000-000000000001`
  - `Url` = `"https://example.com/saved"`
  - `Title` = `"Saved Article"`
  - `Description` = `"My saved article"`
  - `UserId` = `aaaa0000-0000-0000-0000-000000000001`
  - `CreatedAt` = `2025-01-15T12:00:00Z`
WHEN GET `/api/bookmarks/bbbb0000-0000-0000-0000-000000000001`
THEN response status is `200 OK`
AND response body is a `BookmarkDto` with:
  - `Id` equals `bbbb0000-0000-0000-0000-000000000001`
  - `Url` equals `"https://example.com/saved"`
  - `Title` equals `"Saved Article"`
  - `Description` equals `"My saved article"`
  - `UserId` equals `aaaa0000-0000-0000-0000-000000000001`
  - `CreatedAt` equals `2025-01-15T12:00:00Z`

### GWT-16: Get bookmark owned by another user returns 403

GIVEN an authenticated user with UserId `aaaa0000-0000-0000-0000-000000000001`
AND a bookmark exists with `Id` = `bbbb0000-0000-0000-0000-000000000002` and `UserId` = `aaaa0000-0000-0000-0000-000000000002`
WHEN GET `/api/bookmarks/bbbb0000-0000-0000-0000-000000000002`
THEN response status is `403 Forbidden`
AND response body does not contain a `BookmarkDto`

### GWT-17: Get nonexistent bookmark returns 404

GIVEN an authenticated user with UserId `aaaa0000-0000-0000-0000-000000000001`
AND no bookmark exists with `Id` = `cccc0000-0000-0000-0000-000000009999`
WHEN GET `/api/bookmarks/cccc0000-0000-0000-0000-000000009999`
THEN response status is `404 Not Found`

### GWT-18: Unauthorized — no auth token on get

GIVEN no authentication credentials (no `Authorization` header)
WHEN GET `/api/bookmarks/bbbb0000-0000-0000-0000-000000000001`
THEN response status is `401 Unauthorized`

---

## DELETE /api/bookmarks/{id} — Delete Bookmark

### GWT-19: Delete own bookmark succeeds

GIVEN an authenticated user with UserId `aaaa0000-0000-0000-0000-000000000001`
AND a bookmark exists with `Id` = `bbbb0000-0000-0000-0000-000000000001` and `UserId` = `aaaa0000-0000-0000-0000-000000000001`
WHEN DELETE `/api/bookmarks/bbbb0000-0000-0000-0000-000000000001`
THEN response status is `204 No Content`
AND response body is empty
AND a subsequent GET `/api/bookmarks/bbbb0000-0000-0000-0000-000000000001` returns `404 Not Found`

### GWT-20: Delete bookmark owned by another user returns 403

GIVEN an authenticated user with UserId `aaaa0000-0000-0000-0000-000000000001`
AND a bookmark exists with `Id` = `bbbb0000-0000-0000-0000-000000000002` and `UserId` = `aaaa0000-0000-0000-0000-000000000002`
WHEN DELETE `/api/bookmarks/bbbb0000-0000-0000-0000-000000000002`
THEN response status is `403 Forbidden`
AND the bookmark with `Id` = `bbbb0000-0000-0000-0000-000000000002` still exists (verified by GET from the owning user returning `200 OK`)

### GWT-21: Delete nonexistent bookmark returns 404

GIVEN an authenticated user with UserId `aaaa0000-0000-0000-0000-000000000001`
AND no bookmark exists with `Id` = `cccc0000-0000-0000-0000-000000009999`
WHEN DELETE `/api/bookmarks/cccc0000-0000-0000-0000-000000009999`
THEN response status is `404 Not Found`

### GWT-22: Unauthorized — no auth token on delete

GIVEN no authentication credentials (no `Authorization` header)
WHEN DELETE `/api/bookmarks/bbbb0000-0000-0000-0000-000000000001`
THEN response status is `401 Unauthorized`

---

## Quality Checklist

- [x] Every endpoint has a happy path GWT (GWT-1, GWT-11, GWT-15, GWT-19)
- [x] Every POST/PUT has at least one validation error GWT (GWT-3 through GWT-9)
- [x] Every auth-protected endpoint has an unauthorized GWT (GWT-10, GWT-14, GWT-18, GWT-22)
- [x] Every path parameter has a not-found GWT (GWT-17, GWT-21)
- [x] No state transitions in this feature (N/A)
- [x] Ownership boundary tested for GET and DELETE (GWT-16, GWT-20)
- [x] Ownership isolation tested on list (GWT-13)
- [x] List ordering tested — `CreatedAt` descending (GWT-11)
- [x] Boundary values tested — Title at 200 chars accepted (GWT-7), at 201 rejected (GWT-6); Description at 1000 chars accepted (GWT-9), at 1001 rejected (GWT-8)
- [x] All field names match contract DTOs: `Id`, `Url`, `Title`, `Description`, `UserId`, `CreatedAt`
- [x] All request field names match `CreateBookmarkRequest`: `Url`, `Title`, `Description`
- [x] All status codes are explicit numbers: 200, 201, 204, 400, 401, 403, 404
- [x] No vague language used

## Summary

| Endpoint | GWTs | Coverage |
|----------|------|----------|
| POST /api/bookmarks | GWT-1 through GWT-10 | Happy path (all fields), happy path (optional null), 5 validation errors, 2 boundary values, 1 unauthorized |
| GET /api/bookmarks | GWT-11 through GWT-14 | Happy path (ordering), empty result, ownership isolation, 1 unauthorized |
| GET /api/bookmarks/{id} | GWT-15 through GWT-18 | Happy path (all fields verified), ownership 403, not found 404, 1 unauthorized |
| DELETE /api/bookmarks/{id} | GWT-19 through GWT-22 | Happy path (with side-effect verification), ownership 403, not found 404, 1 unauthorized |
| **Total** | **22 GWTs** | |
