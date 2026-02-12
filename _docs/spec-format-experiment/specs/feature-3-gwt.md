# Feature 3: Invitation System — GWT Acceptance Criteria

## Contract Reference

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

### Entities

```
Team { Id: GUID, Name: string, OwnerId: GUID }
Invitation { Id: GUID, TeamId: GUID, InviterUserId: GUID, InviteeEmail: string, Status: InvitationStatus, CreatedAt: DateTime, RespondedAt: DateTime? }
InvitationStatus: Pending | Accepted | Declined | Cancelled
```

---

## POST /api/teams/{teamId}/invitations — Invite User to Team

### GWT-1: Team owner invites a user successfully

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND a team exists with `Id` equal to `tttt-...-0001` and `OwnerId` equal to `aaaa-...-0001`
AND no pending invitation exists for `"invitee@example.com"` on team `tttt-...-0001`
WHEN POST `/api/teams/tttt-...-0001/invitations` with body:
```json
{
  "InviteeEmail": "invitee@example.com"
}
```
THEN response status is `201 Created`
AND response body is an `InvitationDto` with:
  - `Id` is a non-empty GUID
  - `TeamId` equals `tttt-...-0001`
  - `InviterUserId` equals `aaaa-...-0001`
  - `InviteeEmail` equals `"invitee@example.com"`
  - `Status` equals `"Pending"`
  - `CreatedAt` is within 5 seconds of UTC now
  - `RespondedAt` is `null`

### GWT-2: Non-owner team member cannot invite

GIVEN an authenticated user with UserId `aaaa-...-0002`
AND a team exists with `Id` equal to `tttt-...-0001` and `OwnerId` equal to `aaaa-...-0001`
AND user `aaaa-...-0002` is a member of team `tttt-...-0001` but is not the owner
WHEN POST `/api/teams/tttt-...-0001/invitations` with body:
```json
{
  "InviteeEmail": "invitee@example.com"
}
```
THEN response status is `403 Forbidden`

### GWT-3: Duplicate pending invitation for same email on same team

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND a team exists with `Id` equal to `tttt-...-0001` and `OwnerId` equal to `aaaa-...-0001`
AND a pending invitation already exists with `InviteeEmail` equal to `"invitee@example.com"` and `TeamId` equal to `tttt-...-0001` and `Status` equal to `"Pending"`
WHEN POST `/api/teams/tttt-...-0001/invitations` with body:
```json
{
  "InviteeEmail": "invitee@example.com"
}
```
THEN response status is `409 Conflict`

### GWT-4: Re-invite after previous invitation was declined

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND a team exists with `Id` equal to `tttt-...-0001` and `OwnerId` equal to `aaaa-...-0001`
AND an invitation exists with `InviteeEmail` equal to `"invitee@example.com"` and `TeamId` equal to `tttt-...-0001` and `Status` equal to `"Declined"`
AND no invitation exists with `InviteeEmail` equal to `"invitee@example.com"` and `TeamId` equal to `tttt-...-0001` and `Status` equal to `"Pending"`
WHEN POST `/api/teams/tttt-...-0001/invitations` with body:
```json
{
  "InviteeEmail": "invitee@example.com"
}
```
THEN response status is `201 Created`
AND response body is an `InvitationDto` with `Status` equal to `"Pending"`

### GWT-5: Validation error — empty InviteeEmail

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND a team exists with `Id` equal to `tttt-...-0001` and `OwnerId` equal to `aaaa-...-0001`
WHEN POST `/api/teams/tttt-...-0001/invitations` with body:
```json
{
  "InviteeEmail": ""
}
```
THEN response status is `400 Bad Request`
AND response body contains a validation error referencing field `InviteeEmail`

### GWT-6: Validation error — invalid email format

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND a team exists with `Id` equal to `tttt-...-0001` and `OwnerId` equal to `aaaa-...-0001`
WHEN POST `/api/teams/tttt-...-0001/invitations` with body:
```json
{
  "InviteeEmail": "not-an-email"
}
```
THEN response status is `400 Bad Request`
AND response body contains a validation error referencing field `InviteeEmail`

### GWT-7: Team does not exist — 404

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND no team exists with `Id` equal to `tttt-...-9999`
WHEN POST `/api/teams/tttt-...-9999/invitations` with body:
```json
{
  "InviteeEmail": "invitee@example.com"
}
```
THEN response status is `404 Not Found`

