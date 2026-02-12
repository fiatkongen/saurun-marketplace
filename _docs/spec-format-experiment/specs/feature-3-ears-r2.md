# Feature 3: Invitation System — EARS Requirements + Property Descriptions (R2)

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

---

## EARS Requirements

### POST /api/teams/{teamId}/invitations — Create Invitation

REQ-1: When an authenticated team owner sends a POST to /api/teams/{teamId}/invitations with a valid CreateInvitationRequest, the system shall create a new invitation with status "Pending", set CreatedAt to the current UTC timestamp, set RespondedAt to null, set InviterUserId to the authenticated user's Id, and return 201 with an InvitationDto.

REQ-2: If the requesting user is not authenticated, then the system shall return 401.

REQ-3: If the requesting user is not the owner of the team identified by {teamId}, then the system shall return 403.

REQ-4: If no team exists with the given {teamId}, then the system shall return 404.

REQ-5: If the CreateInvitationRequest.InviteeEmail is null or empty, then the system shall return 400.

REQ-6: If the CreateInvitationRequest.InviteeEmail is not a valid email format, then the system shall return 400.

REQ-7: If a Pending invitation already exists for the same InviteeEmail on the team identified by {teamId}, then the system shall return 409.

### GET /api/teams/{teamId}/invitations — List Team Invitations

REQ-8: When an authenticated team member sends a GET to /api/teams/{teamId}/invitations, the system shall return 200 with a list of InvitationDto for that team.

REQ-9: If the requesting user is not authenticated, then the system shall return 401.

REQ-10: If the requesting user is not a member of the team identified by {teamId}, then the system shall return 403.

REQ-11: If no team exists with the given {teamId}, then the system shall return 404.

### PUT /api/invitations/{id}/accept — Accept Invitation

REQ-12: When an authenticated user whose email matches the InviteeEmail of a Pending invitation sends a PUT to /api/invitations/{id}/accept, the system shall transition the invitation status from "Pending" to "Accepted", set RespondedAt to the current UTC timestamp, and return 200 with an InvitationDto.

REQ-13: If the requesting user is not authenticated, then the system shall return 401.

REQ-14: If the requesting user's email does not match the InviteeEmail of the invitation identified by {id}, then the system shall return 403.

REQ-15: If no invitation exists with the given {id}, then the system shall return 404.

REQ-16: If the invitation identified by {id} has status "Accepted", then the system shall return 409.

REQ-17: If the invitation identified by {id} has status "Declined", then the system shall return 409.

REQ-18: If the invitation identified by {id} has status "Cancelled", then the system shall return 409.

### PUT /api/invitations/{id}/decline — Decline Invitation

REQ-19: When an authenticated user whose email matches the InviteeEmail of a Pending invitation sends a PUT to /api/invitations/{id}/decline, the system shall transition the invitation status from "Pending" to "Declined", set RespondedAt to the current UTC timestamp, and return 200 with an InvitationDto.

REQ-20: If the requesting user is not authenticated, then the system shall return 401.

REQ-21: If the requesting user's email does not match the InviteeEmail of the invitation identified by {id}, then the system shall return 403.

REQ-22: If no invitation exists with the given {id}, then the system shall return 404.

REQ-23: If the invitation identified by {id} has status "Accepted", then the system shall return 409.

REQ-24: If the invitation identified by {id} has status "Declined", then the system shall return 409.

REQ-25: If the invitation identified by {id} has status "Cancelled", then the system shall return 409.

### DELETE /api/invitations/{id} — Cancel Invitation

REQ-26: When an authenticated user who is the inviter (InviterUserId) of a Pending invitation sends a DELETE to /api/invitations/{id}, the system shall transition the invitation status from "Pending" to "Cancelled", set RespondedAt to the current UTC timestamp, and return 200 with an InvitationDto.

REQ-27: When an authenticated user who is the owner of the invitation's team sends a DELETE to /api/invitations/{id} for a Pending invitation, the system shall transition the invitation status from "Pending" to "Cancelled", set RespondedAt to the current UTC timestamp, and return 200 with an InvitationDto.

