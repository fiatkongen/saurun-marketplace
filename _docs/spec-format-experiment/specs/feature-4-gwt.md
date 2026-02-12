# Feature 4: Comment Thread with Moderation — GWT Acceptance Criteria

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

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a post exists with Id `pppp-...-0001`
WHEN POST `/api/posts/pppp-...-0001/comments` with body:
```json
{
  "Content": "This is a great post!",
  "ParentId": null
}
```
THEN response status is `201 Created`
AND response body is a `CommentDto` with:
  - `Id` is a non-empty GUID
  - `PostId` equals `pppp-...-0001`
  - `AuthorId` equals `aaaa-...-0001`
  - `ParentId` is `null`
  - `Content` equals `"This is a great post!"`
  - `Status` equals `"Active"`
  - `EditCount` equals `0`
  - `CreatedAt` is within 5 seconds of UTC now
  - `EditedAt` is `null`

### GWT-2: Create nested reply (depth 1)

GIVEN an authenticated user with AuthorId `aaaa-...-0002`
AND a post exists with Id `pppp-...-0001`
AND a comment exists with Id `cccc-...-0001` on post `pppp-...-0001` with `ParentId` `null` (depth 0)
WHEN POST `/api/posts/pppp-...-0001/comments` with body:
```json
{
  "Content": "I agree with you!",
  "ParentId": "cccc-...-0001"
}
```
THEN response status is `201 Created`
AND response body is a `CommentDto` with:
  - `ParentId` equals `cccc-...-0001`
  - `Status` equals `"Active"`
  - `EditCount` equals `0`

### GWT-3: Create nested reply at max depth (depth 3)

GIVEN an authenticated user
AND a post exists with Id `pppp-...-0001`
AND a comment chain exists: `cccc-...-0001` (depth 0) → `cccc-...-0002` (depth 1) → `cccc-...-0003` (depth 2)
WHEN POST `/api/posts/pppp-...-0001/comments` with body:
```json
{
  "Content": "Deepest allowed reply",
  "ParentId": "cccc-...-0003"
}
```
THEN response status is `201 Created`
AND response body is a `CommentDto` with:
  - `ParentId` equals `cccc-...-0003`

### GWT-4: Reject nested reply exceeding max depth 3

GIVEN an authenticated user
AND a post exists with Id `pppp-...-0001`
AND a comment chain exists: `cccc-...-0001` (depth 0) → `cccc-...-0002` (depth 1) → `cccc-...-0003` (depth 2) → `cccc-...-0004` (depth 3)
WHEN POST `/api/posts/pppp-...-0001/comments` with body:
```json
{
  "Content": "Too deep",
  "ParentId": "cccc-...-0004"
}
```
THEN response status is `400 Bad Request`
AND response body contains a validation error indicating maximum nesting depth of 3 exceeded

### GWT-5: Validation error — empty Content

GIVEN an authenticated user
AND a post exists with Id `pppp-...-0001`
WHEN POST `/api/posts/pppp-...-0001/comments` with body:
```json
{
  "Content": "",
  "ParentId": null
}
```
THEN response status is `400 Bad Request`
AND response body contains a validation error referencing field `Content`

### GWT-6: Validation error — Content exceeds 5000 characters

GIVEN an authenticated user
AND a post exists with Id `pppp-...-0001`
WHEN POST `/api/posts/pppp-...-0001/comments` with body:
```json
{
  "Content": "<string of 5001 characters>",
  "ParentId": null
}
```
THEN response status is `400 Bad Request`
AND response body contains a validation error referencing field `Content`

### GWT-7: Not found — nonexistent postId

GIVEN an authenticated user
AND no post exists with Id `pppp-...-9999`
WHEN POST `/api/posts/pppp-...-9999/comments` with body:
```json
{
  "Content": "Comment on ghost post",
  "ParentId": null
}
```
THEN response status is `404 Not Found`

### GWT-8: Not found — ParentId references nonexistent comment

GIVEN an authenticated user
AND a post exists with Id `pppp-...-0001`
AND no comment exists with Id `cccc-...-9999`
WHEN POST `/api/posts/pppp-...-0001/comments` with body:
```json
{
  "Content": "Reply to nothing",
  "ParentId": "cccc-...-9999"
}
```
THEN response status is `404 Not Found`