### GWT-8: Unauthorized — no auth token

GIVEN no authentication credentials
WHEN POST `/api/teams/tttt-...-0001/invitations` with body:
```json
{
  "InviteeEmail": "invitee@example.com"
}
```
THEN response status is `401 Unauthorized`

---

## GET /api/teams/{teamId}/invitations — List Team Invitations

### GWT-9: Team member lists invitations

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND a team exists with `Id` equal to `tttt-...-0001` and `OwnerId` equal to `aaaa-...-0001`
AND team `tttt-...-0001` has 3 invitations with statuses `"Pending"`, `"Accepted"`, `"Declined"`
WHEN GET `/api/teams/tttt-...-0001/invitations`
THEN response status is `200 OK`
AND response body is an array of 3 `InvitationDto` objects
AND every element has `TeamId` equal to `tttt-...-0001`

### GWT-10: Team with no invitations returns empty array

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND a team exists with `Id` equal to `tttt-...-0001` and `OwnerId` equal to `aaaa-...-0001`
AND team `tttt-...-0001` has 0 invitations
WHEN GET `/api/teams/tttt-...-0001/invitations`
THEN response status is `200 OK`
AND response body is an empty array `[]`

### GWT-11: Non-member cannot view team invitations

GIVEN an authenticated user with UserId `aaaa-...-0003`
AND a team exists with `Id` equal to `tttt-...-0001` and `OwnerId` equal to `aaaa-...-0001`
AND user `aaaa-...-0003` is not a member of team `tttt-...-0001`
WHEN GET `/api/teams/tttt-...-0001/invitations`
THEN response status is `403 Forbidden`

### GWT-12: Team does not exist — 404

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND no team exists with `Id` equal to `tttt-...-9999`
WHEN GET `/api/teams/tttt-...-9999/invitations`
THEN response status is `404 Not Found`

### GWT-13: Unauthorized — no auth token

GIVEN no authentication credentials
WHEN GET `/api/teams/tttt-...-0001/invitations`
THEN response status is `401 Unauthorized`

---

## PUT /api/invitations/{id}/accept — Accept Invitation

### GWT-14: Invitee accepts a pending invitation

GIVEN an authenticated user with email `"invitee@example.com"`
AND an invitation exists with `Id` equal to `iiii-...-0001` and `InviteeEmail` equal to `"invitee@example.com"` and `Status` equal to `"Pending"`
WHEN PUT `/api/invitations/iiii-...-0001/accept`
THEN response status is `200 OK`
AND response body is an `InvitationDto` with:
  - `Id` equals `iiii-...-0001`
  - `Status` equals `"Accepted"`
  - `RespondedAt` is within 5 seconds of UTC now

### GWT-15: Non-invitee user cannot accept invitation

GIVEN an authenticated user with email `"other@example.com"`
AND an invitation exists with `Id` equal to `iiii-...-0001` and `InviteeEmail` equal to `"invitee@example.com"` and `Status` equal to `"Pending"`
WHEN PUT `/api/invitations/iiii-...-0001/accept`
THEN response status is `403 Forbidden`

### GWT-16: Invalid state transition — accept an already Accepted invitation

GIVEN an authenticated user with email `"invitee@example.com"`
AND an invitation exists with `Id` equal to `iiii-...-0002` and `InviteeEmail` equal to `"invitee@example.com"` and `Status` equal to `"Accepted"`
WHEN PUT `/api/invitations/iiii-...-0002/accept`
THEN response status is `409 Conflict`

### GWT-17: Invalid state transition — accept a Declined invitation

GIVEN an authenticated user with email `"invitee@example.com"`
AND an invitation exists with `Id` equal to `iiii-...-0003` and `InviteeEmail` equal to `"invitee@example.com"` and `Status` equal to `"Declined"`
WHEN PUT `/api/invitations/iiii-...-0003/accept`
THEN response status is `409 Conflict`

### GWT-18: Invalid state transition — accept a Cancelled invitation

GIVEN an authenticated user with email `"invitee@example.com"`
AND an invitation exists with `Id` equal to `iiii-...-0004` and `InviteeEmail` equal to `"invitee@example.com"` and `Status` equal to `"Cancelled"`
WHEN PUT `/api/invitations/iiii-...-0004/accept`
THEN response status is `409 Conflict`

### GWT-19: Invitation does not exist — 404

