# Feature 1: Bookmark CRUD — EARS Specification (R2)

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

### Authentication & Authorization

REQ-1: If a request to POST /api/bookmarks does not include a valid authentication token, then the system shall return 401.

REQ-2: If a request to GET /api/bookmarks does not include a valid authentication token, then the system shall return 401.

REQ-3: If a request to GET /api/bookmarks/{id} does not include a valid authentication token, then the system shall return 401.

REQ-4: If a request to DELETE /api/bookmarks/{id} does not include a valid authentication token, then the system shall return 401.

REQ-5: If an authenticated user sends GET /api/bookmarks/{id} for a bookmark where BookmarkDto.UserId does not match the authenticated user's ID, then the system shall return 404.

REQ-6: If an authenticated user sends DELETE /api/bookmarks/{id} for a bookmark where BookmarkDto.UserId does not match the authenticated user's ID, then the system shall return 404.

### POST /api/bookmarks — Create Bookmark

REQ-7: When an authenticated user sends a valid CreateBookmarkRequest to POST /api/bookmarks, the system shall create a new bookmark and return 201 with a BookmarkDto representing the created bookmark.

REQ-8: If a CreateBookmarkRequest is submitted with a null or empty Url, then the system shall return 400.

REQ-9: If a CreateBookmarkRequest is submitted with a Url that is not a valid URL format, then the system shall return 400.

REQ-10: If a CreateBookmarkRequest is submitted with a null or empty Title, then the system shall return 400.

REQ-11: If a CreateBookmarkRequest is submitted with a Title longer than 200 characters, then the system shall return 400.

REQ-12: If a CreateBookmarkRequest is submitted with a Description longer than 1000 characters, then the system shall return 400.

REQ-13: If a CreateBookmarkRequest is submitted with a whitespace-only Url, then the system shall return 400.

REQ-14: If a CreateBookmarkRequest is submitted with a whitespace-only Title, then the system shall return 400.

### GET /api/bookmarks — List Bookmarks

REQ-15: When an authenticated user sends GET /api/bookmarks, the system shall return 200 with an array of BookmarkDto containing only bookmarks where BookmarkDto.UserId matches the authenticated user's ID.

REQ-16: When an authenticated user sends GET /api/bookmarks, the system shall return the BookmarkDto array ordered by BookmarkDto.CreatedAt descending.

REQ-17: When an authenticated user who has zero bookmarks sends GET /api/bookmarks, the system shall return 200 with an empty array.

### GET /api/bookmarks/{id} — Get Single Bookmark

REQ-18: When an authenticated user sends GET /api/bookmarks/{id} for a bookmark they own, the system shall return 200 with the corresponding BookmarkDto.

REQ-19: If an authenticated user sends GET /api/bookmarks/{id} and no bookmark with that Id exists, then the system shall return 404.

REQ-20: If an authenticated user sends GET /api/bookmarks/{id} with an Id that is not a valid GUID, then the system shall return 400.

### DELETE /api/bookmarks/{id} — Delete Bookmark

REQ-21: When an authenticated user sends DELETE /api/bookmarks/{id} for a bookmark they own, the system shall delete the bookmark and return 204.

REQ-22: If an authenticated user sends DELETE /api/bookmarks/{id} and no bookmark with that Id exists, then the system shall return 404.

REQ-23: If an authenticated user sends DELETE /api/bookmarks/{id} with an Id that is not a valid GUID, then the system shall return 400.

---

## Property Descriptions

### Identity & Ownership

PROP-1: For all BookmarkDto responses, Id must be a non-empty GUID (not equal to 00000000-0000-0000-0000-000000000000).

PROP-2: For all BookmarkDto responses, UserId must equal the authenticated user's ID.

PROP-3: For all POST /api/bookmarks requests that return 201, the returned BookmarkDto.Id must be unique across all previously created bookmarks.

### Timestamps

PROP-4: For all BookmarkDto responses from POST /api/bookmarks, CreatedAt must be within 5 seconds of the server's current UTC time at the moment of creation.