### GWT-9: Unauthorized — no auth token

GIVEN no authentication credentials
WHEN POST `/api/posts/pppp-...-0001/comments` with body:
```json
{
  "Content": "Anonymous comment",
  "ParentId": null
}
```
THEN response status is `401 Unauthorized`

---

## GET /api/posts/{postId}/comments — List Comments on Post

### GWT-10: List comments returns all Active and Edited comments for non-admin user

GIVEN an authenticated non-admin user
AND a post exists with Id `pppp-...-0001`
AND 3 comments exist on post `pppp-...-0001`:
  - `cccc-...-0001` with `Status` `"Active"`
  - `cccc-...-0002` with `Status` `"Edited"`
  - `cccc-...-0003` with `Status` `"Flagged"`
WHEN GET `/api/posts/pppp-...-0001/comments`
THEN response status is `200 OK`
AND response body is an array of 2 `CommentDto` objects
AND the array contains comments with Ids `cccc-...-0001` and `cccc-...-0002`
AND the array does NOT contain comment with Id `cccc-...-0003`

### GWT-11: List comments includes Flagged comments for admin user

GIVEN an authenticated admin user
AND a post exists with Id `pppp-...-0001`
AND 3 comments exist on post `pppp-...-0001`:
  - `cccc-...-0001` with `Status` `"Active"`
  - `cccc-...-0002` with `Status` `"Edited"`
  - `cccc-...-0003` with `Status` `"Flagged"`
WHEN GET `/api/posts/pppp-...-0001/comments`
THEN response status is `200 OK`
AND response body is an array of 3 `CommentDto` objects
AND the array contains comments with Ids `cccc-...-0001`, `cccc-...-0002`, and `cccc-...-0003`

### GWT-12: List comments excludes Deleted and Removed comments for all users

GIVEN an authenticated admin user
AND a post exists with Id `pppp-...-0001`
AND 4 comments exist on post `pppp-...-0001`:
  - `cccc-...-0001` with `Status` `"Active"`
  - `cccc-...-0002` with `Status` `"Deleted"`
  - `cccc-...-0003` with `Status` `"Removed"`
  - `cccc-...-0004` with `Status` `"Approved"`
WHEN GET `/api/posts/pppp-...-0001/comments`
THEN response status is `200 OK`
AND response body is an array of 2 `CommentDto` objects
AND the array contains comments with Ids `cccc-...-0001` and `cccc-...-0004`
AND the array does NOT contain comments with Ids `cccc-...-0002` or `cccc-...-0003`

### GWT-13: List comments returns empty array when post has no comments

GIVEN a post exists with Id `pppp-...-0001`
AND the post has 0 comments
WHEN GET `/api/posts/pppp-...-0001/comments`
THEN response status is `200 OK`
AND response body is an empty array `[]`

### GWT-14: List comments — public access (no auth required)

GIVEN no authentication credentials
AND a post exists with Id `pppp-...-0001`
AND 2 comments exist on post `pppp-...-0001`:
  - `cccc-...-0001` with `Status` `"Active"`
  - `cccc-...-0002` with `Status` `"Flagged"`
WHEN GET `/api/posts/pppp-...-0001/comments`
THEN response status is `200 OK`
AND response body is an array of 1 `CommentDto` object
AND the array contains comment with Id `cccc-...-0001`
AND the array does NOT contain comment with Id `cccc-...-0002`

### GWT-15: Not found — nonexistent postId

GIVEN no post exists with Id `pppp-...-9999`
WHEN GET `/api/posts/pppp-...-9999/comments`
THEN response status is `404 Not Found`

---

## PUT /api/comments/{id} — Edit Comment

### GWT-16: Edit own Active comment — happy path

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Active"`, `EditCount` `0`, `CreatedAt` within the last 24 hours
WHEN PUT `/api/comments/cccc-...-0001` with body:
```json
{
  "Content": "Updated comment content"
}
```
THEN response status is `200 OK`
AND response body is a `CommentDto` with:
  - `Id` equals `cccc-...-0001`
  - `Content` equals `"Updated comment content"`
  - `Status` equals `"Edited"`
  - `EditCount` equals `1`
  - `EditedAt` is within 5 seconds of UTC now

