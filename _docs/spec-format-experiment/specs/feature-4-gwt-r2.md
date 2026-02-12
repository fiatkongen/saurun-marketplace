# Feature 4: Comment Thread with Moderation — GWT Acceptance Criteria (R2)

## Contract Reference

```csharp
public record CommentDto(Guid Id, Guid PostId, Guid AuthorId, Guid? ParentId, string Content, string Status, int EditCount, DateTime CreatedAt, DateTime? EditedAt);
public record CreateCommentRequest(string Content, Guid? ParentId);
public record UpdateCommentRequest(string Content);
public record ModerateCommentRequest(string Decision); // "approve" or "remove"

public static class CommentRoutes
{
    public const string Create = "/api/posts/{postId}/comments";
    public const string List = "/api/posts/{postId}/comments";
    public const string Update = "/api/comments/{id}";
    public const string Delete = "/api/comments/{id}";
    public const string Flag = "/api/comments/{id}/flag";
    public const string Moderate = "/api/comments/{id}/moderate";
}
```

## State Machine Reference

```
Active → Edited       (on PUT /api/comments/{id})
Active → Flagged      (on PUT /api/comments/{id}/flag)
Active → Deleted      (on DELETE /api/comments/{id})
Edited → Flagged      (on PUT /api/comments/{id}/flag)
Edited → Deleted      (on DELETE /api/comments/{id})
Flagged → Approved    (on PUT /api/comments/{id}/moderate with Decision "approve")
Flagged → Removed     (on PUT /api/comments/{id}/moderate with Decision "remove")
```

All other transitions are invalid (e.g., Deleted → Active, Approved → Edited, Removed → anything).

---

## POST /api/posts/{postId}/comments — Create Comment

### GWT-1: Create top-level comment

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a post exists with Id `pppp0000-0000-0000-0000-000000000001`
WHEN POST `/api/posts/pppp0000-0000-0000-0000-000000000001/comments` with body:
```json
{
  "Content": "This is a great post!",
  "ParentId": null
}
```
THEN response status is `201 Created`
AND response body is a `CommentDto` with:
  - `Id` is a non-empty GUID (not `00000000-0000-0000-0000-000000000000`)
  - `PostId` equals `pppp0000-0000-0000-0000-000000000001`
  - `AuthorId` equals `aaaa0000-0000-0000-0000-000000000001`
  - `ParentId` is `null`
  - `Content` equals `"This is a great post!"`
  - `Status` equals `"Active"`
  - `EditCount` equals `0`
  - `CreatedAt` is a UTC DateTime within 5 seconds of the current time
  - `EditedAt` is `null`

