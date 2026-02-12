# Feature 4: Comment Thread with Moderation — EARS Requirements + Property Descriptions (R2)

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

---

## EARS Requirements

### Authentication & Authorization

REQ-1: If a request to POST /api/posts/{postId}/comments does not include a valid authentication token, then the system shall return 401.

REQ-2: If a request to PUT /api/comments/{id} does not include a valid authentication token, then the system shall return 401.

REQ-3: If a request to DELETE /api/comments/{id} does not include a valid authentication token, then the system shall return 401.

REQ-4: If a request to PUT /api/comments/{id}/flag does not include a valid authentication token, then the system shall return 401.

REQ-5: If a request to PUT /api/comments/{id}/moderate does not include a valid authentication token, then the system shall return 401.

REQ-6: If an authenticated non-admin user sends PUT /api/comments/{id}/moderate, then the system shall return 403.

REQ-7: If an authenticated user sends PUT /api/comments/{id} for a comment where CommentDto.AuthorId does not match the authenticated user's ID, then the system shall return 403.

REQ-8: If an authenticated user sends DELETE /api/comments/{id} for a comment where CommentDto.AuthorId does not match the authenticated user's ID, then the system shall return 403.

REQ-9: If an authenticated user sends PUT /api/comments/{id}/flag for a comment where CommentDto.AuthorId matches the authenticated user's ID, then the system shall return 403.

### POST /api/posts/{postId}/comments — Create Comment

REQ-10: When an authenticated user sends a valid CreateCommentRequest to POST /api/posts/{postId}/comments, the system shall create a new comment with Status "Active", EditCount 0, EditedAt null, AuthorId set to the authenticated user's ID, PostId set to the {postId} path parameter, CreatedAt set to the current UTC timestamp, and return 201 with a CommentDto.

REQ-11: If no post exists with the given {postId}, then the system shall return 404.

REQ-12: If the CreateCommentRequest.Content is null or empty, then the system shall return 400.

REQ-13: If the CreateCommentRequest.Content is whitespace-only, then the system shall return 400.

REQ-14: If the CreateCommentRequest.Content length exceeds 5000 characters, then the system shall return 400.

REQ-15: If the CreateCommentRequest.ParentId is non-null and no comment exists with that ParentId on the post identified by {postId}, then the system shall return 400.

REQ-16: If the CreateCommentRequest.ParentId is non-null and the resulting nesting depth would exceed 3 levels, then the system shall return 400.

### GET /api/posts/{postId}/comments — List Comments

REQ-17: When an unauthenticated user sends GET /api/posts/{postId}/comments, the system shall return 200 with a list of CommentDto excluding any comments with Status "Flagged" or "Removed".

REQ-18: When an authenticated non-admin user sends GET /api/posts/{postId}/comments, the system shall return 200 with a list of CommentDto excluding any comments with Status "Flagged" or "Removed".

REQ-19: When an authenticated admin user sends GET /api/posts/{postId}/comments, the system shall return 200 with a list of all CommentDto for that post regardless of Status.

REQ-20: If no post exists with the given {postId} in a GET /api/posts/{postId}/comments request, then the system shall return 404.

REQ-21: When there are zero visible comments for a post, the system shall return 200 with an empty array.

### PUT /api/comments/{id} — Edit Comment

REQ-22: When an authenticated user who is the author of the comment sends PUT /api/comments/{id} with a valid UpdateCommentRequest, and the comment has Status "Active" or "Edited", and EditCount is less than 3, and the comment's CreatedAt is less than 24 hours before the current UTC time, the system shall update the comment Content, increment EditCount by 1, set EditedAt to the current UTC timestamp, transition Status to "Edited", and return 200 with a CommentDto.

REQ-23: If no comment exists with the given {id} in a PUT /api/comments/{id} request, then the system shall return 404.

REQ-24: If the UpdateCommentRequest.Content is null or empty, then the system shall return 400.

REQ-25: If the UpdateCommentRequest.Content is whitespace-only, then the system shall return 400.

REQ-26: If the UpdateCommentRequest.Content length exceeds 5000 characters, then the system shall return 400.

