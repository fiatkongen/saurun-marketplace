# Transcript: Subscription Management Domain (Without Skill)

## Task

Implement a .NET 9 subscription management domain with:
- Subscription entity with states: Trial, Active, PastDue, Cancelled
- Plans: Basic ($9/mo), Pro ($29/mo), Enterprise ($99/mo)
- Business rules for upgrades, downgrades, cancellation, trial, and past-due handling
- SubscriptionService domain service for orchestration
- Result<T> for fallible operations
- xUnit tests with NSubstitute

## Approach

I analyzed the requirements and identified the following domain concepts:

1. **Value Objects**: `Plan` (immutable record with static instances), `SubscriptionState` (enum)
2. **Entity**: `Subscription` (aggregate root with state machine behavior)
3. **Domain Service**: `SubscriptionService` (orchestrates plan changes and time-based transitions)
4. **Infrastructure Abstraction**: `IDateTimeProvider` (for testable time)
5. **Result Type**: `Result<T>` (success/failure monad)

## Files Created

### Domain Layer (`Domain/`)

| File | Purpose |
|------|---------|
| `SubscriptionState.cs` | Enum: Trial, Active, PastDue, Cancelled |
| `Plan.cs` | Value object with static instances (Basic, Pro, Enterprise), price comparison methods |
| `Result.cs` | Generic result type with Success/Failure factory methods and implicit conversion |
| `IDateTimeProvider.cs` | Abstraction for current time (enables test control) |
| `Subscription.cs` | Aggregate root with state machine: factory methods, transitions, query methods |

### Services Layer (`Services/`)

| File | Purpose |
|------|---------|
| `SubscriptionService.cs` | Orchestrates plan changes (auto-detects upgrade vs downgrade), cancellation/reactivation, and time-based transitions (trial expiry, past-due auto-cancel, pending downgrade application) |

### Tests (`Tests/`)

| File | Tests |
|------|-------|
| `PlanTests.cs` | 10 tests: pricing, ordering, FromName lookup, upgrade/downgrade comparison |
| `SubscriptionTests.cs` | 27 tests: all entity state transitions, boundary conditions, query methods |
| `SubscriptionServiceTests.cs` | 17 tests: service orchestration, time-based transitions, full lifecycle scenarios |

## Design Decisions

1. **Plan as sealed record with static instances** — Acts as a smart enum. No database of plans needed. `IsUpgradeFrom`/`IsDowngradeFrom` based on price comparison.

2. **Subscription owns its state transitions** — Each transition method (Upgrade, Downgrade, Cancel, etc.) validates preconditions and returns `Result<Subscription>`. The entity enforces its own invariants.

3. **SubscriptionService adds orchestration** — `ChangePlan` auto-detects upgrade vs downgrade. `ProcessTimeBasedTransitions` handles trial expiry, past-due auto-cancel, and pending downgrade application in priority order.

4. **Pending downgrade pattern** — Downgrades don't change the plan immediately. A `PendingDowngradePlan` property is set and applied when the billing period ends (via `ProcessTimeBasedTransitions` or `ApplyPendingDowngrade`).

5. **Cooling-off period** — Cancellation sets `CancelledAtUtc`. Reactivation checks if within 14 days. Boundary condition: exactly 14 days is still valid.

6. **IDateTimeProvider** — Injected into SubscriptionService, substituted with NSubstitute in tests. Entity methods take `DateTime utcNow` as parameter directly (no DI needed at entity level).

## Business Rules Mapping

| Rule | Implementation |
|------|---------------|
| Cannot downgrade during trial | `Downgrade()` checks `State == Trial` → failure |
| Cancellation has 14-day cooling-off | `Cancel()` stores `CancelledAtUtc`, `Reactivate()` checks `utcNow - CancelledAtUtc <= 14 days` |
| Upgrades take effect immediately | `Upgrade()` changes `Plan` and resets billing period |
| Downgrades at end of billing period | `Downgrade()` sets `PendingDowngradePlan`, `ApplyPendingDowngrade()` applies at period end |
| PastDue auto-cancel after 30 days | `ShouldAutoCancelPastDue()` checks `utcNow - PastDueSinceUtc >= 30 days`, processed in `ProcessTimeBasedTransitions` |
| Trial lasts 14 days → Active | `IsTrialExpired()` checks `utcNow >= TrialEndsAtUtc`, `Activate()` transitions to Active |

## Test Coverage Summary

- **54 total tests** across 3 test files
- All state transitions tested with both success and failure cases
- Boundary conditions tested (exact 14-day cooling-off, exact 30-day past-due)
- Full lifecycle integration tests (trial → active → upgrade → cancel → reactivate, and past-due → auto-cancel)
- NSubstitute used for IDateTimeProvider in SubscriptionServiceTests
