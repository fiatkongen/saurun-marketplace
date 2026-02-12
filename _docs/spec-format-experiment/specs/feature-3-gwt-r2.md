# Feature 3: Invitation System — GWT Acceptance Criteria

## Contract References

```csharp
public record InvitationDto(Guid Id, Guid TeamId, Guid InviterUserId, string InviteeEmail, string Status, DateTime CreatedAt, DateTime? RespondedAt);
public record CreateInvitationRequest(string InviteeEmail);
```

---

## POST /api/teams/{teamId}/invitations — Invite User to Team

### GWT-1: Team owner creates invitation successfully

GIVEN authenticated user "owner@test.com" is the owner of team {teamId}
WHEN POST /api/teams/{teamId}/invitations with body `{ "InviteeEmail": "newuser@test.com" }`
THEN response status is 201
AND response body is `InvitationDto` with:
  - `Id` is a non-empty GUID
  - `TeamId` equals {teamId}
  - `InviterUserId` equals the authenticated user's ID
  - `InviteeEmail` equals "newuser@test.com"
  - `Status` equals "Pending"
  - `CreatedAt` is within 5 seconds of now (UTC)
  - `RespondedAt` is null

### GWT-2: Non-owner member attempts to invite — forbidden

GIVEN authenticated user "member@test.com" is a member (not owner) of team {teamId}
WHEN POST /api/teams/{teamId}/invitations with body `{ "InviteeEmail": "newuser@test.com" }`
THEN response status is 403

### GWT-3: Unauthenticated request — unauthorized

GIVEN no authentication token is provided
WHEN POST /api/teams/{teamId}/invitations with body `{ "InviteeEmail": "newuser@test.com" }`
THEN response status is 401

### GWT-4: Team does not exist — not found

GIVEN authenticated user "owner@test.com"
AND no team exists with ID {teamId}
WHEN POST /api/teams/{teamId}/invitations with body `{ "InviteeEmail": "newuser@test.com" }`
THEN response status is 404

### GWT-5: Invalid email format — validation error

GIVEN authenticated user "owner@test.com" is the owner of team {teamId}
WHEN POST /api/teams/{teamId}/invitations with body `{ "InviteeEmail": "not-an-email" }`
THEN response status is 400
AND response body contains a validation error referencing field `InviteeEmail`

### GWT-6: Empty InviteeEmail — validation error

GIVEN authenticated user "owner@test.com" is the owner of team {teamId}
WHEN POST /api/teams/{teamId}/invitations with body `{ "InviteeEmail": "" }`
THEN response status is 400
AND response body contains a validation error referencing field `InviteeEmail`

### GWT-7: Duplicate pending invitation for same email and team — conflict

GIVEN authenticated user "owner@test.com" is the owner of team {teamId}
AND a pending invitation already exists for "existing@test.com" on team {teamId}
WHEN POST /api/teams/{teamId}/invitations with body `{ "InviteeEmail": "existing@test.com" }`
THEN response status is 409

### GWT-8: Re-invite after previous invitation was declined — succeeds

GIVEN authenticated user "owner@test.com" is the owner of team {teamId}
AND a declined invitation exists for "previously-declined@test.com" on team {teamId} (Status = "Declined")
WHEN POST /api/teams/{teamId}/invitations with body `{ "InviteeEmail": "previously-declined@test.com" }`
THEN response status is 201
AND response body is `InvitationDto` with `Status` equals "Pending"
AND response body `InviteeEmail` equals "previously-declined@test.com"

### GWT-9: Re-invite after previous invitation was cancelled — succeeds

GIVEN authenticated user "owner@test.com" is the owner of team {teamId}
AND a cancelled invitation exists for "previously-cancelled@test.com" on team {teamId} (Status = "Cancelled")
WHEN POST /api/teams/{teamId}/invitations with body `{ "InviteeEmail": "previously-cancelled@test.com" }`
THEN response status is 201
AND response body is `InvitationDto` with `Status` equals "Pending"

### GWT-10: Non-member user attempts to invite — forbidden

GIVEN authenticated user "stranger@test.com" is not a member of team {teamId}
WHEN POST /api/teams/{teamId}/invitations with body `{ "InviteeEmail": "newuser@test.com" }`
THEN response status is 403

---

## GET /api/teams/{teamId}/invitations — List Team Invitations

### GWT-11: Team member lists invitations successfully

GIVEN authenticated user "member@test.com" is a member of team {teamId}
AND team {teamId} has 3 invitations (1 Pending, 1 Accepted, 1 Declined)
WHEN GET /api/teams/{teamId}/invitations
THEN response status is 200
AND response body is an array of 3 `InvitationDto` objects
AND each element contains fields: `Id`, `TeamId`, `InviterUserId`, `InviteeEmail`, `Status`, `CreatedAt`, `RespondedAt`

### GWT-12: Team owner lists invitations successfully

