# Feature 3: Invitation System — EARS Requirements + Property Descriptions

## Contract Reference

```csharp
public record InvitationDto(Guid Id, Guid TeamId, Guid InviterUserId, string InviteeEmail, string Status, DateTime CreatedAt, DateTime? RespondedAt);
public record CreateInvitationRequest(string InviteeEmail);
```

---

## EARS Requirements

### POST /api/teams/{teamId}/invitations — Create Invitation

REQ-1: When an authenticated team owner sends a POST to /api/teams/{teamId}/invitations with a valid CreateInvitationRequest, the system shall create a new invitation with status "Pending" and return 201 with an InvitationDto.

REQ-2: If the requesting user is not authenticated, then the system shall return 401.

REQ-3: If the requesting user is not the owner of the team identified by {teamId}, then the system shall return 403.

REQ-4: If no team exists with the given {teamId}, then the system shall return 404.

REQ-5: If the CreateInvitationRequest.InviteeEmail is not a valid email format, then the system shall return 400.

REQ-6: If a Pending invitation already exists for the same InviteeEmail on the same team identified by {teamId}, then the system shall return 409.

### GET /api/teams/{teamId}/invitations — List Team Invitations

REQ-7: When an authenticated team member sends a GET to /api/teams/{teamId}/invitations, the system shall return 200 with a list of InvitationDto for that team.

REQ-8: If the requesting user is not authenticated, then the system shall return 401.

REQ-9: If the requesting user is not a member of the team identified by {teamId}, then the system shall return 403.

REQ-10: If no team exists with the given {teamId}, then the system shall return 404.

### PUT /api/invitations/{id}/accept — Accept Invitation

REQ-11: When an authenticated user whose email matches the InviteeEmail of a Pending invitation sends a PUT to /api/invitations/{id}/accept, the system shall transition the invitation status from "Pending" to "Accepted", set RespondedAt to the current UTC timestamp, and return 200 with an InvitationDto.

REQ-12: If the requesting user is not authenticated, then the system shall return 401.

REQ-13: If the requesting user's email does not match the InviteeEmail of the invitation identified by {id}, then the system shall return 403.

REQ-14: If no invitation exists with the given {id}, then the system shall return 404.

REQ-15: If the invitation identified by {id} has a status other than "Pending", then the system shall return 409.

### PUT /api/invitations/{id}/decline — Decline Invitation

REQ-16: When an authenticated user whose email matches the InviteeEmail of a Pending invitation sends a PUT to /api/invitations/{id}/decline, the system shall transition the invitation status from "Pending" to "Declined", set RespondedAt to the current UTC timestamp, and return 200 with an InvitationDto.

REQ-17: If the requesting user is not authenticated, then the system shall return 401.

REQ-18: If the requesting user's email does not match the InviteeEmail of the invitation identified by {id}, then the system shall return 403.

REQ-19: If no invitation exists with the given {id}, then the system shall return 404.

REQ-20: If the invitation identified by {id} has a status other than "Pending", then the system shall return 409.

### DELETE /api/invitations/{id} — Cancel Invitation

REQ-21: When an authenticated user who is either the inviter (InviterUserId) or the team owner of the invitation's team sends a DELETE to /api/invitations/{id} for a Pending invitation, the system shall transition the invitation status from "Pending" to "Cancelled", set RespondedAt to the current UTC timestamp, and return 200 with an InvitationDto.

REQ-22: If the requesting user is not authenticated, then the system shall return 401.

REQ-23: If the requesting user is neither the InviterUserId nor the owner of the invitation's team, then the system shall return 403.

REQ-24: If no invitation exists with the given {id}, then the system shall return 404.

REQ-25: If the invitation identified by {id} has a status other than "Pending", then the system shall return 409.

### Cross-Cutting — Duplicate Prevention

REQ-26: While a Pending invitation exists for a given InviteeEmail and TeamId, when a POST to /api/teams/{teamId}/invitations is received with the same InviteeEmail, the system shall return 409.

---

## Property Descriptions

### Identity & Referential Integrity

PROP-1: For all InvitationDto responses, Id must be a non-empty GUID that uniquely identifies the invitation.

PROP-2: For all InvitationDto responses, TeamId must match the GUID of an existing team.

PROP-3: For all InvitationDto responses, InviterUserId must match the GUID of the authenticated user who created the invitation.

PROP-4: For all InvitationDto responses, InviteeEmail must be a non-empty string in valid email format.

### Status Invariants

PROP-5: For all InvitationDto responses, Status must be one of: "Pending", "Accepted", "Declined", "Cancelled".

PROP-6: For all Invitation entities in state "Pending", the only valid transitions are "Accepted", "Declined", "Cancelled".

PROP-7: For all Invitation entities in state "Accepted", no status transitions are valid.

PROP-8: For all Invitation entities in state "Declined", no status transitions are valid.

PROP-9: For all Invitation entities in state "Cancelled", no status transitions are valid.

### Timestamp Invariants

PROP-10: For all InvitationDto responses, CreatedAt must be a UTC timestamp equal to the time the invitation was created and must never change after creation.

PROP-11: For all InvitationDto responses where Status is "Pending", RespondedAt must be null.

PROP-12: For all InvitationDto responses where Status is "Accepted", "Declined", or "Cancelled", RespondedAt must be a non-null UTC timestamp greater than or equal to CreatedAt.

PROP-13: For all InvitationDto responses, RespondedAt must never change once set to a non-null value.

### Uniqueness Constraints

PROP-14: For all Invitation entities within a single team, there must be at most one invitation with status "Pending" for any given InviteeEmail.

### Idempotency

PROP-15: For all DELETE /api/invitations/{id} requests where the invitation has already been cancelled, repeated calls must return 409.

### Authorization Boundaries

PROP-16: For all POST /api/teams/{teamId}/invitations requests, only the user whose Id matches the team's OwnerId is authorized (all others receive 403).

PROP-17: For all GET /api/teams/{teamId}/invitations requests, only users who are members of the team identified by {teamId} are authorized (all others receive 403).

PROP-18: For all PUT /api/invitations/{id}/accept requests, only the user whose authenticated email matches InviteeEmail is authorized (all others receive 403).

PROP-19: For all PUT /api/invitations/{id}/decline requests, only the user whose authenticated email matches InviteeEmail is authorized (all others receive 403).

PROP-20: For all DELETE /api/invitations/{id} requests, only users whose Id matches InviterUserId or the team's OwnerId are authorized (all others receive 403).

### Output Completeness

PROP-21: For all valid CreateInvitationRequest inputs, the returned InvitationDto.Status must equal "Pending".

PROP-22: For all valid CreateInvitationRequest inputs, the returned InvitationDto.RespondedAt must be null.

PROP-23: For all valid PUT /api/invitations/{id}/accept requests, the returned InvitationDto.Status must equal "Accepted".

PROP-24: For all valid PUT /api/invitations/{id}/decline requests, the returned InvitationDto.Status must equal "Declined".

PROP-25: For all valid DELETE /api/invitations/{id} requests on a Pending invitation, the returned InvitationDto.Status must equal "Cancelled".