### GWT-17: Edit own Edited comment (second edit)

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Edited"`, `EditCount` `2`, `CreatedAt` within the last 24 hours
WHEN PUT `/api/comments/cccc-...-0001` with body:
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
  - `EditedAt` is within 5 seconds of UTC now

### GWT-18: Reject edit when editCount already at max (3)

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Edited"`, `EditCount` `3`, `CreatedAt` within the last 24 hours
WHEN PUT `/api/comments/cccc-...-0001` with body:
```json
{
  "Content": "One edit too many"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating maximum edit count of 3 reached

### GWT-19: Reject edit when comment is older than 24 hours

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Active"`, `EditCount` `0`, `CreatedAt` equal to `2025-01-01T00:00:00Z` (more than 24 hours ago)
WHEN PUT `/api/comments/cccc-...-0001` with body:
```json
{
  "Content": "Too late to edit"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating the 24-hour edit window has expired

### GWT-20: Invalid state transition — edit a Flagged comment

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Flagged"`, `CreatedAt` within the last 24 hours
WHEN PUT `/api/comments/cccc-...-0001` with body:
```json
{
  "Content": "Trying to edit flagged comment"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating that editing a comment with `Status` `"Flagged"` is not allowed

### GWT-21: Invalid state transition — edit a Deleted comment

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Deleted"`
WHEN PUT `/api/comments/cccc-...-0001` with body:
```json
{
  "Content": "Trying to edit deleted comment"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating that editing a comment with `Status` `"Deleted"` is not allowed

### GWT-22: Invalid state transition — edit an Approved comment

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Approved"`
WHEN PUT `/api/comments/cccc-...-0001` with body:
```json
{
  "Content": "Trying to edit approved comment"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating that editing a comment with `Status` `"Approved"` is not allowed

### GWT-23: Invalid state transition — edit a Removed comment

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Removed"`
WHEN PUT `/api/comments/cccc-...-0001` with body:
```json
{
  "Content": "Trying to edit removed comment"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating that editing a comment with `Status` `"Removed"` is not allowed

### GWT-24: Validation error — empty Content on edit

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Active"`, `CreatedAt` within the last 24 hours
WHEN PUT `/api/comments/cccc-...-0001` with body:
```json
{
  "Content": ""
}
```
THEN response status is `400 Bad Request`
AND response body contains a validation error referencing field `Content`

### GWT-25: Validation error — Content exceeds 5000 characters on edit

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Active"`, `CreatedAt` within the last 24 hours
WHEN PUT `/api/comments/cccc-...-0001` with body:
```json
{
  "Content": "<string of 5001 characters>"
}
```
THEN response status is `400 Bad Request`
AND response body contains a validation error referencing field `Content`

### GWT-26: Ownership boundary — edit another user's comment

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0002`, `AuthorId` `aaaa-...-0002`, `Status` `"Active"`
WHEN PUT `/api/comments/cccc-...-0002` with body:
```json
{
  "Content": "Hijacking your comment"
}
```
THEN response status is `403 Forbidden`
AND the comment with Id `cccc-...-0002` retains its original `Content`

### GWT-27: Not found — nonexistent comment Id

GIVEN an authenticated user
AND no comment exists with Id `cccc-...-9999`
WHEN PUT `/api/comments/cccc-...-9999` with body:
```json
{
  "Content": "Editing a ghost"
}
```
THEN response status is `404 Not Found`

### GWT-28: Unauthorized — no auth token

GIVEN no authentication credentials
WHEN PUT `/api/comments/cccc-...-0001` with body:
```json
{
  "Content": "Anonymous edit"
}
```
THEN response status is `401 Unauthorized`

---

## DELETE /api/comments/{id} — Delete Comment (Soft)

### GWT-29: Delete own Active comment

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Active"`
WHEN DELETE `/api/comments/cccc-...-0001`
THEN response status is `204 No Content`
AND the comment with Id `cccc-...-0001` has `Status` `"Deleted"` in the database
AND subsequent GET `/api/posts/{postId}/comments` does NOT include comment `cccc-...-0001`