PROP-5: For all BookmarkDto responses, CreatedAt must be a valid UTC DateTime.

PROP-6: For all BookmarkDto responses, CreatedAt must be immutable — retrieving the same bookmark via GET /api/bookmarks/{id} at different times must return the same CreatedAt value.

### Field Integrity

PROP-7: For all valid CreateBookmarkRequest inputs, the returned BookmarkDto.Url must exactly equal the submitted CreateBookmarkRequest.Url.

PROP-8: For all valid CreateBookmarkRequest inputs, the returned BookmarkDto.Title must exactly equal the submitted CreateBookmarkRequest.Title.

PROP-9: For all valid CreateBookmarkRequest inputs where Description is non-null, the returned BookmarkDto.Description must exactly equal the submitted CreateBookmarkRequest.Description.

PROP-10: For all valid CreateBookmarkRequest inputs where Description is null, the returned BookmarkDto.Description must be null.

### Ordering

PROP-11: For all GET /api/bookmarks responses containing two or more items, each BookmarkDto[i].CreatedAt must be greater than or equal to BookmarkDto[i+1].CreatedAt (descending order).

### Idempotency

PROP-12: For all DELETE /api/bookmarks/{id} requests, the first call for an existing owned bookmark must return 204, and all subsequent calls with the same Id must return 404.

### Boundary Constraints

PROP-13: For all CreateBookmarkRequest inputs where Title has length 1, the API must return 201 with a BookmarkDto.

PROP-14: For all CreateBookmarkRequest inputs where Title has length 200, the API must return 201 with a BookmarkDto.

PROP-15: For all CreateBookmarkRequest inputs where Title has length 201, the API must return 400.

PROP-16: For all CreateBookmarkRequest inputs where Description has length 1000, the API must return 201 with a BookmarkDto.

PROP-17: For all CreateBookmarkRequest inputs where Description has length 1001, the API must return 400.

### Isolation

PROP-18: For all GET /api/bookmarks responses, every BookmarkDto.UserId in the array must equal the authenticated user's ID (no cross-user data leakage).

PROP-19: For all GET /api/bookmarks/{id} requests where the bookmark exists but BookmarkDto.UserId differs from the authenticated user's ID, the API must return 404 (not 403).

PROP-20: For all DELETE /api/bookmarks/{id} requests where the bookmark exists but BookmarkDto.UserId differs from the authenticated user's ID, the API must return 404 (not 403).

### Consistency

PROP-21: For all POST /api/bookmarks requests that return 201, the created bookmark must be retrievable via GET /api/bookmarks/{id} using the returned BookmarkDto.Id and must return an identical BookmarkDto.

PROP-22: For all POST /api/bookmarks requests that return 201, the created bookmark must appear in the subsequent GET /api/bookmarks response.

PROP-23: For all DELETE /api/bookmarks/{id} requests that return 204, the deleted bookmark must no longer appear in GET /api/bookmarks responses.

PROP-24: For all DELETE /api/bookmarks/{id} requests that return 204, a subsequent GET /api/bookmarks/{id} with the same Id must return 404.

---

## Quality Checklist

- [x] Every endpoint has at least one Event-driven EARS requirement (REQ-7, REQ-15/16/17, REQ-18, REQ-21)
- [x] Every POST has at least one Unwanted EARS requirement (REQ-8 through REQ-14)
- [x] Every auth-protected endpoint has an Unwanted requirement (REQ-1 through REQ-4)
- [x] Every path parameter has an Unwanted requirement for not found (REQ-19, REQ-22) and invalid format (REQ-20, REQ-23)
- [x] No state transitions applicable (single entity, no lifecycle states)
- [x] Every business rule has a Property or Unwanted requirement
- [x] All field names match BookmarkDto and CreateBookmarkRequest exactly
- [x] All status codes are explicit numbers (201, 200, 204, 400, 401, 404)
- [x] Properties cover cross-cutting invariants (timestamps, IDs, ownership, ordering, consistency)
- [x] No vague language anywhere
