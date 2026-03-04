# Inventory Reservation System - Implementation Transcript

## Skills Loaded

- `dotnet-tactical-ddd`: Rich domain models, value objects, aggregates, Result<T>, domain events, private constructors + static factories
- `dotnet-tdd`: Red-Green-Refactor workflow, test-first discipline, NSubstitute for mocking, behavioral test naming

## Approach

Built a .NET 9 inventory reservation domain model following strict TDD with tactical DDD patterns. Every production type was written only after a failing test demanded it.

## TDD Cycles

### Cycle 1: Quantity Value Object

**RED:** Wrote `QuantityTests.cs` with 9 tests covering: Create with positive value, Create with zero/negative (Theory), Subtract sufficient/insufficient, Add, equality, comparison operators. Compilation failed -- `Quantity` class did not exist.

**GREEN:** Wrote `Quantity.cs` as a `ValueObject` subclass with:
- Private constructor, `Create()` factory returning `Result<Quantity>`
- `Add()`, `Subtract()` (returns Result), `IComparable<Quantity>`, operator overloads `>=`, `<=`, `>`, `<`
- Internal `Zero()` and `FromInt()` helpers for known-safe values within the aggregate

**Result:** 9/9 passed.

### Cycle 2: ProductId Value Object

**RED:** Wrote `ProductIdTests.cs` with 3 tests: valid Guid, empty Guid failure, equality. Compilation failed.

**GREEN:** Wrote `ProductId.cs` as a `ValueObject` with private ctor, `Create()` factory, Guid validation.

**Result:** 12/12 passed.

### Cycle 3: Reservation Entity

**RED:** Wrote `ReservationTests.cs` with 10 tests covering:
- Create returns Pending status with correct ExpiresAt (15 min from now)
- Confirm when Pending succeeds, when already Confirmed fails, when expired fails
- Cancel when Pending succeeds, when Confirmed fails
- Expire when overdue succeeds, when not yet expired fails
- IsExpired predicate for both cases

Compilation failed -- `Reservation` and `ReservationStatus` did not exist.

**GREEN:** Wrote `ReservationStatus.cs` enum (Pending, Confirmed, Expired, Cancelled) and `Reservation.cs` entity with:
- Private constructor, `Create()` factory
- `Confirm(utcNow)`, `Cancel()`, `Expire(utcNow)` behavior methods all returning `Result`
- `IsExpired(utcNow)` predicate
- Status transition guards preventing invalid transitions

**Result:** 22/22 passed.

### Cycle 4: ReservationOutcome (Partial Reservation Info)

**RED:** Wrote `ReservationResultTests.cs` with 3 tests: FullyReserved has zero shortfall, PartiallyReserved reports correct shortfall, PartiallyReserved reservation has correct quantity. Compilation failed.

**GREEN:** Wrote `ReservationOutcome.cs` with `FullyReserved()` and `PartiallyReserved()` factory methods, `Shortfall` property, `IsFullyReserved` computed property.

**Result:** 25/25 passed.

### Cycle 5: Domain Events

Wrote `InventoryEvents.cs` with immutable record events (past tense, per skill rules):
- `InventoryItemCreatedEvent`
- `StockReservedEvent` (includes Shortfall for partial reservations)
- `ReservationConfirmedEvent`
- `ReservationCancelledEvent`
- `ReservationExpiredEvent`

No test cycle needed -- events are records used by tests in the next cycle.

### Cycle 6: InventoryItem Aggregate (Core)

**RED:** Wrote `InventoryItemTests.cs` with 26 tests organized in 5 test classes:
- **InventoryItemCreateTests** (4 tests): valid creation, reserved starts at zero, raises InventoryItemCreatedEvent, version starts at 0
- **InventoryItemReserveTests** (10 tests): full reservation, decreases available/increases reserved, partial reservation with shortfall, nothing available returns failure, raises StockReservedEvent, partial event has correct shortfall, increments version, proactively expires overdue before checking, adds to collection, multiple reservations tracked
- **InventoryItemConfirmTests** (4 tests): decreases reserved quantity, raises event, unknown ID fails, increments version
- **InventoryItemCancelTests** (4 tests): returns stock to available, raises event, unknown ID fails, increments version
- **InventoryItemExpireTests** (3 tests): returns stock, does not expire active, raises event per expired
- **InventoryItemConcurrencyTests** (1 test): version increases on each operation

Compilation failed.

