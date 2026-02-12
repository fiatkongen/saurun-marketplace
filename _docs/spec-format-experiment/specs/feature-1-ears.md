# Feature 1: Bookmark CRUD — EARS Specification

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

## EARS Requirements

### Authentication

REQ-1: If a request to any BookmarkRoutes endpoint does not include a valid authentication token, then the system shall return 401.

REQ-2: If an authenticated user requests GET /api/bookmarks/{id} for a bookmark where BookmarkDto.UserId does not match the authenticated user's ID, then the system shall return 404.

REQ-3: If an authenticated user requests DELETE /api/bookmarks/{id} for a bookmark where BookmarkDto.UserId does not match the authenticated user's ID, then the system shall return 404.

### POST /api/bookmarks — Create Bookmark

REQ-4: When an authenticated user sends a valid CreateBookmarkRequest to POST /api/bookmarks, the system shall return 201 with a BookmarkDto representing the created bookmark.

REQ-5: If a CreateBookmarkRequest is submitted with a null or empty Url, then the system shall return 400.

REQ-6: If a CreateBookmarkRequest is submitted with a Url that is not a valid URL format, then the system shall return 400.

REQ-7: If a CreateBookmarkRequest is submitted with a null or empty Title, then the system shall return 400.

REQ-8: If a CreateBookmarkRequest is submitted with a Title longer than 200 characters, then the system shall return 400.

REQ-9: If a CreateBookmarkRequest is submitted with a Description longer than 1000 characters, then the system shall return 400.

### GET /api/bookmarks — List Bookmarks

REQ-10: When an authenticated user sends GET /api/bookmarks, the system shall return 200 with an array of BookmarkDto containing only bookmarks where BookmarkDto.UserId matches the authenticated user's ID.

REQ-11: When an authenticated user sends GET /api/bookmarks, the system shall return the BookmarkDto array ordered by CreatedAt descending.

REQ-12: When an authenticated user with no bookmarks sends GET /api/bookmarks, the system shall return 200 with an empty array.

### GET /api/bookmarks/{id} — Get Single Bookmark

REQ-13: When an authenticated user sends GET /api/bookmarks/{id} for a bookmark they own, the system shall return 200 with the corresponding BookmarkDto.

REQ-14: If an authenticated user sends GET /api/bookmarks/{id} and no bookmark exists with that Id, then the system shall return 404.

### DELETE /api/bookmarks/{id} — Delete Bookmark

REQ-15: When an authenticated user sends DELETE /api/bookmarks/{id} for a bookmark they own, the system shall delete the bookmark and return 204.

REQ-16: If an authenticated user sends DELETE /api/bookmarks/{id} and no bookmark exists with that Id, then the system shall return 404.

---

## Property Descriptions

### Identity & Ownership

PROP-1: For all BookmarkDto responses, Id must be a non-empty GUID.

PROP-2: For all BookmarkDto responses, UserId must equal the authenticated user's ID.

### Timestamps

PROP-3: For all BookmarkDto responses from POST /api/bookmarks, CreatedAt must be within 5 seconds of the server's current UTC time at the moment of creation.

PROP-4: For all BookmarkDto responses, CreatedAt must be a valid UTC DateTime.

### Field Integrity

PROP-5: For all valid CreateBookmarkRequest inputs, the returned BookmarkDto.Url must equal the submitted CreateBookmarkRequest.Url.

PROP-6: For all valid CreateBookmarkRequest inputs, the returned BookmarkDto.Title must equal the submitted CreateBookmarkRequest.Title.

PROP-7: For all valid CreateBookmarkRequest inputs where Description is non-null, the returned BookmarkDto.Description must equal the submitted CreateBookmarkRequest.Description.

PROP-8: For all valid CreateBookmarkRequest inputs where Description is null, the returned BookmarkDto.Description must be null.

### Ordering

PROP-9: For all GET /api/bookmarks responses containing two or more items, each BookmarkDto[i].CreatedAt must be greater than or equal to BookmarkDto[i+1].CreatedAt (descending order).

### Idempotency

PROP-10: For all DELETE /api/bookmarks/{id} requests, the first call for an existing owned bookmark must return 204, and all subsequent calls with the same Id must return 404.

### Boundary Constraints

PROP-11: For all CreateBookmarkRequest inputs where Title has length 1, the API must return 201 with a BookmarkDto.

PROP-12: For all CreateBookmarkRequest inputs where Title has length 200, the API must return 201 with a BookmarkDto.

PROP-13: For all CreateBookmarkRequest inputs where Title has length 201, the API must return 400.

PROP-14: For all CreateBookmarkRequest inputs where Description has length 1000, the API must return 201 with a BookmarkDto.

PROP-15: For all CreateBookmarkRequest inputs where Description has length 1001, the API must return 400.

### Isolation

PROP-16: For all GET /api/bookmarks responses, every BookmarkDto.UserId in the array must equal the authenticated user's ID (no cross-user leakage).

PROP-17: For all GET /api/bookmarks/{id} requests where the bookmark exists but BookmarkDto.UserId differs from the authenticated user's ID, the API must return 404 (not 403).

---

## Quality Checklist

- [x] Every endpoint has at least one Event-driven EARS requirement (REQ-4, REQ-10/11/12, REQ-13, REQ-15)
- [x] Every POST has at least one Unwanted EARS requirement (REQ-5 through REQ-9)
- [x] Every auth-protected endpoint has an Unwanted requirement (REQ-1, REQ-2, REQ-3)
- [x] Every path parameter has an Unwanted requirement for not found (REQ-14, REQ-16)
- [x] No state transitions applicable (single entity, no lifecycle states)
- [x] Every business rule has a Property or Unwanted requirement
- [x] All field names match BookmarkDto and CreateBookmarkRequest exactly
- [x] All status codes are explicit numbers (201, 200, 204, 400, 401, 404)
- [x] Properties cover cross-cutting invariants (timestamps, IDs, ownership, ordering)
- [x] No vague language anywhere
