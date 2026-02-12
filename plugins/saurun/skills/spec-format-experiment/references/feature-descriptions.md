# Test Features for Spec Format Experiment

5 features of escalating complexity. Each feature includes a description, endpoints, entities, rules, and a completeness checklist for scoring.

---

## Feature 1: Bookmark CRUD (Simple Baseline)

**Complexity:** Low — single entity, 4 endpoints, no state transitions.
**What it tests:** Baseline format comparison.

### Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | /api/bookmarks | Required | Create bookmark |
| GET | /api/bookmarks | Required | List user's bookmarks |
| GET | /api/bookmarks/{id} | Required | Get single bookmark |
| DELETE | /api/bookmarks/{id} | Required | Delete bookmark |

### Entity

```
Bookmark { id: GUID, url: string, title: string, description: string?, userId: GUID, createdAt: DateTime }
```

### Rules

- url: required, must be valid URL format
- title: required, 1-200 characters
- description: optional, max 1000 characters
- Users can only see/delete their own bookmarks
- Deleting a nonexistent bookmark returns 404
- Listing returns bookmarks ordered by createdAt descending

### Contract Types (C#)

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

### Completeness Checklist (16 items)

- [ ] POST happy path → 201 + BookmarkDto
- [ ] POST empty url → 400
- [ ] POST invalid URL format → 400
- [ ] POST empty title → 400
- [ ] POST title > 200 chars → 400
- [ ] POST description > 1000 chars → 400
- [ ] POST no auth → 401
- [ ] GET list happy path → 200 + BookmarkDto[]
- [ ] GET list no auth → 401
- [ ] GET single happy path → 200 + BookmarkDto
- [ ] GET single not found → 404
- [ ] GET single other user's bookmark → 403 or 404
- [ ] GET single no auth → 401
- [ ] DELETE happy path → 204
- [ ] DELETE other user's bookmark → 403 or 404
- [ ] DELETE no auth → 401

---

## Feature 2: File Upload with Validation (Validation-Heavy)

**Complexity:** Medium — single endpoint focus but extensive validation rules and error responses.
**What it tests:** How well each format expresses complex validation constraints.

### Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | /api/uploads | Required | Upload file (multipart) |
| GET | /api/uploads/{id} | Required | Get upload metadata |
| GET | /api/uploads | Required | List user's uploads |

### Entity

```
Upload { id: GUID, fileName: string, fileSize: long, mimeType: string, storagePath: string, userId: GUID, createdAt: DateTime }
```

### Rules

- Max file size: 10MB (10,485,760 bytes)
- Allowed MIME types: image/jpeg, image/png, image/webp, application/pdf
- Filename must be sanitized: no path traversal characters (../, ..\, /, \)
- Empty file (0 bytes) rejected
- Filename max 255 characters
- Users can only see their own uploads
- Oversized file returns 413 (not 400)
- Wrong MIME type returns 415 (not 400)

### Contract Types (C#)

```csharp
public record UploadDto(Guid Id, string FileName, long FileSize, string MimeType, Guid UserId, DateTime CreatedAt);

public static class UploadRoutes
{
    public const string Upload = "/api/uploads";
    public const string Get = "/api/uploads/{id}";
    public const string List = "/api/uploads";
}
```

### Completeness Checklist (16 items)

- [ ] POST happy path (valid image) → 201 + UploadDto
- [ ] POST happy path (valid PDF) → 201 + UploadDto
- [ ] POST file > 10MB → 413
- [ ] POST unsupported MIME type → 415
- [ ] POST empty file (0 bytes) → 400
- [ ] POST no file attached → 400
- [ ] POST filename with path traversal → 400
- [ ] POST filename > 255 chars → 400
- [ ] POST no auth → 401
- [ ] GET single happy path → 200 + UploadDto
- [ ] GET single not found → 404
- [ ] GET single no auth → 401
- [ ] GET single other user's upload → 403 or 404
- [ ] GET list happy path → 200 + UploadDto[]
- [ ] GET list other user's uploads → only returns own uploads
- [ ] GET list no auth → 401

> **Note:** `storagePath` is intentionally omitted from `UploadDto` — it is an internal field not exposed to API consumers.

---

## Feature 3: Invitation System (Authorization Boundaries)

**Complexity:** Medium-high — multiple roles, authorization rules that vary by context.
**What it tests:** How each format handles authorization boundaries and role-based access.

### Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | /api/teams/{teamId}/invitations | Required | Invite user to team |
| GET | /api/teams/{teamId}/invitations | Required | List team invitations |
| PUT | /api/invitations/{id}/accept | Required | Accept invitation |
| PUT | /api/invitations/{id}/decline | Required | Decline invitation |
| DELETE | /api/invitations/{id} | Required | Cancel invitation |