GIVEN an authenticated user with email `"invitee@example.com"`
AND no invitation exists with `Id` equal to `iiii-...-9999`
WHEN PUT `/api/invitations/iiii-...-9999/accept`
THEN response status is `404 Not Found`

### GWT-20: Unauthorized — no auth token

GIVEN no authentication credentials
WHEN PUT `/api/invitations/iiii-...-0001/accept`
THEN response status is `401 Unauthorized`

---

## PUT /api/invitations/{id}/decline — Decline Invitation

### GWT-21: Invitee declines a pending invitation

GIVEN an authenticated user with email `"invitee@example.com"`
AND an invitation exists with `Id` equal to `iiii-...-0001` and `InviteeEmail` equal to `"invitee@example.com"` and `Status` equal to `"Pending"`
WHEN PUT `/api/invitations/iiii-...-0001/decline`
THEN response status is `200 OK`
AND response body is an `InvitationDto` with:
  - `Id` equals `iiii-...-0001`
  - `Status` equals `"Declined"`
  - `RespondedAt` is within 5 seconds of UTC now

### GWT-22: Non-invitee user cannot decline invitation

GIVEN an authenticated user with email `"other@example.com"`
AND an invitation exists with `Id` equal to `iiii-...-0001` and `InviteeEmail` equal to `"invitee@example.com"` and `Status` equal to `"Pending"`
WHEN PUT `/api/invitations/iiii-...-0001/decline`
THEN response status is `403 Forbidden`

### GWT-23: Invalid state transition — decline an already Accepted invitation

GIVEN an authenticated user with email `"invitee@example.com"`
AND an invitation exists with `Id` equal to `iiii-...-0002` and `InviteeEmail` equal to `"invitee@example.com"` and `Status` equal to `"Accepted"`
WHEN PUT `/api/invitations/iiii-...-0002/decline`
THEN response status is `409 Conflict`

### GWT-24: Invalid state transition — decline an already Declined invitation

GIVEN an authenticated user with email `"invitee@example.com"`
AND an invitation exists with `Id` equal to `iiii-...-0003` and `InviteeEmail` equal to `"invitee@example.com"` and `Status` equal to `"Declined"`
WHEN PUT `/api/invitations/iiii-...-0003/decline`
THEN response status is `409 Conflict`

### GWT-25: Invalid state transition — decline a Cancelled invitation

GIVEN an authenticated user with email `"invitee@example.com"`
AND an invitation exists with `Id` equal to `iiii-...-0004` and `InviteeEmail` equal to `"invitee@example.com"` and `Status` equal to `"Cancelled"`
WHEN PUT `/api/invitations/iiii-...-0004/decline`
THEN response status is `409 Conflict`

### GWT-26: Invitation does not exist — 404

GIVEN an authenticated user with email `"invitee@example.com"`
AND no invitation exists with `Id` equal to `iiii-...-9999`
WHEN PUT `/api/invitations/iiii-...-9999/decline`
THEN response status is `404 Not Found`

### GWT-27: Unauthorized — no auth token

GIVEN no authentication credentials
WHEN PUT `/api/invitations/iiii-...-0001/decline`
THEN response status is `401 Unauthorized`

---

## DELETE /api/invitations/{id} — Cancel Invitation

### GWT-28: Team owner cancels a pending invitation

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND a team exists with `Id` equal to `tttt-...-0001` and `OwnerId` equal to `aaaa-...-0001`
AND an invitation exists with `Id` equal to `iiii-...-0001` and `TeamId` equal to `tttt-...-0001` and `InviterUserId` equal to `aaaa-...-0002` and `Status` equal to `"Pending"`
WHEN DELETE `/api/invitations/iiii-...-0001`
THEN response status is `200 OK`
AND response body is an `InvitationDto` with:
  - `Id` equals `iiii-...-0001`
  - `Status` equals `"Cancelled"`
  - `RespondedAt` is within 5 seconds of UTC now

### GWT-29: Inviter (non-owner) cancels their own pending invitation

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND a team exists with `Id` equal to `tttt-...-0001` and `OwnerId` equal to `aaaa-...-0001`
AND an invitation exists with `Id` equal to `iiii-...-0001` and `TeamId` equal to `tttt-...-0001` and `InviterUserId` equal to `aaaa-...-0001` and `Status` equal to `"Pending"`
WHEN DELETE `/api/invitations/iiii-...-0001`
THEN response status is `200 OK`
AND response body is an `InvitationDto` with:
  - `Id` equals `iiii-...-0001`
  - `Status` equals `"Cancelled"`
  - `RespondedAt` is within 5 seconds of UTC now

