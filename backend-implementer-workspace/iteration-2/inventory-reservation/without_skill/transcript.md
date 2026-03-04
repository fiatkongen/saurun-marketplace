# Transcript: Inventory Reservation System (Without Skill)

## Task
Build a .NET 9 inventory reservation domain with InventoryItem aggregate, Reservation entity, value objects, Result<T> pattern, optimistic concurrency, and xUnit tests.

## Approach

Implemented a pure domain model (no infrastructure/persistence) following DDD tactical patterns.

### Step 1: Foundation Types
Created common base classes and the Result<T> monad:
- `Result` / `Result<T>` -- success/failure monad with implicit conversion from value
- `Entity<TId>` -- identity-based equality
- `AggregateRoot<TId>` -- extends Entity with `Version` field for optimistic concurrency

### Step 2: Value Objects
- `ProductId` -- wraps Guid, rejects Guid.Empty
- `ReservationId` -- wraps Guid, rejects Guid.Empty
- `Quantity` -- non-negative int with arithmetic operators (+, -, >, <, >=, <=) and `Min()`. Subtraction throws on negative result.

### Step 3: Enum
- `ReservationStatus` -- Pending, Confirmed, Expired, Cancelled

### Step 4: Reservation Entity
- Created internally by InventoryItem (constructor is `internal`)
- State transitions enforced: only Pending reservations can be Confirmed/Cancelled/Expired
- `IsExpired(DateTime utcNow)` checks both status AND time

### Step 5: InventoryItem Aggregate Root
Core operations:
- **Reserve(quantity, utcNow, expiration?)** -- Auto-expires stale reservations first. Supports partial fills: reserves min(requested, available), returns ReservationResult with shortfall info. Fails only on zero quantity or zero available stock.
- **ConfirmReservation(id, utcNow)** -- Checks expiration, transitions to Confirmed, decrements ReservedQuantity (stock is "sold").
- **CancelReservation(id, utcNow)** -- Returns stock to Available, transitions to Cancelled.
- **ExpireStaleReservations(utcNow)** -- Bulk expire all pending-past-deadline reservations.
- **Restock(quantity)** -- Increases available stock.

All mutating operations call `IncrementVersion()` for optimistic concurrency detection.

### Step 6: Tests
57 xUnit tests across 4 test classes:
- **ResultTests** (7 tests) -- Success/Failure construction, value access, implicit conversion, invariant enforcement
- **ValueObjectTests** (12 tests) -- ProductId, ReservationId, Quantity construction/equality/arithmetic/comparison edge cases
- **ReservationEntityTests** (5 tests) -- Expiration detection, status transition enforcement
- **InventoryItemTests** (33 tests) -- Reserve happy path, partial fill, edge cases (zero qty, no stock), confirm/cancel happy + edge, expiration mechanics, restock, version tracking, full lifecycle scenarios, mixed outcome multi-reservation scenarios

### Build Issue
Initial build failed because test files were missing `using Xunit;`. Fixed by adding the import to all 4 test files.

## Final Result
- **Build:** Succeeded (0 warnings, 0 errors)
- **Tests:** 57/57 passed in 0.24 seconds
- **Files:** 12 source files across Domain/ and Tests/

## File Listing

```
outputs/
  InventoryReservation.sln
  Domain/
    Domain.csproj
    Common/
      Result.cs
      Entity.cs
      AggregateRoot.cs
      ReservationResult.cs
    Enums/
      ReservationStatus.cs
    ValueObjects/
      ProductId.cs
      ReservationId.cs
      Quantity.cs
    Entities/
      Reservation.cs
    Aggregates/
      InventoryItem.cs
  Tests/
    Tests.csproj
    ResultTests.cs
    ValueObjectTests.cs
    ReservationEntityTests.cs
    InventoryItemTests.cs
```
