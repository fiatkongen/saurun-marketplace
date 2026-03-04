# Transcript: Inventory Reservation System (Without Skill)

## Task
Build a .NET 9 inventory reservation domain with InventoryItem aggregate, Reservation entity, value objects, Result<T>, optimistic concurrency, and xUnit tests.

## Execution Steps

### 1. Created directory structure
```
outputs/
  Domain/
    Common/        - Result, Entity, AggregateRoot base types
    ValueObjects/  - ProductId, ReservationId, Quantity, ReservationResult
    Enums/         - ReservationStatus
    Entities/      - Reservation
    Aggregates/    - InventoryItem
  Tests/           - ValueObjectTests, ReservationTests, InventoryItemTests, ResultTests
```

### 2. Wrote foundational types (parallel batch)
- **Result.cs** / **Result&lt;T&gt;** - Success/Failure pattern with error message, value access guarded on success
- **Entity&lt;TId&gt;** - Base entity with Id and equality by Id
- **AggregateRoot&lt;TId&gt;** - Extends Entity with Version field for optimistic concurrency

### 3. Wrote value objects (parallel batch)
- **ProductId** - Guid wrapper, rejects Guid.Empty, record for value equality
- **ReservationId** - Same pattern as ProductId
- **Quantity** - Non-negative int wrapper with arithmetic operators (+, -), comparison operators, throws on negative result
- **ReservationResult** - Captures reservation outcome: reserved qty, requested qty, shortfall, IsPartial flag

### 4. Wrote enum
- **ReservationStatus** - Pending, Confirmed, Expired, Cancelled

### 5. Wrote Reservation entity
- Created by InventoryItem aggregate (internal constructor)
- Status transitions: Pending -> Confirmed, Pending -> Cancelled, Pending -> Expired
- Guards: can't confirm expired, can't cancel non-pending, can't expire non-overdue
- IsExpired check considers both time AND status (confirmed reservations are never "expired")

### 6. Wrote InventoryItem aggregate
- Tracks AvailableQuantity and ReservedQuantity
- **Reserve()**: Supports partial reservations (reserves what's available, reports shortfall). Auto-expires overdue reservations before checking availability. Default 15-minute expiry, configurable.
- **ConfirmReservation()**: Checks expiry first (auto-expires if overdue), then confirms. Reserved stock leaves system permanently (ReservedQuantity decreases, AvailableQuantity unchanged).
- **CancelReservation()**: Returns reserved stock to available.
- **ExpireOverdueReservations()**: Batch expires all overdue pending reservations.
- **Restock()**: Adds to available quantity.
- Every mutation increments Version for optimistic concurrency.

### 7. Wrote project files
- Domain.csproj (net9.0, InternalsVisibleTo for Tests)
- Tests.csproj (xunit 2.9.3, FluentAssertions 7.1.0)
- InventoryReservation.sln

### 8. Wrote tests - 57 total

**ResultTests (4):** Success/failure creation, value access, error access, guarded value on failure.

**ValueObjectTests (11):** ProductId empty guard, uniqueness, value equality. ReservationId empty guard. Quantity negative guard, zero valid, arithmetic, subtraction underflow throws, comparison operators, value equality.

**ReservationTests (10):** New is Pending. Confirm succeeds. Confirm expired fails. Confirm already confirmed fails. Cancel succeeds. Cancel confirmed fails. Expire overdue succeeds. Expire non-overdue fails. IsExpired true when past. IsExpired false when not pending.

**InventoryItemTests (32):**
- Reserve: full qty, partial (shortfall), zero fails, no stock fails, all stock then retry fails, 15min default expiry, custom expiry, version increment, multiple reservations tracking
- Confirm: permanent stock removal, nonexistent fails, expired auto-returns stock, already confirmed fails, version increment
- Cancel: returns stock, nonexistent fails, confirmed fails, version increment
- Expire: batch returns stock, only overdue, ignores non-pending, auto-expire on reserve
- Restock: increases available, zero throws
- Concurrency: version starts at 0, each mutation increments, simulated concurrent reservation conflict via version, simulated concurrent confirm/cancel conflict
- Complex: full lifecycle (reserve-confirm, reserve-cancel, reserve-expire), mixed outcomes, partial then restock then full

### 9. Build & fix
- First build failed: missing `using Xunit;` in all test files. Added.
- Second build: success, 0 warnings, 0 errors.

### 10. Test run
- **57 passed, 0 failed** in 0.25 seconds.

## Design Decisions

1. **Partial reservations** - Reserve returns success with shortfall info rather than failing. Caller decides what to do with partial fill. Zero available still fails (no reservation created).

2. **Auto-expire on Reserve** - When reserving, expired reservations are reclaimed first. This prevents phantom "out of stock" when all stock is held by expired-but-not-yet-processed reservations.

3. **Confirm checks expiry** - If you try to confirm an expired reservation, the aggregate auto-expires it and returns the stock. This prevents race conditions where expiry processing hasn't run yet.

4. **Version on aggregate only** - Following DDD convention, version lives on the aggregate root, not individual entities. Each state-changing operation on InventoryItem increments it.

5. **Reservation constructor is internal** - Only InventoryItem can create reservations, enforcing the invariant that stock accounting stays consistent.

6. **Value objects as records** - ProductId, ReservationId, Quantity use C# records for structural equality without boilerplate.
