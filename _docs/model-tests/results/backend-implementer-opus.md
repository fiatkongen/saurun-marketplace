TDD workflow demonstrated: tests written BEFORE implementation.

Tests: 13 tests across 3 classes (EmailAddressTests: 4, TeamInvitationTests: 7, CreateInvitationCommandHandlerTests: 4).

Domain Model:
- EmailAddress value object: private ctor, static Create() returning Result<T>, case-insensitive normalization, IEquatable, regex validation
- TeamInvitationId: readonly record struct with New() factory
- TeamInvitation entity: private ctor, private setters, static Create() factory returning Result<TeamInvitation>, Accept() behavior method with state machine (Pending→Accepted, auto-expire), IsExpired() method, EF Core parameterless ctor
- InvitationRole + InvitationStatus enums

Command Handler:
- CreateInvitationCommandHandler with injected ITeamInvitationRepository, ITeamMemberRepository, ITimeProvider
- Validates email via EmailAddress.Create()
- Checks existing membership via repository
- Creates invitation with 7-day expiration (via entity factory + ITimeProvider)
- Returns Result<TeamInvitationId>

Testing Patterns:
- NSubstitute for mocking (ITeamInvitationRepository, ITeamMemberRepository, ITimeProvider)
- Arg.Do<T> for capturing persisted entities
- Theory/InlineData for parameterized email tests
- Time abstraction via ITimeProvider (injectable, testable)

EF Core: OwnsOne() for EmailAddress, string conversions for enums, composite index (TeamId + Status).

Key: Time abstraction (ITimeProvider) injected into handler and passed to entity Create(). Entity never calls DateTimeOffset.UtcNow directly.