### GWT-2: Create nested reply (depth 1)

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000002`
AND a post exists with Id `pppp0000-0000-0000-0000-000000000001`
AND a comment exists with Id `cccc0000-0000-0000-0000-000000000001` on post `pppp0000-0000-0000-0000-000000000001` with `ParentId` `null` (depth 0)
WHEN POST `/api/posts/pppp0000-0000-0000-0000-000000000001/comments` with body:
```json
{
  "Content": "I agree with you!",
  "ParentId": "cccc0000-0000-0000-0000-000000000001"
}
```
THEN response status is `201 Created`
AND response body is a `CommentDto` with:
  - `ParentId` equals `cccc0000-0000-0000-0000-000000000001`
  - `Content` equals `"I agree with you!"`
  - `Status` equals `"Active"`
  - `EditCount` equals `0`

### GWT-3: Create nested reply at max depth (depth 3)

GIVEN an authenticated user
AND a post exists with Id `pppp0000-0000-0000-0000-000000000001`
AND a comment chain exists:
  - `cccc0000-0000-0000-0000-000000000001` (depth 0, `ParentId` `null`)
  - `cccc0000-0000-0000-0000-000000000002` (depth 1, `ParentId` `cccc0000-0000-0000-0000-000000000001`)
  - `cccc0000-0000-0000-0000-000000000003` (depth 2, `ParentId` `cccc0000-0000-0000-0000-000000000002`)
WHEN POST `/api/posts/pppp0000-0000-0000-0000-000000000001/comments` with body:
```json
{
  "Content": "Deepest allowed reply",
  "ParentId": "cccc0000-0000-0000-0000-000000000003"
}
```
THEN response status is `201 Created`
AND response body is a `CommentDto` with:
  - `ParentId` equals `cccc0000-0000-0000-0000-000000000003`
  - `Status` equals `"Active"`

### GWT-4: Reject nested reply exceeding max depth 3

GIVEN an authenticated user
AND a post exists with Id `pppp0000-0000-0000-0000-000000000001`
AND a comment chain exists:
  - `cccc0000-0000-0000-0000-000000000001` (depth 0)
  - `cccc0000-0000-0000-0000-000000000002` (depth 1)
  - `cccc0000-0000-0000-0000-000000000003` (depth 2)
  - `cccc0000-0000-0000-0000-000000000004` (depth 3)
WHEN POST `/api/posts/pppp0000-0000-0000-0000-000000000001/comments` with body:
```json
{
  "Content": "Too deep",
  "ParentId": "cccc0000-0000-0000-0000-000000000004"
}
```
THEN response status is `400 Bad Request`
AND response body is a ProblemDetails object with `errors` dictionary containing key `"ParentId"` indicating maximum nesting depth of 3 exceeded

### GWT-5: Content at exactly 5000 characters is accepted

GIVEN an authenticated user
AND a post exists with Id `pppp0000-0000-0000-0000-000000000001`
WHEN POST `/api/posts/pppp0000-0000-0000-0000-000000000001/comments` with body:
```json
{
  "Content": "A<repeated 5000 total characters>",
  "ParentId": null
}
```
THEN response status is `201 Created`
AND response body is a `CommentDto` with `Content` of length 5000

### GWT-6: Validation error — Content exceeds 5000 characters

GIVEN an authenticated user
AND a post exists with Id `pppp0000-0000-0000-0000-000000000001`
WHEN POST `/api/posts/pppp0000-0000-0000-0000-000000000001/comments` with body:
```json
{
  "Content": "A<repeated 5001 total characters>",
  "ParentId": null
}
```
THEN response status is `400 Bad Request`
AND response body is a ProblemDetails object with `errors` dictionary containing key `"Content"`

### GWT-7: Validation error — Content is empty string

GIVEN an authenticated user
AND a post exists with Id `pppp0000-0000-0000-0000-000000000001`
WHEN POST `/api/posts/pppp0000-0000-0000-0000-000000000001/comments` with body:
```json
{
  "Content": "",
  "ParentId": null
}
```
THEN response status is `400 Bad Request`
AND response body is a ProblemDetails object with `errors` dictionary containing key `"Content"`

### GWT-8: Content at exactly 1 character is accepted

GIVEN an authenticated user
AND a post exists with Id `pppp0000-0000-0000-0000-000000000001`
WHEN POST `/api/posts/pppp0000-0000-0000-0000-000000000001/comments` with body:
```json
{
  "Content": "A",
  "ParentId": null
}
```
THEN response status is `201 Created`
AND response body is a `CommentDto` with `Content` equals `"A"`

### GWT-9: Not found — nonexistent postId

GIVEN an authenticated user
AND no post exists with Id `pppp0000-0000-0000-0000-000000009999`
WHEN POST `/api/posts/pppp0000-0000-0000-0000-000000009999/comments` with body:
```json
{
  "Content": "Comment on ghost post",
  "ParentId": null
}
```
THEN response status is `404 Not Found`

### GWT-10: Not found — ParentId references nonexistent comment

GIVEN an authenticated user
AND a post exists with Id `pppp0000-0000-0000-0000-000000000001`
AND no comment exists with Id `cccc0000-0000-0000-0000-000000009999`
WHEN POST `/api/posts/pppp0000-0000-0000-0000-000000000001/comments` with body:
```json
{
  "Content": "Reply to nothing",
  "ParentId": "cccc0000-0000-0000-0000-000000009999"
}
```
THEN response status is `404 Not Found`

### GWT-11: Unauthorized — no auth token

GIVEN no authentication credentials (no `Authorization` header)
WHEN POST `/api/posts/pppp0000-0000-0000-0000-000000000001/comments` with body:
```json
{
  "Content": "Anonymous comment",
  "ParentId": null
}
```
THEN response status is `401 Unauthorized`

---

## GET /api/posts/{postId}/comments — List Comments on Post

### GWT-12: List comments returns Active, Edited, and Approved comments for non-admin user

GIVEN an authenticated non-admin user
AND a post exists with Id `pppp0000-0000-0000-0000-000000000001`
AND 5 comments exist on post `pppp0000-0000-0000-0000-000000000001`:
  - `cccc0000-0000-0000-0000-000000000001` with `Status` `"Active"`
  - `cccc0000-0000-0000-0000-000000000002` with `Status` `"Edited"`
  - `cccc0000-0000-0000-0000-000000000003` with `Status` `"Flagged"`
  - `cccc0000-0000-0000-0000-000000000004` with `Status` `"Approved"`
  - `cccc0000-0000-0000-0000-000000000005` with `Status` `"Deleted"`
WHEN GET `/api/posts/pppp0000-0000-0000-0000-000000000001/comments`
THEN response status is `200 OK`
AND response body is a JSON array of 3 `CommentDto` objects
AND the array contains comments with Ids `cccc0000-0000-0000-0000-000000000001`, `cccc0000-0000-0000-0000-000000000002`, and `cccc0000-0000-0000-0000-000000000004`
AND the array does NOT contain comments with Ids `cccc0000-0000-0000-0000-000000000003` or `cccc0000-0000-0000-0000-000000000005`

### GWT-13: List comments includes Flagged comments for admin user

GIVEN an authenticated admin user
AND a post exists with Id `pppp0000-0000-0000-0000-000000000001`
AND 4 comments exist on post `pppp0000-0000-0000-0000-000000000001`:
  - `cccc0000-0000-0000-0000-000000000001` with `Status` `"Active"`
  - `cccc0000-0000-0000-0000-000000000002` with `Status` `"Edited"`
  - `cccc0000-0000-0000-0000-000000000003` with `Status` `"Flagged"`
  - `cccc0000-0000-0000-0000-000000000004` with `Status` `"Approved"`
WHEN GET `/api/posts/pppp0000-0000-0000-0000-000000000001/comments`
THEN response status is `200 OK`
AND response body is a JSON array of 4 `CommentDto` objects
AND the array contains comments with Ids `cccc0000-0000-0000-0000-000000000001`, `cccc0000-0000-0000-0000-000000000002`, `cccc0000-0000-0000-0000-000000000003`, and `cccc0000-0000-0000-0000-000000000004`

### GWT-14: List comments excludes Deleted and Removed comments for all users including admins

GIVEN an authenticated admin user
AND a post exists with Id `pppp0000-0000-0000-0000-000000000001`
AND 4 comments exist on post `pppp0000-0000-0000-0000-000000000001`:
  - `cccc0000-0000-0000-0000-000000000001` with `Status` `"Active"`
  - `cccc0000-0000-0000-0000-000000000002` with `Status` `"Deleted"`
  - `cccc0000-0000-0000-0000-000000000003` with `Status` `"Removed"`
  - `cccc0000-0000-0000-0000-000000000004` with `Status` `"Approved"`
WHEN GET `/api/posts/pppp0000-0000-0000-0000-000000000001/comments`
THEN response status is `200 OK`
AND response body is a JSON array of 2 `CommentDto` objects
AND the array contains comments with Ids `cccc0000-0000-0000-0000-000000000001` and `cccc0000-0000-0000-0000-000000000004`
AND the array does NOT contain comments with Ids `cccc0000-0000-0000-0000-000000000002` or `cccc0000-0000-0000-0000-000000000003`

### GWT-15: List comments returns empty array when post has no comments

GIVEN a post exists with Id `pppp0000-0000-0000-0000-000000000001`
AND the post has 0 comments
WHEN GET `/api/posts/pppp0000-0000-0000-0000-000000000001/comments`
THEN response status is `200 OK`
AND response body is an empty JSON array `[]`

### GWT-16: Public access — Flagged comments hidden from unauthenticated users

GIVEN no authentication credentials (no `Authorization` header)
AND a post exists with Id `pppp0000-0000-0000-0000-000000000001`
AND 3 comments exist on post `pppp0000-0000-0000-0000-000000000001`:
  - `cccc0000-0000-0000-0000-000000000001` with `Status` `"Active"`
  - `cccc0000-0000-0000-0000-000000000002` with `Status` `"Flagged"`
  - `cccc0000-0000-0000-0000-000000000003` with `Status` `"Approved"`
WHEN GET `/api/posts/pppp0000-0000-0000-0000-000000000001/comments`
THEN response status is `200 OK`
AND response body is a JSON array of 2 `CommentDto` objects
AND the array contains comments with Ids `cccc0000-0000-0000-0000-000000000001` and `cccc0000-0000-0000-0000-000000000003`
AND the array does NOT contain comment with Id `cccc0000-0000-0000-0000-000000000002`

### GWT-17: Not found — nonexistent postId

GIVEN no post exists with Id `pppp0000-0000-0000-0000-000000009999`
WHEN GET `/api/posts/pppp0000-0000-0000-0000-000000009999/comments`
THEN response status is `404 Not Found`

---

## PUT /api/comments/{id} — Edit Comment

### GWT-18: Edit own Active comment — happy path (Active -> Edited)

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with:
  - `Id` = `cccc0000-0000-0000-0000-000000000001`
  - `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`
  - `Status` = `"Active"`
  - `EditCount` = `0`
  - `CreatedAt` within the last 24 hours
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001` with body:
```json
{
  "Content": "Updated comment content"
}
```
THEN response status is `200 OK`
AND response body is a `CommentDto` with:
  - `Id` equals `cccc0000-0000-0000-0000-000000000001`
  - `Content` equals `"Updated comment content"`
  - `Status` equals `"Edited"`
  - `EditCount` equals `1`
  - `EditedAt` is a UTC DateTime within 5 seconds of the current time