REQ-27: If the comment identified by {id} has EditCount equal to 3, then the system shall return 409.

REQ-28: If the comment identified by {id} has a CreatedAt equal to or older than 24 hours from the current UTC time, then the system shall return 409.

REQ-29: If the comment identified by {id} has Status "Flagged", then the system shall return 409.

REQ-30: If the comment identified by {id} has Status "Deleted", then the system shall return 409.

REQ-31: If the comment identified by {id} has Status "Approved", then the system shall return 409.

REQ-32: If the comment identified by {id} has Status "Removed", then the system shall return 409.

### DELETE /api/comments/{id} — Soft Delete Comment

REQ-33: When an authenticated user who is the author of the comment sends DELETE /api/comments/{id}, and the comment has Status "Active" or "Edited", the system shall transition the comment Status to "Deleted" and return 200 with a CommentDto.

REQ-34: If no comment exists with the given {id} in a DELETE /api/comments/{id} request, then the system shall return 404.

REQ-35: If the comment identified by {id} has Status "Flagged", then the system shall return 409.

REQ-36: If the comment identified by {id} has Status "Deleted", then the system shall return 409.

REQ-37: If the comment identified by {id} has Status "Approved", then the system shall return 409.

REQ-38: If the comment identified by {id} has Status "Removed", then the system shall return 409.

### PUT /api/comments/{id}/flag — Flag Comment

REQ-39: When an authenticated user who is not the author of the comment sends PUT /api/comments/{id}/flag, and the comment has Status "Active" or "Edited", the system shall transition the comment Status to "Flagged" and return 200 with a CommentDto.

REQ-40: If no comment exists with the given {id} in a PUT /api/comments/{id}/flag request, then the system shall return 404.

REQ-41: If the comment identified by {id} has Status "Flagged", then the system shall return 409.

REQ-42: If the comment identified by {id} has Status "Deleted", then the system shall return 409.

REQ-43: If the comment identified by {id} has Status "Approved", then the system shall return 409.

REQ-44: If the comment identified by {id} has Status "Removed", then the system shall return 409.

### PUT /api/comments/{id}/moderate — Moderate Flagged Comment

REQ-45: When an authenticated admin user sends PUT /api/comments/{id}/moderate with ModerateCommentRequest.Decision equal to "approve", and the comment has Status "Flagged", the system shall transition the comment Status to "Approved" and return 200 with a CommentDto.

REQ-46: When an authenticated admin user sends PUT /api/comments/{id}/moderate with ModerateCommentRequest.Decision equal to "remove", and the comment has Status "Flagged", the system shall transition the comment Status to "Removed" and return 200 with a CommentDto.

REQ-47: If no comment exists with the given {id} in a PUT /api/comments/{id}/moderate request, then the system shall return 404.

REQ-48: If the ModerateCommentRequest.Decision is not "approve" and not "remove", then the system shall return 400.

REQ-49: If the ModerateCommentRequest.Decision is null or empty, then the system shall return 400.

REQ-50: If the comment identified by {id} has Status "Active", then the system shall return 409.

REQ-51: If the comment identified by {id} has Status "Edited", then the system shall return 409.

REQ-52: If the comment identified by {id} has Status "Deleted", then the system shall return 409.

REQ-53: If the comment identified by {id} has Status "Approved", then the system shall return 409.

REQ-54: If the comment identified by {id} has Status "Removed", then the system shall return 409.

---

## Property Descriptions

### Identity & Referential Integrity

PROP-1: For all CommentDto responses, Id must be a non-empty GUID (not equal to 00000000-0000-0000-0000-000000000000).

PROP-2: For all POST /api/posts/{postId}/comments requests that return 201, the returned CommentDto.Id must be unique across all previously created comments.

PROP-3: For all CommentDto responses, PostId must match the GUID of an existing post.

PROP-4: For all CommentDto responses, AuthorId must match the GUID of the authenticated user who created the comment.

PROP-5: For all CommentDto responses where ParentId is non-null, ParentId must match the Id of an existing comment on the same post (same PostId).

