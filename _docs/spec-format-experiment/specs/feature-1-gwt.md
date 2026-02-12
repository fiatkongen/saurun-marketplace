# Feature 1: Bookmark CRUD — GWT Acceptance Criteria

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

### GWT-1: Create bookmark with all fields

GIVEN an authenticated user with UserId `aaaa-...-0001`
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
  - `Id` is a non-empty GUID
  - `Url` equals `"https://example.com/article"`
  - `Title` equals `"Example Article"`
  - `Description` equals `"An interesting article about testing"`
  - `UserId` equals `aaaa-...-0001`
  - `CreatedAt` is within 5 seconds of UTC now
AND the `Location` header contains `/api/bookmarks/{Id}`

### GWT-2: Create bookmark with optional Description omitted

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

### GWT-3: Validation error — missing Url

GIVEN an authenticated user
WHEN POST `/api/bookmarks` with body:
```json
{
  "Url": "",
  "Title": "No URL"
}
```
THEN response status is `400 Bad Request`
AND response body contains a validation error referencing field `Url`

### GWT-4: Validation error — invalid Url format

GIVEN an authenticated user
WHEN POST `/api/bookmarks` with body:
```json
{
  "Url": "not-a-valid-url",
  "Title": "Bad URL"
}
```
THEN response status is `400 Bad Request`
AND response body contains a validation error referencing field `Url`

### GWT-5: Validation error — missing Title

GIVEN an authenticated user
WHEN POST `/api/bookmarks` with body:
```json
{
  "Url": "https://example.com",
  "Title": ""
}
```
THEN response status is `400 Bad Request`
AND response body contains a validation error referencing field `Title`

### GWT-6: Validation error — Title exceeds 200 characters

GIVEN an authenticated user
WHEN POST `/api/bookmarks` with body:
```json
{
  "Url": "https://example.com",
  "Title": "<string of 201 characters>"
}
```
THEN response status is `400 Bad Request`
AND response body contains a validation error referencing field `Title`

### GWT-7: Validation error — Description exceeds 1000 characters

GIVEN an authenticated user
WHEN POST `/api/bookmarks` with body:
```json
{
  "Url": "https://example.com",
  "Title": "Valid Title",
  "Description": "<string of 1001 characters>"
}
```
THEN response status is `400 Bad Request`
AND response body contains a validation error referencing field `Description`

### GWT-8: Unauthorized — no auth token

GIVEN no authentication credentials
WHEN POST `/api/bookmarks` with body:
```json
{
  "Url": "https://example.com",
  "Title": "Unauthorized Bookmark"
}
```
THEN response status is `401 Unauthorized`
AND response body contains no `BookmarkDto`

---

## GET /api/bookmarks — List User's Bookmarks

### GWT-9: List bookmarks returns user's bookmarks ordered by CreatedAt descending

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND the user has 3 bookmarks with `CreatedAt` values `2025-01-01T10:00:00Z`, `2025-01-02T10:00:00Z`, `2025-01-03T10:00:00Z`
WHEN GET `/api/bookmarks`
THEN response status is `200 OK`
AND response body is an array of 3 `BookmarkDto` objects
AND the first element has `CreatedAt` equal to `2025-01-03T10:00:00Z`
AND the second element has `CreatedAt` equal to `2025-01-02T10:00:00Z`
AND the third element has `CreatedAt` equal to `2025-01-01T10:00:00Z`
AND every element has `UserId` equal to `aaaa-...-0001`

### GWT-10: List bookmarks returns empty array when user has no bookmarks

GIVEN an authenticated user with UserId `aaaa-...-0002`
AND the user has 0 bookmarks
WHEN GET `/api/bookmarks`
THEN response status is `200 OK`
AND response body is an empty array `[]`

