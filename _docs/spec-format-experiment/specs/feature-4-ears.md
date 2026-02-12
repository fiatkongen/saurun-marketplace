# Feature 4: Comment Thread with Moderation — EARS Requirements + Property Descriptions

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

### POST /api/posts/{postId}/comments — Create Comment

REQ-1: When an authenticated user sends a POST to /api/posts/{postId}/comments with a valid CreateCommentRequest, the system shall create a new comment with status "Active", EditCount 0, EditedAt null, and return 201 with a CommentDto.

REQ-2: If the requesting user is not authenticated, then the system shall return 401.

REQ-3: If no post exists with the given {postId}, then the system shall return 404.

REQ-4: If the CreateCommentRequest.Content is null or empty, then the system shall return 400.

REQ-5: If the CreateCommentRequest.Content length exceeds 5000 characters, then the system shall return 400.

REQ-6: If the CreateCommentRequest.ParentId is non-null and no comment exists with that ParentId on the post identified by {postId}, then the system shall return 400.

REQ-7: If the CreateCommentRequest.ParentId is non-null and the resulting nesting depth would exceed 3 levels, then the system shall return 400.

### GET /api/posts/{postId}/comments — List Comments

REQ-8: When an unauthenticated user sends a GET to /api/posts/{postId}/comments, the system shall return 200 with a list of CommentDto excluding any comments with status "Flagged" or "Removed".

REQ-9: When an authenticated non-admin user sends a GET to /api/posts/{postId}/comments, the system shall return 200 with a list of CommentDto excluding any comments with status "Flagged" or "Removed".

REQ-10: When an authenticated admin user sends a GET to /api/posts/{postId}/comments, the system shall return 200 with a list of all CommentDto for that post regardless of status.

REQ-11: If no post exists with the given {postId}, then the system shall return 404.

### PUT /api/comments/{id} — Edit Comment

REQ-12: When an authenticated user who is the author of the comment sends a PUT to /api/comments/{id} with a valid UpdateCommentRequest, and the comment has status "Active" or "Edited", and EditCount is less than 3, and the comment's CreatedAt is within 24 hours of the current UTC time, the system shall update the comment content, increment EditCount by 1, set EditedAt to the current UTC timestamp, transition status to "Edited", and return 200 with a CommentDto.

REQ-13: If the requesting user is not authenticated, then the system shall return 401.

REQ-14: If the requesting user is not the AuthorId of the comment identified by {id}, then the system shall return 403.

REQ-15: If no comment exists with the given {id}, then the system shall return 404.

REQ-16: If the UpdateCommentRequest.Content is null or empty, then the system shall return 400.

REQ-17: If the UpdateCommentRequest.Content length exceeds 5000 characters, then the system shall return 400.

REQ-18: If the comment identified by {id} has EditCount equal to 3, then the system shall return 409.

REQ-19: If the comment identified by {id} has a CreatedAt older than 24 hours from the current UTC time, then the system shall return 409.

REQ-20: If the comment identified by {id} has status "Flagged", "Deleted", "Approved", or "Removed", then the system shall return 409.

### DELETE /api/comments/{id} — Soft Delete Comment

REQ-21: When an authenticated user who is the author of the comment sends a DELETE to /api/comments/{id}, and the comment has status "Active" or "Edited", the system shall transition the comment status to "Deleted" and return 200 with a CommentDto.

REQ-22: If the requesting user is not authenticated, then the system shall return 401.

REQ-23: If the requesting user is not the AuthorId of the comment identified by {id}, then the system shall return 403.

REQ-24: If no comment exists with the given {id}, then the system shall return 404.

REQ-25: If the comment identified by {id} has status "Flagged", "Deleted", "Approved", or "Removed", then the system shall return 409.

### PUT /api/comments/{id}/flag — Flag Comment

REQ-26: When an authenticated user who is not the author of the comment sends a PUT to /api/comments/{id}/flag, and the comment has status "Active" or "Edited", the system shall transition the comment status to "Flagged" and return 200 with a CommentDto.

REQ-27: If the requesting user is not authenticated, then the system shall return 401.

REQ-28: If the requesting user is the AuthorId of the comment identified by {id}, then the system shall return 403.

REQ-29: If no comment exists with the given {id}, then the system shall return 404.

REQ-30: If the comment identified by {id} has status "Flagged", "Deleted", "Approved", or "Removed", then the system shall return 409.