PROP-6: For all CommentDto responses, Id must be immutable — the same comment always returns the same Id.

### Status Invariants

PROP-7: For all CommentDto responses, Status must be one of: "Active", "Edited", "Flagged", "Deleted", "Approved", "Removed".

PROP-8: For all Comment entities in state "Active", the only valid transitions are "Edited", "Flagged", "Deleted".

PROP-9: For all Comment entities in state "Edited", the only valid transitions are "Flagged", "Deleted".

PROP-10: For all Comment entities in state "Flagged", the only valid transitions are "Approved", "Removed".

PROP-11: For all Comment entities in state "Deleted", no status transitions are valid.

PROP-12: For all Comment entities in state "Approved", no status transitions are valid.

PROP-13: For all Comment entities in state "Removed", no status transitions are valid.

### Timestamp Invariants

PROP-14: For all CommentDto responses, CreatedAt must be a valid UTC DateTime.

PROP-15: For all CommentDto responses from POST /api/posts/{postId}/comments, CreatedAt must be within 5 seconds of the server's current UTC time at the moment of creation.

PROP-16: For all CommentDto responses, CreatedAt must be immutable — retrieving the same comment at different times must return the same CreatedAt value.

PROP-17: For all CommentDto responses where Status is "Active", EditedAt must be null.

PROP-18: For all CommentDto responses where EditCount is greater than 0, EditedAt must be a non-null UTC timestamp greater than or equal to CreatedAt.

PROP-19: For all CommentDto responses, EditedAt must never decrease — each edit must set EditedAt to a value greater than the previous EditedAt.

PROP-20: For all CommentDto responses, once EditedAt is set to a non-null value, it must never revert to null.

### EditCount Invariants

PROP-21: For all CommentDto responses, EditCount must be an integer between 0 and 3 inclusive.

PROP-22: For all CommentDto responses where Status is "Active", EditCount must equal 0.

PROP-23: For all valid PUT /api/comments/{id} requests, the returned CommentDto.EditCount must equal the previous EditCount plus 1.

PROP-24: For all CommentDto responses, EditCount must never decrease.

PROP-25: For all CommentDto responses, EditCount must never change except via PUT /api/comments/{id} (flag, delete, and moderate operations must not alter EditCount).

### Content Invariants

PROP-26: For all CommentDto responses, Content must be a non-empty string with length between 1 and 5000 characters inclusive.

PROP-27: For all valid CreateCommentRequest inputs, the returned CommentDto.Content must exactly equal the submitted CreateCommentRequest.Content.

PROP-28: For all valid UpdateCommentRequest inputs, the returned CommentDto.Content must exactly equal the submitted UpdateCommentRequest.Content.

PROP-29: For all CommentDto responses, Content must never change except via PUT /api/comments/{id} (flag, delete, and moderate operations must not alter Content).

### Nesting Invariants

PROP-30: For all CommentDto responses where ParentId is non-null, the chain of ParentId references must not exceed 3 levels of depth.

PROP-31: For all CommentDto responses where ParentId is null, the comment is a root-level comment (depth 1).

PROP-32: For all CommentDto responses, ParentId must be immutable — it must never change after creation.

### Visibility Invariants

PROP-33: For all GET /api/posts/{postId}/comments responses to unauthenticated users, no CommentDto in the list must have Status equal to "Flagged" or "Removed".

PROP-34: For all GET /api/posts/{postId}/comments responses to authenticated non-admin users, no CommentDto in the list must have Status equal to "Flagged" or "Removed".

PROP-35: For all GET /api/posts/{postId}/comments responses to admin users, the list must include CommentDto of every status present for that post.

PROP-36: For all GET /api/posts/{postId}/comments responses, every CommentDto in the list must have PostId equal to the {postId} path parameter.

### Authorization Boundaries

PROP-37: For all PUT /api/comments/{id} requests, only the user whose ID matches CommentDto.AuthorId is authorized (all others receive 403).

PROP-38: For all DELETE /api/comments/{id} requests, only the user whose ID matches CommentDto.AuthorId is authorized (all others receive 403).