### GWT-19: Edit own Edited comment — third edit at boundary (EditCount 2 -> 3)

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with:
  - `Id` = `cccc0000-0000-0000-0000-000000000001`
  - `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`
  - `Status` = `"Edited"`
  - `EditCount` = `2`
  - `CreatedAt` within the last 24 hours
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001` with body:
```json
{
  "Content": "Third and final edit"
}
```
THEN response status is `200 OK`
AND response body is a `CommentDto` with:
  - `Content` equals `"Third and final edit"`
  - `Status` equals `"Edited"`
  - `EditCount` equals `3`
  - `EditedAt` is a UTC DateTime within 5 seconds of the current time

### GWT-20: Reject edit when EditCount already at max (3)

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with:
  - `Id` = `cccc0000-0000-0000-0000-000000000001`
  - `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`
  - `Status` = `"Edited"`
  - `EditCount` = `3`
  - `CreatedAt` within the last 24 hours
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001` with body:
```json
{
  "Content": "One edit too many"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating maximum edit count of 3 reached
AND the comment retains `EditCount` `3` and `Content` unchanged

### GWT-21: Reject edit when comment is older than 24 hours

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with:
  - `Id` = `cccc0000-0000-0000-0000-000000000001`
  - `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`
  - `Status` = `"Active"`
  - `EditCount` = `0`
  - `CreatedAt` = `2025-01-01T00:00:00Z` (more than 24 hours ago)
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001` with body:
```json
{
  "Content": "Too late to edit"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating the 24-hour edit window has expired

### GWT-22: Edit at exactly 24 hours minus 1 second is accepted

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with:
  - `Id` = `cccc0000-0000-0000-0000-000000000001`
  - `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`
  - `Status` = `"Active"`
  - `EditCount` = `0`
  - `CreatedAt` = 23 hours and 59 minutes ago
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001` with body:
```json
{
  "Content": "Just in time edit"
}
```
THEN response status is `200 OK`
AND response body is a `CommentDto` with:
  - `Status` equals `"Edited"`
  - `EditCount` equals `1`