### Entities

```
Team { id: GUID, name: string, ownerId: GUID }
Invitation { id: GUID, teamId: GUID, inviterUserId: GUID, inviteeEmail: string, status: InvitationStatus, createdAt: DateTime, respondedAt: DateTime? }
InvitationStatus: Pending | Accepted | Declined | Cancelled
```

### Rules

- Only team owner can invite
- Only the invitee (matched by email from auth token) can accept/decline
- Only the inviter OR team owner can cancel
- Non-members cannot view team invitations
- Cannot invite same email twice to same team while an invitation is pending
- Status transitions: Pending → Accepted, Pending → Declined, Pending → Cancelled. No other transitions.
- Accepted/Declined/Cancelled invitations cannot be modified
- Team must exist (404 if not)
- inviteeEmail must be valid email format

### Contract Types (C#)

```csharp
public record InvitationDto(Guid Id, Guid TeamId, Guid InviterUserId, string InviteeEmail, string Status, DateTime CreatedAt, DateTime? RespondedAt);
public record CreateInvitationRequest(string InviteeEmail);

public static class InvitationRoutes
{
    public const string Create = "/api/teams/{teamId}/invitations";
    public const string List = "/api/teams/{teamId}/invitations";
    public const string Accept = "/api/invitations/{id}/accept";
    public const string Decline = "/api/invitations/{id}/decline";
    public const string Cancel = "/api/invitations/{id}";
}
```

### Completeness Checklist (18 items)

- [ ] POST invite happy path → 201 + InvitationDto (status = "Pending")
- [ ] POST invite as non-owner → 403
- [ ] POST invite no auth → 401
- [ ] POST invite invalid email → 400
- [ ] POST invite duplicate pending → 409
- [ ] POST invite to nonexistent team → 404
- [ ] GET list as team member → 200 + InvitationDto[]
- [ ] GET list as non-member → 403
- [ ] PUT accept as invitee → 200 + InvitationDto (status = "Accepted")
- [ ] PUT accept as non-invitee → 403
- [ ] PUT accept already accepted → 409 (Conflict — invitation already actioned)
- [ ] PUT decline as invitee → 200 + InvitationDto (status = "Declined")
- [ ] PUT decline as non-invitee → 403
- [ ] PUT decline already declined → 409 (Conflict — invitation already actioned)
- [ ] DELETE cancel as inviter → 204
- [ ] DELETE cancel as team owner → 204
- [ ] DELETE cancel as other user → 403
- [ ] DELETE cancel already accepted → 409 (Conflict — invitation already actioned)

---

## Feature 4: Comment Thread with Moderation (State Machine)

**Complexity:** High — nested entity with state transitions, soft delete, moderation rules.
**What it tests:** State machines and nested entity relationships.

### Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | /api/posts/{postId}/comments | Required | Create comment |
| GET | /api/posts/{postId}/comments | Public | List comments on post |
| PUT | /api/comments/{id} | Required | Edit comment |
| DELETE | /api/comments/{id} | Required | Delete comment (soft) |
| PUT | /api/comments/{id}/flag | Required | Flag comment |
| PUT | /api/comments/{id}/moderate | Required (Admin) | Moderate flagged comment |

### Entity

```
Comment { id: GUID, postId: GUID, authorId: GUID, parentId: GUID?, content: string, status: CommentStatus, editCount: int, createdAt: DateTime, editedAt: DateTime? }
CommentStatus: Active | Edited | Flagged | Deleted | Approved | Removed
```

### Rules

**State transitions:**
- Active → Edited (on PUT), Active → Flagged (on flag), Active → Deleted (on DELETE)
- Edited → Flagged (on flag), Edited → Deleted (on DELETE)
- Flagged → Approved (admin moderate), Flagged → Removed (admin moderate)
- No other transitions allowed (e.g., Deleted → Active is invalid)

**Business rules:**
- Max 3 edits per comment (editCount tracks)
- Comments older than 24 hours cannot be edited
- Flagged comments are hidden from non-admins in listings (still exist, just filtered)
- Nested comments max depth 3 (parentId chain)
- Content: required, 1-5000 characters
- Only the author can edit/delete their own comments
- Any authenticated user can flag a comment (not their own)
- Only admins can moderate

### Contract Types (C#)

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

### Completeness Checklist (28 items)