PROP-39: For all PUT /api/comments/{id}/flag requests, any authenticated user except the user whose ID matches CommentDto.AuthorId is authorized (the author receives 403).

PROP-40: For all PUT /api/comments/{id}/moderate requests, only admin users are authorized (all non-admin users receive 403).

### Edit Window Boundary

PROP-41: For all PUT /api/comments/{id} requests where the comment's CreatedAt is equal to or older than 24 hours before the current UTC time, the system must return 409.

PROP-42: For all PUT /api/comments/{id} requests where the comment's CreatedAt is less than 24 hours before the current UTC time and EditCount is less than 3, and Status is "Active" or "Edited", and UpdateCommentRequest.Content is valid, the system must return 200.

### Boundary Constraints — Content Length

PROP-43: For all CreateCommentRequest inputs where Content has length 1, the API must return 201 with a CommentDto (assuming all other preconditions are met).

PROP-44: For all CreateCommentRequest inputs where Content has length 5000, the API must return 201 with a CommentDto (assuming all other preconditions are met).

PROP-45: For all CreateCommentRequest inputs where Content has length 5001, the API must return 400.

PROP-46: For all UpdateCommentRequest inputs where Content has length 1, the API must return 200 with a CommentDto (assuming all other preconditions are met).

PROP-47: For all UpdateCommentRequest inputs where Content has length 5000, the API must return 200 with a CommentDto (assuming all other preconditions are met).

PROP-48: For all UpdateCommentRequest inputs where Content has length 5001, the API must return 400.

### Boundary Constraints — EditCount

PROP-49: For all PUT /api/comments/{id} requests where EditCount equals 2 and all other preconditions are met, the system must return 200 with a CommentDto where EditCount equals 3.

PROP-50: For all PUT /api/comments/{id} requests where EditCount equals 3, the system must return 409 regardless of all other preconditions.

### Boundary Constraints — Nesting Depth

PROP-51: For all CreateCommentRequest inputs where ParentId references a comment at depth 2, the system must return 201 with a CommentDto (depth 3 is the maximum allowed).

PROP-52: For all CreateCommentRequest inputs where ParentId references a comment at depth 3, the system must return 400 (depth 4 would exceed the maximum).

### Idempotency

PROP-53: For all DELETE /api/comments/{id} requests where the comment has Status "Deleted", repeated calls must return 409.

PROP-54: For all PUT /api/comments/{id}/flag requests where the comment has Status "Flagged", repeated calls must return 409.

### Output Completeness — Create

PROP-55: For all valid CreateCommentRequest inputs, the returned CommentDto.Status must equal "Active".

PROP-56: For all valid CreateCommentRequest inputs, the returned CommentDto.EditCount must equal 0.

PROP-57: For all valid CreateCommentRequest inputs, the returned CommentDto.EditedAt must be null.

PROP-58: For all valid CreateCommentRequest inputs, the returned CommentDto.PostId must equal the {postId} path parameter.

PROP-59: For all valid CreateCommentRequest inputs, the returned CommentDto.AuthorId must equal the authenticated user's ID.

PROP-60: For all valid CreateCommentRequest inputs where ParentId is non-null, the returned CommentDto.ParentId must equal the submitted CreateCommentRequest.ParentId.

PROP-61: For all valid CreateCommentRequest inputs where ParentId is null, the returned CommentDto.ParentId must be null.

### Output Completeness — Edit

PROP-62: For all valid PUT /api/comments/{id} requests, the returned CommentDto.Status must equal "Edited".

PROP-63: For all valid PUT /api/comments/{id} requests, the returned CommentDto.EditedAt must be a non-null UTC timestamp.

### Output Completeness — Delete

PROP-64: For all valid DELETE /api/comments/{id} requests, the returned CommentDto.Status must equal "Deleted".

### Output Completeness — Flag

PROP-65: For all valid PUT /api/comments/{id}/flag requests, the returned CommentDto.Status must equal "Flagged".

### Output Completeness — Moderate