GIVEN authenticated user "owner@test.com" is the owner of team {teamId}
AND team {teamId} has 2 invitations
WHEN GET /api/teams/{teamId}/invitations
THEN response status is 200
AND response body is an array of 2 `InvitationDto` objects

### GWT-13: Non-member attempts to list invitations — forbidden

GIVEN authenticated user "stranger@test.com" is not a member of team {teamId}
WHEN GET /api/teams/{teamId}/invitations
THEN response status is 403

### GWT-14: Unauthenticated request — unauthorized

GIVEN no authentication token is provided
WHEN GET /api/teams/{teamId}/invitations
THEN response status is 401

### GWT-15: Team does not exist — not found

GIVEN authenticated user "owner@test.com"
AND no team exists with ID {teamId}
WHEN GET /api/teams/{teamId}/invitations
THEN response status is 404

### GWT-16: Team has no invitations — returns empty array

GIVEN authenticated user "member@test.com" is a member of team {teamId}
AND team {teamId} has 0 invitations
WHEN GET /api/teams/{teamId}/invitations
THEN response status is 200
AND response body is an empty array `[]`

---

## PUT /api/invitations/{id}/accept — Accept Invitation

### GWT-17: Invitee accepts a pending invitation successfully

GIVEN authenticated user "invitee@test.com"
AND invitation {id} exists with `Status` = "Pending" and `InviteeEmail` = "invitee@test.com"
WHEN PUT /api/invitations/{id}/accept
THEN response status is 200
AND response body is `InvitationDto` with:
  - `Id` equals {id}
  - `Status` equals "Accepted"
  - `RespondedAt` is within 5 seconds of now (UTC)

### GWT-18: Non-invitee attempts to accept — forbidden

GIVEN authenticated user "other@test.com"
AND invitation {id} exists with `Status` = "Pending" and `InviteeEmail` = "invitee@test.com"
WHEN PUT /api/invitations/{id}/accept
THEN response status is 403

### GWT-19: Unauthenticated request — unauthorized

GIVEN no authentication token is provided
AND invitation {id} exists with `Status` = "Pending"
WHEN PUT /api/invitations/{id}/accept
THEN response status is 401

### GWT-20: Invitation does not exist — not found

GIVEN authenticated user "invitee@test.com"
AND no invitation exists with ID {id}
WHEN PUT /api/invitations/{id}/accept
THEN response status is 404

### GWT-21: Accept an already-accepted invitation — invalid state transition

GIVEN authenticated user "invitee@test.com"
AND invitation {id} exists with `Status` = "Accepted" and `InviteeEmail` = "invitee@test.com"
WHEN PUT /api/invitations/{id}/accept
THEN response status is 409

### GWT-22: Accept a declined invitation — invalid state transition

GIVEN authenticated user "invitee@test.com"
AND invitation {id} exists with `Status` = "Declined" and `InviteeEmail` = "invitee@test.com"
WHEN PUT /api/invitations/{id}/accept
THEN response status is 409

### GWT-23: Accept a cancelled invitation — invalid state transition

GIVEN authenticated user "invitee@test.com"
AND invitation {id} exists with `Status` = "Cancelled" and `InviteeEmail` = "invitee@test.com"
WHEN PUT /api/invitations/{id}/accept
THEN response status is 409

---

## PUT /api/invitations/{id}/decline — Decline Invitation

### GWT-24: Invitee declines a pending invitation successfully

GIVEN authenticated user "invitee@test.com"
AND invitation {id} exists with `Status` = "Pending" and `InviteeEmail` = "invitee@test.com"
WHEN PUT /api/invitations/{id}/decline
THEN response status is 200
AND response body is `InvitationDto` with:
  - `Id` equals {id}
  - `Status` equals "Declined"
  - `RespondedAt` is within 5 seconds of now (UTC)

### GWT-25: Non-invitee attempts to decline — forbidden

GIVEN authenticated user "other@test.com"
AND invitation {id} exists with `Status` = "Pending" and `InviteeEmail` = "invitee@test.com"
WHEN PUT /api/invitations/{id}/decline
THEN response status is 403

### GWT-26: Unauthenticated request — unauthorized

GIVEN no authentication token is provided
AND invitation {id} exists with `Status` = "Pending"
WHEN PUT /api/invitations/{id}/decline
THEN response status is 401

### GWT-27: Invitation does not exist — not found

GIVEN authenticated user "invitee@test.com"
AND no invitation exists with ID {id}
WHEN PUT /api/invitations/{id}/decline
THEN response status is 404

### GWT-28: Decline an already-declined invitation — invalid state transition

GIVEN authenticated user "invitee@test.com"
AND invitation {id} exists with `Status` = "Declined" and `InviteeEmail` = "invitee@test.com"
WHEN PUT /api/invitations/{id}/decline
THEN response status is 409

### GWT-29: Decline an accepted invitation — invalid state transition

