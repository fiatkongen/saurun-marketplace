TDD workflow demonstrated: tests written BEFORE implementation.

Tests: 8 tests across 2 classes (TeamInvitationTests: 5, CreateInvitationCommandHandlerTests: 4, including TeamNotFound).

Domain Model:
- EmailAddress value object: private ctor, static Create() returning Result<T>, case-insensitive normalization, ValueObject base class, System.Net.Mail validation
- TeamInvitationId: ValueObject with Create()/From() factories
- TeamInvitation entity: private ctor, private setters, static Create() factory returning Result<TeamInvitation>, Accept() behavior method with state checks (expired, already accepted), IsExpired() method, EF Core parameterless ctor
- TeamRole + InvitationStatus enums

Command Handler:
- CreateInvitationCommandHandler implementing IRequestHandler<> (MediatR)
- Validates email via EmailAddress.Create()
- Gets team from repository (null check)
- Checks membership via team.IsMember()
- Creates invitation and persists
- Returns Result<TeamInvitationId>

Testing Patterns:
- NSubstitute for mocking (ITeamRepository, ITeamInvitationRepository)
- Correct naming convention (Handle_ValidRequest_CreatesInvitation)

EF Core: OwnsOne() for EmailAddress, string conversions for enums, index on Email and Status.
Repository implementation included (AddAsync calls SaveChangesAsync).

Issues:
- Uses DateTimeOffset.UtcNow directly in TeamInvitation.Create() and Accept() instead of injecting time abstraction (less testable — test for 7-day expiry uses time window comparison rather than exact match)
- MediatR IRequestHandler dependency not in requirements (minor — valid pattern choice)
- Uses TestHelpers.CreateExpiredInvitation() in test without showing implementation (test completeness gap)