### GWT-11: List bookmarks does not include other users' bookmarks

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND user `aaaa-...-0001` has 2 bookmarks
AND user `aaaa-...-0002` has 3 bookmarks
WHEN GET `/api/bookmarks`
THEN response status is `200 OK`
AND response body is an array of 2 `BookmarkDto` objects
AND every element has `UserId` equal to `aaaa-...-0001`

### GWT-12: Unauthorized — no auth token

GIVEN no authentication credentials
WHEN GET `/api/bookmarks`
THEN response status is `401 Unauthorized`

---

## GET /api/bookmarks/{id} — Get Single Bookmark

### GWT-13: Get bookmark by Id returns the bookmark

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND a bookmark exists with `Id` equal to `bbbb-...-0001` owned by user `aaaa-...-0001`
WHEN GET `/api/bookmarks/bbbb-...-0001`
THEN response status is `200 OK`
AND response body is a `BookmarkDto` with:
  - `Id` equals `bbbb-...-0001`
  - `UserId` equals `aaaa-...-0001`
  - `Url`, `Title`, `Description`, `CreatedAt` match the stored bookmark

### GWT-14: Get bookmark owned by another user returns 403

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND a bookmark exists with `Id` equal to `bbbb-...-0002` owned by user `aaaa-...-0002`
WHEN GET `/api/bookmarks/bbbb-...-0002`
THEN response status is `403 Forbidden`
AND response body contains no `BookmarkDto`

### GWT-15: Get nonexistent bookmark returns 404

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND no bookmark exists with `Id` equal to `cccc-...-9999`
WHEN GET `/api/bookmarks/cccc-...-9999`
THEN response status is `404 Not Found`

### GWT-16: Unauthorized — no auth token

GIVEN no authentication credentials
WHEN GET `/api/bookmarks/bbbb-...-0001`
THEN response status is `401 Unauthorized`

---

## DELETE /api/bookmarks/{id} — Delete Bookmark

### GWT-17: Delete own bookmark succeeds

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND a bookmark exists with `Id` equal to `bbbb-...-0001` owned by user `aaaa-...-0001`
WHEN DELETE `/api/bookmarks/bbbb-...-0001`
THEN response status is `204 No Content`
AND subsequent GET `/api/bookmarks/bbbb-...-0001` returns `404 Not Found`

### GWT-18: Delete bookmark owned by another user returns 403

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND a bookmark exists with `Id` equal to `bbbb-...-0002` owned by user `aaaa-...-0002`
WHEN DELETE `/api/bookmarks/bbbb-...-0002`
THEN response status is `403 Forbidden`
AND the bookmark with `Id` `bbbb-...-0002` still exists in the database

### GWT-19: Delete nonexistent bookmark returns 404

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND no bookmark exists with `Id` equal to `cccc-...-9999`
WHEN DELETE `/api/bookmarks/cccc-...-9999`
THEN response status is `404 Not Found`

### GWT-20: Unauthorized — no auth token

GIVEN no authentication credentials
WHEN DELETE `/api/bookmarks/bbbb-...-0001`
THEN response status is `401 Unauthorized`

---

## Quality Checklist

- [x] Every endpoint has a happy path GWT (GWT-1, GWT-9, GWT-13, GWT-17)
- [x] Every POST/PUT has at least one validation error GWT (GWT-3 through GWT-7)
- [x] Every auth-protected endpoint has an unauthorized GWT (GWT-8, GWT-12, GWT-16, GWT-20)
- [x] Every path parameter has a not-found GWT (GWT-15, GWT-19)
- [x] No state transitions apply to this feature (N/A)
- [x] Ownership boundary tested for GET and DELETE (GWT-14, GWT-18)
- [x] List isolation tested — user cannot see other users' bookmarks (GWT-11)
- [x] List ordering tested — CreatedAt descending (GWT-9)
- [x] All field names match contract DTOs: `Id`, `Url`, `Title`, `Description`, `UserId`, `CreatedAt`
- [x] All status codes are explicit: 200, 201, 204, 400, 401, 403, 404
- [x] No vague language used