### GWT-30: Delete own Edited comment

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Edited"`
WHEN DELETE `/api/comments/cccc-...-0001`
THEN response status is `204 No Content`
AND the comment with Id `cccc-...-0001` has `Status` `"Deleted"` in the database

### GWT-31: Invalid state transition — delete a Flagged comment

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Flagged"`
WHEN DELETE `/api/comments/cccc-...-0001`
THEN response status is `409 Conflict`
AND response body contains an error indicating that deleting a comment with `Status` `"Flagged"` is not allowed

### GWT-32: Invalid state transition — delete an already Deleted comment

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Deleted"`
WHEN DELETE `/api/comments/cccc-...-0001`
THEN response status is `409 Conflict`
AND response body contains an error indicating that deleting a comment with `Status` `"Deleted"` is not allowed

### GWT-33: Invalid state transition — delete an Approved comment

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Approved"`
WHEN DELETE `/api/comments/cccc-...-0001`
THEN response status is `409 Conflict`
AND response body contains an error indicating that deleting a comment with `Status` `"Approved"` is not allowed

### GWT-34: Invalid state transition — delete a Removed comment

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Removed"`
WHEN DELETE `/api/comments/cccc-...-0001`
THEN response status is `409 Conflict`
AND response body contains an error indicating that deleting a comment with `Status` `"Removed"` is not allowed

### GWT-35: Ownership boundary — delete another user's comment

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0002`, `AuthorId` `aaaa-...-0002`, `Status` `"Active"`
WHEN DELETE `/api/comments/cccc-...-0002`
THEN response status is `403 Forbidden`
AND the comment with Id `cccc-...-0002` retains `Status` `"Active"`

### GWT-36: Not found — nonexistent comment Id

GIVEN an authenticated user
AND no comment exists with Id `cccc-...-9999`
WHEN DELETE `/api/comments/cccc-...-9999`
THEN response status is `404 Not Found`

### GWT-37: Unauthorized — no auth token

GIVEN no authentication credentials
WHEN DELETE `/api/comments/cccc-...-0001`
THEN response status is `401 Unauthorized`

---

## PUT /api/comments/{id}/flag — Flag Comment

### GWT-38: Flag an Active comment

GIVEN an authenticated user with AuthorId `aaaa-...-0002`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Active"`
WHEN PUT `/api/comments/cccc-...-0001/flag`
THEN response status is `200 OK`
AND response body is a `CommentDto` with:
  - `Id` equals `cccc-...-0001`
  - `Status` equals `"Flagged"`

### GWT-39: Flag an Edited comment

GIVEN an authenticated user with AuthorId `aaaa-...-0002`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Edited"`
WHEN PUT `/api/comments/cccc-...-0001/flag`
THEN response status is `200 OK`
AND response body is a `CommentDto` with:
  - `Status` equals `"Flagged"`

### GWT-40: Reject flagging own comment

GIVEN an authenticated user with AuthorId `aaaa-...-0001`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Active"`
WHEN PUT `/api/comments/cccc-...-0001/flag`
THEN response status is `403 Forbidden`
AND response body contains an error indicating users cannot flag their own comments

### GWT-41: Invalid state transition — flag a Deleted comment

GIVEN an authenticated user with AuthorId `aaaa-...-0002`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Deleted"`
WHEN PUT `/api/comments/cccc-...-0001/flag`
THEN response status is `409 Conflict`
AND response body contains an error indicating that flagging a comment with `Status` `"Deleted"` is not allowed

### GWT-42: Invalid state transition — flag an already Flagged comment

GIVEN an authenticated user with AuthorId `aaaa-...-0002`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Flagged"`
WHEN PUT `/api/comments/cccc-...-0001/flag`
THEN response status is `409 Conflict`
AND response body contains an error indicating that flagging a comment with `Status` `"Flagged"` is not allowed

### GWT-43: Invalid state transition — flag an Approved comment

GIVEN an authenticated user with AuthorId `aaaa-...-0002`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Approved"`
WHEN PUT `/api/comments/cccc-...-0001/flag`
THEN response status is `409 Conflict`
AND response body contains an error indicating that flagging a comment with `Status` `"Approved"` is not allowed