### PUT /api/comments/{id}/moderate — Moderate Flagged Comment

REQ-31: When an authenticated admin user sends a PUT to /api/comments/{id}/moderate with ModerateCommentRequest.Decision equal to "approve", and the comment has status "Flagged", the system shall transition the comment status to "Approved" and return 200 with a CommentDto.

REQ-32: When an authenticated admin user sends a PUT to /api/comments/{id}/moderate with ModerateCommentRequest.Decision equal to "remove", and the comment has status "Flagged", the system shall transition the comment status to "Removed" and return 200 with a CommentDto.

REQ-33: If the requesting user is not authenticated, then the system shall return 401.

REQ-34: If the requesting user is not an admin, then the system shall return 403.

REQ-35: If no comment exists with the given {id}, then the system shall return 404.

REQ-36: If the ModerateCommentRequest.Decision is not "approve" or "remove", then the system shall return 400.

REQ-37: If the comment identified by {id} has a status other than "Flagged", then the system shall return 409.

---

## Property Descriptions

### Identity & Referential Integrity

PROP-1: For all CommentDto responses, Id must be a non-empty GUID that uniquely identifies the comment.

PROP-2: For all CommentDto responses, PostId must match the GUID of an existing post.

PROP-3: For all CommentDto responses, AuthorId must match the GUID of the authenticated user who created the comment.

PROP-4: For all CommentDto responses where ParentId is non-null, ParentId must match the Id of an existing comment on the same post.

### Status Invariants

PROP-5: For all CommentDto responses, Status must be one of: "Active", "Edited", "Flagged", "Deleted", "Approved", "Removed".

PROP-6: For all Comment entities in state "Active", the only valid transitions are "Edited", "Flagged", "Deleted".

PROP-7: For all Comment entities in state "Edited", the only valid transitions are "Flagged", "Deleted".

PROP-8: For all Comment entities in state "Flagged", the only valid transitions are "Approved", "Removed".

PROP-9: For all Comment entities in state "Deleted", no status transitions are valid.

PROP-10: For all Comment entities in state "Approved", no status transitions are valid.

PROP-11: For all Comment entities in state "Removed", no status transitions are valid.

### Timestamp Invariants

PROP-12: For all CommentDto responses, CreatedAt must be a UTC timestamp equal to the time the comment was created and must never change after creation.

PROP-13: For all CommentDto responses where Status is "Active", EditedAt must be null.

PROP-14: For all CommentDto responses where EditCount is greater than 0, EditedAt must be a non-null UTC timestamp greater than or equal to CreatedAt.

PROP-15: For all CommentDto responses, EditedAt must never decrease — each edit must set EditedAt to a value greater than or equal to the previous EditedAt.

### EditCount Invariants

PROP-16: For all CommentDto responses, EditCount must be an integer between 0 and 3 inclusive.

PROP-17: For all CommentDto responses where Status is "Active", EditCount must equal 0.

PROP-18: For all valid PUT /api/comments/{id} requests, the returned CommentDto.EditCount must equal the previous EditCount plus 1.

PROP-19: For all CommentDto responses, EditCount must never decrease.

### Content Invariants

PROP-20: For all CommentDto responses, Content must be a non-empty string with length between 1 and 5000 characters inclusive.

PROP-21: For all valid CreateCommentRequest inputs, the returned CommentDto.Content must equal the submitted CreateCommentRequest.Content.

PROP-22: For all valid UpdateCommentRequest inputs, the returned CommentDto.Content must equal the submitted UpdateCommentRequest.Content.

### Nesting Invariants

PROP-23: For all CommentDto responses where ParentId is non-null, the chain of ParentId references must not exceed 3 levels of depth.

PROP-24: For all CommentDto responses where ParentId is null, the comment is a root-level comment (depth 1).

### Visibility Invariants

PROP-25: For all GET /api/posts/{postId}/comments responses to unauthenticated or non-admin users, no CommentDto in the list must have Status equal to "Flagged" or "Removed".

PROP-26: For all GET /api/posts/{postId}/comments responses to admin users, the list must include CommentDto of every status.

### Authorization Boundaries

PROP-27: For all PUT /api/comments/{id} requests, only the user whose Id matches CommentDto.AuthorId is authorized (all others receive 403).

PROP-28: For all DELETE /api/comments/{id} requests, only the user whose Id matches CommentDto.AuthorId is authorized (all others receive 403).

