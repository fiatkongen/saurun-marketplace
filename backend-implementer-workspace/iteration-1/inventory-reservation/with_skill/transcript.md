# Inventory Reservation System - Implementation Transcript

## Skills Used
- `dotnet-tactical-ddd` - Rich domain models, Value Objects, AggregateRoot, Result<T>, private setters, factory methods
- `dotnet-tdd` - Red-Green-Refactor cycle, test-first, real domain objects in tests

## TDD Workflow

### Phase 1: Project Setup
- Created `Domain.csproj` (.NET 9, no dependencies) and `Tests.csproj` (xUnit 2.9.3)
- Copied base classes from `dotnet-tactical-ddd/base-classes.cs` into `Domain/Common/`:
  - `Result<T>` (railway-oriented error handling)
  - `ValueObject` (immutable, value equality)
  - `Entity<TId>` (identity-based equality)
  - `AggregateRoot<TId>` (domain events collection)
  - `IDomainEvent` interface

### Phase 2: RED - Write All Tests First
Wrote 57 tests across 6 test files before any production domain code existed:

1. **QuantityTests** (8 tests) - Value object creation, add/subtract, equality, validation
2. **ProductIdTests** (3 tests) - Value object creation, empty GUID rejection, equality
3. **ReservationTests** (10 tests) - Entity creation, state transitions (Confirm/Cancel/Expire), status guards, expiry check
4. **InventoryItemTests** (22 tests) - Aggregate creation, reserve (full/partial/zero), confirm/cancel/expire, domain events, version tracking, multi-reservation flows, overdue expiration
5. **ReservationResultTests** (4 tests) - IsFull/IsEmpty/IsPartial classification
6. **ConcurrencyTests** (8 tests) - Version increments on each mutation, zero-stock no-op

Build failed with 132 compile errors (all types missing) -- confirmed RED.

### Phase 3: GREEN - Implement Minimal Code

#### Value Objects
- `Quantity` - Wraps int, prevents negatives, Add/Subtract with Result<T>, Zero singleton
- `ProductId` - Wraps Guid, rejects Guid.Empty

#### Entities
- `ReservationStatus` - Enum: Pending, Confirmed, Expired, Cancelled
- `Reservation` - Entity with state machine (Pending -> Confirmed|Cancelled|Expired), factory with validation, IsExpired(asOf) method
- `ReservationResult` - Holds Reserved/Shortfall quantities + optional Reservation, with IsFull/IsEmpty/IsPartial helpers

#### Aggregate Root
- `InventoryItem` - AggregateRoot tracking ProductId, AvailableQuantity, ReservedQuantity, Version, and a private collection of Reservations. Key behaviors:
  - `Reserve()` - Partial reservation support (reserves what's available, returns shortfall info), raises StockReservedEvent
  - `ConfirmReservation()` - Permanently removes from reserved, raises ReservationConfirmedEvent
  - `CancelReservation()` - Returns stock to available, raises ReservationCancelledEvent
  - `ExpireReservation()` - Returns stock to available, raises ReservationExpiredEvent
  - `ExpireOverdueReservations()` - Batch expire past-due pending reservations
  - Optimistic concurrency via Version field incremented on every mutation

#### Domain Events (5 events, all immutable records)
- InventoryItemCreatedEvent
- StockReservedEvent (includes shortfall)
- ReservationConfirmedEvent
- ReservationCancelledEvent
- ReservationExpiredEvent

Result: 56 passed, 1 failed (test assertion bug in ExpireReservations test - expected 90 but correct answer was 80).

### Phase 4: Fix Test Bug
Fixed `ExpireReservations_ExpiresOnlyPastDueReservations` assertion: after reserving 10+20 from 100 stock and expiring the 10, available is 80 (not 90). All 57 tests pass.

### Phase 5: REFACTOR
- Fixed CS8618 warnings by initializing non-nullable properties with `null!` in EF Core parameterless constructors
- Clean build: 0 warnings, 0 errors
- All 57 tests still pass

## DDD Patterns Applied
| Pattern | Implementation |
|---------|---------------|
| Value Objects | `Quantity`, `ProductId` - immutable, private ctor, factory with Result<T>, value equality |
| Entity | `Reservation` - identity, private setters, state machine with guards |
| Aggregate Root | `InventoryItem` - single entry point for all mutations, private collection, domain events |
| Factory Methods | `Create()` on all types returning `Result<T>`, private constructors |
| Domain Events | 5 immutable record events (past tense naming) |
| Result<T> | All operations return Result, no exceptions for business rules |
| Optimistic Concurrency | Version field incremented on every state-changing operation |
| Private Setters | All domain properties use `{ get; private set; }` |
| EF Core Ready | Parameterless constructors, backing fields for collections |

## Final Test Summary
```
Passed!  - Failed: 0, Passed: 57, Skipped: 0, Total: 57, Duration: 13 ms
```