### GWT-44: Invalid state transition — flag a Removed comment

GIVEN an authenticated user with AuthorId `aaaa-...-0002`
AND a comment exists with Id `cccc-...-0001`, `AuthorId` `aaaa-...-0001`, `Status` `"Removed"`
WHEN PUT `/api/comments/cccc-...-0001/flag`
THEN response status is `409 Conflict`
AND response body contains an error indicating that flagging a comment with `Status` `"Removed"` is not allowed

### GWT-45: Not found — nonexistent comment Id

GIVEN an authenticated user
AND no comment exists with Id `cccc-...-9999`
WHEN PUT `/api/comments/cccc-...-9999/flag`
THEN response status is `404 Not Found`

### GWT-46: Unauthorized — no auth token

GIVEN no authentication credentials
WHEN PUT `/api/comments/cccc-...-0001/flag`
THEN response status is `401 Unauthorized`

---

## PUT /api/comments/{id}/moderate — Moderate Flagged Comment

### GWT-47: Approve a Flagged comment

GIVEN an authenticated admin user
AND a comment exists with Id `cccc-...-0001`, `Status` `"Flagged"`
WHEN PUT `/api/comments/cccc-...-0001/moderate` with body:
```json
{
  "Decision": "approve"
}
```
THEN response status is `200 OK`
AND response body is a `CommentDto` with:
  - `Id` equals `cccc-...-0001`
  - `Status` equals `"Approved"`

### GWT-48: Remove a Flagged comment

GIVEN an authenticated admin user
AND a comment exists with Id `cccc-...-0001`, `Status` `"Flagged"`
WHEN PUT `/api/comments/cccc-...-0001/moderate` with body:
```json
{
  "Decision": "remove"
}
```
THEN response status is `200 OK`
AND response body is a `CommentDto` with:
  - `Id` equals `cccc-...-0001`
  - `Status` equals `"Removed"`

### GWT-49: Invalid state transition — moderate an Active comment