PROP-29: For all PUT /api/comments/{id}/flag requests, any authenticated user except the user whose Id matches CommentDto.AuthorId is authorized (the author receives 403).

PROP-30: For all PUT /api/comments/{id}/moderate requests, only admin users are authorized (all non-admin users receive 403).

### Edit Window Boundary

PROP-31: For all PUT /api/comments/{id} requests where the comment's CreatedAt is exactly 24 hours before the current UTC time, the system must return 409.

PROP-32: For all PUT /api/comments/{id} requests where the comment's CreatedAt is less than 24 hours before the current UTC time and EditCount is less than 3, and status is "Active" or "Edited", and Content is valid, the system must return 200.

### Boundary Constraints

PROP-33: For all CreateCommentRequest inputs where Content has length 1, the API must return 201 with a CommentDto.

PROP-34: For all CreateCommentRequest inputs where Content has length 5000, the API must return 201 with a CommentDto.

PROP-35: For all CreateCommentRequest inputs where Content has length 5001, the API must return 400.

PROP-36: For all UpdateCommentRequest inputs where Content has length 1, the API must return 200 with a CommentDto (assuming all other preconditions are met).

PROP-37: For all UpdateCommentRequest inputs where Content has length 5000, the API must return 200 with a CommentDto (assuming all other preconditions are met).

PROP-38: For all UpdateCommentRequest inputs where Content has length 5001, the API must return 400.

### Idempotency

PROP-39: For all DELETE /api/comments/{id} requests where the comment has already been soft-deleted (status "Deleted"), repeated calls must return 409.

### Output Completeness

PROP-40: For all valid CreateCommentRequest inputs, the returned CommentDto.Status must equal "Active".

PROP-41: For all valid CreateCommentRequest inputs, the returned CommentDto.EditCount must equal 0.

PROP-42: For all valid CreateCommentRequest inputs, the returned CommentDto.EditedAt must be null.

PROP-43: For all valid PUT /api/comments/{id} requests, the returned CommentDto.Status must equal "Edited".

PROP-44: For all valid DELETE /api/comments/{id} requests, the returned CommentDto.Status must equal "Deleted".

PROP-45: For all valid PUT /api/comments/{id}/flag requests, the returned CommentDto.Status must equal "Flagged".

PROP-46: For all valid PUT /api/comments/{id}/moderate requests with Decision "approve", the returned CommentDto.Status must equal "Approved".

PROP-47: For all valid PUT /api/comments/{id}/moderate requests with Decision "remove", the returned CommentDto.Status must equal "Removed".

### Moderation Decision Invariant

PROP-48: For all ModerateCommentRequest inputs, Decision must be exactly "approve" or "remove" (case-sensitive, no other values accepted).

---

## Quality Checklist

- [x] Every endpoint has at least one Event-driven EARS requirement (REQ-1, REQ-8/9/10, REQ-12, REQ-21, REQ-26, REQ-31/32)
- [x] Every POST/PUT has at least one Unwanted EARS requirement (REQ-4/5/6/7, REQ-16/17/18/19/20, REQ-30, REQ-36/37)
- [x] Every auth-protected endpoint has an Unwanted requirement (REQ-2, REQ-13, REQ-22, REQ-27, REQ-33/34)
- [x] Every path parameter has an Unwanted requirement for not found (REQ-3, REQ-11, REQ-15, REQ-24, REQ-29, REQ-35)
- [x] Every state transition is covered — valid transitions as Event-driven (REQ-12, REQ-21, REQ-26, REQ-31/32), invalid as Unwanted (REQ-20, REQ-25, REQ-30, REQ-37)
- [x] Every business rule has a Property or Unwanted requirement (edit limit REQ-18/PROP-16, 24h window REQ-19/PROP-31, nesting depth REQ-7/PROP-23, content length REQ-4/5/16/17/PROP-33-38, self-flag REQ-28/PROP-29, admin-only moderate REQ-34/PROP-30, visibility PROP-25/26)
- [x] All field names match CommentDto, CreateCommentRequest, UpdateCommentRequest, ModerateCommentRequest exactly
- [x] All status codes are explicit numbers (201, 200, 400, 401, 403, 404, 409)
- [x] Properties cover cross-cutting invariants (timestamps PROP-12/13/14/15, IDs PROP-1/2/3/4, ownership PROP-27/28/29/30, edit count PROP-16/17/18/19)
- [x] No vague language anywhere