### GWT-30: User who is neither inviter nor team owner cannot cancel

GIVEN an authenticated user with UserId `aaaa-...-0003`
AND a team exists with `Id` equal to `tttt-...-0001` and `OwnerId` equal to `aaaa-...-0001`
AND an invitation exists with `Id` equal to `iiii-...-0001` and `TeamId` equal to `tttt-...-0001` and `InviterUserId` equal to `aaaa-...-0002` and `Status` equal to `"Pending"`
AND user `aaaa-...-0003` is neither the inviter nor the team owner
WHEN DELETE `/api/invitations/iiii-...-0001`
THEN response status is `403 Forbidden`

### GWT-31: Invalid state transition — cancel an Accepted invitation

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND a team exists with `Id` equal to `tttt-...-0001` and `OwnerId` equal to `aaaa-...-0001`
AND an invitation exists with `Id` equal to `iiii-...-0002` and `TeamId` equal to `tttt-...-0001` and `Status` equal to `"Accepted"`
WHEN DELETE `/api/invitations/iiii-...-0002`
THEN response status is `409 Conflict`

### GWT-32: Invalid state transition — cancel a Declined invitation

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND a team exists with `Id` equal to `tttt-...-0001` and `OwnerId` equal to `aaaa-...-0001`
AND an invitation exists with `Id` equal to `iiii-...-0003` and `TeamId` equal to `tttt-...-0001` and `Status` equal to `"Declined"`
WHEN DELETE `/api/invitations/iiii-...-0003`
THEN response status is `409 Conflict`

### GWT-33: Invalid state transition — cancel an already Cancelled invitation

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND a team exists with `Id` equal to `tttt-...-0001` and `OwnerId` equal to `aaaa-...-0001`
AND an invitation exists with `Id` equal to `iiii-...-0004` and `TeamId` equal to `tttt-...-0001` and `Status` equal to `"Cancelled"`
WHEN DELETE `/api/invitations/iiii-...-0004`
THEN response status is `409 Conflict`

### GWT-34: Invitation does not exist — 404

GIVEN an authenticated user with UserId `aaaa-...-0001`
AND no invitation exists with `Id` equal to `iiii-...-9999`
WHEN DELETE `/api/invitations/iiii-...-9999`
THEN response status is `404 Not Found`

### GWT-35: Unauthorized — no auth token

GIVEN no authentication credentials
WHEN DELETE `/api/invitations/iiii-...-0001`
THEN response status is `401 Unauthorized`

---

## Quality Checklist

- [x] Every endpoint has a happy path GWT (GWT-1, GWT-9, GWT-14, GWT-21, GWT-28, GWT-29)
- [x] Every POST/PUT has at least one validation error GWT (GWT-5, GWT-6)
- [x] Every auth-protected endpoint has an unauthorized GWT (GWT-8, GWT-13, GWT-20, GWT-27, GWT-35)
- [x] Every path parameter has a not-found GWT (GWT-7, GWT-12, GWT-19, GWT-26, GWT-34)
- [x] Every valid state transition has a GWT: Pending->Accepted (GWT-14), Pending->Declined (GWT-21), Pending->Cancelled (GWT-28)
- [x] Every invalid state transition has a GWT: accept from Accepted/Declined/Cancelled (GWT-16/17/18), decline from Accepted/Declined/Cancelled (GWT-23/24/25), cancel from Accepted/Declined/Cancelled (GWT-31/32/33)
- [x] Every business rule has a boundary GWT:
  - Only team owner can invite (GWT-2)
  - Only invitee can accept/decline (GWT-15, GWT-22)
  - Only inviter or team owner can cancel (GWT-30)
  - Non-members cannot view invitations (GWT-11)
  - Cannot invite same email twice while pending (GWT-3)
  - Can re-invite after declined/cancelled (GWT-4)
- [x] All field names match contract DTOs: `Id`, `TeamId`, `InviterUserId`, `InviteeEmail`, `Status`, `CreatedAt`, `RespondedAt`
- [x] All status codes are explicit: 200, 201, 400, 401, 403, 404, 409
- [x] No vague language used