GIVEN an authenticated admin user
AND a comment exists with Id `cccc-...-0001`, `Status` `"Active"`
WHEN PUT `/api/comments/cccc-...-0001/moderate` with body:
```json
{
  "Decision": "approve"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating that moderating a comment with `Status` `"Active"` is not allowed

### GWT-50: Invalid state transition — moderate an Edited comment

GIVEN an authenticated admin user
AND a comment exists with Id `cccc-...-0001`, `Status` `"Edited"`
WHEN PUT `/api/comments/cccc-...-0001/moderate` with body:
```json
{
  "Decision": "approve"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating that moderating a comment with `Status` `"Edited"` is not allowed

### GWT-51: Invalid state transition — moderate a Deleted comment

GIVEN an authenticated admin user
AND a comment exists with Id `cccc-...-0001`, `Status` `"Deleted"`
WHEN PUT `/api/comments/cccc-...-0001/moderate` with body:
```json
{
  "Decision": "approve"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating that moderating a comment with `Status` `"Deleted"` is not allowed

### GWT-52: Invalid state transition — moderate an Approved comment

GIVEN an authenticated admin user
AND a comment exists with Id `cccc-...-0001`, `Status` `"Approved"`
WHEN PUT `/api/comments/cccc-...-0001/moderate` with body:
```json
{
  "Decision": "remove"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating that moderating a comment with `Status` `"Approved"` is not allowed

### GWT-53: Invalid state transition — moderate a Removed comment

GIVEN an authenticated admin user
AND a comment exists with Id `cccc-...-0001`, `Status` `"Removed"`
WHEN PUT `/api/comments/cccc-...-0001/moderate` with body:
```json
{
  "Decision": "approve"
}
```
THEN response status is `409 Conflict`
AND response body contains an error indicating that moderating a comment with `Status` `"Removed"` is not allowed

### GWT-54: Validation error — invalid Decision value

GIVEN an authenticated admin user
AND a comment exists with Id `cccc-...-0001`, `Status` `"Flagged"`
WHEN PUT `/api/comments/cccc-...-0001/moderate` with body:
```json
{
  "Decision": "maybe"
}
```
THEN response status is `400 Bad Request`
AND response body contains a validation error referencing field `Decision` indicating allowed values are `"approve"` and `"remove"`

### GWT-55: Validation error — empty Decision

GIVEN an authenticated admin user
AND a comment exists with Id `cccc-...-0001`, `Status` `"Flagged"`
WHEN PUT `/api/comments/cccc-...-0001/moderate` with body:
```json
{
  "Decision": ""
}
```
THEN response status is `400 Bad Request`
AND response body contains a validation error referencing field `Decision`

### GWT-56: Auth boundary — non-admin user attempts moderation

GIVEN an authenticated non-admin user with AuthorId `aaaa-...-0002`
AND a comment exists with Id `cccc-...-0001`, `Status` `"Flagged"`
WHEN PUT `/api/comments/cccc-...-0001/moderate` with body:
```json
{
  "Decision": "approve"
}
```
THEN response status is `403 Forbidden`
AND the comment with Id `cccc-...-0001` retains `Status` `"Flagged"`

### GWT-57: Not found — nonexistent comment Id

GIVEN an authenticated admin user
AND no comment exists with Id `cccc-...-9999`
WHEN PUT `/api/comments/cccc-...-9999/moderate` with body:
```json
{
  "Decision": "approve"
}
```
THEN response status is `404 Not Found`

### GWT-58: Unauthorized — no auth token

GIVEN no authentication credentials
WHEN PUT `/api/comments/cccc-...-0001/moderate` with body:
```json
{
  "Decision": "approve"
}
```
THEN response status is `401 Unauthorized`

---

## Quality Checklist

- [x] Every endpoint has a happy path GWT (GWT-1, GWT-10, GWT-16, GWT-29, GWT-38, GWT-47/48)
- [x] Every POST/PUT has at least one validation error GWT (GWT-5/6, GWT-24/25, GWT-54/55)
- [x] Every auth-protected endpoint has an unauthorized GWT (GWT-9, GWT-28, GWT-37, GWT-46, GWT-58)
- [x] GET listing endpoint is public — verified public access behavior (GWT-14)
- [x] Every path parameter has a not-found GWT (GWT-7/8, GWT-15, GWT-27, GWT-36, GWT-45, GWT-57)
- [x] Every valid state transition has a GWT:
  - Active → Edited (GWT-16)
  - Active → Flagged (GWT-38)
  - Active → Deleted (GWT-29)
  - Edited → Flagged (GWT-39)
  - Edited → Deleted (GWT-30)
  - Flagged → Approved (GWT-47)
  - Flagged → Removed (GWT-48)
- [x] Every invalid state transition has a GWT:
  - Edit: Flagged (GWT-20), Deleted (GWT-21), Approved (GWT-22), Removed (GWT-23)
  - Delete: Flagged (GWT-31), Deleted (GWT-32), Approved (GWT-33), Removed (GWT-34)
  - Flag: Deleted (GWT-41), Flagged (GWT-42), Approved (GWT-43), Removed (GWT-44)
  - Moderate: Active (GWT-49), Edited (GWT-50), Deleted (GWT-51), Approved (GWT-52), Removed (GWT-53)
- [x] Every business rule has a boundary GWT:
  - Max 3 edits (GWT-17 at boundary, GWT-18 exceeding)
  - 24-hour edit window (GWT-19)
  - Flagged hidden from non-admins (GWT-10, GWT-11, GWT-14)
  - Max nesting depth 3 (GWT-3 at boundary, GWT-4 exceeding)
  - Content 1-5000 chars (GWT-5 empty, GWT-6 exceeding)
  - Only author can edit/delete (GWT-26, GWT-35)
  - Cannot flag own comment (GWT-40)
  - Only admins can moderate (GWT-56)
- [x] All field names match contract DTOs: `Id`, `PostId`, `AuthorId`, `ParentId`, `Content`, `Status`, `EditCount`, `CreatedAt`, `EditedAt`, `Decision`
- [x] All status codes are explicit: 200, 201, 204, 400, 401, 403, 404, 409
- [x] No vague language used