- [ ] POST create happy path → 201 + CommentDto (status = "Active")
- [ ] POST create with parentId (reply) → 201
- [ ] POST create depth > 3 → 400
- [ ] POST create empty content → 400
- [ ] POST create content > 5000 chars → 400
- [ ] POST create no auth → 401
- [ ] POST create on nonexistent post → 404
- [ ] GET list happy path → 200 + CommentDto[] (flagged filtered for non-admins)
- [ ] GET list as admin → 200 + includes flagged comments
- [ ] PUT edit happy path → 200 + CommentDto (status = "Edited", editCount incremented)
- [ ] PUT edit no auth → 401
- [ ] PUT edit by non-author → 403
- [ ] PUT edit after 3 edits → 400 or 409
- [ ] PUT edit after 24 hours → 400 or 409
- [ ] PUT edit deleted comment → 400 or 409
- [ ] PUT edit flagged comment → 400 or 409
- [ ] DELETE happy path → 204 (status → "Deleted")
- [ ] DELETE no auth → 401
- [ ] DELETE by non-author → 403
- [ ] DELETE already deleted → 404 or 409
- [ ] PUT flag happy path → 200 (status → "Flagged")
- [ ] PUT flag no auth → 401
- [ ] PUT flag own comment → 400 or 403
- [ ] PUT flag deleted comment → 400 or 409
- [ ] PUT moderate approve → 200 (status → "Approved")
- [ ] PUT moderate remove → 200 (status → "Removed")
- [ ] PUT moderate no auth → 401
- [ ] PUT moderate by non-admin → 403

---

## Feature 5: Recipe Search with Faceted Filtering (Cross-Entity Query)

**Complexity:** High — multiple filter parameters, pagination, sorting, cross-entity joins.
**What it tests:** Query parameter combinations, pagination behavior, sorting edge cases.

### Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | /api/recipes/search | Public | Search recipes with filters |

### Entities (assumed existing)

```
Recipe { id: GUID, title: string, description: string, category: string, prepTimeMinutes: int, authorId: GUID, createdAt: DateTime }
Rating { recipeId: GUID, userId: GUID, value: int (1-5) }
```

### Query Parameters

| Param | Type | Required | Default | Validation |
|-------|------|----------|---------|------------|
| q | string | No | null | Min 2 chars if provided |
| category | string | No | null | Must be valid category enum |
| maxPrepTime | int | No | null | Must be > 0 |
| minRating | decimal | No | null | Must be 1.0-5.0 |
| sort | string | No | see below | Must be: relevance, newest, rating, prepTime |
| page | int | No | 1 | Must be >= 1 |
| pageSize | int | No | 20 | Must be 10-50 |

### Rules

- Default sort: "relevance" when q is present, "newest" when q is absent
- q searches title + description (case-insensitive, partial match)
- category is exact match
- maxPrepTime: recipes with prepTimeMinutes <= value
- minRating: recipes with average rating >= value
- Empty results return 200 with empty items (NOT 404)
- Invalid sort value → 400
- page < 1 → 400
- pageSize outside 10-50 → 400
- q provided but < 2 chars → 400
- Filters combine with AND logic

### Contract Types (C#)

```csharp
public record RecipeSearchDto(Guid Id, string Title, string Description, string Category, int PrepTimeMinutes, decimal AverageRating, int RatingCount, Guid AuthorId, DateTime CreatedAt);
public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize, int TotalPages, bool HasNextPage);

public static class SearchRoutes
{
    public const string Search = "/api/recipes/search";
}
```

### Completeness Checklist (19 items)

- [ ] GET search no filters → 200 + PagedResult (default sort: newest)
- [ ] GET search with q → 200 + results matching title/description
- [ ] GET search with q → default sort is relevance
- [ ] GET search with category filter → 200 + only matching category
- [ ] GET search with maxPrepTime → 200 + only recipes <= time
- [ ] GET search with minRating → 200 + only recipes >= rating
- [ ] GET search with combined filters → AND logic applied
- [ ] GET search sort=rating → ordered by average rating descending
- [ ] GET search sort=prepTime → ordered by prepTimeMinutes ascending
- [ ] GET search pagination (page=2, pageSize=10) → correct slice
- [ ] GET search empty results → 200 + empty items array (not 404)
- [ ] GET search invalid sort → 400
- [ ] GET search invalid category → 400
- [ ] GET search maxPrepTime <= 0 → 400
- [ ] GET search minRating < 1.0 or > 5.0 → 400
- [ ] GET search page < 1 → 400
- [ ] GET search pageSize < 10 → 400
- [ ] GET search pageSize > 50 → 400
- [ ] GET search q with 1 char → 400