GIVEN authenticated user "invitee@test.com"
AND invitation {id} exists with `Status` = "Accepted" and `InviteeEmail` = "invitee@test.com"
WHEN PUT /api/invitations/{id}/decline
THEN response status is 409

### GWT-30: Decline a cancelled invitation — invalid state transition

GIVEN authenticated user "invitee@test.com"
AND invitation {id} exists with `Status` = "Cancelled" and `InviteeEmail` = "invitee@test.com"
WHEN PUT /api/invitations/{id}/decline
THEN response status is 409

---

## DELETE /api/invitations/{id} — Cancel Invitation

### GWT-31: Team owner cancels a pending invitation

GIVEN authenticated user "owner@test.com" is the owner of the team associated with invitation {id}
AND invitation {id} exists with `Status` = "Pending"
WHEN DELETE /api/invitations/{id}
THEN response status is 200
AND response body is `InvitationDto` with:
  - `Id` equals {id}
  - `Status` equals "Cancelled"
  - `RespondedAt` is within 5 seconds of now (UTC)

### GWT-32: Inviter (non-owner) cancels a pending invitation they sent

GIVEN authenticated user "inviter@test.com" is the `InviterUserId` of invitation {id} (but not the team owner)
AND invitation {id} exists with `Status` = "Pending"
WHEN DELETE /api/invitations/{id}
THEN response status is 200
AND response body is `InvitationDto` with `Status` equals "Cancelled"

### GWT-33: Unrelated team member attempts to cancel — forbidden

GIVEN authenticated user "bystander@test.com" is a team member but is neither the team owner nor the inviter of invitation {id}
AND invitation {id} exists with `Status` = "Pending"
WHEN DELETE /api/invitations/{id}
THEN response status is 403

### GWT-34: Invitee attempts to cancel — forbidden

GIVEN authenticated user "invitee@test.com" is the invitee (matched by email) but not the inviter or team owner
AND invitation {id} exists with `Status` = "Pending" and `InviteeEmail` = "invitee@test.com"
WHEN DELETE /api/invitations/{id}
THEN response status is 403

### GWT-35: Non-member attempts to cancel — forbidden

GIVEN authenticated user "stranger@test.com" is not a member of the team and is not the inviter
AND invitation {id} exists with `Status` = "Pending"
WHEN DELETE /api/invitations/{id}
THEN response status is 403

### GWT-36: Unauthenticated request — unauthorized

GIVEN no authentication token is provided
AND invitation {id} exists with `Status` = "Pending"
WHEN DELETE /api/invitations/{id}
THEN response status is 401

### GWT-37: Invitation does not exist — not found

GIVEN authenticated user "owner@test.com"
AND no invitation exists with ID {id}
WHEN DELETE /api/invitations/{id}
THEN response status is 404

### GWT-38: Cancel an already-accepted invitation — invalid state transition

GIVEN authenticated user "owner@test.com" is the owner of the team associated with invitation {id}
AND invitation {id} exists with `Status` = "Accepted"
WHEN DELETE /api/invitations/{id}
THEN response status is 409

### GWT-39: Cancel an already-declined invitation — invalid state transition

GIVEN authenticated user "owner@test.com" is the owner of the team associated with invitation {id}
AND invitation {id} exists with `Status` = "Declined"
WHEN DELETE /api/invitations/{id}
THEN response status is 409

### GWT-40: Cancel an already-cancelled invitation — invalid state transition

GIVEN authenticated user "owner@test.com" is the owner of the team associated with invitation {id}
AND invitation {id} exists with `Status` = "Cancelled"
WHEN DELETE /api/invitations/{id}
THEN response status is 409

---

## Quality Checklist

- [x] Every endpoint has a happy path GWT (GWT-1, GWT-11/12, GWT-17, GWT-24, GWT-31/32)
- [x] Every POST/PUT has at least one validation error GWT (GWT-5, GWT-6)
- [x] Every auth-protected endpoint has an unauthorized GWT (GWT-3, GWT-14, GWT-19, GWT-26, GWT-36)
- [x] Every path parameter has a not-found GWT (GWT-4, GWT-15, GWT-20, GWT-27, GWT-37)
- [x] Every state transition (valid AND invalid) has a GWT (GWT-17/21/22/23, GWT-24/28/29/30, GWT-31/38/39/40)
- [x] Every business rule has a GWT that tests the boundary (owner-only invite: GWT-2/10, invitee-only accept/decline: GWT-18/25, cancel auth: GWT-33/34/35, duplicate pending: GWT-7, non-member list: GWT-13)
- [x] All field names match the contract DTOs exactly (`Id`, `TeamId`, `InviterUserId`, `InviteeEmail`, `Status`, `CreatedAt`, `RespondedAt`)
- [x] All status codes are explicit numbers (200, 201, 400, 401, 403, 404, 409)
- [x] No vague language anywhere