REQ-28: If the requesting user is not authenticated, then the system shall return 401.

REQ-29: If the requesting user is neither the InviterUserId of the invitation nor the owner of the invitation's team, then the system shall return 403.

REQ-30: If no invitation exists with the given {id}, then the system shall return 404.

REQ-31: If the invitation identified by {id} has status "Accepted", then the system shall return 409.

REQ-32: If the invitation identified by {id} has status "Declined", then the system shall return 409.

REQ-33: If the invitation identified by {id} has status "Cancelled", then the system shall return 409.

### Cross-Cutting — Duplicate Prevention

REQ-34: While a Pending invitation exists for a given InviteeEmail and TeamId, when a POST to /api/teams/{teamId}/invitations is received with the same InviteeEmail, the system shall return 409.

REQ-35: When a POST to /api/teams/{teamId}/invitations is received with an InviteeEmail that has a previous non-Pending invitation (status "Accepted", "Declined", or "Cancelled") on the same team, the system shall create a new Pending invitation and return 201 with an InvitationDto.

---

## Property Descriptions

### Identity & Referential Integrity

PROP-1: For all InvitationDto responses, Id must be a non-empty GUID that uniquely identifies the invitation.

PROP-2: For all InvitationDto responses, TeamId must match the GUID of an existing team.

PROP-3: For all InvitationDto responses, InviterUserId must match the GUID of the authenticated user who created the invitation.

PROP-4: For all InvitationDto responses, InviteeEmail must be a non-empty string in valid email format.

PROP-5: For all InvitationDto responses, Id must be immutable — the same invitation always returns the same Id.

### Status Invariants

PROP-6: For all InvitationDto responses, Status must be one of: "Pending", "Accepted", "Declined", "Cancelled".

PROP-7: For all Invitation entities in state "Pending", the only valid transitions are "Accepted", "Declined", "Cancelled".

PROP-8: For all Invitation entities in state "Accepted", no status transitions are valid.

PROP-9: For all Invitation entities in state "Declined", no status transitions are valid.

PROP-10: For all Invitation entities in state "Cancelled", no status transitions are valid.

### Timestamp Invariants

PROP-11: For all InvitationDto responses, CreatedAt must be a UTC timestamp set at invitation creation time and must never change after creation.

PROP-12: For all InvitationDto responses where Status is "Pending", RespondedAt must be null.

PROP-13: For all InvitationDto responses where Status is "Accepted", RespondedAt must be a non-null UTC timestamp greater than or equal to CreatedAt.

PROP-14: For all InvitationDto responses where Status is "Declined", RespondedAt must be a non-null UTC timestamp greater than or equal to CreatedAt.

PROP-15: For all InvitationDto responses where Status is "Cancelled", RespondedAt must be a non-null UTC timestamp greater than or equal to CreatedAt.

PROP-16: For all InvitationDto responses, once RespondedAt is set to a non-null value, it must never change.

### Uniqueness Constraints

PROP-17: For all Invitation entities within a single team (same TeamId), there must be at most one invitation with status "Pending" for any given InviteeEmail.

### Authorization Boundaries

PROP-18: For all POST /api/teams/{teamId}/invitations requests, only the user whose Id matches the team's OwnerId is authorized (all others receive 403).

PROP-19: For all GET /api/teams/{teamId}/invitations requests, only users who are members of the team identified by {teamId} are authorized (all others receive 403).

PROP-20: For all PUT /api/invitations/{id}/accept requests, only the user whose authenticated email matches InvitationDto.InviteeEmail is authorized (all others receive 403).

PROP-21: For all PUT /api/invitations/{id}/decline requests, only the user whose authenticated email matches InvitationDto.InviteeEmail is authorized (all others receive 403).

PROP-22: For all DELETE /api/invitations/{id} requests, only users whose Id matches InvitationDto.InviterUserId or the team's OwnerId are authorized (all others receive 403).

### Output Completeness — Create

PROP-23: For all valid CreateInvitationRequest inputs, the returned InvitationDto.Status must equal "Pending".