PROP-66: For all valid PUT /api/comments/{id}/moderate requests with Decision "approve", the returned CommentDto.Status must equal "Approved".

PROP-67: For all valid PUT /api/comments/{id}/moderate requests with Decision "remove", the returned CommentDto.Status must equal "Removed".

### Moderation Decision Invariant

PROP-68: For all ModerateCommentRequest inputs, Decision must be exactly "approve" or "remove" (case-sensitive, no other values accepted).

### Field Immutability

PROP-69: For all CommentDto responses, PostId must never change after comment creation.

PROP-70: For all CommentDto responses, AuthorId must never change after comment creation.

PROP-71: For all CommentDto responses, ParentId must never change after comment creation.

### Consistency

PROP-72: For all POST /api/posts/{postId}/comments requests that return 201, the created comment must appear in the subsequent GET /api/posts/{postId}/comments response (subject to visibility rules based on the requesting user's role).

PROP-73: For all DELETE /api/comments/{id} requests that return 200 with Status "Deleted", the comment must no longer appear in GET /api/posts/{postId}/comments responses to non-admin users.

PROP-74: For all PUT /api/comments/{id}/flag requests that return 200 with Status "Flagged", the comment must no longer appear in GET /api/posts/{postId}/comments responses to non-admin users.

PROP-75: For all PUT /api/comments/{id}/moderate requests that return 200 with Status "Removed", the comment must no longer appear in GET /api/posts/{postId}/comments responses to non-admin users.

PROP-76: For all PUT /api/comments/{id}/moderate requests that return 200 with Status "Approved", the comment must appear in GET /api/posts/{postId}/comments responses to all users.

### Side-Effect Isolation

PROP-77: For all PUT /api/comments/{id}/flag requests, the operation must not alter Content, EditCount, EditedAt, CreatedAt, PostId, AuthorId, or ParentId of the comment.

PROP-78: For all DELETE /api/comments/{id} requests, the operation must not alter Content, EditCount, EditedAt, CreatedAt, PostId, AuthorId, or ParentId of the comment.

PROP-79: For all PUT /api/comments/{id}/moderate requests, the operation must not alter Content, EditCount, EditedAt, CreatedAt, PostId, AuthorId, or ParentId of the comment.

---

## Quality Checklist

- [x] Every endpoint has at least one Event-driven EARS requirement (REQ-10, REQ-17/18/19, REQ-22, REQ-33, REQ-39, REQ-45/46)
- [x] Every POST/PUT has at least one Unwanted EARS requirement (REQ-12/13/14/15/16, REQ-24/25/26/27/28/29-32, REQ-41-44, REQ-48/49/50-54)
- [x] Every auth-protected endpoint has an Unwanted requirement (REQ-1 through REQ-6)
- [x] Every path parameter has an Unwanted requirement for not found (REQ-11, REQ-20, REQ-23, REQ-34, REQ-40, REQ-47)
- [x] Every state transition is covered — valid transitions as Event-driven (REQ-22, REQ-33, REQ-39, REQ-45/46), invalid as Unwanted (REQ-29-32, REQ-35-38, REQ-41-44, REQ-50-54)
- [x] Every business rule has a Property or Unwanted requirement (edit limit REQ-27/PROP-21/50, 24h window REQ-28/PROP-41/42, nesting depth REQ-16/PROP-30/51/52, content length REQ-12-14/24-26/PROP-43-48, self-flag REQ-9/PROP-39, admin-only moderate REQ-6/PROP-40, visibility REQ-17/18/19/PROP-33-35, max 3 edits PROP-49/50)
- [x] All field names match CommentDto, CreateCommentRequest, UpdateCommentRequest, ModerateCommentRequest exactly
- [x] All status codes are explicit numbers (201, 200, 400, 401, 403, 404, 409)
- [x] Properties cover cross-cutting invariants (timestamps PROP-14-20, IDs PROP-1-6, ownership PROP-37-40, edit count PROP-21-25, immutability PROP-69-71, consistency PROP-72-76, side-effect isolation PROP-77-79)
- [x] No vague language anywhere