### GWT-23: Invalid state transition — edit a Flagged comment

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Flagged"`, `CreatedAt` within the last 24 hours
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001` with body:
```json
{
  "Content": "Trying to edit flagged comment"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating that editing a comment with `Status` `"Flagged"` is not allowed

### GWT-24: Invalid state transition — edit a Deleted comment

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Deleted"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001` with body:
```json
{
  "Content": "Trying to edit deleted comment"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating that editing a comment with `Status` `"Deleted"` is not allowed

### GWT-25: Invalid state transition — edit an Approved comment

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Approved"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001` with body:
```json
{
  "Content": "Trying to edit approved comment"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating that editing a comment with `Status` `"Approved"` is not allowed

### GWT-26: Invalid state transition — edit a Removed comment

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Removed"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001` with body:
```json
{
  "Content": "Trying to edit removed comment"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating that editing a comment with `Status` `"Removed"` is not allowed

### GWT-27: Validation error — empty Content on edit

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Active"`, `CreatedAt` within the last 24 hours
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001` with body:
```json
{
  "Content": ""
}
```
THEN response status is `400 Bad Request`
AND response body is a ProblemDetails object with `errors` dictionary containing key `"Content"`

### GWT-28: Validation error — Content exceeds 5000 characters on edit

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Active"`, `CreatedAt` within the last 24 hours
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001` with body:
```json
{
  "Content": "A<repeated 5001 total characters>"
}
```
THEN response status is `400 Bad Request`
AND response body is a ProblemDetails object with `errors` dictionary containing key `"Content"`

### GWT-29: Ownership boundary — edit another user's comment

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000002`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000002`, `Status` = `"Active"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000002` with body:
```json
{
  "Content": "Hijacking your comment"
}
```
THEN response status is `403 Forbidden`
AND the comment with Id `cccc0000-0000-0000-0000-000000000002` retains its original `Content` unchanged

### GWT-30: Not found — nonexistent comment Id

GIVEN an authenticated user
AND no comment exists with Id `cccc0000-0000-0000-0000-000000009999`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000009999` with body:
```json
{
  "Content": "Editing a ghost"
}
```
THEN response status is `404 Not Found`

### GWT-31: Unauthorized — no auth token

GIVEN no authentication credentials (no `Authorization` header)
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001` with body:
```json
{
  "Content": "Anonymous edit"
}
```
THEN response status is `401 Unauthorized`

---

## DELETE /api/comments/{id} — Delete Comment (Soft)

### GWT-32: Delete own Active comment (Active -> Deleted)

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Active"`
WHEN DELETE `/api/comments/cccc0000-0000-0000-0000-000000000001`
THEN response status is `204 No Content`
AND response body is empty
AND the comment with Id `cccc0000-0000-0000-0000-000000000001` has `Status` `"Deleted"` in the database
AND subsequent GET `/api/posts/{postId}/comments` does NOT include comment `cccc0000-0000-0000-0000-000000000001`

### GWT-33: Delete own Edited comment (Edited -> Deleted)

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Edited"`
WHEN DELETE `/api/comments/cccc0000-0000-0000-0000-000000000001`
THEN response status is `204 No Content`
AND response body is empty
AND the comment with Id `cccc0000-0000-0000-0000-000000000001` has `Status` `"Deleted"` in the database

### GWT-34: Invalid state transition — delete a Flagged comment

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Flagged"`
WHEN DELETE `/api/comments/cccc0000-0000-0000-0000-000000000001`
THEN response status is `409 Conflict`
AND response body contains an error indicating that deleting a comment with `Status` `"Flagged"` is not allowed

### GWT-35: Invalid state transition — delete an already Deleted comment

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Deleted"`
WHEN DELETE `/api/comments/cccc0000-0000-0000-0000-000000000001`
THEN response status is `409 Conflict`
AND response body contains an error indicating that deleting a comment with `Status` `"Deleted"` is not allowed

### GWT-36: Invalid state transition — delete an Approved comment

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Approved"`
WHEN DELETE `/api/comments/cccc0000-0000-0000-0000-000000000001`
THEN response status is `409 Conflict`
AND response body contains an error indicating that deleting a comment with `Status` `"Approved"` is not allowed

### GWT-37: Invalid state transition — delete a Removed comment

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Removed"`
WHEN DELETE `/api/comments/cccc0000-0000-0000-0000-000000000001`
THEN response status is `409 Conflict`
AND response body contains an error indicating that deleting a comment with `Status` `"Removed"` is not allowed

### GWT-38: Ownership boundary — delete another user's comment

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000002`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000002`, `Status` = `"Active"`
WHEN DELETE `/api/comments/cccc0000-0000-0000-0000-000000000002`
THEN response status is `403 Forbidden`
AND the comment with Id `cccc0000-0000-0000-0000-000000000002` retains `Status` `"Active"`

### GWT-39: Not found — nonexistent comment Id

GIVEN an authenticated user
AND no comment exists with Id `cccc0000-0000-0000-0000-000000009999`
WHEN DELETE `/api/comments/cccc0000-0000-0000-0000-000000009999`
THEN response status is `404 Not Found`

### GWT-40: Unauthorized — no auth token

GIVEN no authentication credentials (no `Authorization` header)
WHEN DELETE `/api/comments/cccc0000-0000-0000-0000-000000000001`
THEN response status is `401 Unauthorized`

---

## PUT /api/comments/{id}/flag — Flag Comment

### GWT-41: Flag an Active comment (Active -> Flagged)

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000002`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Active"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/flag`
THEN response status is `200 OK`
AND response body is a `CommentDto` with:
  - `Id` equals `cccc0000-0000-0000-0000-000000000001`
  - `Status` equals `"Flagged"`

### GWT-42: Flag an Edited comment (Edited -> Flagged)

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000002`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Edited"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/flag`
THEN response status is `200 OK`
AND response body is a `CommentDto` with:
  - `Id` equals `cccc0000-0000-0000-0000-000000000001`
  - `Status` equals `"Flagged"`

### GWT-43: Reject flagging own comment

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000001`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Active"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/flag`
THEN response status is `403 Forbidden`
AND response body contains an error indicating users cannot flag their own comments
AND the comment retains `Status` `"Active"`

### GWT-44: Invalid state transition — flag a Deleted comment

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000002`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Deleted"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/flag`
THEN response status is `409 Conflict`
AND response body contains an error indicating that flagging a comment with `Status` `"Deleted"` is not allowed

### GWT-45: Invalid state transition — flag an already Flagged comment

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000002`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Flagged"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/flag`
THEN response status is `409 Conflict`
AND response body contains an error indicating that flagging a comment with `Status` `"Flagged"` is not allowed

### GWT-46: Invalid state transition — flag an Approved comment

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000002`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Approved"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/flag`
THEN response status is `409 Conflict`
AND response body contains an error indicating that flagging a comment with `Status` `"Approved"` is not allowed

### GWT-47: Invalid state transition — flag a Removed comment

GIVEN an authenticated user with AuthorId `aaaa0000-0000-0000-0000-000000000002`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `AuthorId` = `aaaa0000-0000-0000-0000-000000000001`, `Status` = `"Removed"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/flag`
THEN response status is `409 Conflict`
AND response body contains an error indicating that flagging a comment with `Status` `"Removed"` is not allowed

### GWT-48: Not found — nonexistent comment Id

GIVEN an authenticated user
AND no comment exists with Id `cccc0000-0000-0000-0000-000000009999`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000009999/flag`
THEN response status is `404 Not Found`

### GWT-49: Unauthorized — no auth token

GIVEN no authentication credentials (no `Authorization` header)
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/flag`
THEN response status is `401 Unauthorized`

---

## PUT /api/comments/{id}/moderate — Moderate Flagged Comment

### GWT-50: Approve a Flagged comment (Flagged -> Approved)

GIVEN an authenticated admin user
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `Status` = `"Flagged"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/moderate` with body:
```json
{
  "Decision": "approve"
}
```
THEN response status is `200 OK`
AND response body is a `CommentDto` with:
  - `Id` equals `cccc0000-0000-0000-0000-000000000001`
  - `Status` equals `"Approved"`

### GWT-51: Remove a Flagged comment (Flagged -> Removed)

GIVEN an authenticated admin user
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `Status` = `"Flagged"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/moderate` with body:
```json
{
  "Decision": "remove"
}
```
THEN response status is `200 OK`
AND response body is a `CommentDto` with:
  - `Id` equals `cccc0000-0000-0000-0000-000000000001`
  - `Status` equals `"Removed"`

### GWT-52: Approved comment no longer hidden from non-admin listing

GIVEN an authenticated admin user
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `Status` = `"Flagged"`, `PostId` = `pppp0000-0000-0000-0000-000000000001`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/moderate` with body:
```json
{
  "Decision": "approve"
}
```
AND response status is `200 OK`
THEN a subsequent GET `/api/posts/pppp0000-0000-0000-0000-000000000001/comments` by a non-admin user returns `200 OK`
AND the response array includes comment with Id `cccc0000-0000-0000-0000-000000000001` and `Status` `"Approved"`

### GWT-53: Invalid state transition — moderate an Active comment

GIVEN an authenticated admin user
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `Status` = `"Active"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/moderate` with body:
```json
{
  "Decision": "approve"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating that moderating a comment with `Status` `"Active"` is not allowed

### GWT-54: Invalid state transition — moderate an Edited comment

GIVEN an authenticated admin user
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `Status` = `"Edited"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/moderate` with body:
```json
{
  "Decision": "approve"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating that moderating a comment with `Status` `"Edited"` is not allowed

### GWT-55: Invalid state transition — moderate a Deleted comment

GIVEN an authenticated admin user
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `Status` = `"Deleted"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/moderate` with body:
```json
{
  "Decision": "approve"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating that moderating a comment with `Status` `"Deleted"` is not allowed

### GWT-56: Invalid state transition — moderate an Approved comment

GIVEN an authenticated admin user
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `Status` = `"Approved"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/moderate` with body:
```json
{
  "Decision": "remove"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating that moderating a comment with `Status` `"Approved"` is not allowed

### GWT-57: Invalid state transition — moderate a Removed comment

GIVEN an authenticated admin user
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `Status` = `"Removed"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/moderate` with body:
```json
{
  "Decision": "approve"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating that moderating a comment with `Status` `"Removed"` is not allowed

### GWT-58: Validation error — invalid Decision value

GIVEN an authenticated admin user
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `Status` = `"Flagged"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/moderate` with body:
```json
{
  "Decision": "maybe"
}
```
THEN response status is `400 Bad Request`
AND response body is a ProblemDetails object with `errors` dictionary containing key `"Decision"` indicating allowed values are `"approve"` and `"remove"`

### GWT-59: Validation error — empty Decision

GIVEN an authenticated admin user
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `Status` = `"Flagged"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/moderate` with body:
```json
{
  "Decision": ""
}
```
THEN response status is `400 Bad Request`
AND response body is a ProblemDetails object with `errors` dictionary containing key `"Decision"`

### GWT-60: Auth boundary — non-admin user attempts moderation

GIVEN an authenticated non-admin user with AuthorId `aaaa0000-0000-0000-0000-000000000002`
AND a comment exists with `Id` = `cccc0000-0000-0000-0000-000000000001`, `Status` = `"Flagged"`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/moderate` with body:
```json
{
  "Decision": "approve"
}
```
THEN response status is `403 Forbidden`
AND the comment with Id `cccc0000-0000-0000-0000-000000000001` retains `Status` `"Flagged"`

### GWT-61: Not found — nonexistent comment Id

GIVEN an authenticated admin user
AND no comment exists with Id `cccc0000-0000-0000-0000-000000009999`
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000009999/moderate` with body:
```json
{
  "Decision": "approve"
}
```
THEN response status is `404 Not Found`

### GWT-62: Unauthorized — no auth token

GIVEN no authentication credentials (no `Authorization` header)
WHEN PUT `/api/comments/cccc0000-0000-0000-0000-000000000001/moderate` with body:
```json
{
  "Decision": "approve"
}
```
THEN response status is `401 Unauthorized`

---

## Quality Checklist

- [x] Every endpoint has a happy path GWT (GWT-1, GWT-12, GWT-18, GWT-32, GWT-41, GWT-50/51)
- [x] Every POST/PUT has at least one validation error GWT (GWT-6/7, GWT-27/28, GWT-58/59)
- [x] Every auth-protected endpoint has an unauthorized GWT (GWT-11, GWT-31, GWT-40, GWT-49, GWT-62)
- [x] GET listing endpoint is public — verified public access behavior with Flagged filtering (GWT-16)
- [x] Every path parameter has a not-found GWT (GWT-9/10, GWT-17, GWT-30, GWT-39, GWT-48, GWT-61)
- [x] Every valid state transition has a GWT:
  - Active -> Edited (GWT-18)
  - Active -> Flagged (GWT-41)
  - Active -> Deleted (GWT-32)
  - Edited -> Flagged (GWT-42)
  - Edited -> Deleted (GWT-33)
  - Flagged -> Approved (GWT-50)
  - Flagged -> Removed (GWT-51)
- [x] Every invalid state transition has a GWT:
  - Edit: Flagged (GWT-23), Deleted (GWT-24), Approved (GWT-25), Removed (GWT-26)
  - Delete: Flagged (GWT-34), Deleted (GWT-35), Approved (GWT-36), Removed (GWT-37)
  - Flag: Deleted (GWT-44), Flagged (GWT-45), Approved (GWT-46), Removed (GWT-47)
  - Moderate: Active (GWT-53), Edited (GWT-54), Deleted (GWT-55), Approved (GWT-56), Removed (GWT-57)
- [x] Every business rule has a boundary GWT:
  - Max 3 edits: at boundary EditCount 2->3 (GWT-19), exceeding at EditCount 3 (GWT-20)
  - 24-hour edit window: expired (GWT-21), just within window (GWT-22)
  - Flagged hidden from non-admins (GWT-12, GWT-13, GWT-16)
  - Approved re-visible after moderation (GWT-52)
  - Max nesting depth 3: at boundary depth 3 (GWT-3), exceeding depth 4 (GWT-4)
  - Content 1-5000 chars: empty rejected (GWT-7), 1 char accepted (GWT-8), 5000 accepted (GWT-5), 5001 rejected (GWT-6)
  - Only author can edit/delete (GWT-29, GWT-38)
  - Cannot flag own comment (GWT-43)
  - Only admins can moderate (GWT-60)
- [x] All field names match contract DTOs: `Id`, `PostId`, `AuthorId`, `ParentId`, `Content`, `Status`, `EditCount`, `CreatedAt`, `EditedAt`, `Decision`
- [x] All status codes are explicit numbers: 200, 201, 204, 400, 401, 403, 404, 409
- [x] No vague language used

## Summary

| Endpoint | GWTs | Coverage |
|----------|------|----------|
| POST /api/posts/{postId}/comments | GWT-1 through GWT-11 | Happy path (top-level + nested), depth boundary (3 accepted, 4 rejected), content boundary (1 char, 5000, 5001, empty), 2 not-found (postId, parentId), 1 unauthorized |
| GET /api/posts/{postId}/comments | GWT-12 through GWT-17 | Non-admin filtering (Active/Edited/Approved visible, Flagged hidden), admin sees Flagged, Deleted/Removed hidden for all, empty array, public access filtering, not-found postId |
| PUT /api/comments/{id} | GWT-18 through GWT-31 | Happy path (Active->Edited), edit boundary (2->3 accepted, 3 rejected), 24h window (expired + just-in-time), 4 invalid state transitions, 2 validation errors, ownership 403, not-found 404, 1 unauthorized |
| DELETE /api/comments/{id} | GWT-32 through GWT-40 | Happy path (Active->Deleted, Edited->Deleted), 4 invalid state transitions, ownership 403, not-found 404, 1 unauthorized |
| PUT /api/comments/{id}/flag | GWT-41 through GWT-49 | Happy path (Active->Flagged, Edited->Flagged), self-flag rejected, 4 invalid state transitions, not-found 404, 1 unauthorized |
| PUT /api/comments/{id}/moderate | GWT-50 through GWT-62 | Happy path (approve + remove), approved re-visible side-effect, 5 invalid state transitions, 2 validation errors (invalid + empty Decision), non-admin 403, not-found 404, 1 unauthorized |
| **Total** | **62 GWTs** | |