PROP-24: For all valid CreateInvitationRequest inputs, the returned InvitationDto.RespondedAt must be null.

PROP-25: For all valid CreateInvitationRequest inputs, the returned InvitationDto.InviteeEmail must equal the submitted CreateInvitationRequest.InviteeEmail.

PROP-26: For all valid CreateInvitationRequest inputs, the returned InvitationDto.TeamId must equal the {teamId} path parameter.

PROP-27: For all valid CreateInvitationRequest inputs, the returned InvitationDto.InviterUserId must equal the authenticated user's Id.

### Output Completeness — Accept/Decline/Cancel

PROP-28: For all valid PUT /api/invitations/{id}/accept requests, the returned InvitationDto.Status must equal "Accepted".

PROP-29: For all valid PUT /api/invitations/{id}/decline requests, the returned InvitationDto.Status must equal "Declined".

PROP-30: For all valid DELETE /api/invitations/{id} requests on a Pending invitation, the returned InvitationDto.Status must equal "Cancelled".

PROP-31: For all valid PUT /api/invitations/{id}/accept requests, the returned InvitationDto.RespondedAt must be a non-null UTC timestamp.

PROP-32: For all valid PUT /api/invitations/{id}/decline requests, the returned InvitationDto.RespondedAt must be a non-null UTC timestamp.

PROP-33: For all valid DELETE /api/invitations/{id} requests on a Pending invitation, the returned InvitationDto.RespondedAt must be a non-null UTC timestamp.

### Field Immutability

PROP-34: For all InvitationDto responses, TeamId must never change after invitation creation.

PROP-35: For all InvitationDto responses, InviterUserId must never change after invitation creation.

PROP-36: For all InvitationDto responses, InviteeEmail must never change after invitation creation.

### Idempotency

PROP-37: For all DELETE /api/invitations/{id} requests where the invitation has status "Cancelled", repeated calls must return 409.

PROP-38: For all PUT /api/invitations/{id}/accept requests where the invitation has status "Accepted", repeated calls must return 409.

PROP-39: For all PUT /api/invitations/{id}/decline requests where the invitation has status "Declined", repeated calls must return 409.

### List Output

PROP-40: For all GET /api/teams/{teamId}/invitations responses, every InvitationDto in the list must have TeamId equal to the {teamId} path parameter.

PROP-41: For all GET /api/teams/{teamId}/invitations responses, the list must include invitations of all statuses ("Pending", "Accepted", "Declined", "Cancelled") for that team.

---

## Quality Checklist

- [x] Every endpoint has at least one Event-driven EARS requirement (REQ-1, REQ-8, REQ-12, REQ-19, REQ-26/27)
- [x] Every POST/PUT has at least one Unwanted EARS requirement (REQ-5/6/7, REQ-16/17/18, REQ-23/24/25)
- [x] Every auth-protected endpoint has an Unwanted requirement (REQ-2, REQ-9, REQ-13, REQ-20, REQ-28)
- [x] Every path parameter has an Unwanted requirement for not found (REQ-4, REQ-11, REQ-15, REQ-22, REQ-30)
- [x] Every state transition is covered — valid transitions as Event-driven (REQ-1, REQ-12, REQ-19, REQ-26/27), invalid as Unwanted (REQ-16/17/18, REQ-23/24/25, REQ-31/32/33)
- [x] Every business rule has a Property or Unwanted requirement (owner-only invite REQ-3/PROP-18, invitee-only accept/decline REQ-14/REQ-21/PROP-20/21, inviter-or-owner cancel REQ-29/PROP-22, member-only list REQ-10/PROP-19, duplicate prevention REQ-7/REQ-34/PROP-17, email validation REQ-5/6)
- [x] All field names match InvitationDto, CreateInvitationRequest exactly
- [x] All status codes are explicit numbers (201, 200, 400, 401, 403, 404, 409)
- [x] Properties cover cross-cutting invariants (timestamps PROP-11/12/13/14/15/16, IDs PROP-1/2/3/5, ownership PROP-18/19/20/21/22, immutability PROP-34/35/36)
- [x] No vague language anywhere