**GREEN:** Wrote `InventoryItem.cs` aggregate with:
- Private constructor, `Create()` factory
- `Reserve(quantity, utcNow)` -- proactively expires overdue reservations, supports partial reservations, returns `Result<ReservationOutcome>`
- `ConfirmReservation(reservationId, utcNow)` -- permanently removes from reserved
- `CancelReservation(reservationId)` -- returns stock to available
- `ExpireOverdueReservations(utcNow)` -- batch expiration
- `Version` field incremented on every mutation (optimistic concurrency token)
- Private backing collection `_reservations` with `IReadOnlyList` projection
- Domain events raised on every state change

**Result:** 51/51 passed.

**REFACTOR:** Fixed nullable warnings on EF Core parameterless constructors by assigning `null!` to non-nullable properties. All 51 tests still passed.

### Cycle 7: Edge Cases, Version, and Concurrency Tests

**GREEN (all tests passed immediately against existing code):**

Wrote `InventoryItemEdgeCaseTests.cs` with 20 additional tests:
- **InventoryItemEdgeCaseTests** (7 tests): reserve-cancel-reserve cycle, reserve-confirm stock accounting, confirm after expiry fails, cancel after confirm fails, multiple partial drain then expire refill, exact available is fully reserved, one-more-than-available is partial, mixed status only expires pending
- **InventoryItemVersionTests** (5 tests): starts at zero, monotonically increases, N reserves = version N (Theory)
- **InventoryItemDomainEventTests** (3 tests): full reservation event has zero shortfall, partial event reports shortfall, cancel event contains quantity
- **ConcurrencyTests** (4 tests): simulated concurrent version conflict detection, concurrent reserve+confirm, race condition on low stock, sequential operations version predictability

**Result:** 71/71 passed.

### Cycle 8: Repository Interface

Wrote `IInventoryItemRepository.cs` with methods: `GetByIdAsync`, `GetByProductIdAsync`, `AddAsync`, `SaveChangesAsync`. No test needed -- interface only (no implementation behavior).

## Final Results

```
Build succeeded. 0 Warning(s), 0 Error(s)
Total tests: 71, Passed: 71, Failed: 0
Duration: ~0.24 seconds
```

## DDD Patterns Applied

| Pattern | Implementation |
|---------|---------------|
| Value Objects | `Quantity`, `ProductId` -- immutable, value equality, factory with `Result<T>` |
| Entity | `Reservation` -- identity-based, private setters, behavior methods |
| Aggregate Root | `InventoryItem` -- single entry point, private collection, enforces invariants |
| Domain Events | 5 immutable records (past tense), raised inside aggregate |
| Result<T> | All operations that can fail return `Result` or `Result<T>` |
| Private Constructors | All domain types have private ctors + static `Create()` factories |
| Optimistic Concurrency | `Version` field incremented on every mutation |
| Proactive Expiration | `Reserve()` expires overdue reservations before checking availability |
| Partial Reservations | `ReservationOutcome` reports shortfall when partially fulfilled |
| Repository Interface | `IInventoryItemRepository` per aggregate root |

## Files Written

### Domain/
- `Domain.csproj`
- `Common/Result.cs` -- Railway-oriented error handling
- `Common/ValueObject.cs` -- Value equality base class
- `Common/Entity.cs` -- Identity-based equality base class
- `Common/AggregateRoot.cs` -- Domain event collection base class
- `Common/IDomainEvent.cs` -- Domain event marker interface
- `ValueObjects/Quantity.cs` -- Positive integer value object with arithmetic
- `ValueObjects/ProductId.cs` -- Non-empty GUID wrapper
- `ValueObjects/ReservationOutcome.cs` -- Partial reservation result
- `Entities/ReservationStatus.cs` -- Enum: Pending, Confirmed, Expired, Cancelled
- `Entities/Reservation.cs` -- Entity with status transitions
- `Entities/InventoryItem.cs` -- Aggregate root with all business rules
- `Events/InventoryEvents.cs` -- 5 domain event records
- `Interfaces/IInventoryItemRepository.cs` -- Repository contract

### Tests/
- `Tests.csproj`
- `Unit/QuantityTests.cs` -- 9 tests
- `Unit/ProductIdTests.cs` -- 3 tests
- `Unit/ReservationTests.cs` -- 10 tests
- `Unit/ReservationResultTests.cs` -- 3 tests
- `Unit/InventoryItemTests.cs` -- 26 tests
- `Unit/InventoryItemEdgeCaseTests.cs` -- 15 tests
- `Unit/ConcurrencyTests.cs` -- 4 tests + 1 from InventoryItemTests
